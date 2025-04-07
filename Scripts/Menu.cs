using System.Drawing;
using Godot;

namespace Cardium.Scripts;

public partial class Menu : Node2D
{
	private Button _newGameButton = null!;
	private Button _exitButton = null!;

	private Dungeon? dungeon;
	
	public override void _Ready()
	{
		_newGameButton = GetNode<Button>("Screen/Menu/NewGameButton");
		_exitButton = GetNode<Button>("Screen/Menu/ExitButton");
		
		_newGameButton.Pressed += OnNewGameButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;

		GenerateDungeon();
	}

	public override void _Process(double delta)
	{
		
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventKey { Pressed: true, KeyLabel: Key.R }:
				GenerateDungeon();
				break;
		}
	}

	private void GenerateDungeon()
	{
		if (dungeon != null) RemoveChild(dungeon.WallLayer);
		dungeon = new DungeonGenerator().Generate(new Size(31, 31));
		AddChild(dungeon.WallLayer);
	}

	private void OnNewGameButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
	}

	private void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
}