using Godot;

namespace Cardium.Scripts.Labels;

public partial class LabelWithShadow : Node2D
{
    private Label _label;
    private Label _shadow;
    
    public string Text;
    public Color Color;
    
    public override void _Ready()
    {
        var font = GD.Load<FontFile>("res://Assets/Fonts/alagard.ttf");

        ZIndex = 100;
        
        _label = new Label();
        _label.Text = Text;
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", 40);
        _label.Modulate = Color;
        _label.ZIndex = 1;
        AddChild(_label);
        
        var shadow = new Label();
        shadow.Text = Text;
        shadow.AddThemeFontOverride("font", font);
        shadow.AddThemeFontSizeOverride("font_size", 40);
        shadow.Modulate = new Color(0, 0, 0, 1f);
        shadow.Position = new Vector2(4, 4);
        shadow.ZIndex = _label.ZIndex - 1;
        AddChild(shadow);
    }

    public override void _Process(double delta)
    {
        
    }
}