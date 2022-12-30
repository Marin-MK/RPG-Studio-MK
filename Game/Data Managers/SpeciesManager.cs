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

public class SpeciesManager : BaseDataManager
{
    public SpeciesManager(bool FromPBS = false)
        : base("Species", "species.dat", "pokemon.txt", "species", FromPBS) { }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsHash((key, value) =>
        {
            string id = Ruby.Symbol.FromPtr(key);
            Data.Species.Add(id, new Species(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Data.Species.Add(id, new Species(id, hash));
        }, Data.SetLoadProgress);
        Game.Species.PrevolutionsToRegister.ForEach(p =>
        {
            p.Item2.Species.Species.Prevolutions.Add(new Evolution((SpeciesResolver) p.Item1, p.Item2.Type, p.Item2.Parameters, true));
        });
        Game.Species.PrevolutionsToRegister.Clear();
    }

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsHash(Data.Species.Values, s => Ruby.Symbol.ToPtr(s.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Data.Species.Clear();
    }
}
