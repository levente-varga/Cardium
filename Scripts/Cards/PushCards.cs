using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class PushCard : DirectionalCard {
    public int Distance = 2;
    public int Damage = 1;
    
    public PushCard() {
        Name = "Push";
        Description = $"Pushes an enemy away {Highlight($"{Distance}")} tiles. Deals {Highlight($"{Damage}")} damage to it and enemies in the path.";
        Rarity = CardRarity.Common;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Push.png");
        Type = CardType.Combat;
    }
    
    public override bool OnPlay(Player player, Direction direction, World world) {
        // TODO: Implement
        
        return true;
    }
}