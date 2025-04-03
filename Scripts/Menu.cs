using Godot;

namespace Cardium.Scripts;

public partial class Menu : Node2D
{
	private Button NewGameButton = null!;
	private Button ExitButton = null!;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		NewGameButton = GetNode<Button>("Screen/Menu/NewGameButton");
		ExitButton = GetNode<Button>("Screen/Menu/ExitButton");
		
		NewGameButton.Pressed += OnNewGameButtonPressed;
		ExitButton.Pressed += OnExitButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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