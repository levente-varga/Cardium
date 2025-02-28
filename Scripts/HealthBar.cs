using Godot;

namespace Cardium.Scripts;

public partial class HealthBar : Polygon2D
{
	private float _smoothHealth;
	
	public float Health;
	
	private float _gap = 8;
	[Export] public float Gap
	{
		get => _gap;
		set
		{
			_gap = value;
			UpdatePolygon();
		}
	}
	
	private float _thickness = 8;
	[Export] public float Thickness 
	{
		get => _thickness;
		set
		{
			_thickness = value;
			UpdatePolygon();
		}
	}
	
	private float _horizontalMargin = 8;
	[Export] public float HorizontalMargin
	{
		get => _horizontalMargin;
		set
		{
			_horizontalMargin = value;
			UpdatePolygon();
		}	
	}
	
	private float _width = 64;
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
		_smoothHealth = Health;
		UpdatePolygon();
	}
	
	private void UpdatePolygon()
	{
		Polygon = new Vector2[]
		{
			new (HorizontalMargin, -Gap),
			new (HorizontalMargin + SmoothWidth, -Gap),
			new (HorizontalMargin + SmoothWidth, -Gap - Thickness),
			new (HorizontalMargin, -Gap - Thickness),
			new (HorizontalMargin, -Gap)
		};
	}

	public override void _Process(double delta)
	{
		_smoothHealth = Mathf.Lerp(_smoothHealth, Health, 0.1f);
		
		Polygon[1] = new Vector2(HorizontalMargin + SmoothWidth, -Gap);
		Polygon[2] = new Vector2(HorizontalMargin + SmoothWidth, -Gap - Thickness);
	}
}