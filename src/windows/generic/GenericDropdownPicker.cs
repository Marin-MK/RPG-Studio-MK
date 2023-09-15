using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class GenericDropdownPicker : PopupWindow
{
    public bool Apply = false;
    public int Value;

    DropdownBox DropdownBox;

    public GenericDropdownPicker(string Title, string Label, int Index, List<string> Items)
    {
        this.Value = Index;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(272, 166);
        SetSize(MaximumSize);
        Center();

        DropdownBox = new DropdownBox(this);
        DropdownBox.SetPosition(31, 74);
        DropdownBox.SetSize(210, 24);
        DropdownBox.SetItems(Items.Select(i => new TreeNode(i)).ToList());
        DropdownBox.SetSelectedIndex(Value);

        Label TextLabel = new Label(this);
        TextLabel.SetFont(Fonts.Paragraph);
        TextLabel.SetText(Label);
        TextLabel.SetPosition(31, 50);

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
        this.Value = DropdownBox.SelectedIndex;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
