﻿using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditSelfSwitchCommandWindow : PopupWindow
{
    public bool Apply = false;
    public int EventID;
    public char Switch;
    public bool Value;
    public EventCommand NewCommand;

    DropdownBox EventBox;
    DropdownBox SelfSwitchBox;
    RadioBox OnBox;

    public EditSelfSwitchCommandWindow(Map Map, Event Event, EventCommand Command)
    {
        SetTitle("Set Self Switch");
        MinimumSize = MaximumSize = new Size(276, 190);
        SetSize(MaximumSize);
        Center();

        Container MainContainer = new Container(this);
        MainContainer.AutoResize = true;
        MainContainer.SetPosition(20, 50);

        Label EventLabel = new Label(MainContainer);
        EventLabel.SetFont(Fonts.Paragraph);
        EventLabel.SetText("Event:");
        EventLabel.SetPosition(0, 5);
        EventBox = new DropdownBox(MainContainer);
        EventBox.SetFont(Fonts.Paragraph);
        EventBox.SetPosition(80, 0);
        EventBox.SetSize(160, 25);
        List<TreeNode> Items = new List<TreeNode>()
        {
            new TreeNode("This event", -1)
        };
        List<int> keys = Map.Events.Keys.ToList();
        keys.Sort();
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i] == Event.ID) continue;
            Items.Add(new TreeNode($"{Utilities.Digits(keys[i], 3)}: {Map.Events[keys[i]].Name}", keys[i]));
        }
        EventBox.SetItems(Items);
        if (Command.Parameters.Count == 3)
        {
            int EventID = (int) (long) Command.Parameters[2];
            EventBox.SetSelectedIndex(Items.FindIndex(i => (int) i.Object == EventID));
        }
        else EventBox.SetSelectedIndex(0);

        Label SwitchLabel = new Label(MainContainer);
        SwitchLabel.SetFont(Fonts.Paragraph);
        SwitchLabel.SetPosition(0, 40);
        SwitchLabel.SetText("Self Switch:");

        SelfSwitchBox = new DropdownBox(MainContainer);
        SelfSwitchBox.SetFont(Fonts.Paragraph);
        SelfSwitchBox.SetPosition(80, 35);
        SelfSwitchBox.SetSize(48, 25);
        SelfSwitchBox.SetItems(new List<TreeNode>()
        {
            new TreeNode("A"), new TreeNode("B"), new TreeNode("C"),
            new TreeNode("D"), new TreeNode("E"), new TreeNode("F")
        });
        SelfSwitchBox.SetSelectedIndex(((string) Command.Parameters[0])[0] - 'A');

        Label ValueLabel = new Label(MainContainer);
        ValueLabel.SetFont(Fonts.Paragraph);
        ValueLabel.SetPosition(0, 70);
        ValueLabel.SetText("Value:");

        OnBox = new RadioBox(MainContainer);
        OnBox.SetFont(Fonts.Paragraph);
        OnBox.SetPosition(80, 70);
        OnBox.SetText("ON");
        OnBox.SetChecked((long) Command.Parameters[1] == 0);

        RadioBox OffBox = new RadioBox(MainContainer);
        OffBox.SetFont(Fonts.Paragraph);
        OffBox.SetPosition(130, 70);
        OffBox.SetText("OFF");
        OffBox.SetChecked(!OnBox.Checked);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
    }

    private void OK()
    {
        Apply = true;
        int idx = EventBox.SelectedIndex;
        if (idx == -1 || idx >= EventBox.Items.Count) idx = EventBox.Items.Count - 1;
        EventID = (int) EventBox.Items[idx].Object;
        Switch = (char) ('A' + SelfSwitchBox.SelectedIndex);
        Value = OnBox.Checked;
        List<object> parameters = new List<object>() { Switch.ToString(), Value ? 0L : 1L };
        if (EventID != -1) parameters.Add((long) EventID);
        NewCommand = new EventCommand(CommandCode.ControlSelfSwitch, 0, parameters);
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
