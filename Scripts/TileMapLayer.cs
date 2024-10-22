using Godot;
using System;

public partial class TileMapLayer : Godot.TileMapLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("TileMap dimensions: ", GetUsedRect().Size);
		GD.Print("TileMap position: ", Position);
		GD.Print("Top left tile position: ", ToGlobal(MapToLocal(GetUsedRect().Position)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
