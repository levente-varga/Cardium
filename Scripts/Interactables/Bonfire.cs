using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Bonfire : Interactable {
  public enum BonfireState {
    Unlit,
    Lit,
    Extinguished,
  }

  public BonfireState State { get; private set; } = BonfireState.Unlit;
  public bool Extinguishable = true;

  public override void _Ready() {
    base._Ready();

    SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/Bonfire.png"));
  }

  public override void OnNudge(Player player, World world) {
    OnInteract(player, world);
    base.OnNudge(player, world);
  }

  public override void OnInteract(Player player, World world) {
    if (State == BonfireState.Extinguished) return;

    base.OnInteract(player, world);

    if (State == BonfireState.Lit) {
      SpawnFloatingLabel("Rested", color: Global.White);
      var healAmount = player.MaxHealth - player.Health;
      if (healAmount > 0) player.Heal(healAmount);
      world.InventoryMenu.Open();
      if (!Extinguishable) return;
      SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/ExtinguishedBonfire.png"), "extinguished");
      State = BonfireState.Extinguished;
      return;
    }

    State = BonfireState.Lit;
    Interacted = true;
    Statistics.BonfiresLit++;

    world.Camera.Shake(25);
    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Bonfire.png"), 4, 12);
    SpawnFallingLabel("Lit!");
  }
}