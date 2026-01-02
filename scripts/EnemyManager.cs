using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager
{
	public List<float> healths { get; private set; } = new();
	public List<Vector2> positions { get; private set; } = new();
	public List<Vector2> speeds { get; private set; } = new();
	public int count { get; private set; } = 0;
	public int aliveCount { get; private set; } = 0;

	private const float availableTimerStartValue = -100.0f;
	private const float availableTimerEndValue = -80.0f; // 20 frames later
	private GameManager gameManager = null;

	// Temporary hard coded wave controls
	private float dtAccumulator = 0.0f;
	private float waveTimer = 5.0f;
	private int waveSize = 10;

	public void Initialize(GameManager _gameManager, int _numberOfUnit)
	{
		gameManager = _gameManager;

		healths.Clear();
		positions.Clear();

		// Random positions
		for(int i = 0; i < _numberOfUnit; ++i)
		{
			AddNewSlot();
			SetupUnit(i);
		}
	}

	private int AddNewSlot()
	{
		healths.Add(0.0f);
		positions.Add(new());
		speeds.Add(new());

		return count++; // As we return the index of the slot, increase total count after
	}

	private void SetupUnit(int _id)
	{
		healths[_id] = 100.0f;
		float posX = GD.Randf() * GD.Randf() * 50.0f;
		float posY = GD.Randf() * GD.Randf() * 50.0f;
		positions[_id] = new(posX, posY);

		float speedNormal = GD.Randf() * 4.0f + 1.0f;
		speeds[_id] = new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f).Normalized() * speedNormal;
	}

	public void Update(double _dt)
	{
		float dt = (float)_dt;
		ManageWaveSpawn(dt); // Spawn the wave before proper update to make sure the newly spawned are taken into account in aliveCount

		aliveCount = 0;
		for(int i = 0; i < healths.Count; ++i)
		{
			if(healths[i] <= 0.0)
			{
				if(healths[i] <= availableTimerEndValue)
				{
					healths[i] += 1.0f; // cruise toward timer end value
				}
				continue;
			}

			aliveCount++; // using the update to keep track of how many enemies are still alive

			bool flipX = false;
			bool flipY = false;

			if(positions[i].X > 49.0f && speeds[i].X > 0.0f)
				flipX = true;
			else if(positions[i].X < 1.0f && speeds[i].X < 0.0f)
				flipX = true;

			if(positions[i].Y > 49.0f && speeds[i].Y > 0.0f)
				flipY = true;
			else if(positions[i].Y < 1.0f && speeds[i].Y < 0.0f)
				flipY = true;

			if(flipX || flipY)
				speeds[i] = new(speeds[i].X * (flipX ? -1.0f : 1.0f), speeds[i].Y * (flipY ? -1.0f : 1.0f));

			positions[i] += speeds[i] * dt;
		}
	}

	public void Spawn(int _amount)
	{
		// First look for already created empty slots
		List<int> spawnedIndices = new();

		for(int i = 0; i < count && spawnedIndices.Count < _amount; ++i) // while we have room to search and we did not spawn enough
		{
			if(SlotAvailable(i) == false)
				continue;

			//Slot is Available
			SetupUnit(i);
			spawnedIndices.Add(i);
		}

		// if not enough slots, create more
		while(spawnedIndices.Count < _amount)
		{
			int id = AddNewSlot();
			SetupUnit(id);
			spawnedIndices.Add(id);
		}

		gameManager.OnEnemySpawn(spawnedIndices);
	}

	public void Damage(List<int> _ids, float _amount) { _ids.ForEach((id) => Damage(id, _amount)); }

	public void Damage(int _id, float _amount)
	{
		healths[_id] -= _amount;

		if(Alive(_id) == false)
		{
			healths[_id] = availableTimerStartValue; // Start timer before slot is available
		}
	}

	public Vector2 GetPosition(int _id) { return positions[_id]; }
	public float GetHealth(int _id) { return healths[_id]; }
	public bool Alive(int _id) { return healths[_id] > 0.0; }

	private bool SlotAvailable(int _id)
	{
		if(Alive(_id))
			return false;
		// Make sure some time passed for all systems to clear their reference to this index before reusing it
		return healths[_id] > availableTimerEndValue; // startValue < endValue, as we increase the value, we check if it's high enough
	}

	private void ManageWaveSpawn(float _dt)
	{
		dtAccumulator += _dt;
		if(dtAccumulator > waveTimer)
		{
			dtAccumulator -= waveTimer;
			Spawn(waveSize);
		}
	}
}
