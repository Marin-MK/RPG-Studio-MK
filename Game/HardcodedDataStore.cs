﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class HardcodedDataStore
{
    public required List<string> Habitats { get; init; }
    public required List<string> GrowthRates { get; init; }
    public required List<string> GenderRatios { get; init; }
    public required List<string> EvolutionMethods { get; init; }
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

    public static HardcodedDataStore Create(string fileName)
    {
        Logger.WriteLine("Loading hardcoded data JSON from '{0}'", fileName);
        string dataText = File.ReadAllText(fileName);
        var rawData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(dataText, new JsonSerializerOptions() { AllowTrailingCommas = true });
        Validate(rawData);
        var dataStore = new HardcodedDataStore()
        {
            Habitats = rawData["habitats"],
            GrowthRates = rawData["growth_rates"],
            GenderRatios = rawData["gender_ratios"],
            EvolutionMethods = rawData["evolution_methods"],
            MoveCategories = rawData["move_categories"],
            MoveTargets = rawData["move_targets"],
            Natures = rawData["natures"],
            Weathers = rawData["weathers"],
            ItemPockets = rawData["item_pockets"],
            ItemFieldUses = rawData["item_field_uses"],
            ItemBattleUses = rawData["item_battle_uses"],
            BodyColors = rawData["body_colors"],
            BodyShapes = rawData["body_shapes"],
            EggGroups = rawData["egg_groups"],
            EncounterTypes = rawData["encounter_types"]
        };
        return dataStore;
    }

    private static void Validate(Dictionary<string, List<string>> data)
    {
        if (!data.ContainsKey("habitats") || data["habitats"].Count == 0) throw new HardcodedDataException("habitats");
        if (!data.ContainsKey("growth_rates") || data["growth_rates"].Count == 0) throw new HardcodedDataException("growth_rates");
        if (!data.ContainsKey("gender_ratios") || data["gender_ratios"].Count == 0) throw new HardcodedDataException("gender_ratios");
        if (!data.ContainsKey("evolution_methods") || data["evolution_methods"].Count == 0) throw new HardcodedDataException("evolution_methods");
        if (!data.ContainsKey("move_categories") || data["move_categories"].Count == 0) throw new HardcodedDataException("move_categories");
        if (!data.ContainsKey("move_targets") || data["move_targets"].Count == 0) throw new HardcodedDataException("move_targets");
        if (!data.ContainsKey("natures") || data["natures"].Count == 0) throw new HardcodedDataException("natures");
        if (!data.ContainsKey("weathers") || data["weathers"].Count == 0) throw new HardcodedDataException("weathers");
        if (!data.ContainsKey("item_pockets") || data["item_pockets"].Count == 0) throw new HardcodedDataException("item_pockets");
        if (!data.ContainsKey("item_field_uses") || data["item_field_uses"].Count == 0) throw new HardcodedDataException("item_field_uses");
        if (!data.ContainsKey("item_battle_uses") || data["item_battle_uses"].Count == 0) throw new HardcodedDataException("item_battle_uses");
        if (!data.ContainsKey("body_colors") || data["body_colors"].Count == 0) throw new HardcodedDataException("body_colors");
        if (!data.ContainsKey("body_shapes") || data["body_shapes"].Count == 0) throw new HardcodedDataException("body_shapes");
        if (!data.ContainsKey("egg_groups") || data["egg_groups"].Count == 0) throw new HardcodedDataException("egg_groups");
        if (!data.ContainsKey("encounter_types") || data["encounter_types"].Count == 0) throw new HardcodedDataException("encounter_types");
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