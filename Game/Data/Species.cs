using amethyst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static rubydotnet.Ruby;

namespace RPGStudioMK.Game;

public class Species
{
	public static nint Class = nint.Zero;
	public static List<(Species, Evolution)> PrevolutionsToRegister = new List<(Species, Evolution)>();

    public string ID;
	public string SpeciesName;
	public int Form;
	public string RealName;
	public string? RealFormName;
	public string RealCategory;
	public string RealPokedexEntry;
	public int PokedexForm;
	public string Type1;
	public string? Type2;
	public Stats BaseStats;
	public Stats EVs;
	public int BaseEXP;
	public GrowthRate GrowthRate;
	public GenderRatio GenderRatio;
	public int CatchRate;
	public int Happiness;
	public List<(int, string)> Moves;
	public List<string> TutorMoves;
	public List<string> EggMoves;
	public List<string> Abilities;
	public List<string> HiddenAbilities;
	public List<string> WildItemCommon;
	public List<string> WildItemUncommon;
	public List<string> WildItemRare;
	public List<EggGroup> EggGroups;
	public int HatchSteps;
	public string? Incense;
	public List<SpeciesResolver> Offspring;
	public List<Evolution> Evolutions;
	public List<Evolution> Prevolutions;
	public float Height;
	public float Weight;
	public BodyColor Color;
	public BodyShape Shape;
	public Habitat Habitat;
	public int Generation;
	public List<string> Flags;
	public string? MegaStone;
	public string? MegaMove;
	public int UnmegaForm;
	public int MegaMessage;

	/// <summary>
	/// DO NOT USE!
	/// </summary>
	public Species()
	{

	}

