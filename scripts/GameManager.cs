using Godot;
using System;
using System.Collections.Generic;
using System.Resources;

public partial class GameManager : Node
{
	[Export] private double trimCheckTimer = 5.0;
	[Export] private string BuildingsDataPath;
	[Export] private bool drawTreeDebug = false;
	[Export] private bool drawBuildingGrid = false;
	[Export] private ModelsDisplayer displayer;
	[Export] private BuilderMenu builderMenu;
	private BuildingsManager buildingsManager = new();
	private EnemyManager enemyManager = new();
	private ResourcesManager resourcesManager = new();
	private double dtCounter = 0.0;

	private QuadTree tree;

	public override void _Ready()
	{
		int n = 10;
		enemyManager.Initialize(this, buildingsManager, n);

		tree = new(new(0.0f, 0.0f, 50.0f, 50.0f), enemyManager);

		for(int i = 0; i < enemyManager.count; ++i)
			tree.SubmitElement(i, enemyManager.GetPosition(i));

		buildingsManager.Initialize(this, enemyManager, resourcesManager, tree, new(50, 50));
		buildingsManager.LoadData(BuildingsDataPath);

		displayer.Initialize(new(tree.root.boundingBox.w * -0.5f, tree.root.boundingBox.h * -0.5f), buildingsManager.allBuildingNames);

		builderMenu.Initialize(this, buildingsManager.allBuildableBuildingNames);

		resourcesManager.Credit(new(100.0f, 0.0f, 0.0f, 0.0f)); // starting resources

		buildingsManager.AddBuilding(displayer.WorldToGrid(new(0, 0, 0)), "Nexus"); // Central starting building
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
		tree.SubmitElements(orphanIndices);

		if(drawTreeDebug)
			tree.DrawDebug(displayer.gridStart);

		dtCounter += _dt;
		if(dtCounter > trimCheckTimer)
		{
			tree.CheckTrim();
			dtCounter = 0.0;
		}
		/**					**/

		displayer.Update(enemyManager);

		if(builderMenu.selectedBuilding != "")
		{
			Vector3 mouseWorldPos = InputManager.GetMousePosOnGamePlane();
			displayer.MoveGhost(mouseWorldPos);
		}

		if(drawBuildingGrid)
		{
			Vector2 offset = displayer.gridStart;
			Vector2 size = new(tree.root.boundingBox.w, tree.root.boundingBox.h);
			float height = 0.1f;

			for(int y = 0; y <= tree.root.boundingBox.h; ++y)
			{
				//Draw horizontal line, x = 0 to x max on Y
				DrawDebugManager.DebugDrawLine(new Vector3(offset.X, height, offset.Y + y), new Vector3(offset.X + size.X, height, offset.Y + y));
			}
			for(int x = 0; x <= tree.root.boundingBox.w; ++x)
			{
				//Draw vertical line, y = 0 to y max on X
				DrawDebugManager.DebugDrawLine(new Vector3(offset.X + x, height, offset.Y), new Vector3(offset.X + x, height, offset.Y + size.Y));
			}
		}
	}

	public void AddBuilding(Vector3 _pos)
	{
		if(builderMenu.selectedBuilding == "")
			return;

		Vector2I gridPos = displayer.WorldToGrid(_pos);
		buildingsManager.AddBuilding(gridPos, builderMenu.selectedBuilding);
	}

	public void OnBuildingAdded(Building _b)
	{
		displayer.AddBuilding(_b);
		enemyManager.OnBuildingAdded(_b);
	}

	public void OnBuildingRemoved(Building _b)
	{
		displayer.RemoveBuilding(_b);
		enemyManager.OnBuildingRemoved(buildingsManager.buildings.IndexOf(_b));
	}

	public void OnEnemySpawn(List<int> _indices) { tree.SubmitElements(_indices); }
	public void OnBuildingGhostChange(string _name)
	{
		bool offsetGhostCenter = false;
		if(_name != "")
		{
			// Offset the center of the ghost only for buildings that sit on the center of a grid cell -> all the odd width/height
			// Even width/height buildings sit on grid cell edges
			JSONFormats.Building staticData = buildingsManager.GetBuildingStaticData(_name);
			offsetGhostCenter = staticData.Footprint.X % 2 != 0;
		}
		displayer.UpdateBuildingGhost(_name, offsetGhostCenter);
	}
}
