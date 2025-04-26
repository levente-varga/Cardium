using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Spider : Enemy {
  protected override int MaxLevel => 2;
  
  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Spider.png"), 8, 12);
    Name = "Spider";
    MaxHealth = new List<int> { 10, 17, 30, }[Level];
    Health = MaxHealth;
    BaseVision = 4;
    BaseCombatVision = 5;
    BaseArmor = new List<int> { 0, 2, 5, }[Level];;
    BaseDamage = new List<int> { 2, 5, 8, }[Level];;
    BaseLuck = 0f;
    BaseRange = 1;
    Description = "A spider enemy.";

    Inventory.Add(new PushCard());
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