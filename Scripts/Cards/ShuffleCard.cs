using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class ShuffleCard : PlayerTargetingCard {
  public ShuffleCard() {
    Name = "Shuffle";
    Rarity = CardRarity.Rare;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shuffle.png");
    Type = CardType.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Puts cards from hand into deck, shuffles it, then draws the same amount of cards.";
  }

  public override bool OnPlay(Player player, World world) {
    List<Card> cards = new();

    foreach (var card in player.Hand.GetCards()) {
      if (card == this) continue;
      if (!player.Hand.Remove(card, false)) {
        continue;
      }

      cards.Add(card);
    }

    foreach (var card in cards) {
      player.Deck.Add(card);
    }

    player.Deck.Deck.Shuffle();

    player.Hand.DrawCards(cards.Count, false);

    return true;
  }
}