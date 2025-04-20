using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts;

internal enum SelectionMode {
	None,
	Enemy,
	Location,
	Interactable,
}

public partial class World : Node2D {
	[Export] public Camera Camera = null!;
	[Export] public Player Player = null!;
	[Export] public Hand Hand = null!;
	[Export] public Label DebugLabel1 = null!;
	[Export] public Label DebugLabel2 = null!;
	[Export] public Label DebugLabel3 = null!;
	[Export] public Label DebugLabel4 = null!;
	
	[Export] public Overlay Overlay = null!;
	
	private readonly List<CardLoot> _loot = new ();
  private readonly List<TileMapLayer> _layers = new ();
  
  private SelectionMode _selectionMode = SelectionMode.None;
  private TargetingCard? _selectedTargetingCard;
  private int _selectionRange = 0;
  private Vector2I _selectionOrigin;
  private Vector2I _selectedCell;
  private bool _selectionCancelled = false;
  private bool _selectionConfirmed = false;

  private AStarGrid2D _grid = new ();
  private Vector2I? _end = null;
  private Line2D _line = new ();

  private readonly Dungeon _dungeon;
  private CombatManager _combatManager = null!;

  public World() {
    //_dungeon = Dungeon.Generate(99, 99, 299);
    _dungeon = Dungeon.GenerateLobby();
  }

  public Vector2I HoveredCell => GetTilePosition(GetGlobalMousePosition());
  
  public override void _Ready() {
    _combatManager = new CombatManager(Player, this, DebugLabel1);
    
    //Input.MouseMode = Input.MouseModeEnum.Hidden;
    //SetupFogOfWar();
    SetupPath();
    UpdatePath();

    AddChild(_dungeon.WallLayer);
    AddChild(_dungeon.DecorLayer);
    AddChild(_dungeon.GroundLayer);
    AddChild(_dungeon.FogLayer);

    _dungeon.Interactables.FirstOrDefault(i => i is Door)!.OnNudgedEvent += () => {
			Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Exited");
    };
    
    _dungeon.FogLayer.ZIndex = 10;

    SpawnPlayer(Player, _dungeon.Player.Position);
    
    foreach (var enemy in _dungeon.Enemies) {
	    SpawnEnemy(enemy);
    }

    foreach (var interactable in _dungeon.Interactables) {
	    SpawnInteractable(interactable);
    }
    
    Camera.JumpToTarget();
  }
	
  public override void _Process(double delta)
  {
    DebugLabel1.Visible = Global.Debug;
    DebugLabel2.Visible = Global.Debug;
    DebugLabel3.Visible = Global.Debug;
    DebugLabel4.Visible = Global.Debug;
    DebugLabel3.Text = "Region: " + _dungeon.Rect + "\n"
                       + "Hovered cell: " + HoveredCell + "\n"
                       + "Selection mode: " + _selectionMode + "\n"
                       + "Selection range: " + _selectionRange + "\n"
                       + "Selection origin: " + _selectionOrigin + "\n"
                       + "Selection confirmed: " + _selectionConfirmed + "\n"
                       + "Selection cancelled: " + _selectionCancelled + "\n";

    if (_selectionMode == SelectionMode.None) {
	    Overlay.Tiles = new List<Overlay.OverlayTile>();
    }
    else {
	    if (_selectedTargetingCard == null) {
		    Overlay.Tiles = new List<Overlay.OverlayTile> { new () {Position = HoveredCell, Color = Colors.Red} };
	    }
	    else {
		    Overlay.Tiles = _selectedTargetingCard.GetHighlightedTiles(Player, HoveredCell, this)
			    .Select(tile => new Overlay.OverlayTile { Position = tile, Color = Colors.Red })
			    .ToList();
	    }
    }
    
    QueueRedraw();
  }

