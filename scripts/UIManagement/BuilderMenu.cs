using Godot;
using System;
using System.Collections.Generic;
using System.Resources;

public partial class BuilderMenu : Control
{
	[Export] private HBoxContainer buttonContainer;
	[Export] private Texture2D icon;
	private GameManager gameManager = null;

	public string selectedBuilding { get; private set; } = "";
	public JSONFormats.Building selectedBuildingStaticData = null;

	public void Initialize(GameManager _gameManager, List<string> _buildingNames)
	{
		gameManager = _gameManager;

		foreach(Node n in buttonContainer.GetChildren())
			n.QueueFree(); // Remove test buttons

		foreach(string name in _buildingNames)
			buttonContainer.AddChild(CreateButton(name));
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

	public void Cancel()
	{
		if(selectedBuilding == "")
			return;

		selectedBuilding = "";
		gameManager.OnBuildingGhostChange(selectedBuilding);
	}

	private void OnButtonClic(string _buildingName)
	{
		if(selectedBuilding == _buildingName)
			return;

		selectedBuilding = _buildingName;
		gameManager.OnBuildingGhostChange(selectedBuilding);
	}
}
