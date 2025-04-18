namespace Cardium.Scripts;

public partial class Interactable : TileAlignedGameObject {
  private bool _solid = true;
  public bool Solid {
    get => _solid;
    protected set {
      _solid = value;
      OnSolidityChangeEvent?.Invoke(this, _solid);
    }
  }
  public bool Interacted = false;

  public delegate void OnSolidityChangeDelegate(Interactable interactable, bool solid);
  public event OnSolidityChangeDelegate? OnSolidityChangeEvent;

  public virtual void OnInteract(Player player, Camera camera) => Blink();
}