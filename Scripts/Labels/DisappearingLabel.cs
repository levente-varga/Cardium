using Godot;

namespace Cardium.Scripts.Labels;

public partial class DisappearingLabel : LabelWithShadow
{
    protected readonly ulong SpawnTime = Time.GetTicksMsec();
    public float LifetimeMillis = 1200f;
    
    public override void _Ready()
    {
        base._Ready();
    }
    
    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() - SpawnTime > LifetimeMillis)
        {
            QueueFree();
        }
        
        var progress = 1f - (Time.GetTicksMsec() - SpawnTime) / LifetimeMillis;
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Mathf.Pow(progress, 0.5f));
        
        base._Process(delta);
    }
}