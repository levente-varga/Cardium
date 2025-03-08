namespace Cardium.Scripts;

public class Buff
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public int Turns { get; protected set; }
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
        RemainingTurns = Turns;
    }
    
    protected virtual void OnRemove()
    {
        
    }
}