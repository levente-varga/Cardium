using Godot;

namespace Cardium.Scripts;

public partial class Enemy : Entity
{
    public override void _Ready()
    {
        base._Ready();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnTurn(Entity source)
    {
        base.OnTurn(source);
        
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
    }

    public override void OnDamaged(Entity source, int damage)
    {
        Blink();
        
        GD.Print(Name + " received " + damage + " damage from " + source.Name + ". Current health: " + Health + "/" + MaxHealth);
        
        base.OnDamaged(source, damage);
    }

    public override void OnTargeted(Entity source)
    {
        base.OnTargeted(source);
    }
    
    public override void OnDeath(Entity source)
    {
        base.OnDeath(source);
    }
}