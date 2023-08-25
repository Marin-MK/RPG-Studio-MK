using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class Condition
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("Condition", Page.Class);
        }

        public static bool Switch1Valid(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@switch1_valid") == Ruby.True;
        }
        public static bool Switch2Valid(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@switch2_valid") == Ruby.True;
        }
        public static bool VariableValid(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@variable_valid") == Ruby.True;
        }
        public static bool SelfSwitchValid(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@self_switch_valid") == Ruby.True;
        }
        public static int Switch1ID(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@switch1_id"));
        }
        public static int Switch2ID(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@switch2_id"));
        }
        public static int VariableID(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@variable_id"));
        }
        public static int VariableValue(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@variable_value"));
        }
        public static string SelfSwitchCh(IntPtr Self)
        {
            return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@self_switch_ch"));
        }
    }
}
