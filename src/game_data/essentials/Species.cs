using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}")]
public class Species : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["Species"];

    public string ID;
    [Obsolete]
    public int? IDNumber;
	public SpeciesResolver BaseSpecies;
	public int Form;
	public string Name;
    public string? FormName;
	public string Category;
	public string PokedexEntry;
	public int PokedexForm;
	public TypeResolver Type1;
	public TypeResolver? Type2;
	public Stats BaseStats;
	public Stats EVs;
	public int BaseEXP;
	public string GrowthRate;
	public string GenderRatio;
	public int CatchRate;
	public int Happiness;
	public List<(int Level, MoveResolver Move)> Moves;
	public List<MoveResolver> TutorMoves;
	public List<MoveResolver> EggMoves;
	public List<AbilityResolver> Abilities;
	public List<AbilityResolver> HiddenAbilities;
	public List<ItemResolver> WildItemCommon;
	public List<ItemResolver> WildItemUncommon;
	public List<ItemResolver> WildItemRare;
	public List<string> EggGroups;
	public int HatchSteps;
	public ItemResolver? Incense;
	public List<SpeciesResolver> Offspring;
	public List<Evolution> Evolutions;
	public List<Evolution> Prevolutions;
	public float Height;
	public float Weight;
	public string Color;
	public string Shape;
	public string Habitat;
	public int Generation;
	public List<string> Flags;
	public ItemResolver? MegaStone;
	public MoveResolver? MegaMove;
	public int UnmegaForm;
	public int MegaMessage;

	/// <summary>
	/// DO NOT USE!
	/// </summary>
	public Species()
	{

	}

    public static Species Create()
    {
        Species s = new Species();
	    s.Form = 0;
	    s.Name = "";
	    s.Category = "";
	    s.PokedexEntry = "";
	    s.Type1 = (TypeResolver) (Type) Data.Sources.Types[0].Object;
	    s.BaseStats = new Stats(45);
	    s.EVs = new Stats(0);
	    s.BaseEXP = 32;
	    s.GrowthRate = Data.HardcodedData.GrowthRates[0];
	    s.GenderRatio = Data.HardcodedData.GenderRatios[0];
	    s.CatchRate = 255;
	    s.Happiness = 70;
	    s.Moves = new List<(int Level, MoveResolver Move)>();
	    s.TutorMoves = new List<MoveResolver>();
	    s.EggMoves = new List<MoveResolver>();
	    s.Abilities = new List<AbilityResolver>() { (AbilityResolver) (Ability) Data.Sources.Abilities[0].Object };
	    s.HiddenAbilities = new List<AbilityResolver>();
	    s.WildItemCommon = new List<ItemResolver>();
	    s.WildItemUncommon = new List<ItemResolver>();
	    s.WildItemRare = new List<ItemResolver>();
	    s.EggGroups = new List<string>() { Data.HardcodedData.EggGroups[0] };
	    s.HatchSteps = 5548;
        s.Offspring = new List<SpeciesResolver>();
        s.Evolutions = new List<Evolution>();
        s.Prevolutions = new List<Evolution>();
        s.Height = 1.0f;
        s.Weight = 1.0f;
        s.Color = Data.HardcodedData.BodyColors[0];
        s.Shape = Data.HardcodedData.BodyShapes[0];
        s.Habitat = Data.HardcodedData.Habitats[0];
        s.Generation = 0;
        s.Flags = new List<string>();
        return s;
}

	public Species(string ID, Dictionary<string, string> hash)
	{
        if (!Game.Data.IsVersionAtLeast(EssentialsVersion.v20)) this.IDNumber = Convert.ToInt32(ID);
        if (hash.ContainsKey("InternalName")) this.ID = hash["InternalName"];
        else this.ID = ID;
        this.BaseSpecies = (SpeciesResolver) ID;
        this.Name = hash["Name"];
        if (hash.ContainsKey("Types"))
        {
            if (hash["Types"].Contains(","))
            {
                string[] _types = hash["Types"].Split(',');
                this.Type1 = (TypeResolver) _types[0];
                this.Type2 = (TypeResolver) _types[1];
            }
            else this.Type1 = (TypeResolver) hash["Types"];
        }
        else
        {
            this.Type1 = (TypeResolver) hash["Type1"];
            if (hash.ContainsKey("Type2")) this.Type2 = (TypeResolver) hash["Type2"];
        }
        string[] _stats = hash["BaseStats"].Split(',');
        this.BaseStats = new Stats();
        this.BaseStats.HP = Convert.ToInt32(_stats[0]);
        this.BaseStats.Attack = Convert.ToInt32(_stats[1]);
        this.BaseStats.Defense = Convert.ToInt32(_stats[2]);
        this.BaseStats.SpecialAttack = Convert.ToInt32(_stats[4]);
        this.BaseStats.SpecialDefense = Convert.ToInt32(_stats[5]);
        this.BaseStats.Speed = Convert.ToInt32(_stats[3]);
        if (hash.ContainsKey("EVs"))
        {
            string[] _evs = hash["EVs"].Split(',');
            this.EVs = new Stats();
            for (int i = 0; i < _evs.Length - 1; i += 2)
            {
                string stat = _evs[i];
                int amt = Convert.ToInt32(_evs[i + 1]);
                int idx = stat switch
                {
                    "HP" => this.EVs.HP = amt,
                    "ATTACK" => this.EVs.Attack = amt,
                    "DEFENSE" => this.EVs.Defense = amt,
                    "SPECIAL_ATTACK" => this.EVs.SpecialAttack = amt,
                    "SPECIAL_DEFENSE" => this.EVs.SpecialDefense = amt,
                    "SPEED" => this.EVs.Speed = amt,
                    _ => throw new Exception("Invalid stat.")
                };
            }
        }
        else
        {
            string[] _evs = hash["EffortPoints"].Split(',');
            this.EVs = new Stats(_evs.Select(x => Convert.ToInt32(x.Trim())).ToList());
        }
        this.Form = 0;
        this.Category = hash.ContainsKey("Category") ? hash["Category"] : hash["Kind"];
        this.PokedexEntry = hash["Pokedex"];
        this.BaseEXP = Convert.ToInt32(hash.ContainsKey("BaseExp") ? hash["BaseExp"] : hash["BaseEXP"]);
        this.GrowthRate = Data.HardcodedData.Assert(hash["GrowthRate"], Data.HardcodedData.GrowthRates);
        this.GenderRatio = Data.HardcodedData.Assert(hash.ContainsKey("GenderRatio") ? hash["GenderRatio"] : hash["GenderRate"], Data.HardcodedData.GenderRatios);
        this.CatchRate = Convert.ToInt32(hash.ContainsKey("CatchRate") ? hash["CatchRate"] : hash["Rareness"]);
        this.Happiness = Convert.ToInt32(hash["Happiness"]);
        string[] _moves = hash["Moves"].Split(',');
        this.Moves = new List<(int, MoveResolver)>();
        for (int i = 0; i < _moves.Length - 1; i += 2)
        {
            int level = Convert.ToInt32(_moves[i]);
            MoveResolver move = (MoveResolver) _moves[i + 1];
            this.Moves.Add((level, move));
        }
		if (hash.ContainsKey("TutorMoves")) this.TutorMoves = hash["TutorMoves"].Split(',').Select(m => (MoveResolver) m.Trim()).ToList();
		else this.TutorMoves = new List<MoveResolver>();
		if (hash.ContainsKey("EggMoves")) this.EggMoves = hash["EggMoves"].Split(',').Select(m => (MoveResolver) m.Trim()).ToList();
		else this.EggMoves = new List<MoveResolver>();
        this.Abilities = hash["Abilities"].Split(',').Select(m => (AbilityResolver) m.Trim()).ToList();
		if (hash.ContainsKey("HiddenAbilities")) this.HiddenAbilities = hash["HiddenAbilities"].Split(',').Select(m => (AbilityResolver) m.Trim()).ToList();
        else if (hash.ContainsKey("HiddenAbility")) this.HiddenAbilities = hash["HiddenAbility"].Split(',').Select(m => (AbilityResolver) m.Trim()).ToList();
		else this.HiddenAbilities = new List<AbilityResolver>();
		if (hash.ContainsKey("WildItemCommon")) this.WildItemCommon = hash["WildItemCommon"].Split(',').Select(m => (ItemResolver) m.Trim()).ToList();
		else this.WildItemCommon = new List<ItemResolver>();
		if (hash.ContainsKey("WildItemUncommon")) this.WildItemUncommon = hash["WildItemUncommon"].Split(',').Select(m => (ItemResolver) m.Trim()).ToList();
		else this.WildItemUncommon = new List<ItemResolver>();
		if (hash.ContainsKey("WildItemRare")) this.WildItemRare = hash["WildItemRare"].Split(',').Select(m => (ItemResolver) m.Trim()).ToList();
		else this.WildItemRare = new List<ItemResolver>();
        this.EggGroups = (hash.ContainsKey("EggGroups") ? hash["EggGroups"] : hash["Compatibility"]).Split(',').Select(m => Data.HardcodedData.Assert(m, Data.HardcodedData.EggGroups)).ToList();
        this.HatchSteps = Convert.ToInt32(hash.ContainsKey("HatchSteps") ? hash["HatchSteps"] : hash["StepsToHatch"]);
        if (hash.ContainsKey("Incense")) this.Incense = (ItemResolver) hash["Incense"];
		if (hash.ContainsKey("Offspring")) this.Offspring = hash["Offspring"].Split(',').Select(s => (SpeciesResolver) s.Trim()).ToList();
		else this.Offspring = new List<SpeciesResolver>();
		this.Evolutions = new List<Evolution>();
		this.Prevolutions = new List<Evolution>();
		if (hash.ContainsKey("Evolutions"))
		{
			string[] _evos = hash["Evolutions"].Split(',');
			for (int i = 0; i < _evos.Length - 2; i += 3)
			{
				string species = _evos[i];
				string method = _evos[i + 1];
				string param = _evos[i + 2];
				Evolution evo = new Evolution((SpeciesResolver) species, method, string.IsNullOrEmpty(param) ? new List<object>() : new List<object>() { param }, false);
				this.Evolutions.Add(evo);
			}
		}
        this.Height = (float) Convert.ToDouble(hash["Height"]);
        this.Weight = (float) Convert.ToDouble(hash["Weight"]);
		this.Color = Data.HardcodedData.Assert(hash["Color"], Data.HardcodedData.BodyColors);
		this.Shape = Data.HardcodedData.Assert(hash["Shape"], Data.HardcodedData.BodyShapes);
		if (hash.ContainsKey("Habitat")) this.Habitat = Data.HardcodedData.Assert(hash["Habitat"], Data.HardcodedData.Habitats);
		this.Generation = Convert.ToInt32(hash["Generation"]);
		if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').ToList();
		else this.Flags = new List<string>();
		if (hash.ContainsKey("MegaStone")) this.MegaStone = (ItemResolver) hash["MegaStone"];
		if (hash.ContainsKey("MegaMove")) this.MegaMove = (MoveResolver) hash["MegaMove"];
		if (hash.ContainsKey("UnmegaForm")) this.UnmegaForm = Convert.ToInt32(hash["UnmegaForm"]);
		if (hash.ContainsKey("MegaMessage")) this.MegaMessage = Convert.ToInt32(hash["MegaMessage"]);
	}

    public void LoadFormPBS(string ID, Dictionary<string, string> hash)
    {
        if (hash.ContainsKey("Types"))
        {
            if (hash["Types"].Contains(','))
            {
                string[] _types = hash["Types"].Split(',').Select(x => x.Trim()).ToArray();
                this.Type1 = (TypeResolver) _types[0];
                this.Type2 = (TypeResolver) _types[1];
            }
            else this.Type1 = (TypeResolver) hash["Types"];
        }
        if (hash.ContainsKey("Type1"))
        {
            this.Type1 = (TypeResolver) hash["Type1"];
        }
        if (hash.ContainsKey("Type2"))
        {
            this.Type1 = (TypeResolver) hash["Type2"];
        }
        if (hash.ContainsKey("BaseStats"))
        {
            string[] _stats = hash["BaseStats"].Split(',');
            this.BaseStats = new Stats();
            this.BaseStats.HP = Convert.ToInt32(_stats[0]);
            this.BaseStats.Attack = Convert.ToInt32(_stats[1]);
            this.BaseStats.Defense = Convert.ToInt32(_stats[2]);
            this.BaseStats.SpecialAttack = Convert.ToInt32(_stats[4]);
            this.BaseStats.SpecialDefense = Convert.ToInt32(_stats[5]);
            this.BaseStats.Speed = Convert.ToInt32(_stats[3]);
        }
        if (hash.ContainsKey("EVs"))
        {
            string[] _evs = hash["EVs"].Split(',');
            this.EVs = new Stats();
            for (int i = 0; i < _evs.Length - 1; i += 2)
            {
                string stat = _evs[i];
                int amt = Convert.ToInt32(_evs[i + 1]);
                int idx = stat switch
                {
                    "HP" => this.EVs.HP = amt,
                    "ATTACK" => this.EVs.Attack = amt,
                    "DEFENSE" => this.EVs.Defense = amt,
                    "SPECIAL_ATTACK" => this.EVs.SpecialAttack = amt,
                    "SPECIAL_DEFENSE" => this.EVs.SpecialDefense = amt,
                    "SPEED" => this.EVs.Speed = amt,
                    _ => throw new Exception("Invalid stat.")
                };
            }
        }
        if (hash.ContainsKey("EffortPoints"))
        {
            this.EVs = new Stats(hash["EffortPoints"].Split(',').Select(x => Convert.ToInt32(x.Trim())).ToList());
        }
        if (hash.ContainsKey("CatchRate"))
        {
            this.CatchRate = Convert.ToInt32(hash["CatchRate"]);
        }
        if (hash.ContainsKey("Rareness"))
        {
            this.CatchRate = Convert.ToInt32(hash["Rareness"]);
        }
        if (hash.ContainsKey("Happiness"))
        {
            this.Happiness = Convert.ToInt32(hash["Happiness"]);
        }
        if (hash.ContainsKey("Abilities"))
        {
            this.Abilities = hash["Abilities"].Split(',').Select(m => (AbilityResolver) m.Trim()).ToList();
        }
        if (hash.ContainsKey("HiddenAbilities") || hash.ContainsKey("HiddenAbility"))
        {
            this.HiddenAbilities = (hash.ContainsKey("HiddenAbilities") ? hash["HiddenAbilities"] : hash["HiddenAbility"]).Split(',').Select(m => (AbilityResolver) m.Trim()).ToList();
        }
        if (hash.ContainsKey("Moves"))
        {
            string[] _moves = hash["Moves"].Split(',');
            this.Moves = new List<(int, MoveResolver)>();
            for (int i = 0; i < _moves.Length - 1; i += 2)
            {
                int level = Convert.ToInt32(_moves[i]);
                MoveResolver move = (MoveResolver) _moves[i + 1];
                this.Moves.Add((level, move));
            }
        }
        if (hash.ContainsKey("TutorMoves"))
        {
            this.TutorMoves = hash["TutorMoves"].Split(',').Select(m => (MoveResolver) m.Trim()).ToList();
        }
        if (hash.ContainsKey("EggMoves"))
        {
            this.EggMoves = hash["EggMoves"].Split(',').Select(m => (MoveResolver) m.Trim()).ToList();
        }
        if (hash.ContainsKey("EggGroups") || hash.ContainsKey("Compatibility"))
        {
            this.EggGroups = (hash.ContainsKey("EggGroups") ? hash["EggGroups"] : hash["Compatibility"]).Split(',').Select(m => Data.HardcodedData.Assert(m, Data.HardcodedData.EggGroups)).ToList();
        }
        if (hash.ContainsKey("HatchSteps"))
        {
            this.HatchSteps = Convert.ToInt32(hash["HatchSteps"]);
        }
        if (hash.ContainsKey("StepsToHatch"))
        {
            this.HatchSteps = Convert.ToInt32(hash["StepsToHatch"]);
        }
        if (hash.ContainsKey("Offspring"))
        {
            this.Offspring = hash["Offspring"].Split(',').Select(s => (SpeciesResolver) s.Trim()).ToList();
        }
        if (hash.ContainsKey("Height"))
        {
            this.Height = (float) Convert.ToDouble(hash["Height"]);
        }
        if (hash.ContainsKey("Weight"))
        {
            this.Weight = (float) Convert.ToDouble(hash["Weight"]);
        }
        if (hash.ContainsKey("Color"))
        {
		    this.Color = Data.HardcodedData.Assert(hash["Color"], Data.HardcodedData.BodyColors);
        }
        if (hash.ContainsKey("Shape"))
        {
            this.Shape = Data.HardcodedData.Assert(hash["Shape"], Data.HardcodedData.BodyShapes);
        }
        if (hash.ContainsKey("Habitat"))
        {
            this.Habitat = Data.HardcodedData.Assert(hash["Habitat"], Data.HardcodedData.Habitats);
        }
        if (hash.ContainsKey("Category"))
        {
            this.Category = hash["Category"];
        }
        if (hash.ContainsKey("Kind"))
        {
            this.Category = hash["Kind"];
        }
        if (hash.ContainsKey("Pokedex"))
        {
            this.PokedexEntry = hash["Pokedex"];
        }
        if (hash.ContainsKey("FormName"))
        {
            this.FormName = hash["FormName"];
        }
        if (hash.ContainsKey("Generation"))
        {
            this.Generation = Convert.ToInt32(hash["Generation"]);
        }
        if (hash.ContainsKey("Flags"))
        {
            this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        }
        if (hash.ContainsKey("WildItemCommon"))
        {
            this.WildItemCommon = hash["WildItemCommon"].Split(',').Select(m => (ItemResolver) m.Trim()).ToList();
        }
        if (hash.ContainsKey("WildItemUncommon"))
        {
            this.WildItemUncommon = hash["WildItemUncommon"].Split(',').Select(m => (ItemResolver) m.Trim()).ToList();
        }
        if (hash.ContainsKey("WildItemRare"))
        {
            this.WildItemRare = hash["WildItemRare"].Split(',').Select(m => (ItemResolver) m.Trim()).ToList();
        }
        if (hash.ContainsKey("Evolutions"))
        {
            string[] _evos = hash["Evolutions"].Split(',');
            this.Evolutions = new List<Evolution>();
            for (int i = 0; i < _evos.Length - 2; i += 3)
            {
                string species = _evos[i];
                string method = _evos[i + 1];
                string param = _evos[i + 2];
                Evolution evo = new Evolution((SpeciesResolver)species, method, string.IsNullOrEmpty(param) ? new List<object>() : new List<object>() { param }, false);
                this.Evolutions.Add(evo);
            }
        }
        if (hash.ContainsKey("MegaStone"))
        {
            this.MegaStone = (ItemResolver) hash["MegaStone"];
        }
        if (hash.ContainsKey("MegaMove"))
        {
            this.MegaMove = (MoveResolver) hash["MegaMove"];
        }
        if (hash.ContainsKey("MegaMessage"))
        {
            this.MegaMessage = Convert.ToInt32(hash["MegaMessage"]);
        }
        if (hash.ContainsKey("UnmegaForm"))
        {
            this.UnmegaForm = Convert.ToInt32(hash["UnmegaForm"]);
        }
        if (hash.ContainsKey("PokedexForm"))
        {
            this.PokedexForm = Convert.ToInt32(hash["PokedexForm"]);
        }
    }

	public Species(nint Data)
	{
		this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
		this.BaseSpecies = (SpeciesResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@species"));
		this.Form = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@form"));
		this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
		this.FormName = Ruby.GetIVar(Data, "@real_form_name") == Ruby.Nil ? null : Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_form_name"));
		this.Category = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_category"));
		this.PokedexEntry = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_pokedex_entry"));
		this.PokedexForm = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@pokedex_form"));
		nint TypeArray = Ruby.GetIVar(Data, "@types");
        if (TypeArray == Ruby.Nil && Ruby.GetIVar(Data, "@type1") != Ruby.Nil)
        {
            Type1 = (TypeResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@type1"));
            nint t2 = Ruby.GetIVar(Data, "@type2");
            if (t2 != Ruby.Nil) Type2 = (TypeResolver) Ruby.Symbol.FromPtr(t2);
        }
		else
        {
            if (Ruby.Array.Length(TypeArray) == 2) Type2 = (TypeResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(TypeArray, 1));
		    Type1 = (TypeResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(TypeArray, 0));
        }
		this.BaseStats = new Stats(Ruby.GetIVar(Data, "@base_stats"));
		this.EVs = new Stats(Ruby.GetIVar(Data, "@evs"));
		this.BaseEXP = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@base_exp"));
		string rgrowth = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@growth_rate"));
		this.GrowthRate = Game.Data.HardcodedData.Assert(rgrowth, Game.Data.HardcodedData.GrowthRates);
		string rratio = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@gender_ratio"));
		this.GenderRatio = Game.Data.HardcodedData.Assert(rratio, Game.Data.HardcodedData.GenderRatios);
		this.CatchRate = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@catch_rate"));
		this.Happiness = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@happiness"));
		nint MoveArray = Ruby.GetIVar(Data, "@moves");
		int MoveArrayLength = (int) Ruby.Array.Length(MoveArray);
		this.Moves = new List<(int, MoveResolver)>();
		for (int i = 0; i < MoveArrayLength; i++)
		{
			nint robj = Ruby.Array.Get(MoveArray, i);
			int level = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(robj, 0));
			MoveResolver move = (MoveResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(robj, 1));
			this.Moves.Add((level, move));
		}
		nint TutorMoveArray = Ruby.GetIVar(Data, "@tutor_moves");
		int TutorMoveArrayLength = (int) Ruby.Array.Length(TutorMoveArray);
		this.TutorMoves = new List<MoveResolver>();
		for (int i = 0; i < TutorMoveArrayLength; i++)
		{
			string move = Ruby.Symbol.FromPtr(Ruby.Array.Get(TutorMoveArray, i));
			this.TutorMoves.Add((MoveResolver) move);
		}
		nint EggMoveArray = Ruby.GetIVar(Data, "@egg_moves");
		int EggMoveArrayLength = (int) Ruby.Array.Length(EggMoveArray);
		this.EggMoves = new List<MoveResolver>();
		for (int i = 0; i < EggMoveArrayLength; i++)
		{
			string move = Ruby.Symbol.FromPtr(Ruby.Array.Get(EggMoveArray, i));
			this.EggMoves.Add((MoveResolver) move);
		}
		nint AbilityArray = Ruby.GetIVar(Data, "@abilities");
		int AbilityArrayLength = (int) Ruby.Array.Length(AbilityArray);
		this.Abilities = new List<AbilityResolver>();
		for (int i = 0; i < AbilityArrayLength; i++)
		{
			string ability = Ruby.Symbol.FromPtr(Ruby.Array.Get(AbilityArray, i));
			this.Abilities.Add((AbilityResolver) ability);
		}
		nint HiddenAbilityArray = Ruby.GetIVar(Data, "@hidden_abilities");
		int HiddenAbilityArrayLength = (int) Ruby.Array.Length(HiddenAbilityArray);
		this.HiddenAbilities = new List<AbilityResolver>();
		for (int i = 0; i < HiddenAbilityArrayLength; i++)
		{
			string ability = Ruby.Symbol.FromPtr(Ruby.Array.Get(HiddenAbilityArray, i));
			this.HiddenAbilities.Add((AbilityResolver) ability);
		}
        nint WildItemCommonData = Ruby.GetIVar(Data, "@wild_item_common");
		nint WildItemUncommonData = Ruby.GetIVar(Data, "@wild_item_uncommon");
		nint WildItemRareData = Ruby.GetIVar(Data, "@wild_item_rare");
        this.WildItemCommon = new List<ItemResolver>();
        this.WildItemUncommon = new List<ItemResolver>();
        this.WildItemRare = new List<ItemResolver>();
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            int WildItemCommonArrayLength = (int) Ruby.Array.Length(WildItemCommonData);
            for (int i = 0; i < WildItemCommonArrayLength; i++)
            {
                string item = Ruby.Symbol.FromPtr(Ruby.Array.Get(WildItemCommonData, i));
			    this.WildItemCommon.Add((ItemResolver) item);
            }
            int WildItemUncommonArrayLength = (int) Ruby.Array.Length(WildItemUncommonData);
            for (int i = 0; i < WildItemUncommonArrayLength; i++)
            {
                string item = Ruby.Symbol.FromPtr(Ruby.Array.Get(WildItemUncommonData, i));
			    this.WildItemUncommon.Add((ItemResolver) item);
            }
            int WildItemRareArrayLength = (int) Ruby.Array.Length(WildItemRareData);
            for (int i = 0; i < WildItemRareArrayLength; i++)
            {
                string item = Ruby.Symbol.FromPtr(Ruby.Array.Get(WildItemRareData, i));
			    this.WildItemRare.Add((ItemResolver) item);
            }
        }
        else
        {
            if (WildItemCommonData != Ruby.Nil) this.WildItemCommon.Add((ItemResolver) Ruby.Symbol.FromPtr(WildItemCommonData));
            if (WildItemUncommonData != Ruby.Nil) this.WildItemUncommon.Add((ItemResolver) Ruby.Symbol.FromPtr(WildItemUncommonData));
            if (WildItemRareData != Ruby.Nil) this.WildItemRare.Add((ItemResolver) Ruby.Symbol.FromPtr(WildItemRareData));
        }
        nint EggGroupsArray = Ruby.GetIVar(Data, "@egg_groups");
        int EggGroupsArrayLength = (int) Ruby.Array.Length(EggGroupsArray);
        this.EggGroups = new List<string>();
        for (int i = 0; i < EggGroupsArrayLength; i++)
        {
            string egggroup = Ruby.Symbol.FromPtr(Ruby.Array.Get(EggGroupsArray, i));
			this.EggGroups.Add(Game.Data.HardcodedData.Assert(egggroup, Game.Data.HardcodedData.EggGroups));
        }
		this.HatchSteps = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@hatch_steps"));
		this.Incense = Ruby.GetIVar(Data, "@incense") == Ruby.Nil ? null : (ItemResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@incense"));
		nint OffspringArray = Ruby.GetIVar(Data, "@offspring");
		this.Offspring = new List<SpeciesResolver>();
        if (OffspringArray != Ruby.Nil)
        {
		    int OffspringArraylength = (int) Ruby.Array.Length(OffspringArray);
		    for (int i = 0; i < OffspringArraylength; i++)
		    {
			    string species = Ruby.Symbol.FromPtr(Ruby.Array.Get(OffspringArray, i));
			    this.Offspring.Add((SpeciesResolver) species);
		    }
        }
		this.Height = Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@height")) / 10f;
		this.Weight = Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@weight")) / 10f;
		string rcolor = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@color"));
		this.Color = Game.Data.HardcodedData.Assert(rcolor, Game.Data.HardcodedData.BodyColors);
		string rshape = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@shape"));
		this.Shape = Game.Data.HardcodedData.Assert(rshape, Game.Data.HardcodedData.BodyShapes);
		string rhabitat = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@habitat"));
        this.Habitat = Game.Data.HardcodedData.Assert(rhabitat, Game.Data.HardcodedData.Habitats);
		this.Generation = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@generation"));
		nint FlagsArray = Ruby.GetIVar(Data, "@flags");
		this.Flags = new List<string>();
        if (FlagsArray != Ruby.Nil)
        {
		    int FlagsArrayLength = (int) Ruby.Array.Length(FlagsArray);
		    for (int i = 0; i < FlagsArrayLength; i++)
		    {
			    string flag = Ruby.String.FromPtr(Ruby.Array.Get(FlagsArray, i));
			    this.Flags.Add(flag);
            }
		}
		this.MegaStone = Ruby.GetIVar(Data, "@mega_stone") == Ruby.Nil ? null : (ItemResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@mega_stone"));
        this.MegaMove = Ruby.GetIVar(Data, "@mega_move") == Ruby.Nil ? null : (MoveResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@mega_move"));
		this.UnmegaForm = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@unmega_form"));
		this.MegaMessage = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@mega_message"));
		nint EvolutionsArray = Ruby.GetIVar(Data, "@evolutions");
		int EvolutionsArrayLength = (int) Ruby.Array.Length(EvolutionsArray);
		this.Evolutions = new List<Evolution>();
		this.Prevolutions = new List<Evolution>();
		for (int i = 0; i < EvolutionsArrayLength; i++)
		{
			Evolution evo = new Evolution(Ruby.Array.Get(EvolutionsArray, i));
            if (evo.Prevolution) continue; // Skip prevolutions; we add this later in one pass over all species for reliability
			this.Evolutions.Add(evo);
		}
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        Ruby.SetIVar(e, "@species", Ruby.Symbol.ToPtr(this.BaseSpecies));
        Ruby.SetIVar(e, "@form", Ruby.Integer.ToPtr(this.Form));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@real_form_name", this.FormName == null ? Ruby.Nil : Ruby.String.ToPtr(this.FormName));
        Ruby.SetIVar(e, "@real_category", Ruby.String.ToPtr(this.Category));
        Ruby.SetIVar(e, "@real_pokedex_entry", Ruby.String.ToPtr(this.PokedexEntry));
        Ruby.SetIVar(e, "@pokedex_form", Ruby.Integer.ToPtr(this.PokedexForm));
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            nint TypeArray = Ruby.Array.Create();
            Ruby.Array.Push(TypeArray, Ruby.Symbol.ToPtr(Type1));
            if (Type2 != null) Ruby.Array.Push(TypeArray, Ruby.Symbol.ToPtr(Type2));
            Ruby.SetIVar(e, "@types", TypeArray);
        }
        else
        {
            Ruby.SetIVar(e, "@type1", Ruby.Symbol.ToPtr(Type1));
            if (!string.IsNullOrEmpty(Type2)) Ruby.SetIVar(e, "@type2", Ruby.Symbol.ToPtr(Type2));
        }
		Ruby.SetIVar(e, "@base_stats", this.BaseStats.Save());
		Ruby.SetIVar(e, "@evs", this.EVs.Save());
		Ruby.SetIVar(e, "@base_exp", Ruby.Integer.ToPtr(this.BaseEXP));
		Ruby.SetIVar(e, "@growth_rate", Ruby.Symbol.ToPtr(this.GrowthRate));
		Ruby.SetIVar(e, "@gender_ratio", Ruby.Symbol.ToPtr(this.GenderRatio));
		Ruby.SetIVar(e, "@catch_rate", Ruby.Integer.ToPtr(this.CatchRate));
		Ruby.SetIVar(e, "@happiness", Ruby.Integer.ToPtr(this.Happiness));
		nint MovesArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@moves", MovesArray);
		foreach ((int Level, string Move) in Moves)
		{
			nint MoveArray = Ruby.Array.Create(2);
			Ruby.Array.Set(MoveArray, 0, Ruby.Integer.ToPtr(Level));
			Ruby.Array.Set(MoveArray, 1, Ruby.Symbol.ToPtr(Move));
			Ruby.Array.Push(MovesArray, MoveArray);
		}
		nint TutorMovesArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@tutor_moves", TutorMovesArray);
		foreach (string Move in TutorMoves)
		{
			Ruby.Array.Push(TutorMovesArray, Ruby.Symbol.ToPtr(Move));
		}
		nint EggMovesArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@egg_moves", EggMovesArray);
		foreach (string Move in EggMoves)
		{
			Ruby.Array.Push(EggMovesArray, Ruby.Symbol.ToPtr(Move));
		}
		nint AbilitiesArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@abilities", AbilitiesArray);
		foreach (string Ability in Abilities)
		{
			Ruby.Array.Push(AbilitiesArray, Ruby.Symbol.ToPtr(Ability));
		}
        nint HiddenAbilitiesArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@hidden_abilities", HiddenAbilitiesArray);
        foreach (string Ability in HiddenAbilities)
        {
            Ruby.Array.Push(HiddenAbilitiesArray, Ruby.Symbol.ToPtr(Ability));
        }
		nint WildItemCommonArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@wild_item_common", WildItemCommonArray);
		foreach (string Item in WildItemCommon)
		{
			Ruby.Array.Push(WildItemCommonArray, Ruby.Symbol.ToPtr(Item));
		}
		nint WildItemUncommonArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@wild_item_uncommon", WildItemUncommonArray);
		foreach (string Item in WildItemUncommon)
		{
			Ruby.Array.Push(WildItemUncommonArray, Ruby.Symbol.ToPtr(Item));
		}
        nint WildItemRareArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@wild_item_rare", WildItemRareArray);
        foreach (string Item in WildItemRare)
        {
            Ruby.Array.Push(WildItemRareArray, Ruby.Symbol.ToPtr(Item));
        }
        nint EggGroupsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@egg_groups", EggGroupsArray);
        foreach (string group in EggGroups)
        {
            Ruby.Array.Push(EggGroupsArray, Ruby.Symbol.ToPtr(group));
        }
		Ruby.SetIVar(e, "@hatch_steps", Ruby.Integer.ToPtr(this.HatchSteps));
		Ruby.SetIVar(e, "@incense", this.Incense == null ? Ruby.Nil : Ruby.Symbol.ToPtr(this.Incense));
		nint OffSpringArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@offspring", OffSpringArray);
		foreach (string Species in Offspring)
		{
			Ruby.Array.Push(OffSpringArray, Ruby.Symbol.ToPtr(Species));
		}
		Ruby.SetIVar(e, "@height", Ruby.Integer.ToPtr((int) (this.Height * 10)));
		Ruby.SetIVar(e, "@weight", Ruby.Integer.ToPtr((int) (this.Weight * 10)));
		Ruby.SetIVar(e, "@color", Ruby.Symbol.ToPtr(this.Color));
		Ruby.SetIVar(e, "@shape", Ruby.Symbol.ToPtr(this.Shape));
		Ruby.SetIVar(e, "@habitat", Ruby.Symbol.ToPtr(this.Habitat));
		Ruby.SetIVar(e, "@generation", Ruby.Integer.ToPtr(this.Generation));
		nint FlagsArray = Ruby.Array.Create();
		Ruby.SetIVar(e, "@flags", FlagsArray);
		foreach (string Flag in Flags)
		{
			Ruby.Array.Push(FlagsArray, Ruby.String.ToPtr(Flag));
		}
		Ruby.SetIVar(e, "@mega_stone", this.MegaStone == null ? Ruby.Nil : Ruby.Symbol.ToPtr(this.MegaStone));
		Ruby.SetIVar(e, "@mega_move", this.MegaMove == null ? Ruby.Nil : Ruby.Symbol.ToPtr(this.MegaMove));
		Ruby.SetIVar(e, "@unmega_form", Ruby.Integer.ToPtr(this.UnmegaForm));
		Ruby.SetIVar(e, "@mega_message", Ruby.Integer.ToPtr(this.MegaMessage));
        nint EvolutionsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@evolutions", EvolutionsArray);
        foreach (Evolution evo in Evolutions)
        {
            Ruby.Array.Push(EvolutionsArray, evo.Save());
        }
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Species s = new Species();
		s.ID = this.ID;
		s.BaseSpecies = (SpeciesResolver) this.BaseSpecies.ID;
		s.Form = this.Form;
		s.Name = this.Name;
		s.FormName = this.FormName;
		s.Category = this.Category;
		s.PokedexEntry = this.PokedexEntry;
		s.PokedexForm = this.PokedexForm;
		s.Type1 = (TypeResolver) this.Type1.ID;
		if (this.Type2 != null) s.Type2 = (TypeResolver) this.Type2.ID;
		s.BaseStats = (Stats) this.BaseStats.Clone();
		s.EVs = (Stats) this.EVs.Clone();
		s.BaseEXP = this.BaseEXP;
		s.GrowthRate = this.GrowthRate;
		s.GenderRatio = this.GenderRatio;
		s.CatchRate = this.CatchRate;
		s.Happiness = this.Happiness;
		s.Moves = this.Moves.Select(x => (x.Level, (MoveResolver) x.Move.ID)).ToList();
		s.TutorMoves = this.TutorMoves.Select(x => (MoveResolver) x.ID).ToList();
		s.EggMoves = this.EggMoves.Select(x => (MoveResolver) x.ID).ToList();
		s.Abilities = this.Abilities.Select(x => (AbilityResolver) x.ID).ToList();
		s.HiddenAbilities = this.HiddenAbilities.Select(x => (AbilityResolver) x.ID).ToList();
		s.WildItemCommon = this.WildItemCommon.Select(x => (ItemResolver) x.ID).ToList();
        s.WildItemUncommon = this.WildItemUncommon.Select(x => (ItemResolver) x.ID).ToList();
        s.WildItemRare = this.WildItemRare.Select(x => (ItemResolver) x.ID).ToList();
        s.EggGroups = new List<string>(this.EggGroups);
        s.HatchSteps = this.HatchSteps;
        if (this.Incense != null) s.Incense = (ItemResolver) this.Incense.ID;
        s.Offspring = this.Offspring.Select(x => (SpeciesResolver) x.ID).ToList();
        s.Evolutions = this.Evolutions.Select(evo => (Evolution) evo.Clone()).ToList();
        s.Prevolutions = this.Evolutions.Select(evo => (Evolution) evo.Clone()).ToList();
        s.Height = this.Height;
        s.Weight = this.Weight;
        s.Color = this.Color;
        s.Shape = this.Shape;
        s.Habitat = this.Habitat;
        s.Generation = this.Generation;
        s.Flags = new List<string>(this.Flags);
        if (this.MegaStone != null) s.MegaStone = (ItemResolver) this.MegaStone.ID;
        if (this.MegaMove != null) s.MegaMove = (MoveResolver) this.MegaMove.ID;
        s.UnmegaForm = this.UnmegaForm;
        s.MegaMessage = this.MegaMessage;
        return s;
    }
}

[DebuggerDisplay("{ID}")]
public class SpeciesResolver : IDataResolver
{
	public string ID { get; set; }
    [JsonIgnore]
	public bool Valid => !string.IsNullOrEmpty(ID) && Data.Species.ContainsKey(ID);
    [JsonIgnore]
    public Species Species => Data.Species[ID];

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public SpeciesResolver() { }

    public SpeciesResolver(string ID)
	{
		this.ID = ID;
	}

    public SpeciesResolver(Species Species)
    {
        this.ID = Species.ID;
    }

    public static implicit operator string(SpeciesResolver s) => s.ID;
	public static implicit operator Species(SpeciesResolver s) => s.Species;
	public static explicit operator SpeciesResolver(Species s) => new SpeciesResolver(s);
	public static explicit operator SpeciesResolver(string ID) => new SpeciesResolver(ID);
}
