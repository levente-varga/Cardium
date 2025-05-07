using Godot;

namespace Cardium.Scripts.Labels;

public partial class FallingLabel : DisappearingLabel {
  private Vector2 _velocity = new(0, 0);

  public override void _Ready() {
    _velocity = new Vector2(Global.Random.Next(80, 100), Global.Random.Next(-500, -400));

    base._Ready();
  }

  public override void _Process(double delta) {
    _velocity += new Vector2(0, 1200f) * (float)delta;
    Position += _velocity * (float)delta;

    base._Process(delta);
  }
}