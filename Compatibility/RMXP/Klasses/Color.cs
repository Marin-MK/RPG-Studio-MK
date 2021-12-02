using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static class Color
        {
            public static IntPtr Class;

            public static void Create()
            {
                Class = Ruby.Class.Define("Color");
                Ruby.Class.DefineClassMethod(Class, "_load", _load);
                Ruby.Class.DefineMethod(Class, "_dump", _dump);
                Ruby.Class.DefineMethod(Class, "initialize", initialize);
            }

            public static int Red(IntPtr Self)
            {
                return (int) Ruby.Float.FromPtr(Ruby.GetIVar(Self, "@red"));
            }
            public static int Green(IntPtr Self)
            {
                return (int) Ruby.Float.FromPtr(Ruby.GetIVar(Self, "@green"));
            }
            public static int Blue(IntPtr Self)
            {
                return (int) Ruby.Float.FromPtr(Ruby.GetIVar(Self, "@blue"));
            }
            public static int Alpha(IntPtr Self)
            {
                return (int) Ruby.Float.FromPtr(Ruby.GetIVar(Self, "@alpha"));
            }

            static IntPtr _load(IntPtr Self, IntPtr Args)
            {
                Ruby.Array.Expect(Args, 1);
                IntPtr unpacked = Ruby.Funcall(Ruby.Array.Get(Args, 0), "unpack", Ruby.String.ToPtr("E4"));
                return Ruby.Funcall(Self, "new", Ruby.Array.Get(unpacked, 0), Ruby.Array.Get(unpacked, 1), Ruby.Array.Get(unpacked, 2), Ruby.Array.Get(unpacked, 3));
            }

            static IntPtr _dump(IntPtr Self, IntPtr Args)
            {
                IntPtr array = Ruby.Array.Create();
                Ruby.Pin(array);
                Ruby.Array.Set(array, 0, Ruby.GetIVar(Self, "@red"));
                Ruby.Array.Set(array, 1, Ruby.GetIVar(Self, "@green"));
                Ruby.Array.Set(array, 2, Ruby.GetIVar(Self, "@blue"));
                Ruby.Array.Set(array, 3, Ruby.GetIVar(Self, "@alpha"));
                Ruby.Unpin(array);
                return Ruby.Funcall(array, "pack", Ruby.String.ToPtr("E4"));
            }

            static IntPtr initialize(IntPtr Self, IntPtr Args)
            {
                Ruby.Array.Expect(Args, 0, 1, 2, 3, 4);
                IntPtr Red = Ruby.Float.ToPtr(0),
                       Green = Red,
                       Blue = Red,
                       Alpha = Ruby.Float.ToPtr(255);
                long len = Ruby.Array.Length(Args);
                if (len >= 1)
                {
                    Ruby.Array.Expect(Args, 0, "Integer", "Float");
                    if (Ruby.Array.Is(Args, 0, "Integer")) Red = Ruby.Float.ToPtr(Ruby.Integer.FromPtr(Ruby.Array.Get(Args, 0)));
                    else Red = Ruby.Array.Get(Args, 0);
                }
                if (len >= 2)
                {
                    Ruby.Array.Expect(Args, 1, "Integer", "Float");
                    if (Ruby.Array.Is(Args, 1, "Integer")) Green = Ruby.Float.ToPtr(Ruby.Integer.FromPtr(Ruby.Array.Get(Args, 1)));
                    else Green = Ruby.Array.Get(Args, 1);
                }
                if (len >= 3)
                {
                    Ruby.Array.Expect(Args, 2, "Integer", "Float");
                    if (Ruby.Array.Is(Args, 2, "Integer")) Blue = Ruby.Float.ToPtr(Ruby.Integer.FromPtr(Ruby.Array.Get(Args, 2)));
                    else Blue = Ruby.Array.Get(Args, 2);
                }
                if (len >= 4)
                {
                    Ruby.Array.Expect(Args, 3, "Integer", "Float");
                    if (Ruby.Array.Is(Args, 3, "Integer")) Alpha = Ruby.Float.ToPtr(Ruby.Integer.FromPtr(Ruby.Array.Get(Args, 3)));
                    else Alpha = Ruby.Array.Get(Args, 3);
                }
                Ruby.SetIVar(Self, "@red", Red);
                Ruby.SetIVar(Self, "@green", Green);
                Ruby.SetIVar(Self, "@blue", Blue);
                Ruby.SetIVar(Self, "@alpha", Alpha);
                return Ruby.Nil;
            }
        }
    }
}
