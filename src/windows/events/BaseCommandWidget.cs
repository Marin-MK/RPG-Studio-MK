using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets.CommandWidgets;

namespace RPGStudioMK.Widgets;

public class BaseCommandWidget : Widget
{
    public static Dictionary<CommandCategory, List<CommandCode>> CommandTypes = new Dictionary<CommandCategory, List<CommandCode>>()
    {
        { CommandCategory.General, 
            new List<CommandCode>()
            {
                CommandCode.ShowText, CommandCode.ChangeTextOptions, CommandCode.ShowChoices, CommandCode.ChangeWindowskin, 
                CommandCode.SetMoveRoute, CommandCode.InputNumber, CommandCode.WaitForMoveCompletion, CommandCode.Script, 
                CommandCode.ChangeMoney, CommandCode.Comment, CommandCode.Wait 
            }
        },
        { CommandCategory.Flow,
            new List<CommandCode>()
            {
                CommandCode.ControlSwitches, CommandCode.ConditionalBranch, CommandCode.ControlVariables, CommandCode.Loop, 
                CommandCode.ControlSelfSwitch, CommandCode.BreakLoop, CommandCode.ExitEventProcessing, CommandCode.Label, 
                CommandCode.EraseEvent, CommandCode.JumpToLabel, CommandCode.CallCommonEvent
            }
        },
        { CommandCategory.Map, 
            new List<CommandCode>()
            {
                CommandCode.TransferPlayer, CommandCode.ScrollMap, CommandCode.SetEventLocation, CommandCode.ChangeMapSettings, 
                CommandCode.ShowAnimation, CommandCode.ChangeFogColorTone, CommandCode.SetWeatherEffects, CommandCode.ChangeFogOpacity, 
                CommandCode.ChangeTransparencyFlag, CommandCode.ChangeScreenColorTone
            }
        },
        { CommandCategory.ImageSound,
            new List<CommandCode>()
            {
                CommandCode.ShowPicture, CommandCode.PlayBGM, CommandCode.MovePicture, CommandCode.FadeOutBGM, CommandCode.RotatePicture, 
                CommandCode.PlayBGS, CommandCode.ChangePictureColorTone, CommandCode.FadeOutBGS, CommandCode.ErasePicture, CommandCode.PlaySE, 
                CommandCode.PlayME, CommandCode.StopSE, CommandCode.RestoreBGMBGS, CommandCode.MemorizeBGMBGS
            }
        },
        {
            CommandCategory.Blank,
            new List<CommandCode>() { CommandCode.Blank }
        }
    };

