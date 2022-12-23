using amethyst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// May contain _X for different forms.
    /// </summary>
    public string ID;
	/// <summary>
	/// The raw species itself.
	/// </summary>
	public string SpeciesName;
	/// <summary>
	/// The form of the species.
	/// </summary>
	public int Form;
	public string RealName;
	public string? RealFormName;
	public string RealCategory;
	public string RealPokedexEntry;
	/// <summary>
	/// The form shown in the pokedex.
	/// </summary>
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

	public Species(nint Data)
	{
		/* :@id, :@species, :@form, :@real_name, :@real_form_name, :@real_category, :@real_pokedex_entry, :@pokedex_form, :@types,
		 * :@base_stats, :@evs, :@base_exp, :@growth_rate, :@gender_ratio, :@catch_rate, :@happiness, :@moves, :@tutor_moves,
		 * :@egg_moves, :@abilities, :@hidden_abilities, :@wild_item_common, :@wild_item_uncommon, :@wild_item_rare, :@egg_groups,
		 * :@hatch_steps, :@incense, :@offspring, :@evolutions, :@height, :@weight, :@color, :@shape, :@habitat, :@generation,
		 * :@flags, :@mega_stone, :@mega_move, :@unmega_form, :@mega_message
		 */
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
        this.GrowthRate = rgrowth switch
		{
			"Medium" => GrowthRate.Medium,
			"Fast" => GrowthRate.Fast,
			"Parabolic" => GrowthRate.Parabolic,
			"Slow" => GrowthRate.Slow,
			"Erratic" => GrowthRate.Erratic,
			"Fluctuating" => GrowthRate.Fluctuating,
			_ => throw new Exception($"Invalid growth rate '{rgrowth}'.")
		};
		string rratio = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@gender_ratio"));
        this.GenderRatio = rratio switch
		{
			"AlwaysMale" => GenderRatio.AlwaysMale,
			"AlwaysFemale" => GenderRatio.AlwaysFemale,
			"Genderless" => GenderRatio.Genderless,
			"FemaleOneEighth" => GenderRatio.FemaleOneEighth,
			"Female25Percent" => GenderRatio.Female25Percent,
			"Female50Percent" => GenderRatio.Female50Percent,
			"Female75Percent" => GenderRatio.Female75Percent,
			"FemaleSevenEighths" => GenderRatio.FemaleSevenEighths,
			_ => throw new Exception($"Invalid gender ratio '{rratio}'.")
		};
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
			this.EggGroups.Add(egggroup switch
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
				_ => throw new Exception($"Invalid egg group '{egggroup}'.")
            });
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
		if (Ruby.Is(Ruby.GetIVar(Data, "@height"), "Float"))
		{
			this.Height = (float) Ruby.Float.FromPtr(Ruby.GetIVar(Data, "@height"));
		}
		else this.Height = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@height"));
		if (Ruby.Is(Ruby.GetIVar(Data, "@weight"), "Float"))
		{
			this.Weight = (float) Ruby.Float.FromPtr(Ruby.GetIVar(Data, "@weight"));
		}
		else this.Weight = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@weight"));
		string rcolor = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@color"));
        this.Color = rcolor switch
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
			_ => throw new Exception($"Invalid body color '{rcolor}'.")
		};
		string rshape = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@shape"));
        this.Shape = rshape switch
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
			_ => throw new Exception($"Invalid body shape '{rshape}'.")
		};
		string rhabitat = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@habitat"));
        this.Habitat = rhabitat switch
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
			_ => throw new Exception($"Invalid habitat '{rhabitat}'.")
		};
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
}

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
