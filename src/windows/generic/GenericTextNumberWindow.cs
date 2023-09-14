using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class GenericTextNumberWindow : PopupWindow
{
    public bool Apply = false;
    public string Value1;
    public int Value2;

    TextBox TextBox;
    NumericBox NumericBox;

    public GenericTextNumberWindow(string Title, string Label1, string Value1, string Label2, int Value2, int? MinValue2, int? MaxValue2)
    {
        this.Value1 = Value1;
        this.Value2 = Value2;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(337, 169);
        SetSize(MaximumSize);
        Center();

        TextBox = new TextBox(this);
        TextBox.SetPosition(36, 69);
        TextBox.SetSize(116, 30);
		TextBox.SetText(Value1);

        Label Text1Label = new Label(this);
        Text1Label.SetFont(Fonts.Paragraph);
        Text1Label.SetText(Label1);
		Text1Label.SetPosition(36, 50);

		NumericBox = new NumericBox(this);
		NumericBox.SetPosition(188, 69);
		NumericBox.SetSize(116, 30);
		if (MinValue2 != null) NumericBox.SetMinValue((int) MinValue2);
        if (MaxValue2 != null) NumericBox.SetMaxValue((int) MaxValue2);
        NumericBox.SetValue(Value2);

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
        this.Value1 = TextBox.Text;
        this.Value2 = NumericBox.Value;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
