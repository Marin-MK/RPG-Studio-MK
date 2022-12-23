using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static void LoadTilesets()
    {
        SafeLoad("Tilesets.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            Autotiles.AddRange(new Autotile[Ruby.Array.Length(data) * 7]);
            Tilesets.Add(null);
            for (int i = 0; i < Ruby.Array.Length(data); i++)
            {
                IntPtr tileset = Ruby.Array.Get(data, i);
                if (tileset != Ruby.Nil)
                {
                    Tilesets.Add(new Tileset(tileset));
                }
            }
            Ruby.Unpin(data);
        });
    }

    private static void SaveTilesets()
    {
        SafeSave("Tilesets.rxdata", File =>
        {
            IntPtr tilesets = Ruby.Array.Create();
            Ruby.Pin(tilesets);
            foreach (Tileset tileset in Tilesets)
            {
                if (tileset == null) Ruby.Array.Push(tilesets, Ruby.Nil);
                else
                {
                    IntPtr tilesetdata = tileset.Save();
                    Ruby.Array.Push(tilesets, tilesetdata);
                }
            }
            Ruby.Marshal.Dump(tilesets, File);
            Ruby.Unpin(tilesets);
        });
    }
}