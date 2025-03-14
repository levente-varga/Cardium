using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Door : Interactable
{
    public override void _Ready()
    {
        base._Ready();

        SetAnimation("open", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Door.png"), 8, 12, false, false);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnInteract(Player player, Camera camera)
    {
        base.OnInteract(player, camera);
        
        if (Interacted) return;
        
        Interacted = true;
        Solid = false;
        camera.Shake(10);
        SpawnFallingLabel("Opened!");
        Play("open");
    }
}