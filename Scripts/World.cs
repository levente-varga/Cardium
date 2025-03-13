#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Enemies;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts;

internal enum SelectionMode
{
	None,
	Target,
	Location,
	Interactable,
}

public partial class World : Node2D
{
	[Export] public Camera Camera;
	[Export] public Player Player;
	[Export] public Hand Hand;
	[Export] public Label DebugLabel1;
	[Export] public Label DebugLabel2;
	[Export] public Label DebugLabel3;
	[Export] public Label DebugLabel4;
	
	[Export] public TileMapLayer GroundLayer;
	[Export] public TileMapLayer DecorLayer;
	[Export] public TileMapLayer WallLayer;
	[Export] public TileMapLayer ObjectLayer;
	[Export] public TileMapLayer EnemyLayer;
	[Export] public TileMapLayer EnemyGroupLayer;
	[Export] public TileMapLayer LootLayer;
	[Export] public TileMapLayer FogLayer;
	
	private readonly List<Enemy> _enemies = new();
	private readonly List<CardLoot> _loot = new();
	private readonly List<Interactable> _interactables = new();
    private readonly List<TileMapLayer> _layers = new();
    
    private CombatManager _combatManager;
    
    private SelectionMode _selectionMode = SelectionMode.None;

    private Rect2I _region;
    private AStarGrid2D _grid;
    private Vector2I? _end = null;
    private Line2D _line;
    
    public Vector2I HoveredCell => GetTilePosition(GetGlobalMousePosition());
    
    public override void _Ready()
    {
	    //Input.MouseMode = Input.MouseModeEnum.Hidden;
		    
	    SetupLayers();
	    SetupRegion();
	    SetupFogOfWar();
	    SetupPath();
	    UpdatePath();
	    
	    SpawnInteractables();
	    SpawnEnemies();
	    
	    Player.OnMoveEvent += OnPlayerMove;
	    Player.OnNudgeEvent += OnNudge;
	    Player.OnEnterCombatEvent += entity => { Camera.Focus = true; };
	    Player.OnLeaveCombatEvent += entity => { Camera.Focus = false; };
	    
	    Camera.JumpToTarget();
	    
	    _combatManager = new CombatManager(Player, this, DebugLabel1);
    }
    
    public override void _Process(double delta)
    {
	    DebugLabel3.Text = "Region: " + _region + "\n"
		    + "Hovered cell: " + HoveredCell;
	    
	    QueueRedraw();
    }
    
    private void SetupLayers()
    {
	    _layers.Clear();
	    _layers.Add(GroundLayer);
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
	    _line.DefaultColor = Global.Yellow;

    	_grid = new AStarGrid2D();
    	_grid.Region = _region;
    	_grid.CellSize = Global.TileSize;
    	_grid.Offset = Vector2.Zero;
    	_grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
    	_grid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
	    _grid.Update();
	    
	    foreach (var cell in WallLayer.GetUsedCells().Union(ObjectLayer.GetUsedCells()))
	    {
		    _grid.SetPointSolid(cell);
	    }
    }
    
    private static Vector2I GetTilePosition(Vector2 position) => new (
		Mathf.FloorToInt(position.X / Global.TileSize.X),
		Mathf.FloorToInt(position.Y / Global.TileSize.Y)
	);
    
	private static void EraseCell(TileMapLayer map, Vector2I position) => map.SetCell(position, -1, new Vector2I(-1, -1));

    public bool IsTileEnemy(Vector2I position) => _enemies.Any(enemy => enemy.Position == position);
    public bool IsTileInteractable(Vector2I position) => _interactables.Any(interactable => interactable.Position == position && interactable.Solid);
    public bool IsTileWall(Vector2I position) => WallLayer.GetCellTileData(position) != null;

    public bool IsAnyTileInteractableNextTo(Vector2I position) =>
	    ObjectLayer.GetCellTileData(position + Vector2I.Down) != null ||
	    ObjectLayer.GetCellTileData(position + Vector2I.Up) != null ||
	    ObjectLayer.GetCellTileData(position + Vector2I.Left) != null ||
	    ObjectLayer.GetCellTileData(position + Vector2I.Right) != null;
    
    public Enemy? GetEnemyAt(Vector2I position) => _enemies.FirstOrDefault(enemy => enemy.Position == position);
    
    public bool IsTileEmpty(Vector2I position) => 
	    !IsTileWall(position)
	    && !IsTileInteractable(position)
	    && !IsTileEnemy(position)
	    && Player.Position != position;

