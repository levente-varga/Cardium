using Godot;

namespace Cardium.Scripts;

public partial class Card : Node2D
{
	public enum CardType
	{
		Combat,
		Action,
	}
	
	public enum CardRarity
	{
		Common,
		Rare,
		Epic,
		Legendary
	}
	
	public int Cost { get; protected set; }
	public string Description { get; protected set; }
	public CardType Type { get; protected set; }
	public CardRarity Rarity { get; protected set; }
	public Sprite2D Art { get; protected set; }
	
	private AnimationPlayer _animationPlayer;
	private Node2D _body;
	private Sprite2D _sprite;
	private Control _hitbox;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_body = GetNode<Node2D>("Body");
		_sprite = GetNode<Sprite2D>("Body/Sprite");
		_hitbox = GetNode<Control>("Body/Hitbox");

		SetupHitbox();
	}

	private void SetupHitbox()
	{
		var cardSize = _sprite.Texture.GetSize() * _sprite.Scale;
		_hitbox.Size = cardSize;
		_hitbox.Position = new Vector2(-cardSize.X / 2, -cardSize.Y / 2);

		_hitbox.MouseEntered += OnMouseEntered;
		_hitbox.MouseExited += OnMouseExited;
	}

	public override void _Process(double delta)
	{
		
	}

	public virtual void OnPlay(Player player) {}
	public virtual void OnPlay(Player player, Entity target) {}
	public virtual void OnPlay(Player player, Vector2I position, World world) {}
	public virtual void OnDiscard(Player player) {}
	public virtual void OnDrawn(Player player) {}
	public virtual void OnDestroy(Player player) {}
	

	public void OnMouseEntered() {
		_animationPlayer.Play("Select");
	}

	public void OnMouseExited() {
		_animationPlayer.Play("Deselect");
	}
	
	public void OnMousePressed() {
		//_animationPlayer.Play("Play");
	}
}