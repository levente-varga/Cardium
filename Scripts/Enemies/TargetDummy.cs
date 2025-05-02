using Godot;

namespace Cardium.Scripts.Enemies;

public partial class TargetDummy : Enemy {
  public override void _Ready() {
    base._Ready();

    Name = "Target Dummy";
    MaxHealth = 1000;
    Health = MaxHealth;
    BaseArmor = 0;
    SetStillFrame("idle", GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
  }

  protected override void TakeTurn(Player player, World world) {
    Heal(MaxHealth);
  }
}