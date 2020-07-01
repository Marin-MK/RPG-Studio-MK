using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Table : Ruby.Object
        {
            public new static string KlassName = "Table";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public Table(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Table>(KlassName);
                c.DefineClassMethod("_load", _load);
                c.DefineMethod("initialize", initialize);
            }

            public Ruby.Integer Size
            {
                get => GetIVar("@size").Convert<Ruby.Integer>();
            }
            public Ruby.Integer XSize
            {
                get => GetIVar("@xsize").Convert<Ruby.Integer>();
            }
            public Ruby.Integer YSize
            {
                get => GetIVar("@ysize").Convert<Ruby.Integer>();
            }
            public Ruby.Integer ZSize
            {
                get => GetIVar("@zsize").Convert<Ruby.Integer>();
            }

            public Ruby.Object this[Ruby.Integer Index]
            {
                get
                {
                    Ruby.Object data = GetIVar("@data");
                    Ruby.Array ary = data.Convert<Ruby.Array>();
                    return ary[Index];
                }
                set => GetIVar("@data").Funcall("[]=", Index, value);
            }

            protected static Ruby.Object initialize(Ruby.Object Self, Ruby.Array Args)
            {
                Args.Expect((1, 4));
                Ruby.Integer XSize;
                Ruby.Integer YSize = 1;
                Ruby.Integer ZSize = 1;
                Args[0].Expect(Ruby.Integer.Class);
                XSize = Args[0].Convert<Ruby.Integer>();
                if (Args.Length >= 2)
                {
                    Args[1].Expect(Ruby.Integer.Class);
                    YSize = Args[1].Convert<Ruby.Integer>();
                }
                if (Args.Length >= 3)
                {
                    Args[2].Expect(Ruby.Integer.Class);
                    ZSize = Args[2].Convert<Ruby.Integer>();
                }
                Self.SetIVar("@xsize", XSize);
                Self.SetIVar("@ysize", YSize);
                Self.SetIVar("@zsize", ZSize);
                Self.SetIVar("@size", XSize * YSize * ZSize);
                Self.SetIVar("@data", new Ruby.Array());
                return Ruby.Nil;
            }

            protected static Ruby.Object _load(Ruby.Object Self, Ruby.Array Args)
            {
                Args.Expect(1);
                Ruby.Array unpack = Args[0].Convert<Ruby.String>().Unpack("LLLLLS*");
                Ruby.Object obj = Self.Funcall("new", unpack[1], unpack[2], unpack[3]);
                obj.SetIVar("@data", unpack[(5, unpack.Length)]);
                return obj;
            }
        }
    }
}
