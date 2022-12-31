using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
    public static List<GamePlugin> Plugins = new List<GamePlugin>();
    public static EssentialsVersion EssentialsVersion = EssentialsVersion.Unknown;
    public static bool UsesExternalScripts = false;

    // Used during loading process
    public static bool StopLoading;
    
    public static bool LoadedMetadataFromPBS = false;
    static List<BaseDataManager>? DataManagers;
    static Action<float> OnProgressUpdated;
    static Action<string> OnLoadTextChanging;

    public static void ClearProjectData()
    {
        ProjectPath = null;
        ProjectFilePath = null;
        ProjectRMXPGamePath = null;
        DataPath = null;
        DataManagers.ForEach(dm => dm.Clear());
        Plugins.Clear();
        StopLoading = false;
        UsesExternalScripts = false;
    }

    private static void InitializeDataManagers()
    {
        if (DataManagers != null) return;
        // The order and types of data that are loaded in this format.
        // There are few restrictions, other than:
        // - Tilesets must be loaded before maps
        // - Maps must be loaded before map metadata
        // - Metadata must be loaded before player metadata (this is not a strict requirement,
        //   but player metadata will discard data if metadata loads from PBS)
        DataManagers = new List<BaseDataManager>()
        {
            // RMXP Data
            new GameConfigManager(),
            new TilesetManager(),
            new MapManager(),
            new CommonEventManager(),
            new ScriptManager(),
            new SystemManager(),
            // Important/Useful Essentials Data
            new MapMetadataManager(false),
            new MetadataManager(false),
            new PlayerMetadataManager(),
            // Other Essentials Data
            new SpeciesManager(false),
            new AbilityManager(false),
            new ItemManager(false),
            new MoveManager(false),
            new TypeManager(false),
            new TrainerTypeManager(false),
            new TrainerManager(false),
            new EncounterManager(false),
        };
    }

    public static void LoadGameData(Action<float> OnProgressUpdated, Action<string> OnLoadTextChanging)
    {
        Data.OnProgressUpdated = OnProgressUpdated;
        Data.OnLoadTextChanging = OnLoadTextChanging;
        Compatibility.RMXP.Setup();

        InitializeDataManagers();
        DataManagers.ForEach(dm => dm.InitializeClass());
        DataManagers.ForEach(dm =>
        {
            if (StopLoading) return;
            dm.Load();
        });

        if (StopLoading) return;
        SetLoadText("Loading plugins...");
        PluginManager.LoadAll(true);

        SetLoadText("Loading project...");
        SetLoadProgress(1f);
    }

    public static void SaveGameData()
    {
        DataManagers.ForEach(dm => dm.Save());
    }

    public static void SetProjectPath(string RXProjectFilePath)
    {
        if (!RXProjectFilePath.EndsWith(".rxproj")) throw new Exception("Invalid project file path.");
        string path = Path.GetDirectoryName(RXProjectFilePath).Replace('\\', '/');
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

    public static bool EssentialsAtLeast(EssentialsVersion Version)
    {
        return EssentialsVersion >= Version;
    }

    public static bool IsPrimaryVersion(EssentialsVersion Version)
    {
        switch (Version)
        {
            case EssentialsVersion.v17: return EssentialsVersion >= EssentialsVersion.v17 && EssentialsVersion <= EssentialsVersion.v17_2;
            case EssentialsVersion.v18: return EssentialsVersion == EssentialsVersion.v18 || EssentialsVersion == EssentialsVersion.v18_1;
            case EssentialsVersion.v19: return EssentialsVersion == EssentialsVersion.v19 || EssentialsVersion == EssentialsVersion.v19_1;
            case EssentialsVersion.v20: return EssentialsVersion == EssentialsVersion.v20 || EssentialsVersion == EssentialsVersion.v20_1;
            default: return false;
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
    v20_1   = 9
}