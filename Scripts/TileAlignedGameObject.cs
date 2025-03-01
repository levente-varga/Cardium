using Godot;

namespace Cardium.Scripts;

public partial class TileAlignedGameObject : Sprite2D
{
    public new Vector2I Position { get; set; }
    private Vector2I _previousPosition;
    
    public delegate void OnMoveDelegate(TileAlignedGameObject gameGameObject);
    public event OnMoveDelegate OnMoveEvent;
    
    public override void _Process(double delta)
    {
        if (base.Position.DistanceTo(Position * Global.TileSize) < 1)
        {
            base.Position = Position * Global.TileSize;
        }
        else
        {
            base.Position = base.Position.Lerp(Position * Global.TileSize, Global.LerpWeight * (float) delta);
        }

        if (_previousPosition != Position)
        {
            OnMoveEvent?.Invoke(this);
            _previousPosition = Position;
        }
    }
    
    public void SetPosition(Vector2I position)
    {
        Position = position;
        base.Position = position * Global.TileSize;
    }
}