using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class GenericTextBoxWindow : PopupWindow
{
    public bool Apply = false;
    public string Value;

    TextBox TextBox;

    public GenericTextBoxWindow(string Title, string Label, string Text, bool useMonospace = false)
    {
        this.Value = Text;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(350, 156);
        SetSize(MaximumSize);
        Center();

        TextBox = new TextBox(this);
        TextBox.SetPosition(25, 70);
        TextBox.SetSize(302, 27);
        TextBox.SetText(Text);
        TextBox.SetFont(useMonospace ? Fonts.Monospace : Fonts.Paragraph);

        Label TextLabel = new Label(this);
        TextLabel.SetFont(Fonts.Paragraph);
        TextLabel.SetText(Label);
        TextLabel.SetPosition(25, 44);

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
        this.Value = TextBox.Text;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
