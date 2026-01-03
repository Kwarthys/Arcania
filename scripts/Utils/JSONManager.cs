
using Godot;
using System.Collections.Generic;
using System.Text.Json;

public class JSONManager
{
	public static T Read<T>(string filePath)
	{
		string text = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read).GetAsText();
		return JsonSerializer.Deserialize<T>(text);
	}
}

namespace JSONFormats
{
	public class BuildingsData
	{
		public IList<Building> Buildings { get; set; }
	}

	public class Building
	{
		public string Name { get; set; }
		public float Health { get; set; }
		public Footprint Footprint { get; set; }
		public Cost Cost { get; set; }
		public float BuildTime { get; set; }
		public IList<Effect> Effects { get; set; } // <- Effects applied on the building
		public IList<Weapon> Weapons { get; set; } // <- Effects applied on enemy targets
		public bool Buildable { get; set; } = true;
	}

	public class Cost
	{
		public float Mana { get; set; } = 0.0f;
		public float Fire { get; set; } = 0.0f;
		public float Elec { get; set; } = 0.0f;
		public float Stone { get; set; } = 0.0f;
	}

	public class Footprint
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class Weapon // <- Applies effects to closest target
	{
		public string Type { get; set; } = "Projectile";
		public float Range { get; set; }
		public IList<Effect> Effects { get; set; }
		public float TempDamage { get; set; } = 0.0f;
		public float TempPeriod { get; set; } = 0.0f;
		public Cost TempCost { get; set; } = new();
	}

	public class Effect
	{
		public IList<Effect> Effects { get; set; } // <- With that we build complex stuff :D
		public string Magic { get; set; } = "None";
		public float Damage { get; set; } = 0.0f;
		public float Area { get; set; } = 0.0f;
		public string Affects { get; set; } = "Hostile";
		public Cost Gain { get; set; } = new();
		public Cost Cost { get; set; } = new();
		public float Period { get; set; } = -1.0f;// negative or zero for a ONCE effect
	}
}