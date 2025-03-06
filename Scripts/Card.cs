using System;
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

	private bool _dragging;
	
	private Tween _hoverTween;
	
	private Node2D _base;
	private Node2D _body;
	private Sprite2D _sprite;
	private Button _hitbox;

	public override void _Ready()
	{
		ZIndex = 1;
		
		_sprite = new Sprite2D();
		_sprite.Centered = true;
		_sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Card.png");
		_sprite.Scale = new Vector2(Global.CardScale, Global.CardScale);
		
		_hitbox = new Button();
		_hitbox.Flat = true;
		_hitbox.MouseEntered += OnMouseEntered;
		_hitbox.MouseExited += OnMouseExited;
		_hitbox.Pressed += OnMousePressed;
		_hitbox.ButtonDown += OnMouseDown;
		_hitbox.ButtonUp += OnMouseUp;
		_hitbox.Size = _sprite.Texture.GetSize() * Global.CardScale;
		_hitbox.Position = -_sprite.Texture.GetSize() * Global.CardScale / 2;
		_hitbox.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
		_hitbox.AddThemeStyleboxOverride("pressed", new StyleBoxEmpty());
		_hitbox.AddThemeStyleboxOverride("hover", new StyleBoxEmpty());
		_hitbox.AddThemeStyleboxOverride("disabled", new StyleBoxEmpty());
		_hitbox.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
		_hitbox.FocusMode = Control.FocusModeEnum.None;
		
		_body = new Node2D();
		_body.AddChild(_hitbox);
		_body.AddChild(_sprite);
		
		_base = new Node2D();
		_base.AddChild(_body);
		AddChild(_base);
	}

	public override void _Process(double delta)
	{
		if (_dragging)
		{
			_body.GlobalPosition = _body.GlobalPosition.Lerp(GetGlobalMousePosition(), Global.LerpWeight * (float)delta);
			_body.GlobalRotation = Mathf.Lerp(_body.GlobalRotation, 0, (float)delta);
		}
		else
		{
			_body.Position = _body.Position.Lerp(Vector2.Zero, Global.LerpWeight * (float)delta);
			_body.Rotation = Mathf.Lerp(_body.Rotation, 0, Global.LerpWeight * (float)delta);
		}
	}

	public virtual void OnPlay(Player player) {}
	public virtual void OnPlay(Player player, Entity target) {}
	public virtual void OnPlay(Player player, Vector2I position, World world) {}
	public virtual void OnDiscard(Player player) {}
	public virtual void OnDrawn(Player player) {}
	public virtual void OnDestroy(Player player) {}

	private void ResetHoverTween()
	{
		if (_hoverTween == null) return;
		_hoverTween.Kill();
		_hoverTween.Dispose();
	}

	private void PlayHoverAnimation()
	{
		ResetHoverTween();	
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_base, "position", new Vector2(0, -100), 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	private void PlayUnhoverAnimation()
	{
		ResetHoverTween();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_base, "position", Vector2.Zero, 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}

	public void OnMouseEntered() {
		if (_dragging) return;
		PlayHoverAnimation();
	}

	public void OnMouseExited() {
		if (_dragging) return;
		PlayUnhoverAnimation();
	}
	
	public void OnMousePressed() {
		
	}
	
	public void OnMouseDown() {
		_dragging = true;
		ZIndex = 2;
	}
	
	public void OnMouseUp() {
		PlayUnhoverAnimation();
		_dragging = false;
		ZIndex = 1;
	}
}