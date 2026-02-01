using Godot;
using System;
using System.Collections.Generic;

public partial class ModelsDisplayer : Node
{
	[Export] Mesh unitsMesh;
	[Export] public string buildingModelsPath { get; private set; }
	[Export] public Material constructionMaterial { get; private set; }
	[Export] public PackedScene placeHolderBuildingModel { get; private set; }
	public Vector2 gridStart { get; private set; } = new(-25.0f, -25.0f);

	private BuildingsDisplayer buildingsDisplayer;
	private UnitsDisplayer unitsDisplayer;

	public override void _Ready()
	{
		unitsDisplayer = new();
		AddChild(unitsDisplayer);

		buildingsDisplayer = new();
		AddChild(buildingsDisplayer);
	}

	public void Initialize(Vector2 _gridStart, List<string> _buildingNames)
	{
		gridStart = _gridStart;
		unitsDisplayer.Initialize(this, unitsMesh);
		buildingsDisplayer.Initialize(this, _buildingNames);
	}

	public void Update(EnemyManager _enemyManager)
	{
		unitsDisplayer.UpdateDisplay(_enemyManager);
		buildingsDisplayer.Update(_enemyManager);
	}
	public void AddBuilding(Building _b) { buildingsDisplayer?.AddBuilding(_b); }
	public void RemoveBuilding(Building _b) { buildingsDisplayer?.RemoveBuilding(_b); }
	public void MoveGhost(Vector3 _worldPos) { buildingsDisplayer?.MoveGhost(_worldPos); }
	public void PlaceGhosts(List<Vector3> _positions) { buildingsDisplayer?.PlaceGhosts(_positions); }
	public void UpdateBuildingGhost(string _name, bool _offsetCenter) { buildingsDisplayer?.ChangeGhost(_name, _offsetCenter); }
	public List<Node3D> TransferGhosts(int _count) { return buildingsDisplayer.TransferGhosts(_count); }

	public Vector3 GridToWorld(Vector2I _gridPos)
	{
		return GridToWorld(new Vector2(_gridPos.X, _gridPos.Y));
	}

	public Vector3 GridToWorld(Vector2 _gridPos)
	{
		float x = _gridPos.X + gridStart.X;
		float y = 0.0f;
		float z = _gridPos.Y + gridStart.Y;
		return new(x, y, z);
	}

	public Vector2I WorldToGrid(Vector3 _worldPos)
	{
		int x = Mathf.RoundToInt(_worldPos.X - gridStart.X);
		int y = Mathf.RoundToInt(_worldPos.Z - gridStart.Y);
		return new(x, y);
	}

	public Vector3 SnapToGrid(Vector3 _worldPos)
	{
		return new(Mathf.Round(_worldPos.X), 0.0f, Mathf.Round(_worldPos.Z));
	}
}
