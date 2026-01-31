using Godot;
using System;

public partial class InteractionManager : Node
{
	[Export] private GameManager gameManager;
	[Export] private float timeBeforeDrag = 0.5f;

	private double dtAccumulator = 0.0f;
	private bool mainInteractionPressed = false;
	public bool draggingInteraction { get; private set; } = false;

	public enum InteractionStatus
	{
		Default, // TODO Select buildings, nothing for now
		Construction // Place and update building ghosts
	};

	public InteractionStatus currentInteractionStatus = InteractionStatus.Default;
	public Vector3 dragStart { get; private set; } = Vector3.Zero;

	public void StartMainInteraction()
	{
		if(currentInteractionStatus == InteractionStatus.Default)
			return;

		// Main clic has just been pressed
		mainInteractionPressed = true;
		draggingInteraction = false;
		dtAccumulator = 0.0f;
		dragStart = InputManager.GetMousePosOnGamePlane();
	}

	public void EndMainInteraction()
	{
		if(currentInteractionStatus == InteractionStatus.Default)
			return;

		// Main clic has been released
		if(draggingInteraction)
		{
			gameManager.ConstructBuildingOrder();
		}
		else
		{
			gameManager.ConstructBuildingOrder();
		}

		mainInteractionPressed = false;
		draggingInteraction = false;
	}

	public void CancelInteraction()
	{
		if(currentInteractionStatus == InteractionStatus.Default)
			return;

		gameManager.AbortBuildingMode();

		draggingInteraction = false;
		mainInteractionPressed = false;
	}

	public override void _Process(double _dt)
	{
		if(mainInteractionPressed == false || draggingInteraction)
			return;

		// Counting to detect if we need to swap into drag interaction mode
		dtAccumulator += _dt;
		if(dtAccumulator > timeBeforeDrag)
		{
			draggingInteraction = true;
		}
	}
}
