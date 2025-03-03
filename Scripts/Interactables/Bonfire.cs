using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Bonfire : Interactable
{
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
        
        if (Interacted)
        {
            SpawnFloatingLabel("Rested", color: Global.White);
            if (source.Health == source.MaxHealth) return;
            var healAmount = source.MaxHealth - source.Health;
            source.Heal(healAmount);
            SpawnFallingLabel(healAmount.ToString(), color: Global.Green);
            return;
        }
        
        Interacted = true;
        
        camera.Shake(25);
        SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Bonfire.png"), 4, 12);
        SpawnFallingLabel("Restored!");
    }
}