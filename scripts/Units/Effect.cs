using Godot;
using System;
using System.Collections.Generic;

public class Effect
{
	private List<Effect> effectsApplied = new();
	private float damage = 0.0f;
	private ResourcesManager.Price cost;
}
