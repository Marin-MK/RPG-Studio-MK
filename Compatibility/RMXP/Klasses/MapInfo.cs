using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class MapInfo : Ruby.Object
        {
            public new static string KlassName = "RPG::MapInfo";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public MapInfo(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<MapInfo>("MapInfo", null, "RPG");
            }

            public Ruby.String Name
            {
                get => GetIVar("@name").Convert<Ruby.String>();
            }
            public Ruby.Integer ParentID
            {
                get => GetIVar("@parent_id").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Order
            {
                get => GetIVar("@order").Convert<Ruby.Integer>();
            }
            public bool Expanded
            {
                get => GetIVar("@expanded") == Ruby.True;
            }
            public Ruby.Integer ScrollX
            {
                get => GetIVar("@scroll_x").Convert<Ruby.Integer>();
            }
            public Ruby.Integer ScrollY
            {
                get => GetIVar("@scroll_y").Convert<Ruby.Integer>();
            }
        }
    }
}
