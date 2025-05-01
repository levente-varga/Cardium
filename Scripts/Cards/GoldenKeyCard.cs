using Cardium.Scripts.Cards.Types;
using Cardium.Scripts.Interactables;
using Godot;

namespace Cardium.Scripts.Cards;

public sealed class GoldenKeyCard : InteractableTargetingCard {
  public GoldenKeyCard() {
    Name = "Golden Key";
    Rarity = Rarities.Legendary;
    Unstable = true;
    Range = 1;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/GoldenKey.png");
    Type = Types.Utility;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Unlocks a chest. {Highlight("Unstable")}";
  }

  public override bool OnPlay(Player player, Interactable target, World world) {
    switch (target) {
      case Chest chest:
        chest.OnInteract(player, world);
        break;
      default: return false;
    }

    return true;
  }
}