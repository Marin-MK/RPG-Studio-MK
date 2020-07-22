using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static class Tone
        {
            public static IntPtr Class;

            public static void Create()
            {
                Class = Ruby.Class.Define("Tone");
                Ruby.Class.DefineClassMethod(Class, "_load", _load);
                Ruby.Class.DefineMethod(Class, "initialize", initialize);
            }

            public static int Red(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@red"));
            }
            public static int Green(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@green"));
            }
            public static int Blue(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@blue"));
            }
            public static int Grey(IntPtr Self)
            {
                return (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@grey"));
            }

            static IntPtr _load(IntPtr Self, IntPtr Args)
            {
                Ruby.Array.Expect(Args, 1);
                IntPtr unpacked = Ruby.Funcall(Ruby.Array.Get(Args, 0), "unpack", Ruby.String.ToPtr("D*"));
                return Ruby.Funcall(Self, "new", Ruby.Array.Get(unpacked, 0), Ruby.Array.Get(unpacked, 1), Ruby.Array.Get(unpacked, 2), Ruby.Array.Get(unpacked, 3));
            }

            static IntPtr initialize(IntPtr Self, IntPtr Args)
            {
                Ruby.Array.Expect(Args, 0, 1, 2, 3, 4);
                IntPtr Red = Ruby.Integer.ToPtr(0),
                       Green = Red,
                       Blue = Red,
                       Grey = Red;
                long len = Ruby.Array.Length(Args);
                if (len >= 1)
                {
                    Ruby.Array.Expect(Args, 0, "Integer", "Float");
                    Red = Ruby.Array.Get(Args, 0);
                }
                if (len >= 2)
                {
                    Ruby.Array.Expect(Args, 1, "Integer", "Float");
                    Green = Ruby.Array.Get(Args, 1);
                }
                if (len >= 3)
                {
                    Ruby.Array.Expect(Args, 2, "Integer", "Float");
                    Blue = Ruby.Array.Get(Args, 2);
                }
                if (len >= 4)
                {
                    Ruby.Array.Expect(Args, 3, "Integer", "Float");
                    Grey = Ruby.Array.Get(Args, 3);
                }
                Ruby.SetIVar(Self, "@red", Red);
                Ruby.SetIVar(Self, "@green", Green);
                Ruby.SetIVar(Self, "@blue", Blue);
                Ruby.SetIVar(Self, "@grey", Grey);
                return Ruby.Nil;
            }
        }
    }
}
