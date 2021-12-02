using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class Map
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("Map", RPG.Module);
        }

        public static int TilesetID(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@tileset_id"));
        }
        public static int Width(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@width"));
        }
        public static int Height(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@height"));
        }
        public static bool AutoplayBGM(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@autoplay_bgm") == Ruby.True;
        }
        public static IntPtr BGM(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@bgm");
        }
        public static bool AutoplayBGS(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@autoplay_bgs") == Ruby.True;
        }
        public static IntPtr BGS(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@bgs");
        }
        public static IntPtr EncounterList(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@encounter_list");
        }
        public static int EncounterStep(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@encounter_step"));
        }
        public static IntPtr Data(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@data");
        }
        public static IntPtr Events(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@events");
        }
    }
}
