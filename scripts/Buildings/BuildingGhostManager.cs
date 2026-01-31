using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BuildingGhostManager : Node
{
	private List<Node3D> ghosts = [];
	private ModelsDisplayer modelsDisplayer = null;
	private bool offsetGhostCenter = false;
	private string buildingName = "";
	private static Vector3 HIDDEN_POSITION = new(0.0f, -10000.0f, 0.0f);

	public void Initialize(ModelsDisplayer _modelsDisplayer)
	{
		modelsDisplayer = _modelsDisplayer;
	}

	public void UpdateGhost(Vector3 _pos)
	{
		if(ghosts.Count == 0)
			return;

		if(ghosts.Count > 1)
		{
			for(int i = 1; i < ghosts.Count; ++i)
				ghosts[i].QueueFree();
			ghosts.RemoveRange(1, ghosts.Count - 1);
		}

		ghosts[0].Position = CorrectGhostPos(_pos);
	}

	public void PlaceGhosts(List<Vector3> _positions, Func<string, Node3D> _requestNewModel)
	{
		// Create missing ghosts if needed
		while(_positions.Count > ghosts.Count)
		{
			ghosts.Add(_requestNewModel(buildingName));
			if(ghosts.Last() != null)
				AddChild(ghosts.Last());
		}

		// place ghosts
		for(int i = 0; i < _positions.Count; ++i)
		{
			ghosts[i].Position = CorrectGhostPos(_positions[i]);
		}

		// Move all unnecessary instantiated ghosts far from view
		for(int i = _positions.Count; i < ghosts.Count; ++i)
		{
			ghosts[i].Position = HIDDEN_POSITION;
		}
	}

	public void ChangeGhost(string _buildingName, Node3D _model, bool _offsetCenter)
	{
		foreach(Node3D n in ghosts)
			n.QueueFree();
		ghosts.Clear();

		buildingName = _buildingName;

		if(_model != null)
		{
			ghosts.Add(_model);
			AddChild(_model);
		}

		offsetGhostCenter = _offsetCenter;
	}

	private Vector3 CorrectGhostPos(Vector3 _pos)
	{
		Vector3 snapedPos = modelsDisplayer.SnapToGrid(_pos);
		if(offsetGhostCenter)
		{
			snapedPos.X += 0.5f;
			snapedPos.Z += 0.5f;
		}
		return snapedPos;
	}
}
