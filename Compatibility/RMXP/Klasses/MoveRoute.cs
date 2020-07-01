using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class MoveRoute : Ruby.Object
        {
            public new static string KlassName = "RPG::MoveRoute";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public MoveRoute(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<MoveRoute>("MoveRoute", null, "RPG");
            }

            public bool Repeat
            {
                get => GetIVar("@repeat") == Ruby.True;
            }
            public bool Skippable
            {
                get => GetIVar("@skippable") == Ruby.True;
            }
            public Ruby.Array List
            {
                get => GetIVar("@list").Convert<Ruby.Array>();
            }
        }
    }
}
