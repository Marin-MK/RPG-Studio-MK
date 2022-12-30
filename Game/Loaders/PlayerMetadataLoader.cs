using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static rubydotnet.Ruby;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static void LoadPlayerMetadata()
    {
        SafeLoad("player_metadata.dat", File =>
        {
            nint Data = Marshal.Load(File);
            Ruby.Pin(Data);
            nint Keys = Ruby.Hash.Keys(Data);
            Ruby.Pin(Keys);
            nint KeyCount = (int) Ruby.Array.Length(Keys);
            for (int i = 0; i < KeyCount; i++)
            {
                nint key = Ruby.Array.Get(Keys, i);
                nint robj = Ruby.Hash.Get(Data, key);
                int ckey = (int) Ruby.Integer.FromPtr(key);
                PlayerMetadata.Add(ckey, new PlayerMetadata(robj));
                SetLoadProgress((float) i / (KeyCount - 1));
            }
            Ruby.Unpin(Keys);
            Ruby.Unpin(Data);
        });
    }
    
    // The PBS parser for this data file is included in the global metadata PBS parser,
    // as that PBS file contains both the global metadata and the player metadata.

    private static void SavePlayerMetadata()
    {
        SafeSave("player_metadata.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach (PlayerMetadata p in PlayerMetadata.Values)
            {
                nint pdata = p.Save();
                Ruby.Hash.Set(Data, Ruby.Integer.ToPtr(p.ID), pdata);
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
