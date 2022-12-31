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
        Data.SetLoadText("Loading forms...");
        FormattedTextParser.ParseSectionBasedFile(Data.ProjectPath + "/PBS/pokemon_forms.txt", (id, hash) =>
        {
            string[] _id = id.Split(',').Select(x => x.Trim()).ToArray();
            SpeciesResolver BaseSpecies = (SpeciesResolver) _id[0];
            int form = Convert.ToInt32(_id[1]);
            // The new form is equal to the base species, but with a few changes.
            Species NewSpecies = (Species) BaseSpecies.Species.Clone();
            NewSpecies.LoadFormPBS(_id[0], hash);
            NewSpecies.Form = form;
            NewSpecies.ID = _id[0] + "_" + _id[1];
            Data.Species.Add(NewSpecies.ID, NewSpecies);
        }, Data.SetLoadProgress);
        Game.Species.PrevolutionsToRegister.ForEach(p =>
        {
            p.Item2.Species.Species.Prevolutions.Add(new Evolution((SpeciesResolver) p.Item1, p.Item2.Type, p.Item2.Parameter, true));
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
