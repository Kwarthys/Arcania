using Godot;
using System;

public partial class GameManager : Node
{
	private EnemyManager enemyManager = new();
	[Export] private MultiMeshInstance3D multiMesh;
	public override void _Ready()
	{
		int n = 1000;
		enemyManager.Initialize(n);
		multiMesh.Multimesh.InstanceCount = n;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double _dt)
	{
		enemyManager.Update(_dt);

		for(int i = 0; i < enemyManager.count; ++i)
		{
			Vector3 pos = new(enemyManager.positions[i].X, 0.5f, enemyManager.positions[i].Y);
			multiMesh.Multimesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, pos));
		}
	}
}
