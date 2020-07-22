using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static class EventCommand
        {
            public static IntPtr Class;

            public static void Create()
            {
                Class = Ruby.Class.Define("EventCommand", RPG.Module);
            }

            public static int Code(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@code"));
            }
            public static int Indent(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@indent"));
            }
            public static IntPtr Parameters(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@parameters");
            }
        }
    }
}
