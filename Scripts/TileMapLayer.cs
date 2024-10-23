using System;
using Godot;

public partial class TileMapLayer : Godot.TileMapLayer
{
	AStarGrid2D grid;

	Vector2I cellSize = new Vector2I(16, 16);

	Vector2I start = new Vector2I(2, 2);
	Vector2I end = new Vector2I(10, 10);

	Line2D line;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		line = GetNode<Line2D>("Line2D");

		grid = new AStarGrid2D();
		grid.Region = GetUsedRect();
		grid.CellSize = cellSize;
		grid.Offset = cellSize / 2;
		grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		grid.Update();

		GD.Print("TileMap rect: ", GetUsedRect());
		GD.Print("TileMap position: ", Position);
		GD.Print("Top left tile position: ", ToGlobal(MapToLocal(GetUsedRect().Position)));

		UpdatePath();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Draw()
    {
		DrawRect(new Rect2(start * cellSize, cellSize), Colors.GreenYellow);
    	DrawRect(new Rect2(end * cellSize, cellSize), Colors.OrangeRed);

		for (int x = 0; x < grid.Region.Size.X; x++) {
			for (int y = 0; y < grid.Region.Size.Y; y++) {
				Vector2I cell = new Vector2I(x, y) + grid.Region.Position;
				if (grid.IsPointSolid(cell)) {
					DrawRect(new Rect2(cell.X * cellSize.X, cell.Y * cellSize.Y, cellSize.X, cellSize.Y), Colors.Aquamarine);
				}
			}
		}

        base._Draw();
    }

	private void UpdatePath() {
		line.Points = grid.GetPointPath(start, end);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left) {
			Vector2I mousePosition = new Vector2I((int)mouseButton.GlobalPosition.X, (int)mouseButton.GlobalPosition.Y);
			//Vector2I offsetPosition = mousePosition - ;
			Vector2I cell = mousePosition / cellSize;

			GD.Print("Clisked on cell ", cell);

			if (grid.IsInBoundsv(cell)) {
				grid.SetPointSolid(cell, !grid.IsPointSolid(cell));
			}
			else {
				GD.Print("Was no in bound");
			}

			UpdatePath();
			QueueRedraw();
		}
	}
}
