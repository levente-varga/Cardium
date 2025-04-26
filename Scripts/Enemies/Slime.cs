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

    Inventory.Add(new HealCard());
  }

  public override void _Process(double delta) {
    base._Process(delta);
  }

  protected override void TakeTurn(Player player, World world) {
    base.TakeTurn(player, world);
  }

  public override void ReceiveDamage(Entity source, int damage) {
    base.ReceiveDamage(source, damage);
  }

  protected override void OnDeath(Entity source) {
    base.OnDeath(source);
  }
}