using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class RPG
    {
        public static IntPtr Module;

        public static void Create()
        {
            Module = Ruby.Module.Define("RPG");
        }
    }
}
