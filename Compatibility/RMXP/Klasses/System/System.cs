using System;
using System.Collections.Generic;
using System.Text;
using rubydotnet;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public class System
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("System", RPG.Module);
        }
    }
}
