using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Spider : Enemy {
  protected override int MaxLevel => 4;
  
  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Spider.png"), 8, 12);
    Name = "Spider";
    MaxHealth = new List<int> { 10, 13, 17, 23, 30 }[Level];
    Health = MaxHealth;
    BaseVision = 4;
    BaseCombatVision = 5;
    BaseArmor = new List<int> { 0, 1, 2, 3, 4 }[Level];
    BaseDamage = new List<int> { 2, 3, 4, 5, 6 }[Level];
    BaseLuck = 0f;
    BaseRange = 1;
    Description = "A spider enemy.";

    Inventory.Add(new PushCard());
  }
  
  protected override List<Card> GenerateLoot() {
    List<Card> loot = new();
    Random random = new();
      
    var indexCount = random.Next(1, 3 + Level);
    for (var i = 0; i < indexCount; i++) {
      loot.Add(
        random.Next(100) switch {
          < 40 => new SmiteCard(),
          < 55 => new HolyCard(),
          < 70 => new ChainCard(),
          < 90 => new WoodenKeyCard(),
          < 100 => new GoldenKeyCard(),
          _ => new HealCard(),
        }
      );
    }

    return loot;
  }
}