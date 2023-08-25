using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RPGStudioMK.Game;

public class HardcodedDataStore
{
    public required List<string> Habitats { get; init; }
    public required List<string> GrowthRates { get; init; }
    public required List<string> GenderRatios { get; init; }
    public required List<List<string>> EvolutionMethodsAndTypes { get; init; }
    public required List<string> MoveCategories { get; init; }
    public required List<string> MoveTargets { get; init; }
    public required List<string> Natures { get; init; }
    public required List<string> Weathers { get; init; }
    public required List<string> ItemPockets { get; init; }
    public required List<string> ItemFieldUses { get; init; }
    public required List<string> ItemBattleUses { get; init; }
    public required List<string> BodyColors { get; init; }
    public required List<string> BodyShapes { get; init; }
    public required List<string> EggGroups { get; init; }
    public required List<string> EncounterTypes { get; init; }

    public List<string> EvolutionMethods;

    public List<ListItem> HabitatsListItems;
	public List<ListItem> GrowthRatesListItems;
	public List<ListItem> GenderRatiosListItems;
	public List<ListItem> EvolutionMethodsListItems;
	public List<ListItem> MoveCategoriesListItems;
	public List<ListItem> MoveTargetsListItems;
	public List<ListItem> NaturesListItems;
	public List<ListItem> WeathersListItems;
	public List<ListItem> ItemPocketsListItems;
	public List<ListItem> ItemFieldUsesListItems;
	public List<ListItem> ItemBattleUsesListItems;
	public List<ListItem> BodyColorsListItems;
	public List<ListItem> BodyShapesListItems;
	public List<ListItem> EggGroupsListItems;
	public List<ListItem> EncounterTypesListItems;

