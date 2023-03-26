using System;
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
    [JsonPropertyName("habitats")]
    public List<string> Habitats;
    [JsonPropertyName("growth_rates")]
    public List<string> GrowthRates;
    [JsonPropertyName("gender_ratios")]
    public List<string> GenderRatios;
    [JsonPropertyName("evolution_methods")]
    public List<string> EvolutionMethods;
    [JsonPropertyName("move_categories")]
    public List<string> MoveCategories;
    [JsonPropertyName("move_targets")]
    public List<string> MoveTargets;
    [JsonPropertyName("natures")]
    public List<string> Natures;
    [JsonPropertyName("weathers")]
    public List<string> Weathers;
    [JsonPropertyName("item_pockets")]
    public List<string> ItemPockets;
    [JsonPropertyName("item_field_uses")]
    public List<string> ItemFieldUses;
    [JsonPropertyName("item_battle_uses")]
    public List<string> ItemBattleUses;
    [JsonPropertyName("body_colors")]
    public List<string> BodyColors;
    [JsonPropertyName("body_shapes")]
    public List<string> BodyShapes;
    [JsonPropertyName("egg_groups")]
    public List<string> EggGroups;
    [JsonPropertyName("encounter_types")]
    public List<string> EncounterTypes;

    public static HardcodedDataStore Create(string fileName)
    {
        Logger.WriteLine("Loading hardcoded data JSON from '{0}'", fileName);
        string dataText = File.ReadAllText(fileName);
        var dataStore = JsonSerializer.Deserialize<HardcodedDataStore>(dataText, new JsonSerializerOptions() { AllowTrailingCommas = true, IncludeFields = true });
        dataStore.Validate();
        return dataStore;
    }

    private void Validate()
    {
        if (Habitats == null || Habitats.Count == 0) throw new HardcodedDataException("habitats");
        if (GrowthRates == null || GrowthRates.Count == 0) throw new HardcodedDataException("growth_rates");
        if (GenderRatios == null || GenderRatios.Count == 0) throw new HardcodedDataException("gender_ratios");
        if (EvolutionMethods == null || EvolutionMethods.Count == 0) throw new HardcodedDataException("evolution_methods");
        if (MoveCategories == null || MoveCategories.Count == 0) throw new HardcodedDataException("move_categories");
        if (MoveTargets == null || MoveTargets.Count == 0) throw new HardcodedDataException("move_targets");
        if (Natures == null || Natures.Count == 0) throw new HardcodedDataException("natures");
        if (Weathers == null || Weathers.Count == 0) throw new HardcodedDataException("weathers");
        if (ItemPockets == null || ItemPockets.Count == 0) throw new HardcodedDataException("item_pockets");
        if (ItemFieldUses == null || ItemFieldUses.Count == 0) throw new HardcodedDataException("item_field_uses");
        if (ItemBattleUses == null || ItemBattleUses.Count == 0) throw new HardcodedDataException("item_battle_uses");
        if (BodyColors == null || BodyColors.Count == 0) throw new HardcodedDataException("body_colors");
        if (BodyShapes == null || BodyShapes.Count == 0) throw new HardcodedDataException("body_shapes");
        if (EggGroups == null || EggGroups.Count == 0) throw new HardcodedDataException("egg_groups");
        if (EncounterTypes == null || EncounterTypes.Count == 0) throw new HardcodedDataException("encounter_types");
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