using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Slime : Enemy {
  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Slime.png"), 8, 12);
    Name = "Slime";
    MaxHealth = 3;
    Health = MaxHealth;
    BaseVision = 2;
    BaseArmor = 0;
    BaseDamage = 1;
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