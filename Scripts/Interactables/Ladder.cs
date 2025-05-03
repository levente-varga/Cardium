using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Ladder : Interactable {
  private int _lastPlayerTurnsLived = -2;

  public override void _Ready() {
    base._Ready();

    SetStillFrame("idle", ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Ladder.png"));
  }

  public override void OnNudge(Player player, World world) {
    base.OnNudge(player, world);

    if (player.TurnsLived - _lastPlayerTurnsLived == 1) {
      player.SaveCards();
      Data.LastRunFinished = true;
      Data.Save();

      Data.LoadLobbyData();
      GetTree().ReloadCurrentScene();
      return;
    }

    Blink();

    _lastPlayerTurnsLived = player.TurnsLived;

    world.Camera.Shake(10);
    SpawnFloatingLabel("Leave dungeon?");
  }
}