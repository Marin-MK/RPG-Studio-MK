using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class GenericDoubleNumberPicker : PopupWindow
{
    public bool Apply = false;
    public int Value1;
    public int Value2;

    NumericBox Number1Box;
    NumericBox Number2Box;

    public GenericDoubleNumberPicker(string Title, string Label1, int Value1, int? MinValue1, int? MaxValue1, string Label2, int Value2, int? MinValue2, int? MaxValue2)
    {
        this.Value1 = Value1;
        this.Value2 = Value2;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(337, 169);
        SetSize(MaximumSize);
        Center();

        Number1Box = new NumericBox(this);
        Number1Box.SetPosition(36, 69);
        Number1Box.SetSize(116, 30);
        if (MinValue1 != null) Number1Box.SetMinValue((int) MinValue1);
        if (MaxValue1 != null) Number1Box.SetMaxValue((int) MaxValue1);
        Number1Box.SetValue(Value1);

        Label Text1Label = new Label(this);
        Text1Label.SetFont(Fonts.Paragraph);
        Text1Label.SetText(Label1);
        Text1Label.SetPosition(36, 50);

        Number2Box = new NumericBox(this);
        Number2Box.SetPosition(188, 69);
        Number2Box.SetSize(116, 30);
        if (MinValue2 != null) Number2Box.SetMinValue((int) MinValue2);
        if (MaxValue2 != null) Number2Box.SetMaxValue((int) MaxValue2);
        Number2Box.SetValue(Value2);

        Label Text2Label = new Label(this);
        Text2Label.SetFont(Fonts.Paragraph);
        Text2Label.SetText(Label2);
        Text2Label.SetPosition(188, 50);

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
        this.Value1 = Number1Box.Value;
        this.Value2 = Number2Box.Value;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
