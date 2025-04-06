using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Godot;

namespace Cardium.Scripts;

public class DungeonGenerator {
  public int NumRoomTries;
  public int ExtraConnectorChance => 20;
  public int RoomExtraSize => 0;
  public int WindingPercent => 0;

  //public Dungeon Dungeon = new();
  public List<List<bool>> Walls = new();
  private Dungeon _dungeon = new ();
  
  private readonly Random _random = new ();

  /// For each open position in the dungeon, the index of the connected region
  /// that that position is a part of.
  private List<List<int>> _regions = new();

  /// The index of the current region being carved.
  int _currentRegion = -1;

  public Dungeon Generate(Size size) {
    if (size.Width % 2 == 0 || size.Height % 2 == 0) {
      throw new Exception("The stage must be odd-sized.");
    }

    _dungeon = new Dungeon { Size = size };
    
    InitWalls(_dungeon.Size);
    foreach (var room in GenerateRooms()) {
      Fill(false, room);
    }

    return _dungeon;

    // Fill in all the empty space with mazes.
    for (var y = 1; y < _dungeon.Size.Height; y += 2) {
      for (var x = 1; x < _dungeon.Size.Width; x += 2) {
        var tile = new Vector2I(x, y);
        if (IsWall(tile)) continue;
        //GrowMaze(tile);
      }
    }

    //ConnectRegions();
    //RemoveDeadEnds();

    return new Dungeon();
  }
  
  /*
  /// Implementation of the "growing tree" algorithm from here:
  /// http://www.astrolog.org/labyrnth/algrithm.htm.
  private void GrowMaze(Vector2I start) {
    var cells = new List<Vector2I>();
    Direction? lastDir = null;

    StartRegion();
    Carve(start);

    cells.Add(start);
    while (cells.Count > 0) {
      var cell = cells.Last();

      // See which adjacent cells are open.
      var unmadeCells = new List<Direction>();

      foreach (var dir in Direction.CARDINAL) {
        if (CanCarve(cell, dir)) unmadeCells.Add(dir);
      }

      if (unmadeCells.Count > 0) {
        // Based on how "windy" passages are, try to prefer carving in the
        // same direction.
        Direction? dir;
        if (unmadeCells.Contains(lastDir) && _random.Next(100) > WindingPercent) {
          dir = lastDir;
        } else {
          dir = rng.item(unmadeCells);
        }

        Carve(cell + dir);
        Carve(cell + dir * 2);

        cells.Add(cell + dir * 2);
        lastDir = dir;
      } else {
        // No adjacent uncarved cells.
        cells.Remove(cells.Last());

        // This path has ended.
        lastDir = null;
      }
    }
  }
  */

  /// Places rooms ignoring the existing maze corridors.
  private List<Rect2I> GenerateRooms() {
    List<Rect2I> rooms = new();
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
        _random.Next(0, (_dungeon.Size.Width - width) / 2 * 2 + 1),
        _random.Next(0, (_dungeon.Size.Width - width) / 2 * 2 + 1)
      );

      var room = new Rect2I(position.X, position.Y, width, height);

      var overlaps = rooms.Any(other => room.Intersects(other));
      if (overlaps) continue;

      rooms.Add(room);

