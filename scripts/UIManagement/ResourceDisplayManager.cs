using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class ResourceDisplayManager : Control
{
	public static ResourceDisplayManager Instance;
	[Export] private Color manaColor;
	[Export] private Color fireColor;
	[Export] private Color elecColor;
	[Export] private Color stoneColor;

	private Dictionary<ResourcesManager.Resource, DisplayItem> resourceToItem;

	public override void _Ready()
	{
		Instance = this;

		GridContainer container = new();

		resourceToItem = new()
		{
			{ResourcesManager.Resource.Mana, new()},
			{ResourcesManager.Resource.Fire, new()},
			{ResourcesManager.Resource.Elec, new()},
			{ResourcesManager.Resource.Stone, new()}
		};

		// Add the bars
		foreach(KeyValuePair<ResourcesManager.Resource, DisplayItem> pair in resourceToItem)
		{
			container.AddChild(pair.Value.bar);

			Color c = new(1, 1, 1, 1);
			switch(pair.Key)
			{
				case ResourcesManager.Resource.Mana: c = manaColor; break;
				case ResourcesManager.Resource.Fire: c = fireColor; break;
				case ResourcesManager.Resource.Elec: c = elecColor; break;
				case ResourcesManager.Resource.Stone: c = stoneColor; break;
			}

			pair.Value.bar.ShowPercentage = false;
			pair.Value.bar.MinValue = 0.0;
			pair.Value.bar.MaxValue = 1.0;
			StyleBoxFlat style = new();
			style.BgColor = c;
			pair.Value.bar.AddThemeStyleboxOverride("fill", style);
		}

		// Add the labels
		foreach(KeyValuePair<ResourcesManager.Resource, DisplayItem> pair in resourceToItem)
		{
			container.AddChild(pair.Value.label);

			Color c = new(1, 1, 1, 1);
			switch(pair.Key)
			{
				case ResourcesManager.Resource.Mana: c = manaColor; break;
				case ResourcesManager.Resource.Fire: c = fireColor; break;
				case ResourcesManager.Resource.Elec: c = elecColor; break;
				case ResourcesManager.Resource.Stone: c = stoneColor; break;
			}

			pair.Value.label.AddThemeColorOverride("default_color", c);
			pair.Value.label.FitContent = true;
			pair.Value.label.ScrollActive = false;
			pair.Value.label.AutowrapMode = TextServer.AutowrapMode.Off;
		}

		AddChild(container);
		container.Columns = 4;
		container.AnchorLeft = 0.0f;
		container.AnchorRight = 0.0f;
		container.AnchorTop = 0.0f;
		container.AnchorBottom = 0.0f;

		container.OffsetLeft = 0.0f;
		container.OffsetRight = 0.0f;
		container.OffsetTop = 0.0f;
		container.OffsetBottom = 0.0f;

		container.GrowHorizontal = GrowDirection.Both;
		container.GrowVertical = GrowDirection.End;

		Update(null); // resets everything
	}

	public void Update(ResourcesManager _manager)
	{
		const string format = "0.0";

		foreach(KeyValuePair<ResourcesManager.Resource, DisplayItem> pair in resourceToItem)
		{
			pair.Value.bar.Value = 0;
			if(_manager != null)
			{
				pair.Value.label.Text = _manager.playerResources[pair.Key].ToString(format) + " / " + _manager.storage[pair.Key].ToString(format);
				if(_manager.storage[pair.Key] > 0.0f)
					pair.Value.bar.Value = _manager.playerResources[pair.Key] / _manager.storage[pair.Key];
			}
			else
			{
				pair.Value.label.Text = "0.0 / 0.0";
			}
		}
	}
}

public class DisplayItem
{
	public RichTextLabel label = new();
	public ProgressBar bar = new();
}
