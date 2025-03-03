using System;
using Godot;

namespace Cardium.Scripts;

public partial class EnergyBar : Polygon2D
{
	private float _smoothEnergy;
	
	private float _energy = 1;
	public int Energy
	{
		get => (int)_energy;
		set
		{
			var previousHealth = Energy;
			_energy = Math.Max(0, value);
			if (Energy > MaxEnergy)
			{
				Energy = MaxEnergy;
			}
			if (Energy < previousHealth)
			{
				SpawnFloatingLabel((previousHealth - Energy).ToString());
			}
		}
	}
	
	private float _maxEnergy = 1;
	public int MaxEnergy
	{
		get => (int)_maxEnergy;
		set
		{
			_maxEnergy = Math.Max(1, value);
			if (Energy > MaxEnergy)
			{
				Energy = MaxEnergy;
			}
		}
	}
	
	private float _gap = 0;
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
	private float SmoothWidth => ActualWidth * _smoothEnergy;
	
	public override void _Ready()
	{
		Name = "EnergyBar";
		Color = Global.Purple;
		Visible = true;
		ZIndex = 10;
		_smoothEnergy = _energy / _maxEnergy; 
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
		};
	}

	public override void _Process(double delta)
	{
		_smoothEnergy = Mathf.Lerp(_smoothEnergy, _energy / _maxEnergy, Global.LerpWeight * (float) delta);
		
		UpdatePolygon();
	}
	
	public void SpawnFloatingLabel(string text)
	{
		Labels.FallingLabel label = new()
		{
			Text = text,
			Position = new Vector2(GlobalPosition.X + (HorizontalMargin + ActualWidth) * Global.Scale, GlobalPosition.Y),
			Color = Global.Purple,
		};
		GetTree().Root.AddChild(label);
	}
}