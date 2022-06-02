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
    protected int MarginBetweenWidgets = 3;
    protected int ChildIndent = 20;
    protected bool DrawEndLabels = true;
    protected bool Ready;

    protected Label HeaderLabel;
    protected VStackPanel VStackPanel;

    List<BaseCommandWidget> SubcommandWidgets = new List<BaseCommandWidget>();

    public BaseCommandWidget(IContainer Parent, Color BarColor = null) : base(Parent)
    {
        HeaderLabel = new Label(this);
        HeaderLabel.SetPosition(8, 4);
        HeaderLabel.SetFont(Fonts.UbuntuBold.Use(10));
        if (BarColor != null) HeaderLabel.SetTextColor(BarColor);
        VStackPanel = new VStackPanel(this);
        VStackPanel.SetHDocked(true);
        VStackPanel.OnSizeChanged += _ => UpdateHeight();
        Sprites["bar"] = new Sprite(this.Viewport, new SolidBitmap(4, 32, BarColor ?? new Color(63, 210, 101)));
        OnSizeChanged += _ => ((SolidBitmap) Sprites["bar"].Bitmap).SetSize(4, Size.Height);
    }

    public void SetReady(bool Ready)
    {
        this.Ready = Ready;
        SubcommandWidgets.ForEach(w => w.SetReady(Ready));
    }

    public void SetCommand(Map Map, Event Event, EventPage Page, EventCommand? Command, List<EventCommand> Commands, int Indentation)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        this.Command = Command;
        this.Commands = Commands;
        this.Indentation = Indentation;
        SubcommandWidgets.ForEach(w => w.Dispose());
        SubcommandWidgets.Clear();
        //if (this.Indentation == 0) SetBackgroundColor(28, 50, 73);
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
            VStackPanel.SetPadding(ChildIndent, 24, 0, 0);
            LoadCommand();
        }
        else
        {
            ChildIndent = 0;
            HeaderLabel.SetVisible(false);
            Sprites["bar"].Visible = false;
            VStackPanel.SetPadding(0);
            ParseCommands(Commands);
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
        SetHeight(24 + vh + HeightAdd);
    }

    public virtual void LoadCommand()
    {
        
    }

    protected void CreateWidget(EventCommand Command, int Count = 1, VStackPanel Parent = null)
    {
        BaseCommandWidget w = null;
        if (CommandWidgetLookup.ContainsKey(Command.Code))
        {
            System.Type type = CommandWidgetLookup[Command.Code].WidgetClass;
            w = (BaseCommandWidget) Activator.CreateInstance(type, new object?[] { Parent ?? VStackPanel });
        }
        else w = new BaseCommandWidget(Parent ?? VStackPanel);
        w.SetHDocked(true);
        w.SetMargins(0, MarginBetweenWidgets);
        w.SetCommand(Map, Event, Page, Command, Commands.GetRange(Commands.IndexOf(Command), Count), this.Indentation + 1);
        w.OnSizeChanged += _ =>
        {
            ((VStackPanel) w.Parent).UpdateLayout();
        };
        SubcommandWidgets.Add(w);
    }

    protected void ParseCommands(List<EventCommand> Commands, VStackPanel Parent = null)
    {
        EventCommand? BranchCmd = null;
        int BranchCmdIdx = -1;

        EventCommand? MergeCmd = null;
        int MergeCmdIdx = -1;

        for (int i = 0; i < Commands.Count; i++)
        {
            EventCommand cmd = Commands[i];
            if (MergeCmd != null)
            {
                if (i + 1 >= Commands.Count || Commands[i + 1].Code != MergeSubsequentCommandPairs[MergeCmd.Code])
                {
                    CreateWidget(MergeCmd, i - MergeCmdIdx + 1, Parent);
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
                    CreateWidget(cmd, 1, Parent);
                }
            }
            else if (BranchCmd.Indent == cmd.Indent && cmd.Code == CommandStartEndPairs[BranchCmd.Code])
            {
                CreateWidget(BranchCmd, i - BranchCmdIdx + 1, Parent);
                BranchCmd = null;
                BranchCmdIdx = -1;
            }
        }
    }
}
