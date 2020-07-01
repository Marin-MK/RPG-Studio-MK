using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Map : Ruby.Object
        {
            public new static string KlassName = "RPG::Map";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public Map(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Map>("Map", null, "RPG");
            }

            public Ruby.Integer TilesetID
            {
                get => GetIVar("@tileset_id").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Width
            {
                get => GetIVar("@width").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Height
            {
                get => GetIVar("@height").Convert<Ruby.Integer>();
            }
            public bool AutoplayBGM
            {
                get => GetIVar("@autoplay_bgm") == Ruby.True;
            }
            public AudioFile BGM
            {
                get => GetIVar("@bgm").Convert<AudioFile>();
            }
            public bool AutoplayBGS
            {
                get => GetIVar("@autoplay_bgs") == Ruby.True;
            }
            public AudioFile BGS
            {
                get => GetIVar("@bgs").Convert<AudioFile>();
            }
            public Ruby.Array EncounterList
            {
                get => GetIVar("@encounter_list").Convert<Ruby.Array>();
            }
            public Ruby.Integer EncounterStep
            {
                get => GetIVar("@encounter_step").Convert<Ruby.Integer>();
            }
            public Table Data
            {
                get => GetIVar("@data").Convert<Table>();
            }
            public Ruby.Hash Events
            {
                get => GetIVar("@events").Convert<Ruby.Hash>();
            }
        }
    }
}
