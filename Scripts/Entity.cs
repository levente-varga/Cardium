using System;
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public partial class Entity : TileAlignedGameObject {
  private int _health;
  public int Health {
    get => _health;
    protected set {
      _health = Mathf.Max(0, value);
      StatusBar.Health = _health;
    }
  }
  
  private int _maxHealth;
  public int MaxHealth {
    get => _maxHealth;
    protected set {
      _maxHealth = Mathf.Max(1, value);
      StatusBar.MaxHealth = _maxHealth;
    }
  }
  
  private int _shield;
  public int Shield {
    get => _shield;
    protected set {
      _shield = Mathf.Max(0, value);
      StatusBar.Shield = _shield;
    }
  }
  
  private int _baseArmor;
  public int BaseArmor {
    get => _baseArmor;
    protected set {
      _baseArmor = Mathf.Max(0, value);
      StatusBar.Armor = _baseArmor;
    }
  }
  
  private int _baseDamage;
  public int BaseDamage {
    get => _baseDamage;
    protected set {
      _baseDamage = Mathf.Max(0, value);
      StatusBar.Attack = _baseDamage;
    }
  }
  
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

  public readonly Pile Inventory = new();
  public readonly List<Buff> Buffs = new();

  public StatusBar StatusBar = null!;

  private Vector2I _previousPosition;
  
  public delegate void OnDamagedDelegate(Entity entity, int damage);

  public event OnDamagedDelegate? OnDamagedEvent;
  
  public delegate void OnDeathDelegate(Entity entity);

  public event OnDeathDelegate? OnDeathEvent;

  public override void _Ready() {
    base._Ready();

    Name = "Entity";
    SetupStatusBar();
  }

  private void SetupStatusBar() {
    var statusBarScene = GD.Load<PackedScene>("res://Scenes/tile_aligned_game_object.tscn");
    StatusBar = statusBarScene.Instantiate<StatusBar>();
    StatusBar.Attack = Damage;
    StatusBar.Armor = Armor;
    StatusBar.Health = Health;
    StatusBar.MaxHealth = MaxHealth;
    StatusBar.Shield = Shield;
    StatusBar.Position = new Vector2(0, -6);
    StatusBar.Scale = Vector2.One / 4f;
    AddChild(StatusBar);
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

  public virtual void ReceiveDamage(Entity source, int damage, World world) {
    if (Invincible || damage < 1) return;

    Blink();
    
    if (Global.Random.Next(100) < Luck) {
      SpawnFallingLabel("Miss!");
      OnDamagedEvent?.Invoke(this, 0);
      return;
    }
    
    OnDamaged(source, damage, world);
  }

  protected virtual void OnDamaged(Entity source, int damage, World world) {
    var remainingDamage = Math.Max(1, damage - BaseArmor);
    
    GD.Print($"Total damage received is {remainingDamage}");
    
    if (Shield > 0) {
      var shieldedDamage = Mathf.Min(remainingDamage, Shield);
      GD.Print($"Shielded: {shieldedDamage}");
      remainingDamage -= shieldedDamage;
      Shield -= shieldedDamage;
    }

    GD.Print($"Remaining after shield: {remainingDamage}");
    
    Health -= remainingDamage;
    if (Health <= 0) OnDeath(source, world);
    
    OnDamagedEvent?.Invoke(this, damage);
  }

  protected virtual void OnDeath(Entity source, World world) {
    Health = 0;
    
    OnDeathEvent?.Invoke(this);
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

    OnHealed(amount);
  }
  
  public void SetShield(int amount) {
    if (amount <= 0) return;

    Shield = amount;
  }

  protected virtual void OnHealed(int amount) {
    Health += amount;
    SpawnFloatingLabel(amount.ToString(), color: Global.Green);
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