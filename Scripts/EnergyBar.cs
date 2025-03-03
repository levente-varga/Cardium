using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Cardium.Scripts;

public partial class EnergyBar : Node2D
{
	private float _energy = 1;
	public int Energy
	{
		get => (int)_energy;
		set
		{
			_energy = Math.Max(0, value);
			if (Energy > MaxEnergy)
			{
				_energy = MaxEnergy;
			}
			ReactToEnergyChange();
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
			ReactToMaxEnergyChange();
		}
	}
	
	private const float VerticalGap = 0;
	private const float DotSeparation = 1;
	private const float Thickness = 1;
	private const float HorizontalMargin = 2;

	private readonly List<Polygon2D> _polygons = new();
	
	public override void _Ready()
	{
		ReactToMaxEnergyChange();
		ReactToEnergyChange();
	}
	
	private Polygon2D CreatePolygon(int index)
	{
		var offset = index * (DotSeparation + Thickness) + HorizontalMargin;
		var polygon = new Polygon2D();
		polygon.Color = Global.Purple;
		polygon.Name = "EnergyBarSegment";
		polygon.Visible = index < Energy;
		polygon.ZIndex = 10;
		polygon.Polygon = new Vector2[]
		{
			new (offset, -VerticalGap),
			new (offset, -VerticalGap - Thickness),
			new (offset + Thickness, -VerticalGap - Thickness),
			new (offset + Thickness, -VerticalGap),
		};
		return polygon;
	}
	
	private void ReactToEnergyChange()
	{
		for (var i = 0; i < MaxEnergy; i++)
		{
			_polygons[i].Visible = i < Energy;
		}
	}

	private void ReactToMaxEnergyChange()
	{
		for (var i = 0; i < Math.Max(MaxEnergy, _polygons.Count); i++)
		{
			if (i >= _polygons.Count)
			{
				var polygon = CreatePolygon(i);
				AddChild(polygon);
				_polygons.Add(polygon);
			}
			else if (i >= MaxEnergy)
			{
				_polygons[i].QueueFree();
				_polygons.RemoveAt(i);
			}
		}
	}
	
	public override void _Process(double delta)
	{
		
	}
}