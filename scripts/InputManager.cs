using Godot;
using Godot.Collections;
using System;

public partial class InputManager : Node3D
{
	[Export] private Camera3D camera;
	[Export] private InteractionManager interactionManager;

	private static InputManager Instance;
	public override void _Ready() { Instance = this; }
	public override void _UnhandledInput(InputEvent @event)
	{

		if(@event.IsActionPressed("MainInteraction"))
		{
			interactionManager.StartMainInteraction();
		}
		else if(@event.IsActionReleased("MainInteraction"))
		{
			interactionManager.EndMainInteraction();
		}

		if(@event.IsActionReleased("SecondaryInteraction"))
		{
			interactionManager.CancelInteraction();
		}
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if(@event.IsActionReleased("ui_cancel"))
		{
			interactionManager.CancelInteraction();
		}
	}

	public static Vector3 GetMousePosOnGamePlane()
	{
		if(Instance == null)
			return Vector3.Zero;

		return Instance.GetMousePosRayCast();
	}

	private Vector3 GetMousePosRayCast()
	{
		Vector2 mousePos = GetViewport().GetMousePosition();
		Vector3 from = camera.ProjectRayOrigin(mousePos);
		Vector3 to = from + camera.ProjectRayNormal(mousePos) * 500.0f;

		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas = true;
		query.CollideWithBodies = true;
		Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(query);

		if(result.Count == 0)
		{
			GD.Print("Nothing");
			return Vector3.Zero;
		}

		return (Vector3)result["position"];
	}

}
