using Godot;
using System;

public class Constructor
{
	public Price cost { get; private set; }
	public float buildTime { get; private set; }
	public float completion { get; private set; } = 0.0f;

	private Price costAccumulator = new();

	public Constructor(Price _cost, float _buildTime)
	{
		cost = _cost;
		buildTime = _buildTime;
	}

	public void Update(double _dt, ResourcesManager _playerResources)
	{
		_playerResources.TryConsume(_dt, cost, buildTime, ref costAccumulator, 1.0f, false);
		completion = costAccumulator.AllAboveOrEqual(cost) ? 1.0f : 0.0f; // will do fancy maths later
		return;
	}
}
