using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts.Cards;

public class GoldenKeyCard : InteractableTargetingCard {
    public GoldenKeyCard() {
        Name = "Key";
        Description = "Unlocks a chest.";
        Range = 1;
        Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/GoldenKey.png");
        Type = CardType.Utility;
    }

    public override bool OnPlay(Player player, Interactable target, World world) {
        switch (target) {
            case Chest chest:
                chest.OnInteract(player, world.Camera);
                break;
            default: return false;
        }

        return true;
    }
}