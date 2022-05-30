using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class GenericNumberPicker : PopupWindow
{
    public bool Apply = false;
    public int Value;

    NumericBox NumberBox;

    public GenericNumberPicker(string Title, string Label, int Value, int? MinValue, int? MaxValue)
    {
        this.Value = Value;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(200, 110);
        SetSize(MaximumSize);
        Center();

        NumberBox = new NumericBox(this);
        NumberBox.SetPosition(120, 30);
        NumberBox.SetSize(64, 27);
        if (MinValue != null) NumberBox.MinValue = (int) MinValue;
        if (MaxValue != null) NumberBox.MaxValue = (int) MaxValue;
        NumberBox.SetValue(Value);

        Label TextLabel = new Label(this);
        TextLabel.SetFont(Fonts.ProductSansMedium.Use(14));
        TextLabel.SetText(Label);
        TextLabel.SetPosition(NumberBox.Position.X - TextLabel.Size.Width - 8, 34);

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