  public override void _Input(InputEvent @event) {
    if (InputMap.EventIsAction(@event, "Debug") && @event.IsPressed()) {
	    GD.Print("Debug " + (Global.Debug ? "on" : "off"));
	    Global.Debug = !Global.Debug;
	    _combatManager.UpdateDebugLabel();
    }
    else if (InputMap.EventIsAction(@event, "Select") && @event.IsPressed()) {
	    
    }
    else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
	    if (_selectionMode == SelectionMode.None) return;

	    if (_grid.IsInBoundsv(HoveredCell)) {
		    _selectedCell = HoveredCell;
	    }

	    if (Utils.ManhattanDistanceBetween(_selectionOrigin, _selectedCell) > _selectionRange) {
		    Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Out of range!", color: Global.Red);
		    return;
	    }
	    
	    switch (_selectionMode) {
		    case SelectionMode.Enemy:
			    if (IsEnemy(_selectedCell)) _selectionConfirmed = true;
			    else Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Select an enemy!", color: Global.Red);
			    break;
		    case SelectionMode.Interactable:
			    if (IsInteractable(_selectedCell)) _selectionConfirmed = true;
			    else Utils.SpawnFloatingLabel(GetTree(), Player.GlobalPosition, "Select an object!", color: Global.Red);
			    break;
		    case SelectionMode.Location:
			    _selectionConfirmed = true;
			    break;
		    case SelectionMode.None:
		    default:
			    break;
	    }

	    UpdatePath();
	    QueueRedraw();
    }
    else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right }) {
	    _selectionMode = SelectionMode.None;
	    _selectionCancelled = true;
	    QueueRedraw();
    }
  }
  
  public override void _Draw() {
    if (_end != null) DrawRect(new Rect2(Global.TileToWorld(_end.Value), Global.GlobalTileSize), Global.Yellow, false, 4);
    
    if (Global.Debug) {
	    DrawRect(
		    new Rect2(Global.TileToWorld(HoveredCell), 
			  Global.GlobalTileSize),
				Player.InRange(HoveredCell) ? Colors.Green : Colors.Orange
				);
    }
    
    for (var x = 0; x < _grid.Region.Size.X; x++) {
    	for (var y = 0; y < _grid.Region.Size.Y; y++) {
    		var cell = new Vector2I(x, y) + _grid.Region.Position;
    		if (_grid.IsPointSolid(cell)) {
    			//DrawRect(new Rect2(Global.TileToWorld(cell), Global.TileSize), Colors.Aquamarine);
    		}
    	}
    }

    UpdateFogOfWar();
    
    base._Draw();
  }
  
  private void SetupFogOfWar() {
    for (var x = _dungeon.Rect.Position.X; x < _dungeon.Rect.End.X; x++) {
	    for (var y = _dungeon.Rect.Position.Y; y < _dungeon.Rect.End.Y; y++) {
		    _dungeon.FogLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(0, 0));
	    }
    }
  }
  
  private void SetupPath() {
    _line = GetNode<Line2D>("Line2D");
    _line.DefaultColor = Global.Yellow;

    _grid.Region = _dungeon.Rect;
    _grid.CellSize = Global.GlobalTileSize;
    _grid.Offset = Vector2.Zero;
    _grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
    _grid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Euclidean;
    _grid.Update();
    
    foreach (var cell in _dungeon.WallLayer.GetUsedCells()) {
	    _grid.SetPointSolid(cell);
    }
  }
    
  private static Vector2I GetTilePosition(Vector2 position) => new (
		Mathf.FloorToInt(position.X / Global.GlobalTileSize.X),
		Mathf.FloorToInt(position.Y / Global.GlobalTileSize.Y)
	);
  
	private static void EraseCell(TileMapLayer map, Vector2I position) => map.SetCell(position, -1, new Vector2I(-1, -1));
	
  public bool IsEnemy(Vector2I position) => GetEnemyAt(position) != null;
  public bool IsInteractable(Vector2I position) => GetInteractableAt(position) != null && GetInteractableAt(position) is { Solid: true };
  public bool IsWall(Vector2I position) => _dungeon.WallLayer.GetCellTileData(position) != null;
  
  public bool IsAnyInteractableNextTo(Vector2I position) => _dungeon.Interactables.Any(i => 
    i.Position == position + Vector2I.Right || 
    i.Position == position + Vector2I.Left || 
    i.Position == position + Vector2I.Up || 
    i.Position == position + Vector2I.Down);
  
  public Enemy? GetEnemyAt(Vector2I position) => _combatManager.Enemies.FirstOrDefault(enemy => enemy.Position == position);
  public Interactable? GetInteractableAt(Vector2I position) => _dungeon.Interactables.FirstOrDefault(interactable => interactable.Position == position);
  
  public bool IsEmpty(Vector2I position) => 
    !IsWall(position)
    && !IsInteractable(position)
    && !IsEnemy(position)
    && Player.Position != position;
  
  private void UpdateFogOfWar() {
    var tiles = GetTilesInRange(Player.Position, Player.Vision);
    var edge = GetTilesExactlyInRange(Player.Position, Player.Vision + 1);
    
    foreach (var tile in tiles) _dungeon.FogLayer.SetCell(tile, 0, new Vector2I(2, 0));
    foreach (var tile in edge.Where(tile => _dungeon.FogLayer.GetCellAtlasCoords(tile).X == 2)) {
	    _dungeon.FogLayer.SetCell(tile, 0, new Vector2I(1, 0));
    }
  }
  
  /// <summary>
  ///  Recalculates the path
  /// </summary>
  private void UpdatePath() {
    if (_end is null) return;
    _line.Points = _grid.GetPointPath(Player.Position, _end.Value);
  }
  
  private void OnPlayerMove(Vector2I oldPosition, Vector2I newPosition) {
    PickUpLoot();
    UpdatePath();
    QueueRedraw();
  }
  
	public void OnEntityMove(Vector2I oldPosition, Vector2I newPosition) {
		_grid.SetPointSolid(oldPosition, false);
		_grid.SetPointSolid(newPosition);
	}
	
	private void OnInteractableSolidityChange(Interactable interactable, bool solid) {
		_grid.SetPointSolid(interactable.Position, solid);
	}
	
	private void SpawnEnemy(Enemy enemy) {
		AddChild(enemy);
		enemy.SetPosition(enemy.Position);
		_combatManager.AddEnemy(enemy);
		_grid.SetPointSolid(enemy.Position);
	}
	
	public void KillEnemy(Enemy enemy) {
		RemoveChild(enemy);
		enemy.QueueFree();
		_grid.SetPointSolid(enemy.Position, false);

		foreach (var loot in enemy.Inventory.GetCards().Select(card => new CardLoot { Card = card })) {
			loot.SetPosition(enemy.Position);
			_loot.Add(loot);
			AddChild(loot);
		}
	}
	
  private void SpawnInteractable(Interactable interactable) {
    AddChild(interactable);
    interactable.SetPosition(interactable.Position);
    interactable.OnSolidityChangeEvent += OnInteractableSolidityChange;
    _grid.SetPointSolid(interactable.Position, interactable.Solid);
  }
  
  private void SpawnPlayer(Player player, Vector2I position) {
	  //AddChild(player);
	  player.SetPosition(position);
	  player.OnMoveEvent += OnPlayerMove;
	  _grid.SetPointSolid(player.Position);
  }
  
  private void SpawnLoot(CardLoot loot, Vector2I position) {
    if (!IsEmpty(position)) return;
    AddChild(loot);
    loot.SetPosition(position);
    _loot.Add(loot);
  }
	
  public List<Vector2I> GetInteractablePositions(Vector2I position) {
    List<Vector2I> adjacentPositions = new() {
	    position + Vector2I.Up,
	    position + Vector2I.Down,
	    position + Vector2I.Left,
	    position + Vector2I.Right,
    };
    
    return _dungeon.Interactables.Where(i => adjacentPositions.Contains(i.Position)).ToList().Select(i => i.Position).ToList();
  }
  
  public void Interact(Vector2I position) {
    var interactable = _dungeon.Interactables.FirstOrDefault(interactable => interactable.Position == position);
    interactable?.OnInteract(Player, Camera);
	}
  
	private void PickUpLoot() {
		List<Card> cards = new ();
		foreach (var loot in _loot.Where(loot => loot.Position == Player.Position).ToList()) {
			_loot.Remove(loot);
			RemoveChild(loot);
			loot.QueueFree();
			cards.Add(loot.Card);
		}
		Player.PickUpCards(cards);
	}
	
	public int GetDistanceBetween(Vector2I from, Vector2I to) {
		if (!_grid.IsInBoundsv(from) || !_grid.IsInBoundsv(to)) return -1;
		return _grid.GetPointPath(from, to).Length - 1;
	}
	
	public List<Vector2I>? GetPathBetween(Vector2I from, Vector2I to) {
		if (!_grid.IsInBoundsv(from) || !_grid.IsInBoundsv(to)) return null;
		if (from == to) return new List<Vector2I>();
		var path = _grid.GetPointPath(from, to, true).ToList();
		if (path.Count == 0) return null;
		path = path.GetRange(1, path.Count - 1);
		return path.Select(p => new Vector2I((int)p.X / Global.GlobalTileSize.X, (int)p.Y / Global.GlobalTileSize.Y)).ToList();
	}
	
	public Vector2[] GetPointPathBetween(Vector2I from, Vector2I to) {
		return _grid.GetPointPath(from, to, true).ToList().Select(p => new Vector2(p.X, p.Y) + Global.GlobalTileSize / 2).ToArray();
	}
	
	public static List<Vector2I> GetTilesInRange(Vector2I from, int range) {
		var tiles = new List<Vector2I>();
        
		for (var x = -range; x <= range; x++) {
			for (var y = -(range - Math.Abs(x)); y <= range - Math.Abs(x); y++) {
				tiles.Add(from + new Vector2I(x, y));
			}
		}
        
		return tiles;
	}
	
	private static List<Vector2I> GetTilesExactlyInRange(Vector2I from, int range) {
		var tiles = new List<Vector2I>();
        
		for (var i = -range; i <= range; i++) {
			var x = from.X + i;
			var dy = range - Math.Abs(i);
			var y1 = from.Y + dy;
			var y2 = from.Y - dy;
			tiles.Add(new Vector2I(x, y1));
			if (y1 != y2) tiles.Add(new Vector2I(x, y2));
		}
        
		return tiles;
	}
	
	public List<Vector2I> GetEmptyTilesExactlyInRange(Vector2I position, int range, Vector2I? exclude = null) =>
		GetTilesExactlyInRange(position, range).Where(tile => tile == exclude || IsEmpty(tile)).ToList();
	
	private void SetupSelection(int range, Vector2I from) {
		_selectedCell = from;
		_selectionRange = range;
		_selectionOrigin = from;
		_selectionCancelled = false;
		_selectionConfirmed = false;
	}
	
	public List<Enemy> GetEnemiesInRange(int range, Vector2I from) => _combatManager.Enemies.Where(enemy => Utils.ManhattanDistanceBetween(from, enemy.Position) <= range).ToList();
	public List<Interactable> GetInteractablesInRange(int range, Vector2I from) => _dungeon.Interactables.Where(interactable => Utils.ManhattanDistanceBetween(from, interactable.Position) <= range).ToList();
	
	private async Task<Enemy?> SelectEnemyTarget(int range, Vector2I from) {
		_selectionMode = SelectionMode.Enemy;
		SetupSelection(range, from);
		
		await WaitUntilMet(() => _selectionCancelled || _selectionConfirmed && IsEnemy(_selectedCell));
		
		_selectionMode = SelectionMode.None;
		
		if (_selectionCancelled) return null;
		return GetEnemyAt(_selectedCell);
	}
	
	private async Task<Interactable?> SelectInteractableTarget(int range, Vector2I from) {
		_selectionMode = SelectionMode.Interactable;
		SetupSelection(range, from);
		
		await WaitUntilMet(() => _selectionCancelled || _selectionConfirmed && IsInteractable(_selectedCell));
		
		_selectionMode = SelectionMode.None;

		if (_selectionCancelled) return null;
		return GetInteractableAt(_selectedCell);
	}
	
	private async Task<Vector2I?> SelectLocationTarget(int range, Vector2I from) {
		_selectionMode = SelectionMode.Location;
		SetupSelection(range, from);
		
		await WaitUntilMet(() => _selectionCancelled || _selectionConfirmed);
		
		_selectionMode = SelectionMode.None;

		if (_selectionCancelled) return null;
		return _selectedCell;
	}

	private async Task WaitUntilMet(Func<bool> condition) {
		while (!condition()) {
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}
	
	public async Task<bool> PlayCard(Card card) {
		var success = true;
		
		_selectedTargetingCard = card as TargetingCard;
		
		switch (card) {
			case PlayerTargetingCard playerTargetingCard:
				success = playerTargetingCard.OnPlay(Player);
				break;
			case EnemyTargetingCard enemyTargetingCard:
				var enemy = await SelectEnemyTarget(enemyTargetingCard.Range, Player.Position);
				success = enemy is not null && enemyTargetingCard.OnPlay(Player, enemy, this);
				break;
			case InteractableTargetingCard interactableTargetingCard:
				var interactable = await SelectInteractableTarget(interactableTargetingCard.Range, Player.Position);
				success = interactable is not null && interactableTargetingCard.OnPlay(Player, interactable, this);
				break;
			case LocationTargetingCard locationTargetingCard:
				var position = await SelectLocationTarget(locationTargetingCard.Range, Player.Position);
				success = position is not null && locationTargetingCard.OnPlay(Player, position.Value, this);
				break;
		}
		
		GD.Print("Played card");
		
		_selectedTargetingCard = null;
		
		return success;
	}
}