using Godot;

namespace Cardium.Scripts;

public partial class Camera : Camera2D
{
	[Export] public Node2D Target;

	private bool _zoom;

	public Rect2 ViewRect
	{
		get
		{
			var viewportSize = GetViewportRect().Size;
			var worldViewSize = viewportSize * Zoom;
			var topLeft = GlobalPosition - (worldViewSize / 2);
			return new Rect2(topLeft, worldViewSize);
		}
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var targetCenter = Target.GlobalPosition + new Vector2(Global.TileSize, Global.TileSize) / 2;
		
		// lerp to player position:
		Position = GlobalPosition.Lerp(targetCenter, Global.LerpWeight * (float) delta);
		Zoom = Zoom.Lerp(_zoom ? Vector2.One / 0.7f : Vector2.One, Global.LerpWeight * (float) delta);
	}

	public override void _Input(InputEvent @event)
	{
		// if space pressed, toggle zoom
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
		{
			_zoom = !_zoom;
		}
	}
}