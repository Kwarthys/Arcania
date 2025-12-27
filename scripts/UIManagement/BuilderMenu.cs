using Godot;
using System;
using System.Resources;

public partial class BuilderMenu : Control
{
	[Export] private HBoxContainer buttonContainer;
	[Export] private Texture2D icon;

	private string selectedBuilding = "";

	public void Initialize()
	{
		foreach(Node n in buttonContainer.GetChildren())
			n.QueueFree(); // Remove test buttons

		// TODO Read that from Building Data
		buttonContainer.AddChild(CreateButton("BasicTower"));
		buttonContainer.AddChild(CreateButton("BasicHarvester"));
	}

	private Button CreateButton(string _text)
	{
		Button b = new();
		b.Icon = icon;
		b.IconAlignment = HorizontalAlignment.Center;
		b.VerticalIconAlignment = VerticalAlignment.Top;
		b.ExpandIcon = true;
		b.Text = _text;
		b.FocusMode = FocusModeEnum.None;

		b.Pressed += () => OnButtonClic(_text);

		return b;
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if(@event.IsActionReleased("ui_cancel"))
			OnCancel();
	}

	private void OnCancel()
	{
		if(selectedBuilding == "")
			return;

		selectedBuilding = "";
		GD.Print("Cleared selected building");
	}

	private void OnButtonClic(string _buildingName)
	{
		if(selectedBuilding == _buildingName)
			return;

		selectedBuilding = _buildingName;
		GD.Print("Selected " + selectedBuilding);
	}
}
