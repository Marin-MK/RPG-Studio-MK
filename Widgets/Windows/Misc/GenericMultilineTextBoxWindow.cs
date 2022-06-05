using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class GenericMultilineTextBoxWindow : PopupWindow
{
    public string Text;
    public bool Apply = false;

    NewMultilineTextBox TextBox;

    public GenericMultilineTextBoxWindow(string Title, string Text)
    {
        this.Text = Text;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(400, 200);
        SetSize(MaximumSize);
        Center();

        TextBox = new NewMultilineTextBox(this);
        TextBox.SetFont(Fonts.CabinMedium.Use(9));
        TextBox.SetDocked(true);
        //TextBox.SetText(Text);
        TextBox.SetPadding(20, 30, 20, 50);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
    }

    private void OK()
    {
        this.Text = TextBox.Text;
        Apply = true;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
