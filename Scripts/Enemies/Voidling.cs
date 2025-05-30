using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Voidling : Enemy {
  protected override int MaxLevel => 2;
  
  
  private bool Stealth { set; get; } = true;

  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Voidling.png"), 8, 12);

    Name = "Voidling";
    Description = "Untargetable until it attacks.";
    MaxHealth = new List<int> { 5, 7, 9, 11, 13 }[Level];
    Health = MaxHealth;
    BaseVision = 5;
    BaseCombatVision = 7;
    BaseArmor = 0;
    BaseDamage = new List<int> { 3, 4, 5, 6, 7 }[Level];
    BaseRange = 1;

    SetModulate();
    
    StatusBar.Reset();
  }

  protected override void TakeTurn(Player player, World world) {
    Stealth = Utils.ManhattanDistanceBetween(player.Position, Position) > 1;
    Invincible = Stealth;

    SetModulate();

    base.TakeTurn(player, world);

    Stealth = Utils.ManhattanDistanceBetween(player.Position, Position) > 1;
    Invincible = Stealth;

    SetModulate();
  }

  private void SetModulate() {
    Modulate = new Color(1, 1, 1, Stealth ? 0.25f : 1);
  }
  
  protected override List<Card> GenerateLoot => Utils.GenerateLoot(
    Global.Random.Next(1, 2 + Level), 
    new Dictionary<Type, int> {
      { typeof(HolyCard), 40 },
      { typeof(GoldenKeyCard), 20 },
      { typeof(SmiteCard), 20 },
      { typeof(HurlCard), 20 },
      { typeof(TeleportCard), 10 },
      { typeof(GuideCard), 5 },
      { typeof(EscapeCard), 5 },
    }
  );
}