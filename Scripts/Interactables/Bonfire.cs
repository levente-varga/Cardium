using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Bonfire : Interactable
{
    private bool _active = false;
    
    public override void _Ready()
    {
        base._Ready();

        SetStillFrame(GD.Load<Texture2D>("res://Assets/Sprites/Bonfire.png"));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void OnInteract(Entity source, Camera camera)
    {
        base.OnInteract(source, camera);
        
        SpawnFloatingLabel("!", color: new Color("E6482E"), 60);
        
        if (_active) return;
        
        _active = true;
        
        camera.Shake(25);
        SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Bonfire.png"), 4, 12);
        SpawnFallingLabel("Restored!");
    }
}