using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Bonfire : Interactable
{
    public override void _Ready()
    {
        base._Ready();

        SetAnimation(GD.Load<Texture2D>("res://Assets/TileSets/Fire animated.png"), 4, 12);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnInteract(Entity source)
    {
        base.OnInteract(source);
    }
}