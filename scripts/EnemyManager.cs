using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager
{
	public int count { get; private set; } = 0;
	public int aliveCount { get; private set; } = 0;

	private const float availableTimerStartValue = -100.0f;
	private const float availableTimerEndValue = -80.0f; // 20 frames later
	private GameManager gameManager = null;
	private BuildingsManager buildingsManager = null;

	// Temporary hard coded wave controls
	private float dtAccumulator = 0.0f;
	private float waveTimer = 50.0f;
	private int waveSize = 1;

	private float spawnRadius = 50.0f;

	public class Units
	{
		public List<float> healths { get; private set; } = new();
		public List<Vector2> positions { get; private set; } = new();
		public List<Vector2> speeds { get; private set; } = new();
		public List<int> targetIndex { get; private set; } = new();
		public List<float> stoppingDistanceToTarget { get; private set; } = new();
	}
	public Units units = null;

	public void Initialize(GameManager _gameManager, BuildingsManager _buildingsManager)
	{
		gameManager = _gameManager;
		buildingsManager = _buildingsManager;

		units = new();
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

	private void SetupUnit(int _id, Vector2 _pos)
	{
		units.healths[_id] = 100.0f;
		units.positions[_id] = _pos;

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
				{
					// Already on target
					// Damage it !
					// target.health.Damage(0.1f); Clipping the claws for debugging
					continue;
				}

				units.positions[i] += dt * units.speeds[i];
			}
		}
	}

	public void Spawn(List<Vector2> _positions)
	{
		int amount = _positions.Count;
		int posIndex = 0; // index of the current position to use in _positions array

		// First look for already created empty slots
		List<int> spawnedIndices = new();

		for(int i = 0; i < count && spawnedIndices.Count < amount; ++i) // while we have room to search and we did not spawn enough
		{
			if(SlotAvailable(i) == false)
				continue;

			//Slot is Available
			SetupUnit(i, _positions[posIndex++]);
			spawnedIndices.Add(i);
		}

		// if not enough slots, create more
		while(spawnedIndices.Count < amount)
		{
			int id = AddNewSlot();
			SetupUnit(id, _positions[posIndex++]);
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

	public void OnBuildingRemoved(int _buildingIndex)
	{
		for(int i = 0; i < count; ++i)
		{
			if(units.targetIndex[i] == _buildingIndex)
				units.targetIndex[i] = -1; // Will target a new building in the next update
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

			List<Vector2> positions = new();

			// Find a starting pos for our wave
			float a = GD.Randf() * Mathf.Tau;
			for(int i = 0; i < waveSize; ++i)
			{
				// Angle to position + distance slight variation
				Vector2 position = new(Mathf.Cos(a), Mathf.Sin(a));
				position *= GD.Randf() * 0.05f + 0.95f; // 5% variation
				position *= spawnRadius;
				position += gameManager.gridCenter;
				positions.Add(position);

				// Angle increment by a temporary fixed value
				a += Mathf.Tau / 300.0f; // We say we put 300 units in a full circle
			}

			Spawn(positions);
			waveSize++;
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
				if(candidate == null)
					continue;

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

		Vector2 deltaPos = _b.GetCenterPosition() - units.positions[_id];
		Vector2 deltaNorm = deltaPos.Normalized();

		// Update speed toward target, as buildings don't move just compute it once
		float speedNorm = units.speeds[_id].Length();
		units.speeds[_id] = deltaPos.Normalized() * speedNorm;

		// Compute stopping distance to touch building bbox
		Vector2 sideNormal;
		float fixedSize;
		if(Mathf.Abs(deltaPos.X) / _b.bbox.w > Mathf.Abs(deltaPos.Y) / _b.bbox.h)
		{
			// Hit on vertical side
			sideNormal = deltaPos.Y > 0.0f ? Vector2.Left : Vector2.Right;
			fixedSize = _b.bbox.w * 0.5f;
		}
		else
		{
			// Hit on horizontal side
			sideNormal = deltaPos.X > 0.0f ? Vector2.Down : Vector2.Up;
			fixedSize = _b.bbox.h * 0.5f;
		}

		units.stoppingDistanceToTarget[_id] = fixedSize / deltaNorm.Dot(sideNormal);
	}
}
