using MKEditor.Game;
using System;
using System.Collections.Generic;
using System.Text;
using ODL;
using System.Linq;

namespace MKEditor.Widgets
{
    public class CommandAPIHandlerWidget : Widget
    {
        public BasicCommand Command;
        public bool Selected { get; protected set; } = false;
        public Dictionary<string, Widget> DynamicReadOnlyWidgets = new Dictionary<string, Widget>();
        public Dictionary<string, Widget> DynamicWindowWidgets = new Dictionary<string, Widget>();
        public Dictionary<string, dynamic> DynamicWindowWidgetObjects = new Dictionary<string, dynamic>();

        Label HeaderLabel;
        Container WidgetContainer;
        PopupWindow PopupWindow;

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

            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, this.Size.Height, new Color(55, 187, 255)));
            Sprites["hover"].Visible = false;

            HeaderLabel = new Label(this);
            HeaderLabel.SetPosition(19, 2);

            WidgetContainer = new Container(this);
            WidgetContainer.SetPosition(19, 20);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            WidgetContainer.SetWidth(Size.Width);
            ((SolidBitmap) Sprites["hover"].Bitmap).SetSize(2, Size.Height);
        }

        public void SetCommand(BasicCommand Command)
        {
            this.Command = Command;
            this.Sprites["bullet"].X = Command.Indent * 24 + 6;
            this.Sprites["bullet"].Y = 9;
            ((SolidBitmap) this.Sprites["hover"].Bitmap).SetSize(2, this.Size.Height);
            this.CommandType = CommandPlugins.CommandTypes.Find(t => t.Identifier == Command.Identifier.Substring(1)).EmptyClone();
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
                Widget w = ProcessWidgetType(type, parent);
                ProcessWidget(w, widget, false);
                DynamicReadOnlyWidgets.Add(widget.UniqueID, w);
            }
            Reload();
        }

        public void Reload()
        {
            dynamic widgets = this.CommandType.CallLoadReadOnly(new CommandUtility(Command.Parameters));
            for (int i = 0; i < widgets.Count; i++)
            {
                dynamic widget = widgets[i];
                ProcessWidget(DynamicReadOnlyWidgets[widget.UniqueID], widget, false);
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

        public Widget ProcessWidgetType(string Type, Widget Parent)
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
                case "Dropdown":
                    return new DropdownBox(Parent);
                case "RadioButton":
                    return new RadioBox(Parent);
                case "TextBox":
                    return new TextBox(Parent);
                default:
                    throw new Exception($"Invalid widget type '{Type}'!");
            }
        }

        public void ProcessWidget(Widget w, dynamic widget, bool InWindow)
        {
            w.SetPosition(widget.X, widget.Y);
            w.SetSize(
                widget.Width == -1 ? (InWindow ? PopupWindow.Size.Width : WidgetContainer.Size.Width) : widget.Width,
                widget.Height == -1 ? (InWindow ? PopupWindow.Size.Height : WidgetContainer.Size.Height) : widget.Height
            );
            if (w is MultilineDynamicLabel)
            {
                ((MultilineDynamicLabel) w).SetColors(this.CommandType.TextColors);
                if (!string.IsNullOrEmpty(widget.Text)) ((MultilineDynamicLabel) w).SetText(widget.Text);
                if (!widget.Parse) ((MultilineDynamicLabel) w).Parsing = widget.Parse;
                ((MultilineDynamicLabel) w).SetEnabled(widget.Enabled);
            }
            else if (w is DynamicLabel)
            {
                ((DynamicLabel) w).SetColors(this.CommandType.TextColors);
                if (!string.IsNullOrEmpty(widget.Text)) ((DynamicLabel) w).SetText(widget.Text);
                ((DynamicLabel) w).SetEnabled(widget.Enabled);
            }
            else if (w is MultilineTextBox)
            {
                if (!string.IsNullOrEmpty(widget.Text)) ((MultilineTextBox) w).SetText(widget.Text);
                if (widget.Index != -1) ((MultilineTextBox) w).SetCaretIndex(widget.Index);
                if (widget.Focus)
                {
                    Window.UI.SetSelectedWidget(((MultilineTextBox) w).TextArea);
                    ((MultilineTextBox) w).TextArea.OnWidgetSelected(new BaseEventArgs());
                }
                ((MultilineTextBox) w).TextArea.OnTextChanged += delegate (BaseEventArgs e)
                {
                    widget.Text = ((MultilineTextBox) w).Text;
                };
                //((MultilineTextBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is TextBox)
            {
                if (!string.IsNullOrEmpty(widget.Text)) ((TextBox) w).SetInitialText(widget.Text);
                if (widget.Index != -1) ((TextBox) w).SetCaretIndex(widget.Index);
                if (widget.Focus)
                {
                    Window.UI.SetSelectedWidget(((TextBox) w).TextArea);
                    ((TextBox) w).TextArea.OnWidgetSelected(new BaseEventArgs());
                }
                ((TextBox) w).OnTextChanged += delegate (BaseEventArgs e)
                {
                    widget.Text = ((TextBox) w).Text;
                };
                ((TextBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is GameSwitchBox)
            {
                ((GameSwitchBox) w).GroupID = widget.GroupID;
                ((GameSwitchBox) w).SwitchID = widget.SwitchID;
                ((GameSwitchBox) w).ResetText();
                ((GameSwitchBox) w).OnSwitchChanged += delegate (BaseEventArgs e)
                {
                    widget.GroupID = ((GameSwitchBox) w).GroupID;
                    widget.SwitchID = ((GameSwitchBox) w).SwitchID;
                };
                ((GameSwitchBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is DropdownBox)
            {
                List<ListItem> Items = new List<ListItem>();
                for (int i = 0; i < widget.Items.Count; i++) Items.Add(new ListItem(widget.Items[i]));
                ((DropdownBox) w).SetItems(Items);
                if (Items.Count > 0) ((DropdownBox) w).SetSelectedIndex(widget.Index);
                ((DropdownBox) w).OnSelectionChanged += delegate (BaseEventArgs e)
                {
                    widget.Index = ((DropdownBox) w).SelectedIndex;
                };
                ((DropdownBox) w).SetEnabled(widget.Enabled);
            }
            else if (w is RadioBox)
            {
                w.SetHeight(16);
                if (!string.IsNullOrEmpty(widget.Text)) ((RadioBox) w).SetText(widget.Text);
                ((RadioBox) w).SetChecked(widget.Selected);
                ((RadioBox) w).OnCheckChanged += delegate (BaseEventArgs e)
                {
                    widget.Selected = ((RadioBox) w).Checked;
                    if (((RadioBox) w).Checked && widget.OnSelected != null)
                    {
                        dynamic widgets = widget.OnSelected?.Invoke();
                        for (int i = 0; i < widgets.Count; i++)
                        {
                            Widget w = DynamicWindowWidgets[widgets[i].UniqueID];
                            ProcessWidget(w, widgets[i], InWindow);
                        }
                    }
                };
                ((RadioBox) w).SetEnabled(widget.Enabled);
            }
        }

        public void EditWindow()
        {
            PopupWindow = new PopupWindow();
            OldParameters = new Dictionary<string, object>(Command.Parameters);
            dynamic basewidgets = this.CommandType.CallCreateWindow(new CommandUtility(Command.Parameters));
            for (int i = 0; i < basewidgets.Count; i++)
            {
                dynamic widget = basewidgets[i];
                string type = widget.GetType().Name;
                Widget parent = null;
                if (widget.Parent != null)
                {
                    foreach (string parentwidgetid in DynamicWindowWidgets.Keys)
                    {
                        if (parentwidgetid == widget.parent.UniqueID)
                        {
                            parent = DynamicWindowWidgets[parentwidgetid];
                            break;
                        }
                    }
                }
                if (parent == null) parent = PopupWindow;
                Widget w = ProcessWidgetType(type, parent);
                ProcessWidget(w, widget, true);
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
                SaveWindow();
                CloseWindow();
                Reload();
            });
            PopupWindow.Center();
        }

        public void SaveWindow()
        {
            this.CommandType.CallSaveWindow(new CommandUtility(Command.Parameters));
        }

        public void CloseWindow()
        {
            PopupWindow.Close();
            foreach (Widget w in DynamicWindowWidgets.Values)
            {
                if (!w.Disposed) throw new Exception($"Window disposed, but one widget is not.");
            }
            DynamicWindowWidgets.Clear();
            DynamicWindowWidgetObjects.Clear();
            Reload();
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
                SetBackgroundColor(this.Selected ? new Color(28, 50, 73) : Color.ALPHA);
                Sprites["hover"].Visible = this.WidgetIM.Hovering;
                if (Selected) ((CommandBox) Parent.Parent.Parent).OnSelectionChanged?.Invoke(new BaseEventArgs());
            }
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            Sprites["hover"].Visible = this.WidgetIM.Hovering;
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
