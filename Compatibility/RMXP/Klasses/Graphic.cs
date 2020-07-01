using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Graphic : Ruby.Object
        {
            public new static string KlassName = "RPG::Event::Page::Graphic";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public Graphic(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Graphic>("Graphic", null, "RPG::Event::Page");
            }

            public Ruby.Integer TileID
            {
                get => GetIVar("@tile_id").Convert<Ruby.Integer>();
            }
            public Ruby.String CharacterName
            {
                get => GetIVar("@character_name").Convert<Ruby.String>();
            }
            public Ruby.Integer CharacterHue
            {
                get => GetIVar("@character_hue").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Direction
            {
                get => GetIVar("@direction").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Pattern
            {
                get => GetIVar("@pattern").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Opacity
            {
                get => GetIVar("@opacity").Convert<Ruby.Integer>();
            }
            public Ruby.Integer BlendType
            {
                get => GetIVar("@blend_type").Convert<Ruby.Integer>();
            }
        }
    }
}
