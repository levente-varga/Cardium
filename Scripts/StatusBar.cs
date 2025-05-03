using System;
using Godot;

namespace Cardium.Scripts;

public partial class StatusBar : Node2D {
	[Export] public Control HealthPart = null!;
	[Export] public Control ArmorPart = null!;
	[Export] public Control AttackPart = null!;
	[Export] public RichTextLabel HealthLabel = null!;
	[Export] public RichTextLabel ArmorLabel = null!;
	[Export] public RichTextLabel AttackLabel = null!;
	[Export] public Polygon2D HealthBar = null!;
	[Export] public Polygon2D ShieldBar = null!;
	
	private float _smoothHealthRatio;
	private float _smoothShieldRatio;
	
	private float SmoothHealthWidth => 56 * _smoothHealthRatio;
	private float SmoothShieldWidth => 56 * _smoothShieldRatio;

	private Vector2[] _healthPolygonBuffer = new Vector2[4];
	private Vector2[] _shieldPolygonBuffer = new Vector2[4];
	
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
			UpdateHealthLabel();
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
  
	private float _shield;
	public int Shield {
		get => (int)_shield;
		set {
			var previousShield = Shield;
			var totalChange = value - previousShield;
			_shield = Math.Max(0, value);
			UpdateHealthLabel();
			if (totalChange < 0) {
				SpawnFloatingLabel(Mathf.Abs(totalChange).ToString());
			}
		}
	}
	
	private int _armor;
	public int Armor {
		get => _armor;
		set {
			_armor = Mathf.Max(0, value);
			UpdateArmorLabel();
		}
	}
	
	private int _extraArmor;
	public int ExtraArmor {
		get => _extraArmor;
		set {
			_extraArmor = Mathf.Max(0, value);
			UpdateArmorLabel();
		}
	}
	
	private int _attack;
	public int Attack {
		get => _attack;
		set {
			_attack = Mathf.Max(0, value);
			UpdateAttackLabel();
		}
	}

	private void UpdateHealthLabel() {
		HealthLabel.Text = $"{Health}{(Shield > 0 ? $"[color={Global.Yellow.ToHtml()}]+{Shield}" : "")}";
	}
	
	private void UpdateArmorLabel() {
		ArmorPart.Visible = Armor > 0;
		ArmorLabel.Text = $"{Armor}{(ExtraArmor > 0 ? $"[color={Global.Yellow.ToHtml()}]+{ExtraArmor}" : "")}";
	}
	
	private void UpdateAttackLabel() {
		AttackPart.Visible = Attack > 0;
		AttackLabel.Text = $"{Attack}";
	}


	public override void _Ready() {
		
	}

	public override void _Process(double delta) {
		_smoothHealthRatio = Mathf.Lerp(_smoothHealthRatio, _health / _maxHealth, Global.LerpWeight * (float)delta);
		_smoothShieldRatio = Mathf.Lerp(_smoothShieldRatio, _shield / _maxHealth, Global.LerpWeight * (float)delta);
		UpdatePolygons();
	}
	
	private void UpdatePolygons() {
		UpdateHealthPolygon();
		UpdateShieldPolygon();
	}
	
	private void UpdateHealthPolygon() {
		_healthPolygonBuffer[0].X = 4;
		_healthPolygonBuffer[0].Y = 0;
		
		_healthPolygonBuffer[1].X = 4 + SmoothHealthWidth;
		_healthPolygonBuffer[1].Y = 0;
		
		_healthPolygonBuffer[2].X = 4 + SmoothHealthWidth;
		_healthPolygonBuffer[2].Y = 4;
		
		_healthPolygonBuffer[3].X = 4;
		_healthPolygonBuffer[3].Y = 4;

		HealthBar.Polygon = _healthPolygonBuffer;
	}
  
	private void UpdateShieldPolygon() {
		_shieldPolygonBuffer[0].X = 4;
		_shieldPolygonBuffer[0].Y = 0;
		
		_shieldPolygonBuffer[1].X = 4 + SmoothShieldWidth;
		_shieldPolygonBuffer[1].Y = 0;
		
		_shieldPolygonBuffer[2].X = 4 + SmoothShieldWidth;
		_shieldPolygonBuffer[2].Y = 4;
		
		_shieldPolygonBuffer[3].X = 4;
		_shieldPolygonBuffer[3].Y = 4;

		ShieldBar.Polygon = _shieldPolygonBuffer;
	}
	
	private void SpawnFloatingLabel(string text) {
		if (!IsInsideTree()) return;
		Labels.FallingLabel label = new() {
			Text = text,
			Position = GlobalPosition + Vector2.Right * Global.GlobalTileSize,
			Color = Global.Red,
		};
		GetTree().Root.AddChild(label);
	}
}