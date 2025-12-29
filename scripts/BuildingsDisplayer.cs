using Godot;
using System;
using System.Collections.Generic;

public partial class BuildingsDisplayer : Node
{
	private ModelsDisplayer displayer;
	private Dictionary<Building, Node3D> models = new();

	private BuildingGhostManager ghostManager = new();

	private Dictionary<string, PackedScene> buildingScenesPerName = new();

	public void Initialize(ModelsDisplayer _displayer, List<string> _buildingNames)
	{
		displayer = _displayer;
		ghostManager.Initialize(displayer);
		PreLoadBuildingsModel(_buildingNames);
	}

	public void AddBuilding(Building _b)
	{
		if(models.ContainsKey(_b) == false)
		{
			Node3D node = InstantiateBuildingModel(_b.buildingName);
			if(node == null)
			{
				GD.PrintErr("Failed to instantiate " + _b.buildingName);
				return;
			}
			models.Add(_b, node);
			AddChild(node);
		}

		Reposition(_b);
	}

	public void RemoveBuilding(Building _b)
	{
		if(models.ContainsKey(_b))
		{
			models[_b].QueueFree();
			models.Remove(_b);
		}
	}

	public void DrawTargetDebug(EnemyManager _enemyManager)
	{
		foreach(Building b in models.Keys)
		{
			if(b.weapons == null)
				continue;

			foreach(Weapon w in b.weapons)
			{
				if(w.targetIndex == -1)
					continue;

				Vector2 targetPos = _enemyManager.GetPosition(w.targetIndex);
				DrawDebugManager.DebugDrawLine(models[b].Position + Vector3.Up * 7.0f, displayer.GridToWorld(targetPos) + Vector3.Up * 0.5f);
			}
		}
	}

	private void Reposition(Building _b)
	{
		Node3D node = models[_b];
		node.Position = displayer.GridToWorld(_b.GetCenterPosition());
	}

	private void PreLoadBuildingsModel(List<string> _names)
	{
		foreach(string name in _names)
			ResourceLoader.LoadThreadedRequest(BuildingNameToScene(name));
	}

	public void MoveGhost(Vector3 _worldPos) { ghostManager.UpdateGhost(_worldPos); }

	public void ChangeGhost(string _name)
	{
		Node3D model = InstantiateBuildingModel(_name);
		ghostManager.ChangeGhost(model);
		if(model != null)
			AddChild(model);
	}

	private PackedScene GetBuildingScene(string _name)
	{
		if(buildingScenesPerName.ContainsKey(_name) == false)
		{
			string scenePath = BuildingNameToScene(_name);

			if(ResourceLoader.LoadThreadedGetStatus(scenePath) == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				buildingScenesPerName.Add(_name, (PackedScene)ResourceLoader.LoadThreadedGet(scenePath));
			}
			else
			{
				return null;
			}
		}

		return buildingScenesPerName[_name];
	}

	private Node3D InstantiateBuildingModel(string _name)
	{
		if(_name == "")
			return null;

		PackedScene buildingScene = GetBuildingScene(_name);
		if(buildingScene == null)
			return null;

		Node3D model = buildingScene.Instantiate<Node3D>();
		return model;
	}

	private string BuildingNameToScene(string name) { return displayer.buildingModelsPath + "/" + name + ".tscn"; }
}
