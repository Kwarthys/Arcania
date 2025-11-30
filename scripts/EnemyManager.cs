using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager
{
	public List<float> healths = new();
	public List<Vector2> positions = new();
	public List<Vector2> speeds = new();
	public int count = 0;

	public void Initialize(int _numberOfUnit)
	{
		healths.Clear();
		positions.Clear();

		// Random positions
		for(int i = 0; i < _numberOfUnit; ++i)
		{
			healths.Add(100.0f);
			float posX = (GD.Randf() * GD.Randf() - 0.5f) * 50.0f;
			float posY = (GD.Randf() * GD.Randf() - 0.5f) * 50.0f;
			positions.Add(new(posX, posY));

			float speedNormal = GD.Randf() * 5.0f + 0.2f;
			speeds.Add(new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f).Normalized() * speedNormal);
		}

		count = _numberOfUnit;
	}

	public void Update(double _dt)
	{
		float dt = (float)_dt;
		for(int i = 0; i < healths.Count; ++i)
		{
			bool flipX = false;
			bool flipY = false;

			if(positions[i].X > 24.0f && speeds[i].X > 0.0f)
				flipX = true;
			else if(positions[i].X < -24.0f && speeds[i].X < 0.0f)
				flipX = true;

			if(positions[i].Y > 24.0f && speeds[i].Y > 0.0f)
				flipY = true;
			else if(positions[i].Y < -24.0f && speeds[i].Y < 0.0f)
				flipY = true;

			if(flipX || flipY)
				speeds[i] = new(speeds[i].X * (flipX ? -1.0f : 1.0f), speeds[i].Y * (flipY ? -1.0f : 1.0f));

			positions[i] += speeds[i] * dt;
		}
	}

	public Vector2 GetPosition(int _id) { return positions[_id]; }
}
