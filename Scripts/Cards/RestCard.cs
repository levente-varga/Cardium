using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class RestCard : PlayerTargetingCard {
  public RestCard() {
    Name = "Rest";
    Rarity = CardRarity.Epic;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shuffle.png");
    Type = CardType.Utility;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Shuffles the discard pile into the deck.";
  }

  public override bool OnPlay(Player player) {
    player.ReloadDeck();

    return true;
  }
}