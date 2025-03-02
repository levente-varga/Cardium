using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Enemies;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts;

public partial class World : Node2D
{
	[Export] public Camera Camera;
	[Export] public Player Player;
	[Export] public Label DebugLabel1;
	[Export] public Label DebugLabel2;
	[Export] public Label DebugLabel3;
	[Export] public Label DebugLabel4;
	
	[Export] public TileMapLayer DecorLayer;
	[Export] public TileMapLayer WallLayer;
	[Export] public TileMapLayer ObjectLayer;
	[Export] public TileMapLayer EnemyLayer;
	[Export] public TileMapLayer LootLayer;
	[Export] public TileMapLayer FogLayer;
	
	private readonly List<Enemy> _enemies = new();
	private readonly List<CardLoot> _loot = new();
	private readonly List<Interactable> _interactables = new();
	
    private readonly List<TileMapLayer> _layers = new();

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
	    
	    SpawnInteractables();
	    SpawnEnemies();
	    
	    Player.OnMoveEvent += OnPlayerMove;
	    Player.OnNudgeEvent += OnPlayerNudge;
    }
    
    public override void _Process(double delta)
    {
	    DebugLabel1.Text = "Camera view size: " + Camera.ViewRect.Size;
	    DebugLabel2.Text = "Camera top left: " + Camera.ViewRect.Position;
	    DebugLabel3.Text = "Camera zoom: " + Camera.Zoom;
    }
    
    private void SetupLayers()
    {
	    _layers.Clear();
	    _layers.Add(DecorLayer);
	    _layers.Add(WallLayer);
	    _layers.Add(ObjectLayer);
	    _layers.Add(EnemyLayer);
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
		    }
	    }
    }
    
    private void SetupPath()
    {
    	_line = GetNode<Line2D>("Line2D");

    	_grid = new AStarGrid2D();
    	_grid.Region = _region;
    	_grid.CellSize = Global.TileSize;
    	_grid.Offset = _grid.CellSize / 2;
    	_grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
    	_grid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
	    _grid.Update();
	    
	    foreach (var cell in WallLayer.GetUsedCells().Union(ObjectLayer.GetUsedCells()))
	    {
		    _grid.SetPointSolid(cell);
	    }
    }
    
    private Vector2I GetTilePosition(Vector2 position) => new (
		Mathf.FloorToInt(position.X / Global.TileSize.X),
		Mathf.FloorToInt(position.Y / Global.TileSize.Y)
	);
    
    public bool EnemyExistsAt(Vector2I position) => _enemies.Any(enemy => enemy.Position == position);
    public bool InteractableExistsAt(Vector2I position) => _interactables.Any(enemy => enemy.Position == position);
    public bool WallExistsAt(Vector2I position) => WallLayer.GetCellTileData(position) != null;

    public bool ObjectExistsNextTo(Vector2I position) =>
	    ObjectLayer.GetCellTileData(position + Vector2I.Down) != null ||
	    ObjectLayer.GetCellTileData(position + Vector2I.Up) != null ||
	    ObjectLayer.GetCellTileData(position + Vector2I.Left) != null ||
	    ObjectLayer.GetCellTileData(position + Vector2I.Right) != null;
    
    public Enemy GetEnemyAt(Vector2I position) => _enemies.FirstOrDefault(enemy => enemy.Position == position);
    
    public bool IsTileEmpty(Vector2I position) => 
	    !WallExistsAt(position)
	    && !InteractableExistsAt(position)
	    && !EnemyExistsAt(position)
	    && Player.Position != position;

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
    }

    public override void _Input(InputEvent @event)
    {
    	if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseButton) return;
	    
	    var globalMousePosition = mouseButton.Position + Camera.ViewRect.Position;
    	var cell = GetTilePosition(globalMousePosition);
	    
    	if (_grid.IsInBoundsv(cell)) {
		    _end = cell;
    	}

    	UpdatePath();
    	QueueRedraw();
    }

    private void OnPlayerMove(TileAlignedGameObject gameObject, Vector2I position)
    {
	    PickUpLoot();
	    UpdatePath();
	    QueueRedraw();
    }
    
    private void OnPlayerNudge(TileAlignedGameObject gameObject, Vector2I position)
	{
		Interact(position);
		Attack(position, Player);
	}
    
    private void OnEnemyDeath(Entity entity)
	{
		GD.Print(entity.Name + " died!");
	    _enemies.Remove((Enemy)entity);
	    RemoveChild(entity);
	    entity.QueueFree();
	    
	    var cardLoot = new CardLoot();
	    _loot.Add(cardLoot);
	    AddChild(cardLoot);
	    cardLoot.SetPosition(entity.Position);
	}

    public void Attack(Entity target, Entity source)
    {
	    GD.Print(source.Name + " attacked " + target.Name + " for " + source.Damage + " damage!");
	    target.OnDamaged(source, source.Damage);
	    
	    Camera.Shake(6 * source.Damage);
    }
    
    public void Attack(Vector2I position, Entity source)
	{
	    var enemy = GetEnemyAt(position);
	    if (enemy == null) return;
	    
	    Attack(enemy, source);
	}

    private void SpawnEnemy(Enemy enemy, Vector2I position)
    {
	    if (!IsTileEmpty(position)) return;
	    enemy.OnDeathEvent += OnEnemyDeath;
	    AddChild(enemy);
	    enemy.SetPosition(position);
	    _enemies.Add(enemy);
    }
    
    private void SpawnInteractable(Interactable interactable, Vector2I position)
    {
	    if (!IsTileEmpty(position)) return;
	    AddChild(interactable);
	    interactable.SetPosition(position);
	    _interactables.Add(interactable);
    }
    
    private void SpawnLoot(CardLoot loot, Vector2I position)
    {
	    if (!IsTileEmpty(position)) return;
	    AddChild(loot);
	    loot.SetPosition(position);
	    _loot.Add(loot);
    }
    
    private void SpawnEnemies()
    {
	    foreach (var cell in EnemyLayer.GetUsedCells())
	    {
		    SpawnEnemy(new SlimeEnemy(), cell);
		    EnemyLayer.SetCell(cell, -1, new Vector2I(-1, -1));
	    }
    }
    
    private void SpawnInteractables()
    {
	    foreach (var cell in ObjectLayer.GetUsedCells())
	    {
		    Interactable interactable;
		    
		    if (ObjectLayer.GetCellAtlasCoords(cell) == Global.BonfireAtlasCoords) interactable = new Bonfire();
			else if (ObjectLayer.GetCellAtlasCoords(cell) == Global.ChestAtlasCoords) interactable = new Chest();
		    else if (ObjectLayer.GetCellAtlasCoords(cell) == Global.DoorAtlasCoords) interactable = new Door();
		    else return;
		    
		    SpawnInteractable(interactable, cell);
		    ObjectLayer.SetCell(cell, -1, new Vector2I(-1, -1));
	    }
    }
    
    private void SpawnLoot()
    {
	    foreach (var cell in LootLayer.GetUsedCells())
	    {
		    SpawnLoot(new CardLoot(), cell);
		    LootLayer.SetCell(cell, -1, new Vector2I(-1, -1));
	    }
    }

    public List<Vector2I> GetInteractablePositions(Vector2I position)
    {
	    List<Vector2I> adjacentPositions = new()
	    {
		    position + Vector2I.Up,
		    position + Vector2I.Down,
		    position + Vector2I.Left,
		    position + Vector2I.Right,
	    };
	    
	    return _interactables.Where(i => adjacentPositions.Contains(i.Position)).ToList().Select(i => i.Position).ToList();
    }
    
    public void Interact(Vector2I position)
	{
	    var interactable = _interactables.FirstOrDefault(interactable => interactable.Position == position);
	    interactable?.OnInteract(Player, Camera);
	}

	private void PickUpLoot()
	{
		foreach (var loot in _loot.Where(loot => loot.Position == Player.Position).ToList())
		{
			_loot.Remove(loot);
			RemoveChild(loot);
			loot.QueueFree();
			// TODO: Add loot to player
		}
	}
}