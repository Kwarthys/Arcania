using Godot;
using System;
using System.Collections.Generic;
using System.Resources;

public partial class GameManager : Node
{
	[Export] private MultiMeshInstance3D multiMesh;
	[Export] private double trimCheckTimer = 5.0;
	[Export] private string BuildingsDataPath;
	[Export] private PackedScene turretModel;
	[Export] private PackedScene harvesterModel;
	[Export] bool drawTreeDebug = false;
	private BuildingsManager buildingsManager = new();
	private EnemyManager enemyManager = new();
	private ResourcesManager resourcesManager = new();
	private double dtCounter = 0.0;

	private QuadTree tree;

	public override void _Ready()
	{
		int n = 1000;
		enemyManager.Initialize(n);

		tree = new(new(-25.0f, -25.0f, 50.0f, 50.0f), enemyManager);

		for(int i = 0; i < enemyManager.count; ++i)
			tree.SubmitElement(i, enemyManager.GetPosition(i));

		buildingsManager.Initialize(enemyManager, resourcesManager, tree, new(50, 50));
		buildingsManager.LoadData(BuildingsDataPath);

		foreach(Building b in buildingsManager.buildings)
		{
			Node3D turret = turretModel.Instantiate<Node3D>();
			turret.Position = new(b.position.X, 0.0f, b.position.Y);
			AddChild(turret);
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double _dt)
	{
		DrawDebugManager.Reset();

		enemyManager.Update(_dt);
		buildingsManager.Update(_dt);

		ResourceDisplayManager.Instance.Update(resourcesManager.playerResources);

		List<int> orphanIndices = new();
		tree.CheckDepartures(orphanIndices);

		orphanIndices.ForEach((id) => tree.SubmitElement(id, enemyManager.positions[id]));

		if(drawTreeDebug)
			tree.DrawDebug();

		int meshesToDisplay = 0;
		for(int i = 0; i < enemyManager.count; ++i)
		{
			if(enemyManager.Alive(i))
				meshesToDisplay++;
		}
		multiMesh.Multimesh.InstanceCount = meshesToDisplay;

		int meshIndex = 0;
		for(int i = 0; i < enemyManager.count; ++i)
		{
			if(enemyManager.Alive(i) == false)
				continue;

			Vector3 pos = new(enemyManager.positions[i].X, 0.5f, enemyManager.positions[i].Y);
			multiMesh.Multimesh.SetInstanceTransform(meshIndex++, new Transform3D(Basis.Identity, pos));
		}

		dtCounter += _dt;
		if(dtCounter > trimCheckTimer)
		{
			tree.CheckTrim();
			dtCounter = 0.0;
		}
	}

	public void AddBuilding(Vector3 _pos, bool _tower)
	{
		Vector2I pos = new(Mathf.FloorToInt(_pos.X), Mathf.FloorToInt(_pos.Z));
		Node3D model;
		if(_tower)
		{
			model = turretModel.Instantiate<Node3D>();
			buildingsManager.AddTower(pos);

		}
		else
		{
			model = harvesterModel.Instantiate<Node3D>();
			buildingsManager.AddHarvester(pos);
		}

		model.Position = new(pos.X, 0.0f, pos.Y);
		AddChild(model);
	}
}
