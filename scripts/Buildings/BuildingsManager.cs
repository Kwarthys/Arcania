using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingsManager
{
	private EnemyManager enemyManager = null;
	private QuadTree tree = null;
	private List<Building> buildings = new();
	public void LoadData(string _path)
	{
		JSONFormats.BuildingsData data = JSONManager.Read<JSONFormats.BuildingsData>(_path);
		GD.Print(data);

		foreach(JSONFormats.Building building in data.Buildings)
		{
			GD.Print(building.Name + ": " + building.Cost.Mana);
		}
	}

	public void Initialize(EnemyManager _enemyManager, QuadTree _tree)
	{
		enemyManager = _enemyManager;
		tree = _tree;

		for(int i = 0; i < 100; ++i)
		{
			buildings.Add(new());
			buildings.Last().position = new(GD.RandRange(-20, 20), GD.RandRange(-20, 20));
		}
	}

	public void Update(double _dt)
	{
		foreach(Building b in buildings)
		{
			if(b.fireDTCounter < b.firePeriod)
				b.fireDTCounter += _dt;

			bool justUpdatedTarget = false;

			// Building is a tower
			if(b.targetIndex == -1)
			{
				// find it a target
				QuadTree.TreeBox rangeBox = new(b.position.X, b.position.Y, b.range);
				List<int> indicesInBox = tree.GetElementsIn(rangeBox);

				// Find closest
				int closest = -1;
				float distSquared = 0.0f;
				foreach(int id in indicesInBox)
				{
					float currentDistanceSquared = enemyManager.GetPosition(id).DistanceSquaredTo(b.position);
					if(closest == -1 || currentDistanceSquared < distSquared)
					{
						closest = id;
						distSquared = currentDistanceSquared;
					}
				}

				if(distSquared < b.range * b.range)
				{
					b.targetIndex = closest;
					justUpdatedTarget = true;
				}
			}

			// if we now have a target
			if(b.targetIndex != -1)
			{
				Vector2 targetPos = enemyManager.GetPosition(b.targetIndex);
				DrawDebugManager.DebugDrawLine(new(b.position.X, 10.0f, b.position.Y), new(targetPos.X, 0.5f, targetPos.Y));

				if(justUpdatedTarget == false) // don't dist check if we got it this frame as we just did it
				{
					// Range check as it might have moved
					if(enemyManager.GetPosition(b.targetIndex).DistanceSquaredTo(b.position) > b.range * b.range)
					{
						b.targetIndex = -1;
						continue; // we lost our target, wait for next update to find another one
					}
				}

				// Shoot Check
				if(b.fireDTCounter > b.firePeriod)
				{
					// shoot !
					b.fireDTCounter = 0.0;
				}
			}
		}
	}
}
