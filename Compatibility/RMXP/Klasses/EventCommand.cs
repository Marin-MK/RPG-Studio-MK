using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class EventCommand : Ruby.Object
        {
            public new static string KlassName = "RPG::EventCommand";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public EventCommand(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<EventCommand>("EventCommand", null, "RPG");
            }

            public Ruby.Integer Code
            {
                get => GetIVar("@code").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Indent
            {
                get => GetIVar("@indent").Convert<Ruby.Integer>();
            }
            public Ruby.Array Parameters
            {
                get => GetIVar("@parameters").Convert<Ruby.Array>();
            }
        }
    }
}
