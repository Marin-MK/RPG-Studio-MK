using System;
using System.Collections.Generic;
using System.Text;
using odl;
using RPGStudioMK.Game;
using System.Linq;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class CommandBox : Widget
    {
        public static int Indent = 28;

        public Event EventData { get; protected set; }
        public EventPage PageData { get; protected set; }
        public int SelectedIndex
        {
            get
            {
                Widget w = StackPanel.Widgets.Find(w => ((CommandAPIHandlerWidget) w).Selected);
                return StackPanel.Widgets.IndexOf(w);
            }
        }

        Container MainContainer;
        VStackPanel StackPanel;

        public BaseEvent OnSelectionChanged;
        public BaseEvent OnDoubleClicked;

        public int HoverStartIndex = -1;
        public int HoverEndIndex = -1;

        public int SelectionStartIndex = -1;
        public int SelectionEndIndex = -1;

        protected Sprite SelSprite { get => (Sprite) StackPanel.Sprites["selection"]; }
        protected Sprite HoverSprite { get => (Sprite) StackPanel.Sprites["hover"]; }
        protected Sprite BranchSprite { get => (Sprite) StackPanel.Sprites["branches"]; }

        public CommandBox(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            MainContainer = new Container(this);
            MainContainer.SetPosition(1, 2);
            MainContainer.VAutoScroll = true;
            VScrollBar vs = new VScrollBar(this);
            vs.ScrollStep = 4;
            MainContainer.SetVScrollBar(vs);
            StackPanel = new VStackPanel(MainContainer);
            StackPanel.Sprites["selection"] = new Sprite(StackPanel.Viewport, new SolidBitmap(1, 1, new Color(28, 50, 73)));
            StackPanel.Sprites["selection"].Visible = false;
            StackPanel.Sprites["hover"] = new Sprite(StackPanel.Viewport, new SolidBitmap(2, 1, new Color(55, 187, 255)));
            StackPanel.Sprites["hover"].Visible = false;
            StackPanel.Sprites["branches"] = new Sprite(StackPanel.Viewport);
            this.OnDoubleClicked += delegate (BaseEventArgs e)
            {
                EditCommand();
            };

            OnWidgetSelected += WidgetSelected;

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.DELETE), delegate (BaseEventArgs e) { DeleteCommand(); }),
                new Shortcut(this, new Key(Keycode.UP), delegate (BaseEventArgs e) { MoveUp(); }),
                new Shortcut(this, new Key(Keycode.DOWN), delegate (BaseEventArgs e) { MoveDown(); })
            });
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            Sprites["bg"].Bitmap?.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(Size, 86, 108, 134);
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, 10, 23, 37);
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(1, 1, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.Lock();
            MainContainer.SetSize(Size.Width - 13, Size.Height - 4);
            StackPanel.SetWidth(MainContainer.Size.Width);
            MainContainer.VScrollBar.SetPosition(Size.Width - 10, 2);
            MainContainer.VScrollBar.SetSize(8, Size.Height - 4);
            MainContainer.UpdateAutoScroll();
        }

        public void SetEventPage(Event EventData, EventPage PageData)
        {
            int OldScroll = MainContainer.ScrolledY;
            this.EventData = EventData;
            this.PageData = PageData;
            while (StackPanel.Widgets.Count > 0) StackPanel.Widgets[0].Dispose();
            int OldIndent = 0;
            for (int i = 0; i < PageData.Commands.Count; i++)
            {
                BasicCommand cmd = PageData.Commands[i];
                if (StackPanel.Widgets.Count > 0)
                {
                    CommandAPIHandlerWidget lastwidget = (CommandAPIHandlerWidget) StackPanel.Widgets.Last();
                    if (lastwidget.CommandType.IsSubBranch && cmd.Indent <= lastwidget.Indent)
                    {
                        // Last command started a new branch, but it's empty
                        InsertEmptyCommand(lastwidget.Indent + 1);
                    }
                }
                if (OldIndent > cmd.Indent)
                {
                    // Went from a higher indent to a lower indent (e.g. end of a branch)
                    // Then insert an empty command
                    InsertEmptyCommand(OldIndent);
                }
                CommandAPIHandlerWidget cew = new CommandAPIHandlerWidget(StackPanel);
                cew.SetWidth(Size.Width - 13);
                cew.SetCommand(PageData, cmd);
                OldIndent = cmd.Indent;
            }
            if (StackPanel.Widgets.Count > 0)
            {
                // Last command started a new branch, but it's empty (and we're through the whole list, so the earlier
                // check isn't fired)
                CommandAPIHandlerWidget widget = (CommandAPIHandlerWidget) StackPanel.Widgets.Last();
                if (widget.Command != null && widget.CommandType.IsSubBranch) InsertEmptyCommand(widget.Indent + 1);
                for (int i = widget.Indent; i > 0; i--)
                {
                    InsertEmptyCommand(i);
                }
            }
            InsertEmptyCommand(0);
            UpdateList();
            MainContainer.ScrolledY = OldScroll;
            MainContainer.UpdateAutoScroll();
        }

        public void RedrawCommands()
        {
            BasicCommand selectedcommand = null;
            foreach (CommandAPIHandlerWidget widget in StackPanel.Widgets)
            {
                if (widget.Selected)
                {
                    selectedcommand = widget.Command;
                    break;
                }
            }
            SetEventPage(EventData, PageData);
            foreach (CommandAPIHandlerWidget widget in StackPanel.Widgets)
            {
                if (widget.Command == selectedcommand)
                {
                    widget.SetSelected(true);
                    break;
                }
            }
            UpdateHoverOrSelection(true);
            UpdateHoverOrSelection(false);
        }

        public void InsertEmptyCommand(int Indent)
        {
            CommandAPIHandlerWidget w = new CommandAPIHandlerWidget(StackPanel);
            w.SetWidth(Size.Width - 13);
            w.SetCommand(PageData, null, Indent);
        }

        public void NewCommand(int WidgetIndex, int CommandIndex, int Indent)
        {
            CommandPicker picker = new CommandPicker();
            picker.OnClosed += delegate (BaseEventArgs e)
            {
                if (picker.ChosenCommand != null)
                {
                    CommandAPIHandlerWidget cmdwidget = new CommandAPIHandlerWidget(StackPanel);
                    // Remove from last index and insert at intended index
                    StackPanel.Widgets.Remove(cmdwidget);
                    StackPanel.Widgets.Insert(WidgetIndex, cmdwidget);
                    cmdwidget.SetWidth(Size.Width - 13);
                    CommandUtility Utility = new CommandUtility(this, PageData.Commands, CommandIndex, new Dictionary<string, object>());
                    picker.ChosenCommand.CallCreateBlank(Utility);
                    BasicCommand cmd = new BasicCommand(Indent, ":" + picker.ChosenCommand.Identifier, Utility.Parameters);
                    PageData.Commands.Insert(CommandIndex, cmd);
                    cmdwidget.SetCommand(PageData, cmd, Indent);
                    cmdwidget.SetSelected(true);
                    cmdwidget.EditWindow(delegate (BaseEventArgs e)
                    {
                        SelectionStartIndex = WidgetIndex;
                        SelectionEndIndex = WidgetIndex;
                        this.Window.UI.SetSelectedWidget(this);
                        this.OnWidgetSelected(new BaseEventArgs());
                    });
                }
            };
        }

        public void EditCommand()
        {
            CommandAPIHandlerWidget chw = (CommandAPIHandlerWidget) StackPanel.Widgets[SelectedIndex];
            if (chw.Command == null)
            {
                int idx = -1;
                for (int i = SelectedIndex; i >= 0; i--)
                {
                    CommandAPIHandlerWidget w = (CommandAPIHandlerWidget) StackPanel.Widgets[i];
                    if (w.Command != null)
                    {
                        idx = PageData.Commands.IndexOf(w.Command) + 1;
                        break;
                    }
                }
                NewCommand(SelectedIndex, idx == -1 ? 0 : idx, chw.Indent);
            }
            else chw.EditWindow();
        }

        public DynamicCommandType GetCommandType(BasicCommand Command)
        {
            foreach (CommandAPIHandlerWidget cmdwidget in StackPanel.Widgets)
            {
                if (cmdwidget.Command == Command) return cmdwidget.CommandType;
            }
            return CommandPlugins.CommandTypes.Find(t => ":" + t.Identifier == Command.Identifier);
        }

        public void DeleteCommand()
        {
            if (SelectionStartIndex == -1 || SelectionEndIndex == -1 || StackPanel.Widgets.Count == 1) return;
            CommandAPIHandlerWidget mainwidget = (CommandAPIHandlerWidget) StackPanel.Widgets[SelectionStartIndex];
            if (mainwidget.Command == null || !mainwidget.CommandType.IsDeletable) return;
            List<Widget> Widgets = StackPanel.Widgets.GetRange(SelectionStartIndex, SelectionEndIndex - SelectionStartIndex + 1);
            bool RemovedSomething = false;
            Widgets.ForEach(widget =>
            {
                CommandAPIHandlerWidget w = (CommandAPIHandlerWidget) widget;
                if (w.Command != null)
                {
                    PageData.Commands.Remove(w.Command);
                    RemovedSomething = true;
                }
            });
            if (RemovedSomething)
            {
                if (SelectionStartIndex >= StackPanel.Widgets.Count - 1) SelectionStartIndex -= 1;
                RedrawCommands();
                if (SelectionStartIndex > 0)
                {
                    if (((CommandAPIHandlerWidget) StackPanel.Widgets[SelectionStartIndex]).Command == null) SelectionStartIndex -= 1;
                }
                ((CommandAPIHandlerWidget) StackPanel.Widgets[SelectionStartIndex]).SetSelected(true);
            }
        }

        public void MoveUp()
        {
            if (SelectionStartIndex <= 0) return;
            StackPanel.Widgets.ForEach(w => ((CommandAPIHandlerWidget) w).SetSelected(false));
            SelectionStartIndex -= 1;
            ((CommandAPIHandlerWidget) StackPanel.Widgets[SelectionStartIndex]).SetSelected(true);
        }

        public void MoveDown()
        {
            if (SelectionStartIndex >= StackPanel.Widgets.Count - 1) return;
            StackPanel.Widgets.ForEach(w => ((CommandAPIHandlerWidget) w).SetSelected(false));
            SelectionStartIndex += 1;
            ((CommandAPIHandlerWidget) StackPanel.Widgets[SelectionStartIndex]).SetSelected(true);
        }

        public void UpdateList()
        {
            StackPanel.UpdateLayout();
            HoverSprite.Visible = false;
            SelSprite.Visible = false;
            BranchSprite.Bitmap?.Dispose();
            BranchSprite.Bitmap = new Bitmap(StackPanel.Size);
            BranchSprite.Bitmap.Unlock();
            int SelectionStartY = -1;
            int HoverStartY = -1;
            for (int i = 0; i < StackPanel.Widgets.Count; i++)
            {
                CommandAPIHandlerWidget cmdwidget = (CommandAPIHandlerWidget) StackPanel.Widgets[i];
                if (i == SelectionStartIndex) SelectionStartY = cmdwidget.Position.Y;
                if (i == HoverStartIndex) HoverStartY = cmdwidget.Position.Y;
                if (i == SelectionEndIndex) DrawSelection(SelectionStartY, cmdwidget.Position.Y + cmdwidget.Size.Height);
                if (i == HoverEndIndex) DrawHover(HoverStartY, cmdwidget.Position.Y + cmdwidget.Size.Height);
                if (cmdwidget.CommandType == null) continue;
                if (cmdwidget.CommandType.HasBranches)
                {
                    for (int j = i + 1; j < StackPanel.Widgets.Count; j++)
                    {
                        CommandAPIHandlerWidget w = (CommandAPIHandlerWidget) StackPanel.Widgets[j];
                        if ((w.Command == null || ":" + cmdwidget.CommandType.BranchIdentifier != w.Command.Identifier) && cmdwidget.Indent == w.Indent ||
                            w.Command != null && ":" + cmdwidget.CommandType.BranchIdentifier == w.Command.Identifier && cmdwidget.Indent > w.Indent)
                        {
                            DrawBranchVertical(cmdwidget.Indent, cmdwidget.Position.Y + 14, w.Position.Y - 6);
                            break;
                        }
                    }
                }
                else if (cmdwidget.CommandType.IsSubBranch)
                {
                    DrawBranchHorizontal(cmdwidget.Indent, cmdwidget.Position.Y + 12);
                }
            }
            BranchSprite.Bitmap.Lock();
        }

        public void DrawBranchVertical(int Indent, int StartY, int EndY)
        {
            for (int y = StartY; y < EndY - 2; y += 2)
            {
                BranchSprite.Bitmap.SetPixel(7 + Indent * CommandBox.Indent, y, Color.WHITE);
                BranchSprite.Bitmap.SetPixel(8 + Indent * CommandBox.Indent, y, Color.WHITE);
            }
            BranchSprite.Bitmap.FillRect(6 + Indent * CommandBox.Indent, EndY - 2, 4, 2, Color.WHITE);
        }

        public void DrawBranchHorizontal(int Indent, int Y)
        {
            for (int x = 10 + Indent * CommandBox.Indent; x < 25 + Indent * CommandBox.Indent; x += 2)
            {
                BranchSprite.Bitmap.SetPixel(x, Y, Color.WHITE);
                BranchSprite.Bitmap.SetPixel(x, Y + 1, Color.WHITE);
            }
        }

        public void DrawSelection(int StartY, int EndY)
        {
            ((SolidBitmap) SelSprite.Bitmap).SetSize(Size.Width - 13, EndY - StartY);
            SelSprite.Y = StartY;
            SelSprite.Visible = true;
        }

        public void DrawHover(int StartY, int EndY)
        {
            ((SolidBitmap) HoverSprite.Bitmap).SetSize(2, EndY - StartY);
            HoverSprite.Y = StartY;
            HoverSprite.Visible = true;
        }

        public void UpdateHoverOrSelection(bool Selection)
        {
            if (Selection)
            {
                this.SelectionStartIndex = -1;
                this.SelectionEndIndex = -1;
            }
            else
            {
                this.HoverStartIndex = -1;
                this.HoverEndIndex = -1;
            }
            for (int i = 0; i < StackPanel.Widgets.Count; i++)
            {
                CommandAPIHandlerWidget cmdwidget = (CommandAPIHandlerWidget) StackPanel.Widgets[i];
                if (cmdwidget.Selected && Selection || cmdwidget.WidgetIM.Hovering && !Selection)
                {
                    if (Selection) this.SelectionStartIndex = i;
                    else this.HoverStartIndex = i;
                    if (cmdwidget.Command == null) break;
                    if (cmdwidget.CommandType.HasBranches)
                    {
                        for (int j = i + 1; j < StackPanel.Widgets.Count; j++)
                        {
                            CommandAPIHandlerWidget w = (CommandAPIHandlerWidget) StackPanel.Widgets[j];
                            if (cmdwidget.Indent == w.Indent && (w.Command == null || !w.CommandType.IsSubBranch))
                            {
                                if (Selection) this.SelectionEndIndex = j - 1;
                                else this.HoverEndIndex = j - 1;
                                break;
                            }
                        }
                    }
                    else if (cmdwidget.CommandType.IsSubBranch)
                    {
                        for (int j = i + 1; j < StackPanel.Widgets.Count; j++)
                        {
                            CommandAPIHandlerWidget w = (CommandAPIHandlerWidget) StackPanel.Widgets[j];
                            if (w.Indent <= cmdwidget.Indent)
                            {
                                if (Selection) this.SelectionEndIndex = j - 1;
                                else this.HoverEndIndex = j - 1;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            if (Selection && this.SelectionStartIndex != -1 && this.SelectionEndIndex == -1) this.SelectionEndIndex = this.SelectionStartIndex;
            else if (!Selection && this.HoverStartIndex != -1 && this.HoverEndIndex == -1) this.HoverEndIndex = this.HoverStartIndex;
            UpdateList();
        }
    }
}
