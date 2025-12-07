using Godot;
using Godot.Collections;
using System;

public partial class InputManager : Node3D
{
	[Export] private Camera3D camera;
	[Export] private GameManager gameManager;

	public override void _UnhandledInput(InputEvent @event)
	{
		bool isMain = @event.IsActionReleased("MainInteraction");
		bool isSec = @event.IsActionReleased("SecondaryInteraction");
		if(isMain || isSec)
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
				return;
			}

			Vector3 worldHitPos = (Vector3)result["position"];
			gameManager.AddBuilding(worldHitPos, isMain);
		}
	}

}
