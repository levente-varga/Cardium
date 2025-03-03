using System;
using Godot;

namespace Cardium.Scripts;

public partial class EnergyDot : Node2D
{
    private const float Size = 1;
    
    private readonly Random _random = new ();
    private Vector2 _velocity = Vector2.Zero;
    
    private ulong _throwTime;
    private const float DefaultLifetimeMillis = 1200f;
    
    private Polygon2D _dot;
    private Polygon2D _shadow;

    public bool IsThrown { get; private set; }

    public override void _Ready()
    {
        Name = "EnergyDot";
        ZIndex = 10;
        
        _dot = new Polygon2D();
        _dot.Color = Global.Purple;
        _dot.Name = "Dot";
        _dot.Visible = true;
        _dot.ZIndex = 10;
        _dot.Polygon = new Vector2[]
        {
            new (0, 0),
            new (Size, 0),
            new (Size, Size),
            new (0, Size)
        };
        AddChild(_dot);
        
        _shadow = new Polygon2D();
        _shadow.Color = Global.Black;
        _shadow.Name = "Shadow";
        _shadow.Visible = true;
        _shadow.ZIndex = _dot.ZIndex - 1;
        _shadow.Position = _dot.Position + Vector2.One;
        _shadow.Polygon = _dot.Polygon;
        AddChild(_shadow);
    }
    
    public void Throw()
    {
        if (IsThrown) return;
        _throwTime = Time.GetTicksMsec();
        _velocity = new Vector2(_random.Next(400, 500), _random.Next(-50, 50));
        IsThrown = true;
    }

    public void CancelThrow()
    {
        FinishThrow();
        Visible = true;
        IsThrown = false;
    }

    private void FinishThrow()
    {
        _velocity = Vector2.Zero;
        Position = Vector2.Zero;
        Visible = false;
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1);
    }

    public override void _Process(double delta)
    {
        if (_velocity == Vector2.Zero) return;
        
        if (Time.GetTicksMsec() - _throwTime > DefaultLifetimeMillis)
        {
            FinishThrow();
            return;
        }
        
        var progress = 1f - (Time.GetTicksMsec() - _throwTime) / DefaultLifetimeMillis;
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Mathf.Pow(progress, 0.5f));
        
        _velocity = _velocity.Lerp(new Vector2(0, 0), 3 * (float) delta);
        Position += _velocity * (float) delta;
    }
}