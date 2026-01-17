using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class ResourceDisplayManager : Control
{
	public static ResourceDisplayManager Instance;
	[Export] private RichTextLabel manaText;
	[Export] private RichTextLabel fireText;
	[Export] private RichTextLabel elecText;
	[Export] private RichTextLabel stoneText;

	private Dictionary<RichTextLabel, ResourcesManager.Resource> labelToResource;

	public override void _Ready()
	{
		Instance = this;

		labelToResource = new()
		{
			{manaText, ResourcesManager.Resource.Mana},
			{fireText, ResourcesManager.Resource.Fire},
			{elecText, ResourcesManager.Resource.Elec},
			{stoneText, ResourcesManager.Resource.Stone}
		};

		Update(new()); // resets everything
	}

	public void Update(ResourcesManager _manager)
	{
		const string format = "0.0";

		foreach(KeyValuePair<RichTextLabel, ResourcesManager.Resource> pair in labelToResource)
		{
			pair.Key.Text = _manager.playerResources[pair.Value].ToString(format) + " / " + _manager.storage[pair.Value].ToString(format);
		}
	}
}
