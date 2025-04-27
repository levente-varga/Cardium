using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Enemy : Entity {
  protected Path Path = new();

  private int _level;
  public int Level {
    get => _level;
    init => _level = Mathf.Clamp(value, 0, MaxLevel);
  }
  protected virtual int MaxLevel => 0;

  protected bool PlayerInVision;
  protected int LastSeenPlayerDistance;
  
  protected int BaseCombatVision = 3;
  public int TempCombatVision { private get; set; }
  public int CombatVision => BaseCombatVision + TempCombatVision;
  
  public override void _Ready() {
    base._Ready();
    AddChild(Path);
    
    Inventory.AddAll(GenerateLoot());

    HealthBar.Visible = false;
    BaseCombatVision = BaseVision + 3;
  }

  public override void _Process(double delta) {
    Path.Visible = true;
    base._Process(delta);
  }

  protected void UpdateValue(Player player, World world) {
    if (!PlayerInVision && Utils.ManhattanDistanceBetween(player.Position, Position) > Vision) return;

    LastSeenPlayerDistance = world.GetDistanceBetween(Position, player.Position);

    if (!PlayerInVision) {
      if (LastSeenPlayerDistance != -1 && LastSeenPlayerDistance <= Vision) {
        PlayerInVision = true;
        HealthBar.Visible = Data.ShowHealth;
      }
    }
    else {
      if (LastSeenPlayerDistance != -1 && LastSeenPlayerDistance > CombatVision) {
        PlayerInVision = false;
        HealthBar.Visible = false;
      }
    }
  }

  protected override void TakeTurn(Player player, World world) {
    UpdateValue(player, world);
    if (!PlayerInVision) return;

    if (TurnsLived % 2 == 0) return;

    Path.SetPath(world.GetPointPathBetween(Position, player.Position));

    if (LastSeenPlayerDistance == -1) {
      // Unreachable, but might still be able to get closer
      SpawnDebugFloatingLabel("[Debug] Unreachable");
    }
    else if (LastSeenPlayerDistance <= BaseRange) {
      Nudge(VectorToDirection(player.Position - Position));
      player.ReceiveDamage(this, BaseDamage, world);
      return;
    }

    var path = world.GetPathBetween(Position, player.Position);
    if (path is { Count: > 1 }) {
      Move(path[0], world);
    }
    else {
      SpawnDebugFloatingLabel("[Debug] Unable to move");
    }

    base.TakeTurn(player, world);
  }

  protected virtual List<Card> GenerateLoot() => new List<Card>();
}