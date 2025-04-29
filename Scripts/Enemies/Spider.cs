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
    BaseArmor = new List<int> { 0, 1, 2, 3, 5 }[Level];
    BaseDamage = new List<int> { 2, 3, 4, 5, 6 }[Level];
    BaseLuck = 0f;
    BaseRange = 1;
    Description = "A spider enemy.";
  }
  
  protected override List<Card> GenerateLoot => Utils.GenerateLoot(
    Global.Random.Next(1, 3 + Level), 
    new Dictionary<Type, int> {
      { typeof(HurlCard), 40 },
      { typeof(SmiteCard), 40 },
      { typeof(HolyCard), 15 },
      { typeof(ChainCard), 15 },
      { typeof(WoodenKeyCard), 10 },
      { typeof(GoldenKeyCard), 5 },
      { typeof(GuideCard), 1 },
    }
  );
}