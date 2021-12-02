using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class MapInfo
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("MapInfo", RPG.Module);
        }

        public static string Name(IntPtr Self)
        {
            return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@name"));
        }
        public static int ParentID(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@parent_id"));
        }
        public static int Order(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@order"));
        }
        public static bool Expanded(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@expanded") == Ruby.True;
        }
        public static int ScrollX(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@scroll_x"));
        }
        public static int ScrollY(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@scroll_y"));
        }
    }
}
