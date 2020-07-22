using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static class AudioFile
        {
            public static IntPtr Class;

            public static void Create()
            {
                Class = Ruby.Class.Define("AudioFile", RPG.Module);
            }

            public static string Name(IntPtr Self)
            {
                return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@name"));
            }
            public static int Volume(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@volume"));
            }
            public static int Pitch(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@pitch"));
            }
        }
    }
}
