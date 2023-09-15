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
    NumericBox NumberBox;
    Button AddButton;
    Button DeleteButton;

    public GenericListWindow(string Title, List<string> Items, bool NumericAndOrdered = false)
    {
        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(199, 400);
        SetSize(MaximumSize);
        Center();

        ListBox = new ListBox(this);
        ListBox.SetDocked(true);
        ListBox.SetPadding(16, 36, 16, 160);
        ListBox.SetItems(Items.Select(i => new TreeNode(i)).ToList());

        if (NumericAndOrdered)
        {
            NumberBox = new NumericBox(this);
            NumberBox.SetHDocked(true);
            NumberBox.SetBottomDocked(true);
            NumberBox.SetHeight(30);
            NumberBox.SetPadding(16, 0, 16, 126);
            NumberBox.OnEnterPressed += _ =>
            {
                AddButton.OnClicked.Invoke(new BaseEventArgs());
                NumberBox.SetValue(0);
            };
            ListBox.SetPadding(ListBox.Padding.Left, ListBox.Padding.Up, ListBox.Padding.Right, ListBox.Padding.Down + 4);
        }
        else
        {
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
        }

        AddButton = new Button(this);
        AddButton.SetHDocked(true);
        AddButton.SetBottomDocked(true);
        AddButton.SetPadding(16, 0, 16, 88);
        AddButton.SetText("Add Item");
        AddButton.OnClicked += _ =>
        {
            int Idx = ListBox.SelectedIndex;
			TreeNode Item = new TreeNode(NumericAndOrdered ? NumberBox.Value.ToString() : NameBox.Text);
            if (NumericAndOrdered)
            {
                ListBox.Items.Add(Item);
                ListBox.Items.Sort((TreeNode a, TreeNode b) => Convert.ToInt32(a.Text).CompareTo(Convert.ToInt32(b.Text)));
                Idx = ListBox.Items.IndexOf(Item);
            }
            else
            {
                if (Idx >= 0) ListBox.Items.Insert(Idx, Item);
                else ListBox.Items.Add(Item);
            }
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
            if (ListBox.SelectedIndex == -1) return;
            ListBox.RemoveItem(ListBox.SelectedItem);
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
        this.Value = ListBox.Items.Select(i => i.Text).ToList();
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
