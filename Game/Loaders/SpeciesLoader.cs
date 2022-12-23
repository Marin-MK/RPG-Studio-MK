using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static void LoadSpecies()
    {
        if (IsPrimaryVersion(EssentialsVersion.v20)) LoadSpeciesV20();
        else throw new Exception($"No procedure found to load species data for games using version {EssentialsVersion}.");
    }

    private static void LoadSpeciesV20()
    {
        SafeLoad("species.dat", File =>
        {
            nint data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            nint KeyArray = Ruby.Hash.Keys(data);
            Ruby.Pin(KeyArray);
            long length = Ruby.Array.Length(KeyArray);
            for (int i = 0; i < length; i++)
            {
                nint keyptr = Ruby.Array.Get(KeyArray, i);
                string intname = Ruby.Symbol.FromPtr(keyptr);
                nint valueptr = Ruby.Hash.Get(data, keyptr);
                Species obj = new Species(valueptr);
                Species.Add(intname, obj);
                SetLoadProgress((float) i / length);
            }
            Ruby.Unpin(KeyArray);
            Ruby.Unpin(data);
        });
    }
}
