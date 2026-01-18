using Godot;
using System;
using System.Collections.Generic;

public class Effect // Used to represent both damaging and economical powers of buildings
{
	public Price gain;
	public Price cost;
	private Price costAccumulator = new();
	public float period;
	//public float damage;
	//public float activityRatio = 1.0f; // Buildings can be slowed down to preserve resources
	public float dtAccumulator = 0.0f;
	public bool timerBasedPeriod = true;
	//public List<Effect> nestedEffects = new();

	public Effect(JSONFormats.Effect _effectData)
	{
		gain = new(_effectData.Gain);
		cost = new(_effectData.Cost);
		// damage = _effectData.Damage; // simplified for now
		period = _effectData.Period;

		// simplified for now
		//foreach(JSONFormats.Effect nestedEffect in _effectData.Effects)
		//{
		//	nestedEffects.Add(new(nestedEffect));
		//}

		// Finished reading, now precompute some stuff

		// If no cost, period is only based on time.
		// With cost, this cost will be consumed by the building during the given period, no need for timers
		timerBasedPeriod = cost.IsZero();
	}

	public void Update(double _dt, ResourcesManager _playerResources)
	{
		if(timerBasedPeriod)
		{
			dtAccumulator += (float)_dt;
			if(dtAccumulator > period)
			{
				dtAccumulator -= period;
				_playerResources.Credit(gain);
			}
		}
		else
		{
			if(costAccumulator.CanPay(cost))
			{
				if(_playerResources.CanStore(gain))
				{
					costAccumulator -= cost;
					_playerResources.Credit(gain);
				}
			}

			if(costAccumulator.CanPay(cost) == false) // Only consume resources if not blocked by storage limits
			{
				_playerResources.TryConsume(_dt, cost, period, ref costAccumulator);
			}
		}
	}
}
