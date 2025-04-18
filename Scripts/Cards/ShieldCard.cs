using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class ShieldCard : PlayerTargetingCard {
    public int ExtraArmor = 2;
    public int Duration = 3;
    
    public ShieldCard() {
        Name = "Shield";
        Description = $"Raises Armor by {Highlight($"{ExtraArmor}")} for {Highlight($"{Duration}")} turns.";
        Rarity = CardRarity.Common;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shield.png");
        Type = CardType.Combat;
    }
    
    public override bool OnPlay(Player player) {
        // TODO: Implement
        
        return true;
    }
}