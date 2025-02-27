using Godot;

namespace Cardium.Scripts;

public partial class Card : Node2D
{
	public enum Type
	{
		Combat,
		Action,
	}
	
	private AnimationPlayer _animationPlayer;
	private Node2D _body;
	private Sprite2D _sprite;
	private Control _hitbox;
	
	private CardBehavior _behavior;
	

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

	// public abstract void OnPlay(Player player);
	// public abstract void OnDiscard(Player player);
	// public abstract void OnDrawn(Player player);
	// public abstract void OnDestroy(Player player);
	

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