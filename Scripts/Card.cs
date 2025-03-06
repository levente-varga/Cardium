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

	private Tween _hoverTween;
	
	private Node2D _body;
	private Sprite2D _sprite;
	private Control _hitbox;

	public override void _Ready()
	{
		_sprite = new Sprite2D();
		_sprite.Centered = true;
		_sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Card.png");
		_sprite.Scale = new Vector2(6, 6);
		
		_hitbox = new Control();
		_hitbox.MouseEntered += OnMouseEntered;
		_hitbox.MouseExited += OnMouseExited;
		_hitbox.Size = _sprite.Texture.GetSize() * 6;
		_hitbox.Position = -_sprite.Texture.GetSize() * 6 / 2;
		
		_body = new Node2D();
		_body.AddChild(_hitbox);
		_body.AddChild(_sprite);
		AddChild(_body);
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
		if (_hoverTween != null)
		{
			_hoverTween.Kill();
			_hoverTween.Dispose();
		}
		
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_body, "position", new Vector2(0, -100), 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}

	public void OnMouseExited() {
		if (_hoverTween != null)
		{
			_hoverTween.Kill();
			_hoverTween.Dispose();
		}
		
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_body, "position", Vector2.Zero, 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	public void OnMousePressed() {
		//_animationPlayer.Play("Play");
	}
}