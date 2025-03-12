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
	public Texture2D Art { get; protected set; }

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
	private RichTextLabel _description;
	private Label _nameLabel;
	
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
		_sprite.Centered = false;
		_sprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Combat card.png");
		_sprite.Scale = new Vector2(Global.CardScale, Global.CardScale);
		
		_art = new Sprite2D();
		_art.Centered = true;
		_art.Texture = Art;
		_art.Scale = new Vector2(Global.CardScale, Global.CardScale);
		_art.Position = new Vector2(19, 15) * Global.CardScale;
		
		_artBackground = new Sprite2D();
		_artBackground.Centered = true;
		_artBackground.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cards/Art background.png");
		_artBackground.Scale = new Vector2(Global.CardScale, Global.CardScale);
		_artBackground.Position = new Vector2(19, 15) * Global.CardScale;
		
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
		_hitbox.MouseFilter = Control.MouseFilterEnum.Stop;
		
		_description = new RichTextLabel();
		_description.Text = Description;
		_description.BbcodeEnabled = true;
		_description.MouseFilter = Control.MouseFilterEnum.Ignore;
		
		_descriptionArea = new Control();
		_descriptionArea.Position = new Vector2(3, 33) * Global.CardScale;
		_descriptionArea.Size = new Vector2(32, 18) * Global.CardScale;
		_description.MouseFilter = Control.MouseFilterEnum.Ignore;
		_descriptionArea.AddChild(_description);
		
		_nameLabel = new Label();
		_nameLabel.Text = Name;
		_nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_nameLabel.VerticalAlignment = VerticalAlignment.Center;
		_nameLabel.Position = new Vector2(7, 28) * Global.CardScale;
		_nameLabel.Size = new Vector2(24, 4) * Global.CardScale;
		var font = GD.Load<FontFile>("res://Assets/Fonts/alagard.ttf");
		_nameLabel.AddThemeFontOverride("font", font);
		_nameLabel.AddThemeFontSizeOverride("font_size", 20);
		
		_body = new Node2D();
		_body.AddChild(_hitbox);
		_hitbox.AddChild(_artBackground);
		_hitbox.AddChild(_art);
		_hitbox.AddChild(_sprite);
		//_hitbox.AddChild(_descriptionArea);
		_hitbox.AddChild(_nameLabel);
		
		SetupCostMarker();
		
		_base = new Node2D();
		_base.AddChild(_body);
		AddChild(_base);
	}

	private void SetupCostMarker()
	{
		for (var i = 0; i < Cost; i++)
		{
			var costMarker = new Sprite2D();
			costMarker.Texture = GD.Load<Texture2D>(i == Cost - 1 ? "res://Assets/Sprites/Cards/EnergyEnd.png" : "res://Assets/Sprites/Cards/EnergyDot.png");
			costMarker.Centered = false;
			costMarker.Scale = Vector2.One * Global.CardScale;
			costMarker.Position = new Vector2(-16 + i * 2, -25) * Global.CardScale;
			_body.AddChild(costMarker);
		}
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