using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingsManager
{
	private PackedScene turretModel;
	private EnemyManager enemyManager = null;
	private ResourcesManager resourcesManager = null;
	private QuadTree tree = null;
	public List<Building> buildings { get; private set; } = new();
	public void LoadData(string _path)
	{
		JSONFormats.BuildingsData data = JSONManager.Read<JSONFormats.BuildingsData>(_path);
		GD.Print(data);

		foreach(JSONFormats.Building building in data.Buildings)
		{
			GD.Print(building.Name + ": " + building.Cost.Mana);
		}
	}

	public void Initialize(EnemyManager _enemyManager, ResourcesManager _resourcesManager, QuadTree _tree)
	{
		enemyManager = _enemyManager;
		tree = _tree;
		resourcesManager = _resourcesManager;
	}

	public void AddTower(Vector2I _pos)
	{
		buildings.Add(Building.MakeBasicTower());
		buildings.Last().position = _pos;
	}

	public void AddHarvester(Vector2I _pos)
	{
		buildings.Add(Building.MakeBasicHarvester());
		buildings.Last().position = _pos;
	}

	public void Update(double _dt)
	{
		foreach(Building b in buildings)
		{
			b.weapons?.ForEach((w) => w.Update(_dt, b.position, enemyManager, tree));
			b.refiners?.ForEach((r) => r.Update(_dt, ref resourcesManager.playerResources));
		}
	}
}
