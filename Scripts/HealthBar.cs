using System;
using Godot;

namespace Cardium.Scripts;

public partial class HealthBar : Node2D {
  private Label _label = null!;
  private Polygon2D _healthPolygon = null!;
  private Polygon2D _shieldPolygon = null!;

  private float _smoothHealth;
  private float _smoothShield;

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
  
  private float _shield;

  public int Shield {
    get => (int)_shield;
    set {
      GD.Print($"Setting shield to {value}");
      
      var previousShield = Shield;
      var totalChange = value - previousShield;
      _shield = Math.Max(0, value);

      GD.Print($"Shield is now {value}");
      
      if (totalChange < 0) {
        SpawnFloatingLabel(Mathf.Abs(totalChange).ToString());
      }
    }
  }

  private const int VerticalGap = 2;
  private const int Thickness = 2;
  private const int HorizontalMargin = 2;
  private const int Width = 16;

  private static float ActualHealthWidth => Width - 2 * HorizontalMargin;
  private float SmoothHealthWidth => ActualHealthWidth * _smoothHealth;
  
  private float SmoothShieldWidth => _smoothShield * ActualHealthWidth / MaxHealth;

  private Vector2[] _healthPolygonBuffer = new Vector2[4];
  private Vector2[] _shieldPolygonBuffer = new Vector2[4];

  public override void _Ready() {
    Name = "HealthBar";
    
    _smoothHealth = _health / _maxHealth;
    _smoothShield = _shield;

    _healthPolygon = new Polygon2D();
    _healthPolygon.Color = Global.Red;
    _healthPolygon.ZIndex = 10;
    AddChild(_healthPolygon);
    
    _shieldPolygon = new Polygon2D();
    _shieldPolygon.Color = Global.Yellow;
    _shieldPolygon.ZIndex = 10;
    AddChild(_shieldPolygon);

    _label = new Label();
    _label.Scale = Vector2.One / 6f;
    _label.Position = new Vector2(2, -8f);
    AddChild(_label);
    
    UpdatePolygons();
  }

  private void UpdatePolygons() {
    UpdateHealthPolygon();
    UpdateShieldPolygon();
  }
  
  private void UpdateHealthPolygon() {
    _healthPolygonBuffer[0].X = HorizontalMargin;
    _healthPolygonBuffer[0].Y = -VerticalGap;
    _healthPolygonBuffer[1].X = HorizontalMargin;
    _healthPolygonBuffer[1].Y = -VerticalGap - Thickness;
    _healthPolygonBuffer[2].X = HorizontalMargin + SmoothHealthWidth;
    _healthPolygonBuffer[2].Y = -VerticalGap - Thickness;
    _healthPolygonBuffer[3].X = HorizontalMargin + SmoothHealthWidth;
    _healthPolygonBuffer[3].Y = -VerticalGap;

    _healthPolygon.Polygon = _healthPolygonBuffer;
  }
  
  private void UpdateShieldPolygon() {
    _shieldPolygonBuffer[0].X = HorizontalMargin + SmoothHealthWidth;
    _shieldPolygonBuffer[0].Y = -VerticalGap;
    _shieldPolygonBuffer[1].X = HorizontalMargin + SmoothHealthWidth;
    _shieldPolygonBuffer[1].Y = -VerticalGap - Thickness;
    _shieldPolygonBuffer[2].X = HorizontalMargin + SmoothHealthWidth + SmoothShieldWidth;
    _shieldPolygonBuffer[2].Y = -VerticalGap - Thickness;
    _shieldPolygonBuffer[3].X = HorizontalMargin + SmoothHealthWidth + SmoothShieldWidth;
    _shieldPolygonBuffer[3].Y = -VerticalGap;

    _shieldPolygon.Polygon = _shieldPolygonBuffer;
  }

  public override void _Process(double delta) {
    _smoothHealth = Mathf.Lerp(_smoothHealth, _health / _maxHealth, Global.LerpWeight * (float)delta);
    _smoothShield = Mathf.Lerp(_smoothShield, _shield, Global.LerpWeight * (float)delta);

    _label.Text = $"{_health}{(_shield > 0 ? $" + {_shield}" : "")}";

    UpdatePolygons();
  }

  private void SpawnFloatingLabel(string text) {
    Labels.FallingLabel label = new() {
      Text = text,
      Position = new Vector2(GlobalPosition.X + (HorizontalMargin + ActualHealthWidth) * Global.TileScale, GlobalPosition.Y),
      Color = Global.Red,
    };
    GetTree().Root.AddChild(label);
  }
}