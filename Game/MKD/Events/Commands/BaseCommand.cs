using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game.EventCommands;

public class BaseCommand
{
    public EventCommand Command;

    public Map m;
    public Event ev;

    public BaseCommand(EventCommand Command)
    {
        this.Command = Command;
    }

    public int Int(int ParameterIndex)
    {
        return (int) (long) Command.Parameters[ParameterIndex];
    }

    public string String(int ParameterIndex)
    {
        return (string) Command.Parameters[ParameterIndex];
    }

    public string Switch(int SwitchID)
    {
        return $"Switch [{Utilities.Digits(SwitchID, 3)}: {Data.System.Switches[SwitchID - 1]}]";
    }

    public string Variable(int VariableID)
    {
        return $"Variable [{Utilities.Digits(VariableID, 3)}: {Data.System.Variables[VariableID - 1]}]";
    }

    public char SelfSwitch(int SelfSwitchID)
    {
        return (char) ('A' + SelfSwitchID);
    }

    public string Event(int EventID)
    {
        if (m.Events.ContainsKey(EventID)) return $"[{m.Events[EventID].Name}]";
        else return $"[{Utilities.Digits(EventID, 3)}]";
    }

    public string Dir(int Direction, bool Capitalize = false)
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

    public string Button(int Button)
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

    public string OnOff(int State)
    {
        return State == 0 ? "ON" : "OFF";
    }

    public string Operator(int ID)
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

    public virtual string GetText(Map m, Event ev)
    {
        return Command.Code.ToString();
    }
}
