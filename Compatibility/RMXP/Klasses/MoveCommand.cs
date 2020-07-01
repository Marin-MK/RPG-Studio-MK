using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class MoveCommand : Ruby.Object
        {
            public new static string KlassName = "RPG::MoveCommand";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public MoveCommand(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<MoveCommand>("MoveCommand", null, "RPG");
            }

            public Ruby.Integer Code
            {
                get => GetIVar("@code").Convert<Ruby.Integer>();
            }
            public Ruby.Array Parameters
            {
                get => GetIVar("@parameters").Convert<Ruby.Array>();
            }
        }
    }
}
