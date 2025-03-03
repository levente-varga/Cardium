using System;
using Godot;

namespace Cardium.Scripts;

public partial class HealthBar : Polygon2D
{
	private float _smoothHealth;
	
	private float _health = 1;
	public int Health
	{
		get => (int)_health;
		set
		{
			var previousHealth = Health;
			_health = Math.Max(0, value);
			if (Health > MaxHealth)
			{
				_health = MaxHealth;
			}
			if (Health < previousHealth)
			{
				SpawnFloatingLabel((previousHealth - Health).ToString());
			}
		}
	}
	
	private float _maxHealth = 1;
	public int MaxHealth
	{
		get => (int)_maxHealth;
		set
		{
			_maxHealth = Math.Max(1, value);
			if (Health > MaxHealth)
			{
				Health = MaxHealth;
			}
		}
	}
	
	private const float VerticalGap = 2;
	private const float Thickness = 2;
	private const float HorizontalMargin = 2;
	private const float Width = 16;
	
	private static float ActualWidth => Width - 2 * HorizontalMargin;
	private float SmoothWidth => ActualWidth * _smoothHealth;
	
	public override void _Ready()
	{
		Name = "HealthBar";
		Color = Global.Red;
		Visible = true;
		ZIndex = 10;
		_smoothHealth = _health / _maxHealth; 
		UpdatePolygon();
	}
	
	private void UpdatePolygon()
	{
		Polygon = new Vector2[]
		{
			new (HorizontalMargin, -VerticalGap),
			new (HorizontalMargin, -VerticalGap - Thickness),
			new (HorizontalMargin + SmoothWidth, -VerticalGap - Thickness),
			new (HorizontalMargin + SmoothWidth, -VerticalGap),
		};
	}

	public override void _Process(double delta)
	{
		_smoothHealth = Mathf.Lerp(_smoothHealth, _health / _maxHealth, Global.LerpWeight * (float) delta);
		
		UpdatePolygon();
	}

	private void SpawnFloatingLabel(string text)
	{
		Labels.FallingLabel label = new()
		{
			Text = text,
			Position = new Vector2(GlobalPosition.X + (HorizontalMargin + ActualWidth) * Global.Scale, GlobalPosition.Y),
			Color = Global.Red,
		};
		GetTree().Root.AddChild(label);
	}
}