	public Species(string ID, Dictionary<string, string> hash)
	{
        this.ID = ID;
        this.SpeciesName = ID;
        this.RealName = hash["Name"];
        if (hash["Types"].Contains(","))
        {
            string[] _types = hash["Types"].Split(',');
            this.Type1 = _types[0];
            this.Type2 = _types[1];
        }
        string[] _stats = hash["BaseStats"].Split(',');
        this.BaseStats = new Stats();
        this.BaseStats.HP = Convert.ToInt32(_stats[0]);
        this.BaseStats.Attack = Convert.ToInt32(_stats[1]);
        this.BaseStats.Defense = Convert.ToInt32(_stats[2]);
        this.BaseStats.SpecialAttack = Convert.ToInt32(_stats[3]);
        this.BaseStats.SpecialDefense = Convert.ToInt32(_stats[4]);
        this.BaseStats.Speed = Convert.ToInt32(_stats[5]);
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
        this.Form = 0;
        this.RealCategory = hash["Category"];
        this.RealPokedexEntry = hash["Pokedex"];
        this.BaseEXP = Convert.ToInt32(hash["BaseExp"]);
        this.GrowthRate = GrowthRateStrToEnum(hash["GrowthRate"]);
        this.GenderRatio = GenderRatioStrToEnum(hash["GenderRatio"]);
        this.CatchRate = Convert.ToInt32(hash["CatchRate"]);
        this.Happiness = Convert.ToInt32(hash["Happiness"]);
        string[] _moves = hash["Moves"].Split(',');
        this.Moves = new List<(int, string)>();
        for (int i = 0; i < _moves.Length - 1; i += 2)
        {
            int level = Convert.ToInt32(_moves[i]);
            string move = _moves[i + 1];
            this.Moves.Add((level, move));
        }
		if (hash.ContainsKey("TutorMoves")) this.TutorMoves = hash["TutorMoves"].Split(',').Select(m => m.Trim()).ToList();
		else this.TutorMoves = new List<string>();
		if (hash.ContainsKey("EggMoves")) this.EggMoves = hash["EggMoves"].Split(',').Select(m => m.Trim()).ToList();
		else this.EggMoves = new List<string>();
        this.Abilities = hash["Abilities"].Split(',').Select(m => m.Trim()).ToList();
		if (hash.ContainsKey("HiddenAbilities")) this.HiddenAbilities = hash["HiddenAbilities"].Split(',').Select(m => m.Trim()).ToList();
		else this.HiddenAbilities = new List<string>();
		if (hash.ContainsKey("WildItemCommon")) this.WildItemCommon = hash["WildItemCommon"].Split(',').Select(m => m.Trim()).ToList();
		else this.WildItemCommon = new List<string>();
		if (hash.ContainsKey("WildItemUncommon")) this.WildItemUncommon = hash["WildItemUncommon"].Split(',').Select(m => m.Trim()).ToList();
		else this.WildItemUncommon = new List<string>();
		if (hash.ContainsKey("WildItemRare")) this.WildItemRare = hash["WildItemRare"].Split(',').Select(m => m.Trim()).ToList();
		else this.WildItemRare = new List<string>();
        this.EggGroups = hash["EggGroups"].Split(',').Select(m => EggGroupStrToEnum(m)).ToList();
        this.HatchSteps = Convert.ToInt32(hash["HatchSteps"]);
        if (hash.ContainsKey("Incense")) this.Incense = hash["Incense"];
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
				EvolutionType method = Evolution.MethodStrToEnum(_evos[i + 1]);
				string param = _evos[i + 2];
				Evolution evo = new Evolution((SpeciesResolver) species, method, string.IsNullOrEmpty(param) ? new List<object>() : new List<object>() { param }, false);
				this.Evolutions.Add(evo);
				PrevolutionsToRegister.Add((this, evo));
			}
		}
        this.Height = (float) Convert.ToDouble(hash["Height"]);
        this.Weight = (float) Convert.ToDouble(hash["Weight"]);
		this.Color = ColorStrToEnum(hash["Color"]);
		this.Shape = ShapeStrToEnum(hash["Shape"]);
		if (hash.ContainsKey("Habitat")) this.Habitat = HabitatStrToEnum(hash["Habitat"]);
		this.Generation = Convert.ToInt32(hash["Generation"]);
		if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').ToList();
		else this.Flags = new List<string>();
		if (hash.ContainsKey("MegaStone")) this.MegaStone = hash["MegaStone"];
		if (hash.ContainsKey("MegaMove")) this.MegaMove = hash["MegaMove"];
		if (hash.ContainsKey("UnmegaForm")) this.UnmegaForm = Convert.ToInt32(hash["UnmegaForm"]);
		if (hash.ContainsKey("MegaMessage")) this.MegaMessage = Convert.ToInt32(hash["MegaMessage"]);
	}

	private GrowthRate GrowthRateStrToEnum(string growth)
	{
		return growth switch
		{
			"Medium" => GrowthRate.Medium,
			"Fast" => GrowthRate.Fast,
			"Parabolic" => GrowthRate.Parabolic,
			"Slow" => GrowthRate.Slow,
			"Erratic" => GrowthRate.Erratic,
			"Fluctuating" => GrowthRate.Fluctuating,
			_ => throw new Exception($"Invalid growth rate '{growth}'.")
		};
	}

	private GenderRatio GenderRatioStrToEnum(string ratio)
	{
		return ratio switch
        {
            "AlwaysMale" => GenderRatio.AlwaysMale,
            "AlwaysFemale" => GenderRatio.AlwaysFemale,
            "Genderless" => GenderRatio.Genderless,
            "FemaleOneEighth" => GenderRatio.FemaleOneEighth,
            "Female25Percent" => GenderRatio.Female25Percent,
            "Female50Percent" => GenderRatio.Female50Percent,
            "Female75Percent" => GenderRatio.Female75Percent,
            "FemaleSevenEighths" => GenderRatio.FemaleSevenEighths,
            _ => throw new Exception($"Invalid gender ratio '{ratio}'.")
        };
    }

	private EggGroup EggGroupStrToEnum(string group)
	{
		return group switch
		{
			"Undiscovered" => EggGroup.Undiscovered,
			"Monster" => EggGroup.Monster,
			"Water1" => EggGroup.Water1,
			"Bug" => EggGroup.Bug,
			"Flying" => EggGroup.Flying,
			"Field" => EggGroup.Field,
			"Fairy" => EggGroup.Fairy,
			"Grass" => EggGroup.Grass,
			"Humanlike" => EggGroup.HumanLike,
			"Water3" => EggGroup.Water3,
			"Mineral" => EggGroup.Mineral,
			"Amorphous" => EggGroup.Amorphous,
			"Water2" => EggGroup.Water2,
			"Ditto" => EggGroup.Ditto,
			"Dragon" => EggGroup.Dragon,
			_ => throw new Exception($"Invalid egg group '{group}'.")
		};
    }

	private BodyColor ColorStrToEnum(string color)
	{
		return color switch
        {
            "Red" => BodyColor.Red,
            "Blue" => BodyColor.Blue,
            "Yellow" => BodyColor.Yellow,
            "Green" => BodyColor.Green,
            "Black" => BodyColor.Black,
            "Brown" => BodyColor.Brown,
            "Purple" => BodyColor.Purple,
            "Gray" => BodyColor.Gray,
            "White" => BodyColor.White,
            "Pink" => BodyColor.Pink,
            _ => throw new Exception($"Invalid body color '{color}'.")
        };
    }

	private BodyShape ShapeStrToEnum(string shape)
	{
		return shape switch
        {
            "Head" => BodyShape.Head,
            "Serpentine" => BodyShape.Serpentine,
            "Finned" => BodyShape.Finned,
            "HeadArms" => BodyShape.HeadAndArms,
            "HeadBase" => BodyShape.HeadAndBase,
            "BipedalTail" => BodyShape.BipedalWithTail,
            "HeadLegs" => BodyShape.HeadAndLegs,
            "Quadruped" => BodyShape.Quadruped,
            "Winged" => BodyShape.Winged,
            "Multiped" => BodyShape.Multiped,
            "MultiBody" => BodyShape.MultiBody,
            "Bipedal" => BodyShape.Bipedal,
            "MultiWinged" => BodyShape.MultiWinged,
            "Insectoid" => BodyShape.Insectoid,
            _ => throw new Exception($"Invalid body shape '{shape}'.")
        };
    }

    private Habitat HabitatStrToEnum(string habitat)
    {
        return habitat switch
        {
            "None" => Habitat.None,
            "Grassland" => Habitat.Grassland,
            "Forest" => Habitat.Forest,
            "WatersEdge" => Habitat.WatersEdge,
            "Sea" => Habitat.Sea,
            "Cave" => Habitat.Cave,
            "Mountain" => Habitat.Mountain,
            "RoughTerrain" => Habitat.RoughTerrain,
            "Urban" => Habitat.Urban,
            "Rare" => Habitat.Rare,
            _ => throw new Exception($"Invalid habitat '{habitat}'.")
        };
    }

	public Species(nint Data)
	{
		this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
		this.SpeciesName = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@species"));
		this.Form = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@form"));
		this.RealName = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
		this.RealFormName = Ruby.GetIVar(Data, "@real_form_name") == Ruby.Nil ? null : Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_form_name"));
		this.RealCategory = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_category"));
		this.RealPokedexEntry = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_pokedex_entry"));
		this.PokedexForm = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@pokedex_form"));
		nint TypeArray = Ruby.GetIVar(Data, "@types");
		if (Ruby.Array.Length(TypeArray) == 2) Type2 = Ruby.Symbol.FromPtr(Ruby.Array.Get(TypeArray, 1));
		Type1 = Ruby.Symbol.FromPtr(Ruby.Array.Get(TypeArray, 0));
		this.BaseStats = new Stats(Ruby.GetIVar(Data, "@base_stats"));
		this.EVs = new Stats(Ruby.GetIVar(Data, "@evs"));
		this.BaseEXP = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@base_exp"));
		string rgrowth = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@growth_rate"));
		this.GrowthRate = GrowthRateStrToEnum(rgrowth);
		string rratio = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@gender_ratio"));
		this.GenderRatio = GenderRatioStrToEnum(rratio);
		this.CatchRate = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@catch_rate"));
		this.Happiness = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@happiness"));
		nint MoveArray = Ruby.GetIVar(Data, "@moves");
		int MoveArrayLength = (int) Ruby.Array.Length(MoveArray);
		this.Moves = new List<(int, string)>();
		for (int i = 0; i < MoveArrayLength; i++)
		{
			nint robj = Ruby.Array.Get(MoveArray, i);
			int level = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(robj, 0));
			string move = Ruby.Symbol.FromPtr(Ruby.Array.Get(robj, 1));
			this.Moves.Add((level, move));
		}
		nint TutorMoveArray = Ruby.GetIVar(Data, "@tutor_moves");
		int TutorMoveArrayLength = (int) Ruby.Array.Length(TutorMoveArray);
		this.TutorMoves = new List<string>();
		for (int i = 0; i < TutorMoveArrayLength; i++)
		{
			string move = Ruby.Symbol.FromPtr(Ruby.Array.Get(TutorMoveArray, i));
			this.TutorMoves.Add(move);
		}
		nint EggMoveArray = Ruby.GetIVar(Data, "@egg_moves");
		int EggMoveArrayLength = (int) Ruby.Array.Length(EggMoveArray);
		this.EggMoves = new List<string>();
		for (int i = 0; i < EggMoveArrayLength; i++)
		{
			string move = Ruby.Symbol.FromPtr(Ruby.Array.Get(EggMoveArray, i));
			this.EggMoves.Add(move);
		}
		nint AbilityArray = Ruby.GetIVar(Data, "@abilities");
		int AbilityArrayLength = (int) Ruby.Array.Length(AbilityArray);
		this.Abilities = new List<string>();
		for (int i = 0; i < AbilityArrayLength; i++)
		{
			string ability = Ruby.Symbol.FromPtr(Ruby.Array.Get(AbilityArray, i));
			this.Abilities.Add(ability);
		}
		nint HiddenAbilityArray = Ruby.GetIVar(Data, "@hidden_abilities");
		int HiddenAbilityArrayLength = (int) Ruby.Array.Length(HiddenAbilityArray);
		this.HiddenAbilities = new List<string>();
		for (int i = 0; i < HiddenAbilityArrayLength; i++)
		{
			string ability = Ruby.Symbol.FromPtr(Ruby.Array.Get(HiddenAbilityArray, i));
			this.HiddenAbilities.Add(ability);
		}
        nint WildItemCommonArray = Ruby.GetIVar(Data, "@wild_item_common");
        int WildItemCommonArrayLength = (int) Ruby.Array.Length(WildItemCommonArray);
        this.WildItemCommon = new List<string>();
        for (int i = 0; i < WildItemCommonArrayLength; i++)
        {
            string item = Ruby.Symbol.FromPtr(Ruby.Array.Get(WildItemCommonArray, i));
			this.WildItemCommon.Add(item);
        }
		nint WildItemUncommonArray = Ruby.GetIVar(Data, "@wild_item_uncommon");
        int WildItemUncommonArrayLength = (int) Ruby.Array.Length(WildItemUncommonArray);
        this.WildItemUncommon = new List<string>();
        for (int i = 0; i < WildItemUncommonArrayLength; i++)
        {
            string item = Ruby.Symbol.FromPtr(Ruby.Array.Get(WildItemUncommonArray, i));
			this.WildItemUncommon.Add(item);
        }
		nint WildItemRareArray = Ruby.GetIVar(Data, "@wild_item_rare");
        int WildItemRareArrayLength = (int) Ruby.Array.Length(WildItemRareArray);
        this.WildItemRare = new List<string>();
        for (int i = 0; i < WildItemRareArrayLength; i++)
        {
            string item = Ruby.Symbol.FromPtr(Ruby.Array.Get(WildItemRareArray, i));
			this.WildItemRare.Add(item);
        }
        nint EggGroupsArray = Ruby.GetIVar(Data, "@egg_groups");
        int EggGroupsArrayLength = (int) Ruby.Array.Length(EggGroupsArray);
        this.EggGroups = new List<EggGroup>();
        for (int i = 0; i < EggGroupsArrayLength; i++)
        {
            string egggroup = Ruby.Symbol.FromPtr(Ruby.Array.Get(EggGroupsArray, i));
			this.EggGroups.Add(EggGroupStrToEnum(egggroup));
        }
		this.HatchSteps = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@hatch_steps"));
		this.Incense = Ruby.GetIVar(Data, "@incense") == Ruby.Nil ? null : Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@incense"));
		nint OffspringArray = Ruby.GetIVar(Data, "@offspring");
		int OffspringArraylength = (int) Ruby.Array.Length(OffspringArray);
		this.Offspring = new List<SpeciesResolver>();
		for (int i = 0; i < OffspringArraylength; i++)
		{
			string species = Ruby.Symbol.FromPtr(Ruby.Array.Get(OffspringArray, i));
			this.Offspring.Add((SpeciesResolver) species);
		}
		this.Height = Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@height")) / 10f;
		this.Weight = Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@weight")) / 10f;
		string rcolor = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@color"));
		this.Color = ColorStrToEnum(rcolor);
		string rshape = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@shape"));
		this.Shape = ShapeStrToEnum(rshape);
		string rhabitat = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@habitat"));
        this.Habitat = HabitatStrToEnum(rhabitat);
		this.Generation = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@generation"));
		nint FlagsArray = Ruby.GetIVar(Data, "@flags");
		int FlagsArrayLength = (int) Ruby.Array.Length(FlagsArray);
		this.Flags = new List<string>();
		for (int i = 0; i < FlagsArrayLength; i++)
		{
			string flag = Ruby.String.FromPtr(Ruby.Array.Get(FlagsArray, i));
			this.Flags.Add(flag);
		}
		this.MegaStone = Ruby.GetIVar(Data, "@mega_stone") == Ruby.Nil ? null : Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@mega_stone"));
        this.MegaMove = Ruby.GetIVar(Data, "@mega_move") == Ruby.Nil ? null : Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@mega_move"));
		this.UnmegaForm = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@unmega_form"));
		this.MegaMessage = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@mega_message"));
		nint EvolutionsArray = Ruby.GetIVar(Data, "@evolutions");
		int EvolutionsArrayLength = (int) Ruby.Array.Length(EvolutionsArray);
		this.Evolutions = new List<Evolution>();
		this.Prevolutions = new List<Evolution>();
		for (int i = 0; i < EvolutionsArrayLength; i++)
		{
			Evolution evo = new Evolution(Ruby.Array.Get(EvolutionsArray, i));
			if (evo.Prevolution) this.Prevolutions.Add(evo);
            else this.Evolutions.Add(evo);
		}
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
		Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
		Ruby.SetIVar(e, "@species", Ruby.Symbol.ToPtr(this.SpeciesName));
		Ruby.SetIVar(e, "@form", Ruby.Integer.ToPtr(this.Form));
		Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.RealName));
		Ruby.SetIVar(e, "@real_form_name", this.RealFormName == null ? Ruby.Nil : Ruby.String.ToPtr(this.RealFormName));
		Ruby.SetIVar(e, "@real_category", Ruby.String.ToPtr(this.RealCategory));
		Ruby.SetIVar(e, "@real_pokedex_entry", Ruby.String.ToPtr(this.RealPokedexEntry));
		Ruby.SetIVar(e, "@pokedex_form", Ruby.Integer.ToPtr(this.PokedexForm));
		nint TypeArray = Ruby.Array.Create();
		Ruby.Array.Push(TypeArray, Ruby.Symbol.ToPtr(Type1));
		if (Type2 != null) Ruby.Array.Push(TypeArray, Ruby.Symbol.ToPtr(Type2));
		Ruby.SetIVar(e, "@types", TypeArray);
		Ruby.SetIVar(e, "@base_stats", this.BaseStats.Save());
		Ruby.SetIVar(e, "@evs", this.EVs.Save());
		Ruby.SetIVar(e, "@base_exp", Ruby.Integer.ToPtr(this.BaseEXP));
        string rgrowth = this.GrowthRate switch
        {
            GrowthRate.Medium => "Medium",
            GrowthRate.Fast => "Fast",
            GrowthRate.Parabolic => "Parabolic",
            GrowthRate.Slow => "Slow",
            GrowthRate.Erratic => "Erratic",
            GrowthRate.Fluctuating => "Fluctuating",
            _ => throw new Exception($"Invalid growth rate '{this.GrowthRate}'.")
        };
		Ruby.SetIVar(e, "@growth_rate", Ruby.Symbol.ToPtr(rgrowth));
        string rratio = this.GenderRatio switch
        {
            GenderRatio.AlwaysMale => "AlwaysMale",
            GenderRatio.AlwaysFemale => "AlwaysFemale",
            GenderRatio.Genderless => "Genderless",
            GenderRatio.FemaleOneEighth => "FemaleOneEighth",
            GenderRatio.Female25Percent => "Female25Percent",
            GenderRatio.Female50Percent => "Female50Percent",
            GenderRatio.Female75Percent => "Female75Percent",
            GenderRatio.FemaleSevenEighths => "FemaleSevenEighths",
            _ => throw new Exception($"Invalid gender ratio '{this.GenderRatio}'.")
        };
		Ruby.SetIVar(e, "@gender_ratio", Ruby.Symbol.ToPtr(rratio));
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
        foreach (EggGroup group in EggGroups)
        {
			string rgroup = group switch
			{
				EggGroup.Undiscovered => "Undiscovered",
				EggGroup.Monster => "Monster",
				EggGroup.Water1 => "Water1",
				EggGroup.Bug => "Bug",
				EggGroup.Flying => "Flying",
				EggGroup.Field => "Field",
				EggGroup.Fairy => "Fairy",
				EggGroup.Grass => "Grass",
				EggGroup.HumanLike => "Humanlike",
				EggGroup.Water3 => "Water3",
				EggGroup.Mineral => "Mineral",
				EggGroup.Amorphous => "Amorphous",
				EggGroup.Water2 => "Water2",
				EggGroup.Ditto => "Ditto",
				EggGroup.Dragon => "Dragon",
				_ => throw new Exception($"Invalid egg group '{group}'.")
			};
            Ruby.Array.Push(EggGroupsArray, Ruby.Symbol.ToPtr(rgroup));
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
        string rcolor = this.Color switch
        {
            BodyColor.Red => "Red",
            BodyColor.Blue => "Blue",
            BodyColor.Yellow => "Yellow",
            BodyColor.Green => "Green",
            BodyColor.Black => "Black",
            BodyColor.Brown => "Brown",
            BodyColor.Purple => "Purple",
            BodyColor.Gray => "Gray",
            BodyColor.White => "White",
            BodyColor.Pink => "Pink",
            _ => throw new Exception($"Invalid body color '{this.Color}'.")
        };
		Ruby.SetIVar(e, "@color", Ruby.Symbol.ToPtr(rcolor));
        string rshape = this.Shape switch
        {
            BodyShape.Head => "Head",
            BodyShape.Serpentine => "Serpentine",
            BodyShape.Finned => "Finned",
            BodyShape.HeadAndArms => "HeadArms",
            BodyShape.HeadAndBase => "HeadBase",
            BodyShape.BipedalWithTail => "BipedalTail",
            BodyShape.HeadAndLegs => "HeadLegs",
            BodyShape.Quadruped => "Quadruped",
            BodyShape.Winged => "Winged",
            BodyShape.Multiped => "Multiped",
            BodyShape.MultiBody => "MultiBody",
            BodyShape.Bipedal => "Bipedal",
            BodyShape.MultiWinged => "MultiWinged",
            BodyShape.Insectoid => "Insectoid",
            _ => throw new Exception($"Invalid body shape '{this.Shape}'.")
        };
		Ruby.SetIVar(e, "@shape", Ruby.Symbol.ToPtr(rshape));
        string rhabitat = this.Habitat switch
        {
            Habitat.None => "None",
            Habitat.Grassland => "Grassland",
            Habitat.Forest => "Forest",
            Habitat.WatersEdge => "WatersEdge",
            Habitat.Sea => "Sea",
            Habitat.Cave => "Cave",
            Habitat.Mountain => "Mountain",
            Habitat.RoughTerrain => "RoughTerrain",
            Habitat.Urban => "Urban",
            Habitat.Rare => "Rare",
            _ => throw new Exception($"Invalid habitat '{this.Habitat}'.")
        };
		Ruby.SetIVar(e, "@habitat", Ruby.Symbol.ToPtr(rhabitat));
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
}

[DebuggerDisplay("{ID}")]
public class SpeciesResolver
{
	private string _id;
	public string ID { get => _id; set { _id = value; _species = null; } }
	private Species _species;
	public Species Species
	{
		get
		{
			if (_species != null) return _species;
			_species = Data.Species[ID];
			return _species;
		}
	}

	public SpeciesResolver(string ID)
	{
		this.ID = ID;
	}

	public SpeciesResolver(Species Species)
	{
		this.ID = Species.ID;
		_species = Species;
	}

	public static implicit operator string(SpeciesResolver s) => s.ID; // string x = speciesResolver
	public static implicit operator Species(SpeciesResolver s) => s.Species; // Species x = speciesResolver
	public static explicit operator SpeciesResolver(Species s) => new SpeciesResolver(s); // SpeciesResolver x = (SpeciesResolver) species
	public static explicit operator SpeciesResolver(string ID) => new SpeciesResolver(ID); // SpeciesResolver x = (SpeciesResolver) str
}
