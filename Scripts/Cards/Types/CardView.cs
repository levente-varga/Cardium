using System;
using Godot;

namespace Cardium.Scripts.Cards.Types;

public partial class CardView : Node2D {
	public enum CardState {
		Idle,
		Selected,
		Dragging,
		Playing,
		Played,
		Discarded,
	}
	
	[Export] private Node2D _hoverBase = null!;
	[Export] private Node2D _base = null!;
	[Export] private Sprite2D _art = null!;
	[Export] private Button _hitbox = null!;
	[Export] private RichTextLabel _descriptionLabel = null!;
	[Export] private Label _nameLabel = null!;
	[Export] private Line2D _debugFrame = null!;
	
	public bool Enabled = true;
	
	private bool _dragging;
	private bool _hoverable;
	
	private Tween? _hoverTween;
	private Tween? _scaleTween;

	public Card Card { get; private set; } = null!;
	
	private CardState _state;
	public bool OverPlayArea { get; private set; }
	public bool OverDiscardArea { get; private set; }
	
	private Vector2 _mouseDownPosition;
	
	public delegate void OnDragStartDelegate(CardView cardView);
	public event OnDragStartDelegate? OnDragStartEvent;
	
	public delegate void OnDragEndDelegate(CardView cardView, Vector2 mousePosition);
	public event OnDragEndDelegate? OnDragEndEvent;
	
	public delegate void OnDragDelegate(CardView cardView, Vector2 mousePosition);
	public event OnDragDelegate? OnDragEvent;
	
	public delegate void OnMouseEnteredDelegate(CardView cardView);
	public event OnMouseEnteredDelegate? OnMouseEnteredEvent;
	
	public delegate void OnMouseExitedDelegate(CardView cardView);
	public event OnMouseExitedDelegate? OnMouseExitedEvent;

	
	private float _shakeIntensity = 5f; // Max shake offset
	private float _shakeDecay = 5f; // How fast the shake fades
	private Vector2 _originalPosition;
	private float _shakeAmount = 5f;

	public void Init(Card card, bool hoverable = true) {
		Card = card;
		_hoverable = hoverable;
	}

	private Color GetStateColor() {
		return _state switch {
			CardState.Idle => new Color("CCCCCC"),
			CardState.Selected => new Color("77FF77"),
			CardState.Dragging => new Color("FFCC55"),
			CardState.Playing => new Color("FF7777"),
			CardState.Played => new Color("7777FF"),
			CardState.Discarded => new Color("444444"),
			_ => new Color("FF00FF"),
		};
	}
	
	public override void _Ready() {
		_art.Texture = Card.Art;
		
		_hitbox.MouseEntered += OnMouseEntered;
		_hitbox.MouseExited += OnMouseExited;
		_hitbox.ButtonDown += OnMouseDown;
		_hitbox.ButtonUp += OnMouseUp;

		_descriptionLabel.Text = $"[center]{Card.Description}[/center]";
		
		_nameLabel.Text = Card.Name;
		
		//SetupLevelMarker();
	}

	/*
	private void SetupLevelMarker() {
		for (var i = 0; i < Card.Level; i++) {
			var costMarker = new Sprite2D();
			costMarker.Texture = GD.Load<Texture2D>(i == Card.Level - 1 ? "res://Assets/Sprites/Cards/EnergyEnd.png" : "res://Assets/Sprites/Cards/EnergyDot.png");
			costMarker.Centered = false;
			costMarker.Scale = Vector2.One * Global.CardScale;
			costMarker.Position = new Vector2(-16 + i * 2, -25) * Global.CardScale;
			_body.AddChild(costMarker);
		}
	}
	*/

	public override void _Process(double delta) {
		_debugFrame.DefaultColor = GetStateColor();
		_debugFrame.Visible = Global.Debug;
		
		if (_dragging) {
			_base.GlobalPosition = _base.GlobalPosition.Lerp(GetGlobalMousePosition(), Global.LerpWeight * (float)delta);
			_base.GlobalRotation = Mathf.Lerp(_base.GlobalRotation, 0, (float)delta);
			OnDragEvent?.Invoke(this, GetViewport().GetMousePosition());
		}
		else {
			if (_state == CardState.Playing) {
				_base.GlobalRotation = Mathf.Lerp(_base.GlobalRotation, 0, (float)delta);
			}
			else {
				_base.Position = _base.Position.Lerp(Vector2.Zero, Global.LerpWeight * (float)delta);
				_base.Rotation = Mathf.Lerp(_base.Rotation, 0, Global.LerpWeight * (float)delta);
			}
		}
	}

	public void OnEnterPlayArea() {
		PlayScaleAnimation(1.2f);
		OverPlayArea = true;
	}
	
	public void OnExitPlayArea() {
		PlayResetAnimation();
		_state = _dragging ? CardState.Dragging : CardState.Idle;
		OverPlayArea = false;
	}
	
	public void OnEnterDiscardArea() {
		PlayScaleAnimation(0.75f);
		OverDiscardArea = true;
	}
	
	public void OnExitDiscardArea() {
		PlayResetAnimation();
		_state = _dragging ? CardState.Dragging : CardState.Idle;
		OverDiscardArea = false;
	}
	
	public void OnEnterPlayingMode() {
		PlayMoveToPlayingPositionAnimation();
		_state = CardState.Playing;
		_dragging = false;
	}
	
	public void OnExitPlayingMode() {
		_state = CardState.Idle;
		PlayResetAnimation();
	}

	private void ResetHoverTween() => ResetTween(_hoverTween);
	private void ResetScaleTween() => ResetTween(_scaleTween);
	private static void ResetTween(Tween? tween) {
		if (tween == null) return;
		tween.Kill();
		tween.Dispose();
	}
	
	private void PlayScaleAnimation(float scale) {
		ResetScaleTween();
		_scaleTween = CreateTween();
		_scaleTween.TweenProperty(_base, "scale", new Vector2(scale, scale), 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	private void PlayHoverAnimation() {
		ResetHoverTween();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_hoverBase, "position", new Vector2(0, -100), 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	private void PlayResetAnimation() {
		ResetHoverTween();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_hoverBase, "position", Vector2.Zero, 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		ResetScaleTween();
		_scaleTween = CreateTween();
		_scaleTween.TweenProperty(_base, "scale", Vector2.One, 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	private void PlayMoveToPlayingPositionAnimation() {
		ResetHoverTween();
		var camera = GetViewport().GetCamera2D();
		if (camera == null) return;
		_hoverBase.Position = Vector2.Zero;
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_base, "global_position", camera.GlobalPosition - new Vector2(800, 0), 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	private void OnMouseEntered() {
		if (!Enabled || _dragging) return;
		_state = CardState.Selected;
		OnMouseEnteredEvent?.Invoke(this);
		if (_hoverable) PlayHoverAnimation();
	}

	private void OnMouseExited() {
		if (!Enabled || _dragging) return;
		_state = CardState.Idle;
		OnMouseExitedEvent?.Invoke(this);
		if (_hoverable) PlayResetAnimation();
	}
	
	private void OnMouseDown() {
		if (!Enabled) return;
		
		_mouseDownPosition = GetViewport().GetMousePosition();
		_dragging = true;
		_state = CardState.Dragging;
		ZIndex = 11;
		PlayResetAnimation();
		OnDragStartEvent?.Invoke(this);
	}
	
	private void OnMouseUp() {
		if (!Enabled) return;
		
		PlayResetAnimation();
		_dragging = false;
		_state = CardState.Idle;
		ZIndex = 0;
		OnDragEndEvent?.Invoke(this, GetViewport().GetMousePosition());
	}
}