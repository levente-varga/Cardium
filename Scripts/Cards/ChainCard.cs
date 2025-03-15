using System.Linq;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class ChainCard : EnemyTargetingCard
{
    public int Damage { get; set; } = 1;
    public int BounceRange { get; set; } = 3;
    public int Bounces { get; set; } = 2;
    
    public ChainCard()
    {
        DisplayName = "Chain";
        Description = "Deals 1 damage to an enemy, bounces to up to 2 other enemies in range 3.";
        Cost = 2;
        Range = 3;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Chain.png");
        Type = CardType.Combat;
    }

    public override void OnPlay(Player player, Enemy enemy, World world)
    {
        var otherEnemies = world.GetEnemiesInRange(BounceRange, enemy.Position);

        for (var i = 0; i < otherEnemies.Count && otherEnemies.Count < Bounces - 1; i++)
        {
            otherEnemies.AddRange(world.GetEnemiesInRange(BounceRange, otherEnemies[i].Position).Where(e => !otherEnemies.Contains(e)));
        }
        
        enemy.ReceiveDamage(player, Damage);
        for (var i = 0; i < otherEnemies.Count && i < Bounces; i++)
        {
            otherEnemies[i].ReceiveDamage(player, Damage);
        }
    }
}