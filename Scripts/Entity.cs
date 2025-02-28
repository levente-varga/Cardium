using Godot;

namespace Cardium.Scripts;

public partial class Entity : Sprite2D
{
    public int Health { get; protected set; } = 5;
    public int Energy { get; protected set; } = 5;

    public int MaxHealth;
    public int MaxEnergy;
    public int Armor;
    public int Damage;
    public int Luck;
    public float Vision;
    public float Range;
    public string Description;
    
    public delegate void OnMoveDelegate();
    public event OnMoveDelegate OnMoveEvent;
    
    public new Vector2I Position { get; protected set; }
    private Vector2I _previousPosition;
    
    public override void _Ready()
    {
        Position = Vector2I.Zero;
        Name = "Entity";
    }

    public override void _Process(double delta)
    {
        base.Position = base.Position.Lerp(Position * Global.TileSize, 0.1f);
        
        if (_previousPosition != Position)
        {
            OnMoveEvent?.Invoke();
            _previousPosition = Position;
        }
    }
    
    public virtual void OnTurn(Player player)
    {
        
    }

    public virtual void OnDamaged(Player player, int damage)
    {
        
    }

    public virtual void OnTargeted(Player player)
    {
        
    }
    
    public virtual void OnDeath(Player player)
    {
        
    }
}