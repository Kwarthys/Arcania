using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

public class Building
{
	public BoundingBoxI bbox = new(0, 0, 1, 1);
	public float health = 100.0f;
	public List<Weapon> weapons; // Apply damage effects to enemies
	public string buildingName { get; private set; }
	public List<BuildingEffect> effects = new(); // only economy effects for now

	public static Building ConstructBuildingFromStaticData(JSONFormats.Building _staticData)
	{
		Building b = new();
		b.buildingName = _staticData.Name;
		b.health = _staticData.Health;
		b.bbox = new(0, 0, _staticData.Footprint.X, _staticData.Footprint.Y);

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

		return b;
	}

	public Vector2I GetPosition() { return new(bbox.x, bbox.y); }
	public Vector2 GetCenterPosition() { return new(bbox.x + bbox.w / 2.0f, bbox.y + bbox.h / 2.0f); }
	public void SetPosition(Vector2I _pos)
	{
		bbox.x = _pos.X - bbox.w / 2;
		bbox.y = _pos.Y - bbox.h / 2;
	}
}

public class Weapon
{
	public float range = 10.0f;
	public float damage = 100.0f;
	public float firePeriod = 2.0f;
	public int targetIndex = -1;
	public List<BuildingEffect> effects = new();

	public Price shotCost = new(5.0f, 0.0f, 0.0f, 0.0f);
	private Price accumulator = new();

	public void Update(double _dt, Vector2 _pos, EnemyManager _enemyManager, QuadTree _tree, ref ResourcesManager _playerResources)
	{
		if(accumulator < shotCost)
		{
			Price deltaPrice = shotCost * ((float)_dt / firePeriod);
			if(_playerResources.tryPay(deltaPrice))
			{
				accumulator += deltaPrice;
			}
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
			if(accumulator >= shotCost)
			{
				// shoot ! Will later apply the weapon's effect
				accumulator -= shotCost;
				_enemyManager.Damage(targetIndex, damage);
			}
		}
	}
}

public class BuildingEffect // Used to represent both damaging and economical powers of buildings
{
	public Price gain;
	public Price cost;
	private Price costAccumulator = new();
	public float period;
	//public float damage;
	//public float activityRatio = 1.0f; // Buildings can be slowed down to preserve resources
	public float dtAccumulator = 0.0f;
	public bool timerBasedPeriod = true;
	//public List<BuildingEffect> nestedEffects = new();

	public BuildingEffect(JSONFormats.Effect _effectData)
	{
		gain = new(_effectData.Gain);
		cost = new(_effectData.Cost);
		// damage = _effectData.Damage; // simplified for now
		period = _effectData.Period;

		// simplified for now
		//foreach(JSONFormats.Effect nestedEffect in _effectData.Effects)
		//{
		//	nestedEffects.Add(new(nestedEffect));
		//}

		// Finished reading, now precompute some stuff

		// If no cost, period is only based on time.
		// With cost, this cost will be consumed by the building during the given period, no need for timers
		timerBasedPeriod = cost.IsZero();
	}

	public void Update(double _dt, ref ResourcesManager _playerResources)
	{
		if(timerBasedPeriod)
		{
			dtAccumulator += (float)_dt;
			if(dtAccumulator > period)
			{
				dtAccumulator -= period;
				_playerResources.Credit(gain);
			}
		}
		else
		{
			Price consumption = cost * ((float)_dt / period); // * activityRatio; // keep it simple for now
			if(_playerResources.Afford(consumption))
			{
				_playerResources.Pay(consumption);
				costAccumulator += consumption;

				if(costAccumulator >= cost)
				{
					costAccumulator -= cost;
					_playerResources.Credit(gain);
				}
			}

		}
	}
}
