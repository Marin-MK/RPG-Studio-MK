using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets.CommandWidgets;

namespace RPGStudioMK.Widgets;

public class BaseCommandWidget : Widget
{
    static Dictionary<CommandCode, CommandCode> CommandStartEndPairs = new Dictionary<CommandCode, CommandCode>()
    {
        { CommandCode.ConditionalBranch, CommandCode.BranchConditionalEnd },
        { CommandCode.ShowChoices, CommandCode.BranchEnd }
    };

    static Dictionary<CommandCode, CommandCode> MergeSubsequentCommandPairs = new Dictionary<CommandCode, CommandCode>()
    {
        { CommandCode.ShowText, CommandCode.MoreText },
        { CommandCode.Script, CommandCode.MoreScript },
        { CommandCode.Comment, CommandCode.MoreComment },
        { CommandCode.SetMoveRoute, CommandCode.MoreMoveRoute }
    };

    static Dictionary<CommandCode, System.Type> CommandWidgetLookup = new Dictionary<CommandCode, System.Type>()
    {
        { CommandCode.Blank, typeof(BlankWidget) },
        { CommandCode.ConditionalBranch, typeof(ConditionalWidget) },
        { CommandCode.ShowChoices, typeof(ChoiceWidget) },
        { CommandCode.Comment, typeof(CommandWidgets.TextWidget) },
        { CommandCode.Script, typeof(CommandWidgets.TextWidget) },
        { CommandCode.ShowText, typeof(CommandWidgets.TextWidget) },
        { CommandCode.SetMoveRoute, typeof(MoveRouteWidget) },
        { CommandCode.WaitForMoveCompletion, typeof(WaitForMoveCompletionWidget) },
        { CommandCode.Wait, typeof(WaitWidget) },
        { CommandCode.ControlSwitches, typeof(SetSwitchWidget) },
        { CommandCode.ControlVariables, typeof(SetVariableWidget) }
    };

    protected delegate void EditEvent(bool Applied = true, bool ResetCommand = false, int GlobalIndexToCountFrom = -1);

    protected List<CommandUndoAction> UndoList = new List<CommandUndoAction>();
    protected List<CommandUndoAction> RedoList = new List<CommandUndoAction>();
    protected Map Map;
    protected Event Event;
    protected EventPage Page;
    protected EventCommand? Command;
    protected List<EventCommand> Commands;
    protected int Indentation;
    protected int HeightAdd = 0;
    protected int MarginBetweenWidgets = 2;
    protected int ChildIndent = 20;
    protected bool DrawEndLabels = true;
    protected bool Selected = false;
    protected int StandardHeight = 20;
    protected int GlobalCommandIndex = -1;
    protected bool InitialDownKey = true;
    protected bool InitialUpKey = true;
    protected bool InitialPageDownKey = true;
    protected bool InitialPageUpKey = true;
    protected bool InitialDeleteKey = true;
    protected bool InitialUndoKey = true;
    protected bool InitialRedoKey = true;

    protected Label HeaderLabel;
    protected VStackPanel VStackPanel;
    protected BaseCommandWidget MainCommandWidget;

    List<BaseCommandWidget> SubcommandWidgets = new List<BaseCommandWidget>();

