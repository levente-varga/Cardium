using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class PushCard : DirectionalCard
{
    public PushCard() {
        Name = "Push";
        Description = "Pushes an enemy away 2 tiles. Deals 1 damage. Enemies in the path also take 1 damage.";
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Push.png");
        Type = CardType.Combat;
    }
    
    public override bool OnPlay(Player player, Direction direction, World world) {
        // TODO: Implement
        
        return true;
    }
}