using System;
using Godot;

namespace Cardium.Scripts.Labels;

public partial class DisappearingLabel : LabelWithShadow {
  protected readonly ulong SpawnTime = Time.GetTicksMsec();
  public float? LifetimeMillis;
  private const float DefaultLifetimeMillis = 1200f;
  private float _progress;

  public override void _Process(double delta) {
    _progress = (Time.GetTicksMsec() - SpawnTime) / (LifetimeMillis ?? DefaultLifetimeMillis);

    if (_progress >= 1) {
      QueueFree();
      QueueFreeLabels();
      return;
    }

    Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Mathf.Pow(1 - _progress, 0.5f));

    base._Process(delta);
  }
}