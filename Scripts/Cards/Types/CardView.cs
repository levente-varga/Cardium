using Godot;

namespace Cardium.Scripts.Cards.Types;

public partial class CardView : Node2D {
	public enum CardState {
		Idle,
		Selected,
		Dragging,
		Ready,
		Played,
	}
	
	public bool Enabled = true;
	
	private bool _dragging;
	private bool _shaking;
	
	private Tween? _hoverTween;

	public Card Card { get; private set; } = null!;

	[Export] private Node2D _hoverBase = null!;
	[Export] private Node2D _base = null!;
	[Export] private Sprite2D _art = null!;
	[Export] private Button _hitbox = null!;
	[Export] private RichTextLabel _descriptionLabel = null!;
	[Export] private Label _nameLabel = null!;
	[Export] private Line2D _debugFrame = null!;
	
	public CardState State { get; protected set; }
	public bool InPlayArea { get; protected set; }
	
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

	public void Init(Card card) {
		Card = card;
	}
	
	public override void _Ready() {
		_art.Texture = Card.Art;
		
		_hitbox.MouseEntered += OnMouseEntered;
		_hitbox.MouseExited += OnMouseExited;
		_hitbox.Pressed += OnMousePressed;
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
		_debugFrame.DefaultColor = State switch {
			CardState.Selected => Colors.Green,
			CardState.Dragging => Colors.Yellow,
			CardState.Ready => Colors.Orange,
			CardState.Played => Colors.Purple,
			_ => Colors.Gray
		};
		_debugFrame.Visible = Global.Debug;

		if (_shaking) {
			_base.GlobalPosition += new Vector2(
				(float)GD.RandRange(-_shakeAmount, _shakeAmount),
				(float)GD.RandRange(-_shakeAmount, _shakeAmount)
			);

			// Gradually reduce shake intensity
			//_shakeAmount = Mathf.Max(0, _shakeAmount - (ShakeDecay * (float)delta));
		}
		if (_dragging) {
			_base.GlobalPosition = _base.GlobalPosition.Lerp(GetGlobalMousePosition(), Global.LerpWeight * (float)delta);
			_base.GlobalRotation = Mathf.Lerp(_base.GlobalRotation, 0, (float)delta);
			OnDragEvent?.Invoke(this, GetViewport().GetMousePosition());
		}
		else {
			_base.Position = _base.Position.Lerp(Vector2.Zero, Global.LerpWeight * (float)delta);
			_base.Rotation = Mathf.Lerp(_base.Rotation, 0, Global.LerpWeight * (float)delta);
		}
	}

	
	public virtual void OnDiscard(Player player) {}
	public virtual void OnDrawn(Player player) {}
	public virtual void OnDestroy(Player player) {}

	public virtual void OnEnterPlayArea() {
		_shaking = true;
		State = CardState.Ready;
		InPlayArea = true;
	}
	
	public virtual void OnExitPlayArea() {
		_shaking = false;
		State = _dragging ? CardState.Dragging : CardState.Idle;
		InPlayArea = false;
	}

	private void ResetHoverTween() {
		if (_hoverTween == null) return;
		_hoverTween.Kill();
		_hoverTween.Dispose();
	}

	private void PlayHoverAnimation() {
		ResetHoverTween();	
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_hoverBase, "position", new Vector2(0, -100), 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	private void PlayUnhoverAnimation() {
		ResetHoverTween();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_hoverBase, "position", Vector2.Zero, 0.4f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
	}
	
	private void OnMouseEntered() {
		if (!Enabled || _dragging) return;
		State = CardState.Selected;
		OnMouseEnteredEvent?.Invoke(this);
		PlayHoverAnimation();
	}

	private void OnMouseExited() {
		if (!Enabled || _dragging) return;
		State = CardState.Idle;
		OnMouseExitedEvent?.Invoke(this);
		PlayUnhoverAnimation();
	}
	
	private void OnMousePressed() {
		
	}
	
	private void OnMouseDown() {
		if (!Enabled) return;
		
		_mouseDownPosition = GetViewport().GetMousePosition();
		_dragging = true;
		State = CardState.Dragging;
		ZIndex = 1;
		PlayUnhoverAnimation();
		OnDragStartEvent?.Invoke(this);
	}
	
	private void OnMouseUp() {
		if (!Enabled) return;
		
		PlayUnhoverAnimation();
		_dragging = false;
		State = CardState.Idle;
		ZIndex = 0;
		OnDragEndEvent?.Invoke(this, GetViewport().GetMousePosition());
	}
}