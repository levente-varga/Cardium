using System;
using Godot;

public partial class TileMapLayer : Godot.TileMapLayer
{
	AStarGrid2D grid;

	Vector2I start = new Vector2I(2, 2);
	Vector2I end = new Vector2I(10, 10);

	Line2D line;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		line = GetNode<Line2D>("Line2D");

		grid = new AStarGrid2D();
		grid.Region = GetUsedRect();
		grid.CellSize = TileSet.TileSize;
		grid.Offset = grid.CellSize / 2;
		grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		grid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Max;
		grid.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan;
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
		DrawRect(new Rect2(start * grid.CellSize, grid.CellSize), Colors.GreenYellow);
    	DrawRect(new Rect2(end * grid.CellSize, grid.CellSize), Colors.OrangeRed);

		for (int x = 0; x < grid.Region.Size.X; x++) {
			for (int y = 0; y < grid.Region.Size.Y; y++) {
				Vector2I cell = new Vector2I(x, y) + grid.Region.Position;
				if (grid.IsPointSolid(cell)) {
					DrawRect(new Rect2(cell.X * grid.CellSize.X, cell.Y * grid.CellSize.Y, grid.CellSize.X, grid.CellSize.Y), Colors.Aquamarine);
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
			Vector2I cell = (Vector2I)(mouseButton.Position / grid.CellSize);

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
