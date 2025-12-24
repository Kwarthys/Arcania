using Godot;
using System;
using System.Collections.Generic;
using System.Resources;

public class Building
{
	public Vector2I position = new(0, 0);
	public float health = 100.0f;

	public List<Weapon> weapons; // Apply damage effects to enemies
	public List<Refiner> refiners; // Make changes to player's resources

	public static Building MakeBasicTower()
	{
		Building tower = new();
		tower.weapons = [new()];
		return tower;
	}

	public static Building MakeBasicHarvester()
	{
		Building harvester = new();
		harvester.refiners = [new()];
		return harvester;
	}
}

public class Weapon
{
	public float range = 10.0f;
	public float damage = 100.0f;
	public float firePeriod = 2.0f;
	public int targetIndex = -1;

	public Price shotCost = new(5.0f, 0.0f, 0.0f, 0.0f);
	private Price accumulator = new();

	public void Update(double _dt, Vector2I _pos, EnemyManager _enemyManager, QuadTree _tree, ref ResourcesManager _playerResources)
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

public class Refiner
{
	public Price delta = new(1.0f, 0.0f, 0.0f, 0.0f);
	public float period = 1.0f;
	public float activityRatio = 1.0f;
	private double dtCounter = 0.0f;

	public void Update(double _dt, ref Price _playerResources)
	{
		if(dtCounter < period)
		{
			dtCounter += _dt;
			return;
		}

		Price actualDelta = delta * activityRatio;
		if(_playerResources >= -actualDelta)
		{
			_playerResources += delta * activityRatio;
			dtCounter = 0.0;
		}
	}
}
