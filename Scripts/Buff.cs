namespace Cardium.Scripts;

public class Buff
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public int DurationTurns { get; protected set; }
    private int RemainingTurns { get; set; }

    private Entity _target;

    public Entity Target
    {
        get => _target; 
        set
        {
            _target = value;
            OnApply();
        }
    }

    public Buff(string name, string description, int durationTurns, Entity target)
    {
        Name = name;
        Description = description;
        DurationTurns = durationTurns;
        RemainingTurns = durationTurns;
        Target = target;
    }
    
    public virtual void OnStartOfTurn()
    {
        
    }
    
    public virtual void OnEndOfTurn()
    {
        RemainingTurns--;
        if (RemainingTurns == 0)
        {
            OnRemove();
            Target.Buffs.Remove(this);
        }
    }
    
    protected virtual void OnApply()
    {
        
    }
    
    protected virtual void OnRemove()
    {
        
    }
}