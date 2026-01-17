using Godot;
using System.Collections.Generic;
using System.Linq;

public class Building
{
	public BoundingBoxI bbox = new(0, 0, 1, 1);
	public Health health { get; private set; } = new(100.0f);
	public List<Weapon> weapons; // Apply damage (todo effects) to enemies
	public string buildingName { get; private set; }
	public List<Effect> effects = new(); // only economy effects for now
	public Constructor constructor { get; private set; }

	private Price storage = null;

	public static Building ConstructBuildingFromStaticData(JSONFormats.Building _staticData)
	{
		Building b = new();
		b.buildingName = _staticData.Name;
		b.health = new(_staticData.Health);
		b.bbox = new(0, 0, _staticData.Footprint.X, _staticData.Footprint.Y);

		if(_staticData.Cost != null)
		{
			b.constructor = new(new(_staticData.Cost), _staticData.BuildTime);
		}

		if(_staticData.Weapons != null)
		{
			b.weapons = new();
			foreach(JSONFormats.Weapon weaponData in _staticData.Weapons)
			{
				b.weapons.Add(new());
				Weapon w = b.weapons.Last();
				w.range = weaponData.Range;
				w.damage = weaponData.TempDamage;
				w.shotCost = new(weaponData.TempCost);
				w.firePeriod = weaponData.TempPeriod;
			}
		}

		if(_staticData.Effects != null)
		{
			b.effects = new();
			foreach(JSONFormats.Effect effectData in _staticData.Effects)
			{
				b.effects.Add(new(effectData));
			}
		}

		if(_staticData.Storage != null)
			b.storage = new Price(_staticData.Storage);

		return b;
	}

	public void Update(double _dt, ResourcesManager _playerResources, EnemyManager _enemyManager, QuadTree _tree)
	{
		if(health.alive == false)
			return; // don't update while destructed

		if(constructor != null)
		{
			constructor.Update(_dt, _playerResources);
			if(constructor.completion > 1.0f - Mathf.Epsilon)
			{
				OnConstructionComplete(_playerResources);
				constructor = null; // Construction complete, remove constructor
			}
			return;
		}

		weapons?.ForEach((w) => w.Update(_dt, GetCenterPosition(), _enemyManager, _tree, _playerResources));
		effects?.ForEach((r) => r.Update(_dt, _playerResources));
	}

	public Vector2I GetPosition() { return new(bbox.x, bbox.y); }
	public Vector2 GetCenterPosition() { return new(bbox.x + bbox.w / 2.0f, bbox.y + bbox.h / 2.0f); }
	public void SetPosition(Vector2I _pos)
	{
		bbox.x = _pos.X;
		bbox.y = _pos.Y;
	}

	private void OnConstructionComplete(ResourcesManager _playerResources)
	{
		// Apply storage increase if needed
		if(storage != null)
			_playerResources.storage += storage;
	}

	public void OnDestruction(ResourcesManager _playerResources)
	{
		// Remove storage if needed
		if(storage != null)
			_playerResources.storage -= storage;
	}
}

public class Weapon
{
	public float range = 10.0f;
	public float damage = 100.0f;
	public float firePeriod = 2.0f;
	public int targetIndex = -1;
	//public List<Effect> effects = new();

	public Price shotCost = new(5.0f, 0.0f, 0.0f, 0.0f);
	private Price accumulator = new();

	public void Update(double _dt, Vector2 _pos, EnemyManager _enemyManager, QuadTree _tree, ResourcesManager _playerResources)
	{
		if(accumulator.CanPay(shotCost) == false)
		{
			_playerResources.TryConsume(_dt, shotCost, firePeriod, ref accumulator);
		}
		bool justUpdatedTarget = false;

		// Building is a tower
		if(targetIndex == -1)
		{
			// find it a target
			QuadTree.TreeBox rangeBox = new(_pos.X, _pos.Y, range);
			List<int> indicesInBox = _tree.GetElementsIn(rangeBox);

			// Find closest
			int closest = -1;
			float distSquared = 0.0f;
			foreach(int id in indicesInBox)
			{
				float currentDistanceSquared = _enemyManager.GetPosition(id).DistanceSquaredTo(_pos);
				if(closest == -1 || currentDistanceSquared < distSquared)
				{
					closest = id;
					distSquared = currentDistanceSquared;
				}
			}

			if(distSquared < range * range)
			{
				targetIndex = closest;
				justUpdatedTarget = true;
			}
		}

		// if we now have a target
		if(targetIndex != -1)
		{
			if(justUpdatedTarget == false) // don't dist check if we got it this frame as we just did it
			{
				// Range check as it might have moved
				if(_enemyManager.GetHealth(targetIndex) <= 0.0 || _enemyManager.GetPosition(targetIndex).DistanceSquaredTo(_pos) > range * range)
				{
					targetIndex = -1;
					return; // we lost our target, wait for next update to find another one
				}
			}

			// Shoot Check
			if(accumulator.CanPay(shotCost))
			{
				// shoot ! Will later apply the weapon's effect
				accumulator -= shotCost;
				_enemyManager.Damage(targetIndex, damage);
			}
		}
	}
}
