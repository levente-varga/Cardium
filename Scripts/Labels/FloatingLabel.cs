using System;
using Godot;

namespace Cardium.Scripts.Labels;

public partial class FloatingLabel : DisappearingLabel
{
    private readonly Random _random = new ();
    private Vector2 _velocity = new (0, 0);
    
    private Label _label;
    private Label _shadow;
    
    public override void _Ready()
    {
        _velocity = new Vector2(_random.Next(80, 100), _random.Next(-500, -400));
        
        base._Ready();
    }

    public override void _Process(double delta)
    {
        _velocity += new Vector2(0, 1200f) * (float) delta;
        Position += _velocity * (float) delta;
        
        base._Process(delta);
    }
}