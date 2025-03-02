namespace Cardium.Scripts;

public partial class Interactable : TileAlignedGameObject
{
    public bool Solid = true;
    public bool Interacted = false;
    
    public virtual void OnInteract(Entity source, Camera camera)
    {
        Blink();
    }
}