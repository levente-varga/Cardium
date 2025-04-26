using System;
using Godot;

namespace Cardium.Scripts;

public partial class HealthBar : Polygon2D {
	private Label Label = null!;
	
	private float _smoothHealth;
	
	private float _health = 1;
	public int Health {
		get => (int)_health;
		set {
			var previousHealth = Health;
			var totalChange = value - previousHealth;
			_health = Math.Max(0, value);
			if (Health > MaxHealth) {
				_health = MaxHealth;
			}
			if (totalChange < 0) {
				SpawnFloatingLabel(Mathf.Abs(totalChange).ToString());
			}
		}
	}
	
	private float _maxHealth = 1;
	public int MaxHealth {
		get => (int)_maxHealth;
		set {
			_maxHealth = Math.Max(1, value);
			if (Health > MaxHealth) {
				Health = MaxHealth;
			}
		}
	}
	
	private const int VerticalGap = 2;
	private const int Thickness = 2;
	private const int HorizontalMargin = 2;
	private const int Width = 16;
	
	private static float ActualWidth => Width - 2 * HorizontalMargin;
	private float SmoothWidth => ActualWidth * _smoothHealth;
	
	private Vector2[] _polygonBuffer = new Vector2[4];
	
	public override void _Ready() {
		Name = "HealthBar";
		Color = Global.Red;
		Visible = true;
		ZIndex = 10;
		_smoothHealth = _health / _maxHealth; 
		UpdatePolygon();

		Label = new Label();
		Label.Scale = Vector2.One / 6f;
		Label.Position = new Vector2(2, -8f);
		AddChild(Label);
	}
	
	private void UpdatePolygon() {
		_polygonBuffer[0].X = HorizontalMargin;
		_polygonBuffer[0].Y = -VerticalGap;
		_polygonBuffer[1].X = HorizontalMargin;
		_polygonBuffer[1].Y = -VerticalGap - Thickness;
		_polygonBuffer[2].X = HorizontalMargin + SmoothWidth;
		_polygonBuffer[2].Y = -VerticalGap - Thickness;
		_polygonBuffer[3].X = HorizontalMargin + SmoothWidth;
		_polygonBuffer[3].Y = -VerticalGap;
		
		Polygon = _polygonBuffer;
	}

	public override void _Process(double delta) {
		_smoothHealth = Mathf.Lerp(_smoothHealth, _health / _maxHealth, Global.LerpWeight * (float) delta);
		
		Label.Text = $"{_health}";
		
		UpdatePolygon();
	}

	private void SpawnFloatingLabel(string text) {
		Labels.FallingLabel label = new() {
			Text = text,
			Position = new Vector2(GlobalPosition.X + (HorizontalMargin + ActualWidth) * Global.TileScale, GlobalPosition.Y),
			Color = Global.Red,
		};
		GetTree().Root.AddChild(label);
	}
}