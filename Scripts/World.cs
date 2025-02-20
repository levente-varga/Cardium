using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class World : Node2D
{
    private readonly List<TileMapLayer> _layers = new();
    private static readonly Vector2I TileSize = new(64, 64);

    private AStarGrid2D _grid;
    private Vector2I _start = new(2, 2);
    private Vector2I _end = new(14, 10);
    private Line2D _line;

    public override void _Ready()
    {
	    SetupLayers();
	    SetupPath();
	    UpdatePath();
    }
    
    private void SetupLayers()
    {
        foreach (var child in GetChildren())
        {
            if (child is not TileMapLayer layer) continue;
            _layers.Add(layer);
            layer.TileSet.TileSize = TileSize;
        }
    }

    public override void _Process(double delta)
    {
		
    }
    
    private void SetupPath()
    {
    	_line = GetNode<Line2D>("Line2D");

    	_grid = new AStarGrid2D();
    	_grid.Region = _layers[0].GetUsedRect();
    	_grid.CellSize = TileSize;
    	_grid.Offset = _grid.CellSize / 2;
    	_grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
    	_grid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
    	_grid.Update();

    	GD.Print("TileMap rect: ", _layers[0].GetUsedRect());
    	GD.Print("TileMap position: ", Position);
    	GD.Print("Top left tile position: ", ToGlobal(_layers[0].MapToLocal(_layers[0].GetUsedRect().Position)));
    }

    /// <summary>
    /// Draws the path from the start to the end
    /// </summary>
    public override void _Draw()
    {
    	DrawRect(new Rect2(_start * _grid.CellSize, _grid.CellSize), Colors.GreenYellow);
    	DrawRect(new Rect2(_end * _grid.CellSize, _grid.CellSize), Colors.OrangeRed);

    	for (var x = 0; x < _grid.Region.Size.X; x++) {
    		for (var y = 0; y < _grid.Region.Size.Y; y++) {
    			var cell = new Vector2I(x, y) + _grid.Region.Position;
    			if (_grid.IsPointSolid(cell)) {
    				DrawRect(new Rect2(cell.X * _grid.CellSize.X, cell.Y * _grid.CellSize.Y, _grid.CellSize.X, _grid.CellSize.Y), Colors.Aquamarine);
    			}
    		}
    	}

    	base._Draw();
    }

    /// <summary>
    ///  Recalculates the path
    /// </summary>
    private void UpdatePath() {
    	_line.Points = _grid.GetPointPath(_start, _end);
    	GD.Print("Path: ", _grid.GetIdPath(_start, _end));
    }

    public override void _Input(InputEvent @event)
    {
    	if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseButton) return;
    	var camera = GetViewport().GetCamera2D();

    	var cell = (Vector2I)(mouseButton.Position / camera.Zoom / _grid.CellSize);

    	if (_grid.IsInBoundsv(cell)) {
    		_grid.SetPointSolid(cell, !_grid.IsPointSolid(cell));
    	}

    	UpdatePath();
    	QueueRedraw();
    }
}