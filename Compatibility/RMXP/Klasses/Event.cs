using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Event : Ruby.Object
        {
            public new static string KlassName = "RPG::Event";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public Event(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Event>("Event", null, "RPG");
            }

            public Ruby.Integer ID
            {
                get => GetIVar("@id").Convert<Ruby.Integer>();
            }
            public Ruby.String Name
            {
                get => GetIVar("@name").Convert<Ruby.String>();
            }
            public Ruby.Integer X
            {
                get => GetIVar("@x").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Y
            {
                get => GetIVar("@y").Convert<Ruby.Integer>();
            }
            public Ruby.Array Pages
            {
                get => GetIVar("@pages").Convert<Ruby.Array>();
            }
        }
    }
}
