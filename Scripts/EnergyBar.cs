using System;
using System.Collections.Generic;
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
	
	private const float VerticalGap = 1;
	private const float DotSeparation = 1;
	private const float Thickness = 1;
	private const float HorizontalMargin = 2;

	private readonly List<EnergyDot> _dots = new();
	
	public override void _Ready()
	{
		ReactToMaxEnergyChange();
		ReactToEnergyChange();
	}
	
	private EnergyDot CreateDot(int index)
	{
		var offset = index * (DotSeparation + Thickness) + HorizontalMargin;
		var dot = new EnergyDot();
		dot.Position = new Vector2(offset, -VerticalGap);
		return dot;
	}
	
	private void ReactToEnergyChange()
	{
		for (var i = 0; i < MaxEnergy; i++)
		{
			if (i < Energy)
			{
				if (_dots[i].IsThrown) _dots[i].CancelThrow();
				continue;
			}
			if (_dots[i].IsThrown) continue;
			
			_dots[i].Throw();
		}
	}

	private void ReactToMaxEnergyChange()
	{
		for (var i = 0; i < Math.Max(MaxEnergy, _dots.Count); i++)
		{
			if (i >= _dots.Count)
			{
				var dot = CreateDot(i);
				AddChild(dot);
				_dots.Add(dot);
			}
			else if (i >= MaxEnergy)
			{
				_dots[i].QueueFree();
				_dots.RemoveAt(i);
			}
		}
	}
	
	public override void _Process(double delta)
	{
		
	}
}