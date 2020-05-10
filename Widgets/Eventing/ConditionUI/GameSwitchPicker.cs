using System;
using System.Collections.Generic;
using System.Text;
using ODL;

namespace MKEditor.Widgets
{
    public class GameSwitchPicker : Widget
    {
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
        Label ValueLabel;
        DropdownBox ValueBox;

        public GameSwitchPicker(IContainer Parent) : base(Parent)
        {
            CategoryLabel = new Label(this);
            CategoryLabel.SetText("Categories");
            CategoryLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));

            SwitchLabel = new Label(this);
            SwitchLabel.SetText("Switches");
            SwitchLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));

            GroupBox = new ListBox(this);
            RedrawGroupBox();
            GroupBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                GroupNameBox.SetInitialText(Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Name?? "");
                RedrawSwitchBox();
                SwitchBox.SetSelectedIndex(0);
                SwitchBox.MainContainer.VScrollBar.SetValue(0);
            };
            
            SwitchBox = new ListBox(this);
            SwitchBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                SwitchNameBox.SetInitialText(Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches[SwitchBox.SelectedIndex].Name ?? "");
            };

            GroupNameLabel = new Label(this);
            GroupNameLabel.SetText("Name:");
            GroupNameLabel.SetFont(Font.Get("Fonts/ProductSans-M", 12));
            GroupNameBox = new TextBox(this);
            GroupNameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Name = GroupNameBox.Text;
                GroupBox.Redraw();
            };

            SwitchNameLabel = new Label(this);
            SwitchNameLabel.SetText("Name:");
            SwitchNameLabel.SetFont(Font.Get("Fonts/ProductSans-M", 12));
            SwitchNameBox = new TextBox(this);
            SwitchNameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                Editor.ProjectSettings.Switches[GroupBox.SelectedIndex].Switches[SwitchBox.SelectedIndex].Name = SwitchNameBox.Text;
                SwitchBox.Redraw();
            };

            ChangeMaxGroups = new Button(this);
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

            ValueLabel = new Label(this);
            ValueLabel.SetText("Switch must be:");
            ValueLabel.SetFont(Font.Get("Fonts/ProductSans-M", 12));
            ValueBox = new DropdownBox(this);
            ValueBox.SetItems(new List<ListItem>()
            {
                new ListItem("ON/Enabled"),
                new ListItem("OFF/Disabled")
            });

            GroupBox.SetSelectedIndex(0);
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

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            if (Size.Width == 50 && Size.Height == 50) return;

            int w = Size.Width / 2 - 12;

            CategoryLabel.SetPosition(9, -4);
            SwitchLabel.SetPosition(w + 26, -4);

            GroupBox.SetPosition(5, 16);
            GroupBox.SetSize(w, Size.Height - 113);
            SwitchBox.SetPosition(w + 17, 16);
            SwitchBox.SetSize(w, Size.Height - 113);

            GroupNameLabel.SetPosition(8, Size.Height - 87);
            GroupNameBox.SetPosition(55, Size.Height - 92);
            GroupNameBox.SetSize(w - 50, 27);

            SwitchNameLabel.SetPosition(w + 25, Size.Height - 87);
            SwitchNameBox.SetPosition(w + 67, Size.Height - 92);
            SwitchNameBox.SetSize(w - 50, 27);

            ChangeMaxGroups.SetPosition(7, Size.Height - 60);
            ChangeMaxGroups.SetSize(w - 2, 31);
            ChangeMaxSwitches.SetPosition(w + 19, Size.Height - 60);
            ChangeMaxSwitches.SetSize(w - 2, 31);

            ValueLabel.SetPosition(9, Size.Height - 20);
            ValueBox.SetPosition(104, Size.Height - 25);
            ValueBox.SetSize(110, 25);
        }

        public override object GetValue(string Identifier)
        {
            if (Identifier == "group_id") return GroupBox.SelectedIndex + 1;
            else if (Identifier == "switch_id") return SwitchBox.SelectedIndex + 1;
            else if (Identifier == "value") return ValueBox.SelectedIndex == 0;
            throw new Exception($"Invalid identifier '{Identifier}'");
        }

        public override void SetValue(string Identifier, object Value)
        {
            if (Identifier == "group_id") GroupBox.SetSelectedIndex((int) Value - 1);
            else if (Identifier == "switch_id") SwitchBox.SetSelectedIndex((int) Value - 1);
            else if (Identifier == "value") ValueBox.SetSelectedIndex((bool) Value ? 0 : 1);
            else throw new Exception($"Invalid identifier '{Identifier}'");
        }
    }
}
