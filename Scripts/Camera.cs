using System;
using Godot;

namespace Cardium.Scripts;

public partial class Camera : Camera2D
{
	[Export] public Node2D Target;

	private float _shake;
	private readonly Random _random = new Random();

	private bool _focus;
	public bool Focus
	{
		get => _focus;
		set
		{
			_focus = value;
			//SetFocus(_focus);
		}
	}

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
		
		Zoom = Zoom.Lerp(_focus ? Vector2.One / 0.7f : Vector2.One, Global.LerpWeight * (float) delta);
		Scale = Zoom.Inverse();
		Position = GlobalPosition.Lerp(targetCenter, Global.LerpWeight * (float) delta);
		
		if (_shake > 0)
		{
			// Apply random offsets to the camera's position for the shake effect
			var offset = new Vector2(
				(float)(_random.NextDouble() * 2 - 1),
				(float)(_random.NextDouble() * 2 - 1)
			);
			if (offset == Vector2.Zero) offset = Vector2.One;
			offset = offset.Normalized() * _shake;

			// Apply the offset to the camera's position
			Position += offset;

			// Gradually reduce the shake intensity (smooth shake reduction)
			_shake = Mathf.Lerp(_shake, 0f, Global.LerpWeight * (float)delta); // You can adjust the reduction speed
		}
	}

	private void SetFocus(bool focus)
	{
		var tween = CreateTween();
		
		tween.TweenProperty(this, "zoom", focus ? new Vector2(1.5f, 1.5f) : new Vector2(1f, 1f), 0.5f);
		tween.SetEase(Tween.EaseType.OutIn);
		tween.Play();
	}
	
	public void Shake(float amount)
	{
		_shake = amount;
	}
}