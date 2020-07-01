using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Tone : Ruby.Object
        {
            public new static string KlassName = "Tone";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public Tone(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Tone>(KlassName);
                c.DefineClassMethod("_load", _load);
                c.DefineMethod("initialize", initialize);
            }

            public Ruby.Integer Red
            {
                get => GetIVar("@red").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Green
            {
                get => GetIVar("@green").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Blue
            {
                get => GetIVar("@blue").Convert<Ruby.Integer>();
            }
            public Ruby.Integer Grey
            {
                get => GetIVar("@grey").Convert<Ruby.Integer>();
            }

            protected static Ruby.Object _load(Ruby.Object Self, Ruby.Array Args)
            {
                Args.Expect(1);
                Ruby.Array unpack = Args[0].Convert<Ruby.String>().Unpack("D*");
                return Self.Funcall("new", unpack[0], unpack[1], unpack[2], unpack[3]);
            }

            protected static Ruby.Object initialize(Ruby.Object Self, Ruby.Array Args)
            {
                Args.Expect((0, 5));
                Ruby.Object Red = (Ruby.Integer) 0;
                Ruby.Object Green = (Ruby.Integer) 0;
                Ruby.Object Blue = (Ruby.Integer) 0;
                Ruby.Object Grey = (Ruby.Integer) 0;
                if (Args.Length >= 1)
                {
                    Args[0].Expect(Ruby.Integer.Class, Ruby.Float.Class);
                    Red = Args[0];
                }
                if (Args.Length >= 2)
                {
                    Args[1].Expect(Ruby.Integer.Class, Ruby.Float.Class);
                    Green = Args[1];
                }
                if (Args.Length >= 3)
                {
                    Args[2].Expect(Ruby.Integer.Class, Ruby.Float.Class);
                    Blue = Args[2];
                }
                if (Args.Length >= 4)
                {
                    Args[3].Expect(Ruby.Integer.Class, Ruby.Float.Class);
                    Grey = Args[3];
                }
                Self.SetIVar("@red", Red);
                Self.SetIVar("@green", Green);
                Self.SetIVar("@blue", Blue);
                Self.SetIVar("@grey", Grey);
                return Ruby.Nil;
            }
        }
    }
}
