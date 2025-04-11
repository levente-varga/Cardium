using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Cardium.Scripts;

public class DungeonGenerator {
  public int NumRoomTries = 100;
  public int ExtraConnectorChance => 5;
  public int RoomExtraSize => 0;
  public int WindingPercent => 70;
  public int MinSmallRooms => 2;
  public Vector2I MinSize => new Vector2I(11, 11);

  private static readonly List<Vector2I> Directions = new() {
    Vector2I.Down, Vector2I.Up, Vector2I.Left, Vector2I.Right
  };
  
  private readonly List<List<TileTypes>> _tiles = new();
  private List<Rect2I> _rooms = new();
  private Vector2I _size;
  private Rect2I _bounds;
  
  private readonly Random _random = new ();

  /// For each open position in the dungeon, the index of the connected region
  /// that that position is a part of.
  private readonly List<List<int>> _regions = new();

  /// The index of the current region being carved.
  int _currentRegion = -1;

  public Dungeon Generate(int width, int height, int roomTries = 100) => Generate(new Vector2I(width, height), roomTries); 
  public Dungeon Generate(Vector2I size, int roomTries = 100) {
    GD.Print($"Started generating a {size.X}x{size.Y} dungeon");
    
    if (size.X % 2 == 0 || size.Y % 2 == 0) {
      throw new Exception("The dungeon must be odd-sized.");
    }
    if (size.X <= MinSize.X || size.Y <= MinSize.Y) {
      throw new Exception($"The dungeon must be at least {MinSize.X}x{MinSize.Y} to ensure everything fits inside.");
    }
    
    NumRoomTries = roomTries;
    _size = size;
    _bounds = new Rect2I(Vector2I.Zero, size);
    
    InitArea(_size);
    GenerateRooms();
    GenerateMaze();
    ConnectRegions();
    RemoveDeadEnds();
    
    return new Dungeon(_tiles, _rooms);
  }
  
  /// Implementation of the "growing tree" algorithm from here:
  /// http://www.astrolog.org/labyrnth/algrithm.htm.
  private void GrowMaze(Vector2I start) {
    var cells = new List<Vector2I>();
    Vector2I lastDirection = Vector2I.Zero;

    StartRegion();
    Carve(start);

    cells.Add(start);
    while (cells.Count > 0) {
      var cell = cells.Last();

      // See which adjacent cells are open.
      var unmadeCells = new List<Vector2I>();

      foreach (var direction in Directions) {
        if (CanCarve(cell, direction)) unmadeCells.Add(direction);
      }

      if (unmadeCells.Count > 0) {
        // Based on how "windy" passages are, try to prefer carving in the
        // same direction.
        Vector2I direction;
        if (unmadeCells.Contains(lastDirection) && _random.Next(100) > WindingPercent) {
          direction = lastDirection;
        } 
        else {
          direction = unmadeCells[_random.Next(0, unmadeCells.Count)];
        }

        Carve(cell + direction);
        Carve(cell + direction * 2);

        cells.Add(cell + direction * 2);
        lastDirection = direction;
      } 
      else {
        cells.Remove(cell);
        lastDirection = Vector2I.Zero;
      }
    }
  }

  private void GenerateRooms() {
    _rooms = new();

    // Generate the minimum required small (3x3) rooms, for a guaranteed exit and spawn point
    var remainingSmallRooms = MinSmallRooms;
    while (remainingSmallRooms > 0) {
      if (PlaceRoom(3, 3)) {
        remainingSmallRooms--;
      }
    }
    
    for (var i = 0; i < NumRoomTries; i++) {
      var size = _random.Next(1, 3 + RoomExtraSize) * 2 + 1;
      var rectangularity = _random.Next(0, 1 + size / 2) * 2;
      var width = size;
      var height = size;
      if (_random.Next(0, 2) == 0) {
        width += rectangularity;
      } 
      else {
        height += rectangularity;
      }

      PlaceRoom(new Vector2I(width, height));
    }
  }

  private bool PlaceRoom(int width, int height) => PlaceRoom(new Vector2I(width, height)); 
  private bool PlaceRoom(Vector2I size) {
    Vector2I position = new (
      _random.Next(0, (_size.X - size.X) / 2) * 2 + 1,
      _random.Next(0, (_size.Y - size.Y) / 2) * 2 + 1
    );

    var room = new Rect2I(position.X, position.Y, size.X, size.Y);

    var overlaps = _rooms.Any(other => room.Intersects(other));
    if (overlaps) return false;

    _rooms.Add(room);

    StartRegion();
    for (var x = room.Position.X; x < room.End.X; x++) {
      var perimeterX = x == room.Position.X || x == room.End.X - 1;
      for (var y = room.Position.Y; y < room.End.Y; y++) {
        var perimeterY = y == room.Position.Y || y == room.End.Y - 1;
        var type = TileTypes.RoomInterior;
        if (perimeterX && perimeterY) type = TileTypes.RoomCorner;
        else if (perimeterX || perimeterY) type = TileTypes.RoomPerimeter;
        Carve(new Vector2I(x, y), type);
      }
    }

    return true;
  }

  private void GenerateMaze() {
    for (var y = 1; y < _size.Y; y += 2) {
      for (var x = 1; x < _size.X; x += 2) {
        var tile = new Vector2I(x, y);
        if (!IsWall(tile)) continue;
        GrowMaze(tile);
      }
    }
  }
  
