using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class SetVariableWidget : BaseCommandWidget
{
    public SetVariableWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex, new Color(255, 255, 128)) { }

    public override void LoadCommand()
    {
        base.LoadCommand();
        int VariableID1 = Int(0);
        int VariableID2 = Int(1);
        string Operator = Int(2) switch
        {
            0 => "=",
            1 => "+=",
            2 => "-=",
            3 => "*=",
            4 => "/=",
            5 => "%=",
            _ => "?"
        };
        string Value = (int) (long) Command.Parameters[3] switch
        {
            0 => Int(4).ToString(),
            1 => GetVariableText(Int(4)),
            2 => $"rand({Int(4)}...{Int(5)})",
            3 => "Item variable",
            4 => "Actor variable",
            5 => "Enemy variable",
            6 => Int(5) switch
            {
                0 => $"[{EventName(Int(4))}]'s MapX",
                1 => $"[{EventName(Int(4))}]'s MapY",
                2 => $"[{EventName(Int(4))}]'s Direction",
                3 => $"[{EventName(Int(4))}]'s ScreenX",
                4 => $"[{EventName(Int(4))}]'s ScreenY",
                5 => $"[{EventName(Int(4))}]'s Terrain Tag",
                _ => "unknown"
            },
            7 => Int(4) switch
            {
                0 => "Map ID",
                1 => "Party length",
                2 => "Money",
                3 => "Steps",
                4 => "Play Time",
                5 => "Timer",
                6 => "Save Count",
                _ => "unknown"
            },
            _ => "unknown"
        };
        if (VariableID1 == VariableID2)
        {
            HeaderLabel.SetText($"Set Variable {GetVariableText(VariableID1)} {Operator} {Value}");
        }
        else
        {
            HeaderLabel.SetText($"Set Variables [{Utilities.Digits(VariableID1, 3)}..{Utilities.Digits(VariableID2, 3)}] {Operator} {Value}");
        }
    }

    private int Int(int Index)
    {
        return (int) (long) Command.Parameters[Index];
    }

    private string EventName(int EventID)
    {
        return EventID == -1 ? "Player" : EventID == 0 ? "Self" : Map.Events[EventID].Name;
    }

    private string GetVariableText(int VariableID)
    {
        return $"[{Utilities.Digits(VariableID, 3)}: {Data.System.Variables[VariableID - 1]}]";
    }

    protected override void Edit(EditEvent Continue)
    {
        //SwitchPicker win = new SwitchPicker((int) (long) Command.Parameters[0]);
        //win.OnClosed += _ =>
        //{
        //    if (!win.Apply)
        //    {
        //        Continue(false);
        //        return;
        //    }
        //    Commands = new List<EventCommand>() { new EventCommand(CommandCode.ControlSwitches, 0, new List<object>() { (long) win.SwitchID }) };
        //    Continue();
        //};
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (e.Handled || this.Indentation == -1)
        {
            CancelDoubleClick();
            return;
        }
        SelectNormally();
    }
}
