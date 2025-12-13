using Godot;
using System;
using System.Collections.Generic;
using System.Resources;

public partial class GameManager : Node
{
	[Export] private double trimCheckTimer = 5.0;
	[Export] private string BuildingsDataPath;
	[Export] bool drawTreeDebug = false;
	[Export] ModelsDisplayer displayer;
	private BuildingsManager buildingsManager = new();
	private EnemyManager enemyManager = new();
	private ResourcesManager resourcesManager = new();
	private double dtCounter = 0.0;

	private QuadTree tree;

	public override void _Ready()
	{
		int n = 1000;
		enemyManager.Initialize(n);

		tree = new(new(0.0f, 0.0f, 50.0f, 50.0f), enemyManager);

		for(int i = 0; i < enemyManager.count; ++i)
			tree.SubmitElement(i, enemyManager.GetPosition(i));

		buildingsManager.Initialize(this, enemyManager, resourcesManager, tree, new(50, 50));
		buildingsManager.LoadData(BuildingsDataPath);

		displayer.Initialize(new(tree.root.boundingBox.w * -0.5f, tree.root.boundingBox.h * -0.5f));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double _dt)
	{
		DrawDebugManager.Reset();

		enemyManager.Update(_dt);
		buildingsManager.Update(_dt);

		ResourceDisplayManager.Instance.Update(resourcesManager.playerResources);

		/** Quad Tree Update **/
		List<int> orphanIndices = new();
		tree.CheckDepartures(orphanIndices);

		orphanIndices.ForEach((id) => tree.SubmitElement(id, enemyManager.positions[id]));

		if(drawTreeDebug)
			tree.DrawDebug();

		dtCounter += _dt;
		if(dtCounter > trimCheckTimer)
		{
			tree.CheckTrim();
			dtCounter = 0.0;
		}
		/**					**/

		displayer.Update(enemyManager);
	}

	public void AddBuilding(Vector3 _pos, bool _isTower)
	{
		Vector2I gridPos = displayer.WorldToGrid(_pos);
		buildingsManager.AddBuilding(gridPos, _isTower);
	}

	public void OnBuildingAdded(Building _b) { displayer.AddBuilding(_b); }
	public void OnBuildingRemoved(Building _b) { displayer.RemoveBuilding(_b); }
}
