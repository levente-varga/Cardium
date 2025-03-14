using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class ChainCard : EnemyTargetingCard
{
    public ChainCard()
    {
        DisplayName = "Chain";
        Description = "Deals 1 damage to an enemy, bounces to up to 2 other enemies in range 3.";
        Cost = 2;
        Range = 3;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Chain.png");
        Type = CardType.Combat;
    }

    public override void OnPlay(Player player, Enemy enemy)
    {
        
    }
}