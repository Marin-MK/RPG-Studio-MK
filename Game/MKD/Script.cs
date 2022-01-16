using System;

namespace RPGStudioMK.Game;

public class Script
{
    public long MagicNumber;
    public string Name;
    public string Content;

    public Script()
    {
        this.MagicNumber = Utilities.Random(0, 100_000_000);
    }

    public Script(IntPtr data)
    {
        this.MagicNumber = (long)Ruby.Integer.FromPtr(Ruby.Array.Get(data, 0));
        this.Name = Ruby.String.FromPtr(Ruby.Array.Get(data, 1));
        IntPtr raw = Ruby.Array.Get(data, 2);
        IntPtr conv = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Zlib"), "inflate", raw);
        this.Content = Ruby.String.FromPtr(conv);
    }

    public IntPtr Save()
    {
        IntPtr s = Ruby.Array.Create();
        Ruby.Pin(s);
        Ruby.Array.Set(s, 0, Ruby.Integer.ToPtr(this.MagicNumber));
        Ruby.Array.Set(s, 1, Ruby.String.ToPtr(this.Name));
        IntPtr conv = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Zlib"), "deflate", Ruby.String.ToPtr(this.Content));
        Ruby.Array.Set(s, 2, conv);
        Ruby.Unpin(s);
        return s;
    }
}
