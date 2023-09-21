using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using RPGStudioMK.Game;
using static rubydotnet.Ruby;

namespace RPGStudioMK;

public static class ScriptDataExtractor
{
	public static HardcodedDataStore Run()
	{
		return new HardcodedDataStore()
		{
			Habitats = ExtractHabitats(),
			GrowthRates = ExtractGrowthRates(),
			GenderRatios = ExtractGenderRatios(),
			EvolutionMethodsAndTypes = ExtractEvolutionMethods(),
			MoveCategories = new List<string>() { "Physical", "Special", "Status" },
			MoveTargets = ExtractMoveTargets(),
			Natures = ExtractNatures(),
			Weathers = ExtractWeathers(),
			ItemPockets = new List<string>() { "OtherItems", "Medicine", "Balls", "TMs", "Berries", "Mail", "BattleItems", "KeyItems" },
			ItemFieldUses = new List<string>() { "None", "OnPokemon", "Direct", "TM", "HM", "TR" },
			ItemBattleUses = new List<string>() { "None", "OnPokemon", "OnMove", "OnBattler", "OnFoe", "Direct" },
			BodyColors = ExtractBodyColors(),
			BodyShapes = ExtractBodyShapes(),
			EggGroups = ExtractEggGroups(),
			EncounterTypes = ExtractEncounterTypes()
		}.Finalize();
	}

	private static List<Script> GetScripts(string mainScriptName)
	{
		List<Script> scriptsToSearch = new List<Script>();
		Data.Plugins.ForEach(x => scriptsToSearch.AddRange(x.Scripts));
		Script? mainScript = Data.Scripts.Find(s => s.Name == mainScriptName);
		if (mainScript is not null) scriptsToSearch.Add(mainScript);
		return scriptsToSearch;
	}

	private static string? MatchID(string line)
	{
		Match m = Regex.Match(line, " *?:id *?=> *?:([A-Za-z0-9_]+)(,|$)");
		if (!m.Success) m = Regex.Match(line, " *?id: *?:([A-Za-z0-9_]+)(,|$)");
		if (m.Success) return m.Groups[1].Value;
		return null;
	}

	private static string? MatchParameter(string line)
	{
		Match m = Regex.Match(line, " *?:parameter *?=> *?:{0,1}([A-Za-z0-9_]+)(,|$)");
		if (!m.Success) m = Regex.Match(line, " *?parameter: *?:{0,1}([A-Za-z0-9_]+)(,|$)");
		if (m.Success) return m.Groups[1].Value;
		return null;
	}

	private static List<string> ExtractData(string dataType)
	{
		List<string> data = new List<string>();
		foreach (Script script in GetScripts(dataType))
		{
			string[] lines = script.Content.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				if (!lines[i].Contains($"GameData::{dataType}.register({{")) continue;
				if (i + 1 >= lines.Length) continue;
				string? value = MatchID(lines[i + 1]);
				if (value is not null) data.Add(value);
			}
		}
		return data;
	}

	public static List<string> ExtractNatures() => ExtractData("Nature");

	public static List<string> ExtractHabitats() => ExtractData("Habitat");

	public static List<string> ExtractGrowthRates() => ExtractData("GrowthRate");
	
	public static List<string> ExtractGenderRatios() => ExtractData("GenderRatio");

	public static List<string> ExtractMoveTargets() => ExtractData("Target");

	public static List<string> ExtractWeathers() => ExtractData("Weather");

	public static List<string> ExtractBodyColors() => ExtractData("BodyColor");

	public static List<string> ExtractBodyShapes() => ExtractData("BodyShape");

	public static List<string> ExtractEggGroups() => ExtractData("EggGroup");

	public static List<string> ExtractEncounterTypes() => ExtractData("EncounterType");

	public static List<(string ID, string DataType)> ExtractEvolutionMethods()
	{
		List<(string, string)> data = new List<(string, string)>();
		foreach (Script script in GetScripts("Evolution"))
		{
			string[] lines = script.Content.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				if (!lines[i].Contains($"GameData::Evolution.register({{")) continue;
				if (i + 1 >= lines.Length) continue;
				string? id = MatchID(lines[i + 1]);
				string? type = MatchParameter(lines[i + 2]);
				if (id is not null) data.Add((id, type switch
				{
					null => "none",
					"Integer" => "number",
					"Item" => "item",
					"Move" => "move",
					"Species" => "species",
					"String" => "string",
					"Type" => "type",
					_ => "string"
				}));
			}
		}
		return data;
	}
}
