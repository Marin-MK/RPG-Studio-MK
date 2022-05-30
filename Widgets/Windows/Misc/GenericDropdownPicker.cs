using System;
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
        MinimumSize = MaximumSize = new Size(200, 110);
        SetSize(MaximumSize);
        Center();

        DropdownBox = new DropdownBox(this);
        DropdownBox.SetPosition(87, 30);
        DropdownBox.SetSize(96, 27);
        DropdownBox.SetItems(Items.Select(i => new ListItem(i)).ToList());
        DropdownBox.SetSelectedIndex(Value);

        Label TextLabel = new Label(this);
        TextLabel.SetFont(Fonts.ProductSansMedium.Use(14));
        TextLabel.SetText(Label);
        TextLabel.SetPosition(DropdownBox.Position.X - TextLabel.Size.Width - 8, 34);

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
