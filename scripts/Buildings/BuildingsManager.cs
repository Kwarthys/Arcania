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
	public List<string> allBuildingNames { get; private set; } = new();

	private int updateOffset = 0;

	private BuildingGrid grid = new();
	public void LoadData(string _path)
	{
		JSONFormats.BuildingsData data = JSONManager.Read<JSONFormats.BuildingsData>(_path);

		foreach(JSONFormats.Building building in data.Buildings)
		{
			allBuildingNames.Add(building.Name);
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

	public bool AddBuilding(Vector2I _gridPos, bool _isTower)
	{
		Building candidate;

		if(_isTower)
			candidate = Building.MakeBasicTower();
		else
			candidate = Building.MakeBasicHarvester();

		candidate.SetPosition(_gridPos);

		if(grid.Available(candidate.bbox) == false)
			return false;

		buildings.Add(candidate);
		grid.AddBuilding(buildings.Count - 1, candidate.bbox);
		gameManager.OnBuildingAdded(buildings.Last());
		return true;
	}

	public void Update(double _dt)
	{
		for(int i = 0; i < buildings.Count; ++i)
		{
			int index = (i + updateOffset) % buildings.Count;
			Building b = buildings[index];

			b.weapons?.ForEach((w) => w.Update(_dt, b.GetCenterPosition(), enemyManager, tree, ref resourcesManager));
			b.refiners?.ForEach((r) => r.Update(_dt, ref resourcesManager.playerResources));
		}

		// Make sure all buildings have the chance to access resources
		updateOffset++;
		if(updateOffset >= buildings.Count)
			updateOffset = 0;
	}
}
