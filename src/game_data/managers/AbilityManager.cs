﻿using RPGStudioMK.Utility;

namespace RPGStudioMK.Game;

public class AbilityManager : BaseDataManager
{
    public AbilityManager() : base("Ability", "abilities.dat", "abilities.txt", "abilitites") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading abilities");
        LoadAsHash((key, value) =>
        {
			if (!Ruby.Is(key, "Symbol", "String")) return;
			string ckey = Ruby.Symbol.FromPtr(key);
            Data.Abilities.Add(ckey, new Ability(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            Logger.WriteLine("Loading abilities from PBS with EV >= v20");
            FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
            {
                Data.Abilities.Add(id, new Ability(id, hash));
            }, Data.SetLoadProgress);
        }
        else
        {
            Logger.WriteLine("Loading abilities from PBS with EV < v20");
            FormattedTextParser.ParseLineByLineCommaBased(PBSFilename, line =>
            {
                Ability abilitydata = new Ability(line);
                Data.Abilities.Add(abilitydata.ID, abilitydata);
            }, Data.SetLoadProgress);
        }
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving abilities data");
        SaveDataAsHash(Data.Abilities.Values, abil => Ruby.Symbol.ToPtr(abil.ID));
    }

	protected override void SavePBS()
	{
		base.SavePBS();
        Logger.WriteLine("Saving abilities PBS");
        SaveAsPBS(Data.Abilities.Values);
	}

	public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing abilities");
        Data.Abilities.Clear();
    }
}
