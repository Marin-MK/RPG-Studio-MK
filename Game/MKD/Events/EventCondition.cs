using System;

namespace RPGStudioMK.Game;

public class EventCondition : ICloneable
{
    public bool Switch1Valid;
    public bool Switch2Valid;
    public bool SelfSwitchValid;
    public bool VariableValid;

    public char SelfSwitchChar;

    public int Switch1ID;
    public int Switch2ID;
    public int VariableValue;
    public int VariableID;

    public EventCondition()
    {
        this.Switch1Valid = false;
        this.Switch1ID = 1;
        this.Switch2Valid = false;
        this.Switch2ID = 1;
        this.VariableValid = false;
        this.VariableID = 1;
        this.VariableValue = 0;
        this.SelfSwitchValid = false;
        this.SelfSwitchChar = 'A';
    }

    public EventCondition(IntPtr data)
    {
        this.Switch1Valid = Ruby.GetIVar(data, "@switch1_valid") == Ruby.True;
        this.Switch2Valid = Ruby.GetIVar(data, "@switch2_valid") == Ruby.True;
        this.SelfSwitchValid = Ruby.GetIVar(data, "@self_switch_valid") == Ruby.True;
        this.VariableValid = Ruby.GetIVar(data, "@variable_valid") == Ruby.True;
        this.SelfSwitchChar = Ruby.String.FromPtr(Ruby.GetIVar(data, "@self_switch_ch"))[0];
        this.Switch1ID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@switch1_id"));
        this.Switch2ID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@switch2_id"));
        this.VariableValue = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@variable_value"));
        this.VariableID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@variable_id"));
    }

    public IntPtr Save()
    {
        IntPtr condition = Ruby.Funcall(Compatibility.RMXP.Condition.Class, "new");
        Ruby.Pin(condition);
        Ruby.SetIVar(condition, "@switch1_valid", this.Switch1Valid ? Ruby.True : Ruby.False);
        Ruby.SetIVar(condition, "@switch2_valid", this.Switch2Valid ? Ruby.True : Ruby.False);
        Ruby.SetIVar(condition, "@self_switch_valid", this.SelfSwitchValid ? Ruby.True : Ruby.False);
        Ruby.SetIVar(condition, "@variable_valid", this.VariableValid ? Ruby.True : Ruby.False);
        Ruby.SetIVar(condition, "@self_switch_ch", Ruby.String.ToPtr(this.SelfSwitchChar.ToString()));
        Ruby.SetIVar(condition, "@switch1_id", Ruby.Integer.ToPtr(this.Switch1ID));
        Ruby.SetIVar(condition, "@switch2_id", Ruby.Integer.ToPtr(this.Switch2ID));
        Ruby.SetIVar(condition, "@variable_value", Ruby.Integer.ToPtr(this.VariableValue));
        Ruby.SetIVar(condition, "@variable_id", Ruby.Integer.ToPtr(this.VariableID));
        Ruby.Unpin(condition);
        return condition;
    }

    public object Clone()
    {
        EventCondition c = new EventCondition();
        c.Switch1Valid = this.Switch1Valid;
        c.Switch2Valid = this.Switch2Valid;
        c.SelfSwitchValid = this.SelfSwitchValid;
        c.VariableValid = this.VariableValid;
        c.SelfSwitchChar = this.SelfSwitchChar;
        c.Switch1ID = this.Switch1ID;
        c.Switch2ID = this.Switch2ID;
        c.VariableValue = this.VariableValue;
        c.VariableID = this.VariableID;
        return c;
    }
}
