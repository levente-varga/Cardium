using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
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
    
    Inventory.AddAll(GenerateLoot);

    StatusBar.Visible = false;
    BaseCombatVision = BaseVision + 3;
  }

  public override void _Process(double delta) {
    Path.Visible = true;
    base._Process(delta);
  }

  protected void UpdateValue(Player player, World world, bool aggro = false) {
    if (!aggro && !PlayerInVision && Utils.ManhattanDistanceBetween(player.Position, Position) > Vision) return;

    LastSeenPlayerDistance = world.GetDistanceBetween(Position, player.Position);

    if (aggro || !PlayerInVision) {
      if (aggro || LastSeenPlayerDistance != -1 && LastSeenPlayerDistance <= Vision) {
        PlayerInVision = true;
      }
    }
    else {
      if (LastSeenPlayerDistance != -1 && LastSeenPlayerDistance > CombatVision) {
        PlayerInVision = false;
      }
    }

    StatusBar.Visible = Data.ShowHealth && PlayerInVision;
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

  protected virtual List<Card> GenerateLoot => new ();

  protected override void OnDamaged(Entity source, int damage, World world) {
    Statistics.TotalDamage += damage;
    
    base.OnDamaged(source, damage, world);
  }

  public override void ReceiveDamage(Entity source, int damage, World world) {
    base.ReceiveDamage(source, damage, world);

    if (source is Player player) UpdateValue(player, world, true);
  }
}