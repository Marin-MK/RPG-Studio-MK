using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class TilesetManager : BaseDataManager
{
    public TilesetManager()
        : base(null, "Tilesets.rxdata", null, "tilesets", false) { }

    protected override void LoadData()
    {
        base.LoadData();
        SafeLoad(Filename, File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            Data.Autotiles.AddRange(new Autotile[Ruby.Array.Length(data) * 7]);
            Data.Tilesets.Add(null);
            for (int i = 0; i < Ruby.Array.Length(data); i++)
            {
                IntPtr tileset = Ruby.Array.Get(data, i);
                if (tileset != Ruby.Nil)
                {
                    Data.Tilesets.Add(new Tileset(tileset));
                }
                if (Data.StopLoading) break;
            }
            Ruby.Unpin(data);
        });
    }

    protected override void SaveData()
    {
        base.SaveData();
        SafeSave(Filename, File =>
        {
            IntPtr tilesets = Ruby.Array.Create();
            Ruby.Pin(tilesets);
            foreach (Tileset tileset in Data.Tilesets)
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

    public override void Clear()
    {
        base.Clear();
        Data.Tilesets.Clear();
    }
}