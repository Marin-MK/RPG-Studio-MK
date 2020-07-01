using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class RPG : Ruby.Object
        {
            public new static string KlassName = "RPG";
            public static Ruby.Module Module { get => (Ruby.Module) GetKlass(KlassName); }

            public RPG(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Module m = Ruby.Module.DefineModule<RPG>(KlassName);
            }
        }
    }
}
