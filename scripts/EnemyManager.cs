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
			float posX = (GD.Randf() - 0.5f) * 50.0f;
			float posY = (GD.Randf() - 0.5f) * 50.0f;
			positions.Add(new(posX, posY));

			float speedNormal = GD.Randf() * 5.0f + 0.2f;
			speeds.Add(positions.Last().Normalized() * speedNormal * -1.0f);
		}

		count = _numberOfUnit;
	}

	public void Update(double _dt)
	{
		float dt = (float)_dt;
		for(int i = 0; i < healths.Count; ++i)
		{
			positions[i] += speeds[i] * dt;

			if(positions[i].LengthSquared() < 1.0f)
			{
				positions[i] = speeds[i] * -10.0f;
			}
		}
	}
}
