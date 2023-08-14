using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace RPGStudioMK.Game;

public static partial class Data
{
    // General Path Data
    public static string ProjectPath;
    public static string ProjectFilePath;
    public static string ProjectRMXPGamePath;
    public static string DataPath;

    // RMXP Data
    public static Dictionary<int, Map> Maps = new Dictionary<int, Map>();
    public static List<Tileset> Tilesets = new List<Tileset>();
    public static List<Autotile> Autotiles = new List<Autotile>();
    public static List<CommonEvent> CommonEvents = new List<CommonEvent>();
    public static List<Script> Scripts = new List<Script>();
    public static System System;
    public static Metadata Metadata;
    public static Dictionary<int, PlayerMetadata> PlayerMetadata = new Dictionary<int, PlayerMetadata>();
    // Essentials Data
    public static Dictionary<string, Species> Species = new Dictionary<string, Species>();
    public static Dictionary<string, Ability> Abilities = new Dictionary<string, Ability>();
    public static Dictionary<string, Move> Moves = new Dictionary<string, Move>();
    public static Dictionary<string, Type> Types = new Dictionary<string, Type>();
    public static Dictionary<string, Item> Items = new Dictionary<string, Item>();
    public static Dictionary<string, TrainerType> TrainerTypes = new Dictionary<string, TrainerType>();
    public static Dictionary<(int Map, int Version), EncounterTable> Encounters = new Dictionary<(int, int), EncounterTable>();
    public static List<Trainer> Trainers = new List<Trainer>();
    // Other Project Data & Settings
    public static HardcodedDataStore HardcodedData;
    private static HardcodedDataStore GlobalHardcodedData;
    public static List<GamePlugin> Plugins = new List<GamePlugin>();
    public static EssentialsVersion EssentialsVersion = EssentialsVersion.Unknown;
    public static bool UsesExternalScripts = false;

    // Used during loading process
    public static bool StopLoading;
    
    // The order and types of data that are loaded in this format.
    // There are few restrictions, other than:
    // - Tilesets must be loaded before maps
    // - Maps must be loaded before map metadata
    // - Metadata must be loaded before player metadata (this is not a strict requirement,
    //   but player metadata will discard data if metadata loads from PBS, so excessive data loading)
    static DataManager DataManager = new DataManager(new List<BaseDataManager>()
    {
        // RMXP Data
        new ScriptManager(),
        new GameConfigManager(),
        new TilesetManager(),
        new MapManager(),
        new CommonEventManager(),
        new SystemManager(),
        // Metadata, player metadata & map metadata
        new MainMetadataManager(),
        // Other Essentials Data
        new SpeciesManager(),
        new AbilityManager(),
        new ItemManager(),
        new MoveManager(),
        new TypeManager(),
        new TrainerTypeManager(),
        new TrainerManager(),
        new EncounterManager(),
    });
    static Action<float> OnProgressUpdated;
    static Action<string> OnLoadTextChanging;

    /// <summary>
    /// First-time setup of classes, data stores, etc.
    /// </summary>
    public static void Setup()
    {
        Logger.WriteLine("Creating RMXP classes");
        Compatibility.RMXP.Setup();
        Logger.WriteLine("Creating Data Managers");
        DataManager.Setup();
        if (!File.Exists("hardcoded_data.json")) throw new Exception("The global harcoded_data.json file is missing. The program can not continue.");
        HardcodedData = HardcodedDataStore.Create("hardcoded_data.json");
    }

    public static void ClearProjectData()
    {
        ProjectPath = null;
        ProjectFilePath = null;
        ProjectRMXPGamePath = null;
        DataPath = null;
        DataManager.Clear();
        Plugins.Clear();
        StopLoading = false;
        UsesExternalScripts = false;
        Sources.InvalidateAll();
        if (GlobalHardcodedData is not null)
        {
            // Switch the project-specific hardcoded data out for the global hardcoded data again
            HardcodedData = GlobalHardcodedData;
        }
    }

