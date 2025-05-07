using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Cards;

public sealed class SmiteCard : EnemyTargetingCard {
  private int Damage => new List<int> { 5, 9, 16, 30 }[Level];
  public override int Range => new List<int> { 3, 3, 4, 5 }[Level];

  public SmiteCard() {
    Name = "Smite";
    Rarity = Rarities.Common;
    MaxLevel = 3;
    Art = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Smite.png");
    Type = Types.Combat;
    UpdateDescription();
  }

  protected sealed override void UpdateDescription() {
    Description = $"Deals {Highlight($"{Damage}")} damage to a single target.";
  }

  public override bool OnPlay(Player player, Enemy target, World world) {
    target.ReceiveDamage(player, Damage, world);

    return true;
  }
}