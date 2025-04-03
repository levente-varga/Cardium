using System;
using Godot;

namespace Cardium.Scripts.Labels;

public partial class DisappearingLabel : LabelWithShadow
{
    protected readonly ulong SpawnTime = Time.GetTicksMsec();
    protected readonly Random Random = new ();
    public float? LifetimeMillis;
    private const float DefaultLifetimeMillis = 1200f;

    
    public override void _Ready()
    {
        base._Ready();
    }
    
    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() - SpawnTime > (LifetimeMillis ?? DefaultLifetimeMillis))
        {
            QueueFree();
            return;
        }
        
        var progress = 1f - (Time.GetTicksMsec() - SpawnTime) / (LifetimeMillis ?? DefaultLifetimeMillis);
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Mathf.Pow(progress, 0.5f));
        
        base._Process(delta);
    }
}