using Godot;
using System;
using System.Diagnostics.Tracing;

public partial class UnitsDisplayer : Node
{
	private MultiMeshInstance3D multiMesh;
	private ModelsDisplayer displayer;
	public override void _Ready()
	{
		multiMesh = new();
		multiMesh.Multimesh = new();
		multiMesh.Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		AddChild(multiMesh);
	}

	public void Initialize(ModelsDisplayer _displayer, Mesh _mesh)
	{
		multiMesh.Multimesh.Mesh = _mesh;
		displayer = _displayer;
	}

	public void UpdateDisplay(EnemyManager _enemyManager)
	{
		int meshesToDisplay = 0;
		for(int i = 0; i < _enemyManager.count; ++i)
		{
			if(_enemyManager.Alive(i))
				meshesToDisplay++;
		}
		multiMesh.Multimesh.InstanceCount = meshesToDisplay;

		int meshIndex = 0;
		for(int i = 0; i < _enemyManager.count; ++i)
		{
			if(_enemyManager.Alive(i) == false)
				continue;

			Vector3 worldPos = displayer.GridToWorld(_enemyManager.positions[i]) + Vector3.Up * 0.5f;
			multiMesh.Multimesh.SetInstanceTransform(meshIndex++, new Transform3D(Basis.Identity, worldPos));
		}
	}
}
