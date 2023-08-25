using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class MoveCommand
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("MoveCommand", RPG.Module);
        }

        public static int Code(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@code"));
        }
        public static IntPtr Parameters(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@parameters");
        }
    }
}
