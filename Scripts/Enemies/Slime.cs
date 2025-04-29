using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Slime : Enemy {
  protected override int MaxLevel => 4;
  
  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Slime.png"), 8, 12);
    Name = "Slime";
    MaxHealth = new List<int> { 5, 7, 9, 12, 15 }[Level];
    Health = MaxHealth;
    BaseVision = 2;
    BaseArmor = new List<int> { 0, 1, 2, 3, 4 }[Level];
    BaseDamage = new List<int> { 1, 2, 3, 4, 5 }[Level];
    BaseLuck = 0f;
    BaseRange = 1;
    Description = "A slime enemy.";
  }
  
  protected override List<Card> GenerateLoot() {
    List<Card> loot = new();
    Random random = new();
      
    var indexCount = random.Next(1, 2 + Level);
    for (var i = 0; i < indexCount; i++) {
      loot.Add(
        random.Next(126) switch {
          < 80 => new HealCard(),
          < 100 => new HurlCard(),
          < 110 => new ShuffleCard(),
          < 120 => new WoodenKeyCard(),
          < 125 => new GoldenKeyCard(),
          < 126 => new GuideCard(),
          _ => new HealCard(),
        }
      );
    }

    return loot;
  }
}