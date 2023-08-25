using RPGStudioMK.Utility;
using System;
using System.Linq;

namespace RPGStudioMK.Game;

public class SpeciesManager : BaseDataManager
{
    public SpeciesManager() : base("Species", "species.dat", "pokemon.txt", "species") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading species");
        LoadAsHash((key, value) =>
        {
            string realID = Ruby.Symbol.FromPtr(Ruby.GetIVar(value, "@id"));
            if (Data.Species.ContainsKey(realID)) return;
            Species speciesdata = new Species(value);
            Data.Species.Add(speciesdata.ID, speciesdata);
        });
        RegisterPrevolutions();
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        Logger.WriteLine("Loading species from PBS");
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Species speciesdata = new Species(id, hash);
            Data.Species.Add(speciesdata.ID, speciesdata);
        }, Data.SetLoadProgress);
        Data.SetLoadText("Loading forms...");
        string formsFile = Data.ProjectPath + "/PBS/pokemon_forms.txt";
        if (!Game.Data.IsVersionAtLeast(EssentialsVersion.v20)) formsFile = Data.ProjectPath + "/PBS/pokemonforms.txt";
        Logger.WriteLine("Loading species forms from PBS");
        FormattedTextParser.ParseSectionBasedFile(formsFile, (id, hash) =>
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
        Logger.WriteLine("Registering prevolutions");
        RegisterPrevolutions();
    }

    protected void RegisterPrevolutions()
    {
        foreach (Species spc in Data.Species.Values)
        {
            spc.Prevolutions.Clear();
		    foreach (Species candidate in Data.Species.Values)
		    {
			    foreach (Evolution evo in candidate.Evolutions)
			    {
				    if (evo.Species.Species != spc) continue;
				    spc.Prevolutions.Add(new Evolution((SpeciesResolver) candidate, evo.Type, evo.Parameter, true));
			    }
		    }
        }
	}

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving species");
        SaveAsHash(Data.Species.Values, s => Ruby.Symbol.ToPtr(s.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing species");
        Data.Species.Clear();
    }
}
