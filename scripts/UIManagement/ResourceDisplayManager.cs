using Godot;
using System;

public partial class ResourceDisplayManager : Control
{
	public static ResourceDisplayManager Instance;
	[Export] private RichTextLabel manaText;
	[Export] private RichTextLabel fireText;
	[Export] private RichTextLabel elecText;
	[Export] private RichTextLabel stoneText;

	public override void _Ready()
	{
		Instance = this;
		Update(new()); // resets everything
	}

	public void Update(Price _playerResources)
	{
		manaText.Text = _playerResources[ResourcesManager.Resource.Mana].ToString();
		fireText.Text = _playerResources[ResourcesManager.Resource.Fire].ToString();
		elecText.Text = _playerResources[ResourcesManager.Resource.Elec].ToString();
		stoneText.Text = _playerResources[ResourcesManager.Resource.Stone].ToString();
	}
}
