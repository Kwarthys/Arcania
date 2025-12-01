using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	private EnemyManager enemyManager = new();
	[Export] private MultiMeshInstance3D multiMesh;

	private double dtCounter = 0.0;
	[Export] private double trimCheckTimer = 5.0;

	private QuadTree tree;

	public override void _Ready()
	{
		int n = 1000;
		enemyManager.Initialize(n);
		multiMesh.Multimesh.InstanceCount = n;

		tree = new(new(-25.0f, -25.0f, 50.0f, 50.0f), enemyManager);

		for(int i = 0; i < enemyManager.count; ++i)
		{
			Vector3 pos = new(enemyManager.positions[i].X, 0.5f, enemyManager.positions[i].Y);
			multiMesh.Multimesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, pos));

			tree.SubmitElement(i, enemyManager.positions[i]);
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double _dt)
	{
		enemyManager.Update(_dt);

		List<int> orphanIndices = new();
		tree.CheckDepartures(orphanIndices);

		orphanIndices.ForEach((id) => tree.SubmitElement(id, enemyManager.positions[id]));

		tree.DrawDebug();

		for(int i = 0; i < enemyManager.count; ++i)
		{
			Vector3 pos = new(enemyManager.positions[i].X, 0.5f, enemyManager.positions[i].Y);
			multiMesh.Multimesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, pos));
		}

		dtCounter += _dt;
		if(dtCounter > trimCheckTimer)
		{
			tree.CheckTrim();
			dtCounter = 0.0;
		}
	}
}
