using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class GenericNumberPicker : PopupWindow
{
    public bool Apply = false;
    public int Value;

    NumericBox NumberBox;

    public GenericNumberPicker(string Title, string Label, int Value, int? MinValue = null, int? MaxValue = null, string? UnitText = null)
    {
        this.Value = Value;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(272, 171);
        SetSize(MaximumSize);
        Center();

        NumberBox = new NumericBox(this);
        NumberBox.SetPosition(31, 69);
        NumberBox.SetSize(158, 30);
        if (MinValue is not null) NumberBox.SetMinValue((int) MinValue);
        if (MaxValue is not null) NumberBox.SetMaxValue((int) MaxValue);
        NumberBox.SetValue(Value);

        Label TextLabel = new Label(this);
        TextLabel.SetFont(Fonts.Paragraph);
        TextLabel.SetText(Label);
        TextLabel.SetPosition(31, 50);

        if (UnitText is not null)
        {
            Label UnitLabel = new Label(this);
            UnitLabel.SetFont(Fonts.Paragraph);
            UnitLabel.SetText(UnitText);
            UnitLabel.SetPosition(195, 75);
        }
        else NumberBox.SetWidth(NumberBox.Size.Width + 42);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    private void OK()
    {
        Apply = true;
        this.Value = NumberBox.Value;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
