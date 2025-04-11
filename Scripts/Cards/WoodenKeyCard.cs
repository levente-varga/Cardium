using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class WoodenKeyCard : InteractableTargetingCard {
    public override void _Ready() {
        DisplayName = "Wooden Key";
        Description = "Unlocks a door.";
        Range = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/WoodenKey.png");
        Type = CardType.Utility;
        
        base._Ready();
    }

    public override bool OnPlay(Player player, Interactable target, World world) {
        switch (target) {
            case Door door:
                door.OnInteract(player, world.Camera);
                break;
            default: return false;
        }

        return true;
    }
}