    public BaseCommandWidget(IContainer Parent, int ParentWidgetIndex, Color BarColor = null) : base(Parent, ParentWidgetIndex)
    {
        HeaderLabel = new Label(this);
        HeaderLabel.SetPosition(8, 2);
        HeaderLabel.SetFont(Fonts.UbuntuBold.Use(10));
        if (BarColor != null) HeaderLabel.SetTextColor(BarColor);
        VStackPanel = new VStackPanel(this);
        VStackPanel.SetHDocked(true);
        VStackPanel.OnSizeChanged += _ => UpdateHeight();
        Sprites["bar"] = new Sprite(this.Viewport, new SolidBitmap(4, StandardHeight, BarColor ?? new Color(63, 210, 101)));
        OnSizeChanged += _ => ((SolidBitmap) Sprites["bar"].Bitmap).SetSize(4, Size.Height);
        OnDoubleLeftMouseDownInside += e =>
        {
            Insert();
            e.Handled = true;
        };
        OnContextMenuOpening += e =>
        {
            if (this.Indentation != -1 && !InsideChild())
            {
                e.Value = true;
                SetSelected(true);
            }
            else e.Value = false;
        };
        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER), _ => Insert(), false),
            new Shortcut(this, new Key(Keycode.SPACE), _ => BaseEdit(), false, e => e.Value = IsEditable()),
            new Shortcut(this, new Key(Keycode.X, Keycode.CTRL), _ => Cut(), false, e => e.Value = this is not BlankWidget),
            new Shortcut(this, new Key(Keycode.C, Keycode.CTRL), _ => Copy(), false, e => e.Value = this is not BlankWidget),
            new Shortcut(this, new Key(Keycode.V, Keycode.CTRL), _ => Paste(), false, e => e.Value = Utilities.IsClipboardValidBinary(BinaryData.EVENT_COMMANDS)),
            new Shortcut(this, new Key(Keycode.DELETE), _ => Delete(), false, e => e.Value = this is not BlankWidget),
            new Shortcut(this, new Key(Keycode.DOWN), _ => SelectNextCommand()),
            new Shortcut(this, new Key(Keycode.UP), _ => SelectPreviousCommand()),
            new Shortcut(this, new Key(Keycode.PAGEDOWN), _ => SelectPageDown()),
            new Shortcut(this, new Key(Keycode.PAGEUP), _ => SelectPageUp()),
            new Shortcut(this, new Key(Keycode.Z, Keycode.CTRL), _ => MainCommandWidget.Undo()),
            new Shortcut(this, new Key(Keycode.Y, Keycode.CTRL), _ => MainCommandWidget.Redo())
        });
        SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("Insert")
            {
                OnClicked = _ => Insert(),
                Shortcut = "Enter"
            },
            new MenuItem("Edit")
            {
                IsClickable = e => e.Value = IsEditable(),
                OnClicked = _ => BaseEdit(),
                Shortcut = "Space"
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = this is not BlankWidget,
                OnClicked = _ => Cut(),
                Shortcut = "Ctrl+X"
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = this is not BlankWidget,
                OnClicked = _ => Copy(),
                Shortcut = "Ctrl+C"
            },
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = Utilities.IsClipboardValidBinary(BinaryData.EVENT_COMMANDS),
                OnClicked = _ => Paste(),
                Shortcut = "Ctrl+V"
            },
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = this is not BlankWidget,
                OnClicked = _ => Delete(),
                Shortcut = "Del"
            }
        });
    }

    public void SetCommand(Map Map, Event Event, EventPage Page, EventCommand? Command, List<EventCommand> Commands, int Indentation, int GlobalCommandIndex)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        this.Command = Command;
        this.Commands = Commands;
        this.Indentation = Indentation;
        this.GlobalCommandIndex = GlobalCommandIndex;
        if (this.Indentation == -1) MainCommandWidget = this;
        SubcommandWidgets.ForEach(w => w.Dispose());
        SubcommandWidgets.Clear();
        if (Command != null)
        {
            HeaderLabel.SetText(Command.Code.ToString());
            HeaderLabel.SetVisible(true);
            VStackPanel.SetPadding(ChildIndent, StandardHeight, 0, 0);
            LoadCommand();
        }
        else
        {
            UndoList.Clear();
            RedoList.Clear();
            ChildIndent = 0;
            HeaderLabel.SetVisible(false);
            Sprites["bar"].Visible = false;
            VStackPanel.SetPadding(0);
            ParseCommands(Commands, VStackPanel, this.GlobalCommandIndex + 1);
        }
        UpdateHeight();
    }

    public (List<CommandUndoAction> UndoList, List<CommandUndoAction> RedoList) GetUndoRedoLists()
    {
        return (new List<CommandUndoAction>(UndoList), new List<CommandUndoAction>(RedoList));
    }

    public void SetUndoRedoLists(List<CommandUndoAction> UndoList, List<CommandUndoAction> RedoList)
    {
        this.UndoList = UndoList;
        this.RedoList = RedoList;
    }

    protected virtual void UpdateHeight()
    {
        VStackPanel.UpdateLayout();
        int vh = VStackPanel.Size.Height;
        if (vh == 1) // No subcommands in VStackPanel
        {
            // Remove the one excess pixel we'd get
            vh = 0;
        }
        if (this.Indentation == -1) SetHeight(Math.Max(vh + HeightAdd, Parent.Size.Height));
        else SetHeight(StandardHeight + vh + HeightAdd);
    }

    public virtual void LoadCommand()
    {
        
    }

    protected BaseCommandWidget CreateWidget(EventCommand Command, int Count, VStackPanel Parent, int GlobalCommandIndex, int ParentWidgetIndex = -1)
    {
        BaseCommandWidget w = null;
        IContainer ParentContainer = Parent ?? VStackPanel;
        if (CommandWidgetLookup.ContainsKey(Command.Code))
        {
            System.Type type = CommandWidgetLookup[Command.Code];
            w = (BaseCommandWidget) Activator.CreateInstance(type, new object?[] { ParentContainer, ParentWidgetIndex });
        }
        else w = new BaseCommandWidget(ParentContainer, ParentWidgetIndex);
        w.MainCommandWidget = this.MainCommandWidget;
        w.SetHDocked(true);
        w.SetMargins(0, MarginBetweenWidgets);
        w.SetCommand(Map, Event, Page, Command, Commands.GetRange(Commands.IndexOf(Command), Count), this.Indentation + 1, GlobalCommandIndex);
        w.OnSizeChanged += _ =>
        {
            ((VStackPanel) w.Parent).UpdateLayout();
        };
        SubcommandWidgets.Insert(ParentWidgetIndex == -1 ? SubcommandWidgets.Count : ParentWidgetIndex, w);
        return w;
    }

    protected void ParseCommands(List<EventCommand> Commands, VStackPanel Parent, int GlobalCommandStartIndex)
    {
        EventCommand? BranchCmd = null;
        int BranchCmdIdx = -1;

        EventCommand? MergeCmd = null;
        int MergeCmdIdx = -1;

        int gcmd = GlobalCommandStartIndex;

        for (int i = 0; i < Commands.Count; i++)
        {
            EventCommand cmd = Commands[i];
            if (MergeCmd != null)
            {
                if (i + 1 >= Commands.Count || Commands[i + 1].Code != MergeSubsequentCommandPairs[MergeCmd.Code])
                {
                    CreateWidget(MergeCmd, i - MergeCmdIdx + 1, Parent, gcmd);
                    gcmd += i - MergeCmdIdx + 1;
                    MergeCmd = null;
                    MergeCmdIdx = -1;
                }
            }
            else if (BranchCmd == null)
            {
                if (CommandStartEndPairs.ContainsKey(cmd.Code))
                {
                    BranchCmd = cmd;
                    BranchCmdIdx = i;
                }
                else
                {
                    if (MergeSubsequentCommandPairs.ContainsKey(cmd.Code))
                    {
                        if (i + 1 < Commands.Count && Commands[i + 1].Code == MergeSubsequentCommandPairs[cmd.Code])
                        {
                            MergeCmd = cmd;
                            MergeCmdIdx = i;
                            continue;
                        }
                    }
                    CreateWidget(cmd, 1, Parent, gcmd);
                    gcmd += 1;
                }
            }
            else if (BranchCmd.Indent == cmd.Indent && cmd.Code == CommandStartEndPairs[BranchCmd.Code])
            {
                CreateWidget(BranchCmd, i - BranchCmdIdx + 1, Parent, gcmd);
                gcmd += i - BranchCmdIdx + 1;
                BranchCmd = null;
                BranchCmdIdx = -1;
            }
        }
    }

    public void DeselectAll(BaseCommandWidget Exception = null)
    {
        SubcommandWidgets.ForEach(s =>
        {
            if (s != Exception) s.SetSelected(false);
            s.DeselectAll(Exception);
        });
    }

    public bool InsideChild()
    {
        return SubcommandWidgets.Exists(s => s.Mouse.Inside || s.InsideChild());
    }

    public void SetSelected(bool Selected)
    {
        if (this.Selected != Selected)
        {
            this.Selected = Selected;
            if (this.Selected)
            {
                MainCommandWidget.DeselectAll(this);
                SetBackgroundColor(28, 50, 73);
                WidgetSelected(new BaseEventArgs());
            }
            else
            {
                SetBackgroundColor(Color.ALPHA);
                if (SelectedWidget) Window.UI.SetSelectedWidget(null);
            }
        }
        else if (Selected && !SelectedWidget) WidgetSelected(new BaseEventArgs());
    }

    protected List<BaseCommandWidget> GetParentCommandWidgets()
    {
        List<BaseCommandWidget> CommandWidgets = new List<BaseCommandWidget>();
        IContainer CurrentWidget = Parent;
        while (true)
        {
            BaseCommandWidget bcw = GetParentCommandWidget(CurrentWidget);
            CommandWidgets.Add(bcw);
            if (bcw.Indentation == -1) break;
            CurrentWidget = bcw.Parent;
        }
        return CommandWidgets;
    }

    protected BaseCommandWidget GetParentCommandWidget(IContainer CurrentWidget)
    {
        if (CurrentWidget is BaseCommandWidget) return (BaseCommandWidget) CurrentWidget;
        return GetParentCommandWidget(CurrentWidget.Parent);
    }

    public BaseCommandWidget GetSelectedWidget()
    {
        for (int i = 0; i < SubcommandWidgets.Count; i++)
        {
            if (SubcommandWidgets[i].Selected) return SubcommandWidgets[i];
            else
            {
                BaseCommandWidget w = SubcommandWidgets[i].GetSelectedWidget();
                if (w != null) return w;
            }
        }
        return null;
    }

    protected int? GetSelectedIndex()
    {
        return GetSelectedWidget()?.GlobalCommandIndex;
    }

    protected void UpdateGlobalIndexes(int StartIndex, int Increase)
    {
        SubcommandWidgets.ForEach(s =>
        {
            if (s.GlobalCommandIndex >= StartIndex) s.GlobalCommandIndex += Increase;
            s.UpdateGlobalIndexes(StartIndex, Increase);
        });
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        // Only perform this mouse input for BaseCommandWidgets; derived classes will include these cases
        // But since we must call Widget.LeftMouseDownInside for the double click event to count, they must
        // still all call this method.
        if (this.GetType() == typeof(BaseCommandWidget))
        {
            if (e.Handled || InsideChild())
            {
                CancelDoubleClick();
                return;
            }
            if (this.Indentation == -1)
            {
                if (!VStackPanel.Mouse.Inside) SubcommandWidgets[^1].SetSelected(true);
            }
            else SetSelected(true);
        }
    }

    protected virtual bool IsEditable()
    {
        return this is not BlankWidget;
    }

    protected void RegisterUndoAction(CommandUndoAction UndoAction)
    {
        UndoList.Add(UndoAction);
        RedoList.Clear();
    }

    protected void Undo()
    {
        if (UndoList.Count > 0)
        {
            CommandUndoAction Action = UndoList.Last();
            Action.Trigger(false);
            UndoList.Remove(Action);
            RedoList.Add(Action);
        }
    }

    protected void Redo()
    {
        if (RedoList.Count > 0)
        {
            CommandUndoAction Action = RedoList.Last();
            Action.Trigger(true);
            RedoList.Remove(Action);
            UndoList.Add(Action);
        }
    }

    protected BaseCommandWidget GetCommandAtGlobalIndex(int GlobalCommandIndex)
    {
        if (this.GlobalCommandIndex == GlobalCommandIndex) return this;
        for (int i = 0; i < SubcommandWidgets.Count; i++)
        {
            BaseCommandWidget bcw = SubcommandWidgets[i].GetCommandAtGlobalIndex(GlobalCommandIndex);
            if (bcw != null) return bcw;
        }
        return null;
    }

    protected BaseCommandWidget InsertCommands(List<EventCommand> Commands, bool Undoable = true)
    {
        if (this.Indentation == -1) return null;
        // Get the global index for our new commands
        int GlobalIndex = this.GlobalCommandIndex;
        // Deselect this widget
        this.SetSelected(false);
        // Find the starting indentation of the commands
        int StartIndent = Commands[0].Indent;
        // Find the new indentation of the commands
        int NewIndent = this.Indentation;
        // Find the indentation difference
        int IndentDiff = this.Indentation - StartIndent;
        // Set the right indentation for our new commands
        Commands.ForEach(c => c.Indent += IndentDiff);
        // Find our parent command widgets
        List<BaseCommandWidget> ParentCommandWidgets = GetParentCommandWidgets();
        // Find the (internal) event command right above where we're inserting the new commands
        EventCommand CommandAbove = GlobalIndex > 0 ? MainCommandWidget.Commands[GlobalIndex - 1] : null;
        // For each parent command widget, update its internal Commands list to include our new commands in case they get redrawn.
        ParentCommandWidgets.ForEach(p =>
        {
            // Our local index is the local index of that command + 1.
            int LocalIndex = CommandAbove is null ? 0 : (p.Commands.IndexOf(CommandAbove) + 1);
            // Add commands to parent's local commands
            p.Commands.InsertRange(LocalIndex, Commands);
        });
        // Increase the global index for all commandwidgets that will be below our new commands
        MainCommandWidget.UpdateGlobalIndexes(GlobalIndex, Commands.Count);
        // This was the index of the selected widget in its parent, and where our new widget will be
        int LocalWidgetIndex = this.Parent.Widgets.IndexOf(this);
        // Be sure we create this widget as a subcommand of our parent
        BaseCommandWidget ParentCommandWidget = ParentCommandWidgets[0];
        // Create our new widget at the correct local index
        BaseCommandWidget NewWidget = ParentCommandWidget.CreateWidget(Commands[0], Commands.Count, ((VStackPanel) Parent), GlobalIndex, LocalWidgetIndex);
        // Select our new widget
        NewWidget.SetSelected(true);
        if (Undoable) MainCommandWidget.RegisterUndoAction(new CommandChangeUndoAction(MainCommandWidget, GlobalIndex, Commands, true));
        // Scroll to newly selected command
        NewWidget.ScrollToThisCommand();
        return NewWidget;
    }

    protected void Insert()
    {
        if (!Viewport.Visible) return;
        if (this.Indentation == -1 && !InsideChild())
        {
            // Call Insert as if we we had selected the last command widget
            if (!VStackPanel.Mouse.Inside) SubcommandWidgets[^1].Insert();
            return;
        }
        NewEventCommandWindow win = new NewEventCommandWindow(Map, Event, Page);
        win.OnClosed += _ =>
        {
            if (!win.Apply) return;
            InsertCommands(win.Commands);
        };
    }

    protected void BaseEdit()
    {
        if (this.Indentation == -1 || !IsEditable() || !Viewport.Visible) return;
        // Record the current first command.
        EventCommand OldMainCommand = this.Command;
        // Record the old number of commands.
        int OldCommandCount = Commands.Count;
        // Call the method that edits the command and creates new command objects.
        Edit((Applied, ResetCommand, GlobalIndexToCountFrom) =>
        {
            bool sel = this.SelectedWidget;
            // Ensure focus is back on our command widget
            WidgetSelected(new BaseEventArgs());
            // If nothing changed, we can just stop right here.
            if (!Applied) return;
            // Re-set the "main" command
            this.Command = Commands[0];
            // Record the new number of commands
            int NewCommandCount = Commands.Count;
            // Get our parent command widgets
            List<BaseCommandWidget> ParentCommandWidgets = GetParentCommandWidgets();
            // For each parent command widget, we update its Commands list to include the proper commands, in case it gets redrawn.
            // We do this even if the size didn't change, because the command references may no longer be the same
            // if new objects were created during editing.
            ParentCommandWidgets.ForEach(p =>
            {
                int LocalIndex = p.Commands.IndexOf(OldMainCommand);
                p.Commands.RemoveRange(LocalIndex, OldCommandCount);
                p.Commands.InsertRange(LocalIndex, Commands);
            });
            // If we created or removed commands, then we need to update the global indexes of other command widgets
            if (OldCommandCount != NewCommandCount)
            {
                // Positive => more commands, negative => less commands
                int Diff = NewCommandCount - OldCommandCount;
                // If no index was set from which point to update the global command indexes, we assume we start at the end of our command.
                // However, if we inserted one single command somewhere, and we want the capability of only inserting that command
                // rather than redrawing the whole command and all of its subcommands, then we can simply start the increment there.
                if (GlobalIndexToCountFrom == -1) GlobalIndexToCountFrom = this.GlobalCommandIndex + OldCommandCount;
                // Now update all global indexes for the widgets below this one
                MainCommandWidget.UpdateGlobalIndexes(GlobalIndexToCountFrom, Diff);
            }
            if (ResetCommand) SetCommand(this.Map, this.Event, this.Page, this.Commands[0], this.Commands, this.Indentation, this.GlobalCommandIndex);
            else LoadCommand();
            UpdateHeight();
        });
    }

    protected virtual void Edit(EditEvent Continue)
    {

    }

    protected void Cut()
    {
        if (this.Indentation == -1 || this is BlankWidget || !Viewport.Visible) return;
        Copy();
        Delete();
    }

    protected void Copy()
    {
        if (this.Indentation == -1 || this is BlankWidget || !Viewport.Visible) return;
        EventCommandList data = new EventCommandList(this.Commands);
        Utilities.SetClipboard(data, BinaryData.EVENT_COMMANDS);
    }

    protected void Paste()
    {
        if (this.Indentation == -1 || !Utilities.IsClipboardValidBinary(BinaryData.EVENT_COMMANDS)) return;
        EventCommandList data = Utilities.GetClipboard<EventCommandList>();
        InsertCommands(data.Commands);
    }

    protected BaseCommandWidget Delete(bool Undoable = true)
    {
        if (this.Indentation == -1 || this is BlankWidget || !Viewport.Visible) return null;
        // Get the global index for our main command
        int GlobalIndex = this.GlobalCommandIndex;
        // The number of commands to remove
        int Count = this.Commands.Count;
        // Get the parent command widgets
        List<BaseCommandWidget> ParentCommandWidgets = GetParentCommandWidgets();
        // For each parent command widget, we remove our commands from its Commands list so it displays properly if it gets redrawn.
        ParentCommandWidgets.ForEach(p =>
        {
            // Get the global command index relative to the parent's global command index
            int LocalIndex = p.Commands.IndexOf(MainCommandWidget.Commands[GlobalIndex]);
            // Remove those commands
            p.Commands.RemoveRange(LocalIndex, Count);
        });
        // Get the index of this command widget in the parent's widget list
        int LocalWidgetIndex = this.Parent.Widgets.IndexOf(this);
        // Removes this command widget from the parent list
        this.Dispose();
        BaseCommandWidget ParentCommandWidget = ParentCommandWidgets[0];
        // Remove this command widget from our parent's subcommand list, because disposing doesn't automatically remove that reference
        ParentCommandWidget.SubcommandWidgets.Remove(this);
        // Decrease the global index for all commandwidgets that were below our old command
        MainCommandWidget.UpdateGlobalIndexes(GlobalIndex, -Count);
        // We fetch the next command now in the list, which is at the index where we just deleted this command widget.
        BaseCommandWidget NewSelectedWidget = (BaseCommandWidget) this.Parent.Widgets[LocalWidgetIndex];
        // Now we select that widget
        NewSelectedWidget.SetSelected(true);
        // Shortcut timing
        Shortcut s = NewSelectedWidget.Shortcuts.Find(s => s.Key.MainKey == Keycode.DELETE);
        if (MainCommandWidget.InitialDeleteKey) NewSelectedWidget.SetTimer($"key_{s.Key.ID}_initial", 300);
        else NewSelectedWidget.SetTimer($"key_{s.Key.ID}", 50);
        // Update positionings
        ((VStackPanel) this.Parent).UpdateLayout();
        if (Undoable) MainCommandWidget.RegisterUndoAction(new CommandChangeUndoAction(MainCommandWidget, GlobalIndex, Commands, false));
        // Scroll to newly selected command
        NewSelectedWidget.ScrollToThisCommand();
        return NewSelectedWidget;
    }

    protected BaseCommandWidget GetNextCommand(int StartIndex = 0)
    {
        if (StartIndex < SubcommandWidgets.Count) return SubcommandWidgets[StartIndex];
        else if (this == MainCommandWidget) return null;
        else
        {
            BaseCommandWidget ParentCommandWidget = GetParentCommandWidget(Parent);
            if (ParentCommandWidget is null) return null;
            return ParentCommandWidget.GetNextCommand(ParentCommandWidget.SubcommandWidgets.IndexOf(this) + 1);
        }
    }

    public override void Update()
    {
        base.Update();
        if (this.Indentation != -1) return;
        if (Input.Press(Keycode.DOWN))
        {
            if (TimerPassed("down_initial"))
            {
                InitialDownKey = false;
                DestroyTimer("down_initial");
            }
            else if (!TimerExists("down_initial"))
            {
                SetTimer("down_initial", 300);
            }
        }
        else InitialDownKey = true;
        if (Input.Press(Keycode.UP))
        {
            if (TimerPassed("up_initial"))
            {
                InitialUpKey = false;
                DestroyTimer("up_initial");
            }
            else if (!TimerExists("up_initial"))
            {
                SetTimer("up_initial", 300);
            }
        }
        else InitialUpKey = true;
        if (Input.Press(Keycode.PAGEDOWN))
        {
            if (TimerPassed("page_down_initial"))
            {
                InitialPageDownKey = false;
                DestroyTimer("page_down_initial");
            }
            else if (!TimerExists("page_down_initial"))
            {
                SetTimer("page_down_initial", 300);
            }
        }
        else InitialPageDownKey = true;
        if (Input.Press(Keycode.PAGEUP))
        {
            if (TimerPassed("page_up_initial"))
            {
                InitialPageUpKey = false;
                DestroyTimer("page_up_initial");
            }
            else if (!TimerExists("page_up_initial"))
            {
                SetTimer("page_up_initial", 300);
            }
        }
        else InitialPageUpKey = true;
        if (Input.Press(Keycode.DELETE))
        {
            if (TimerPassed("delete_initial"))
            {
                InitialDeleteKey = false;
                DestroyTimer("delete_initial");
            }
            else if (!TimerExists("delete_initial"))
            {
                SetTimer("delete_initial", 300);
            }
        }
        else InitialDeleteKey = true;
        if (Input.Press(Keycode.Z) && Input.Press(Keycode.CTRL))
        {
            if (TimerPassed("undo_initial"))
            {
                InitialUndoKey = false;
                DestroyTimer("undo_initial");
            }
            else if (!TimerExists("undo_initial"))
            {
                SetTimer("undo_initial", 300);
            }
        }
        else InitialUndoKey = true;
        if (Input.Press(Keycode.Y) && Input.Press(Keycode.CTRL))
        {
            if (TimerPassed("redo_initial"))
            {
                InitialRedoKey = false;
                DestroyTimer("redo_initial");
            }
            else if (!TimerExists("redo_initial"))
            {
                SetTimer("redo_initial", 300);
            }
        }
        else InitialRedoKey = true;
    }

    protected int GetDisplayY()
    {
        return Viewport.Y - MainCommandWidget.VStackPanel.Viewport.Y - TopCutOff + MainCommandWidget.TopCutOff;
    }

    protected void ScrollToThisCommand()
    {
        int DisplayY = GetDisplayY();
        if (DisplayY < MainCommandWidget.Parent.ScrolledY)
        {
            MainCommandWidget.Parent.ScrolledY -= MainCommandWidget.Parent.ScrolledY - DisplayY + MarginBetweenWidgets;
            ((Widget) MainCommandWidget.Parent).UpdateAutoScroll();
        }
        else if (DisplayY + StandardHeight > MainCommandWidget.Parent.Size.Height + MainCommandWidget.Parent.ScrolledY)
        {
            MainCommandWidget.Parent.ScrolledY += DisplayY + StandardHeight - (MainCommandWidget.Parent.Size.Height + MainCommandWidget.Parent.ScrolledY) + MarginBetweenWidgets;
            ((Widget) MainCommandWidget.Parent).UpdateAutoScroll();
        }
    }

    protected void SelectNextCommand()
    {
        BaseCommandWidget NextCommand = GetNextCommand();
        if (NextCommand is not null)
        {
            NextCommand.SetSelected(true);
            Shortcut s = NextCommand.Shortcuts.Find(s => s.Key.MainKey == Keycode.DOWN);
            if (MainCommandWidget.InitialDownKey) NextCommand.SetTimer($"key_{s.Key.ID}_initial", 300);
            else NextCommand.SetTimer($"key_{s.Key.ID}", 50);
            NextCommand.ScrollToThisCommand();
        }
    }

    protected BaseCommandWidget GetPreviousCommand()
    {
        BaseCommandWidget ParentCommandWidget = GetParentCommandWidget(Parent);
        if (ParentCommandWidget is null) return null;
        int Index = ParentCommandWidget.SubcommandWidgets.IndexOf(this);
        if (Index > 0) return ParentCommandWidget.SubcommandWidgets[Index - 1].GetLastCommand();
        else if (ParentCommandWidget != MainCommandWidget) return ParentCommandWidget;
        else return null;
    }

    protected BaseCommandWidget GetLastCommand()
    {
        if (SubcommandWidgets.Count > 0) return SubcommandWidgets[^1].GetLastCommand();
        else return this;
    }

    protected void SelectPreviousCommand()
    {
        BaseCommandWidget PreviousCommand = GetPreviousCommand();
        if (PreviousCommand is not null)
        {
            PreviousCommand.SetSelected(true);
            Shortcut s = PreviousCommand.Shortcuts.Find(s => s.Key.MainKey == Keycode.UP);
            if (MainCommandWidget.InitialUpKey) PreviousCommand.SetTimer($"key_{s.Key.ID}_initial", 300);
            else PreviousCommand.SetTimer($"key_{s.Key.ID}", 50);
            PreviousCommand.ScrollToThisCommand();
        }
    }

    BaseCommandWidget GetCommandWidgetBelow(int MinTargetDisplayY)
    {
        int DisplayY = GetDisplayY();
        if (DisplayY >= MinTargetDisplayY) return this;
        for (int i = 0; i < SubcommandWidgets.Count; i++)
        {
            BaseCommandWidget bcw = SubcommandWidgets[i].GetCommandWidgetBelow(MinTargetDisplayY);
            if (bcw != null) return bcw;
        }
        return null;
    }

    BaseCommandWidget GetCommandWidgetAbove(int MaxTargetDisplayY)
    {
        int DisplayY = GetDisplayY();
        for (int i = SubcommandWidgets.Count - 1; i >= 0; i--)
        {
            BaseCommandWidget bcw = SubcommandWidgets[i].GetCommandWidgetAbove(MaxTargetDisplayY);
            if (bcw != null) return bcw;
        }
        if (this != MainCommandWidget && DisplayY < MaxTargetDisplayY) return this;
        return null;
    }

    protected void SelectPageDown()
    {
        int TargetDisplayY = MainCommandWidget.Parent.ScrolledY + MainCommandWidget.Parent.Size.Height - StandardHeight * 2;
        BaseCommandWidget NewCommand = MainCommandWidget.GetCommandWidgetBelow(TargetDisplayY);
        if (NewCommand is null) NewCommand = MainCommandWidget.GetLastCommand();
        if (NewCommand.Selected)
        {
            if (NewCommand.GlobalCommandIndex == MainCommandWidget.Commands.Count - 1 ||
                MainCommandWidget.SubcommandWidgets.IndexOf(this) == MainCommandWidget.SubcommandWidgets.Count - 1 &&
                SubcommandWidgets.Count == 0) return;
            TargetDisplayY += MainCommandWidget.Parent.Size.Height - StandardHeight;
            NewCommand = MainCommandWidget.GetCommandWidgetBelow(TargetDisplayY);
            if (NewCommand is null) NewCommand = MainCommandWidget.GetLastCommand();
        }
        if (NewCommand.Selected) return;
        NewCommand.SetSelected(true);
        Shortcut s = NewCommand.Shortcuts.Find(s => s.Key.MainKey == Keycode.PAGEDOWN);
        if (MainCommandWidget.InitialPageDownKey) NewCommand.SetTimer($"key_{s.Key.ID}_initial", 300);
        else NewCommand.SetTimer($"key_{s.Key.ID}", 50);
        NewCommand.ScrollToThisCommand();
    }

    protected void SelectPageUp()
    {
        int CurDisplayY = GetDisplayY();
        int TargetDisplayY = MainCommandWidget.Parent.ScrolledY + StandardHeight;
        BaseCommandWidget NewCommand = MainCommandWidget.GetCommandWidgetAbove(TargetDisplayY);
        if (NewCommand is null) NewCommand = MainCommandWidget.SubcommandWidgets[0];
        int SelDisplayY = NewCommand.GetDisplayY();
        if (NewCommand.Selected)
        {
            if (NewCommand.GlobalCommandIndex == 0) return;
            TargetDisplayY -= MainCommandWidget.Parent.Size.Height - StandardHeight;
            NewCommand = MainCommandWidget.GetCommandWidgetAbove(TargetDisplayY);
            if (NewCommand is null) NewCommand = MainCommandWidget.SubcommandWidgets[0];
        }
        if (NewCommand.Selected) return;
        NewCommand.SetSelected(true);
        Shortcut s = NewCommand.Shortcuts.Find(s => s.Key.MainKey == Keycode.PAGEUP);
        if (MainCommandWidget.InitialPageUpKey) NewCommand.SetTimer($"key_{s.Key.ID}_initial", 300);
        else NewCommand.SetTimer($"key_{s.Key.ID}", 50);
        NewCommand.ScrollToThisCommand();
    }

    public class CommandUndoAction
    {
        protected BaseCommandWidget MainCommandWidget;

        public CommandUndoAction(BaseCommandWidget MainCommandWidget)
        {
            this.MainCommandWidget = MainCommandWidget;
        }

        public virtual void Trigger(bool IsRedo)
        {

        }
    }

    public class CommandChangeUndoAction : CommandUndoAction
    {
        int GlobalCommandIndex;
        List<EventCommand> Commands;
        bool Creation;

        public CommandChangeUndoAction(BaseCommandWidget MainCommandWidget, int GlobalCommandIndex, List<EventCommand> Commands, bool Creation) : base(MainCommandWidget)
        {
            this.GlobalCommandIndex = GlobalCommandIndex;
            this.Commands = new List<EventCommand>();
            Commands.ForEach(c => this.Commands.Add((EventCommand) c.Clone()));
            this.Creation = Creation;
        }

        public override void Trigger(bool IsRedo)
        {
            BaseCommandWidget NewWidget;
            if (IsRedo != Creation)
            {
                // Undo creation + redo deletion
                BaseCommandWidget DeletionWidget = MainCommandWidget.GetCommandAtGlobalIndex(this.GlobalCommandIndex);
                NewWidget = DeletionWidget.Delete(false);
            }
            else
            {
                // Redo creation + undo deletion
                BaseCommandWidget InsertionWidget = MainCommandWidget.GetCommandAtGlobalIndex(this.GlobalCommandIndex);
                NewWidget = InsertionWidget.InsertCommands(Commands, false);
            }
            if (IsRedo)
            {
                Shortcut s = NewWidget.Shortcuts.Find(s => s.Key.MainKey == Keycode.Y);
                if (MainCommandWidget.InitialRedoKey) NewWidget.SetTimer($"key_{s.Key.ID}_initial", 300);
                else NewWidget.SetTimer($"key_{s.Key.ID}", 50);
            }
            else
            {
                Shortcut s = NewWidget.Shortcuts.Find(s => s.Key.MainKey == Keycode.Z);
                if (MainCommandWidget.InitialUndoKey) NewWidget.SetTimer($"key_{s.Key.ID}_initial", 300);
                else NewWidget.SetTimer($"key_{s.Key.ID}", 50);
            }
        }
    }
}