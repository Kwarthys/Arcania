using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager
{
	public class Units
	{
		public List<float> healths { get; private set; } = new();
		public List<Vector2> positions { get; private set; } = new();
		public List<Vector2> speeds { get; private set; } = new();
		public List<int> targetIndex { get; private set; } = new();
		public List<float> stoppingDistanceToTarget { get; private set; } = new();
	}

	public Units units = null;
	public int count { get; private set; } = 0;
	public int aliveCount { get; private set; } = 0;

	private const float availableTimerStartValue = -100.0f;
	private const float availableTimerEndValue = -80.0f; // 20 frames later
	private GameManager gameManager = null;
	private BuildingsManager buildingsManager = null;

	// Temporary hard coded wave controls
	private float dtAccumulator = 0.0f;
	private float waveTimer = 0.5f;
	private int waveSize = 1;

	public void Initialize(GameManager _gameManager, BuildingsManager _buildingsManager, int _numberOfUnit)
	{
		gameManager = _gameManager;
		buildingsManager = _buildingsManager;

		units = new();

		// Random positions
		for(int i = 0; i < _numberOfUnit; ++i)
		{
			AddNewSlot();
			SetupUnit(i);
		}
	}

	private int AddNewSlot()
	{
		units.healths.Add(0.0f);
		units.positions.Add(new());
		units.speeds.Add(new());
		units.targetIndex.Add(-1);
		units.stoppingDistanceToTarget.Add(1.0f);

		return count++; // As we return the index of the slot, increase total count after
	}

	private void SetupUnit(int _id)
	{
		units.healths[_id] = 100.0f;
		float posX = GD.Randf() * 50.0f;
		float posY = GD.Randf() * 50.0f;
		units.positions[_id] = new(posX, posY);

		float speedNormal = GD.Randf() * 4.0f + 1.0f;
		units.speeds[_id] = new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f).Normalized() * speedNormal;

		units.targetIndex[_id] = -1;
	}

	public void Update(double _dt)
	{
		float dt = (float)_dt;
		ManageWaveSpawn(dt); // Spawn the wave before proper update to make sure the newly spawned are taken into account in aliveCount
		FindTargets();

		aliveCount = 0;
		for(int i = 0; i < units.healths.Count; ++i)
		{
			if(units.healths[i] <= 0.0)
			{
				if(units.healths[i] <= availableTimerEndValue)
				{
					units.healths[i] += 1.0f; // cruise toward timer end value
				}
				continue;
			}

			aliveCount++; // using the update to keep track of how many enemies are still alive

			if(HasTarget(i))
			{
				Building target = buildingsManager.buildings[units.targetIndex[i]];
				Vector2 targetPos = target.GetCenterPosition();
				float stoppingDistanceSquared = units.stoppingDistanceToTarget[i];
				stoppingDistanceSquared *= stoppingDistanceSquared;

				if((GetPosition(i) - targetPos).LengthSquared() < stoppingDistanceSquared)
					continue; // Already on target

				units.positions[i] += dt * units.speeds[i];
			}
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
		units.healths[_id] -= _amount;

		if(Alive(_id) == false)
		{
			units.healths[_id] = availableTimerStartValue; // Start timer before slot is available
		}
	}

	public Vector2 GetPosition(int _id) { return units.positions[_id]; }
	public float GetHealth(int _id) { return units.healths[_id]; }
	public bool Alive(int _id) { return units.healths[_id] > 0.0; }
	public bool HasTarget(int _id) { return units.targetIndex[_id] >= 0; }

	public void OnBuildingAdded(Building _newBuilding)
	{
		for(int i = 0; i < count; ++i)
		{
			if(Alive(i) == false)
				continue;
			if(HasTarget(i) == false)
				continue; // this one will target a building in the regular update

			Vector2 targetPos = buildingsManager.buildings[units.targetIndex[i]].GetCenterPosition();
			Vector2 ownPos = GetPosition(i);

			if((ownPos - targetPos).LengthSquared() > (ownPos - _newBuilding.GetCenterPosition()).LengthSquared())
			{
				// Newly placed building is closer than current target, change for it
				SetTarget(i, _newBuilding);
			}
		}
	}

	private bool SlotAvailable(int _id)
	{
		if(Alive(_id))
			return false;
		// Make sure some time passed for all systems to clear their reference to this index before reusing it
		return units.healths[_id] > availableTimerEndValue; // startValue < endValue, as we increase the value, we check if it's high enough
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

	private void FindTargets()
	{
		for(int i = 0; i < count; ++i)
		{
			if(Alive(i) == false)
				continue;
			if(HasTarget(i))
				continue;

			Building closest = null;
			float closestDistSquared = 0.0f;
			foreach(Building candidate in buildingsManager.buildings)
			{
				float distSquared = (candidate.GetCenterPosition() - GetPosition(i)).LengthSquared();
				if(closest == null || distSquared < closestDistSquared)
				{
					closest = candidate;
					closestDistSquared = distSquared;
				}
			}

			if(closest != null)
				SetTarget(i, closest);
		}
	}

	private void SetTarget(int _id, Building _b)
	{
		units.targetIndex[_id] = buildingsManager.buildings.IndexOf(_b);

		// Update speed toward target, as buildings don't move just compute it once
		float speedNorm = units.speeds[_id].Length();
		units.speeds[_id] = (_b.GetCenterPosition() - units.positions[_id]).Normalized() * speedNorm;

		units.stoppingDistanceToTarget[_id] = 0.5f * _b.bbox.w * Mathf.Sqrt2; // Only working for square buildings, will do for now
	}
}
