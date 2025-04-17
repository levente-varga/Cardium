using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class HealCard : PlayerTargetingCard {
    public int HealAmount = 3;
    
    public HealCard() {
        Name = "Heal";
        Description = $"Heals for {HealAmount} missing health.";
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Heal.png");
        Type = CardType.Combat;
    }
    
    public override bool OnPlay(Player player)
    {
        player.Heal(HealAmount);
        
        return true;
    }
}