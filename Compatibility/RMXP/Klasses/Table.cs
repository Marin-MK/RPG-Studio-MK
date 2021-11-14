using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static class Table
        {
            public static IntPtr Class;

            public static void Create()
            {
                Class = Ruby.Class.Define("Table");
                Ruby.Class.DefineClassMethod(Class, "_load", _load);
                Ruby.Class.DefineMethod(Class, "_dump", _dump);
                Ruby.Class.DefineMethod(Class, "initialize", initialize);
            }

            public static int Size(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.Array.Get(Ruby.GetIVar(Self, "@data"), 4));
            }
            public static int XSize(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.Array.Get(Ruby.GetIVar(Self, "@data"), 1));
            }
            public static int YSize(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.Array.Get(Ruby.GetIVar(Self, "@data"), 2));
            }
            public static int ZSize(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.Array.Get(Ruby.GetIVar(Self, "@data"), 3));
            }
            public static IntPtr Data(IntPtr Self)
            {
                return Ruby.GetIVar(Self, "@data");
            }

            public static IntPtr Get(IntPtr Self, int Index)
            {
                return Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "[]", Ruby.Integer.ToPtr(Index + 5));
            }

            public static IntPtr Set(IntPtr Self, int Index, IntPtr Value)
            {
                return Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "[]=", Ruby.Integer.ToPtr(Index + 5), Value);
            }

            static IntPtr initialize(IntPtr Self, IntPtr Args)
            {
                Ruby.Array.Expect(Args, 1, 2, 3);
                Ruby.Array.Expect(Args, 0, "Integer");
                IntPtr XSize = Ruby.Array.Get(Args, 0);
                IntPtr YSize = Ruby.Integer.ToPtr(1);
                IntPtr ZSize = Ruby.Integer.ToPtr(1);
                long len = Ruby.Array.Length(Args);
                if (len >= 2)
                {
                    Ruby.Array.Expect(Args, 1, "Integer");
                    YSize = Ruby.Array.Get(Args, 1);
                }
                if (len >= 3)
                {
                    Ruby.Array.Expect(Args, 2, "Integer");
                    ZSize = Ruby.Array.Get(Args, 2);
                }
                int size = (int) (Ruby.Integer.FromPtr(XSize) * Ruby.Integer.FromPtr(YSize) * Ruby.Integer.FromPtr(ZSize));
                Ruby.SetIVar(Self, "@data", Ruby.Array.Create(size + 5, Ruby.Integer.ToPtr(0)));
                Ruby.SetIVar(Self, "@xsize", XSize);
                Ruby.SetIVar(Self, "@ysize", YSize);
                Ruby.SetIVar(Self, "@zsize", ZSize);
                Ruby.SetIVar(Self, "@size", ZSize);
                Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "[]=", Ruby.Integer.ToPtr(0), ZSize);
                Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "[]=", Ruby.Integer.ToPtr(1), XSize);
                Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "[]=", Ruby.Integer.ToPtr(2), YSize);
                Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "[]=", Ruby.Integer.ToPtr(3), ZSize);
                Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "[]=", Ruby.Integer.ToPtr(4), Ruby.Integer.ToPtr(size));
                return Ruby.Nil;
            }

            static IntPtr _load(IntPtr Self, IntPtr Args)
            {
                Ruby.Array.Expect(Args, 1);
                IntPtr unpacked = Ruby.Funcall(Ruby.Array.Get(Args, 0), "unpack", Ruby.String.ToPtr("LLLLLS*"));
                IntPtr obj = Ruby.Funcall(Self, "new", Ruby.Array.Get(unpacked, 1), Ruby.Array.Get(unpacked, 2), Ruby.Array.Get(unpacked, 3));
                Ruby.SetIVar(obj, "@data", unpacked);
                return obj;
            }

            static IntPtr _dump(IntPtr Self, IntPtr Args)
            {
                return Ruby.Funcall(Ruby.GetIVar(Self, "@data"), "pack", Ruby.String.ToPtr("LLLLLS*"));
            }
        }
    }
}
