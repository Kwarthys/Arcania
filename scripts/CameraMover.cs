using Godot;
using System;
using System.Collections.Generic;

public partial class CameraMover : Node3D
{
	[Export] private float moveSpeed = 2.0f;
	private int moveFlags = 0;
	private const int FLAG_UP = 0x1;
	private const int FLAG_DOWN = 0x10;
	private const int FLAG_LEFT = 0x100;
	private const int FLAG_RIGHT = 0x1000;

	public override void _Process(double _dt)
	{
		Vector2 movement = Vector2.Zero;
		if(HasFlag(FLAG_UP))
			movement.Y += 1.0f;
		if(HasFlag(FLAG_DOWN))
			movement.Y += -1.0f;

		if(HasFlag(FLAG_LEFT))
			movement.X += 1.0f;
		if(HasFlag(FLAG_RIGHT))
			movement.X += -1.0f;

		movement = movement.Normalized() * (float)_dt * moveSpeed;

		Translate(new(movement.X, 0.0f, movement.Y));
	}

	private Dictionary<string, int> actionToFlag = new()
	{
		{"Up", FLAG_UP},
		{"Down", FLAG_DOWN},
		{"Left", FLAG_LEFT},
		{"Right", FLAG_RIGHT}
	};

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		foreach(KeyValuePair<string, int> pair in actionToFlag)
		{
			if(@event.IsActionPressed(pair.Key))
			{
				AddFlag(pair.Value);
				return;
			}
			else if(@event.IsActionReleased(pair.Key))
			{
				if(Input.IsActionPressed(pair.Key) == false)
					RemoveFlag(pair.Value); // Only remove flag if all associated keys are released
				return;
			}
		}
	}

	private void AddFlag(int _flag)
	{
		moveFlags |= _flag;
	}

	private void RemoveFlag(int _flag)
	{
		moveFlags &= ~_flag;
	}

	private bool HasFlag(int _flag)
	{
		return (moveFlags & _flag) != 0;
	}

}
