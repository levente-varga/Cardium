using Cardium.Scripts.Cards;
using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Spider : Enemy {
  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Spider.png"), 8, 12);
    Name = "Spider";
    MaxHealth = 7;
    Health = MaxHealth;
    BaseVision = 4;
    BaseCombatVision = 5;
    BaseArmor = 0;
    BaseDamage = 2;
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