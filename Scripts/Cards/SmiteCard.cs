using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class SmiteCard : EnemyTargetingCard
{
    public int Damage { get; set; } = 5;
    
    public SmiteCard()
    {
        DisplayName = "Smite";
        Description = $"Deals {Damage} damage to a single target.";
        Cost = 2;
        Range = 3;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Smite.png");
        Type = CardType.Combat;
    }
    
    public override bool OnPlay(Player player, Enemy target, World world)
    {
        target.ReceiveDamage(player, Damage);
        
        return true;
    }
}