using System.Collections.Generic;
using Cardium.Scripts.Cards;
using Cardium.Scripts.Cards.Types;
using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Chest : Interactable
{
    public readonly List<Card> Content = new ();
    
    public override void _Ready()
    {
        base._Ready();

        Content.Add(new HurlCard());

        SetAnimation("open", ResourceLoader.Load<Texture2D>("res://Assets/Animations/Chest.png"), 6, 12, false, false);
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
        camera.Shake(10);
        Play("open");
        SpawnFallingLabel("Opened!");
        
        player.PickUpCards(Content); 
    }
}