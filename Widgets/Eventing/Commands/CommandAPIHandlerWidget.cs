using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Text;
using odl;
using System.Linq;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class CommandAPIHandlerWidget : Widget
    {
        public BasicCommand Command;
        public EventPage PageData;
        public int Indent;
        public bool Selected { get; protected set; } = false;
        public Dictionary<string, Widget> DynamicReadOnlyWidgets = new Dictionary<string, Widget>();
        public Dictionary<string, Widget> DynamicWindowWidgets = new Dictionary<string, Widget>();
        public Dictionary<string, dynamic> DynamicWindowWidgetObjects = new Dictionary<string, dynamic>();

        Label HeaderLabel;
        Container WidgetContainer;
        PopupWindow PopupWindow;
        CommandUtility WindowUtility;

        Dictionary<string, object> OldParameters;

        public DynamicCommandType CommandType;

        public CommandAPIHandlerWidget(IContainer Parent) : base(Parent)
        {
            Sprites["bullet"] = new Sprite(this.Viewport);
            Sprites["bullet"].Bitmap = new Bitmap(4, 4);
            Sprites["bullet"].Bitmap.Unlock();
            Sprites["bullet"].Bitmap.FillRect(1, 0, 2, 4, Color.WHITE);
            Sprites["bullet"].Bitmap.FillRect(0, 1, 4, 2, Color.WHITE);
            Sprites["bullet"].Bitmap.Lock();

            HeaderLabel = new Label(this);
            HeaderLabel.SetPosition(19, 3);

            WidgetContainer = new Container(this);
            WidgetContainer.SetPosition(19, 20);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            WidgetContainer.SetWidth(Size.Width - WidgetContainer.Position.X);
        }

        public void SetCommand(EventPage PageData, BasicCommand Command, int Indent = -1)
        {
            this.PageData = PageData;
            this.Command = Command;
            if (Indent == -1 && Command != null) Indent = Command.Indent;
            this.Indent = Indent;
            this.Sprites["bullet"].X = Indent * CommandBox.Indent + 6;
            this.Sprites["bullet"].Y = 9;
            HeaderLabel.SetPosition(19 + Indent * CommandBox.Indent, HeaderLabel.Position.Y);
            if (this.Command == null)
            {
                SetHeight(20);
                return;
            }
            this.CommandType = CommandPlugins.CommandTypes.Find(t => t.Identifier == Command.Identifier.Substring(1)).EmptyClone();
            if (this.Command != null && this.CommandType.IsSubBranch)
            {
                this.Sprites["bullet"].Visible = false;
                WidgetContainer.SetPosition(30 + Indent * CommandBox.Indent, WidgetContainer.Position.Y);
            }
            else
            {
                WidgetContainer.SetPosition(19 + Indent * CommandBox.Indent, WidgetContainer.Position.Y);
            }
            HeaderLabel.SetText(this.CommandType.Name);
            HeaderLabel.SetVisible(this.CommandType.ShowHeader);
            dynamic basewidgets = this.CommandType.CallCreateReadOnly();
            HeaderLabel.SetTextColor(this.CommandType.HeaderColor);
            for (int i = 0; i < basewidgets.Count; i++)
            {
                dynamic widget = basewidgets[i];
                string type = widget.GetType().Name;
                Widget parent = null;
                if (widget.Parent != null)
                {
                    foreach (string parentwidgetid in DynamicReadOnlyWidgets.Keys)
                    {
                        if (parentwidgetid == widget.parent.UniqueID)
                        {
                            parent = DynamicReadOnlyWidgets[parentwidgetid];
                            break;
                        }
                    }
                }
                if (parent == null) parent = WidgetContainer;
                Widget w = ProcessWidgetType(widget, type, parent);
                ProcessWidget(w, widget, false, true);
                DynamicReadOnlyWidgets.Add(widget.UniqueID, w);
            }
            Reload();
        }

        public dynamic GenerateUtility()
        {
            return new CommandUtility((CommandBox) Parent.Parent.Parent, PageData.Commands, PageData.Commands.IndexOf(this.Command), this.Command.Parameters);
        }

        public void Reload()
        {
            dynamic widgets = this.CommandType.CallLoadReadOnly(GenerateUtility());
            for (int i = 0; i < widgets.Count; i++)
            {
                dynamic widget = widgets[i];
                ProcessWidget(DynamicReadOnlyWidgets[widget.UniqueID], widget, false, false);
            }
            HeaderLabel.SetVisible(this.CommandType.ShowHeader);
            HeaderLabel.SetTextColor(this.CommandType.HeaderColor);
            WidgetContainer.SetPosition(WidgetContainer.Position.X, this.CommandType.ShowHeader ? 20 : 3);
            int maxh = 0;
            foreach (Widget w in WidgetContainer.Widgets)
            {
                if (w.Position.Y + w.Size.Height > maxh) maxh = w.Position.Y + w.Size.Height;
            }
            WidgetContainer.SetHeight(maxh);
            this.SetHeight(WidgetContainer.Position.Y + WidgetContainer.Size.Height + 3);
            ((VStackPanel) Parent).UpdateLayout();
        }

        public Widget ProcessWidgetType(dynamic widget, string Type, Widget Parent)
        {
            switch (Type)
            {
                case "MultilineLabel":
                    return new MultilineDynamicLabel(Parent);
                case "Label":
                    return new DynamicLabel(Parent);
                case "MultilineTextBox":
                    return new MultilineTextBox(Parent);
                case "SwitchPicker":
                    return new GameSwitchBox(Parent);
                case "VariablePicker":
                    return new GameVariableBox(Parent);
                case "Dropdown":
                    return new DropdownBox(Parent);
                case "RadioButton":
                    return new RadioBox(Parent);
                case "TextBox":
                    return new TextBox(Parent);
                case "TabView":
                    return new TabView(Parent);
                case "Container":
                    if (widget.Parent != null)
                    {
                        string parentname = widget.Parent.GetType().Name;
                        string indexstring = widget.UniqueID.Substring(widget.UniqueID.IndexOf('.') + 1);
                        if (parentname == "TabView" && Utilities.IsNumeric(indexstring))
                        {
                            TabView tab = DynamicWindowWidgets[widget.Parent.UniqueID];
                            int index = Convert.ToInt32(indexstring);
                            return new Container(tab.Tabs[index]);
                        }
                    }
                    return new Container(Parent);
                case "List":
                    return new ListBox(Parent);
                case "Button":
                    return new Button(Parent);
                case "CheckBox":
                    return new CheckBox(Parent);
                case "ConditionBox":
                    return new ConditionBox(Parent);
                default:
                    throw new Exception($"Invalid widget type '{Type}'!");
            }
        }

        public void ProcessWidget(Widget w, dynamic widget, bool InWindow, bool IsNew)
        {
            w.SetPosition(widget.X, widget.Y);
            int width = widget.Width;
            int height = widget.Height;
            if (widget.Width == -1)
            {
                if (InWindow) width = PopupWindow.Size.Width;
                else width = WidgetContainer.Size.Width - w.Position.X;
            }
            if (widget.Height == -1)
            {
                if (InWindow) height = PopupWindow.Size.Height;
                else height = WidgetContainer.Size.Height - w.Position.Y;
            }
            w.SetSize(width, height);
            if (w is MultilineDynamicLabel)
            {
                ((MultilineDynamicLabel) w).SetColors(this.CommandType.TextColors);
                if (widget.Color != null) ((DynamicLabel) w).Colors[0] = ProcessColor(widget.Color);
                if (widget.Text != null) ((MultilineDynamicLabel) w).SetText(widget.Text);
                if (!widget.Parse) ((MultilineDynamicLabel) w).Parsing = widget.Parse;
                ((MultilineDynamicLabel) w).SetEnabled(widget.Enabled);
            }
            else if (w is DynamicLabel)
            {
                ((DynamicLabel) w).SetColors(this.CommandType.TextColors);
                if (widget.Color != null) ((DynamicLabel) w).Colors[0] = ProcessColor(widget.Color);
                if (widget.Text != null) ((DynamicLabel) w).SetText(widget.Text);
                ((DynamicLabel) w).SetEnabled(widget.Enabled);
            }
            else if (w is MultilineTextBox)
            {
                ((MultilineTextBox) w).SetText(widget.Text);
                if (widget.Index != -1) ((MultilineTextBox) w).SetCaretIndex(widget.Index);
                if (widget.Focus)
                {
                    Window.UI.SetSelectedWidget(((MultilineTextBox) w).TextArea);
                    ((MultilineTextBox) w).TextArea.OnWidgetSelected(new BaseEventArgs());
                }
                if (IsNew)
                {
                    ((MultilineTextBox) w).TextArea.OnTextChanged += delegate (BaseEventArgs e)
                    {
                        widget.Text = ((MultilineTextBox) w).Text;
                        InvokeCallback(widget.OnTextChanged, InWindow);
                    };
                }
                //((MultilineTextBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is TextBox)
            {
                ((TextBox) w).SetInitialText(widget.Text);
                if (widget.Index != -1) ((TextBox) w).SetCaretIndex(widget.Index);
                if (widget.Focus)
                {
                    Window.UI.SetSelectedWidget(((TextBox) w).TextArea);
                    ((TextBox) w).TextArea.OnWidgetSelected(new BaseEventArgs());
                }
                if (IsNew)
                {
                    ((TextBox) w).OnTextChanged += delegate (BaseEventArgs e)
                    {
                        widget.Text = ((TextBox) w).Text;
                        InvokeCallback(widget.OnTextChanged, InWindow);
                    };
                }
                ((TextBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is GameSwitchBox)
            {
                ((GameSwitchBox) w).GroupID = widget.GroupID;
                ((GameSwitchBox) w).SwitchID = widget.SwitchID;
                ((GameSwitchBox) w).ResetText();
                if (IsNew)
                {
                    ((GameSwitchBox) w).OnSwitchChanged += delegate (BaseEventArgs e)
                    {
                        widget.GroupID = ((GameSwitchBox) w).GroupID;
                        widget.SwitchID = ((GameSwitchBox) w).SwitchID;
                    };
                }
                ((GameSwitchBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is GameVariableBox)
            {
                ((GameVariableBox) w).GroupID = widget.GroupID;
                ((GameVariableBox) w).VariableID = widget.VariableID;
                ((GameVariableBox) w).ResetText();
                if (IsNew)
                {
                    ((GameVariableBox) w).OnVariableChanged += delegate (BaseEventArgs e)
                    {
                        widget.GroupID = ((GameVariableBox) w).GroupID;
                        widget.VariableID = ((GameVariableBox) w).VariableID;
                    };
                }
                ((GameVariableBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is DropdownBox)
            {
                List<ListItem> Items = new List<ListItem>();
                for (int i = 0; i < widget.Items.Count; i++) Items.Add(new ListItem(widget.Items[i]));
                ((DropdownBox) w).SetItems(Items);
                if (Items.Count > 0) ((DropdownBox) w).SetSelectedIndex(widget.Index);
                if (IsNew)
                {
                    ((DropdownBox) w).OnSelectionChanged += delegate (BaseEventArgs e)
                    {
                        widget.Index = ((DropdownBox) w).SelectedIndex;
                        InvokeCallback(widget.OnSelectionChanged, InWindow);
                    };
                }
                ((DropdownBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is RadioBox)
            {
                w.SetHeight(16);
                if (!string.IsNullOrEmpty(widget.Text)) ((RadioBox) w).SetText(widget.Text);
                ((RadioBox) w).SetChecked(widget.Selected);
                if (IsNew)
                {
                    ((RadioBox) w).OnCheckChanged += delegate (BaseEventArgs e)
                    {
                        widget.Selected = ((RadioBox) w).Checked;
                        InvokeCallback(widget.OnSelectionChanged, InWindow);
                    };
                }
                ((RadioBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is TabView)
            {
                if (IsNew)
                {
                    for (int i = 0; i < widget.TabContainers.Count; i++)
                    {
                        ((TabView) w).CreateTab(widget.TabNames[i]);
                    }
                }
                ((TabView) w).SetHeaderColor(new Color(59, 91, 124));
                ((TabView) w).SetXOffset(widget.XOffset);
            }
            else if (w is ListBox)
            {
                if (IsNew)
                {
                    ((ListBox) w).OnSelectionChanged += delegate (BaseEventArgs e)
                    {
                        widget.Index = ((ListBox) w).SelectedIndex;
                        InvokeCallback(widget.OnSelectionChanged, InWindow);
                    };
                }
                ((ListBox) w).SetSelectedIndex(widget.Index);
                List<ListItem> items = new List<ListItem>();
                for (int i = 0; i < widget.Items.Count; i++) items.Add(new ListItem(widget.Items[i]));
                ((ListBox) w).SetItems(items);
                ((ListBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is Button)
            {
                ((Button) w).SetText(widget.Text);
                if (IsNew)
                {
                    ((Button) w).OnClicked += delegate (BaseEventArgs e)
                    {
                        InvokeCallback(widget.OnPressed, InWindow);
                    };
                }
                ((Button) w).SetEnabled(widget.Enabled);
            }
            else if (w is CheckBox)
            {
                w.SetHeight(16);
                ((CheckBox) w).SetChecked(widget.Checked);
                ((CheckBox) w).SetText(widget.Text);
                if (IsNew)
                {
                    ((CheckBox) w).OnCheckChanged += delegate (BaseEventArgs e)
                    {
                        widget.Checked = ((CheckBox) w).Checked;
                        InvokeCallback(widget.OnCheckChanged, InWindow);
                    };
                }
                ((CheckBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is ConditionBox)
            {
                ((ConditionBox) w).SetEnabled(widget.Enabled);
                List<BasicCondition> conditions = new List<BasicCondition>();
                if (widget.Conditions != null) for (int i = 0; i < widget.Conditions.Count; i++) conditions.Add((BasicCondition) widget.Conditions[i]);
                ((ConditionBox) w).SetConditions(conditions);
            }
            else if (w is Container)
            {

            }
            w.SetVisible(widget.Visible);
        }

        public void InvokeCallback(dynamic Callback, bool InWindow)
        {
            if (Callback == null) return;
            dynamic widgets = Callback.Invoke();
            if (widgets == null) return;
            for (int i = 0; i < widgets.Count; i++)
            {
                Widget w = DynamicWindowWidgets[widgets[i].UniqueID];
                ProcessWidget(w, widgets[i], InWindow, false);
            }
        }

        public Color ProcessColor(dynamic Object)
        {
            return new Color(Object.Red, Object.Green, Object.Blue, Object.Alpha);
        }

        public void EditWindow(BaseEvent CallBack = null)
        {
            if (this.Command == null || !this.CommandType.IsEditable) return;
            PopupWindow = new PopupWindow();
            OldParameters = new Dictionary<string, object>(Command.Parameters);
            WindowUtility = GenerateUtility();
            dynamic basewidgets = this.CommandType.CallCreateWindow(WindowUtility);
            for (int i = 0; i < basewidgets.Count; i++)
            {
                dynamic widget = basewidgets[i];
                string type = widget.GetType().Name;
                Widget parent = null;
                if (widget.Parent != null)
                {
                    foreach (string parentwidgetid in DynamicWindowWidgets.Keys)
                    {
                        if (parentwidgetid == widget.Parent.UniqueID)
                        {
                            parent = DynamicWindowWidgets[parentwidgetid];
                            break;
                        }
                    }
                }
                if (parent == null) parent = PopupWindow;
                Widget w = ProcessWidgetType(widget, type, parent);
                ProcessWidget(w, widget, true, true);
                DynamicWindowWidgets.Add(widget.UniqueID, w);
                DynamicWindowWidgetObjects.Add(widget.UniqueID, widget);
            }
            int width = CommandType.WindowWidth;
            int height = CommandType.WindowHeight;
            PopupWindow.SetTitle(CommandType.Name);
            PopupWindow.MinimumSize = PopupWindow.MaximumSize = new Size(width, height);
            PopupWindow.SetSize(PopupWindow.MaximumSize);
            PopupWindow.CreateButton("Cancel", delegate (BaseEventArgs e)
            {
                CloseWindow();
            });
            PopupWindow.CreateButton("OK", delegate (BaseEventArgs e)
            {
                SaveWindow(WindowUtility);
                if (!WindowUtility.AwaitCloseFlag)
                {
                    CloseWindow();
                }
            });
            PopupWindow.Center();
            PopupWindow.OnClosed += delegate (BaseEventArgs e)
            {
                Reload();
                CallBack?.Invoke(e);
            };
        }

        public override void Update()
        {
            base.Update();
            if (PopupWindow != null && !PopupWindow.Disposed && WindowUtility != null)
            {
                if (WindowUtility.AwaitCloseFlag && WindowUtility.CanClose) CloseWindow();
                else
                {
                    foreach (dynamic widget in DynamicWindowWidgetObjects.Values)
                    {
                        Widget w = DynamicWindowWidgets[widget.UniqueID];
                        if (w is ConditionBox)
                        {
                            if (widget.AwaitOpen)
                            {
                                widget.AwaitOpen = false;
                                ((ConditionBox) w).Edit(delegate (BaseEventArgs e)
                                {
                                    widget.Conditions = ((ConditionBox) w).Conditions;
                                    InvokeCallback(widget.OnConditionsChanged, true);
                                });
                            }
                        }
                    }
                }
            }
        }

        public void SaveWindow(CommandUtility Utility)
        {
            this.CommandType.CallSaveWindow(Utility);
        }

        public void CloseWindow()
        {
            bool redrawall = WindowUtility.RedrawAll;
            PopupWindow.Close();
            foreach (Widget w in DynamicWindowWidgets.Values)
            {
                if (!w.Disposed) throw new Exception($"Window disposed, but one widget is not.");
            }
            DynamicWindowWidgets.Clear();
            DynamicWindowWidgetObjects.Clear();
            PopupWindow = null;
            WindowUtility = null;
            if (redrawall) ((CommandBox) Parent.Parent.Parent).RedrawCommands();
            else Reload();
        }

        public void SetSelected(bool Selected)
        {
            if (this.Selected != Selected)
            {
                Parent.Widgets.ForEach(w =>
                {
                    if (w != this) ((CommandAPIHandlerWidget) w).SetSelected(false);
                });
                this.Selected = Selected;
                if (Selected)
                {
                    ((CommandBox) Parent.Parent.Parent).UpdateHoverOrSelection(true);
                    ((CommandBox) Parent.Parent.Parent).OnSelectionChanged?.Invoke(new BaseEventArgs());
                }
            }
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            ((CommandBox) Parent.Parent.Parent).UpdateHoverOrSelection(false);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton)
            {
                if (!this.Selected)
                {
                    SetSelected(true);
                    if (TimerExists("double")) DestroyTimer("double");
                    SetTimer("double", 300);
                }
                else
                {
                    if (TimerExists("double") && !TimerPassed("double"))
                    {
                        ((CommandBox) Parent.Parent.Parent).OnDoubleClicked?.Invoke(new BaseEventArgs());
                    }
                    else if (TimerExists("double") && TimerPassed("double"))
                    {
                        ResetTimer("double");
                    }
                    else if (!TimerExists("double"))
                    {
                        SetTimer("double", 300);
                    }
                }
            }
            else if (TimerExists("double"))
            {
                DestroyTimer("double");
            }
        }
    }
}
