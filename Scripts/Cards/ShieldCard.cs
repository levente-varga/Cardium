using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public class ShieldCard : PlayerTargetingCard {
  private int Amount => new List<int> { 2, 4, 7, 10 }[Level];


  public ShieldCard() {
    Name = "Shield";
    Rarity = Rarities.Rare;
    MaxLevel = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Shield.png");
    Type = Types.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Sets a shield of {Highlight($"{Amount}")}.";
  }

  public override bool OnPlay(Player player, World world) {
    player.SetShield(Amount);

    return true;
  }
}