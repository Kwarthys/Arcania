using Godot;
using System;

public partial class ModelsDisplayer : Node
{
	[Export] Mesh unitsMesh;
	[Export] private PackedScene turretModel;
	[Export] private PackedScene harvesterModel;
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

	public void Initialize(Vector2 _gridStart)
	{
		gridStart = _gridStart;
		unitsDisplayer.Initialize(this, unitsMesh);
		buildingsDisplayer.Initialize(this, turretModel, harvesterModel);
	}

	public void Update(EnemyManager _enemyManager)
	{
		unitsDisplayer.UpdateDisplay(_enemyManager);
		buildingsDisplayer.DrawTargetDebug(_enemyManager);
	}
	public void AddBuilding(Building _b) { buildingsDisplayer?.AddBuilding(_b); }
	public void RemoveBuilding(Building _b) { buildingsDisplayer?.RemoveBuilding(_b); }

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
		int x = Mathf.FloorToInt(_worldPos.X - gridStart.X);
		int y = Mathf.FloorToInt(_worldPos.Z - gridStart.Y);
		GD.Print(_worldPos + " -> (" + x + ", " + y + ")");
		return new(x, y);
	}
}
