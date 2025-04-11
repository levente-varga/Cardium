using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public partial class ShuffleCard : PlayerTargetingCard
{
  public ShuffleCard()
  {
    DisplayName = "Reshuffle";
    Description = "Reshuffles hand into deck, and draws the same amount of cards.";
    Cost = 1;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shuffle.png");
    Type = CardType.Combat;
  }
    
  public override void _Ready()
  {
    base._Ready();
  }
    
  public override bool OnPlay(Player player) {
    List<Card> cards = new();
    
    for (var i = 0; i < player.Hand.Size; i++) {
      var card = player.Hand.Remove(i);
      if (card == null) continue;
      cards.Add(card);
    }

    foreach (var card in cards) {
      player.Deck.Add(card);
    }
    
    player.Deck.Shuffle();
    
    player.Hand.DrawCards(cards.Count);
        
    return true;
  }
}