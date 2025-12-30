using Godot;
using System;
using System.Net.Mail;

public class BuildingGhostManager
{
	private Node3D currentGhost = null;
	private ModelsDisplayer modelsDisplayer = null;
	private bool offsetGhostCenter = false;

	public void Initialize(ModelsDisplayer _modelsDisplayer)
	{
		modelsDisplayer = _modelsDisplayer;
	}

	public void UpdateGhost(Vector3 _pos)
	{
		if(currentGhost == null)
			return;

		Vector3 snapedPos = modelsDisplayer.SnapToGrid(_pos);
		if(offsetGhostCenter)
		{
			snapedPos.X += 0.5f;
			snapedPos.Z += 0.5f;
		}
		currentGhost.Position = snapedPos;
	}

	public void ChangeGhost(Node3D _model, bool _offsetCenter)
	{
		currentGhost?.QueueFree();
		currentGhost = _model;
		offsetGhostCenter = _offsetCenter;
	}
}
