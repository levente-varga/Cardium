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
			_health = Math.Max(0, value);
			if (Health > MaxHealth)
			{
				Health = MaxHealth;
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
	
	private float _gap = 2;
	[Export] public float Gap
	{
		get => _gap;
		set
		{
			_gap = value;
			UpdatePolygon();
		}
	}
	
	private float _thickness = 2;
	[Export] public float Thickness 
	{
		get => _thickness;
		set
		{
			_thickness = value;
			UpdatePolygon();
		}
	}
	
	private float _horizontalMargin = 2;
	[Export] public float HorizontalMargin
	{
		get => _horizontalMargin;
		set
		{
			_horizontalMargin = value;
			UpdatePolygon();
		}	
	}
	
	private float _width = 16;
	[Export] public float Width 
	{
		get => _width;
		set
		{
			_width = value;
			UpdatePolygon();
		}
	}
	
	private float ActualWidth => Width - 2 * HorizontalMargin;
	private float SmoothWidth => ActualWidth * _smoothHealth;
	
	public override void _Ready()
	{
		Name = "HealthBar";
		Visible = true;
		ZIndex = 10;
		_smoothHealth = _health / _maxHealth; 
		UpdatePolygon();
	}
	
	private void UpdatePolygon()
	{
		Polygon = new Vector2[]
		{
			new (HorizontalMargin, -Gap),
			new (HorizontalMargin, -Gap - Thickness),
			new (HorizontalMargin + SmoothWidth, -Gap - Thickness),
			new (HorizontalMargin + SmoothWidth, -Gap),
			new (HorizontalMargin, -Gap)
		};
	}

	public override void _Process(double delta)
	{
		_smoothHealth = Mathf.Lerp(_smoothHealth, _health / _maxHealth, 0.1f);
		
		Polygon[3] = new Vector2(HorizontalMargin + SmoothWidth, -Gap);
		Polygon[2] = new Vector2(HorizontalMargin + SmoothWidth, -Gap - Thickness);
	}
}