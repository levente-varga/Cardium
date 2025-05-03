using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class ShuffleCard : PlayerTargetingCard {
  public ShuffleCard() {
    Name = "Shuffle";
    Rarity = Rarities.Rare;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shuffle.png");
    Type = Types.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Returns all cards from hand to deck, shuffles it, then draws a new hand.";
  }

  public override bool OnPlay(Player player, World world) {
    List<Card> cards = new();

    foreach (var card in player.Hand.Cards) {
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