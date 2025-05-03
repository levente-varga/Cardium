using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
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
    BaseArmor = new List<int> { 0, 0, 1, 3, 6 }[Level];
    BaseDamage = new List<int> { 1, 2, 3, 4, 5 }[Level];
    BaseLuck = 0f;
    BaseRange = 1;
    Description = "A slime enemy.";
  }
  
  protected override List<Card> GenerateLoot => Utils.GenerateLoot(
    Global.Random.Next(1, 2 + Level), 
    new Dictionary<Type, int> {
      { typeof(HealCard), 60 },
      { typeof(ShieldCard), 20 },
      { typeof(HurlCard), 20 },
      { typeof(SmiteCard), 20 },
      { typeof(ShuffleCard), 5 },
      { typeof(WoodenKeyCard), 5 },
      { typeof(GoldenKeyCard), 5 },
      { typeof(GuideCard), 1 },
    }
  );
}