      StartRegion();
      for (var x = room.Position.X; x < room.End.X; x++)
        for (var y = room.Position.Y; y < room.End.Y; y++)
          Carve(new Vector2I(x, y));
    }

    return rooms;
  }

  /*
  private void ConnectRegions() {
    // Find all the tiles that can connect two (or more) regions.
    var connectorRegions = <Vector2I, Set<int>>{};
    foreach (var pos in bounds.inflate(-1)) {
      // Can't already be part of a region.
      if (getTile(pos) != Tiles.wall) continue;

      var regions = new Set<int>();
      foreach (var dir in Direction.CARDINAL) {
        var region = _regions[pos + dir];
        if (region != null) regions.add(region);
      }

      if (regions.length < 2) continue;

      connectorRegions[pos] = regions;
    }

    var connectors = connectorRegions.keys.toList();

    // Keep track of which regions have been merged. This maps an original
    // region index to the one it has been merged to.
    var merged = {};
    var openRegions = new Set<int>();
    for (var i = 0; i <= _currentRegion; i++) {
      merged[i] = i;
      openRegions.add(i);
    }

    // Keep connecting regions until we're down to one.
    while (openRegions.length > 1) {
      var connector = rng.item(connectors);

      // Carve the connection.
      AddJunction(connector);

      // Merge the connected regions. We'll pick one region (arbitrarily) and
      // map all of the other regions to its index.
      var regions = connectorRegions[connector]
        .map((region) => merged[region]);
      var dest = regions.first;
      var sources = regions.skip(1).toList();

      // Merge all of the affected regions. We have to look at *all* of the
      // regions because other regions may have previously been merged with
      // some of the ones we're merging now.
      for (var i = 0; i <= _currentRegion; i++) {
        if (sources.contains(merged[i])) {
          merged[i] = dest;
        }
      }

      // The sources are no longer in use.
      openRegions.removeAll(sources);

      // Remove any connectors that aren't needed anymore.
      connectors.removeWhere((pos) {
        // Don't allow connectors right next to each other.
        if (connector - pos < 2) return true;

        // If the connector no long spans different regions, we don't need it.
        var regions = connectorRegions[pos].map((region) => merged[region])
          .toSet();

        if (regions.length > 1) return false;

        // This connecter isn't needed, but connect it occasionally so that the
        // dungeon isn't singly-connected.
        if (_random.Next(extraConnectorChance) == 0) AddJunction(pos);

        return true;
      });
    }
  }
  */

  private void AddJunction(Vector2I tile) {
    SetWall(tile, false);
    /* For supporting door generation
    if (_random.Next(4) < 1) {
      SetTile(tile, _random.Next(3) < 1 ? Tiles.openDoor : Tiles.floor);
    } else {
      SetTile(tile, Tiles.closedDoor);
    }
    */
  }

  /*
  private void RemoveDeadEnds() {
    var done = false;

    while (!done) {
      done = true;

      foreach (var pos in bounds.inflate(-1)) {
        if (IsWall(pos)) continue;

        // If it only has one exit, it's a dead end.
        var exits = 0;
        foreach (var dir in Direction.CARDINAL) {
          if (IsWall(pos + dir)) exits++;
        }

        if (exits != 1) continue;

        done = false;
        SetWall(pos);
      }
    }
  }
  */

  /*
  /// Gets whether an opening can be carved from the given starting
  /// [Cell] at [pos] to the adjacent Cell facing [direction]. Returns `true`
  /// if the starting Cell is in bounds and the destination Cell is filled
  /// (or out of bounds).
  private bool CanCarve(Vector2I pos, Direction direction) {
    // Must end in bounds.
    if (!bounds.contains(pos + direction * 3)) return false;

    // Destination must not be open.
    return IsWall(pos + direction * 2);
  }
  */

  private void StartRegion() {
    _currentRegion++;
  }

  private void Carve(Vector2I tile) {
    SetWall(tile, false);
    _regions[tile.X][tile.Y] = _currentRegion;
  }

  private bool IsWall(Vector2I tile) => Walls[tile.X][tile.Y];
  private bool SetWall(Vector2I tile, bool value = true) => Walls[tile.X][tile.Y] = value;

  private void InitWalls(Size size) {
    for (var x = 0; x < size.Width; x++) {
      Walls.Add(new List<bool>());
      for (var y = 0; y < size.Height; y++) {
        Walls[x].Add(true);
      }
    }
  }
  
  private void Fill(bool wall, Rect2I? area = null) {
    Rect2I fillArea = area ?? new Rect2I(0, 0, _dungeon.Size.Width, _dungeon.Size.Height);
    for (var x = fillArea.Position.X; x < fillArea.Size.X; x++) {
      for (var y = fillArea.Position.Y; y < fillArea.Size.Y; y++) {
        Walls[x][y] = wall;
      }
    }
  }
}