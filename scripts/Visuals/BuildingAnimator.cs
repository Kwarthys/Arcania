using Godot;
using System;
using System.Collections.Generic;

public partial class BuildingAnimator : Node3D
{
	[Export] private Godot.Collections.Array<Node3D> weapons = null;

	public void Update(List<Vector3> _weaponsTargetPos)
	{
		if(weapons == null)
			return;

		if(_weaponsTargetPos.Count != weapons.Count)
			throw new Exception("BuildingAnimator :: Update was geiven incompatible lists: " + _weaponsTargetPos.Count + " targets for " + weapons.Count + " weapons");

		for(int i = 0; i < weapons.Count; ++i)
		{
			if(_weaponsTargetPos[i].Y > -10) // for now, don't move is encoded as negative Y
				weapons[i].LookAt(_weaponsTargetPos[i], Vector3.Up);
			DrawDebugManager.DebugDrawLine(weapons[i].GlobalPosition, _weaponsTargetPos[i]);
		}
	}
}
