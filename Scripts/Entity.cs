using System;
using System.Collections.Generic;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts;

public partial class Entity : TileAlignedGameObject
{
    public int Health { 
        get => HealthBar.Health;
        protected set => HealthBar.Health = value;
    }
    public int MaxHealth
    {
        get => HealthBar.MaxHealth;
        set => HealthBar.MaxHealth = value;
    }
    
    protected int BaseArmor;
    protected int BaseDamage;
    protected int BaseRange;
    protected float BaseLuck;

    public int TempArmor { private get; set; }
    public int TempDamage { private get; set; }
    public int TempRange { private get; set; }
    public float TempLuck { private get; set; }
    
    public int TurnsLived { get; private set; }

    public int Armor => BaseArmor + TempArmor;
    public int Damage => BaseDamage + TempDamage;
    public int Range => BaseRange + TempRange;
    public float Luck => BaseLuck + TempLuck;

    public string Description;
    
    public List<Card> Inventory = new();
    public List<Buff> Buffs = new();
    
    public HealthBar HealthBar;
    
    private Vector2I _previousPosition;
    
    public override void _Ready()
    {
        base._Ready();
        
        Position = Vector2I.Zero;
        Name = "Entity";
        SetupHealthBar();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    
    private void SetupHealthBar()
    {
        HealthBar = new HealthBar();
        AddChild(HealthBar);
        HealthBar.MaxHealth = MaxHealth;
        HealthBar.Health = Health;
        HealthBar.Visible = true;
    }

    protected virtual void TakeTurn(Player player, World world) { }

    public void OnTakeTurn(Player player, World world)
    {
        TurnsLived++;
        GD.Print(Name + "'s turn started.");
        ResetTemporaryStats();
        UpdateBuffsOnStartOfTurn();
        TakeTurn(player, world);
        UpdateBuffsOnEndOfTurn();
        GD.Print(Name + "'s turn ended.");
    }

    public virtual void ReceiveDamage(Entity source, int damage)
    {
        Blink();
        
        // TODO: Implement dodge based on luck
        
        Health -= Math.Max(1, damage - BaseArmor);
        if (Health <= 0) OnDeath(source);
    }
    
    protected virtual void OnDeath(Entity source)
    {
        Health = 0;
    }

    public bool InRange(Vector2I position)
    {
        return ManhattanDistanceTo(position) <= BaseRange;
    }
    
    protected void Move(Direction direction, World world)
    {
        var newPosition = Position + DirectionToVector(direction);
        if (world.IsEmpty(newPosition)) Move(newPosition, world);
        else
        {
            Nudge(direction);
        }
    }
    
    protected void Move(Vector2I newPosition, World world, bool useEnergy = true)
    {
        if (!world.IsEmpty(newPosition)) return;
        
        Position = newPosition;
    }
    
    public void Heal(int amount)
    {
        var actualAmount = Math.Min(MaxHealth - Health, amount);
        if (actualAmount <= 0) return;
        
        Health += actualAmount;
        SpawnFloatingLabel(actualAmount.ToString(), color: Global.Green);
    }

    public void AddBuff(Buff buff)
    {
        buff.Target = this;
        Buffs.Add(buff);
    }

    public void RemoveBuff(Buff buff)
    {
        Buffs.Remove(buff);
    }

    private void UpdateBuffsOnStartOfTurn()
    {
        foreach (var buff in Buffs)
        {
            buff.OnStartOfTurn();
        }
    }
    
    private void UpdateBuffsOnEndOfTurn()
    {
        foreach (var buff in Buffs)
        {
            buff.OnEndOfTurn();
        }
    }
    
    private void ResetTemporaryStats()
    {
        TempArmor = 0;
        TempDamage = 0;
        TempRange = 0;
        TempLuck = 0;
    }
}