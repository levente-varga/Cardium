using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Voidling : Enemy {
  private bool Stealth { set; get; } = true;

  public override void _Ready() {
    base._Ready();

    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Voidling.png"), 8, 12);

    Name = "Voidling";
    Description = "Untargetable until it attacks.";
    MaxHealth = 3;
    Health = MaxHealth;
    BaseVision = 5;
    BaseCombatVision = 9;
    BaseArmor = 0;
    BaseDamage = 4;
    BaseRange = 1;
    
    SetModulate();
  }

  protected override void TakeTurn(Player player, World world) {
    Stealth = Utils.ManhattanDistanceBetween(player.Position, Position) > 1;
    SetModulate();
    
    base.TakeTurn(player, world);
    
    Stealth = Utils.ManhattanDistanceBetween(player.Position, Position) > 1;
    SetModulate();
  }

  private void SetModulate() {
    Modulate = new Color(1, 1, 1, Stealth ? 0.25f : 1);
  }
}