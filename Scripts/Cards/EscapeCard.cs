using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class EscapeCard : PlayerTargetingCard {
  public EscapeCard() {
    Name = "Escape";
    Rarity = CardRarity.Legendary;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shuffle.png");
    Type = CardType.Utility;
    Unstable = true;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Teleports back to the hub, keeping all cards. {Highlight("Unstable")}";
  }

  public override bool OnPlay(Player player, World world) {
    world.QueueEscape();

    return true;
  }
}