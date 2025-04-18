using Godot;

namespace Cardium.Scripts.Labels;

public partial class FloatingLabel : DisappearingLabel {
    private Vector2 _targetPosition;
    public int Height { get; init; } = 200;

    public override void _Ready() {
        _targetPosition = Position + new Vector2(0, -Height);
        Position += Vector2.Up * Global.GlobalTileSize; 
        
        base._Ready();
    }

    public override void _Process(double delta) {
        Position = Position.Lerp(_targetPosition, (float)delta * 5);
        
        base._Process(delta);
    }
}