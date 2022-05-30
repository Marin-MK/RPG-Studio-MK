using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditMoveRouteWindow : PopupWindow
{
    public bool Apply = false;
    public MoveRoute MoveRoute;

    Map Map;
    Event Event;
    MoveRoute OldMoveRoute;

    DropdownBox TargetBox;
    ListBox MoveBox;

    public EditMoveRouteWindow(Map Map, Event Event, MoveRoute mr)
    {
        this.Map = Map;
        this.Event = Event;
        this.OldMoveRoute = mr;
        this.MoveRoute = (MoveRoute) mr.Clone();

        SetTitle("Edit Move Route");
        MinimumSize = MaximumSize = new Size(680, 500);
        SetSize(MaximumSize);
        Center();
        this.OnWidgetSelected += WidgetSelected;

        TargetBox = new DropdownBox(this);
        TargetBox.SetPosition(15, 35);
        TargetBox.SetSize(180, 25);
        TargetBox.SetText("This event");

        MoveBox = new ListBox(this);
        MoveBox.SetVDocked(true);
        MoveBox.SetMargins(15, 64, 0, 64);
        MoveBox.SetWidth(180);

        Grid ButtonGrid = new Grid(this);
        ButtonGrid.SetColumns(
            new GridSize(1),
            new GridSize(1),
            new GridSize(1)
        );
        ButtonGrid.SetDocked(true);
        ButtonGrid.SetMargins(MoveBox.Margins.Left + MoveBox.Size.Width + 8, TargetBox.Position.Y, 15, 48);

        VStackPanel Column1 = new VStackPanel(ButtonGrid);
        VStackPanel Column2 = new VStackPanel(ButtonGrid);
        Column2.SetGridColumn(1);
        VStackPanel Column3 = new VStackPanel(ButtonGrid);
        Column3.SetGridColumn(2);
        VStackPanel[] Columns = new VStackPanel[] { Column1, Column2, Column3 };

        Action<string, int, MoveCode> AddButton = delegate (string Text, int Column, MoveCode MoveCode)
        {
            Button btn = new Button(Columns[Column]);
            btn.SetFont(Fonts.ProductSansMedium.Use(14));
            if (Columns[Column].Widgets.Count > 1)
                btn.SetMargins(0, -3);
            btn.SetText(Text);
            btn.OnClicked += _ =>
            {
                MoveCommand cmd = new MoveCommand(MoveCode, new List<object>());
                InsertCommand(MoveBox.SelectedIndex, cmd);
            };
        };

        Action<string, int, MoveCode, Action<MoveCode>> AddButtonElaborate = delegate (string Text, int Column, MoveCode MoveCode, Action<MoveCode> OnClickEvent)
        {
            Button btn = new Button(Columns[Column]);
            btn.SetFont(Fonts.ProductSansMedium.Use(14));
            if (Columns[Column].Widgets.Count > 1)
                btn.SetMargins(0, -3);
            btn.SetText(Text);
            btn.OnClicked += _ => OnClickEvent(MoveCode);
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
        AddButtonElaborate("Jump...", 0, MoveCode.Jump, code =>
        {
            GenericDoubleNumberPicker win = new GenericDoubleNumberPicker("Jump", "X:", 0, null, null, "Y:", 0, null, null);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                MoveCommand cmd = new MoveCommand(code, new List<object>() { (long) win.Value1, (long) win.Value2 });
                InsertCommand(MoveBox.SelectedIndex, cmd);
            };
        });
        AddButtonElaborate("Wait...", 0, MoveCode.Wait, code =>
        {
            GenericNumberPicker win = new GenericNumberPicker("Wait", "Wait time:", 4, 1, null);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                MoveCommand cmd = new MoveCommand(code, new List<object>() { (long) win.Value });
                InsertCommand(MoveBox.SelectedIndex, cmd);
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
        AddButtonElaborate("Switch ON...", 1, MoveCode.SwitchOn, code =>
        {
            SwitchPicker win = new SwitchPicker(1);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                MoveCommand cmd = new MoveCommand(code, new List<object>() { (long) win.SwitchID });
                InsertCommand(MoveBox.SelectedIndex, cmd);
            };
        });
        AddButtonElaborate("Switch OFF...", 1, MoveCode.SwitchOff, code =>
        {
            SwitchPicker win = new SwitchPicker(1);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                MoveCommand cmd = new MoveCommand(code, new List<object>() { (long)win.SwitchID });
                InsertCommand(MoveBox.SelectedIndex, cmd);
            };
        });
        AddButtonElaborate("Change Speed...", 1, MoveCode.ChangeSpeed, code =>
        {
            GenericDropdownPicker win = new GenericDropdownPicker("Change Speed", "Speed:", 2, new List<string>() { "1: Slowest", "2: Slower", "3: Slow", "4: Fast", "5: Faster", "6: Fastest" });
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                MoveCommand cmd = new MoveCommand(code, new List<object>() { (long) win.Value + 1 });
                InsertCommand(MoveBox.SelectedIndex, cmd);
            };
        });
        AddButtonElaborate("Change Freq...", 1, MoveCode.ChangeFreq, code =>
        {
            GenericDropdownPicker win = new GenericDropdownPicker("Change Freq", "Freq:", 2, new List<string>() { "1: Lowest", "2: Lower", "3: Low", "4: High", "5: Higher", "6: Highest" });
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                MoveCommand cmd = new MoveCommand(code, new List<object>() { (long) win.Value + 1 });
                InsertCommand(MoveBox.SelectedIndex, cmd);
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
        AddButtonElaborate("Change Graphic...", 2, MoveCode.Graphic, code =>
        {
            // Name, hue, direction, pattern
        });
        // Change Opacity...
        // Change Blending...
        // Play SE...
        // Script...

        RedrawMoves();

        CreateButton("Cancel", Cancel);
        CreateButton("OK", OK);

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.DELETE), _ => DeleteCommand())
        });

        this.Window.UI.SetSelectedWidget(this);
    }

    private void RedrawMoves()
    {
        List<ListItem> Items = new List<ListItem>();
        for (int i = 0; i < this.MoveRoute.Commands.Count; i++)
        {
            MoveCommand cmd = this.MoveRoute.Commands[i];
            Items.Add(new ListItem(cmd.ToString(), cmd));
        }
        MoveBox.SetItems(Items);
        if (MoveBox.SelectedIndex == -1) MoveBox.SetSelectedIndex(0);
    }

    private void DeleteCommand()
    {
        if (this.MoveRoute.Commands[MoveBox.SelectedIndex].Code == MoveCode.None) return;
        this.MoveRoute.Commands.RemoveAt(MoveBox.SelectedIndex);
        this.MoveBox.Items.RemoveAt(MoveBox.SelectedIndex);
        if (MoveBox.SelectedIndex == MoveBox.Items.Count) MoveBox.SetSelectedIndex(MoveBox.Items.Count - 1);
        MoveBox.Redraw();
    }

    private void InsertCommand(int Index, MoveCommand Command)
    {
        MoveRoute.Commands.Insert(Index, Command);
        ListItem Item = new ListItem(Command.ToString(), Command);
        MoveBox.Items.Insert(Index, Item);
        MoveBox.SetSelectedIndex(Index + 1);
    }

    private void OK(BaseEventArgs e)
    {
        Apply = true;
        Close();
    }

    private void Cancel(BaseEventArgs e)
    {
        this.MoveRoute = this.OldMoveRoute;
        Close();
    }
}
