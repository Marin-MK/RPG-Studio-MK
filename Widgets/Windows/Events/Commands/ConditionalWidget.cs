using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class ConditionalWidget : BaseCommandWidget
{
    Label ConditionLabel;
    Label ElseLabel;
    Label EndLabel;
    VStackPanel VStackPanel1;
    VStackPanel VStackPanel2;
    ExpandArrow ExpandIfArrow;
    ExpandArrow ExpandElseArrow;

    public ConditionalWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex, new Color(128, 128, 255))
    {
        ConditionLabel = new Label(this);
        ConditionLabel.SetPosition(24, 2);
        ConditionLabel.SetFont(Fonts.CabinMedium.Use(10));
        ConditionLabel.SetTextColor(HeaderLabel.TextColor);
        ElseLabel = new Label(this);
        ElseLabel.SetFont(Fonts.CabinMedium.Use(9));
        ElseLabel.SetText("Else");
        ElseLabel.SetTextColor(HeaderLabel.TextColor);
        EndLabel = new Label(this);
        EndLabel.SetFont(Fonts.CabinMedium.Use(9));
        EndLabel.SetText("End");
        EndLabel.SetTextColor(HeaderLabel.TextColor);
        ExpandIfArrow = new ExpandArrow(this);
        ExpandIfArrow.SetExpanded(true);
        ExpandIfArrow.OnStateChanged += _ =>
        {
            VStackPanel1.SetVisible(ExpandIfArrow.Expanded);
            UpdateHeight();
        };
        ExpandElseArrow = new ExpandArrow(this);
        ExpandElseArrow.SetExpanded(true);
        ExpandElseArrow.OnStateChanged += _ =>
        {
            VStackPanel2.SetVisible(ExpandElseArrow.Expanded);
            UpdateHeight();
        };
        OnSizeChanged += _ =>
        {
            ConditionLabel.SetWidthLimit(Size.Width - ConditionLabel.Position.X - 22);
            ExpandIfArrow.SetPosition(Size.Width - 16, 6);
            ExpandElseArrow.SetPosition(Size.Width - 16, ElseLabel.Position.Y);
        };
        //DrawEndLabels = false;
    }

    private void UpdateLabels()
    {
        int endy = 0;
        if (ElseLabel.Visible)
        {
            int elsey = VStackPanel.Padding.Up + VStackPanel1.Size.Height;
            if (!VStackPanel1.Visible) elsey = 22;
            ElseLabel.SetPosition(8, elsey + 4);
            endy = elsey + VStackPanel2.Margins.Up + VStackPanel2.Size.Height;
            if (!VStackPanel2.Visible) endy = ElseLabel.Visible ? elsey + 22 : 0;
        }
        else
        {
            endy = VStackPanel.Padding.Up + VStackPanel1.Size.Height;
            if (!VStackPanel1.Visible) endy = 22;
        }
        EndLabel.SetPosition(8, endy + 4);
        EndLabel.SetVisible(DrawEndLabels);
        if (DrawEndLabels)
        {
            if (VStackPanel1.Visible && VStackPanel2.Visible) HeightAdd = 22;
            else if (VStackPanel1.Visible && !VStackPanel2.Visible) HeightAdd = ElseLabel.Visible ? 44 : 22;
            else if (!VStackPanel1.Visible && VStackPanel2.Visible) HeightAdd = 22;
            else HeightAdd = ElseLabel.Visible ? 44 : 22;
        }
        else
        {
            HeightAdd = ElseLabel.Visible ? VStackPanel2.Visible ? 0 : 22 : 0;
        }
    }

    public override void LoadCommand()
    {
        // Draw conditional
        HeaderLabel.SetText("If: ");
        if ((ConditionType) Int(0) == ConditionType.Script)
        {
            ConditionLabel.SetFont(Fonts.FiraCode.Use(9));
            ConditionLabel.SetPosition(ConditionLabel.Position.X, 3);
        }
        else
        {
            ConditionLabel.SetFont(Fonts.CabinMedium.Use(9));
            ConditionLabel.SetPosition(ConditionLabel.Position.X, 2);
        }
        ConditionLabel.SetText(GetConditionText());

        VStackPanel1?.Dispose();
        VStackPanel2?.Dispose();

        VStackPanel1 = new VStackPanel(VStackPanel);
        VStackPanel1.SetHDocked(true);

        VStackPanel2 = new VStackPanel(VStackPanel);
        VStackPanel2.SetHDocked(true);
        VStackPanel2.SetMargins(0, 22, 0, 0);

        EventCommand ElseCmd = Commands.Find(c => c.Code == CommandCode.BranchElse && c.Indent == Command.Indent);
        int ElseCmdIdx = Commands.IndexOf(ElseCmd);
        if (ElseCmd != null)
        {
            // Draw true commands
            int gidx = this.GlobalCommandIndex + 1;
            ParseCommands(Commands.GetRange(1, ElseCmdIdx - 1), VStackPanel1, gidx);
            gidx += ElseCmdIdx; // + 1 for the BranchElse command
            VStackPanel1.UpdateLayout();

            // Draw false commands
            ParseCommands(Commands.GetRange(ElseCmdIdx + 1, Commands.Count - ElseCmdIdx - 2), VStackPanel2, gidx);
            VStackPanel2.UpdateLayout();
            ExpandElseArrow.SetVisible(true);
            VStackPanel2.SetVisible(true);
        }
        else
        {
            // Draw true commands
            ParseCommands(Commands.GetRange(1, Commands.Count - 2), VStackPanel1, this.GlobalCommandIndex + 1);
            VStackPanel1.UpdateLayout();
            ElseLabel.SetVisible(false);
            ExpandElseArrow.SetVisible(false);
            VStackPanel2.SetVisible(false);
        }
        UpdateLabels();

        VStackPanel1.OnSizeChanged += _ =>
        {
            UpdateLabels();
            UpdateHeight();
        };
        VStackPanel2.OnSizeChanged += _ =>
        {
            UpdateLabels();
            UpdateHeight();
        };
    }

    private int Int(int Index)
    {
        return (int) (long) Command.Parameters[Index];
    }

    private string OnOff(int Index)
    {
        return Int(Index) == 0 ? "ON" : "OFF";
    }

    private string String(int ParameterIndex)
    {
        return (string) Command.Parameters[ParameterIndex];
    }

    private string Switch(int SwitchID)
    {
        return $"Switch [{Utilities.Digits(SwitchID, 3)}: {Data.System.Switches[SwitchID - 1]}]";
    }

    private string Variable(int VariableID)
    {
        return $"Variable [{Utilities.Digits(VariableID, 3)}: {Data.System.Variables[VariableID - 1]}]";
    }

    private char SelfSwitch(int SelfSwitchID)
    {
        return (char) ('A' + SelfSwitchID);
    }

    private string EventName(int EventID)
    {
        if (Map.Events.ContainsKey(EventID)) return $"[{Map.Events[EventID].Name}]";
        else return $"[{Utilities.Digits(EventID, 3)}]";
    }

    private string Dir(int Direction, bool Capitalize = false)
    {
        string s = Direction switch
        {
            2 => "down",
            4 => "left",
            6 => "right",
            8 => "up",
            _ => "unknown direction"
        };
        if (Capitalize) return s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length - 1);
        else return s;
    }

    private string Button(int Button)
    {
        return Button switch
        {
            2 or 4 or 6 or 8 => Dir(Button, true),
            11 => "A",
            12 => "B",
            13 => "C",
            14 => "X",
            15 => "Y",
            16 => "Z",
            17 => "L",
            18 => "R",
            _ => "Unknown"
        };
    }

    private string Operator(int ID)
    {
        return ID switch
        {
            0 => "==",
            1 => ">=",
            2 => "<=",
            3 => ">",
            4 => "<",
            5 => "!=",
            _ => "???"
        };
    }

    private string GetConditionText()
    {
        return (ConditionType) Int(0) switch
        {
            ConditionType.Switch => $"{Switch(Int(1))} is {OnOff(Int(2))}",
            ConditionType.Variable => $"{Variable(Int(1))} {Operator(Int(4))} {(Int(2) == 0 ? Int(3) : Variable(Int(3)))}",
            ConditionType.SelfSwitch => $"Self Switch {String(1)} is {OnOff(Int(2))}",
            ConditionType.Timer => $"Timer {Int(1) / 60}min {Int(1) % 60}sec or {(Int(2) == 0 ? "more" : "less")}",
            ConditionType.Actor => $"Actor condition",
            ConditionType.Enemy => $"Enemy condition",
            ConditionType.Character => $"{EventName(Int(1))} is facing {Dir(Int(2))}",
            ConditionType.Gold => $"Money {(Int(2) == 0 ? ">=" : "<=")} {Int(1)}",
            ConditionType.Item => $"Item condition",
            ConditionType.Weapon => $"Weapon condition",
            ConditionType.Armor => $"Armor condition",
            ConditionType.Button => $"The {Button(Int(1))} button is held down",
            ConditionType.Script => String(1),
            _ => "Unknown condition"
        };
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        ExpandIfArrow.SetVisible(false);
        ExpandElseArrow.SetVisible(false);
        if (!Mouse.Inside) return;
        if (ElseLabel.Visible)
        {
            int ry = e.Y - Viewport.Y + TopCutOff;
            ExpandIfArrow.SetVisible(ry < ElseLabel.Position.Y);
            ExpandElseArrow.SetVisible(!ExpandIfArrow.Visible);
        }
        else ExpandIfArrow.SetVisible(true);
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        int ry = e.Y - Viewport.Y + TopCutOff;
        if (e.Handled || this.Indentation == -1 || InsideChild() || ExpandIfArrow.Mouse.Inside || ExpandElseArrow.Mouse.Inside)
        {
            CancelDoubleClick();
            return;
        }
        if (ry < StandardHeight || ry >= ElseLabel.Position.Y && ry < ElseLabel.Position.Y + StandardHeight) SetSelected(true);
    }
}

public enum ConditionType
{
    Switch = 0,
    Variable = 1,
    SelfSwitch = 2,
    Timer = 3,
    Actor = 4,
    Enemy = 5,
    Character = 6,
    Gold = 7,
    Item = 8,
    Weapon = 9,
    Armor = 10,
    Button = 11,
    Script = 12
}