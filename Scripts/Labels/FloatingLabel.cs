using Godot;

namespace Cardium.Scripts.Labels;

public partial class FloatingLabel : DisappearingLabel
{
    private Vector2 _velocity = new (0, 0);
    
    public override void _Ready()
    {
        Position += Vector2.Up * Global.GlobalTileSize; 
        _velocity = new Vector2(Random.Next(-50, 50), Random.Next(-300, -240));
        
        base._Ready();
    }

    public override void _Process(double delta)
    {
        _velocity = _velocity.Lerp(new Vector2(0, 0), 3 * (float) delta);
        Position += _velocity * (float) delta;
        
        base._Process(delta);
    }
}