    public static void LoadGameData(Action<float> OnProgressUpdated, Action<string> OnLoadTextChanging)
    {   
        Data.OnProgressUpdated = OnProgressUpdated;
        Data.OnLoadTextChanging = OnLoadTextChanging;
        
        DataManager.Load(false);

        if (StopLoading) return;
        SetLoadText("Loading plugins...");
        PluginManager.LoadAll(true);

        SetLoadText("Loading project...");
        SetLoadProgress(1f);
    }

    public static void SaveGameData()
    {
        DataManager.Save();
    }

    public static void SetProjectPath(string OriginalProjectFilePath)
    {
        if (!OriginalProjectFilePath.EndsWith(".rxproj") && !OriginalProjectFilePath.EndsWith(".mkproj")) throw new Exception("Invalid project file path.");
        string path = Path.GetDirectoryName(OriginalProjectFilePath).Replace('\\', '/');
        ProjectPath = path;
        DataPath = path + "/Data";
        ProjectFilePath = path + "/project.mkproj";
        ProjectRMXPGamePath = path + "/Game.rxproj";
        if (File.Exists(ProjectPath + "/hardcoded_data.json"))
        {
            // Swap the global harcoded data out for project-specific hardcoded data
            GlobalHardcodedData = HardcodedData;
            HardcodedData = HardcodedDataStore.Create(ProjectPath + "/hardcoded_data.json");
        }
    }

    public static void SetLoadProgress(float Progress)
    {
        OnProgressUpdated?.Invoke(Progress);
    }

    public static void SetLoadText(string Text)
    {
        SetLoadProgress(0);
        OnLoadTextChanging?.Invoke(Text);
    }

    public static void AbortLoad()
    {
        Data.StopLoading = true;
    }

    public static bool IsVersionAtLeast(EssentialsVersion Version)
    {
        return EssentialsVersion >= Version;
    }

    public static bool IsPrimaryVersion(EssentialsVersion Version)
    {
        return EssentialsVersion switch
        {
            >= EssentialsVersion.v17 and <= EssentialsVersion.v17_2 => Version == EssentialsVersion.v17,
            EssentialsVersion.v18 or EssentialsVersion.v18_1 => Version == EssentialsVersion.v18,
            EssentialsVersion.v19 or EssentialsVersion.v19_1 => Version == EssentialsVersion.v19,
            EssentialsVersion.v20 or EssentialsVersion.v20_1 => Version == EssentialsVersion.v20,
            EssentialsVersion.v21 or EssentialsVersion.v21_1 => Version == EssentialsVersion.v21,
            _ => false
        };
    }

    public static class Sources
    {
        private static List<ListItem> _saflia;
		private static List<ListItem> _slia;
		private static List<ListItem> _alia;
        private static List<ListItem> _mlia;
        private static List<ListItem> _tlia;
        private static List<ListItem> _ilia;
        private static List<ListItem> _ttlia;
        private static List<Item> _tms;
        private static List<Item> _hms;

