using Godot;

namespace Cardium.Scripts.Enemies;

public partial class TargetDummy : Enemy {
  public override void _Ready() {
    base._Ready();

    SetStillFrame("idle", GD.Load<Texture2D>("res://Assets/Sprites/player.png"));
    Name = "Target Dummy";
    MaxHealth = 1000;
    Health = MaxHealth;
    BaseArmor = 0;
    
    StatusBar.Reset();
  }

  protected override void TakeTurn(Player player, World world) {
    Heal(MaxHealth);
  }
}