using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static class MoveRoute
        {
            public static IntPtr Class;

            public static void Create()
            {
                Class = Ruby.Class.Define("MoveRoute", RPG.Module);
            }

            public static bool Repeat(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@repeat") == Ruby.True;
            }
            public static bool Skippable(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@skippable") == Ruby.True;
            }
            public static IntPtr List(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@list");
            }
        }
    }
}
