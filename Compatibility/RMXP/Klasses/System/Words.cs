using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public class Words
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("Words", System.Class);
        }
    }
}
