using Godot;

namespace Cardium.Scripts.Enemies;

public class SlimeEnemyBehavior : EnemyBehavior
{
    public override int MaxHealth => 2;
    public override int MaxEnergy => 1;
    public override int Armor => 0;
    public override int Damage => 1;
    public override int Range => 1;
    public override int Vision => 3;
    public override string Name => "Slime";
    public override string Description => "A slime enemy.";
    public override Texture2D Sprite => GD.Load<Texture2D>("res://Assets/Sprites/Enemies/slime.png");
    
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