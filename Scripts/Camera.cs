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
	
	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		var targetCenter = Target.GlobalPosition + Global.TileSize / 2;
		
		Position = GlobalPosition.Lerp(targetCenter, Global.LerpWeight * (float) delta);
		Zoom = Zoom.Lerp(_zoom ? Vector2.One / 0.7f : Vector2.One, Global.LerpWeight * (float) delta);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true, Keycode: Key.Space })
		{
			_zoom = !_zoom;
		}
	}
}