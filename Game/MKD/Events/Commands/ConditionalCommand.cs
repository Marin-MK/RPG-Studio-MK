using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game.EventCommands;

public class ConditionalCommand : BaseCommand
{
    public ConditionType ConditionType;

    public ConditionalCommand(EventCommand Command) : base(Command)
    {
        this.ConditionType = (ConditionType) Int(0);
    }

    public override string GetText(Map m, Event ev)
    {
        this.m = m;
        this.ev = ev;
        return ConditionType switch
        {
            ConditionType.Switch => $"{Switch(Int(1))} is {OnOff(Int(2))}",
            ConditionType.Variable => $"{Variable(Int(1))} {Operator(Int(4))} {(Int(2) == 0 ? Int(3) : Variable(Int(3)))}",
            ConditionType.SelfSwitch => $"Self Switch {String(1)} is {OnOff(Int(2))}",
            ConditionType.Timer => $"Timer {Int(1) / 60}min {Int(1) % 60}sec or {(Int(2) == 0 ? "more" : "less")}",
            ConditionType.Actor => $"Actor condition",
            ConditionType.Enemy => $"Enemy condition",
            ConditionType.Character => $"{Event(Int(1))} is facing {Dir(Int(2))}",
            ConditionType.Gold => $"Money {(Int(2) == 0 ? ">=" : "<=")} {Int(1)}",
            ConditionType.Item => $"Item condition",
            ConditionType.Weapon => $"Weapon condition",
            ConditionType.Armor => $"Armor condition",
            ConditionType.Button => $"The {Button(Int(1))} button is held down",
            ConditionType.Script => String(1),
            _ => "Unknown condition"
        };
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
