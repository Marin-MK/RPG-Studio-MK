using System;
using System.Collections.Generic;
using System.Reflection;
using RPGStudioMK.Game;
using RPGStudioMK.Game.EventCommands;
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

    static Dictionary<CommandCode, (System.Type WidgetClass, System.Type CommandClass)> CommandWidgetLookup = new Dictionary<CommandCode, (System.Type, System.Type)>()
    {
        { CommandCode.Blank, (typeof(BlankWidget), typeof(BaseCommand)) },
        { CommandCode.ConditionalBranch, (typeof(ConditionalWidget), typeof(ConditionalCommand)) },
        { CommandCode.ShowChoices, (typeof(ChoiceWidget), typeof(BaseCommand)) },
        { CommandCode.Comment, (typeof(CommandWidgets.TextWidget), typeof(BaseCommand)) },
        { CommandCode.Script, (typeof(CommandWidgets.TextWidget), typeof(BaseCommand)) },
        { CommandCode.ShowText, (typeof(CommandWidgets.TextWidget), typeof(BaseCommand)) }
    };

    protected Map Map;
    protected Event Event;
    protected EventPage Page;
    protected EventCommand? Command;
    protected BaseCommand CommandHelper;
    protected List<EventCommand> Commands;
    protected int Indentation;
    protected int HeightAdd = 0;
    protected int MarginBetweenWidgets = 2;
    protected int ChildIndent = 20;
    protected bool DrawEndLabels = true;
    protected bool Ready;
    protected bool Selected = false;
    protected int StandardHeight = 20;
    protected int GlobalCommandIndex = -1;

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
                Console.WriteLine($"Opening for {GlobalCommandIndex}");
                e.Value = true;
                SetSelected(true);
            }
            else e.Value = false;
        };
        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER), _ => Insert(), false),
            new Shortcut(this, new Key(Keycode.SPACE), _ => BaseEdit(), false, e => e.Value = this is not BlankWidget),
            new Shortcut(this, new Key(Keycode.X, Keycode.CTRL), _ => Cut(), false, e => e.Value = this is not BlankWidget),
            new Shortcut(this, new Key(Keycode.C, Keycode.CTRL), _ => Copy(), false, e => e.Value = this is not BlankWidget),
            new Shortcut(this, new Key(Keycode.V, Keycode.CTRL), _ => Paste(), false, e => e.Value = Utilities.IsClipboardValidBinary(BinaryData.EVENT_COMMANDS)),
            new Shortcut(this, new Key(Keycode.DELETE), _ => Delete(), false, e => e.Value = this is not BlankWidget)
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
                IsClickable = e => e.Value = this is not BlankWidget,
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

    public void SetReady(bool Ready)
    {
        this.Ready = Ready;
        SubcommandWidgets.ForEach(w => w.SetReady(Ready));
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
            if (CommandWidgetLookup.ContainsKey(Command.Code))
            {
                System.Type CommandClass = CommandWidgetLookup[Command.Code].CommandClass;
                CommandHelper = (BaseCommand)Activator.CreateInstance(CommandClass, new object?[] { Command });
            }
            else CommandHelper = new BaseCommand(Command);
            HeaderLabel.SetText(Command.Code.ToString());
            HeaderLabel.SetVisible(true);
            VStackPanel.SetPadding(ChildIndent, StandardHeight, 0, 0);
            LoadCommand();
        }
        else
        {
            ChildIndent = 0;
            HeaderLabel.SetVisible(false);
            Sprites["bar"].Visible = false;
            VStackPanel.SetPadding(0);
            ParseCommands(Commands, VStackPanel, this.GlobalCommandIndex + 1);
        }
        UpdateHeight();
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
        SetHeight(StandardHeight + vh + HeightAdd);
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
            System.Type type = CommandWidgetLookup[Command.Code].WidgetClass;
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
            else SetBackgroundColor(Color.ALPHA);
        }
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

    protected BaseCommandWidget GetSelectedWidget()
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
            if (e.Handled || this.Indentation == -1 || InsideChild())
            {
                CancelDoubleClick();
                return;
            }
            SetSelected(true);
        }
    }

    protected void InsertCommands(List<EventCommand> Commands)
    {
        if (this.Indentation == -1) return;
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
    }

    protected void Insert()
    {
        if (this.Indentation == -1 || !Viewport.Visible) return;
        List<EventCommand> cmds = new List<EventCommand>()
        {
            new EventCommand(CommandCode.ShowText, 0, new List<object>() { "Line 1" }),
            new EventCommand(CommandCode.MoreText, 0, new List<object>() { "Line 2" }),
            new EventCommand(CommandCode.MoreText, 0, new List<object>() { "Line 3" })
        };
        InsertCommands(cmds);
    }

    protected void BaseEdit()
    {
        if (this.Indentation == -1 || this is BlankWidget || !Viewport.Visible) return;
        // Record the current first command.
        EventCommand OldMainCommand = this.Command;
        // Record the old number of commands.
        int OldCommandCount = Commands.Count;
        // Call the method that edits the command and creates new command objects.
        (bool Applied, bool ResetCommand, int GlobalIndexToCountFrom) = Edit();
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
    }

    protected virtual (bool Applied, bool ResetCommand, int GlobalIndexToCountFrom) Edit()
    {
        return (false, false, -1);
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

    protected void Delete()
    {
        if (this.Indentation == -1 || this is BlankWidget || !Viewport.Visible) return;
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
        // Update positionings
        ((VStackPanel) this.Parent).UpdateLayout();
    }
}
