using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Tileset : Ruby.Object
        {
            public new static string KlassName = "RPG::Tileset";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass("Tileset"); }

            public Tileset(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Tileset>("Tileset", null, "RPG");
            }

            public Ruby.Integer ID
            {
                get => GetIVar("@id").Convert<Ruby.Integer>();
            }
            public Ruby.String Name
            {
                get => GetIVar("@name").Convert<Ruby.String>();
            }
            public Ruby.String TilesetName
            {
                get => GetIVar("@tileset_name").Convert<Ruby.String>();
            }
            public Ruby.Array AutotileNames
            {
                get => GetIVar("@autotile_names").Convert<Ruby.Array>();
            }
            public Ruby.String PanoramaName
            {
                get => GetIVar("@panorama_name").Convert<Ruby.String>();
            }
            public Ruby.Integer PanoramaHue
            {
                get => GetIVar("@panorama_hue").Convert<Ruby.Integer>();
            }
            public Ruby.String FogName
            {
                get => GetIVar("@fog_name").Convert<Ruby.String>();
            }
            public Ruby.Integer FogHue
            {
                get => GetIVar("@fog_hue").Convert<Ruby.Integer>();
            }
            public Ruby.Integer FogOpacity
            {
                get => GetIVar("@fog_opacity").Convert<Ruby.Integer>();
            }
            public Ruby.Integer FogBlendType
            {
                get => GetIVar("@fog_blend_type").Convert<Ruby.Integer>();
            }
            public Ruby.Integer FogZoom
            {
                get => GetIVar("@fog_zoom").Convert<Ruby.Integer>();
            }
            public Ruby.Integer FogSX
            {
                get => GetIVar("@fog_sx").Convert<Ruby.Integer>();
            }
            public Ruby.Integer FogSY
            {
                get => GetIVar("@fog_sy").Convert<Ruby.Integer>();
            }
            public Ruby.String BattleBackName
            {
                get => GetIVar("@battleback_name").Convert<Ruby.String>();
            }
            public Table Passages
            {
                get => GetIVar("@passages").Convert<Table>();
            }
            public Table Priorities
            {
                get => GetIVar("@priorities").Convert<Table>();
            }
            public Table TerrainTags
            {
                get => GetIVar("@terrain_tags").Convert<Table>();
            }
        }
    }
}
