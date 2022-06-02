using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class SwitchPicker : PopupWindow
{
    static int SwitchesPerGroup = 25;

    public bool Apply = false;
    public int SwitchID = -1;

    ListBox GroupListBox;
    ListBox SwitchListBox;
    TextBox NameBox;
    Button ChangeMaxButton;

    public SwitchPicker(int InitialID = 1)
    {
        SetTitle("Game Switches");
        MinimumSize = MaximumSize = new Size(370, 640);
        SetSize(MaximumSize);
        Center();

        Font HeaderFont = Fonts.UbuntuBold.Use(11);
        Font ListFont = Fonts.UbuntuRegular.Use(11);
        Font SmallFont = Fonts.ProductSansMedium.Use(11);

        Label GroupLabel = new Label(this);
        GroupLabel.SetFont(HeaderFont);
        GroupLabel.SetPosition(16, 30);
        GroupLabel.SetText("Groups");

        GroupListBox = new ListBox(this);
        GroupListBox.SetFont(ListFont);
        GroupListBox.SetVDocked(true);
        GroupListBox.SetPadding(13, 50, 0, 52);
        GroupListBox.SetWidth(114);
        GroupListBox.OnSelectionChanged += _ => RedrawSwitchList();

        ChangeMaxButton = new Button(this);
        ChangeMaxButton.SetBottomDocked(true);
        ChangeMaxButton.SetPadding(GroupListBox.Padding.Left, 0, 0, 15);
        ChangeMaxButton.SetFont(HeaderFont);
        ChangeMaxButton.SetSize(GroupListBox.Size.Width, 32);
        ChangeMaxButton.SetText("Change Max");

        SwitchListBox = new ListBox(this);
        SwitchListBox.SetFont(ListFont);
        SwitchListBox.SetDocked(true);
        SwitchListBox.SetPadding(GroupListBox.Padding.Left + GroupListBox.Size.Width + 6, 50, 16, 84);
        SwitchListBox.OnSelectionChanged += _ => SwitchSelectionChanged();

        Label SwitchLabel = new Label(this);
        SwitchLabel.SetFont(HeaderFont);
        SwitchLabel.SetPadding(SwitchListBox.Padding.Left + 2, 30, 0, 0);
        SwitchLabel.SetText("Switches");

        Label NameLabel = new Label(this);
        NameLabel.SetFont(SmallFont);
        NameLabel.SetBottomDocked(true);
        NameLabel.SetPadding(SwitchListBox.Padding.Left + 8, 0, 0, 25);
        NameLabel.SetText("Name:");

        NameBox = new TextBox(this);
        NameBox.SetFont(SmallFont);
        NameBox.SetHDocked(true);
        NameBox.SetBottomDocked(true);
        NameBox.SetPadding(SwitchListBox.Padding.Left + 50, 0, 16, 29);
        NameBox.SetHeight(27);
        NameBox.OnTextChanged += _ => NameChanged();

        RedrawGroupList();

        int groupidx = (InitialID - 1) / SwitchesPerGroup;
        int switchidx = (InitialID - 1) % SwitchesPerGroup;
        GroupListBox.SetSelectedIndex(groupidx);
        SwitchListBox.SetSelectedIndex(switchidx);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    private void RedrawGroupList()
    {
        int capacity = Data.System.Switches.Count;
        int groups = (int) Math.Ceiling(capacity / (double) SwitchesPerGroup);
        List<ListItem> Items = new List<ListItem>();
        for (int i = 0; i < groups; i++)
        {
            int start = i * SwitchesPerGroup;
            int end = Math.Min((i + 1) * SwitchesPerGroup, capacity);
            Items.Add(new ListItem($"[{Utilities.Digits(start + 1, 3)} - {Utilities.Digits(end, 3)}]"));
        }
        GroupListBox.SetItems(Items);
        if (GroupListBox.SelectedIndex == -1) GroupListBox.SetSelectedIndex(0);
    }

    private void RedrawSwitchList()
    {
        int capacity = Data.System.Switches.Count;
        int start = GroupListBox.SelectedIndex * SwitchesPerGroup;
        int end = Math.Min((GroupListBox.SelectedIndex + 1) * SwitchesPerGroup, capacity);
        List<ListItem> Items = new List<ListItem>();
        for (int i = start; i < end; i++)
        {
            Items.Add(new ListItem($"{Utilities.Digits(i + 1, 3)}: {Data.System.Switches[i]}"));
        }
        SwitchListBox.SetItems(Items);
        if (SwitchListBox.SelectedIndex == -1) SwitchListBox.SetSelectedIndex(0);
    }

    private void SwitchSelectionChanged()
    {
        int id = GroupListBox.SelectedIndex * SwitchesPerGroup + SwitchListBox.SelectedIndex + 1;
        NameBox.SetText(Data.System.Switches[id - 1]);
    }

    private void NameChanged()
    {
        int id = GroupListBox.SelectedIndex * SwitchesPerGroup + SwitchListBox.SelectedIndex + 1;
        Data.System.Switches[id - 1] = NameBox.Text;
        RedrawSwitchList();
    }

    private void OK()
    {
        SwitchID = GroupListBox.SelectedIndex * SwitchesPerGroup + SwitchListBox.SelectedIndex + 1;
        Apply = true;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
