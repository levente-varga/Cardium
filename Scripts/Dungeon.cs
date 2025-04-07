using System;
using System.Collections.Generic;
using System.Drawing;
using Godot;

namespace Cardium.Scripts;

public class Dungeon
{
    private AStarGrid2D _grid = new ();

    public TileMapLayer GroundLayer = new ();
    public TileMapLayer DecorLayer = new ();
    public TileMapLayer WallLayer = new ();
    public TileMapLayer ObjectLayer = new ();
    public TileMapLayer EnemyLayer = new ();
    public TileMapLayer LootLayer = new ();
    public TileMapLayer FogLayer = new ();
    public Overlay Overlay = new ();

    private Size _size = new (31, 31);
    public Size Size { get => _size; set => SetSize(value); }

    public void SetSize(Size size)
    {
        _size = new Size(
            Math.Max(1, size.Width) / 2 * 2 + 1,
            Math.Max(1, size.Height) / 2 * 2 + 1
            );
    }

    public Dungeon()
    {
        //WallLayer.Scale = new Vector2(4, 4);
        WallLayer.TileSet = ResourceLoader.Load<TileSet>("res://Assets/TileSets/wall_atlas.tres");
    }

    public static Dungeon From(List<List<int>> walls)
    {
        var dungeon = new Dungeon();
        for (var x = 0; x < walls.Count; x++)
        {
            for (var y = 0; y < walls[x].Count; y++)
            {
                //dungeon._grid.SetPointSolid(new Vector2I(x, y), walls[x][y]);
                dungeon.WallLayer.SetCell(
                    new Vector2I(x, y), 
                    0, 
                    walls[x][y] == -1 
                        ? new Vector2I(39, 21)
                        : walls[x][y] == -2 
                            ? new Vector2I(38, 21)
                            : new Vector2I(35 + walls[x][y], 17)
                    );
            }
        }

        dungeon.PrettyWalls();

        return dungeon;
    }

    /// <summary>
    /// Sets wall sprites according to their surrounding cells
    /// </summary>
    private void PrettyWalls()
    {
        
    }
}