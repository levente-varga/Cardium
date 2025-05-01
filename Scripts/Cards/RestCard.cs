using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class RestCard : PlayerTargetingCard {
  public RestCard() {
    Name = "Rest";
    Rarity = Rarities.Legendary;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Rest.png");
    Type = Types.Utility;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Shuffles all other cards from the discard pile into the deck.";
  }

  public override bool OnPlay(Player player, World world) {
    player.ReloadDeck(except: new List<Type> { typeof(RestCard) });

    return true;
  }
}