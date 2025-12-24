using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

public class BuildingsManager
{
	private PackedScene turretModel;
	private EnemyManager enemyManager = null;
	private ResourcesManager resourcesManager = null;
	private GameManager gameManager;
	private QuadTree tree = null;
	public List<Building> buildings { get; private set; } = new();

	private int updateOffset = 0;

	private BuildingGrid grid = new();
	public void LoadData(string _path)
	{
		JSONFormats.BuildingsData data = JSONManager.Read<JSONFormats.BuildingsData>(_path);
		GD.Print(data);

		foreach(JSONFormats.Building building in data.Buildings)
		{
			GD.Print(building.Name + ": " + building.Cost.Mana);
		}
	}

	public void Initialize(GameManager _gameManager, EnemyManager _enemyManager, ResourcesManager _resourcesManager, QuadTree _tree, Vector2I _gridSize)
	{
		gameManager = _gameManager;
		enemyManager = _enemyManager;
		tree = _tree;
		resourcesManager = _resourcesManager;
		grid.Initialize(_gridSize.X, _gridSize.Y);
	}

	public void AddBuilding(Vector2I _gridPos, bool _isTower)
	{
		if(_isTower)
			AddTower(_gridPos);
		else
			AddHarvester(_gridPos);
		gameManager.OnBuildingAdded(buildings.Last());
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
		for(int i = 0; i < buildings.Count; ++i)
		{
			int index = (i + updateOffset) % buildings.Count;
			Building b = buildings[index];

			b.weapons?.ForEach((w) => w.Update(_dt, b.position, enemyManager, tree, ref resourcesManager));
			b.refiners?.ForEach((r) => r.Update(_dt, ref resourcesManager.playerResources));
		}

		// Make sure all buildings have the chance to access resources
		updateOffset++;
		if(updateOffset >= buildings.Count)
			updateOffset = 0;
	}
}
