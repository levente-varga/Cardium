namespace Cardium.Scripts;

public partial class Interactable : TileAlignedGameObject
{
    public virtual void OnInteract(Entity source, Camera camera)
    {
        Blink();
    }
}