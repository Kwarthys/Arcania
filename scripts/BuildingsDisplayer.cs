using Godot;
using System;
using System.Collections.Generic;

public partial class BuildingsDisplayer : Node
{
	private ModelsDisplayer displayer;
	private PackedScene turretModel;
	private PackedScene harvesterModel;
	private Dictionary<Building, Node3D> models = new();

	public void Initialize(ModelsDisplayer _displayer, PackedScene _turretModel, PackedScene _harvesterModel)
	{
		displayer = _displayer;
		turretModel = _turretModel;
		harvesterModel = _harvesterModel;
	}

	public void AddBuilding(Building _b)
	{
		if(models.ContainsKey(_b) == false)
		{
			PackedScene scene;
			if(_b.weapons != null)
				scene = turretModel;
			else
				scene = harvesterModel;

			Node3D node = scene.Instantiate<Node3D>();
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
}
