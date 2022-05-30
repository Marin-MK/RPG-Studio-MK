using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class GenericTextBoxWindow : PopupWindow
{
    public bool Apply = false;
    public string Value;

    TextBox TextBox;

    public GenericTextBoxWindow(string Title, string Label, string Text)
    {
        this.Value = Text;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(200, 110);
        SetSize(MaximumSize);
        Center();

        TextBox = new TextBox(this);
        TextBox.SetPosition(87, 30);
        TextBox.SetSize(96, 27);
        TextBox.SetText(Text);

        Label TextLabel = new Label(this);
        TextLabel.SetFont(Fonts.ProductSansMedium.Use(14));
        TextLabel.SetText(Label);
        TextLabel.SetPosition(TextBox.Position.X - TextLabel.Size.Width - 8, 34);

        CreateButton("Cancel", Cancel);
        CreateButton("OK", OK);
    }

    private void OK(BaseEventArgs e)
    {
        Apply = true;
        this.Value = TextBox.Text;
        Close();
    }

    private void Cancel(BaseEventArgs e)
    {
        Close();
    }
}
