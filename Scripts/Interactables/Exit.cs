using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Exit : Interactable {
  private int _lastPlayerTurnsLived = -2;

  public override void _Ready() {
    base._Ready();

    SetStillFrame("idle", ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Ladder.png"));
  }

  public override void OnNudge(Player player, World world) {
    base.OnNudge(player, world);

    if (player.TurnsLived - _lastPlayerTurnsLived == 1) {
      GetTree().Quit();
      return;
    }

    Blink();

    _lastPlayerTurnsLived = player.TurnsLived;

    world.Camera.Shake(10);
    SpawnFloatingLabel("Quit game?", height: Global.GlobalTileSize.Y * 3);
  }
}