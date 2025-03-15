using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class SmiteCard : EnemyTargetingCard
{
    public SmiteCard()
    {
        DisplayName = "Smite";
        Description = "Deals 3 damage to a single target.";
        Cost = 2;
        Range = 3;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Smite.png");
        Type = CardType.Combat;
    }
    
    public override void OnPlay(Player player, Enemy target, World world)
    {
        target.ReceiveDamage(player, 3);
    }
}