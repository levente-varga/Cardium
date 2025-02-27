using Godot;

namespace Cardium.Scripts;

public partial class GameManager : Node
{
    [Export] public Player Player;
    [Export] public World World;
    
    public bool IsTileEmpty(Vector2I position) => 
        World.WallLayer.GetCellTileData(position) == null
        && World.ObjectLayer.GetCellTileData(position) == null;
}