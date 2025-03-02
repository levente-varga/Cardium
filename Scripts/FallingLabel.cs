using System;
using Godot;

namespace Cardium.Scripts;

public partial class FallingLabel : Node2D
{
    private readonly Random _random = new ();
    private Vector2 _velocity = new (0, 0);
    private ulong _spawnTime = Time.GetTicksMsec();
    
    private Label _label;
    private Label _shadow;
    
    public float LifetimeMillis = 1200f;
    public string Text;
    public Color Color;
    
    public override void _Ready()
    {
        var font = GD.Load<FontFile>("res://Assets/Fonts/alagard.ttf");

        ZIndex = 100;
        
        _label = new Label();
        _label.Text = Text;
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", 40);
        _label.Modulate = Color;
        _label.ZIndex = 1;
        AddChild(_label);
        
        var shadow = new Label();
        shadow.Text = Text;
        shadow.AddThemeFontOverride("font", font);
        shadow.AddThemeFontSizeOverride("font_size", 40);
        shadow.Modulate = new Color(0, 0, 0, 1f);
        shadow.Position = new Vector2(4, 4);
        shadow.ZIndex = _label.ZIndex - 1;
        AddChild(shadow);
        
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