﻿using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public class TestBattler
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("TestBattler", System.Class);
        }
    }
}
