using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Chest : Interactable
{
    public override void _Ready()
    {
        base._Ready();

        SetAnimation("open", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Chest.png"), 6, 12, false, false);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnInteract(Entity source)
    {
        base.OnInteract(source);
        
        Play("open");
    }
}