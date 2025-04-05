using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Ladder : Interactable
{
    public override void _Ready()
    {
        base._Ready();

        SetStillFrame(ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Ladder.png"));
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
        SpawnFallingLabel("Climbed!");
    }
}