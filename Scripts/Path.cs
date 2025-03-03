using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Path : Line2D
{
    public override void _Ready()
    {
        DefaultColor = Global.Yellow;
        Scale = new Vector2(Global.Scale, Global.Scale).Inverse();
        Width = 4;
        ZIndex = 1;
        base._Ready();
    }

    public override void _Process(double delta)
    {
        GlobalPosition = Vector2.Zero;
        Visible = Global.Debug;
        
        base._Process(delta);
    }

    public void SetPath(Vector2[] path)
    {
        Points = path;
    }
}