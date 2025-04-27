using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts.Cards;

public class WoodenKeyCard : InteractableTargetingCard {
  public WoodenKeyCard() {
    Name = "Wooden Key";
    Rarity = CardRarity.Common;
    Range = 1;
    Unstable = true;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/WoodenKey.png");
    Type = CardType.Utility;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Unlocks a door. {Highlight("Unstable")}";
  }

  public override bool OnPlay(Player player, Interactable target, World world) {
    switch (target) {
      case Door door:
        door.OnInteract(player, world);
        break;
      default: return false;
    }

    return true;
  }
}