  private void ConnectRegions() {
    // Find all the tiles that can connect two (or more) regions.
    var connectorRegions = new Dictionary<Vector2I, HashSet<int>>();
    
    var shrunkenBounds = _bounds.Grow(-1);

    for (var x = shrunkenBounds.Position.X; x < shrunkenBounds.End.X; x++) {
      for (var y = shrunkenBounds.Position.Y; y < shrunkenBounds.End.Y; y++) {
        var pos = new Vector2I(x, y);
        
        if (!IsWall(pos)) continue;

        var regions = new HashSet<int>();
        foreach (var dir in Directions) {
          var dirPos = pos + dir;
          if (IsWall(dirPos)) continue;
          var region = _regions[dirPos.X][dirPos.Y];
          regions.Add(region);
        }
        
        if (regions.Count < 2) continue;

        connectorRegions[pos] = regions;
      }
    }
    
    var connectors = connectorRegions.Keys.ToList();
    
    // Keep track of which regions have been merged. This maps an original
    // region index to the one it has been merged into.
    var mergedRegions = new List<int>();
    
    var openRegions = new List<int>();
    for (var i = 0; i <= _currentRegion; i++) {
      mergedRegions.Add(i);
      openRegions.Add(i);
    }
    
    // Keep connecting regions until we're down to one.
    while (openRegions.Count > 1) {
      var connector = connectors[_random.Next(0, connectors.Count)];
      
      // Carve the connection.
      AddJunction(connector);

      // Merge the connected regions. We'll pick one region (arbitrarily) and
      // map all the other regions to its index.
      var regions = connectorRegions[connector].Select(region => mergedRegions[region]).ToList();
      var target = regions.First();
      var sources = regions.Skip(1).ToList();

      // Merge all the affected regions. We have to look at all the
      // regions because other regions may have previously been merged with
      // some of the ones we're merging now.
      for (var i = 0; i <= _currentRegion; i++) {
        if (sources.Contains(mergedRegions[i])) {
          mergedRegions[i] = target;
        }
      }

      // The sources are no longer in use.
      openRegions = openRegions.Except(sources).ToList();
      
      // Remove any connectors that aren't needed anymore.
      connectors.RemoveAll(pos => {
        // Don't allow connectors right next to each other.
        if (connector.DistanceTo(pos) < 2) return true;

        // If the connector no longer spans different regions, we don't need it.
        HashSet<int> adjacentRegions = new ();
        foreach (var region in connectorRegions[pos]) {
          adjacentRegions.Add(mergedRegions[region]);
        }
        
        if (adjacentRegions.Count > 1) return false;
        
        if (_random.Next(ExtraConnectorChance) == 0) AddJunction(pos);

        return true;
      });
    }
  }

  private void AddJunction(Vector2I tile) {
    for (var x = tile.X - 1; x <= tile.X + 1; x++) {
      for (var y = tile.Y - 1; y <= tile.Y + 1; y++) {
        if (Mathf.Abs(x + y) != 1) continue;
        if (_tiles[x][y] == TileTypes.RoomPerimeter) SetTile(new Vector2I(x, y), TileTypes.Doorway);
      }
    }

    SetTile(tile, TileTypes.Entrance);
  }
  
  private void RemoveDeadEnds() {
    var done = false;

    while (!done) {
      done = true;

      var shrunkenBounds = _bounds.Grow(-1);
      for (var x = shrunkenBounds.Position.X; x < shrunkenBounds.End.X; x++) {
        for (var y = shrunkenBounds.Position.Y; y < shrunkenBounds.End.Y; y++) {
          var pos = new Vector2I(x, y);
          
          if (IsWall(pos)) continue;

          // If it only has one exit, it's a dead end.
          var exits = 0;
          foreach (var dir in Directions) {
            if (!IsWall(pos + dir)) exits++;
          }

          if (exits != 1) continue;

          done = false;
          SetTile(pos);
        }
      }
    }
  }
  
  /// Gets whether an opening can be carved from the given starting
  /// [Cell] at [pos] to the adjacent Cell facing [direction]. Returns `true`
  /// if the starting Cell is in bounds and the destination Cell is filled
  /// (or out of bounds).
  private bool CanCarve(Vector2I pos, Vector2I direction) {
    // Must end in bounds.
    if (!new Rect2I(Vector2I.Zero, _size).HasPoint(pos + direction * 3)) return false;

    // Destination must not be open.
    return IsWall(pos + direction * 2);
  }

  private void StartRegion() {
    _currentRegion++;
  }

  private void Carve(Vector2I tile, TileTypes type = TileTypes.Corridor) {
    SetTile(tile, type);
    _regions[tile.X][tile.Y] = _currentRegion;
  }

  private bool IsWall(Vector2I tile) => _tiles[tile.X][tile.Y] == TileTypes.Wall;
  private void SetTile(Vector2I tile, TileTypes value = TileTypes.Wall) => _tiles[tile.X][tile.Y] = value;

  private void InitArea(Vector2I size) {
    for (var x = 0; x < size.X; x++) {
      _tiles.Add(new List<TileTypes>());
      _regions.Add(new List<int>());
      for (var y = 0; y < size.Y; y++) {
        _tiles[x].Add(TileTypes.Wall);
        _regions[x].Add(-1);
      }
    }
  }
}