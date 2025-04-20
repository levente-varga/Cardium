using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Ladder : Interactable {
  private int _lastPlayerTurnsLived = -2;
  
  public override void _Ready() {
    base._Ready();

    SetStillFrame(ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Ladder.png"));
  }

  public override void OnNudge(Player player, Camera camera) {
    base.OnNudge(player, camera);

    if (player.TurnsLived - _lastPlayerTurnsLived == 1) {
      Data.Level = Level.Lobby;
      
      // TODO: Save cards
      
      GetTree().ReloadCurrentScene();
      return;
    }
    
    _lastPlayerTurnsLived = player.TurnsLived;
    
    camera.Shake(10);
    SpawnFloatingLabel("Leave dungeon?");
  }
}