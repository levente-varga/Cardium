using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public int Energy {
        get => EnergyBar.Energy;
        protected set => EnergyBar.Energy = value;
    }
    public int MaxEnergy
    {
        get => EnergyBar.MaxEnergy;
        set => EnergyBar.MaxEnergy = value;
    }
    public int Armor;
    public int Damage;
    public float Luck;
    public int Vision;
    public int CombatVision;
    public int Range;
    public string Description;
    public bool InCombat { get; private set; }
    
    public List<Card> Inventory = new();
    public List<Buff> Buffs = new();
    
    public HealthBar HealthBar;
    public EnergyBar EnergyBar;
    public TurnMarker TurnMarker;
    
    public delegate void OnDeathDelegate(Entity entity);
    public event OnDeathDelegate OnDeathEvent;
    
    public delegate void OnEnterCombatDelegate(Entity entity);
    public event OnEnterCombatDelegate OnEnterCombatEvent;
    
    public delegate void OnLeaveCombatDelegate(Entity entity);
    public event OnLeaveCombatDelegate OnLeaveCombatEvent;
    
    public delegate void OnTurnFinishedDelegate(Entity entity);
    public event OnTurnFinishedDelegate OnTurnFinishedEvent;
    
    private Vector2I _previousPosition;
    
    public override void _Ready()
    {
        base._Ready();
        
        Position = Vector2I.Zero;
        Name = "Entity";
        SetupHealthBar();
        SetupEnergyBar();
        SetupTurnMarker();
        SetInCombatStatus(false);
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
    }

    private void SetupEnergyBar()
    {
        EnergyBar = new EnergyBar();
        AddChild(EnergyBar);
        EnergyBar.MaxEnergy = MaxEnergy;
        EnergyBar.Energy = Energy;
    }
    
    private void SetupTurnMarker()
    {
        TurnMarker = new TurnMarker();
        AddChild(TurnMarker);
        TurnMarker.Visible = false;
    }
    
    public virtual async Task OnTurn(Player player, World world) { }

    public virtual void ReceiveDamage(Entity source, int damage)
    {
        Blink();
        
        // TODO: Implement dodge based on luck
        
        Health -= Math.Min(1, damage - Armor);
        if (Health <= 0) OnDeath(source);
    }

    public virtual void OnTargeted(Entity source) { }

    protected virtual void OnDeath(Entity source)
    {
        Health = 0;
        OnLeaveCombatEvent?.Invoke(this);
        OnDeathEvent?.Invoke(this);
    }

    public bool InRange(Vector2I position)
    {
        return ManhattanDistanceTo(position) <= Range;
    }
    
    public bool InVision(Vector2I position)
    {
        return ManhattanDistanceTo(position) <= Vision;
    }
    
    public bool InCombatVision(Vector2I position)
    {
        return ManhattanDistanceTo(position) <= CombatVision;
    }

    public void OnCombatStart()
    {
        SetInCombatStatus(true);
    }
    
    public void OnCombatEnd()
    {
        SetInCombatStatus(false);
    }
    
    protected void OnTurnFinished()
    {
        TurnMarker.Visible = false;
        OnTurnFinishedEvent?.Invoke(this);
    }

    protected void SetInCombatStatus(bool inCombat)
    {
        InCombat = inCombat;
        if (InCombat)
        {
            HealthBar.Visible = true;
            EnergyBar.Visible = true;
            OnEnterCombatEvent?.Invoke(this);
        }
        else
        {
            HealthBar.Visible = false;
            EnergyBar.Visible = false;
            TurnMarker.Visible = false;
            OnLeaveCombatEvent?.Invoke(this);
        }
    }

    protected async Task Move(Direction direction, World world, bool useEnergy = true)
    {
        var newPosition = Position + DirectionToVector(direction);
        if (world.IsTileEmpty(newPosition)) await Move(newPosition, world, useEnergy);
        else
        {
            if (world.IsTileEnemy(newPosition) && useEnergy) Energy--; 
            Nudge(direction);
        }
    }
    
    protected async Task Move(Vector2I newPosition, World world, bool useEnergy = true)
    {
        GD.Print("Desire to move registered.");
        
        if (!world.IsTileEmpty(newPosition)) return;
        
        if (useEnergy)
        {
            if (Energy <= 0)
            {
                GD.Print("Can't move, no energy left.");
                return;
            }
            Energy--;
        }
        
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
}