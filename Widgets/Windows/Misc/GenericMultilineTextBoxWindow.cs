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

    MultilineTextBox TextBox;

    public GenericMultilineTextBoxWindow(string Title, string Text, bool Monospace = false)
    {
        this.Text = Text;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(680, 560);
        SetSize(MaximumSize);
        Center();

        TextBox = new MultilineTextBox(this);
        TextBox.SetFont(Monospace ? Fonts.FiraCode.Use(9) : Fonts.CabinMedium.Use(9));
        TextBox.SetDocked(true);
        TextBox.SetPadding(20, 30, 20, 50);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        TextBox.Activate();
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
