using System;
using System.Collections.Generic;
using System.Text;
using ODL;
using MKEditor.Game;
using System.Linq;

namespace MKEditor.Widgets
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
            this.EventData = EventData;
            this.PageData = PageData;
            while (StackPanel.Widgets.Count > 0) StackPanel.Widgets[0].Dispose();
            int OldIndent = 0;
            for (int i = 0; i < PageData.Commands.Count; i++)
            {
                BasicCommand cmd = PageData.Commands[i];
                bool emptysub = false;
                int emptyindex = 0;
                if (StackPanel.Widgets.Count > 0)
                {
                    CommandAPIHandlerWidget lastwidget = (CommandAPIHandlerWidget) StackPanel.Widgets.Last();
                    if (lastwidget.CommandType.IsSubBranch && cmd.Indent <= lastwidget.Indent)
                    {
                        emptysub = true;
                        emptyindex = lastwidget.Indent + 1;
                    }
                }
                if (emptysub)
                {
                    // Last command started a new branch, but it's empty
                    CommandAPIHandlerWidget emptyidt = new CommandAPIHandlerWidget(StackPanel);
                    emptyidt.SetWidth(Size.Width - 13);
                    emptyidt.SetCommand(PageData, null, emptyindex);
                }
                if (OldIndent > cmd.Indent)
                {
                    // Went from a higher indent to a lower indent (e.g. end of a branch)
                    // Then insert an empty command
                    CommandAPIHandlerWidget emptyidt = new CommandAPIHandlerWidget(StackPanel);
                    emptyidt.SetWidth(Size.Width - 13);
                    emptyidt.SetCommand(PageData, null, OldIndent);
                }
                CommandAPIHandlerWidget cew = new CommandAPIHandlerWidget(StackPanel);
                cew.SetWidth(Size.Width - 13);
                cew.SetCommand(PageData, cmd);
                OldIndent = cmd.Indent;
            }
            CommandAPIHandlerWidget empty = new CommandAPIHandlerWidget(StackPanel);
            empty.SetWidth(Size.Width - 13);
            empty.SetCommand(PageData, null, 0);
            UpdateList();
        }

        public void NewCommand(int Index, int Indent)
        {
            CommandPicker picker = new CommandPicker();
            picker.OnClosed += delegate (BaseEventArgs e)
            {
                if (picker.ChosenCommand != null)
                {
                    CommandAPIHandlerWidget cmdwidget = new CommandAPIHandlerWidget(StackPanel);
                    // Remove from last index and insert at intended index
                    StackPanel.Widgets.Remove(cmdwidget);
                    StackPanel.Widgets.Insert(Index, cmdwidget);
                    cmdwidget.SetWidth(Size.Width - 13);
                    CommandUtility Utility = new CommandUtility(PageData.Commands, Index, new Dictionary<string, object>());
                    picker.ChosenCommand.CallCreateBlank(Utility);
                    BasicCommand cmd = new BasicCommand(Indent, ":" + picker.ChosenCommand.Identifier, Utility.Parameters);
                    PageData.Commands.Insert(Index, cmd);
                    cmdwidget.SetCommand(PageData, cmd, Indent);
                    cmdwidget.EditWindow();
                    UpdateList();
                }
            };
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
                BranchSprite.Bitmap.SetPixel(8 + Indent * CommandBox.Indent, y, Color.WHITE);
                BranchSprite.Bitmap.SetPixel(9 + Indent * CommandBox.Indent, y, Color.WHITE);
            }
            BranchSprite.Bitmap.FillRect(7 + Indent * CommandBox.Indent, EndY - 2, 4, 2, Color.WHITE);
        }

        public void DrawBranchHorizontal(int Indent, int Y)
        {
            for (int x = 11 + Indent * CommandBox.Indent; x < 26 + Indent * CommandBox.Indent; x += 2)
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

        public void EditCommand()
        {
            CommandAPIHandlerWidget chw = (CommandAPIHandlerWidget) StackPanel.Widgets[SelectedIndex];
            if (chw.Command == null)
            {
                NewCommand(SelectedIndex, chw.Indent);
            }
            else chw.EditWindow();
        }
    }
}
