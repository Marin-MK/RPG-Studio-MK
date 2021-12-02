using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class Graphic
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("Graphic", Page.Class);
        }

        public static int TileID(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@tile_id"));
        }
        public static string CharacterName(IntPtr Self)
        {
            return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@character_name"));
        }
        public static int CharacterHue(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@character_hue"));
        }
        public static int Direction(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@direction"));
        }
        public static int Pattern(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@pattern"));
        }
        public static int Opacity(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@opacity"));
        }
        public static int BlendType(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@blend_type"));
        }
    }
}
