using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.ExceptionServices;

public class BuildingsManager
{
	private EnemyManager enemyManager = null;
	private ResourcesManager resourcesManager = null;
	private GameManager gameManager;
	private QuadTree tree = null;
	public List<Building> buildings { get; private set; } = new();
	public List<string> allBuildingNames { get; private set; } = new();
	public List<string> allBuildableBuildingNames { get; private set; } = new();

	private List<ConstructionQueue> constructionQueues = new();

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

	public void Initialize(GameManager _gameManager, EnemyManager _enemyManager, ResourcesManager _resourcesManager, QuadTree _tree)
	{
		gameManager = _gameManager;
		enemyManager = _enemyManager;
		tree = _tree;
		resourcesManager = _resourcesManager;

		grid.Initialize(_gameManager.gridSize, _gameManager.gridSize, _gameManager.buildingGridMargin);
	}

	public void CreateConstructionQueue(List<Vector2I> _pos, string _buildingName, List<Node3D> _ghosts)
	{
		if(_pos.Count != _ghosts.Count)
			throw new Exception("Buildings Manager Create Construction Queue different number of Positions and Ghosts");

		constructionQueues.Add(new(this));
		for(int i = 0; i < _pos.Count; ++i)
		{
			constructionQueues.Last().AddItem(_pos[i], _buildingName, _ghosts[i]);
		}
		constructionQueues.Last().Advance();
	}

	public bool AddBuilding(Vector2I _gridPos, string _buildingName, ConstructionQueue _q = null)
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

		if(_q != null)
			buildings[index].constructor.queue = _q;

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

	public void OnConstructionQueueAdvance(ConstructionQueue.BuildCommand _command, ConstructionQueue _q)
	{
		bool startSuccess = AddBuilding(_command.gridPos, _command.buildingName, _q);
		_command.ghost.QueueFree();

		if(startSuccess == false)
		{
			if(_q.GetSize() > 0)
			{
				_q.Advance(); // Start some recursion ehe
				return;
			}
		}

		for(int i = constructionQueues.Count - 1; i >= 0; --i)
		{
			if(constructionQueues[i].GetSize() == 0)
				constructionQueues.RemoveAt(i);
		}
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
		// Advance constructionQueue if needed
		if(buildings[_index].constructor != null && buildings[_index].constructor.queue != null)
		{
			// Building died while under construction and belongs to a queue, advance it
			buildings[_index].constructor.queue.Advance();
		}
		// Clear building passive effects
		buildings[_index].OnDestruction(resourcesManager);
		// Clear buildings grid
		grid.ClearSlot(_index, buildings[_index].bbox);
		// warn other components
		gameManager.OnBuildingRemoved(buildings[_index]);
		// remove from list -> Don't remove otherwise all other buildings will change indices
		buildings[_index] = null;
	}
}
