using RPGStudioMK.Widgets;
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
    public required List<(string Method, string DataType)> EvolutionMethodsAndTypes { get; init; }
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

    public List<TreeNode> HabitatsListItems;
	public List<TreeNode> GrowthRatesListItems;
	public List<TreeNode> GenderRatiosListItems;
	public List<TreeNode> EvolutionMethodsListItems;
	public List<TreeNode> MoveCategoriesListItems;
	public List<TreeNode> MoveTargetsListItems;
	public List<TreeNode> NaturesListItems;
	public List<TreeNode> WeathersListItems;
	public List<TreeNode> ItemPocketsListItems;
	public List<TreeNode> ItemFieldUsesListItems;
	public List<TreeNode> ItemBattleUsesListItems;
	public List<TreeNode> BodyColorsListItems;
	public List<TreeNode> BodyShapesListItems;
	public List<TreeNode> EggGroupsListItems;
	public List<TreeNode> EncounterTypesListItems;

	public HardcodedDataStore Finalize()
    {
        this.EvolutionMethods = this.EvolutionMethodsAndTypes.Select(list => list.Item1).ToList();
		this.HabitatsListItems = this.Habitats.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.GrowthRatesListItems = this.GrowthRates.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.GenderRatiosListItems = this.GenderRatios.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.EvolutionMethodsListItems = this.EvolutionMethods.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.MoveCategoriesListItems = this.MoveCategories.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.MoveTargetsListItems = this.MoveTargets.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.NaturesListItems = this.Natures.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.WeathersListItems = this.Weathers.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.ItemPocketsListItems = this.ItemPockets.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.ItemFieldUsesListItems = this.ItemFieldUses.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.ItemBattleUsesListItems = this.ItemBattleUses.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.BodyColorsListItems = this.BodyColors.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.BodyShapesListItems = this.BodyShapes.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
        this.EggGroupsListItems = this.EggGroups.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
		this.EncounterTypesListItems = this.EncounterTypes.Select(item => new TreeNode(item)).OrderBy(item => item.Text).ToList();
        return this;
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
            throw new Exception($"The value '{value}' is invalid. It must be one of [{dataStore.Aggregate((a, b) => a + ", " + b)}].");
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