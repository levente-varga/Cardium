using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class ShuffleCard : PlayerTargetingCard
{
  public ShuffleCard()
  {
    DisplayName = "Shuffle";
    Description = "Puts cards from hand into deck, shuffles it, then draws the same amount of cards.";
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shuffle.png");
    Type = CardType.Combat;
  }
    
  public override void _Ready()
  {
    base._Ready();
  }
    
  public override bool OnPlay(Player player) {
    List<Card> cards = new();
    
    foreach (var card in player.Hand.GetCards()) {
      if (card == this) continue;
      if (player.Hand.Remove(card, false) == null) {
        continue;
      }
      cards.Add(card);
    }
    
    foreach (var card in cards) {
      player.Deck.Add(card);
    }
    
    player.Deck.Shuffle();
    
    player.Hand.DrawCards(cards.Count, false);
        
    return true;
  }
}