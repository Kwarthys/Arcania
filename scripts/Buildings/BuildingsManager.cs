using Godot;
using System;

public class BuildingsManager
{
	public void LoadData(string _path)
	{
		JSONFormats.BuildingsData data = JSONManager.Read<JSONFormats.BuildingsData>(_path);
		GD.Print(data);

		foreach(JSONFormats.Building building in data.Buildings)
		{
			GD.Print(building.Name + ": " + building.Cost.Mana);
		}
	}
}
