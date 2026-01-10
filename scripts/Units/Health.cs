using Godot;
using System;

public class Health // only used for buildings for now, not sure if integrating to units would be a good idea
{
	public float currentHealth { get; private set; }
	public float maxHealth { get; private set; }
	public bool alive { get; private set; } = true;

	public Health(float _maxHealth, float _ratio = 1.0f)
	{
		maxHealth = _maxHealth;
		currentHealth = maxHealth * Mathf.Clamp(_ratio, 0.0f, 1.0f);
	}

	public void Damage(float _amount)
	{
		currentHealth -= _amount;
		CheckBounds();
	}

	public void Heal(float _amount)
	{
		currentHealth += _amount;
		CheckBounds();
	}

	private void CheckBounds()
	{
		if(currentHealth > maxHealth)
		{
			currentHealth = maxHealth;
		}

		alive = currentHealth > 0.0f;
	}
}
