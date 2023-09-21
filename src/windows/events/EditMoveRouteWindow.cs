using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditMoveRouteWindow : PopupWindow
{
    public bool Apply = false;
    public int EventID = 0;
    public MoveRoute MoveRoute;
    public List<EventCommand> NewCommands;

    Map Map;
    Event Event;
    EventPage Page;
    MoveRoute OldMoveRoute;

    DropdownBox TargetBox;
    ListBox MoveBox;

    Dictionary<MoveCode, Action<MoveCode, MoveCommand?, Action<MoveCommand>>> CommandEditFunctions =
        new Dictionary<MoveCode, Action<MoveCode, MoveCommand?, Action<MoveCommand>>>();

    public EditMoveRouteWindow(Map Map, Event Event, EventPage Page, MoveRoute mr, int EventID, bool ThisEventOnly)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        this.OldMoveRoute = mr;
        this.MoveRoute = (MoveRoute) mr.Clone();

        SetTitle("Edit Move Route");
        MinimumSize = MaximumSize = new Size(765, 580);
        SetSize(MaximumSize);
        Center();
        this.OnWidgetSelected += WidgetSelected;

        TargetBox = new DropdownBox(this);
        TargetBox.SetPosition(15, 40);
        TargetBox.SetSize(180, 25);
        TargetBox.SetText("This event");
        TargetBox.SetEnabled(!ThisEventOnly);

		List<TreeNode> Items = new List<TreeNode>
		{
			new TreeNode("This event"),
			new TreeNode("Player")
		};
		List<int> keys = Map.Events.Keys.ToList();
        keys.Sort();
        for (int i = 0; i < keys.Count; i++)
        {
            Items.Add(new TreeNode($"{Utilities.Digits(keys[i], 3)}: {Map.Events[keys[i]].Name}", keys[i]));
        }
        TargetBox.SetItems(Items);
        if (!ThisEventOnly)
        {
            if (EventID == -1) TargetBox.SetSelectedIndex(1);
            else if (EventID == 0) TargetBox.SetSelectedIndex(0);
            else TargetBox.SetSelectedIndex(TargetBox.Items.FindIndex(i => i.Object is int && (int) i.Object == EventID));
        }

        MoveBox = new ListBox(this);
        MoveBox.SetVDocked(true);
        MoveBox.SetPadding(15, 71, 0, 100);
        MoveBox.SetWidth(180);
        MoveBox.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("Edit")
            {
                IsClickable = e => e.Value = MoveBox.HoveringItem is not null && IsCommandEditable((MoveCommand) MoveBox.HoveringItem.Object),
                OnClicked = _ => EditCommand(MoveBox.HoveringItem)
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = IsRealCommand(MoveBox.HoveringItem),
                OnClicked = _ => CutCommand(MoveBox.HoveringItem)
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = IsRealCommand(MoveBox.HoveringItem),
                OnClicked = _ => CopyCommand(MoveBox.HoveringItem)
            },
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = Clipboard.IsValid(BinaryData.MOVE_COMMAND),
                OnClicked = _ => PasteCommand(MoveBox.HoveringItem)
            },
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = IsRealCommand(MoveBox.HoveringItem),
                OnClicked = _ => DeleteCommand(MoveBox.HoveringItem)
            }
        });

        CheckBox RepeatBox = new CheckBox(this);
        RepeatBox.SetBottomDocked(true);
        RepeatBox.SetPadding(20, 0, 0, 78);
        RepeatBox.SetFont(Fonts.Paragraph);
        RepeatBox.SetText("Repeat");
        RepeatBox.SetChecked(MoveRoute.Repeat);
        RepeatBox.OnCheckChanged += _ => MoveRoute.Repeat = RepeatBox.Checked;

        CheckBox SkippableBox = new CheckBox(this);
        SkippableBox.SetBottomDocked(true);
        SkippableBox.SetPadding(20, 0, 0, 58);
        SkippableBox.SetFont(Fonts.Paragraph);
        SkippableBox.SetText("Skippable");
        SkippableBox.SetChecked(MoveRoute.Skippable);
        SkippableBox.OnCheckChanged += _ => MoveRoute.Skippable = SkippableBox.Checked;

        Grid ButtonGrid = new Grid(this);
        ButtonGrid.SetColumns(
            new GridSize(1),
            new GridSize(1),
            new GridSize(1)
        );
        ButtonGrid.SetDocked(true);
        ButtonGrid.SetPadding(MoveBox.Padding.Left + MoveBox.Size.Width + 8, TargetBox.Position.Y - 5, 15, 48);

        VStackPanel Column1 = new VStackPanel(ButtonGrid);
        VStackPanel Column2 = new VStackPanel(ButtonGrid);
        Column2.SetGridColumn(1);
        VStackPanel Column3 = new VStackPanel(ButtonGrid);
        Column3.SetGridColumn(2);
        VStackPanel[] Columns = new VStackPanel[] { Column1, Column2, Column3 };

        void AddButton(string Text, int Column, MoveCode MoveCode)
        {
            Button btn = new Button(Columns[Column]);
            if (Columns[Column].Widgets.Count > 1)
                btn.SetPadding(0, -3);
            btn.SetText(Text);
            btn.OnClicked += _ =>
            {
                MoveCommand cmd = new MoveCommand(MoveCode, new List<object>());
                InsertCommand(MoveBox.SelectedIndex, cmd);
            };
        };

        void AddButtonElaborate(string Text, int Column, MoveCode MoveCode, Action<MoveCode, MoveCommand?, Action<MoveCommand>> OnClickEvent)
        {
            Button btn = new Button(Columns[Column]);
            if (Columns[Column].Widgets.Count > 1)
                btn.SetPadding(0, -3);
            btn.SetText(Text);
            btn.OnClicked += _ =>
            {
                OnClickEvent(MoveCode, null, cmd =>
                {
                    InsertCommand(MoveBox.SelectedIndex, cmd);
                    Window.UI.SetSelectedWidget(this);
                });
            };
            CommandEditFunctions.Add(MoveCode, OnClickEvent);
        };
        
        AddButton("Move Down", 0, MoveCode.Down);
        AddButton("Move Left", 0, MoveCode.Left);
        AddButton("Move Right", 0, MoveCode.Right);
        AddButton("Move Up", 0, MoveCode.Up);
        AddButton("Move Lower Left", 0, MoveCode.LowerLeft);
        AddButton("Move Lower Right", 0, MoveCode.LowerRight);
        AddButton("Move Upper Left", 0, MoveCode.UpperLeft);
        AddButton("Move Upper Right", 0, MoveCode.UpperRight);
        AddButton("Move at Random", 0, MoveCode.Random);
        AddButton("Move toward Player", 0, MoveCode.TowardPlayer);
        AddButton("Move away from Player", 0, MoveCode.AwayFromPlayer);
        AddButton("Move Forward", 0, MoveCode.Forward);
        AddButton("Move Backward", 0, MoveCode.Backward);
        AddButtonElaborate("Jump...", 0, MoveCode.Jump, (code, cmd, Add) =>
        {
            GenericDoubleNumberPicker win = new GenericDoubleNumberPicker("Jump", "X:", (int) (long) (cmd?.Parameters[0] ?? 0L), null, null, "Y:", (int) (long) (cmd?.Parameters[1] ?? 0L), null, null);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long) win.Value1, (long) win.Value2 }));
            };
        });
        AddButtonElaborate("Wait...", 0, MoveCode.Wait, (code, cmd, Add) =>
        {
            GenericNumberPicker win = new GenericNumberPicker("Wait", "Wait time:", (int) (long) (cmd?.Parameters[0] ?? 4L), 1, null, "frames");
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long) win.Value }));
            };
        });

        AddButton("Turn Down", 1, MoveCode.TurnDown);
        AddButton("Turn Left", 1, MoveCode.TurnLeft);
        AddButton("Turn Right", 1, MoveCode.TurnRight);
        AddButton("Turn Up", 1, MoveCode.TurnUp);
        AddButton("Turn 90° Right", 1, MoveCode.TurnRight90);
        AddButton("Turn 90° Left", 1, MoveCode.TurnLeft90);
        AddButton("Turn 180°", 1, MoveCode.Turn180);
        AddButton("Turn 90° Right or Left", 1, MoveCode.TurnRightOrLeft90);
        AddButton("Turn at Random", 1, MoveCode.TurnRandom);
        AddButton("Turn toward Player", 1, MoveCode.TurnTowardPlayer);
        AddButton("Turn away from Player", 1, MoveCode.TurnAwayFromPlayer);
        AddButtonElaborate("Switch ON...", 1, MoveCode.SwitchOn, (code, cmd, Add) =>
        {
            SwitchPicker win = new SwitchPicker((int) (long) (cmd?.Parameters[0] ?? 1L));
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long) win.SwitchID }));
            };
        });
        AddButtonElaborate("Switch OFF...", 1, MoveCode.SwitchOff, (code, cmd, Add) =>
        {
            SwitchPicker win = new SwitchPicker((int) (long) (cmd?.Parameters[0] ?? 1L));
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long)win.SwitchID }));
            };
        });
        AddButtonElaborate("Change Speed...", 1, MoveCode.ChangeSpeed, (code, cmd, Add) =>
        {
            GenericDropdownPicker win = new GenericDropdownPicker("Change Speed", "Speed:", (int) (long) (cmd?.Parameters[0] ?? 3L) - 1, new List<string>() { "1: Slowest", "2: Slower", "3: Slow", "4: Fast", "5: Faster", "6: Fastest" });
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long) win.Value + 1 }));
            };
        });
        AddButtonElaborate("Change Freq...", 1, MoveCode.ChangeFreq, (code, cmd, Add) =>
        {
            GenericDropdownPicker win = new GenericDropdownPicker("Change Freq", "Freq:", (int) (long) (cmd?.Parameters[0] ?? 3L) - 1, new List<string>() { "1: Lowest", "2: Lower", "3: Low", "4: High", "5: Higher", "6: Highest" });
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long) win.Value + 1 }));
            };
        });

        AddButton("Move Animation ON", 2, MoveCode.WalkAnimeOn);
        AddButton("Move Animation OFF", 2, MoveCode.WalkAnimeOff);
        AddButton("Stop Animation ON", 2, MoveCode.StepAnimeOn);
        AddButton("Stop Animation OFF", 2, MoveCode.StepAnimeOff);
        AddButton("Direction Fix ON", 2, MoveCode.DirectionFixOn);
        AddButton("Direction Fix OFF", 2, MoveCode.DirectionFixOff);
        AddButton("Through ON", 2, MoveCode.ThroughOn);
        AddButton("Through OFF", 2, MoveCode.ThroughOff);
        AddButton("Always on Top ON", 2, MoveCode.AlwaysOnTopOn);
        AddButton("Always on Top OFF", 2, MoveCode.AlwaysOnTopOff);
        AddButtonElaborate("Change Graphic...", 2, MoveCode.Graphic, (code, cmd, Add) =>
        {
            EventGraphic gr = new EventGraphic("");
            if (cmd != null)
            {
                gr.CharacterName = (string) cmd.Parameters[0];
                gr.CharacterHue = (int) (long) cmd.Parameters[1];
                gr.Direction = (int) (long) cmd.Parameters[2];
                gr.Pattern = (int) (long) cmd.Parameters[3];
            }
            ChooseGraphicFileExplorerWindow win = new ChooseGraphicFileExplorerWindow(Map, Event, Page, gr, true, "Graphics/Characters");
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { win.Graphic.CharacterName, (long) win.Graphic.CharacterHue, (long) win.Graphic.Direction, (long) win.Graphic.Pattern }));
            };
        });
        AddButtonElaborate("Change Opacity...", 2, MoveCode.Opacity, (code, cmd, Add) =>
        {
            GenericNumberPicker win = new GenericNumberPicker("Opacity", "Opacity:", (int) (long) (cmd?.Parameters[0] ?? 255L), 0, 255);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long) win.Value }));
            };
        });
        AddButtonElaborate("Change Blending...", 2, MoveCode.Blending, (code, cmd, Add) =>
        {
            GenericDropdownPicker win = new GenericDropdownPicker("Blending", "Mode:", (int) (long) (cmd?.Parameters[0] ?? 0L), new List<string>() { "Normal", "Add", "Sub" });
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { (long) win.Value }));
            };
        });
        AddButtonElaborate("Play SE...", 2, MoveCode.PlaySE, (code, cmd, Add) =>
        {
            AudioFile? af = (AudioFile) cmd?.Parameters[0] ?? null;
            AudioPicker win = new AudioPicker("Audio/SE", af?.Name ?? "", af?.Volume ?? 80, af?.Pitch ?? 100);
            win.OnClosed += _ =>
            {
                if (win.Result != null)
                {
                    Add(new MoveCommand(code, new List<object>() { new AudioFile(win.Result?.Filename, (int) win.Result?.Volume, (int) win.Result?.Pitch) }));
                }
            };
        });
        AddButtonElaborate("Script...", 2, MoveCode.Script, (code, cmd, Add) =>
        {
            GenericTextBoxWindow win = new GenericTextBoxWindow("Script", "Code:", (string) (cmd?.Parameters[0] ?? ""), true);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Add(new MoveCommand(code, new List<object>() { win.Value }));
            };
        });

        RedrawMoves();

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        MoveBox.RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.DELETE), _ => DeleteCommand(MoveBox.SelectedItem)),
            new Shortcut(this, new Key(Keycode.SPACE), _ => EditCommand(MoveBox.SelectedItem)),
            new Shortcut(this, new Key(Keycode.X, Keycode.CTRL), _ => CutCommand(MoveBox.SelectedItem)),
            new Shortcut(this, new Key(Keycode.C, Keycode.CTRL), _ => CopyCommand(MoveBox.SelectedItem)),
            new Shortcut(this, new Key(Keycode.V, Keycode.CTRL), _ => PasteCommand(MoveBox.SelectedItem)),
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveBox.MoveDown()),
            new Shortcut(this, new Key(Keycode.UP), _ => MoveBox.MoveUp()),
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        this.Window.UI.SetSelectedWidget(this);
    }

    private void RedrawMoves()
    {
        List<TreeNode> Items = new List<TreeNode>();
        for (int i = 0; i < this.MoveRoute.Commands.Count; i++)
        {
            MoveCommand cmd = this.MoveRoute.Commands[i];
            Items.Add(new TreeNode(cmd.ToString(), cmd));
        }
        MoveBox.SetItems(Items);
        if (MoveBox.SelectedIndex == -1) MoveBox.SetSelectedIndex(0);
    }

    private bool IsRealCommand(TreeNode item)
    {
        return item is not null && ((MoveCommand) item.Object).Code != MoveCode.None;
    }

    private bool IsCommandEditable(MoveCommand Command)
    {
        return CommandEditFunctions.ContainsKey(Command.Code);
    }

    private void DeleteCommand(TreeNode item)
    {
        if (!IsRealCommand(item)) return;
        this.MoveRoute.Commands.RemoveAt(MoveBox.Items.IndexOf(item));
        this.MoveBox.RemoveItem(item);
    }

    private void InsertCommand(int Index, MoveCommand Command)
    {
        MoveRoute.Commands.Insert(Index, Command);
		TreeNode Item = new TreeNode(Command.ToString(), Command);
        MoveBox.InsertItem(Index, Item);
        MoveBox.SetSelectedIndex(MoveBox.Items.IndexOf(Item) + 1);
    }

    private void EditCommand(TreeNode item)
    {
        if (!IsCommandEditable((MoveCommand) item.Object) || !IsRealCommand(item)) return;
        MoveCommand cmd = (MoveCommand) item.Object;
        Action<MoveCode, MoveCommand, Action<MoveCommand>> edit = CommandEditFunctions[cmd.Code];
        edit(cmd.Code, cmd, newcmd =>
        {
            int idx = MoveRoute.Commands.IndexOf(cmd);
            MoveRoute.Commands[idx] = newcmd;
            TreeNode node = MoveBox.Items[idx];
            node.SetText(newcmd.ToString());
            node.SetObject(newcmd);
            MoveBox.RedrawItem(node);
            Window.UI.SetSelectedWidget(this);
        });
    }

    private void CutCommand(TreeNode item)
    {
        if (!IsRealCommand(item)) return;
        CopyCommand(item);
        DeleteCommand(item);
    }

    private void CopyCommand(TreeNode item)
    {
        if (!IsRealCommand(item)) return;
        MoveCommand cmd = (MoveCommand) item.Object;
        Clipboard.SetObject(cmd, BinaryData.MOVE_COMMAND);
    }

    private void PasteCommand(TreeNode item)
    {
        if (!Clipboard.IsValid(BinaryData.MOVE_COMMAND)) return;
        MoveCommand cmd = Clipboard.GetObject<MoveCommand>();
        InsertCommand(item is null ? MoveBox.Items.Count - 1 : MoveBox.Items.IndexOf(item), cmd);
    }

    private void OK()
    {
        Apply = true;
        if (TargetBox.Enabled)
        {
            if (TargetBox.SelectedIndex == 0) this.EventID = 0;
            else if (TargetBox.SelectedIndex == 1) this.EventID = -1;
            else this.EventID = (int) TargetBox.Items[TargetBox.SelectedIndex].Object;
        }
        NewCommands = new List<EventCommand>()
        {
            new EventCommand(CommandCode.SetMoveRoute, 0, new List<object>() { (long) EventID, MoveRoute })
        };
        for (int i = 0; i < MoveRoute.Commands.Count; i++)
        {
            NewCommands.Add(new EventCommand(CommandCode.MoreMoveRoute, 0, new List<object>() { MoveRoute.Commands[i] }));
        }
        Close();
    }

    private void Cancel()
    {
        this.MoveRoute = this.OldMoveRoute;
        Close();
    }
}
