using System;
using Godot;

namespace Cardium.Scripts.Cards.Types;

public partial class CardView : Node2D {
  public enum CardState {
    Idle,
    Hovered,
    Dragging,
    Playing,
  }

  public enum HoverAnimationType {
    None,
    Grow,
    Slide,
  }

  [Export] private Node2D _hoverBase = null!;
  [Export] private Node2D _base = null!;
  [Export] private Sprite2D _art = null!;
  [Export] private Button _hitbox = null!;
  [Export] private RichTextLabel _descriptionLabel = null!;
  [Export] private Label _nameLabel = null!;
  [Export] private Sprite2D _frame = null!;
  [Export] private Node2D _levelContainer = null!;
  [Export] private Sprite2D _protection = null!;
  [Export] private Sprite2D _levelPlaceholder = null!;
  public Card Card = null!;

  public bool Enabled = true;
  public bool Draggable = true;

  public HoverAnimationType HoverAnimation = HoverAnimationType.Slide;
  private CardState _state;
  private bool _dragging;

  private Tween? _hoverTween;
  private Tween? _scaleTween;
  private Tween? _rotationTween;

  private Vector2 _mouseDownPosition;

  public delegate void OnDragStartDelegate(CardView cardView);

  public event OnDragStartDelegate? OnDragStartEvent;

  public delegate void OnDragEndDelegate(CardView cardView, Vector2 mousePosition);

  public event OnDragEndDelegate? OnDragEndEvent;

  public delegate void OnDragDelegate(CardView cardView, Vector2 mousePosition);

  public event OnDragDelegate? OnDragEvent;

  public delegate void OnPressedDelegate(CardView view);

  public event OnPressedDelegate? OnPressedEvent;

  public delegate void OnMouseEnteredDelegate(CardView cardView);

  public event OnMouseEnteredDelegate? OnMouseEnteredEvent;

  public delegate void OnMouseExitedDelegate(CardView cardView);

  public event OnMouseExitedDelegate? OnMouseExitedEvent;

  public override void _Ready() {
    _art.Texture = Card.Art;

    _hitbox.MouseEntered += OnMouseEntered;
    _hitbox.MouseExited += OnMouseExited;
    _hitbox.ButtonDown += OnMouseDown;
    _hitbox.ButtonUp += OnMouseUp;
    _hitbox.Pressed += OnPressed;

    _descriptionLabel.Text = $"[center]{Card.Description}[/center]";

    _nameLabel.Text = Card.Name;

    SetupLevelMarker();
    SetupProtectionMarker();
  }

  private void SetupProtectionMarker() {
    _protection.Visible = Card.Protected;
    _levelPlaceholder.Visible = Card.Protected;
    if (Card.Level == 0) {
      _protection.Position += Vector2.Up;
    }
  }
  
  private void SetupLevelMarker() {
    for (var i = 0; i < Card.Level; i++) {
      var marker = new Sprite2D();
      marker.Texture = GD.Load<Texture2D>(i == Card.Level - 1
        ? "res://Assets/Sprites/Cards/LevelMarkerEnd.png"
        : "res://Assets/Sprites/Cards/LevelMarker.png");
      marker.Centered = false;
      marker.Position = new Vector2(-16 + i * 2, -25);
      _levelContainer.AddChild(marker);
    }
  }

  public override void _Process(double delta) {
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
  private void ResetRotationTween() => ResetTween(_rotationTween);

  private static void ResetTween(Tween? tween) {
    if (tween == null) return;
    tween.Kill();
    tween.Dispose();
  }

  public void PlayScaleAnimation(float scale) {
    ResetScaleTween();
    _scaleTween = CreateTween();
    _scaleTween.TweenProperty(_hoverBase, "scale", new Vector2(scale, scale), 0.4f)
      .SetEase(Tween.EaseType.Out)
      .SetTrans(Tween.TransitionType.Expo);
  }

  public void PlayRotationAnimation(float rotation) {
    ResetRotationTween();
    _rotationTween = CreateTween();
    _rotationTween.TweenProperty(_hoverBase, "rotation_degrees", rotation, 0.4f)
      .SetEase(Tween.EaseType.Out)
      .SetTrans(Tween.TransitionType.Expo);
  }

  public void PlayHoverAnimation() {
    switch (HoverAnimation) {
      case HoverAnimationType.None:
        return;
      case HoverAnimationType.Grow:
        PlayScaleAnimation(1.1f);
        PlayRotationAnimation(new Random().Next(-5, 5));
        break;
      case HoverAnimationType.Slide:
        ResetHoverTween();
        _hoverTween = CreateTween();
        _hoverTween.TweenProperty(_hoverBase, "position", new Vector2(0, -100), 0.4f)
          .SetEase(Tween.EaseType.Out)
          .SetTrans(Tween.TransitionType.Expo);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  public void PlayResetAnimation() {
    ResetHoverTween();
    _hoverTween = CreateTween();
    _hoverTween.TweenProperty(_hoverBase, "position", Vector2.Zero, 0.4f)
      .SetEase(Tween.EaseType.Out)
      .SetTrans(Tween.TransitionType.Expo);
    PlayScaleAnimation(1);
    PlayRotationAnimation(0);
  }

  public void PlayMoveToPlayingPositionAnimation() {
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

    _state = CardState.Hovered;
    OnMouseEnteredEvent?.Invoke(this);
    PlayHoverAnimation();
  }

  private void OnMouseExited() {
    if (!Enabled || _dragging) return;

    _state = CardState.Idle;
    OnMouseExitedEvent?.Invoke(this);
    PlayResetAnimation();
  }

  private void OnMouseDown() {
    if (!Enabled || !Draggable) return;

    _mouseDownPosition = GetViewport().GetMousePosition();
    _dragging = true;
    _state = CardState.Dragging;
    ZIndex = 11;
    OnDragStartEvent?.Invoke(this);
  }

  private void OnMouseUp() {
    if (!Enabled || !Draggable) return;

    PlayResetAnimation();
    _dragging = false;
    _state = CardState.Idle;
    ZIndex = 0;
    OnDragEndEvent?.Invoke(this, GetViewport().GetMousePosition());
  }

  private void OnPressed() {
    if (!Enabled) return;

    OnPressedEvent?.Invoke(this);
  }
}