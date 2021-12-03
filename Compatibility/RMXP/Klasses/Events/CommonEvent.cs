using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public class CommonEvent
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("CommonEvent", RPG.Module);
        }
    }
}
