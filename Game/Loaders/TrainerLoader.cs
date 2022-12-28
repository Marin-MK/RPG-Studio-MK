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
    private static void LoadTrainers()
    {
        SafeLoad("trainers.dat", File =>
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
                Trainers.Add(new Trainer(robj));
                SetLoadProgress((float) i / (KeyCount - 1));
            }
            Ruby.Unpin(Data);
            Ruby.Unpin(Keys);
        });
    }

    private static void LoadTrainersFromPBS(string FilePath)
    {
        FormattedTextParser.ParseSectionBasedFileWithOrder(FilePath, (id, pairs) =>
        {
            Trainers.Add(new Trainer(id, pairs));
        });
    }

    private static void SaveTrainers()
    {
        SafeSave("trainers.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach (Trainer t in Trainers)
            {
                nint tdata = t.Save();
                Ruby.Hash.Set(Data, Ruby.GetIVar(tdata, "@id"), tdata);
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
