using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class AudioFile : Ruby.Object
        {
            public new static string KlassName = "RPG::AudioFile";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public AudioFile(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<AudioFile>("AudioFile", null, "RPG");
            }

            public Ruby.String Name
            {
                get => GetIVar("@name").Convert<Ruby.String>();
            }
            public Ruby.Integer Volume
            {
                get => GetIVar("@volume").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Pitch
            {
                get => GetIVar("@pitch").Convert<Ruby.Integer>();
            }
        }
    }
}
