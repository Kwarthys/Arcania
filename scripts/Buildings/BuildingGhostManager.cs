using Godot;
using System;
using System.Net.Mail;

public class BuildingGhostManager
{
	private Node3D currentGhost = null;
	private ModelsDisplayer modelsDisplayer = null;

	public void Initialize(ModelsDisplayer _modelsDisplayer)
	{
		modelsDisplayer = _modelsDisplayer;
	}

	public void UpdateGhost(Vector3 _pos)
	{
		if(currentGhost == null)
			return;

		Vector3 snapedPos = modelsDisplayer.SnapToGrid(_pos);
		currentGhost.Position = snapedPos;
	}

	public void ChangeGhost(Node3D _model)
	{
		currentGhost?.QueueFree();
		currentGhost = _model;
	}
}
