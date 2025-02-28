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

    public virtual void OnTurn(Player player)
    {
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
    }

    public virtual void OnDamaged(Player player, int damage)
    {
        base.OnDamaged(player, damage);
    }

    public virtual void OnTargeted(Player player)
    {
        
    }
    
    public virtual void OnDeath(Player player)
    {
        base.OnDeath(player);
    }
}