using System;
using Godot;

namespace Cardium.Scripts;

public partial class FallingLabel : Label
{
    private readonly Random _random = new ();
    private Vector2 _velocity = new (0, 0);
    private ulong _spawnTime = Time.GetTicksMsec();
    
    public float LifetimeMillis = 1200f;
    
    public override void _Ready()
    {
        var font = GD.Load<FontFile>("res://Assets/Fonts/alagard.ttf");
        AddThemeFontOverride("font", font);
        AddThemeFontSizeOverride("font_size", 40);
        
        _velocity = new Vector2(_random.Next(80, 100), _random.Next(-500, -400));
    }

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() - _spawnTime > LifetimeMillis)
        {
            QueueFree();
            return;
        }
        _velocity += new Vector2(0, 1200f) * (float) delta;
        Position += _velocity * (float) delta;
        
        var progress = 1f - (Time.GetTicksMsec() - _spawnTime) / LifetimeMillis;
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, MathF.Pow(progress, 0.5f));
    }
}