    public static Dictionary<CommandCategory, (string Name, int Index, Color TopBarColor, Color BottomBarColor)> CategoryInfo = new Dictionary<CommandCategory, (string, int, Color, Color)>()
    {
        { CommandCategory.General, ("General", 0, new Color(27, 148, 119), new Color(113, 221, 59)) },
        { CommandCategory.Flow, ("Flow", 1, new Color(161, 166, 90), new Color(228, 230, 30)) },
        { CommandCategory.Map, ("Map", 2, new Color(113, 75, 231), new Color(214, 173, 176)) },
        { CommandCategory.ImageSound, ("Image/Sound", 3, new Color(183, 30, 120), new Color(239, 117, 60)) },
        { CommandCategory.Other, ("Other", 4, new Color(183, 89, 90), new Color(239, 189, 30)) },
        { CommandCategory.Blank, ("Blank", -1, new Color(86, 108, 134), new Color(86, 108, 134)) }
    };

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

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BlankWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ChoiceWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConditionalWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExpandableCommandWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MoveRouteWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SetSelfSwitchWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SetSwitchWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SetVariableWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CommandWidgets.TextWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TransferPlayerWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(WaitForMoveCompletionWidget))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(WaitWidget))]
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
        { CommandCode.ControlVariables, typeof(SetVariableWidget) },
        { CommandCode.ControlSelfSwitch, typeof(SetSelfSwitchWidget) },
        { CommandCode.TransferPlayer, typeof(TransferPlayerWidget) }
    };

    public BaseCommandWidget MainCommandWidget;
    public BaseCommandWidget SelectionOrigin;
    public List<BaseCommandWidget> SelectedWidgets = new List<BaseCommandWidget>();

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
    protected int MarginBetweenWidgets = 1;
    protected int ChildIndent = 10;
    protected int BarWidth = 4;
    protected int StandardHeight = 28;
    protected int GlobalCommandIndex = -1;
    protected int CommandOffset = 3;
    protected int ShadowSize = 5;
    protected bool ScaleGradientWithSize = false;
    protected bool DrawEndLabels = true;
    protected bool Selected = false;
    protected bool InitialDownKey = true;
    protected bool InitialUpKey = true;
    protected bool InitialPageDownKey = true;
    protected bool InitialPageUpKey = true;
    protected bool InitialDeleteKey = true;
    protected bool InitialUndoKey = true;
    protected bool InitialRedoKey = true;

    protected EventCommandIcon Icon;
    protected Label HeaderLabel;
    protected VStackPanel VStackPanel;
    protected GradientBox GradientBox;
    protected GradientBox BarBox;
    protected ShadowWidget ShadowWidget;
    protected ImageBox NoCommandsBG;

    List<BaseCommandWidget> SubcommandWidgets = new List<BaseCommandWidget>();

    public BaseCommandWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex)
    {
        ShadowWidget = new ShadowWidget(this);
        ShadowWidget.SetVDocked(true);
        ShadowWidget.SetThickness(ShadowSize);
        GradientBox = new GradientBox(this);
        GradientBox.SetPadding(BarWidth + ShadowSize, ShadowSize);
        BarBox = new GradientBox(this);
        BarBox.SetWidth(BarWidth);
        BarBox.SetVDocked(true);
        BarBox.SetPadding(ShadowSize, ShadowSize, 0, ShadowSize);
        Icon = new EventCommandIcon(this);
        Icon.SetPosition(10, 6);
        HeaderLabel = new Label(this);
        HeaderLabel.SetPosition(36, 4 + ShadowSize);
        HeaderLabel.SetFont(Fonts.ParagraphBold);
        VStackPanel = new VStackPanel(this);
        VStackPanel.HDockWidgets = false;
        VStackPanel.OnSizeChanged += _ => UpdateSize();
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
                if (!Selected) SetSelected(true, false);
            }
            else e.Value = false;
        };
        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER), _ => Insert(), false),
            new Shortcut(this, new Key(Keycode.SPACE), _ => BaseEdit(), false, e => e.Value = IsEditable()),
            new Shortcut(this, new Key(Keycode.X, Keycode.CTRL), _ => Cut(), false),
            new Shortcut(this, new Key(Keycode.C, Keycode.CTRL), _ => Copy(), false),
            new Shortcut(this, new Key(Keycode.V, Keycode.CTRL), _ => Paste(), false, e => e.Value = Clipboard.IsValid(BinaryData.EVENT_COMMANDS)),
            new Shortcut(this, new Key(Keycode.A, Keycode.CTRL), _ => SelectAll(), false),
            new Shortcut(this, new Key(Keycode.DELETE), _ => DeleteMayBeMultiple()),
            new Shortcut(this, new Key(Keycode.DOWN), _ => SelectNextCommand(false)),
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.SHIFT), _ => SelectNextCommand(true)),
            new Shortcut(this, new Key(Keycode.UP), _ => SelectPreviousCommand(false)),
            new Shortcut(this, new Key(Keycode.UP, Keycode.SHIFT), _ => SelectPreviousCommand(true)),
            new Shortcut(this, new Key(Keycode.PAGEDOWN), _ => SelectPageDown(false)),
            new Shortcut(this, new Key(Keycode.PAGEDOWN, Keycode.SHIFT), _ => SelectPageDown(true)),
            new Shortcut(this, new Key(Keycode.PAGEUP), _ => SelectPageUp(false)),
            new Shortcut(this, new Key(Keycode.PAGEUP, Keycode.SHIFT), _ => SelectPageUp(true)),
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
                OnClicked = _ => Cut(),
                Shortcut = "Ctrl+X"
            },
            new MenuItem("Copy")
            {
                OnClicked = _ => Copy(),
                Shortcut = "Ctrl+C"
            },
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = Clipboard.IsValid(BinaryData.EVENT_COMMANDS),
                OnClicked = _ => Paste(),
                Shortcut = "Ctrl+V"
            },
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                OnClicked = _ => DeleteMayBeMultiple(),
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
        SubcommandWidgets.ForEach(w => w.Dispose());
        SubcommandWidgets.Clear();
        if (Command != null)
        {
            Icon.SetEventCommand(Command.Code);
            HeaderLabel.SetText(Command.Code.ToString());
            HeaderLabel.RedrawText(true);
            HeaderLabel.SetVisible(true);
            VStackPanel.SetPadding(ShadowSize + ChildIndent, ShadowSize + StandardHeight, ShadowSize, ShadowSize);
            SetCommandColors();
            LoadCommand();
        }
        else
        {
            Icon.SetVisible(false);
            UndoList.Clear();
            RedoList.Clear();
            ChildIndent = 0;
            HeaderLabel.SetVisible(false);
            VStackPanel.SetPadding(CommandOffset, 0, 0, 0);
            ShadowWidget.SetVisible(false);
            ParseCommands(Commands, VStackPanel, this.GlobalCommandIndex + 1, CreateWidget);
            if (Page.Commands.Count == 1) CreateNoCommandsBG();
            else RemoveNoCommandsBG();
        }
        UpdateSize();
    }

    private void CreateNoCommandsBG()
    {
        RemoveNoCommandsBG();
        NoCommandsBG = new ImageBox(this);
        NoCommandsBG.SetBitmap("assets/img/no_commands_bg");
        NoCommandsBG.SetFillMode(FillMode.Center);
    }

    private void RemoveNoCommandsBG()
    {
        NoCommandsBG?.Dispose();
        NoCommandsBG = null;
    }

    private void SetCommandColors()
    {
        CommandCategory cat = GetCommandCategory();
        (_, _, Color TopBarColor, Color BottomBarColor) = CategoryInfo[cat];
        Color IconColor = Bitmap.Interpolate2D(TopBarColor, BottomBarColor, 0.5);
        Icon.SetColor(Command.Code == CommandCode.Blank ? new Color(149, 158, 181) : IconColor);
        Color GradientColor = new Color(IconColor.Red, IconColor.Green, IconColor.Blue, 128);
        GradientBox.SetTopLeftColor(new Color(39, 81, 104));
        GradientBox.SetBottomRightColor(GradientColor);
        BarBox.SetTopColor(TopBarColor);
        BarBox.SetBottomColor(BottomBarColor);
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

    protected virtual void UpdateSize()
    {
        VStackPanel.UpdateLayout();
        int vw = VStackPanel.Size.Width;
        int vh = VStackPanel.Size.Height;
        if (vh == 1) // No subcommands in VStackPanel
        {
            // Remove the one excess pixel we'd get
            vw = 0;
            vh = 0;
        }

        if (this.Indentation == -1)
        {
            vw = Math.Max(Parent.Size.Width, vw);
            vh = Math.Max(vh + HeightAdd, Parent.Size.Height - Position.Y);
        }
        else
        {
            vw = Math.Max(Size.Width, vw);
            vh = StandardHeight + vh + HeightAdd + ShadowSize * 2;
        }
        SetSize(vw, vh);
    }

    public virtual void LoadCommand()
    {
        
    }

    protected int GetStandardWidth(int Indent)
    {
        return MainCommandWidget.Parent.Size.Width - (ChildIndent + ShadowSize) * Indent - CommandOffset;
    }

    protected Point GetStandardLabelPosition()
    {
        return new Point(HeaderLabel.Position.X + HeaderLabel.Size.Width + 8, 9);
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
        w.SetWidth(GetStandardWidth(this.Indentation + 1));
        w.SetMargins(0, MarginBetweenWidgets);
        w.SetCommand(Map, Event, Page, Command, Commands.GetRange(Commands.IndexOf(Command), Count), this.Indentation + 1, GlobalCommandIndex);
        w.OnSizeChanged += _ =>
        {
            ((VStackPanel) w.Parent).UpdateLayout();
        };
        SubcommandWidgets.Insert(ParentWidgetIndex == -1 ? SubcommandWidgets.Count : ParentWidgetIndex, w);
        return w;
    }

    protected void ParseCommands(List<EventCommand> Commands, VStackPanel Parent, int GlobalCommandStartIndex,
        Func<EventCommand, int, VStackPanel, int, int, BaseCommandWidget> CreateCommand)
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
                    CreateCommand(MergeCmd, i - MergeCmdIdx + 1, Parent, gcmd, -1);
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
                    CreateCommand(cmd, 1, Parent, gcmd, -1);
                    gcmd += 1;
                }
            }
            else if (BranchCmd.Indent == cmd.Indent && cmd.Code == CommandStartEndPairs[BranchCmd.Code])
            {
                CreateCommand(BranchCmd, i - BranchCmdIdx + 1, Parent, gcmd, -1);
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
            if (s != Exception) s.SetSelected(false, false);
            s.DeselectAll(Exception);
        });
    }

    public bool InsideChild()
    {
        return SubcommandWidgets.Exists(s => s.Mouse.Inside || s.InsideChild());
    }

    public void SetSelected(bool Selected, bool HoldingShift)
    {
        if (this.Selected != Selected || Selected)
        {
            bool SkipInternal = false;
            if (Selected)
            {
                // We start by deselecting everything so our selection "anchors" around the origin if we have shift down,
                // and if we don't, just to cancel all other selections.
                MainCommandWidget.DeselectAll();
                MainCommandWidget.SelectedWidgets.Clear();
                if (!HoldingShift)
                {
                    MainCommandWidget.SelectionOrigin = this;
                }
                else
                {
                    // We now start fresh by determining which commands should all end up being selected.
                    // There are 3 scenarios:
                    // cmd 1
                    // cmd 2 (selected)
                    // branching cmd 3
                    // - - cmd 4
                    // - - cmd 5 (origin)
                    // cmd 6
                    // In this event, only cmd 2 and cmd 3 should be selected.
                    // i.e. all selected commands must have the same indentation.
                    // In the clause below, the roles are reversed, which leads to the same
                    // effect, only we select the parent of the current widget, rather than
                    // the parent of the origin widget.
                    if (MainCommandWidget.SelectionOrigin.Indentation > this.Indentation)
                    {
                        List<BaseCommandWidget> Parents = MainCommandWidget.SelectionOrigin.GetParentCommandWidgets();
                        for (int i = 0; i < Parents.Count; i++)
                        {
                            BaseCommandWidget prnt = Parents[i];
                            if (prnt.Indentation == this.Indentation)
                            {
                                // Cancel the selection of this command and select the parent instead.
                                MainCommandWidget.SelectionOrigin = prnt;
                                MainCommandWidget.SelectedWidgets.Clear();
                                prnt.SetSelectedInternal(true, false);
                                this.SetSelected(true, true);
                                return;
                            }
                        }
                        throw new Exception("Could not find parent widget with identical indentation.");
                    }
                    else if (MainCommandWidget.SelectionOrigin.Indentation < this.Indentation)
                    {
                        List<BaseCommandWidget> Parents = GetParentCommandWidgets();
                        for (int i = 0; i < Parents.Count; i++)
                        {
                            BaseCommandWidget prnt = Parents[i];
                            if (prnt.Indentation == MainCommandWidget.SelectionOrigin.Indentation)
                            {
                                // Cancel the selection of this command and select the parent instead.
                                MainCommandWidget.SelectedWidgets.Clear();
                                prnt.SetSelected(true, true);
                                return;
                            }
                        }
                        throw new Exception("Could not find parent widget with identical indentation.");
                    }
                    else
                    {
                        // The current command and the selection origin have the same indentation.
                        // If these two commands share the same parent, then they are in the same branch.
                        // If not, that means there is some other branch in between. For instance, one is in
                        // the true branch of a conditional, and one in a false.
                        // They could also be two commands in two entirely different commands, but just with
                        // the same indentation.
                        // Therefore, if they don't have the same parent, no selection can be formed.
                        // If they do, select everything in between by finding the indexes of these widgets in
                        // the parent widget list.

                        // There is another scenario that could be considered:
                        // cmd1
                        // - - cmd 2 (selected)
                        // - - cmd 3
                        // cmd 4
                        // cmd 5
                        // - - cmd 6 (origin)
                        // - - cmd 7
                        // What is the ideal scenario here? Select 1, 4, and 5? Just 5? 4 and 5?
                        // Perhaps the best algorithm is one that moves one level up (i.e. to the selection and origin's parents)
                        // and then sees if their parents are equal, and so forth. This would mean select 1, 4, and 5 here.
                        IContainer OriginParent = MainCommandWidget.SelectionOrigin.Parent;
                        BaseCommandWidget CurrentCommand = this;
                        IContainer CurrentParent = CurrentCommand.Parent;
                        if (OriginParent != CurrentParent)
                        {
                            // Not the same parent, so we can't form a valid selection.
                            // Instead, we cancel the selection altogether; because we deselected all at the start
                            // and did not re-select our commands here.
                            while (MainCommandWidget.SelectionOrigin.Indentation > -1)
                            {
                                MainCommandWidget.SelectionOrigin = MainCommandWidget.SelectionOrigin.GetParentCommandWidget(MainCommandWidget.SelectionOrigin.Parent);
                                OriginParent = MainCommandWidget.SelectionOrigin.Parent;
                                CurrentCommand = CurrentCommand.GetParentCommandWidget(CurrentCommand.Parent);
                                CurrentParent = CurrentCommand.Parent;
                                // Parents are equal one level higher.
                                // Now if we want this selection to be formed, we need to move our selection and origin to these widgets.
                                if (OriginParent == CurrentParent)
                                {
                                    // We've already moved the selection origin, so now we just select our parent command and let
                                    // that system form the selection.
                                    // Notice that this algorithm goes back to the very root; we could also simply tell the program to select
                                    // our parent and set the new origin to the old origin's parent, and go through the whole process again.
                                    // While this is also valid, this is less recursive and just slightly more tangible.
                                    // Not that this entire selection process is tangible, but this just made the most sense to me.
                                    CurrentCommand.SetSelected(true, true);
                                    return;
                                }
                            }
                            return;
                        }
                        int idx1 = OriginParent.Widgets.IndexOf(MainCommandWidget.SelectionOrigin);
                        int idx2 = OriginParent.Widgets.IndexOf(this);
                        int min = Math.Min(idx1, idx2);
                        int max = Math.Max(idx1, idx2);
                        for (int i = min; i <= max; i++)
                        {
                            // i is the index of a command widget in the parent widget list.
                            // These are all between the currently selected command and the origin,
                            // so we select them all.
                            BaseCommandWidget cmd = (BaseCommandWidget) OriginParent.Widgets[i];
                            cmd.SetSelectedInternal(true, false);
                            if (cmd == this) SkipInternal = true;
                        }
                    }
                }
            }
            SetSelectedInternal(Selected, SkipInternal);
        }
        else if (Selected && !SelectedWidget) WidgetSelected(new BaseEventArgs());
    }

    protected void SetSelectedInternal(bool Selected, bool SkipRegistry)
    {
        this.Selected = Selected;
        if (!SkipRegistry) MainCommandWidget.SelectedWidgets.Add(this);
        SubcommandWidgets.ForEach(s => s.SetSelectedInternal(this.Selected, false));
        if (this.Selected) SetAsSelected();
        else SetAsDeselected();
    }

    protected virtual void SetAsSelected()
    {
        WidgetSelected(new BaseEventArgs());
        BarBox.SetColor(GradientBox.TopLeftColor);
        GradientBox.SetBottomRightColor(GradientBox.TopLeftColor);
        Icon.SetColor(Color.WHITE);
    }

    protected virtual void SetAsDeselected()
    {
        if (SelectedWidget) Window.UI.SetSelectedWidget(null);
        SetCommandColors();
    }

    protected void SelectNormally()
    {
        SetSelected(true, Input.Press(Keycode.SHIFT));
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
        HandleStandardInput(e);
    }

    protected virtual void HandleStandardInput(MouseEventArgs e)
    {
        if (e.Handled || InsideChild())
        {
            CancelDoubleClick();
            return;
        }
        bool HoldingShift = Input.Press(Keycode.SHIFT);
        if (this.Indentation == -1)
        {
            if (!VStackPanel.Mouse.Inside) SubcommandWidgets[^1].SetSelected(true, HoldingShift);
        }
        else SetSelected(true, HoldingShift);
    }

    protected virtual bool IsEditable()
    {
        return this is not BlankWidget && MainCommandWidget.SelectedWidgets.Count < 2;
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

    protected BaseCommandWidget InsertSingle(List<EventCommand> Commands, bool Undoable = true)
    {
        if (this.Indentation == -1) return null;
        // Get the global index for our new commands
        int GlobalIndex = this.GlobalCommandIndex;
        // Deselect this widget
        this.SetSelected(false, false);
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
        NewWidget.SetSelected(true, false);
        if (Undoable) MainCommandWidget.RegisterUndoAction(new CommandChangeUndoAction(MainCommandWidget, GlobalIndex, Commands, true));
        MainCommandWidget.RemoveNoCommandsBG();
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
            InsertSingle(win.Commands);
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
            UpdateSize();
        });
    }

    protected virtual void Edit(EditEvent Continue)
    {

    }

    protected void Cut()
    {
        if (this.Indentation == -1 || !Viewport.Visible) return;
        Copy();
        DeleteMayBeMultiple();
    }

    protected (List<EventCommand> EventCommands, List<BaseCommandWidget> CommandWidgets) GetListOfSelectedCommands()
    {
        List<EventCommand> EventCommands = new List<EventCommand>();
        List<BaseCommandWidget> AddedWidgets = new List<BaseCommandWidget>();
        List<BaseCommandWidget> CommandWidgets = new List<BaseCommandWidget>();
        void RegisterWidget(BaseCommandWidget cmd)
        {
            AddedWidgets.AddRange(cmd.SubcommandWidgets);
            cmd.SubcommandWidgets.ForEach(c => RegisterWidget(c));
        }
        MainCommandWidget.SelectedWidgets.Sort(delegate (BaseCommandWidget bcw1, BaseCommandWidget bcw2) { return bcw1.Indentation.CompareTo(bcw2.Indentation); });
        for (int i = 0; i < MainCommandWidget.SelectedWidgets.Count; i++)
        {
            // Ensures we don't add commands that are part of an included parent command,
            // because said parent command has all child commands in its command list.
            if (AddedWidgets.Contains(MainCommandWidget.SelectedWidgets[i])) continue;
            EventCommands.AddRange(MainCommandWidget.SelectedWidgets[i].Commands);
            CommandWidgets.Add(MainCommandWidget.SelectedWidgets[i]);
            AddedWidgets.Add(MainCommandWidget.SelectedWidgets[i]);
            RegisterWidget(MainCommandWidget.SelectedWidgets[i]);
        }
        return (EventCommands, CommandWidgets);
    }

    protected void Copy()
    {
        if (this.Indentation == -1 || !Viewport.Visible) return;
        PurgeBlanksFromSelection();
        (List<EventCommand> Commands, _) = GetListOfSelectedCommands();
        if (Commands.Count == 0) return;
        List<EventCommand> data = new List<EventCommand>(Commands);
        Clipboard.SetObject(data, BinaryData.EVENT_COMMANDS);
    }

    protected void Paste()
    {
        if (this.Indentation == -1 || !Clipboard.IsValid(BinaryData.EVENT_COMMANDS)) return;
        List<EventCommand> data = Clipboard.GetObject<List<EventCommand>>();
        InsertMayBeMultiple(data);
    }

    protected void SelectAll()
    {
        MainCommandWidget.DeselectAll();
        MainCommandWidget.SelectionOrigin = MainCommandWidget.SubcommandWidgets[0];
        MainCommandWidget.SubcommandWidgets[^1].SetSelected(true, true);
    }

    protected BaseCommandWidget DeleteSingleSelected(bool Undoable = true)
    {
        if (this.Indentation == -1 || this is BlankWidget || !Viewport.Visible) return null;
        // Get the global index for our current command
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
        NewSelectedWidget.SetSelected(true, false);
        // Shortcut timing
        Shortcut s = NewSelectedWidget.Shortcuts.Find(s => s.Key.MainKey == Keycode.DELETE);
        if (MainCommandWidget.InitialDeleteKey) NewSelectedWidget.SetTimer($"key_{s.Key.ID}_initial", 300);
        else NewSelectedWidget.SetTimer($"key_{s.Key.ID}", 50);
        // Update positionings
        ((VStackPanel) this.Parent).UpdateLayout();
        if (Undoable) MainCommandWidget.RegisterUndoAction(new CommandChangeUndoAction(MainCommandWidget, GlobalIndex, Commands, false));
        if (MainCommandWidget.Commands.Count == 1) MainCommandWidget.CreateNoCommandsBG();
        // Scroll to newly selected command
        NewSelectedWidget.ScrollToThisCommand();
        return NewSelectedWidget;
    }

    List<List<EventCommand>> EventCommandLists = new List<List<EventCommand>>();
    List<EventCommand> TemporaryCommands;

    protected BaseCommandWidget StoreCommandBundle(EventCommand Command, int Count, VStackPanel Parent, int GlobalCommandIndex, int ParentWidgetIndex = -1)
    {
        List<EventCommand> ThisCommandsCommands = TemporaryCommands.GetRange(TemporaryCommands.IndexOf(Command), Count);
        EventCommandLists.Add(ThisCommandsCommands);
        return null;
    }

    protected void InsertMayBeMultiple(List<EventCommand> Commands, bool Undoable = true)
    {
        EventCommandLists.Clear();
        TemporaryCommands = Commands;
        int CurIdx = GlobalCommandIndex;
        ParseCommands(Commands, null, 0, StoreCommandBundle);
        List<BaseCommandWidget> CreatedWidgets = new List<BaseCommandWidget>();
        for (int i = 0; i < EventCommandLists.Count; i++)
        {
            CreatedWidgets.Add(InsertSingle(EventCommandLists[i], false));
        }
        int min = int.MaxValue;
        int max = int.MinValue;
        BaseCommandWidget mincmd = null;
        BaseCommandWidget maxcmd = null;
        foreach (BaseCommandWidget cmd in CreatedWidgets)
        {
            if (cmd.GlobalCommandIndex < min)
            {
                min = cmd.GlobalCommandIndex;
                mincmd = cmd;
            }
            if (cmd.GlobalCommandIndex > max)
            {
                max = cmd.GlobalCommandIndex;
                maxcmd = cmd;
            }
        }
        MainCommandWidget.SelectionOrigin = mincmd;
        mincmd.SetSelected(true, false);
        maxcmd.SetSelected(true, true);
        if (Undoable)
        {
            MainCommandWidget.RegisterUndoAction(new CommandChangeUndoAction(MainCommandWidget, CurIdx, Commands, true));
        }
        EventCommandLists.Clear();
        TemporaryCommands = null;
    }

    protected void DeleteMayBeMultiple(bool Undoable = true)
    {
        PurgeBlanksFromSelection();
        (List<EventCommand> Commands, List<BaseCommandWidget> SelectedWidgets) = GetListOfSelectedCommands();
        if (Commands.Count == 0) return;
        int GlobalCommandIndex = SelectedWidgets.Min(cmd => cmd.GlobalCommandIndex);
        // Make a copy of the old selected widgets, because once delete one of the widgets,
        // the deletion process with select the next widget in the list, thereby clearing our list of
        // selected widgets.
        for (int i = 0; i < SelectedWidgets.Count; i++)
        {
            SelectedWidgets[i].SetSelectedInternal(true, true);
            SelectedWidgets[i].DeleteSingleSelected(false);
        }
        // That is also the reason we don't need to clear the list of selected widgets ourselves; since the deletion
        // process selects a new command when it is done deleting, that will start a new selection of itself.
        if (Undoable)
        {
            MainCommandWidget.RegisterUndoAction(new CommandChangeUndoAction(MainCommandWidget, GlobalCommandIndex, Commands, false));
        }
    }

    protected void PurgeBlanksFromSelection()
    {
        // Remove all preceding blanks (should not be possible)
        while (MainCommandWidget.SelectedWidgets.Count > 0 && MainCommandWidget.SelectedWidgets[0].Command.Code == CommandCode.Blank)
        {
            MainCommandWidget.SelectedWidgets[0].SetSelected(false, false);
            MainCommandWidget.SelectedWidgets.RemoveAt(0);
        }
        // Remove all blanks at the last position in the list with the same indentation as the first item, or remove if it's the only item
        while (MainCommandWidget.SelectedWidgets.Count > 0 && MainCommandWidget.SelectedWidgets[^1].Command.Code == CommandCode.Blank &&
            (MainCommandWidget.SelectedWidgets[^1].Indentation == MainCommandWidget.SelectedWidgets[0].Indentation || MainCommandWidget.SelectedWidgets.Count == 1))
        {
            MainCommandWidget.SelectedWidgets[^1].SetSelected(false, false);
            MainCommandWidget.SelectedWidgets.RemoveAt(MainCommandWidget.SelectedWidgets.Count - 1);
        }
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

    protected void SelectNextCommand(bool HoldingShift)
    {
        BaseCommandWidget NextCommand = GetNextCommand();
        if (NextCommand is not null)
        {
            NextCommand.SetSelected(true, HoldingShift);
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

    protected void SelectPreviousCommand(bool HoldingShift)
    {
        BaseCommandWidget PreviousCommand = GetPreviousCommand();
        if (PreviousCommand is not null)
        {
            PreviousCommand.SetSelected(true, HoldingShift);
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

    protected void SelectPageDown(bool HoldingShift)
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
        NewCommand.SetSelected(true, HoldingShift);
        Shortcut s = NewCommand.Shortcuts.Find(s => s.Key.MainKey == Keycode.PAGEDOWN);
        if (MainCommandWidget.InitialPageDownKey) NewCommand.SetTimer($"key_{s.Key.ID}_initial", 300);
        else NewCommand.SetTimer($"key_{s.Key.ID}", 50);
        NewCommand.ScrollToThisCommand();
    }

    protected void SelectPageUp(bool HoldingShift)
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
        NewCommand.SetSelected(true, HoldingShift);
        Shortcut s = NewCommand.Shortcuts.Find(s => s.Key.MainKey == Keycode.PAGEUP);
        if (MainCommandWidget.InitialPageUpKey) NewCommand.SetTimer($"key_{s.Key.ID}_initial", 300);
        else NewCommand.SetTimer($"key_{s.Key.ID}", 50);
        NewCommand.ScrollToThisCommand();
    }

    protected CommandCategory GetCommandCategory()
    {
        CommandCode code = Command.Code;
        foreach (KeyValuePair<CommandCategory, List<CommandCode>> kvp in CommandTypes)
        {
            if (kvp.Value.Contains(code))
            {
                return kvp.Key;
            }
        }
        return CommandCategory.Other;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        UpdateBackdrops();
    }

    protected virtual void UpdateBackdrops()
    {
        int w = ScaleGradientWithSize ? Size.Width: GetStandardWidth(Indentation);
        GradientBox.SetSize(w - BarWidth - ShadowSize * 2, Size.Height - GradientBox.Padding.Up - ShadowSize);
        ShadowWidget.SetSize(w, Size.Height);
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
            if (IsRedo != Creation)
            {
                // Undo creation + redo deletion
                // This is the first widget to be deleted.
                BaseCommandWidget DeletionWidget = MainCommandWidget.GetCommandAtGlobalIndex(this.GlobalCommandIndex);
                // Select all widgets corresponding with the event commands in Commands
                // It starts at GlobalCommandIndex up to GlobalCommandIndex + Commands.Count, so we find all BaseCommandWidgets
                // between those two global command indexes and select them.
                // We know we can only have selections with the same indentation and the same parent, so looking inside DeletionWidget's
                // parent will be sufficient to get all relevant widgets. We do not need to manually select all child widgets to delete those too.
                int max = this.GlobalCommandIndex + Commands.Count;
                MainCommandWidget.SelectionOrigin = DeletionWidget;
                MainCommandWidget.SelectedWidgets.Clear();
                IContainer Parent = DeletionWidget.Parent;
                foreach (BaseCommandWidget cmd in Parent.Widgets)
                {
                    if (cmd.GlobalCommandIndex >= this.GlobalCommandIndex && cmd.GlobalCommandIndex < max)
                        MainCommandWidget.SelectedWidgets.Add(cmd);
                }
                DeletionWidget.DeleteMayBeMultiple(false);
            }
            else
            {
                // Redo creation + undo deletion
                BaseCommandWidget InsertionWidget = MainCommandWidget.GetCommandAtGlobalIndex(this.GlobalCommandIndex);
                InsertionWidget.InsertMayBeMultiple(Commands, false);
            }
            BaseCommandWidget NewWidget = (BaseCommandWidget) MainCommandWidget.Window.UI.SelectedWidget;
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
            if (MainCommandWidget.Commands.Count == 1) MainCommandWidget.CreateNoCommandsBG();
            else MainCommandWidget.RemoveNoCommandsBG();
            NewWidget.SetSelectedInternal(true, true);
        }
    }
}

public enum CommandCategory
{
    General,
    Flow,
    Map,
    ImageSound,
    Other,
    Blank
}