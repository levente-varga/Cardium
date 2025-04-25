using Godot;

namespace Cardium.Scripts.Labels;

public partial class LabelWithShadow : Control
{
    private Label _label = new ();
    private Label _shadow = new ();
    
    public string Text = "";
    public Color? Color;
    public int? FontSize = 40;
    
    public override void _Ready()
    {
        var font = GD.Load<FontFile>("res://Assets/Fonts/alagard.ttf");

        ZIndex = 100;
        
        _label = new Label();
        _label.Text = Text;
        _label.HorizontalAlignment = HorizontalAlignment.Center;
        _label.CustomMinimumSize = new Vector2(640, 40);
        _label.Position = -_label.GetCustomMinimumSize() / 2;
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", FontSize ?? 40);
        _label.Modulate = Color ?? new Color("FFFFFF");
        _label.ZIndex = 1;
        AddChild(_label);
        
        _shadow.Text = Text;
        _shadow.CustomMinimumSize = new Vector2(640, 40);
        _shadow.HorizontalAlignment = HorizontalAlignment.Center;
        _shadow.AddThemeFontOverride("font", font);
        _shadow.AddThemeFontSizeOverride("font_size", FontSize ?? 40);
        _shadow.Modulate = new Color(0, 0, 0, 1f);
        _shadow.Position = new Vector2(4, 4) -_label.GetCustomMinimumSize() / 2;
        _shadow.ZIndex = _label.ZIndex - 1;
        AddChild(_shadow);
        
        font.Dispose();
    }

    public override void _Process(double delta)
    {
        
    }

    protected void QueueFreeLabels() {
        _label.QueueFree();
        _shadow.QueueFree();
    }
}