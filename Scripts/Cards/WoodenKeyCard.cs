using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts.Cards;

public sealed class WoodenKeyCard : InteractableTargetingCard {
  public WoodenKeyCard() {
    Name = "Wooden Key";
    Rarity = Rarities.Rare;
    Range = 1;
    Unstable = true;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/WoodenKey.png");
    Type = Types.Utility;
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