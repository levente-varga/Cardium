using System;
using Godot;

namespace Cardium.Scripts;

public partial class Camera : Camera2D {
  [Export] public Node2D Target = null!;
  [Export] public CanvasLayer Canvas = null!;
  [Export] public Control WelcomePanel = null!;
  [Export] public Button HideWelcomePanelButton = null!;

  private float _shake;

  private Vector2 TargetCenter => Target.GlobalPosition + Global.GlobalTileSize / 2;

  private bool _focus;

  public bool Focus {
    get => _focus;
    set {
      _focus = value;
      //SetFocus(_focus);
    }
  }

  public Rect2 ViewRect {
    get {
      var viewportSize = GetViewportRect().Size;
      var worldViewSize = viewportSize * Zoom;
      var topLeft = GlobalPosition - (worldViewSize / 2);
      return new Rect2(topLeft, worldViewSize);
    }
  }

  public override void _Ready() {
    WelcomePanel.Visible = Data.ShowWelcomePanel;
    HideWelcomePanelButton.Pressed += HideWelcomePanel;
    Canvas.Visible = Data.Level == Level.Lobby;
  }

  private void HideWelcomePanel() {
    Data.ShowWelcomePanel = false;
    WelcomePanel.Visible = false;
  }

  public override void _Process(double delta) {
    Zoom = Zoom.Lerp(_focus ? Vector2.One / 0.7f : Vector2.One, Global.LerpWeight * (float)delta);
    Scale = Zoom.Inverse();

    if (_shake > 0) Shake(delta);

    FollowTarget(delta);
  }

  private void FollowTarget(double delta) {
    var offset = Position - TargetCenter;
    Position = GlobalPosition.Lerp(TargetCenter, Global.LerpWeight * (float)delta);

    Canvas.Offset = -offset;
  }

  private void Shake(double delta) {
    var offset = new Vector2(
      (float)(Global.Random.NextDouble() * 2 - 1),
      (float)(Global.Random.NextDouble() * 2 - 1)
    );
    if (offset == Vector2.Zero) offset = Vector2.One;
    offset = offset.Normalized() * _shake;

    Position += offset;

    _shake = Mathf.Lerp(_shake, 0f, Global.LerpWeight * (float)delta);
  }

  private void SetFocus(bool focus) {
    var tween = CreateTween();

    tween.TweenProperty(this, "zoom", focus ? new Vector2(1.5f, 1.5f) : new Vector2(1f, 1f), 0.5f);
    tween.SetEase(Tween.EaseType.OutIn);
    tween.Play();
  }

  public void Shake(float amount) {
    _shake = amount;
  }

  public void JumpToTarget() {
    Position = TargetCenter;
  }
}