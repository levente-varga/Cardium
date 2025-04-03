using Godot;

namespace Cardium.Scripts;

public partial class TurnMarker : Node2D
{
    private Polygon2D _dot = new ();

    public override void _Ready()
    {
        base._Ready();
        
        _dot.Polygon = new Vector2[]
        {
            new (0, 0),
            new (0, 4),
            new (1, 4),
            new (1, 0)
        };
        _dot.Color = Global.Yellow;
        AddChild(_dot);
        _dot.Position = new Vector2(0, -4);
    }
    
    public override void _Process(double delta)
    {
        
    }
}