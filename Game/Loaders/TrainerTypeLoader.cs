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
    private static void LoadTrainerTypes()
    {
        SafeLoad("trainer_types.dat", File =>
        {
            nint Data = Marshal.Load(File);
            Ruby.Pin(Data);
            nint Keys = Ruby.Hash.Keys(Data);
            Ruby.Pin(Keys);
            int KeyCount = (int) Ruby.Array.Length(Keys);
            for (int i = 0; i < KeyCount; i++)
            {
                nint key = Ruby.Array.Get(Keys, i);
                nint robj = Ruby.Hash.Get(Data, key);
                string ckey = Ruby.Symbol.FromPtr(key);
                TrainerTypes.Add(ckey, new TrainerType(robj));
                SetLoadProgress((float) i / (KeyCount - 1));
            }
            Ruby.Unpin(Keys);
            Ruby.Unpin(Data);
        });
    }

    private static void LoadTrainerTypesFromPBS(string FilePath)
    {
        PBSParser.ParseSectionBasedFile(FilePath, (id, hash) =>
        {
            TrainerTypes.Add(id, new TrainerType(id, hash));
        });
    }

    private static void SaveTrainerTypes()
    {
        SafeSave("trainer_types.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach (TrainerType t in TrainerTypes.Values)
            {
                nint tdata = t.Save();
                Ruby.Hash.Set(Data, Ruby.Symbol.ToPtr(t.ID), tdata);
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