        public static List<ListItem> SpeciesAndFormsListItemsAlphabetical 
        {
            get 
            {
                if (recalculateSpeciesAndForms) _saflia = Species.Select(spc => new ListItem(spc.Value.Form != 0 ? $"{spc.Value.Name} ({spc.Value.FormName ?? spc.Value.Form.ToString()})" : spc.Value.Name, spc.Value)).OrderBy(item => item.Name).ToList();
                recalculateSpeciesAndForms = false;
                return _saflia;
            } 
        }
		public static List<ListItem> SpeciesListItemsAlphabetical
		{
			get
			{
                if (recalculateSpecies) _slia = Species.Where(kvp => kvp.Value.Form == 0).Select(spc => new ListItem(spc.Value.Name, spc.Value)).OrderBy(item => item.Name).ToList();
                recalculateSpecies = false;
				return _slia;
			}
		}
		public static List<ListItem> AbilitiesListItemsAlphabetical
        {
            get
            {
                if (recalculateAbilities) _alia = Abilities.Select(abil => new ListItem(abil.Value.Name, abil.Value)).OrderBy(item => item.Name).ToList();
                recalculateAbilities = false;
                return _alia;
			}
        }
        public static List<ListItem> MovesListItemsAlphabetical
        {
            get
            {
                if (recalculateMoves) _mlia = Moves.Select(move => new ListItem(move.Value.Name, move.Value)).OrderBy(item => item.Name).ToList();
                recalculateMoves = false;
                return _mlia;
			}
        }
        public static List<ListItem> TypesListItemsAlphabetical
        {
            get
            {
                if (recalculateTypes) _tlia = Types.Select(type => new ListItem(type.Value.Name, type.Value)).OrderBy(item => item.Name).ToList();
                recalculateTypes = false;
                return _tlia;
			}
        }
        public static List<ListItem> ItemsListItemsAlphabetical
        {
            get
            {
                if (recalculateItems) _ilia = Items.Select(item => new ListItem(item.Value.Name, item.Value)).OrderBy(item => item.Name).ToList();
                recalculateItems = false;
                return _ilia;
			}
        }
        public static List<ListItem> TrainerTypesListItemsAlphabetical
        {
            get
            {
                if (recalculateTrainerTypes) _ttlia = TrainerTypes.Select(ttype => new ListItem(ttype.Value.Name, ttype.Value)).OrderBy(item => item.Name).ToList();
                recalculateTrainerTypes = false;
                return _ttlia;
			}
        }
        public static List<Item> TMs
        {
            get
            {
                if (recalculateTMs)
                {
                    _tms = Items.ToList()
                                .FindAll(kvp => kvp.Value.Move is not null &&
                                        (HardcodedData.Get(kvp.Value.FieldUse, HardcodedData.ItemFieldUses) == "TR" || HardcodedData.Get(kvp.Value.FieldUse, HardcodedData.ItemFieldUses) == "TM"))
                                .Select(kvp => kvp.Value)
                                .ToList();
                }
                recalculateTMs = false;
                return _tms;
            }
        }
        public static List<Item> HMs
        {
            get
            {
                if (recalculateHMs)
                {
                    _hms = Items.ToList()
                                .FindAll(kvp => kvp.Value.Move is not null && HardcodedData.Get(kvp.Value.FieldUse, HardcodedData.ItemFieldUses) == "HM")
                                .Select(kvp => kvp.Value)
                                .ToList();
                }
                recalculateHMs = false;
                return _hms;
            }
        }


        private static bool recalculateSpeciesAndForms = true;
        private static bool recalculateSpecies = true;
        private static bool recalculateAbilities = true;
        private static bool recalculateMoves = true;
        private static bool recalculateTypes = true;
        private static bool recalculateItems = true;
        private static bool recalculateTrainerTypes = true;
        private static bool recalculateTMs = true;
        private static bool recalculateHMs = true;

        public static void InvalidateSpecies()
        {
            recalculateSpeciesAndForms = true;
            recalculateSpecies = true;
        }
		public static void InvalidateAbilities() => recalculateAbilities = true;
		public static void InvalidateMoves() => recalculateMoves = true;
		public static void InvalidateTypes() => recalculateTypes = true;
        public static void InvalidateItems()
        {
            recalculateItems = true;
            recalculateTMs = true;
            recalculateHMs = true;
        }
		public static void InvalidateTrainerTypes() => recalculateTrainerTypes = true;

        public static void InvalidateAll()
        {
            InvalidateSpecies();
            InvalidateAbilities();
            InvalidateMoves();
            InvalidateTypes();
            InvalidateItems();
            InvalidateTrainerTypes();
        }
	}
}

public enum EssentialsVersion
{
    Unknown = 0,
    v17     = 1,
    v17_1   = 2,
    v17_2   = 3,
    v18     = 4,
    v18_1   = 5,
    v19     = 6,
    v19_1   = 7,
    v20     = 8,
    v20_1   = 9,
    v21     = 10,
    v21_1   = 11,
}

public enum Hardcoded
{
    Habitats,
    GrowthRates,
    GenderRatios,
    EvolutionMethods,
    MoveCategories,
    MoveTargets,
    Natures,
    Weathers,
    ItemPockets,
    ItemFieldUses,
    ItemBattleUses,
    BodyColors,
    BodyShapes,
    EggGroups,
    EncounterTypes
}