using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class GameSwitchPicker : PopupWindow
    {
        public int GroupID;
        public int SwitchID;

        Label CategoryLabel;
        Label SwitchLabel;
        ListBox GroupBox;
        ListBox SwitchBox;
        Label GroupNameLabel;
        Label SwitchNameLabel;
        TextBox GroupNameBox;
        TextBox SwitchNameBox;
        Button ChangeMaxGroups;
        Button ChangeMaxSwitches;

        public GameSwitchPicker(int GroupID, int SwitchID) : base()
        {
            this.GroupID = GroupID;
            this.SwitchID = SwitchID;
            MinimumSize = MaximumSize = new Size(361, 409);
            SetTitle("Choose Game Switch");
            SetSize(MaximumSize);
            Center();

            CategoryLabel = new Label(this);
            CategoryLabel.SetPosition(10, 28);
            CategoryLabel.SetText("Categories");
            CategoryLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));

            SwitchLabel = new Label(this);
            SwitchLabel.SetPosition(194, 28);
            SwitchLabel.SetText("Switches");
            SwitchLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));

            GroupBox = new ListBox(this);
            GroupBox.SetPosition(6, 48);
            GroupBox.SetSize(167, 254);
            RedrawGroupBox();
            GroupBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                GroupNameBox.SetInitialText(Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Name?? "");
                RedrawSwitchBox();
                SwitchBox.SetSelectedIndex(0, true);
                SwitchBox.MainContainer.VScrollBar.SetValue(0);
            };

            SwitchBox = new ListBox(this);
            SwitchBox.SetPosition(185, 48);
            SwitchBox.SetSize(167, 254);
            SwitchBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                SwitchNameBox.SetInitialText(Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches[SwitchBox.SelectedIndex].Name ?? "");
            };
            SwitchBox.OnDoubleClicked += delegate (BaseEventArgs e)
            {
                this.SwitchID = SwitchBox.SelectedIndex + 1;
                OK(new BaseEventArgs());
            };

            GroupNameLabel = new Label(this);
            GroupNameLabel.SetPosition(9, 311);
            GroupNameLabel.SetText("Name:");
            GroupNameLabel.SetFont(Font.Get("Fonts/ProductSans-M", 12));
            GroupNameBox = new TextBox(this);
            GroupNameBox.SetPosition(56, 307);
            GroupNameBox.SetSize(117, 27);
            GroupNameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Name = GroupNameBox.Text;
                GroupBox.Redraw();
            };

            SwitchNameLabel = new Label(this);
            SwitchNameLabel.SetPosition(193, 311);
            SwitchNameLabel.SetText("Name:");
            SwitchNameLabel.SetFont(Font.Get("Fonts/ProductSans-M", 12));
            SwitchNameBox = new TextBox(this);
            SwitchNameBox.SetPosition(235, 307);
            SwitchNameBox.SetSize(117, 27);
            SwitchNameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches[SwitchBox.SelectedIndex].Name = SwitchNameBox.Text;
                SwitchBox.Redraw();
            };

            ChangeMaxGroups = new Button(this);
            ChangeMaxGroups.SetPosition(9, 340);
            ChangeMaxGroups.SetSize(163, 29);
            ChangeMaxGroups.SetText("Change Maximum");
            ChangeMaxGroups.OnClicked += delegate (BaseEventArgs e)
            {
                PopupWindow win = new PopupWindow();
                win.SetSize(270, 125);
                win.SetTitle("Set Switch Group capacity");
                Label label = new Label(win);
                label.SetText("Set the maximum available number of groups.");
                label.SetPosition(5, 35);
                Label label2 = new Label(win);
                label2.SetText("Capacity:");
                label2.SetPosition(75, 60);
                NumericBox num = new NumericBox(win);
                num.SetSize(66, 27);
                num.SetPosition(130, 55);
                num.SetValue(Editor.ProjectSettings.SwitchGroupCapacity);
                num.MinValue = 1;
                win.CreateButton("Cancel", delegate (BaseEventArgs e) { win.Close(); });
                win.CreateButton("OK", delegate (BaseEventArgs e)
                {
                    int NewValue = num.Value;
                    if (NewValue == Editor.ProjectSettings.SwitchGroupCapacity)
                    {
                        win.Close();
                        return;
                    }
                    else if (NewValue > Editor.ProjectSettings.SwitchGroupCapacity)
                    {
                        int Extra = NewValue - Editor.ProjectSettings.SwitchGroupCapacity;
                        for (int i = 0; i < Extra; i++) Editor.ProjectSettings.Switches.Add(new GameSwitchGroup() { ID = Editor.ProjectSettings.Switches.Count + 1 });
                        Editor.ProjectSettings.SwitchGroupCapacity = NewValue;
                        RedrawGroupBox();
                        win.Close();
                    }
                    else
                    {
                        int Lost = Editor.ProjectSettings.SwitchGroupCapacity - NewValue;
                        MessageBox box = new MessageBox("Warning",
                            $"By resizing the Switch Group capacity from {Editor.ProjectSettings.SwitchGroupCapacity} to {NewValue}, {Lost} entries will be removed.\n" +
                            "This may cause unforeseen problems if Game Switches from these groups are still in use.\n" +
                            "Would you like to proceed and delete these Switch Groups?", ButtonType.YesNoCancel, IconType.Warning);
                        box.OnButtonPressed += delegate (BaseEventArgs e)
                        {
                            if (box.Result == 0) // Yes -> resize Switch Group capacity and delete Switch Groups
                            {
                                for (int i = Editor.ProjectSettings.Switches.Count - 1; i >= 0; i--)
                                {
                                    if (i == NewValue) break;
                                    Editor.ProjectSettings.Switches[i] = null;
                                }
                                Editor.ProjectSettings.Switches.RemoveRange(NewValue, Lost);
                                Editor.ProjectSettings.SwitchGroupCapacity = NewValue;
                                RedrawGroupBox();
                                win.Close();
                            }
                            else // No, cancel -> do nothing
                            {
                                win.Close();
                            }
                        };
                    }
                });
                win.Center();
            };

            ChangeMaxSwitches = new Button(this);
            ChangeMaxSwitches.SetPosition(188, 340);
            ChangeMaxSwitches.SetSize(163, 29);
            ChangeMaxSwitches.SetText("Change Maximum");
            ChangeMaxSwitches.OnClicked += delegate (BaseEventArgs e)
            {
                PopupWindow win = new PopupWindow();
                win.SetSize(270, 125);
                win.SetTitle("Set Switch capacity");
                Label label = new Label(win);
                label.SetText("Set the maximum available number of switches.");
                label.SetPosition(5, 35);
                Label label2 = new Label(win);
                label2.SetText("Capacity:");
                label2.SetPosition(75, 60);
                NumericBox num = new NumericBox(win);
                num.SetSize(66, 27);
                num.SetPosition(130, 55);
                num.SetValue(Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity);
                num.MinValue = 1;
                win.CreateButton("Cancel", delegate (BaseEventArgs e) { win.Close(); });
                win.CreateButton("OK", delegate (BaseEventArgs e)
                {
                    int NewValue = num.Value;
                    if (NewValue == Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity)
                    {
                        win.Close();
                        return;
                    }
                    else if (NewValue > Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity)
                    {
                        int Extra = NewValue - Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity;
                        for (int i = 0; i < Extra; i++) Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches.Add(new GameSwitch() { ID = Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches.Count + 1 });
                        Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity = NewValue;
                        RedrawSwitchBox();
                        win.Close();
                    }
                    else
                    {
                        int Lost = Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity - NewValue;
                        MessageBox box = new MessageBox("Warning",
                            $"By resizing the Switch capacity from {Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity} to {NewValue}, {Lost} entries will be removed.\n" +
                            "This may cause unforeseen problems if any of these Switches are still in use.\n" +
                            "Would you like to proceed and delete these Switches?", ButtonType.YesNoCancel, IconType.Warning);
                        box.OnButtonPressed += delegate (BaseEventArgs e)
                        {
                            if (box.Result == 0) // Yes -> resize Switch Group capacity and delete Switch Groups
                            {
                                for (int i = Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches.Count - 1; i >= 0; i--)
                                {
                                    if (i == NewValue) break;
                                    Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches[i] = null;
                                }
                                Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches.RemoveRange(NewValue, Lost);
                                Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].SwitchCapacity = NewValue;
                                RedrawSwitchBox();
                                win.Close();
                            }
                            else // No, cancel -> do nothing
                            {
                                win.Close();
                            }
                        };
                    }
                });
                win.Center();
            };

            GroupBox.SetSelectedIndex(this.GroupID - 1);
            SwitchBox.SetSelectedIndex(this.SwitchID - 1);

            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);
        }

        public void RedrawGroupBox()
        {
            List<ListItem> Items = new List<ListItem>();
            for (int i = 0; i < Editor.ProjectSettings.Switches.Count; i++)
            {
                Items.Add(new ListItem(Editor.ProjectSettings.Switches[i]));
            }
            GroupBox.SetItems(Items);
            if (GroupBox.SelectedIndex >= Items.Count) GroupBox.SetSelectedIndex(Items.Count - 1);
        }

        public void RedrawSwitchBox()
        {
            List<ListItem> Items = new List<ListItem>();
            for (int i = 0; i < Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches.Count; i++)
            {
                Items.Add(new ListItem(Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches[i]));
            }
            SwitchBox.SetItems(Items);
            if (SwitchBox.SelectedIndex >= Items.Count) SwitchBox.SetSelectedIndex(Items.Count - 1);
        }

        public void OK(BaseEventArgs e)
        {
            this.GroupID = GroupBox.SelectedIndex + 1;
            this.SwitchID = SwitchBox.SelectedIndex + 1;
            this.Close();
        }

        public void Cancel(BaseEventArgs e)
        {
            this.Close();
        }
    }
}
