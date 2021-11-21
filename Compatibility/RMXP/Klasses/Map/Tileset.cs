using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static class Tileset
        {
            public static IntPtr Class;

            public static void Create()
            {
                Class = Ruby.Class.Define("Tileset", RPG.Module);
            }

            public static int ID(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@id"));
            }
            public static string Name(IntPtr Self)
            {
                return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@name"));
            }
            public static string TilesetName(IntPtr Self)
            {
                return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@tileset_name"));
            }
            public static IntPtr AutotileNames(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@autotile_names");
            }
            public static string PanoramaName(IntPtr Self)
            {
                return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@panorama_name"));
            }
            public static int PanoramaHue(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@panorama_hue"));
            }
            public static string FogName(IntPtr Self)
            {
                return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@fog_name"));
            }
            public static int FogHue(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@fog_hue"));
            }
            public static int FogOpacity(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@fog_opacity"));
            }
            public static int FogBlendType(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@fog_blend_type"));
            }
            public static int FogZoom(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@fog_zoom"));
            }
            public static int FogSX(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@fog_sx"));
            }
            public static int FogSY(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@fog_sy"));
            }
            public static string BattleBackName(IntPtr Self)
            {
                return Ruby.String.FromPtr(Ruby.GetIVar(Self, "@battleback_name"));
            }
            public static IntPtr Passages(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@passages");
            }
            public static IntPtr Priorities(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@priorities");
            }
            public static IntPtr TerrainTags(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@terrain_tags");
            }
        }
    }
}
