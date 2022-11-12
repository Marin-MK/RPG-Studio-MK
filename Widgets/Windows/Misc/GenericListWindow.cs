using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class GenericListWindow : PopupWindow
{
    public bool Apply = false;
    public List<string> Value;

    ListBox ListBox;
    TextBox NameBox;
    Button AddButton;
    Button DeleteButton;

    public GenericListWindow(string Title, List<string> Items)
    {
        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(199, 400);
        SetSize(MaximumSize);
        Center();

        ListBox = new ListBox(this);
        ListBox.SetDocked(true);
        ListBox.SetPadding(16, 36, 16, 160);
        ListBox.SetItems(Items.Select(i => new ListItem(i)).ToList());

        NameBox = new TextBox(this);
        NameBox.SetHDocked(true);
        NameBox.SetBottomDocked(true);
        NameBox.SetHeight(27);
        NameBox.SetPadding(16, 0, 16, 124);
        NameBox.SetDeselectOnEnterPressed(false);
        NameBox.OnEnterPressed += _ =>
        {
            AddButton.OnClicked.Invoke(new BaseEventArgs());
            NameBox.SetText("");
        };

        AddButton = new Button(this);
        AddButton.SetHDocked(true);
        AddButton.SetBottomDocked(true);
        AddButton.SetPadding(16, 0, 16, 88);
        AddButton.SetText("Add Item");
        AddButton.OnClicked += _ =>
        {
            int Idx = ListBox.SelectedIndex;
            if (Idx >= 0) ListBox.Items.Insert(Idx, new ListItem(NameBox.Text));
            else ListBox.Items.Add(new ListItem(NameBox.Text));
            ListBox.SetItems(ListBox.Items);
            ListBox.SetSelectedIndex(Idx);
            DeleteButton.SetEnabled(true);
        };

        DeleteButton = new Button(this);
        DeleteButton.SetHDocked(true);
        DeleteButton.SetBottomDocked(true);
        DeleteButton.SetPadding(16, 0, 16, 50);
        DeleteButton.SetText("Delete Item");
        DeleteButton.OnClicked += _ =>
        {
            ListBox.Items.RemoveAt(ListBox.SelectedIndex);
            ListBox.SetItems(ListBox.Items);
            if (ListBox.SelectedIndex > 0) ListBox.SetSelectedIndex(ListBox.SelectedIndex - 1);
            if (ListBox.Items.Count == 0) DeleteButton.SetEnabled(false);
        };

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
        this.Value = ListBox.Items.Select(i => i.Name).ToList();
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
