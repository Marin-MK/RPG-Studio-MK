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
                SetLoadProgress((float) i / (length - 1));
            }
            Ruby.Unpin(KeyArray);
            Ruby.Unpin(data);
        });
    }

    private static void LoadSpeciesFromPBS(string FilePath)
    {
        PBSParser.ParseSectionBasedFile(ProjectPath + "/PBS/pokemon.txt", (id, hash) =>
        {
            Species.Add(id, new Species(id, hash));
        });
        Game.Species.PrevolutionsToRegister.ForEach(p =>
        {
            p.Item2.Species.Species.Prevolutions.Add(new Evolution((SpeciesResolver) p.Item1, p.Item2.Type, p.Item2.Parameters, true));
        });
        Game.Species.PrevolutionsToRegister.Clear();
    }

    private static void SaveSpecies()
    {
        if (IsPrimaryVersion(EssentialsVersion.v20)) SaveSpeciesV20();
        else throw new Exception($"No procedure found to save species data for games using version {EssentialsVersion}.");
    }

    private static void SaveSpeciesV20()
    {
        SafeSave("species.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach (Species s in Species.Values)
            {
                nint sdata = s.Save();
                Ruby.Hash.Set(Data, Ruby.Symbol.ToPtr(s.ID), sdata);
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
