using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class GameVariablePicker : PopupWindow
    {
        public int GroupID;
        public int VariableID;

        Label CategoryLabel;
        Label VariableLabel;
        ListBox GroupBox;
        ListBox VariableBox;
        Label GroupNameLabel;
        Label VariableNameLabel;
        TextBox GroupNameBox;
        TextBox VariableNameBox;
        Button ChangeMaxGroups;
        Button ChangeMaxVariables;

        public GameVariablePicker(int GroupID, int VariableID) : base()
        {
            this.GroupID = GroupID;
            this.VariableID = VariableID;
            MinimumSize = MaximumSize = new Size(361, 409);
            SetTitle("Choose Game Variable");
            SetSize(MaximumSize);
            Center();

            CategoryLabel = new Label(this);
            CategoryLabel.SetPosition(10, 28);
            CategoryLabel.SetText("Categories");
            CategoryLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));

            VariableLabel = new Label(this);
            VariableLabel.SetPosition(194, 28);
            VariableLabel.SetText("Variables");
            VariableLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));

            GroupBox = new ListBox(this);
            GroupBox.SetPosition(6, 48);
            GroupBox.SetSize(167, 254);
            RedrawGroupBox();
            GroupBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                GroupNameBox.SetInitialText(Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Name?? "");
                RedrawVariableBox();
                VariableBox.SetSelectedIndex(0, true);
                VariableBox.MainContainer.VScrollBar.SetValue(0);
            };

            VariableBox = new ListBox(this);
            VariableBox.SetPosition(185, 48);
            VariableBox.SetSize(167, 254);
            VariableBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                VariableNameBox.SetInitialText(Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables[VariableBox.SelectedIndex].Name ?? "");
            };
            VariableBox.OnDoubleClicked += delegate (BaseEventArgs e)
            {
                this.VariableID = VariableBox.SelectedIndex + 1;
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
                Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Name = GroupNameBox.Text;
                GroupBox.Redraw();
            };

            VariableNameLabel = new Label(this);
            VariableNameLabel.SetPosition(193, 311);
            VariableNameLabel.SetText("Name:");
            VariableNameLabel.SetFont(Font.Get("Fonts/ProductSans-M", 12));
            VariableNameBox = new TextBox(this);
            VariableNameBox.SetPosition(235, 307);
            VariableNameBox.SetSize(117, 27);
            VariableNameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables[VariableBox.SelectedIndex].Name = VariableNameBox.Text;
                VariableBox.Redraw();
            };

            ChangeMaxGroups = new Button(this);
            ChangeMaxGroups.SetPosition(9, 340);
            ChangeMaxGroups.SetSize(163, 29);
            ChangeMaxGroups.SetText("Change Maximum");
            ChangeMaxGroups.OnClicked += delegate (BaseEventArgs e)
            {
                PopupWindow win = new PopupWindow();
                win.SetSize(270, 125);
                win.SetTitle("Set Variable Group capacity");
                Label label = new Label(win);
                label.SetText("Set the maximum available number of groups.");
                label.SetPosition(5, 35);
                Label label2 = new Label(win);
                label2.SetText("Capacity:");
                label2.SetPosition(75, 60);
                NumericBox num = new NumericBox(win);
                num.SetSize(66, 27);
                num.SetPosition(130, 55);
                num.SetValue(Editor.ProjectSettings.VariableGroupCapacity);
                num.MinValue = 1;
                win.CreateButton("Cancel", delegate (BaseEventArgs e) { win.Close(); });
                win.CreateButton("OK", delegate (BaseEventArgs e)
                {
                    int NewValue = num.Value;
                    if (NewValue == Editor.ProjectSettings.VariableGroupCapacity)
                    {
                        win.Close();
                        return;
                    }
                    else if (NewValue > Editor.ProjectSettings.VariableGroupCapacity)
                    {
                        int Extra = NewValue - Editor.ProjectSettings.VariableGroupCapacity;
                        for (int i = 0; i < Extra; i++) Editor.ProjectSettings.Variables.Add(new GameVariableGroup() { ID = Editor.ProjectSettings.Variables.Count + 1 });
                        Editor.ProjectSettings.VariableGroupCapacity = NewValue;
                        RedrawGroupBox();
                        win.Close();
                    }
                    else
                    {
                        int Lost = Editor.ProjectSettings.VariableGroupCapacity - NewValue;
                        MessageBox box = new MessageBox("Warning",
                            $"By resizing the Variable Group capacity from {Editor.ProjectSettings.VariableGroupCapacity} to {NewValue}, {Lost} entries will be removed.\n" +
                            "This may cause unforeseen problems if Game Variables from these groups are still in use.\n" +
                            "Would you like to proceed and delete these Variable Groups?", ButtonType.YesNoCancel, IconType.Warning);
                        box.OnButtonPressed += delegate (BaseEventArgs e)
                        {
                            if (box.Result == 0) // Yes -> resize Switch Group capacity and delete Switch Groups
                            {
                                for (int i = Editor.ProjectSettings.Variables.Count - 1; i >= 0; i--)
                                {
                                    if (i == NewValue) break;
                                    Editor.ProjectSettings.Variables[i] = null;
                                }
                                Editor.ProjectSettings.Variables.RemoveRange(NewValue, Lost);
                                Editor.ProjectSettings.VariableGroupCapacity = NewValue;
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

            ChangeMaxVariables = new Button(this);
            ChangeMaxVariables.SetPosition(188, 340);
            ChangeMaxVariables.SetSize(163, 29);
            ChangeMaxVariables.SetText("Change Maximum");
            ChangeMaxVariables.OnClicked += delegate (BaseEventArgs e)
            {
                PopupWindow win = new PopupWindow();
                win.SetSize(270, 125);
                win.SetTitle("Set Variable capacity");
                Label label = new Label(win);
                label.SetText("Set the maximum available number of variables.");
                label.SetPosition(5, 35);
                Label label2 = new Label(win);
                label2.SetText("Capacity:");
                label2.SetPosition(75, 60);
                NumericBox num = new NumericBox(win);
                num.SetSize(66, 27);
                num.SetPosition(130, 55);
                num.SetValue(Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity);
                num.MinValue = 1;
                win.CreateButton("Cancel", delegate (BaseEventArgs e) { win.Close(); });
                win.CreateButton("OK", delegate (BaseEventArgs e)
                {
                    int NewValue = num.Value;
                    if (NewValue == Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity)
                    {
                        win.Close();
                        return;
                    }
                    else if (NewValue > Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity)
                    {
                        int Extra = NewValue - Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity;
                        for (int i = 0; i < Extra; i++) Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables.Add(new GameVariable() { ID = Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables.Count + 1 });
                        Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity = NewValue;
                        RedrawVariableBox();
                        win.Close();
                    }
                    else
                    {
                        int Lost = Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity - NewValue;
                        MessageBox box = new MessageBox("Warning",
                            $"By resizing the Variable capacity from {Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity} to {NewValue}, {Lost} entries will be removed.\n" +
                            "This may cause unforeseen problems if any of these Variables are still in use.\n" +
                            "Would you like to proceed and delete these Variables?", ButtonType.YesNoCancel, IconType.Warning);
                        box.OnButtonPressed += delegate (BaseEventArgs e)
                        {
                            if (box.Result == 0) // Yes -> resize Switch Group capacity and delete Switch Groups
                            {
                                for (int i = Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables.Count - 1; i >= 0; i--)
                                {
                                    if (i == NewValue) break;
                                    Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables[i] = null;
                                }
                                Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables.RemoveRange(NewValue, Lost);
                                Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].VariableCapacity = NewValue;
                                RedrawVariableBox();
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
            VariableBox.SetSelectedIndex(this.VariableID - 1);

            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);
        }

        public void RedrawGroupBox()
        {
            List<ListItem> Items = new List<ListItem>();
            for (int i = 0; i < Editor.ProjectSettings.Variables.Count; i++)
            {
                Items.Add(new ListItem(Editor.ProjectSettings.Variables[i]));
            }
            GroupBox.SetItems(Items);
            if (GroupBox.SelectedIndex >= Items.Count) GroupBox.SetSelectedIndex(Items.Count - 1);
        }

        public void RedrawVariableBox()
        {
            List<ListItem> Items = new List<ListItem>();
            for (int i = 0; i < Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables.Count; i++)
            {
                Items.Add(new ListItem(Editor.ProjectSettings.Variables[GroupBox.SelectedIndex].Variables[i]));
            }
            VariableBox.SetItems(Items);
            if (VariableBox.SelectedIndex >= Items.Count) VariableBox.SetSelectedIndex(Items.Count - 1);
        }

        public void OK(BaseEventArgs e)
        {
            this.GroupID = GroupBox.SelectedIndex + 1;
            this.VariableID = VariableBox.SelectedIndex + 1;
            this.Close();
        }

        public void Cancel(BaseEventArgs e)
        {
            this.Close();
        }
    }
}
