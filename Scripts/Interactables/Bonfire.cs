using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Bonfire : Interactable
{
    bool active = false;
    
    public override void _Ready()
    {
        base._Ready();

        SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/Bonfire.png"));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnInteract(Entity source)
    {
        base.OnInteract(source);
        
        if (active) return;
        
        active = true;
        
        SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Bonfire.png"), 4, 12);
        SpawnFloatingLabel("Restored!", color: new Color("ffffff"));
    }
}