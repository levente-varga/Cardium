using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Cardium.Scripts;

public partial class World : Node2D
{
	[Export] public Camera Camera;
	[Export] public Player Player;
	[Export] public Label DebugLabel1;
	[Export] public Label DebugLabel2;
	[Export] public Label DebugLabel3;
	
	[Export] public TileMapLayer DecorLayer;
	[Export] public TileMapLayer WallLayer;
	[Export] public TileMapLayer ObjectLayer;
	[Export] public TileMapLayer LootLayer;
	[Export] public TileMapLayer FogLayer;
	
    private readonly List<TileMapLayer> _layers = new();
    private static readonly Vector2I TileSize = new(64, 64);
    private static readonly float SpriteScale = 4f;

    private Rect2I _region;
    private AStarGrid2D _grid;
    private Vector2I? _end = null;
    private Line2D _line;
    
    public override void _Ready()
    {
	    SetupLayers();
	    SetupRegion();
	    SetupFogOfWar();
	    SetupPath();
	    UpdatePath();

	    Player.OnMoveEvent += OnPlayerMove;
    }
    
    public override void _Process(double delta)
    {
		
    }
    
    private void SetupLayers()
    {
	    _layers.Clear();
	    _layers.Add(DecorLayer);
	    _layers.Add(WallLayer);
	    _layers.Add(ObjectLayer);
	    _layers.Add(LootLayer);
    }

    private void SetupRegion()
    {
	    var topLeft = new Vector2I(int.MaxValue, int.MaxValue);
	    var bottomRight = new Vector2I(int.MinValue, int.MinValue);

	    foreach (var layer in _layers)
	    {
		    topLeft = new Vector2I(
			    Math.Min(topLeft.X, layer.GetUsedRect().Position.X),
			    Math.Min(topLeft.Y, layer.GetUsedRect().Position.Y)
		    );
		    bottomRight = new Vector2I(
			    Math.Max(bottomRight.X, layer.GetUsedRect().End.X),
			    Math.Max(bottomRight.Y, layer.GetUsedRect().End.Y)
		    );
	    }

	    var region = new Rect2I { Position = topLeft, Size = bottomRight - topLeft };
	    GD.Print("World region: ", region);

	    _region = region;
    }

    private void SetupFogOfWar()
    {
	    for (var x = _region.Position.X; x < _region.End.X; x++)
	    {
		    for (var y = _region.Position.Y; y < _region.End.Y; y++)
		    {
			    FogLayer.SetCell(new Vector2I(x, y), 2, new Vector2I(0, 0));
			    GD.Print("Set fog of war cell: ", new Vector2I(x, y));
		    }
	    }
    }
    
    private void SetupPath()
    {
    	_line = GetNode<Line2D>("Line2D");

    	_grid = new AStarGrid2D();
    	_grid.Region = _region;
    	_grid.CellSize = TileSize;
    	_grid.Offset = _grid.CellSize / 2;
    	_grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
    	_grid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
	    _grid.Update();
	    
	    foreach (var cell in WallLayer.GetUsedCells().Union(ObjectLayer.GetUsedCells()))
	    {
		    _grid.SetPointSolid(cell);
	    }
	    
	    GD.Print(-1 % 64);
	    
	    GD.Print("Used cells: ", WallLayer.GetUsedCells());

    	GD.Print("TileMap rect: ", _layers[0].GetUsedRect());
    	GD.Print("TileMap position: ", Position);
    	GD.Print("Top left tile position: ", ToGlobal(_layers[0].MapToLocal(_layers[0].GetUsedRect().Position)));
    }
    
    private Vector2I GetTilePosition(Vector2 position) => new (
		Mathf.FloorToInt(position.X / TileSize.X),
		Mathf.FloorToInt(position.Y / TileSize.Y)
	);
    
    public bool IsTileEmpty(Vector2I position) => 
	    WallLayer.GetCellTileData(position) == null
	    && ObjectLayer.GetCellTileData(position) == null;

    /// <summary>
    /// Draws the path from the start to the end
    /// </summary>
    public override void _Draw()
    {
    	if (_end != null) DrawRect(new Rect2(_end.Value * _grid.CellSize, _grid.CellSize), new Color("F4B41B"), false, 4);
    	//DrawRect(new Rect2(_end * _grid.CellSize, _grid.CellSize), Colors.OrangeRed);

    	for (var x = 0; x < _grid.Region.Size.X; x++) {
    		for (var y = 0; y < _grid.Region.Size.Y; y++) {
    			var cell = new Vector2I(x, y) + _grid.Region.Position;
    			if (_grid.IsPointSolid(cell)) {
    				//DrawRect(new Rect2(cell.X * _grid.CellSize.X, cell.Y * _grid.CellSize.Y, _grid.CellSize.X, _grid.CellSize.Y), Colors.Aquamarine);
    			}
    		}
    	}

	    UpdateFogOfWar();
	    
    	base._Draw();
    }

    private void UpdateFogOfWar()
    {
	    var roundedVision = (int)Math.Ceiling(Player.Vision + 1);
	    var topLeftTile = Player.Position - new Vector2I(roundedVision, roundedVision);
	    for (var x = topLeftTile.X; x < topLeftTile.X + roundedVision * 2; x++)
	    {
		    for (var y = topLeftTile.Y; y < topLeftTile.Y + roundedVision * 2; y++)
		    {
			    var coords = new Vector2I(x, y);
			    if (((Vector2)Player.Position).DistanceTo(coords) >= Player.Vision)
			    {
				    if (FogLayer.GetCellAtlasCoords(coords).X == 2)
				    {
					    FogLayer.SetCell(coords, 2, new Vector2I(1, 0));
				    }
			    }
			    else
			    {
				    //DrawRect(new Rect2(x * TileSize.X, y * TileSize.Y, TileSize.X, TileSize.Y), new Color("F4B41B"));
				    FogLayer.SetCell(coords, 2, new Vector2I(2, 0));
			    }
		    }
	    }
    }

    /// <summary>
    ///  Recalculates the path
    /// </summary>
    private void UpdatePath() {
	    if (_end is null) return;
	    
    	_line.Points = _grid.GetPointPath(Player.Position, _end.Value);
    	GD.Print("Path: ", _grid.GetIdPath(Player.Position, _end.Value));
    }

    public override void _Input(InputEvent @event)
    {
    	if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseButton) return;
	    
    	var cell = GetTilePosition(mouseButton.Position + Camera.ViewRect.Position);

	    DebugLabel1.Text = "Camera view size: " + Camera.ViewRect.Size;
	    DebugLabel2.Text = "Camera top left: " + Camera.ViewRect.Position;
	    DebugLabel3.Text = "Mouse position: " + mouseButton.Position;
	    
	    
    	if (_grid.IsInBoundsv(cell)) {
		    _end = cell;
    	}

    	UpdatePath();
    	QueueRedraw();
    }

    private void OnPlayerMove()
    {
	    UpdatePath();
	    QueueRedraw();
    }
}