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
    private static void LoadAbilities()
    {
        SafeLoad("abilities.dat", File =>
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
                Abilities.Add(ckey, new Ability(robj));
                SetLoadProgress((float) i / (KeyCount - 1));
            }
            Ruby.Unpin(Keys);
            Ruby.Unpin(Data);
        });
    }

    private static void LoadAbilitiesFromPBS(string FilePath)
    {
        FormattedTextParser.ParseSectionBasedFile(FilePath, (id, hash) =>
        {
            Abilities.Add(id, new Ability(id, hash));
        });
    }

    private static void SaveAbilities()
    {
        SafeSave("abilities.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach (Ability a in Abilities.Values)
            {
                nint adata = a.Save();
                Ruby.Hash.Set(Data, Ruby.Symbol.ToPtr(a.ID), adata);
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
