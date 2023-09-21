using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RPGStudioMK.Game;

public class EncounterTable : IGameData
{
    public static nint Class => BaseDataManager.Classes["Encounter"];

    public string ID;
    public int MapID;
    public int Version;
    /// <summary>
    /// Key: EncounterType
    /// </summary>
    public Dictionary<string, int> StepChances;
    /// <summary>
    /// Key: EncounterType
    /// </summary>
    public Dictionary<string, List<Encounter>> Encounters;

    private EncounterTable()
    {

    }

    public EncounterTable(int MapID, int Version, List<string> lines)
    {
        this.ID = MapID.ToString() + "_" + Version.ToString();
        this.MapID = MapID;
        this.Version = Version;
        this.StepChances = new Dictionary<string, int>();
        this.Encounters = new Dictionary<string, List<Encounter>>();
        string? CurrentType = null;
        List<Encounter> CurrentEncounters = new List<Encounter>();
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            string[] split = line.Split(',');
            try
            {
                string type = Data.HardcodedData.Assert(split[0], Data.HardcodedData.EncounterTypes);
                if (split.Length == 2) StepChances.Add(type, Convert.ToInt32(split[1]));
                else StepChances.Add(type, 0);
                if (CurrentType != null)
                {
                    // Add previous encounter list
                    this.Encounters.Add(CurrentType, CurrentEncounters);
                    CurrentEncounters = new List<Encounter>();
                }
                CurrentType = type;
            }
            catch (Exception)
            {
                // Not a valid encounter type
                if (split.Length < 3) // But it was supposed to be
                {
                    throw; // Rethrow exception
                }
                Encounter enc = new Encounter(split);
                CurrentEncounters.Add(enc);
            }
        }
        this.Encounters.Add(CurrentType, CurrentEncounters);
    }

    public EncounterTable(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        this.MapID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@map"));
        this.Version = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@version"));
        nint StepChancesHash = Ruby.GetIVar(Data, "@step_chances");
        nint StepChancesKeys = Ruby.Hash.Keys(StepChancesHash);
        Ruby.Pin(StepChancesKeys);
        int StepChancesKeyLength = (int) Ruby.Array.Length(StepChancesKeys);
        this.StepChances = new Dictionary<string, int>();
        for (int i = 0; i < StepChancesKeyLength; i++)
        {
            nint rkey = Ruby.Array.Get(StepChancesKeys, i);
            nint rvalue = Ruby.Hash.Get(StepChancesHash, rkey);
            string key = Ruby.Symbol.FromPtr(rkey);
            int value = (int) Ruby.Integer.FromPtr(rvalue);
            this.StepChances.Add(Game.Data.HardcodedData.Assert(key, Game.Data.HardcodedData.EncounterTypes), value);
        }
        Ruby.Unpin(StepChancesKeys);
        nint TypesHash = Ruby.GetIVar(Data, "@types");
        nint TypesKeys = Ruby.Hash.Keys(TypesHash);
        Ruby.Pin(TypesKeys);
        int TypesKeyLength = (int) Ruby.Array.Length(TypesKeys);
        this.Encounters = new Dictionary<string, List<Encounter>>();
        for (int i = 0; i < TypesKeyLength; i++)
        {
            nint rkey = Ruby.Array.Get(TypesKeys, i);
            nint rvalue = Ruby.Hash.Get(TypesHash, rkey);
            string key = Ruby.Symbol.FromPtr(rkey);
            int ValueLength = (int) Ruby.Array.Length(rvalue);
            List<Encounter> Encs = new List<Encounter>();
            for (int j = 0; j < ValueLength; j++)
            {
                nint renc = Ruby.Array.Get(rvalue, j);
                Encs.Add(new Encounter(renc));
            }
            this.Encounters.Add(Game.Data.HardcodedData.Assert(key, Game.Data.HardcodedData.EncounterTypes), Encs);
        }
        Ruby.Unpin(TypesKeys);
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        Ruby.SetIVar(e, "@map", Ruby.Integer.ToPtr(this.MapID));
        Ruby.SetIVar(e, "@version", Ruby.Integer.ToPtr(this.Version));
        nint StepChancesHash = Ruby.Hash.Create();
        Ruby.SetIVar(e, "@step_chances", StepChancesHash);
        foreach (KeyValuePair<string, int> kvp in StepChances)
        {
            nint rkey = Ruby.Symbol.ToPtr(kvp.Key);
            nint rvalue = Ruby.Integer.ToPtr(kvp.Value);
            Ruby.Hash.Set(StepChancesHash, rkey, rvalue);
        }
        nint TypesHash = Ruby.Hash.Create();
        Ruby.SetIVar(e, "@types", TypesHash);
        foreach (KeyValuePair<string, List<Encounter>> kvp in Encounters)
        {
            nint rkey = Ruby.Symbol.ToPtr(kvp.Key);
            nint Ary = Ruby.Array.Create();
            Ruby.Hash.Set(TypesHash, rkey, Ary);
            foreach (Encounter enc in kvp.Value)
            {
                Ruby.Array.Push(Ary, enc.Save());
            }
        }
        Ruby.Unpin(e);
        return e;
	}

	public string SaveToString()
	{
		throw new NotImplementedException();
	}

	public object Clone()
    {
        EncounterTable t = new EncounterTable();
        t.ID = this.ID;
        t.MapID = this.MapID;
        t.Version = this.Version;
        t.StepChances = new Dictionary<string, int>(this.StepChances);
        t.Encounters = new Dictionary<string, List<Encounter>>();
        foreach (KeyValuePair<string, List<Encounter>> kvp in this.Encounters)
        {
            List<Encounter> NewList = kvp.Value.Select(x => (Encounter) x.Clone()).ToList();
            t.Encounters.Add(kvp.Key, NewList);
        }
        return t;
    }
}

[DebuggerDisplay("{Probability},{Species},{MinLevel}..{MaxLevel}")]
public class Encounter
{
    public int Probability;
    public SpeciesResolver Species;
    public int MinLevel;
    public int MaxLevel;

    private Encounter() { }

    public Encounter(string[] Line)
    {
        this.Probability = Convert.ToInt32(Line[0]);
        this.Species = (SpeciesResolver) Line[1];
        this.MinLevel = Convert.ToInt32(Line[2]);
        if (Line.Length == 4) this.MaxLevel = Convert.ToInt32(Line[3]);
        else this.MaxLevel = this.MinLevel;
    }

    public Encounter(nint Data)
    {
        this.Probability = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(Data, 0));
        this.Species = (SpeciesResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(Data, 1));
        this.MinLevel = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(Data, 2));
        this.MaxLevel = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(Data, 3));
    }

    public nint Save()
    {
        nint e = Ruby.Array.Create(4);
        Ruby.Pin(e);
        Ruby.Array.Set(e, 0, Ruby.Integer.ToPtr(this.Probability));
        Ruby.Array.Set(e, 1, Ruby.Symbol.ToPtr(this.Species));
        Ruby.Array.Set(e, 2, Ruby.Integer.ToPtr(this.MinLevel));
        Ruby.Array.Set(e, 3, Ruby.Integer.ToPtr(this.MaxLevel));
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Encounter e = new Encounter();
        e.Probability = this.Probability;
        e.Species = (SpeciesResolver) this.Species.ID;
        e.MinLevel = this.MinLevel;
        e.MaxLevel = this.MaxLevel;
        return e;
    }
}