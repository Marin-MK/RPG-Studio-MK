using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class GenericTextBoxWindow : PopupWindow
{
    public bool Apply = false;
    public string Value;

    TextBox TextBox;

    public GenericTextBoxWindow(string Title, string Label, string Text, bool Wide = false)
    {
        this.Value = Text;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(Wide ? 300 : 200, 110);
        SetSize(MaximumSize);
        Center();

        TextBox = new TextBox(this);
        TextBox.SetPosition(87, 30);
        TextBox.SetSize(Wide ? 196 : 96, 27);
        TextBox.SetText(Text);

        Label TextLabel = new Label(this);
        TextLabel.SetFont(Fonts.Paragraph);
        TextLabel.SetText(Label);
        TextLabel.SetPosition(TextBox.Position.X - TextLabel.Size.Width - 8, 34);

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
