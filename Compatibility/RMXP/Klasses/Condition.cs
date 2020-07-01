using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Condition : Ruby.Object
        {
            public new static string KlassName = "RPG::Event::Page::Condition";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public Condition(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Condition>("Condition", null, "RPG::Event::Page");
            }

            public bool Switch1Valid
            {
                get => GetIVar("@switch1_valid") == Ruby.True;
            }
            public bool Switch2Valid
            {
                get => GetIVar("@switch2_valid") == Ruby.True;
            }
            public bool VariableValid
            {
                get => GetIVar("@variable_valid") == Ruby.True;
            }
            public bool SelfSwitchValid
            {
                get => GetIVar("@self_switch_valid") == Ruby.True;
            }
            public Ruby.Integer Switch1ID
            {
                get => GetIVar("@switch1_id").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Switch2ID
            {
                get => GetIVar("@switch2_id").Convert<Ruby.Integer>();
            }
            public Ruby.Integer VariableID
            {
                get => GetIVar("@variable_id").Convert<Ruby.Integer>();
            }
            public Ruby.Integer VariableValue
            {
                get => GetIVar("@variable_value").Convert<Ruby.Integer>();
            }
            public Ruby.String SelfSwitchCh
            {
                get => GetIVar("@self_switch_ch").Convert<Ruby.String>();
            }
        }
    }
}
