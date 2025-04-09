using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace Cardium.Scripts;

public class DungeonGenerator {
  public int NumRoomTries = 100;
  public int ExtraConnectorChance => 20;
  public int RoomExtraSize => 1;
  public int WindingPercent => 50;

  private static readonly List<Vector2I> Directions = new() {
    Vector2I.Down, Vector2I.Up, Vector2I.Left, Vector2I.Right
  };

  //public Dungeon Dungeon = new();
  private readonly List<List<Tiles>> _tiles = new();
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
    NumRoomTries = roomTries;
    _size = size;
    _bounds = new Rect2I(Vector2I.Zero, size);
    if (size.X % 2 == 0 || size.Y % 2 == 0) {
      throw new Exception("The stage must be odd-sized.");
    }
    
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
        // No adjacent uncarved cells.
        cells.Remove(cell);

        // This path has ended.
        lastDirection = Vector2I.Zero;
      }
    }
  }

  /// Places rooms ignoring the existing maze corridors.
  private void GenerateRooms() {
    _rooms = new();
    for (var i = 0; i < NumRoomTries; i++) {
      // Pick a random room size. The funny math here does two things:
      // - It makes sure rooms are odd-sized to line up with maze.
      // - It avoids creating rooms that are too rectangular: too tall and
      //   narrow or too wide and flat.
      // TODO: This isn't very flexible or tunable. Do something better here.
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

      Vector2I position = new (
        _random.Next(0, (_size.X - width) / 2) * 2 + 1,
        _random.Next(0, (_size.Y - height) / 2) * 2 + 1
      );

      var room = new Rect2I(position.X, position.Y, width, height);

      var overlaps = _rooms.Any(other => room.Intersects(other));
      if (overlaps) continue;

      _rooms.Add(room);

      StartRegion();
      for (var x = room.Position.X; x < room.End.X; x++) {
        var perimeterX = x == room.Position.X || x == room.End.X - 1;
        for (var y = room.Position.Y; y < room.End.Y; y++) {
          var perimeterY = y == room.Position.Y || y == room.End.Y - 1;
          var type = Tiles.RoomInterior;
          if (perimeterX && perimeterY) type = Tiles.RoomCorner;
          else if (perimeterX || perimeterY) type = Tiles.RoomPerimeter;
          Carve(new Vector2I(x, y), type);
        }
      }
    }
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
  
  [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
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
    SetWall(tile, Tiles.RoomEntrance);
    
    // For supporting door generation:
    /*
    if (_random.Next(4) < 1) {
      SetTile(tile, _random.Next(3) < 1 ? Tiles.openDoor : Tiles.floor);
    } else {
      SetTile(tile, Tiles.closedDoor);
    }
    */
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
          SetWall(pos);
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

  private void Carve(Vector2I tile, Tiles type = Tiles.Corridor) {
    SetWall(tile, type);
    _regions[tile.X][tile.Y] = _currentRegion;
  }

  private bool IsWall(Vector2I tile) => _tiles[tile.X][tile.Y] == Tiles.Wall;
  private void SetWall(Vector2I tile, Tiles value = Tiles.Wall) => _tiles[tile.X][tile.Y] = value;

  private void InitArea(Vector2I size) {
    for (var x = 0; x < size.X; x++) {
      _tiles.Add(new List<Tiles>());
      _regions.Add(new List<int>());
      for (var y = 0; y < size.Y; y++) {
        _tiles[x].Add(Tiles.Wall);
        _regions[x].Add(-1);
      }
    }
  }
}