using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
    public static Dictionary<string, Item> TMsHMs = new Dictionary<string, Item>();
    public static Dictionary<string, TrainerType> TrainerTypes = new Dictionary<string, TrainerType>();
    public static Dictionary<(int Map, int Version), EncounterTable> Encounters = new Dictionary<(int, int), EncounterTable>();
    public static List<Trainer> Trainers = new List<Trainer>();
    // Other Project Data & Settings
    public static HardcodedDataStore HardcodedData;
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
    public static DataManager DataManager = new DataManager(new List<BaseDataManager>()
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
        //if (!File.Exists("hardcoded_data.json")) throw new Exception("The global harcoded_data.json file is missing. The program can not continue.");
        //HardcodedData = HardcodedDataStore.Create("hardcoded_data.json");
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
    }

	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SpeciesResolver))]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ItemResolver))]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TypeResolver))]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MoveResolver))]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AbilityResolver))]
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
        PluginManager.SaveAll();
    }

    public static void SaveScriptsRXDATA(string filename)
    {
        DataManager.SaveScriptsRXDATA(filename);
    }

    public static void SetProjectPath(string OriginalProjectFilePath)
    {
        if (!OriginalProjectFilePath.EndsWith(".rxproj") && !OriginalProjectFilePath.EndsWith(".mkproj")) throw new Exception("Invalid project file path.");
        string path = Path.GetDirectoryName(OriginalProjectFilePath).Replace('\\', '/');
        ProjectPath = path;
        DataPath = path + "/Data";
        ProjectFilePath = path + "/project.mkproj";
        ProjectRMXPGamePath = path + "/Game.rxproj";
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
        private static List<TreeNode> _saflia;
		private static List<TreeNode> _slia;
		private static List<TreeNode> _alia;
        private static List<TreeNode> _mlia;
        private static List<TreeNode> _tlia;
        private static List<TreeNode> _ilia;
        private static List<TreeNode> _ttlia;
        private static List<TreeNode> _tmshms;
        private static List<TreeNode> _fcs;

        public static List<TreeNode> SpeciesAndForms 
        {
            get 
            {
                if (recalculateSpeciesAndForms) _saflia = Data.Species.Select(spc => new TreeNode(spc.Value.Form != 0 ? $"{spc.Value.Name} ({spc.Value.FormName ?? spc.Value.Form.ToString()})" : spc.Value.Name, spc.Value)).OrderBy(item => item.Text).ToList();
                recalculateSpeciesAndForms = false;
                return _saflia;
            } 
        }
		public static List<TreeNode> Species
		{
			get
			{
                if (recalculateSpecies) _slia = Data.Species.Where(kvp => kvp.Value.Form == 0).Select(spc => new TreeNode(spc.Value.Name, spc.Value)).OrderBy(item => item.Text).ToList();
                recalculateSpecies = false;
				return _slia;
			}
		}
		public static List<TreeNode> Abilities
        {
            get
            {
                if (recalculateAbilities) _alia = Data.Abilities.Select(abil => new TreeNode(abil.Value.Name, abil.Value)).OrderBy(item => item.Text).ToList();
                recalculateAbilities = false;
                return _alia;
			}
        }
        public static List<TreeNode> Moves
        {
            get
            {
                if (recalculateMoves) _mlia = Data.Moves.Select(move => new TreeNode(move.Value.Name, move.Value)).OrderBy(item => item.Text).ToList();
                recalculateMoves = false;
                return _mlia;
			}
        }
        public static List<TreeNode> Types
        {
            get
            {
                if (recalculateTypes) _tlia = Data.Types.Select(type => new TreeNode(type.Value.Name, type.Value)).OrderBy(item => item.Text).ToList();
                recalculateTypes = false;
                return _tlia;
			}
        }
        public static List<TreeNode> Items
        {
            get
            {
                if (recalculateItems) _ilia = Data.Items.Select(item => new TreeNode(item.Value.Name, item.Value)).OrderBy(item => item.Text).ToList();
                recalculateItems = false;
                return _ilia;
			}
        }
        public static List<TreeNode> TrainerTypes
        {
            get
            {
                if (recalculateTrainerTypes) _ttlia = Data.TrainerTypes.Select(ttype => new TreeNode(ttype.Value.Name, ttype.Value)).OrderBy(item => item.Text).ToList();
                recalculateTrainerTypes = false;
                return _ttlia;
			}
        }
        public static List<TreeNode> TMsHMs
        {
            get
            {
                if (recalculateTMsHMs)
                {
                    _tmshms = Data.TMsHMs
                        .Select(kvp => new TreeNode($"{kvp.Value.Name} - {(kvp.Value.Move.Valid ? kvp.Value.Move.Move.Name : kvp.Value.Move.ID)}", kvp.Value))
                        .ToList();
                    _tmshms.Sort(delegate (TreeNode a, TreeNode b)
                    {
                        Item aI = (Item) a.Object;
                        Item bI = (Item) b.Object;
                        Match aM = Regex.Match(aI.ID, @"(TM|TR|HM)(\d+)");
                        Match bM = Regex.Match(bI.ID, @"(TM|TR|HM)(\d+)");
                        if (aM.Success)
                        {
                            if (bM.Success)
                            {
                                int cmp = aM.Groups[1].Value.CompareTo(bM.Groups[1].Value);
                                if (cmp != 0) return cmp;
                                int aNum = Convert.ToInt32(aM.Groups[2].Value);
                                int bNum = Convert.ToInt32(bM.Groups[2].Value);
                                return aNum.CompareTo(bNum);
							}
                            return 1;
                        }
                        else if (bM.Success)
                        {
                            return -1;
                        }
                        return aI.ID.CompareTo(bI.ID);
                    });
                }
                recalculateTMsHMs = false;
                return _tmshms;
            }
        }
        public static List<TreeNode> FunctionCodes
        {
            get
            {
                if (recalculateFunctionCodes)
                {
                    _fcs = new List<TreeNode>();
					foreach (Script scr in Data.Scripts)
					{
						foreach (string line in scr.Content.Split('\n').ToList())
						{
							if (!line.StartsWith("class Battle::Move::")) continue;
							Match m = Regex.Match(line, @"^class Battle::Move::([A-Za-z0-9_]*) < Battle::Move$");
							if (!m.Success) continue;
							_fcs.Add(new TreeNode(m.Groups[1].Value.ToString()));
						}
					}
                    _fcs.Sort(delegate (TreeNode a, TreeNode b) { return a.Text.CompareTo(b.Text); });
				}
                recalculateFunctionCodes = false;
                return _fcs;
            }
        }

        private static bool recalculateSpeciesAndForms = true;
        private static bool recalculateSpecies = true;
        private static bool recalculateAbilities = true;
        private static bool recalculateMoves = true;
        private static bool recalculateTypes = true;
        private static bool recalculateItems = true;
        private static bool recalculateTrainerTypes = true;
        private static bool recalculateTMsHMs = true;
        private static bool recalculateFunctionCodes = true;

        public static void InvalidateSpecies()
        {
            recalculateSpeciesAndForms = true;
            recalculateSpecies = true;
        }
		public static void InvalidateAbilities() => recalculateAbilities = true;
		public static void InvalidateMoves() => recalculateMoves = true;
		public static void InvalidateTypes() => recalculateTypes = true;
        public static void InvalidateItems() => recalculateItems = true;
        public static void InvalidateTMs() => recalculateTMsHMs = true;
		public static void InvalidateTrainerTypes() => recalculateTrainerTypes = true;
        public static void InvalidateFunctionCodes() => recalculateFunctionCodes = true;

        public static void InvalidateAll()
        {
            InvalidateSpecies();
            InvalidateAbilities();
            InvalidateMoves();
            InvalidateTypes();
            InvalidateItems();
            InvalidateTrainerTypes();
            InvalidateFunctionCodes();
        }
	}
}

public interface IDataResolver
{
	public string ID { get; set; }
	public bool Valid { get; }
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