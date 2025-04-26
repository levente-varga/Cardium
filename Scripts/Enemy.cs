namespace Cardium.Scripts;

public partial class Enemy : Entity {
  protected Path Path = new();

  protected bool PlayerInVision;
  protected int LastSeenPlayerDistance;

  public delegate void OnDeathDelegate(Enemy enemy);

  public event OnDeathDelegate? OnDeathEvent;

  protected int BaseCombatVision = 3;
  public int TempCombatVision { private get; set; }
  public int CombatVision => BaseCombatVision + TempCombatVision;


  public override void _Ready() {
    base._Ready();
    AddChild(Path);

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
      if (Global.Debug) SpawnFloatingLabel("[Debug] Unreachable", color: Global.Magenta, fontSize: 20);
    }
    else if (LastSeenPlayerDistance <= BaseRange) {
      Nudge(VectorToDirection(player.Position - Position));
      player.ReceiveDamage(this, BaseDamage);
      return;
    }

    var path = world.GetPathBetween(Position, player.Position);
    if (path is { Count: > 1 }) {
      Move(path[0], world);
    }
    else {
      if (Global.Debug) SpawnFloatingLabel("[Debug] Unable to move", color: Global.Magenta, fontSize: 20);
    }

    base.TakeTurn(player, world);
  }

  protected override void OnDeath(Entity source) {
    base.OnDeath(source);

    OnDeathEvent?.Invoke(this);
  }
}