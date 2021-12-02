using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class Event
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("Event", RPG.Module);
        }

        public static int ID(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@id"));
        }
        public static string Name(IntPtr Self)
        {
            return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@name"));
        }
        public static int X(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@x"));
        }
        public static int Y(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@y"));
        }
        public static IntPtr Pages(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@pages");
        }
    }
}
