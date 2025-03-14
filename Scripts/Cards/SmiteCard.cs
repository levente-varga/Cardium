using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class SmiteCard : LocationTargetingCard
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
    
    public override void OnPlay(Player player, Vector2I position, World world)
    {
        if (world.GetEnemyAt(position) is not null)
        {
            world.GetEnemyAt(position)?.ReceiveDamage(player, 3);
        }
    }
}