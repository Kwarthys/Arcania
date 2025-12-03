using Godot;
using System;

public class Building
{
	public Vector2I position = new(0, 0);
	public float range = 5.0f;
	public float damage = 100.0f;
	public double fireDTCounter = 0.0;
	public float firePeriod = 2.0f;
	public int targetIndex = -1;
}