	public static HardcodedDataStore Create(string fileName)
    {
        Logger.WriteLine("Loading hardcoded data JSON from '{0}'", fileName);
        string dataText = File.ReadAllText(fileName);
        var rawData = JsonSerializer.Deserialize<Dictionary<string, object>>(dataText, new JsonSerializerOptions() { AllowTrailingCommas = true });
        PreValidate(rawData);
		var dataStore = new HardcodedDataStore()
        {
            Habitats = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["habitats"]),
            GrowthRates = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["growth_rates"]),
            GenderRatios = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["gender_ratios"]),
            EvolutionMethodsAndTypes = JsonSerializer.Deserialize<List<List<string>>>((JsonElement) rawData["evolution_methods"]),
            MoveCategories = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["move_categories"]),
            MoveTargets = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["move_targets"]),
            Natures = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["natures"]),
            Weathers = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["weathers"]),
            ItemPockets = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["item_pockets"]),
            ItemFieldUses = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["item_field_uses"]),
            ItemBattleUses = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["item_battle_uses"]),
            BodyColors = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["body_colors"]),
            BodyShapes = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["body_shapes"]),
            EggGroups = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["egg_groups"]),
            EncounterTypes = JsonSerializer.Deserialize<List<string>>((JsonElement) rawData["encounter_types"]),
	    };
		PostValidate(dataStore);
        dataStore.EvolutionMethods = dataStore.EvolutionMethodsAndTypes.Select(list => list[0]).ToList();
		dataStore.HabitatsListItems = dataStore.Habitats.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.GrowthRatesListItems = dataStore.GrowthRates.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.GenderRatiosListItems = dataStore.GenderRatios.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.EvolutionMethodsListItems = dataStore.EvolutionMethods.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.MoveCategoriesListItems = dataStore.MoveCategories.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.MoveTargetsListItems = dataStore.MoveTargets.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.NaturesListItems = dataStore.Natures.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.WeathersListItems = dataStore.Weathers.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.ItemPocketsListItems = dataStore.ItemPockets.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.ItemFieldUsesListItems = dataStore.ItemFieldUses.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.ItemBattleUsesListItems = dataStore.ItemBattleUses.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.BodyColorsListItems = dataStore.BodyColors.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.BodyShapesListItems = dataStore.BodyShapes.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
        dataStore.EggGroupsListItems = dataStore.EggGroups.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();
		dataStore.EncounterTypesListItems = dataStore.EncounterTypes.Select(item => new ListItem(item)).OrderBy(item => item.Name).ToList();

		return dataStore;
    }

    private static void PreValidate(Dictionary<string, object> data)
    {
        if (!data.ContainsKey("habitats")) throw new HardcodedDataException("habitats");
        if (!data.ContainsKey("growth_rates")) throw new HardcodedDataException("growth_rates");
        if (!data.ContainsKey("gender_ratios")) throw new HardcodedDataException("gender_ratios");
        if (!data.ContainsKey("evolution_methods")) throw new HardcodedDataException("evolution_methods");
        if (!data.ContainsKey("move_categories")) throw new HardcodedDataException("move_categories");
        if (!data.ContainsKey("move_targets")) throw new HardcodedDataException("move_targets");
        if (!data.ContainsKey("natures")) throw new HardcodedDataException("natures");
        if (!data.ContainsKey("weathers")) throw new HardcodedDataException("weathers");
        if (!data.ContainsKey("item_pockets")) throw new HardcodedDataException("item_pockets");
        if (!data.ContainsKey("item_field_uses")) throw new HardcodedDataException("item_field_uses");
        if (!data.ContainsKey("item_battle_uses")) throw new HardcodedDataException("item_battle_uses");
        if (!data.ContainsKey("body_colors")) throw new HardcodedDataException("body_colors");
        if (!data.ContainsKey("body_shapes")) throw new HardcodedDataException("body_shapes");
        if (!data.ContainsKey("egg_groups")) throw new HardcodedDataException("egg_groups");
        if (!data.ContainsKey("encounter_types")) throw new HardcodedDataException("encounter_types");
    }

	private static void PostValidate(HardcodedDataStore dataStore)
	{
		if (dataStore.Habitats.Count == 0) throw new HardcodedDataException("habitats");
		if (dataStore.GrowthRates.Count == 0) throw new HardcodedDataException("growth_rates");
		if (dataStore.GenderRatios.Count == 0) throw new HardcodedDataException("gender_ratios");
		if (dataStore.EvolutionMethodsAndTypes.Count == 0) throw new HardcodedDataException("evolution_methods");
		if (dataStore.MoveCategories.Count == 0) throw new HardcodedDataException("move_categories");
		if (dataStore.MoveTargets.Count == 0) throw new HardcodedDataException("move_targets");
		if (dataStore.Natures.Count == 0) throw new HardcodedDataException("natures");
		if (dataStore.Weathers.Count == 0) throw new HardcodedDataException("weathers");
		if (dataStore.ItemPockets.Count == 0) throw new HardcodedDataException("item_pockets");
		if (dataStore.ItemFieldUses.Count == 0) throw new HardcodedDataException("item_field_uses");
		if (dataStore.ItemBattleUses.Count == 0) throw new HardcodedDataException("item_battle_uses");
		if (dataStore.BodyColors.Count == 0) throw new HardcodedDataException("body_colors");
		if (dataStore.BodyShapes.Count == 0) throw new HardcodedDataException("body_shapes");
		if (dataStore.EggGroups.Count == 0) throw new HardcodedDataException("egg_groups");
		if (dataStore.EncounterTypes.Count == 0) throw new HardcodedDataException("encounter_types");
	}

	public string Get(int index, List<string> dataStore)
    {
        if (index >= dataStore.Count) throw new Exception($"Invalid value '{index}'. It must be between 0 and {dataStore.Count - 1}.");
        return dataStore[index];
    }

    public string? TryGet(int index, List<string> dataStore)
    {
        return index < dataStore.Count ? dataStore[index] : null;
    }

    public bool IsValid(string value, List<string> dataStore)
    {
        return dataStore.Contains(value);
    }

    public string Assert(string value, List<string> dataStore)
    {
        if (!IsValid(value, dataStore))
        {
            string ary = "[";
            for (int i = 0; i < dataStore.Count; i++)
            {
                ary += dataStore[i];
                if (i < dataStore.Count - 1) ary += ", ";
            }
            ary += "]";
            throw new Exception($"The value '{value}' is invalid. It must be one of {ary}.");
        }
        return value;
    }

    public int AssertIndex(string value, List<string> dataStore)
    {
        Assert(value, dataStore);
        return dataStore.IndexOf(value);
    }
}

public class HardcodedDataException : Exception
{
    public HardcodedDataException(string dataType) : base($"Something went wrong trying to read the '{dataType}' key from hardcoded_data.json. Make sure it exists and is non-empty.")
    {
        
    }
}