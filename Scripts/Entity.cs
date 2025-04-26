using System;
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Entity : TileAlignedGameObject {
  public int Health {
    get => HealthBar.Health;
    protected set => HealthBar.Health = value;
  }

  public int MaxHealth {
    get => HealthBar.MaxHealth;
    set => HealthBar.MaxHealth = value;
  }

  protected int BaseArmor = 0;
  protected int BaseDamage = 1;
  protected int BaseRange = 1;
  protected int BaseVision = 3;
  protected float BaseLuck = 0;

  public int TempArmor { private get; set; }
  public int TempDamage { private get; set; }
  public int TempRange { private get; set; }
  public int TempVision { private get; set; }
  public float TempLuck { private get; set; }

  public int TurnsLived { get; protected set; }

  public int Armor => BaseArmor + TempArmor;
  public int Damage => BaseDamage + TempDamage;
  public int Range => BaseRange + TempRange;
  public int Vision => BaseVision + TempVision;
  public float Luck => BaseLuck + TempLuck;

  protected bool Invincible = false;

  public string Description = "";

  public Pile Inventory = new();
  public List<Buff> Buffs = new();

  public HealthBar HealthBar = new();

  private Vector2I _previousPosition;
  
  public delegate void OnDamagedDelegate(Entity entity, int damage);

  public event OnDamagedDelegate? OnDamagedEvent;

  public override void _Ready() {
    base._Ready();

    Name = "Entity";
    SetupHealthBar();
  }

  public override void _Process(double delta) {
    base._Process(delta);
  }

  private void SetupHealthBar() {
    AddChild(HealthBar);
    HealthBar.MaxHealth = MaxHealth;
    HealthBar.Health = Health;
    HealthBar.Visible = Data.ShowHealth;
  }

  protected virtual void TakeTurn(Player player, World world) {
  }

  public void OnTakeTurn(Player player, World world) {
    TurnsLived++;

    ResetTemporaryStats();
    UpdateBuffsOnStartOfTurn();
    TakeTurn(player, world);
    UpdateBuffsOnEndOfTurn();
  }

  public virtual void ReceiveDamage(Entity source, int damage) {
    if (Invincible || damage < 1) return;

    Blink();
    
    if (new Random().Next(100) < Luck) {
      SpawnFallingLabel("Miss!");
      OnDamagedEvent?.Invoke(this, 0);
      return;
    }

    OnDamagedEvent?.Invoke(this, damage);

    Health -= Math.Max(1, damage - BaseArmor);
    if (Health <= 0) OnDeath(source);
  }

  protected virtual void OnDeath(Entity source) {
    Health = 0;
  }

  public bool InRange(Vector2I position) {
    return ManhattanDistanceTo(position) <= BaseRange;
  }

  protected void Move(Direction direction, World world) {
    var newPosition = Position + DirectionToVector(direction);
    if (world.IsEmpty(newPosition)) Move(newPosition, world);
    else Nudge(direction);
  }

  protected void Move(Vector2I newPosition, World world, bool useEnergy = true) {
    if (!world.IsEmpty(newPosition)) return;

    Position = newPosition;
  }

  public void Heal(int amount) {
    var actualAmount = Math.Min(MaxHealth - Health, amount);
    if (actualAmount <= 0) return;

    Health += actualAmount;
    SpawnFloatingLabel(actualAmount.ToString(), color: Global.Green);
  }

  public void AddBuff(Buff buff) {
    buff.Target = this;
    Buffs.Add(buff);
  }

  public void RemoveBuff(Buff buff) {
    Buffs.Remove(buff);
  }

  private void UpdateBuffsOnStartOfTurn() {
    foreach (var buff in Buffs) {
      buff.OnStartOfTurn();
    }
  }

  private void UpdateBuffsOnEndOfTurn() {
    foreach (var buff in Buffs) {
      buff.OnEndOfTurn();
    }
  }

  protected void ResetTemporaryStats() {
    TempArmor = 0;
    TempDamage = 0;
    TempRange = 0;
    TempLuck = 0;
    TempVision = 0;
  }
}