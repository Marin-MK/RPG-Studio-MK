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
    public static string ProjectPath;
    public static string ProjectFilePath;
    public static string ProjectRMXPGamePath;
    public static string DataPath;

    public static Dictionary<int, Map> Maps = new Dictionary<int, Map>();
    public static List<Tileset> Tilesets = new List<Tileset>();
    public static List<Autotile> Autotiles = new List<Autotile>();
    public static List<CommonEvent> CommonEvents = new List<CommonEvent>();
    public static Dictionary<string, Species> Species = new Dictionary<string, Species>();
    public static List<Script> Scripts = new List<Script>();
    public static System System;

    public static bool StopLoading;

    private static nint GameDataModule;
    private static nint SpeciesClass;

    public static EssentialsVersion EssentialsVersion = EssentialsVersion.Unknown;
    public static bool UsesExternalScripts = false;

    static Action<float> OnProgressUpdated;
    static Action<string> OnLoadTextChanging;

    public static void ClearProjectData()
    {
        ProjectPath = null;
        ProjectFilePath = null;
        ProjectRMXPGamePath = null;
        DataPath = null;
        Maps.Clear();
        Tilesets.Clear();
        Autotiles.Clear();
        CommonEvents.Clear();
        Species.Clear();
        Scripts.Clear();
        System = null;
        StopLoading = false;
        UsesExternalScripts = false;
    }

    public static void LoadGameData(Action<float> OnProgressUpdated, Action<string> OnLoadTextChanging)
    {
        Data.OnProgressUpdated = OnProgressUpdated;
        Data.OnLoadTextChanging = OnLoadTextChanging;
        Compatibility.RMXP.Setup();

        if (GameDataModule == nint.Zero) GameDataModule = Ruby.Module.Define("GameData");
        if (SpeciesClass == nint.Zero) SpeciesClass = Ruby.Class.Define("Species", GameDataModule, null);
        if (StopLoading) return;

        LoadTilesets();
        if (StopLoading) return;

        LoadScripts();
        if (StopLoading) return;

        SetLoadText("Loading maps...");
        LoadMaps();
        if (StopLoading) return;

        SetLoadText("Loading project...");
        LoadSystem();
        if (StopLoading) return;

        LoadCommonEvents();
        if (StopLoading) return;
        LoadGameINI();

        if (StopLoading) return;
        SetLoadText("Loading species...");
        LoadSpecies();

        SetLoadText("Loading project...");
        SetLoadProgress(1f);
    }

    public static void SaveGameData()
    {
        SaveTilesets();
        SaveScripts();
        SaveMaps();
        SaveSystem();
        SaveCommonEvents();
        SaveGameINI();
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

    public static void AbortLoad()
    {
        StopLoading = true;
    }

    private static void LoadError(string File, string ErrorMessage)
    {
        string text = ErrorMessage switch
        {
            "Errno::EACCES" => $"RPG Studio MK was unable to load '{File}' because it was likely in use by another process.\nPlease try again.",
            "Errno::ENOENT" => $"RPG Studio MK was unable to load '{File}' because it does not exist.",
            "TypeError" => $"RPG Studio MK was unable to load '{File}' because it contains incorrect data. Are you sure this file has the correct name?",
            "EOFError" => $"RPG Studio MK was unable to load '{File}' because it was empty or contained invalid data.\nIt may be corrupt or outdated.",
            _ => $"RPG Studio MK was unable to load '{File}'.\n\n" + ErrorMessage + "\n\nPlease try again."
        };
        MessageBox mbox = new MessageBox("Error", text, ButtonType.OK, IconType.Error);
        AbortLoad();
    }

    private static void SaveError(string File, string ErrorMessage)
    {
        string text = ErrorMessage switch
        {
            "Errno::EACCES" => $"RPG Studio MK was unable to save '{File}' because it was likely in use by another process.\n\n" +
                                "All other data has been saved successfully. Please try again.",
            _ => $"RPG Studio MK was unable to save '{File}'.\n\n{ErrorMessage}\n\nAll other data has been saved successfully. Please try again."
        };
        MessageBox mbox = new MessageBox("Error", text, ButtonType.OK, IconType.Error);
        // Keep saving; prefer corrupting data if something is seriously wrong, which is doubtful, over
        // the prospect of losing all data in memory if the issue is only something minor, in a small section of the program.
    }

    private static (bool Success, string Error) SafeLoad(string Filename, Action<IntPtr> Action)
    {
        (bool Success, string Error) = SafelyOpenAndCloseFile(DataPath + "/" + Filename, "rb", Action);
        if (!Success) LoadError("Data/" + Filename, Error);
        return (Success, Error);
    }

    private static (bool Success, string Error) SafeSave(string Filename, Action<IntPtr> Action)
    {
        (bool Success, string Error) = SafelyOpenAndCloseFile(DataPath + "/" + Filename, "wb", Action);
        if (!Success) SaveError("Data/" + Filename, Error);
        return (Success, Error);
    }

    private static (bool Success, string Error) SafelyOpenAndCloseFile(string Filename, string Mode, Action<IntPtr> Action, int Tries = 10, int DelayInMS = 40)
    {
        int Total = Tries;
        while (Tries > 0)
        {
            if (Ruby.Protect(_ =>
            {
                IntPtr File = Ruby.File.Open(Filename, Mode);
                Ruby.Pin(File);
                Action(File);
                Ruby.File.Close(File);
                Ruby.Unpin(File);
                return IntPtr.Zero;
            }))
            {
                if (Tries != Total)
                    Console.WriteLine($"{Filename.Split('/').Last()} opened after {Total - Tries + 1} attempt(s) and {DelayInMS * (Total - Tries + 1)}ms.");
                return (true, null);
            }
            string ErrorType = Ruby.GetErrorType();
            if (ErrorType != "Errno::EACCES")
            {
                // Other error than simultaneous access, no point in retrying.
                return (false, Ruby.GetErrorText());
            }
            Thread.Sleep(DelayInMS);
            Tries--;
        }
        Console.WriteLine($"{Filename.Split('/').Last()} failed to open after {Total} attempt(s) and {DelayInMS * Total}ms.");
        return (false, "Errno::EACCES");
    }

    private static void SetLoadProgress(float Progress)
    {
        OnProgressUpdated?.Invoke(Progress);
    }

    private static  void SetLoadText(string Text)
    {
        SetLoadProgress(0);
        OnLoadTextChanging?.Invoke(Text);
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