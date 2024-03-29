﻿using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class VariablePicker : PopupWindow
{
    static int VariablesPerGroup = 25;

    public bool Apply = false;
    public int VariableID = -1;

    ListBox GroupListBox;
    ListBox VariableListBox;
    TextBox NameBox;
    Button ChangeMaxButton;

    public VariablePicker(int InitialID = 1)
    {
        SetTitle("Game Variables");
        MinimumSize = MaximumSize = new Size(370, 640);
        SetSize(MaximumSize);
        Center();

        Label GroupLabel = new Label(this);
        GroupLabel.SetFont(Fonts.ParagraphBold);
        GroupLabel.SetPosition(16, 30);
        GroupLabel.SetText("Groups");

        GroupListBox = new ListBox(this);
        GroupListBox.SetFont(Fonts.Paragraph);
        GroupListBox.SetVDocked(true);
        GroupListBox.SetPadding(13, 50, 0, 52);
        GroupListBox.SetWidth(114);
        GroupListBox.OnSelectionChanged += _ => RedrawVariableList();

        ChangeMaxButton = new Button(this);
        ChangeMaxButton.SetBottomDocked(true);
        ChangeMaxButton.SetPadding(GroupListBox.Padding.Left, 0, 0, 15);
        ChangeMaxButton.SetSize(GroupListBox.Size.Width, 32);
        ChangeMaxButton.SetText("Change Max");

        VariableListBox = new ListBox(this);
        VariableListBox.SetFont(Fonts.Paragraph);
        VariableListBox.SetDocked(true);
        VariableListBox.SetPadding(GroupListBox.Padding.Left + GroupListBox.Size.Width + 6, 50, 16, 84);
        VariableListBox.OnSelectionChanged += _ => VariableSelectionChanged();

        Label VariableLabel = new Label(this);
        VariableLabel.SetFont(Fonts.ParagraphBold);
        VariableLabel.SetPadding(VariableListBox.Padding.Left + 2, 30, 0, 0);
        VariableLabel.SetText("Variables");

        Label NameLabel = new Label(this);
        NameLabel.SetFont(Fonts.Paragraph);
        NameLabel.SetBottomDocked(true);
        NameLabel.SetPadding(VariableListBox.Padding.Left + 8, 0, 0, 25);
        NameLabel.SetText("Name:");

        NameBox = new TextBox(this);
        NameBox.SetFont(Fonts.Paragraph);
        NameBox.SetHDocked(true);
        NameBox.SetBottomDocked(true);
        NameBox.SetPadding(VariableListBox.Padding.Left + 50, 0, 16, 29);
        NameBox.SetHeight(27);
        NameBox.OnTextChanged += _ => NameChanged();

        RedrawGroupList();

        int groupidx = (InitialID - 1) / VariablesPerGroup;
        int varidx = (InitialID - 1) % VariablesPerGroup;
        GroupListBox.SetSelectedIndex(groupidx);
        VariableListBox.SetSelectedIndex(varidx);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    private void RedrawGroupList()
    {
        int capacity = Data.System.Variables.Count;
        int groups = (int) Math.Ceiling(capacity / (double) VariablesPerGroup);
        List<TreeNode> Items = new List<TreeNode>();
        for (int i = 0; i < groups; i++)
        {
            int start = i * VariablesPerGroup;
            int end = Math.Min((i + 1) * VariablesPerGroup, capacity);
            Items.Add(new TreeNode($"[{Utilities.Digits(start + 1, 3)} - {Utilities.Digits(end, 3)}]"));
        }
        GroupListBox.SetItems(Items);
        if (GroupListBox.SelectedIndex == -1) GroupListBox.SetSelectedIndex(0);
    }

    private void RedrawVariableList()
    {
        int capacity = Data.System.Variables.Count;
        int start = GroupListBox.SelectedIndex * VariablesPerGroup;
        int end = Math.Min((GroupListBox.SelectedIndex + 1) * VariablesPerGroup, capacity);
        List<TreeNode> Items = new List<TreeNode>();
        for (int i = start; i < end; i++)
        {
            Items.Add(new TreeNode($"{Utilities.Digits(i + 1, 3)}: {Data.System.Variables[i]}"));
        }
        VariableListBox.SetItems(Items);
        if (VariableListBox.SelectedIndex == -1) VariableListBox.SetSelectedIndex(0);
    }

    private void VariableSelectionChanged()
    {
        int id = GroupListBox.SelectedIndex * VariablesPerGroup + VariableListBox.SelectedIndex + 1;
        NameBox.SetText(Data.System.Variables[id - 1]);
    }

    private void NameChanged()
    {
        int id = GroupListBox.SelectedIndex * VariablesPerGroup + VariableListBox.SelectedIndex + 1;
        Data.System.Variables[id - 1] = NameBox.Text;
        RedrawVariableList();
    }

    private void OK()
    {
        VariableID = GroupListBox.SelectedIndex * VariablesPerGroup + VariableListBox.SelectedIndex + 1;
        Apply = true;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
