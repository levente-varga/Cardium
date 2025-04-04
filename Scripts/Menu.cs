using Godot;

namespace Cardium.Scripts;

public partial class Menu : Node2D
{
	private Button _newGameButton = null!;
	private Button _exitButton = null!;
	
	public override void _Ready()
	{
		_newGameButton = GetNode<Button>("Screen/Menu/NewGameButton");
		_exitButton = GetNode<Button>("Screen/Menu/ExitButton");
		
		_newGameButton.Pressed += OnNewGameButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
	}

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