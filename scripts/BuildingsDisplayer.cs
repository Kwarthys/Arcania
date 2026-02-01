using Godot;
using System;
using System.Collections.Generic;

public partial class BuildingsDisplayer : Node
{
	private ModelsDisplayer displayer;
	private Dictionary<Building, Node3D> models = new();

	private BuildingGhostManager ghostManager = null;

	private Dictionary<string, PackedScene> buildingScenesPerName = new();

	private Dictionary<Building, List<MeshInstance3D>> constructingBuildings = new();

	public void Initialize(ModelsDisplayer _displayer, List<string> _buildingNames)
	{
		displayer = _displayer;
		PreLoadBuildingsModel(_buildingNames);

		ghostManager = new(InstantiateBuildingModel);
		AddChild(ghostManager);
		ghostManager.Initialize(displayer);
	}

	public void Update(EnemyManager _enemyManager)
	{
		UpdateConstructingBuildings();
		PlayBuildingsAnimations(_enemyManager);
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
		MarkForConstruction(_b);
	}

	private void PlayBuildingsAnimations(EnemyManager _enemyManager)
	{
		foreach(KeyValuePair<Building, Node3D> pair in models)
		{
			if(pair.Key.constructor != null)
				continue; // Still under construction

			if(pair.Key.weapons == null)
				continue; // only weapons are animated for now

			if(pair.Value is BuildingAnimator animator)
			{
				List<Vector3> worldPositions = new();
				foreach(Weapon w in pair.Key.weapons)
				{
					if(w.targetIndex < 0.0)
					{
						worldPositions.Add(new Vector3(0, -1000, 0)); // don't move
						continue;
					}

					Vector2 targetGridPos = _enemyManager.GetPosition(w.targetIndex);
					Vector3 targetPos = displayer.GridToWorld(targetGridPos);
					targetPos.Y += 0.5f;
					worldPositions.Add(targetPos);
				}
				animator.Update(worldPositions);
			}
		}
	}

	private void UpdateConstructingBuildings()
	{
		List<Building> toRemove = null;
		// Manage shader for building still under construction
		foreach(KeyValuePair<Building, List<MeshInstance3D>> pair in constructingBuildings)
		{
			if(pair.Key.constructor == null)
			{
				if(toRemove == null)
					toRemove = [pair.Key];
				else
					toRemove.Add(pair.Key);

				continue; // Construction complete
			}

			float completion = pair.Key.constructor.completion;
			foreach(MeshInstance3D meshInstance in pair.Value)
			{
				ShaderMaterial mat = (ShaderMaterial)meshInstance.MaterialOverride;
				mat.SetShaderParameter("completion", completion);
			}
		}

		if(toRemove == null)
			return;

		foreach(Building b in toRemove)
		{
			foreach(MeshInstance3D meshInstance in constructingBuildings[b])
				meshInstance.MaterialOverride = null;

			constructingBuildings.Remove(b);
		}
	}

	private void MarkForConstruction(Building _b)
	{
		if(_b.constructor == null)
			return;

		List<Node> toScan = [models[_b]];
		List<MeshInstance3D> meshInstances = new();

		Aabb bounds = new();

		while(toScan.Count > 0)
		{
			Node scanning = toScan[0];
			toScan.RemoveAt(0);

			if(scanning is MeshInstance3D)
			{
				MeshInstance3D meshInstance = (MeshInstance3D)scanning;
				meshInstances.Add(meshInstance);
				bounds = bounds.Merge(meshInstance.GlobalTransform * meshInstance.GetAabb()); // Make that bounding box in World coordinates
			}

			foreach(Node n in scanning.GetChildren())
				toScan.Add(n);
		}

		foreach(MeshInstance3D instance in meshInstances)
		{
			instance.MaterialOverride = (Material)displayer.constructionMaterial.Duplicate(true);
			ShaderMaterial mat = (ShaderMaterial)instance.MaterialOverride;
			mat.SetShaderParameter("completion", 0.0f);
			mat.SetShaderParameter("startEndHeights", new Vector2(bounds.Position.Y, bounds.End.Y));
		}

		constructingBuildings.Add(_b, meshInstances);
	}

	public void RemoveBuilding(Building _b)
	{
		if(models.ContainsKey(_b))
		{
			models[_b].QueueFree();
			models.Remove(_b);
		}

		if(constructingBuildings.ContainsKey(_b))
		{
			constructingBuildings.Remove(_b);
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
		{
			string path = BuildingNameToScene(name);
			if(ResourceLoader.Exists(path))
				ResourceLoader.LoadThreadedRequest(path);
		}
	}

	public void MoveGhost(Vector3 _worldPos) { ghostManager.UpdateGhost(_worldPos); }
	public void PlaceGhosts(List<Vector3> _positions) { ghostManager.PlaceGhosts(_positions); }
	public List<Node3D> TransferGhosts(int _count) { return ghostManager.TransferGhosts(_count); }

	public void ChangeGhost(string _name, bool _offsetCenter)
	{
		Node3D model = InstantiateBuildingModel(_name);
		ghostManager.ChangeGhost(_name, model, _offsetCenter);
	}

	private PackedScene GetBuildingScene(string _name)
	{
		if(buildingScenesPerName.ContainsKey(_name) == false)
		{
			string scenePath = BuildingNameToScene(_name);

			if(ResourceLoader.Exists(scenePath) == false)
			{
				buildingScenesPerName.Add(_name, null); // Avoid asking resource loader each time for a missing building
				return null;
			}

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
			return displayer.placeHolderBuildingModel.Instantiate<Node3D>();

		return buildingScene.Instantiate<Node3D>();
	}

	private string BuildingNameToScene(string name) { return displayer.buildingModelsPath + "/" + name + ".tscn"; }
}
