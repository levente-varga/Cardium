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

    public override void OnInteract(Player player, Camera camera)
    {
        base.OnInteract(player, camera);
        
        if (Interacted)
        {
            SpawnFloatingLabel("Rested", color: Global.White);
            if (player.Health == player.MaxHealth) return;
            var healAmount = player.MaxHealth - player.Health;
            player.Heal(healAmount);
            return;
        }
        
        Interacted = true;
        
        camera.Shake(25);
        SetAnimation("idle", GD.Load<Texture2D>("res://Assets/Animations/Bonfire.png"), 4, 12);
        SpawnFallingLabel("Lit!");
    }
}