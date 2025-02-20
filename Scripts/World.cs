using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public abstract partial class World : Node2D
{
    private List<WorldLayer> _layers = new();
    private static readonly Vector2I TileSize = new(64, 64);


    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
		
    }
}