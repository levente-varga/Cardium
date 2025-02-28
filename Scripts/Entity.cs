using Godot;

namespace Cardium.Scripts;

public partial class Entity : Sprite2D
{
    public delegate void OnMoveDelegate();
    public event OnMoveDelegate OnMoveEvent;
    
    public new Vector2I Position { get; protected set; }
    private Vector2I _previousPosition;
    
    public override void _Ready()
    {
        Position = Vector2I.Zero;
    }

    public override void _Process(double delta)
    {
        base.Position = base.Position.Lerp(Position * Global.TileSize, 0.1f);
        
        if (_previousPosition != Position)
        {
            OnMoveEvent?.Invoke();
            _previousPosition = Position;
        }
    }
}