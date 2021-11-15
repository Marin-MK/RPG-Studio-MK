using System;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class EventCondition
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

        public EventCondition(IntPtr data)
        {
            this.Switch1Valid = Ruby.GetIVar(data, "@switch1_valid") == Ruby.True;
            this.Switch1Valid = Ruby.GetIVar(data, "@switch2_valid") == Ruby.True;
            this.SelfSwitchValid = Ruby.GetIVar(data, "@self_switch_valid") == Ruby.True;
            this.VariableValid = Ruby.GetIVar(data, "@variable_valid") == Ruby.True;
            this.SelfSwitchChar = Ruby.String.FromPtr(Ruby.GetIVar(data, "@self_switch_ch"))[0];
            this.Switch1ID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@switch1_id"));
            this.Switch2ID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@switch2_id"));
            this.VariableValue = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@variable_value"));
            this.VariableID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@variable_id"));
        }
    }
}
