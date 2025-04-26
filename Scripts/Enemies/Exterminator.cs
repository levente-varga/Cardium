using Godot;

namespace Cardium.Scripts.Enemies;

public partial class Exterminator : Enemy {
  enum ExterminatorState {
    Aggressive,
    Defensive,
  }
  
  private ExterminatorState _state = ExterminatorState.Aggressive;

  private int _turnsInCurrentState;
  
  public override void _Ready() {
    base._Ready();

    SetAnimation("defense", GD.Load<Texture2D>("res://Assets/Animations/ExterminatorDefense.png"), 4, 24, false, false);
    SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Exterminator.png"), 8, 12);
    Name = "Exterminator";
    Description = "The final boss.";
    MaxHealth = 100;
    Health = MaxHealth;
    BaseVision = 3;
    BaseCombatVision = 5;
    BaseArmor = 5;
    BaseDamage = 5;
    BaseRange = 1;
    BaseLuck = 10;
    
    OnDamagedEvent += OnDamaged;
  }

  private void OnDamaged(Entity entity, int damage) {
    if (_state == ExterminatorState.Aggressive && _turnsInCurrentState >= 3) {
      _state = ExterminatorState.Defensive;
      _turnsInCurrentState = 0;
      Play("defense");
    }
  }

  protected override void TakeTurn(Player player, World world) {
    UpdateValue(player, world);
    ResetTemporaryStats();
    Heal(1);

    if (!PlayerInVision) return;

    _turnsInCurrentState++;

    if (_state == ExterminatorState.Aggressive) {
      if (LastSeenPlayerDistance == -1) {
        // Unreachable, but might still be able to get closer
        SpawnDebugFloatingLabel("[Debug] Unreachable");
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
        SpawnDebugFloatingLabel("[Debug] Unable to move");
      }
    }

    if (_state == ExterminatorState.Defensive) {
      TempArmor = BaseArmor;
      Heal(1);
      if (_turnsInCurrentState >= 4) {
        _state = ExterminatorState.Aggressive;
        _turnsInCurrentState = 0;
        PlayBackwards("defense");
        AnimationFinished += SwitchToIdleAnimation;
      }
    }
  }

  private void SwitchToIdleAnimation() {
    Play("idle");
    AnimationFinished -= SwitchToIdleAnimation;
  }
}