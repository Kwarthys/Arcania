using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Resources;

public partial class GameManager : Node
{
	[Export] private double trimCheckTimer = 5.0;
	[Export] private string BuildingsDataPath;
	[Export] public int gridSize { get; private set; } = 50;
	[Export] public int buildingGridMargin { get; private set; } = 2;
	[Export] private bool drawTreeDebug = false;
	[Export] private bool drawBuildingGrid = false;
	[Export] private ModelsDisplayer displayer;
	[Export] private BuilderMenu builderMenu;
	[Export] private InteractionManager interactionManager;
	public Vector2 gridCenter { get; private set; } = Vector2.Zero;
	private BuildingsManager buildingsManager = new();
	private EnemyManager enemyManager = new();
	private ResourcesManager resourcesManager = new();
	private double dtCounter = 0.0;

	private QuadTree tree;

	public override void _Ready()
	{
		gridCenter = new Vector2(gridSize, gridSize) * 0.5f;

		enemyManager.Initialize(this, buildingsManager);

		tree = new(new(0.0f, 0.0f, gridSize, gridSize), enemyManager);

		for(int i = 0; i < enemyManager.count; ++i)
			tree.SubmitElement(i, enemyManager.GetPosition(i));

		buildingsManager.Initialize(this, enemyManager, resourcesManager, tree);
		buildingsManager.LoadData(BuildingsDataPath);

		displayer.Initialize(-gridCenter, buildingsManager.allBuildingNames);

		builderMenu.Initialize(this, buildingsManager.allBuildableBuildingNames);

		resourcesManager.playerResources[ResourcesManager.Resource.Mana] += 100.0f; // bypass ceil

		buildingsManager.AddBuilding(displayer.WorldToGrid(new(0, 0, 0)), "Nexus"); // Central starting building
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double _dt)
	{
		DrawDebugManager.Reset();

		enemyManager.Update(_dt);
		buildingsManager.Update(_dt);

		ResourceDisplayManager.Instance.Update(resourcesManager);
		UpdateQuadTree(_dt);
		displayer.Update(enemyManager);

		UpdateConstructionGhosts();

		if(drawBuildingGrid)
			DrawDebugBuildingGrid();
	}

	public void ConstructBuildingOrder()
	{
		if(builderMenu.selectedBuilding == "")
			return;

		Vector2I gridPos = displayer.WorldToGrid(InputManager.GetMousePosOnGamePlane());
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
			JSONFormats.Building staticData = buildingsManager.GetBuildingStaticData(_name);
			// store there for later
			builderMenu.selectedBuildingStaticData = staticData;
			// Offset the center of the ghost only for buildings that sit on the center of a grid cell -> all the odd width/height
			// Even width/height buildings sit on grid cell edges
			offsetGhostCenter = staticData.Footprint.X % 2 != 0;

			interactionManager.currentInteractionStatus = InteractionManager.InteractionStatus.Construction;
		}
		else
		{
			interactionManager.currentInteractionStatus = InteractionManager.InteractionStatus.Default;
		}

		displayer.UpdateBuildingGhost(_name, offsetGhostCenter);
	}

	public void AbortBuildingMode()
	{
		builderMenu.Cancel();
	}

	private void UpdateConstructionGhosts()
	{
		if(interactionManager.currentInteractionStatus != InteractionManager.InteractionStatus.Construction)
			return;

		Vector3 mouseWorldPos = InputManager.GetMousePosOnGamePlane();
		if(interactionManager.draggingInteraction)
		{
			JSONFormats.Building staticData = builderMenu.selectedBuildingStaticData;
			Vector3 dragVector = mouseWorldPos - interactionManager.dragStart;

			Vector3 offset = Vector3.Zero;
			int number = 0;
			if(Mathf.Abs(dragVector.X) > Mathf.Abs(dragVector.Z))
			{
				number = Mathf.FloorToInt(Mathf.Abs(dragVector.X) / staticData.Footprint.X);
				offset.X = staticData.Footprint.X;
				if(dragVector.X < 0.0f)
					offset.X *= -1.0f;
			}
			else
			{
				number = Mathf.FloorToInt(Mathf.Abs(dragVector.Z) / staticData.Footprint.Y);
				offset.Z = staticData.Footprint.Y;
				if(dragVector.Z < 0.0f)
					offset.Z *= -1.0f;
			}

			List<Vector3> positions = [];
			for(int i = 0; i < number + 1; ++i)
			{
				positions.Add(interactionManager.dragStart + i * offset);
			}

			displayer.PlaceGhosts(positions);
		}
		else
		{
			displayer.MoveGhost(mouseWorldPos);
		}
	}

	private void UpdateQuadTree(double _dt)
	{
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
	}

	private void DrawDebugBuildingGrid()
	{
		Vector2 offset = displayer.gridStart;
		Vector2 size = new(tree.root.boundingBox.w, tree.root.boundingBox.h);
		float height = 0.1f;

		for(int y = 0; y <= tree.root.boundingBox.h; ++y)
		{
			if(y < buildingGridMargin || y > size.Y - buildingGridMargin)
			{
				if(y != 0 && y != size.Y)
					continue;

				//Draw horizontal line, x = 0 to x max on Y
				DrawDebugManager.DebugDrawLine(new Vector3(offset.X, height, offset.Y + y), new Vector3(offset.X + size.X, height, offset.Y + y));
			}
			else
			{
				//Draw horizontal line only inside the margins
				DrawDebugManager.DebugDrawLine(new Vector3(offset.X + buildingGridMargin, height, offset.Y + y), new Vector3(offset.X + size.X - buildingGridMargin, height, offset.Y + y));
			}

		}
		for(int x = 0; x <= tree.root.boundingBox.w; ++x)
		{
			if(x < buildingGridMargin || x > size.X - buildingGridMargin)
			{
				if(x != 0 && x != size.X)
					continue;

				//Draw vertical line, y = 0 to y max on X
				DrawDebugManager.DebugDrawLine(new Vector3(offset.X + x, height, offset.Y), new Vector3(offset.X + x, height, offset.Y + size.Y));
			}
			else
			{
				//Draw vertical line, only inside the margins
				DrawDebugManager.DebugDrawLine(new Vector3(offset.X + x, height, offset.Y + buildingGridMargin), new Vector3(offset.X + x, height, offset.Y + size.Y - buildingGridMargin));
			}
		}
	}
}
