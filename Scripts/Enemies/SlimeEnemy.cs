using Godot;

namespace Cardium.Scripts.Enemies;

public partial class SlimeEnemy : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        Texture = GD.Load<Texture2D>("res://assets/Sprites/Enemies/slime.png");
        Name = "Slime";
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnTurn(Player player)
    {
        if (Energy > 0)
        {
            // TODO: Implement enemy AI
        }
    }

    public override void OnDamaged(Player player, int damage)
    {
        base.OnDamaged(player, damage);
    }

    public override void OnTargeted(Player player)
    {
        
    }
    
    public override void OnDeath(Player player)
    {
        base.OnDeath(player);
    }
}