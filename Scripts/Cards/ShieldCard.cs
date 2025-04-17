using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class ShieldCard : PlayerTargetingCard
{
    public ShieldCard() {
        Name = "Shield";
        Description = "Raises Armor by 2 for 3 turns.";
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shield.png");
        Type = CardType.Combat;
    }
    
    public override bool OnPlay(Player player) {
        // TODO: Implement
        
        return true;
    }
}