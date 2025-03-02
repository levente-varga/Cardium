using System;
using Godot;

namespace Cardium.Scripts;

public partial class Camera : Camera2D
{
	[Export] public Node2D Target;

	private bool _zoom;
	private float _shake;
	private Random _random = new Random();

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
		
		if (_shake > 0)
		{
			// Apply random offsets to the camera's position for the shake effect
			var offsetX = (float)(_random.NextDouble() * 2 - 1) * _shake; // Random X offset between -shakeAmount and shakeAmount
			var offsetY = (float)(_random.NextDouble() * 2 - 1) * _shake; // Random Y offset between -shakeAmount and shakeAmount

			// Apply the offset to the camera's position
			Position += new Vector2(offsetX, offsetY);

			// Gradually reduce the shake intensity (smooth shake reduction)
			_shake = Mathf.Lerp(_shake, 0f, Global.LerpWeight * (float)delta); // You can adjust the reduction speed
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true, Keycode: Key.Space })
		{
			_zoom = !_zoom;
		}
	}
	
	public void Shake(float amount)
	{
		_shake = amount;
	}
}