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
	private bool _shaking;
	
	private Tween _hoverTween;
	
	private Node2D _base;
	private Node2D _body;
	private Sprite2D _sprite;
	private Sprite2D _art;
	private Sprite2D _artBackground;
	private Button _hitbox;
	private Control _descriptionArea;
	
	private Vector2 _mouseDownPosition;
	
	public delegate void OnDragStartDelegate(Card card);
	public event OnDragStartDelegate OnDragStartEvent;
	
	public delegate void OnDragEndDelegate(Card card, Vector2 mousePosition);
	public event OnDragEndDelegate OnDragEndEvent;
	
	public delegate void OnDragDelegate(Card card, Vector2 mousePosition);
	public event OnDragDelegate OnDragEvent;

	
	private float ShakeIntensity = 5f; // Max shake offset
	private float ShakeDecay = 5f; // How fast the shake fades
	private Vector2 _originalPosition;
	private float _shakeAmount = 5f;
	

	public override void _Ready()
	{
		ZIndex = 1;
		
		_sprite = new Sprite2D();
		_sprite.Centered = true;
		_sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Combat card.png");
		_sprite.Scale = new Vector2(Global.CardScale, Global.CardScale);
		
		_art = new Sprite2D();
		_art.Centered = true;
		_art.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Hurl.png");
		_art.Scale = new Vector2(Global.CardScale, Global.CardScale);
		_art.Position = new Vector2(0, -12) * Global.CardScale;
		
		_artBackground = new Sprite2D();
		_artBackground.Centered = true;
		_artBackground.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Art background.png");
		_artBackground.Scale = new Vector2(Global.CardScale, Global.CardScale);
		_artBackground.Position = new Vector2(0, -12) * Global.CardScale;
		
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

		_descriptionArea = new Control();
		_descriptionArea.Position = new Vector2(3, 30);
		_descriptionArea.Size = new Vector2(32, 21);
		
		_body = new Node2D();
		_body.AddChild(_hitbox);
		_body.AddChild(_artBackground);
		_body.AddChild(_art);
		_body.AddChild(_sprite);
		_body.AddChild(_descriptionArea);
		
		_base = new Node2D();
		_base.AddChild(_body);
		AddChild(_base);
	}

	public override void _Process(double delta)
	{
		if (_shaking)
		{
			_body.GlobalPosition += new Vector2(
				(float)GD.RandRange(-_shakeAmount, _shakeAmount),
				(float)GD.RandRange(-_shakeAmount, _shakeAmount)
			);

			// Gradually reduce shake intensity
			//_shakeAmount = Mathf.Max(0, _shakeAmount - (ShakeDecay * (float)delta));
		}
		if (_dragging)
		{
			_body.GlobalPosition = _body.GlobalPosition.Lerp(GetGlobalMousePosition(), Global.LerpWeight * (float)delta);
			_body.GlobalRotation = Mathf.Lerp(_body.GlobalRotation, 0, (float)delta);
			OnDragEvent?.Invoke(this, GetViewport().GetMousePosition());
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

	public virtual void OnEnterPlayArea()
	{
		_shaking = true;	
	}
	
	public virtual void OnExitPlayArea()
	{
		_shaking = false;
	}

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
	
	private void OnMouseEntered() {
		if (_dragging) return;
		PlayHoverAnimation();
	}

	private void OnMouseExited() {
		if (_dragging) return;
		PlayUnhoverAnimation();
	}
	
	private void OnMousePressed() {
		
	}
	
	private void OnMouseDown() {
		_mouseDownPosition = GetViewport().GetMousePosition();
		_dragging = true;
		ZIndex = 2;
		OnDragStartEvent?.Invoke(this);
	}
	
	private void OnMouseUp() {
		PlayUnhoverAnimation();
		_dragging = false;
		ZIndex = 1;
		OnDragEndEvent?.Invoke(this, GetViewport().GetMousePosition());
	}
}