    /// <summary>
    /// Draws the path from the start to the end
    /// </summary>
    public override void _Draw()
    {
    	if (_end != null) DrawRect(new Rect2(_end.Value * _grid.CellSize, _grid.CellSize), Global.Yellow, false, 4);

	    if (Global.Debug)
	    {
		    DrawRect(new Rect2(HoveredCell * _grid.CellSize, _grid.CellSize),
			    Player.InRange(HoveredCell) ? Colors.Green : Colors.Orange);
	    }


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
	    var extendedVision = Player.Vision + 1;
	    
	    for (var x = -extendedVision; x <= extendedVision; x++)
	    {
		    for (var y = -(extendedVision - Math.Abs(x)); y <= extendedVision - Math.Abs(x); y++)
		    {
			    var coords = Player.Position + new Vector2I(x, y);
			    if (Math.Abs(x) + Math.Abs(y) == extendedVision)
			    {
				    if (FogLayer.GetCellAtlasCoords(coords).X == 2)
				    {
					    FogLayer.SetCell(coords, 2, new Vector2I(1, 0));
				    }
			    }
			    else
			    {
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
	    if (InputMap.EventIsAction(@event, "Debug") && @event.IsPressed())
	    {
		    GD.Print("Debug " + (Global.Debug ? "on" : "off"));
		    Global.Debug = !Global.Debug;
		    _combatManager.UpdateDebugLabel();
	    }
	    
    	if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseButton) return;
	    
	    var globalMousePosition = mouseButton.Position + Camera.ViewRect.Position;
    	var cell = GetTilePosition(globalMousePosition);
	    
    	if (_grid.IsInBoundsv(cell)) {
		    _end = cell;
    	}

    	UpdatePath();
    	QueueRedraw();
    }

    private void OnPlayerMove(TileAlignedGameObject source, Vector2I oldPosition, Vector2I newPosition)
    {
	    PickUpLoot();
	    UpdatePath();
	    QueueRedraw();
    }
    
    private void OnNudge(TileAlignedGameObject source, Vector2I position)
	{
		Interact(position);
		Attack(position, Player);
	}
    
    private void OnEnemyDeath(Entity entity)
	{
		GD.Print(entity.Name + " died!");
	    _enemies.Remove((Enemy)entity);
	    Player.OnMoveEvent -= ((Enemy)entity).OnPlayerMove;
	    entity.OnNudgeEvent -= OnNudge;
	    entity.OnDeathEvent -= OnEnemyDeath;
	    entity.OnEnterCombatEvent -= AddEnemyToCombat;
	    RemoveChild(entity);
	    entity.QueueFree();
	    _grid.SetPointSolid(entity.Position, false);
	    
	    var cardLoot = new CardLoot();
	    _loot.Add(cardLoot);
	    AddChild(cardLoot);
	    cardLoot.SetPosition(entity.Position);
	}

    public void Attack(Entity target, Entity source)
    {
	    GD.Print(source.Name + " attacked " + target.Name + " for " + source.Damage + " damage!");
	    target.ReceiveDamage(source, source.Damage);
	    
	    Camera.Shake(6 * source.Damage);
    }
    
    public void Attack(Vector2I position, Entity source)
	{
	    var enemy = GetEnemyAt(position);
	    if (enemy == null) return;
	    
	    Attack(enemy, source);
	}
    
    private void AddEnemyToCombat(Entity entity)
	{
		var enemy = (Enemy)entity;
		
	    _combatManager.EnterCombat(enemy);
	    if (enemy.GroupId is not null)
	    {
		    foreach (var e in _enemies)
		    {
			    if (!e.InCombat && e.GroupId == enemy.GroupId)
			    {
				    e.OnCombatStart();
			    }
		    }
	    }
	    
	    GD.Print(entity.Name + " entered combat.");
	}

	private void OnEntityMove(TileAlignedGameObject source, Vector2I oldPosition, Vector2I newPosition)
	{
		_grid.SetPointSolid(oldPosition, false);
		_grid.SetPointSolid(newPosition, true);
	}

	private void OnInteractableSolidityChange(Interactable interactable, bool solid)
	{
		_grid.SetPointSolid(interactable.Position, solid);
	}
    
	private void SpawnEnemy(Enemy enemy, Vector2I position)
	{
		if (!IsTileEmpty(position)) return;
		enemy.OnDeathEvent += OnEnemyDeath;
		enemy.OnEnterCombatEvent += AddEnemyToCombat;
		enemy.OnNudgeEvent += OnNudge;
		enemy.OnMoveEvent += OnEntityMove;
		Player.OnMoveEvent += enemy.OnPlayerMove;
		AddChild(enemy);
		enemy.SetPosition(position);
		_grid.SetPointSolid(position);
		_enemies.Add(enemy);
	}
	
    private void SpawnInteractable(Interactable interactable, Vector2I position)
    {
	    if (!IsTileEmpty(position)) return;
	    AddChild(interactable);
	    interactable.SetPosition(position);
	    interactable.OnSolidityChangeEvent += OnInteractableSolidityChange;
	    _grid.SetPointSolid(interactable.Position, interactable.Solid);
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
		    Enemy enemy;
		    
		    var atlasCoords = EnemyLayer.GetCellAtlasCoords(cell);
		    EraseCell(EnemyLayer, cell);

		    if (atlasCoords == Global.SlimeAtlasCoords) enemy = new Slime();
		    else if (atlasCoords == Global.SpiderAtlasCoords) enemy = new Spider();
		    else if (atlasCoords == Global.RangerAtlasCoords) enemy = new Ranger();
		    else continue;
		    
		    var groupIdAtlasCoords = EnemyGroupLayer.GetCellAtlasCoords(cell);
		    var groupId = groupIdAtlasCoords.X - Global.ZeroAtlasCoords.X;
		    if (groupIdAtlasCoords.Y == Global.ZeroAtlasCoords.Y && groupId is >= 0 and <= 9)
		    {
			    enemy.GroupId = groupId;
		    }
		    EraseCell(EnemyGroupLayer, cell);
		    
		    SpawnEnemy(enemy, cell);
	    }
    }
    
    private void SpawnInteractables()
    {
	    foreach (var cell in ObjectLayer.GetUsedCells())
	    {
		    Interactable interactable;
		    
		    var atlasCoords = ObjectLayer.GetCellAtlasCoords(cell);
		    EraseCell(ObjectLayer, cell);
		    
		    if (atlasCoords == Global.BonfireAtlasCoords) interactable = new Bonfire();
			else if (atlasCoords == Global.ChestAtlasCoords) interactable = new Chest();
		    else if (atlasCoords == Global.DoorAtlasCoords) interactable = new Door();
		    else continue;
		    
		    SpawnInteractable(interactable, cell);
	    }
    }
    
    private void SpawnLoot()
    {
	    foreach (var cell in LootLayer.GetUsedCells())
	    {
		    SpawnLoot(new CardLoot(), cell);
		    EraseCell(LootLayer, cell);
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
			Hand.AddCard();
		}
	}

	public int GetDistanceBetween(Vector2I from, Vector2I to)
	{
		if (!_grid.IsInBoundsv(from) || !_grid.IsInBoundsv(to)) return -1;
		return _grid.GetPointPath(from, to).Length - 1;
	}
	
	public List<Vector2I>? GetPathBetween(Vector2I from, Vector2I to)
	{
		if (!_grid.IsInBoundsv(from) || !_grid.IsInBoundsv(to)) return null;
		if (from == to) return new List<Vector2I>();
		var path = _grid.GetPointPath(from, to).ToList();
		if (path.Count == 0) return null;
		path = path.GetRange(1, path.Count - 1);
		return path.Select(p => new Vector2I((int)p.X / Global.TileSize.X, (int)p.Y / Global.TileSize.Y)).ToList();
	}

	public Vector2[] GetPointPathBetween(Vector2I from, Vector2I to)
	{
		return _grid.GetPointPath(from, to).ToList().Select(p => new Vector2(p.X, p.Y) + Global.TileSize / 2).ToArray();
	}

	private static List<Vector2I> GetTilesExactlyInRange(Vector2I position, int range)
	{
		var r = Mathf.CeilToInt(range);
        
		var tiles = new List<Vector2I>();
        
		for (var i = -r; i <= r; i++)
		{
			var x = position.X + i;
			var dy = r - Math.Abs(i);
			var y1 = position.Y + (dy);
			var y2 = position.Y - (dy);
			tiles.Add(new Vector2I(x, y1));
			if (y1 != y2) tiles.Add(new Vector2I(x, y2));
		}
        
		return tiles;
	}
    
	public List<Vector2I> GetEmptyTilesExactlyInRange(Vector2I position, int range, Vector2I? exclude = null)
	{
		var tiles = GetTilesExactlyInRange(position, range);

		for (var i = 0; i < tiles.Count; i++)
		{
			var tile = tiles[i];
			if (tile == exclude) continue;
			if (!IsTileEmpty(tile)) tiles.Remove(tile);
		}
        
		return tiles;
	}

	private async Task<Enemy> SelectEnemyTarget(int range, Vector2I from)
	{
		Enemy target = new Enemy();
		_selectionMode = SelectionMode.Target;
		
		// TODO: Implement entity targeting
		
		_selectionMode = SelectionMode.None;
		return target;
	}
	
	private async Task<Interactable> SelectInteractableTarget(int range, Vector2I from)
	{
		Interactable target = new Interactable();
		_selectionMode = SelectionMode.Interactable;
		
		// TODO: Implement entity targeting
		
		_selectionMode = SelectionMode.None;
		return target;
	}
	
	private async Task<Vector2I> SelectLocationTarget(int range, Vector2I from)
	{
		Vector2I target = Vector2I.Zero;
		_selectionMode = SelectionMode.Location;
		
		// TODO: Implement entity targeting
		
		_selectionMode = SelectionMode.None;
		return target;
	}
	
	public async void PlayCard(Card card)
	{
		switch (card)
		{
			case PlayerTargetingCard playerTargetingCard:
				playerTargetingCard.OnPlay(Player);
				break;
			case EnemyTargetingCard enemyTargetingCard:
				var enemy = await SelectEnemyTarget(enemyTargetingCard.Range, Player.Position);
				enemyTargetingCard.OnPlay(Player, enemy);
				break;
			case InteractableTargetingCard interactableTargetingCard:
				var interactable = await SelectInteractableTarget(interactableTargetingCard.Range, Player.Position);
				interactableTargetingCard.OnPlay(Player, interactable);
				break;
			case LocationTargetingCard locationTargetingCard:
				var position = await SelectLocationTarget(locationTargetingCard.Range, Player.Position);
				locationTargetingCard.OnPlay(Player, position, this);
				break;
		}
	}
}