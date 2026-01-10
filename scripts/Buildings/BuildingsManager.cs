using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

public class BuildingsManager
{
	private EnemyManager enemyManager = null;
	private ResourcesManager resourcesManager = null;
	private GameManager gameManager;
	private QuadTree tree = null;
	public List<Building> buildings { get; private set; } = new();
	public List<string> allBuildingNames { get; private set; } = new();
	public List<string> allBuildableBuildingNames { get; private set; } = new();

	private int updateOffset = 0;

	private JSONFormats.BuildingsData buildingsStaticData;
	private Dictionary<string, int> buildingStaticDataIndexPerBuildingName = new();

	private BuildingGrid grid = new();
	public void LoadData(string _path)
	{
		buildingsStaticData = JSONManager.Read<JSONFormats.BuildingsData>(_path);

		foreach(JSONFormats.Building building in buildingsStaticData.Buildings)
		{
			allBuildingNames.Add(building.Name);

			if(building.Buildable)
				allBuildableBuildingNames.Add(building.Name);
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

	public bool AddBuilding(Vector2I _gridPos, string _buildingName)
	{
		Building candidate = Building.ConstructBuildingFromStaticData(GetBuildingStaticData(_buildingName));
		int xCenterOffset = Mathf.FloorToInt(candidate.bbox.w * 0.5f);
		int yCenterOffset = Mathf.FloorToInt(candidate.bbox.h * 0.5f);
		candidate.SetPosition(_gridPos - new Vector2I(xCenterOffset, yCenterOffset));

		if(grid.Available(candidate.bbox) == false)
			return false;

		int index = -1; // try to reuse an empty spot
		for(int i = 0; i < buildings.Count; ++i)
		{
			if(buildings[i] == null)
			{
				index = i;
				break;
			}
		}

		if(index != -1)
		{
			buildings[index] = candidate;
		}
		else
		{
			buildings.Add(candidate);
			index = buildings.Count - 1;
		}

		grid.AddBuilding(index, candidate.bbox);
		gameManager.OnBuildingAdded(buildings[index]);
		return true;
	}

	public void Update(double _dt)
	{
		bool oneDestroyed = false;

		for(int i = 0; i < buildings.Count; ++i)
		{
			int index = (i + updateOffset) % buildings.Count;

			if(buildings[index] == null)
				continue; // already destroyed and marked as such

			buildings[index].Update(_dt, resourcesManager, enemyManager, tree);

			if(oneDestroyed == false && buildings[index].health.alive == false)
				oneDestroyed = true; // flag to mark destroyed buildings
		}

		if(oneDestroyed)
			CheckForDestroyedBuildings(); // At least one is down, check them all

		// Make sure all buildings have the chance to access resources
		updateOffset++;
		if(updateOffset >= buildings.Count)
			updateOffset = 0;
	}

	public JSONFormats.Building GetBuildingStaticData(string _name)
	{
		if(buildingStaticDataIndexPerBuildingName.ContainsKey(_name) == false)
		{
			for(int i = 0; i < buildingsStaticData.Buildings.Count; ++i)
			{
				if(buildingsStaticData.Buildings[i].Name == _name)
				{
					buildingStaticDataIndexPerBuildingName.Add(_name, i);
					break;
				}
			}
		}

		if(buildingStaticDataIndexPerBuildingName.ContainsKey(_name) == false)
		{
			GD.PrintErr("Could not find StaticData for Building " + _name);
			return null;
		}

		return buildingsStaticData.Buildings[buildingStaticDataIndexPerBuildingName[_name]];
	}

	private void CheckForDestroyedBuildings()
	{
		for(int i = buildings.Count - 1; i >= 0; --i)
		{
			if(buildings[i] == null)
				continue;

			if(buildings[i].health.alive)
				continue;

			RemoveBuilding(i);
		}
	}

	private void RemoveBuilding(int _index)
	{
		// Clear buildings grid
		grid.ClearSlot(_index, buildings[_index].bbox);
		// warn other components
		gameManager.OnBuildingRemoved(buildings[_index]);
		// remove from list -> Don't remove otherwise all other buildings will change indices
		buildings[_index] = null;
	}
}
