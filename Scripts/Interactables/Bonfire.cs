using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Bonfire : Interactable {
  public enum BonfireState {
    Unlit,
    Lit,
    Extinguished,
  }

  public BonfireState State { get; private set; } = BonfireState.Unlit;

  public override void _Ready() {
    base._Ready();

    SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/Bonfire.png"));
  }

  public override void OnInteract(Player player, Camera camera) {
    if (State == BonfireState.Extinguished) return;
    
    base.OnInteract(player, camera);
    
    if (State == BonfireState.Lit) {
      SpawnFloatingLabel("Rested", color: Global.White);
      var healAmount = player.MaxHealth - player.Health;
      if (healAmount > 0) player.Heal(healAmount);
      SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/ExtinguishedBonfire.png"));
      State = BonfireState.Extinguished;
      return;
    }
    
    State = BonfireState.Lit;
    Interacted = true;

    camera.Shake(25);
    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Bonfire.png"), 4, 12);
    SpawnFallingLabel("Lit!");
  }
}