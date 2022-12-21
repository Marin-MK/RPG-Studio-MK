using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace RPGStudioMK.Game;

public static class Data
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

    public static EssentialsVersion EssentialsVersion = EssentialsVersion.Unknown;
    public static bool UsesExternalScripts = false;

    static Action<float> OnProgressUpdated;

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

    public static void LoadGameData(Action<float> OnProgressUpdated)
    {
        Data.OnProgressUpdated = OnProgressUpdated;
        Compatibility.RMXP.Setup();
        if (StopLoading) return;
        LoadTilesets();
        if (StopLoading) return;
        LoadScripts();
        if (StopLoading) return;
        LoadMaps();
        if (StopLoading) return;
        LoadSystem();
        if (StopLoading) return;
        LoadCommonEvents();
        if (StopLoading) return;
        LoadGameINI();
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
        string path = RXProjectFilePath;
        while (path.Contains("\\")) path = path.Replace('\\', '/');
        List<string> splits = new List<string>(path.Split('/'));
        string projectfile = splits[splits.Count - 1];
        splits.RemoveAt(splits.Count - 1);
        path = "";
        for (int i = 0; i < splits.Count; i++)
        {
            path += splits[i];
            if (i != splits.Count - 1) path += "/";
        }
        Data.ProjectPath = path;
        Data.DataPath = path + "/Data";
        Data.ProjectFilePath = path + "/project.mkproj";
        Data.ProjectRMXPGamePath = path + "/Game.rxproj";
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
                return (false, ErrorType);
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

    private static void LoadTilesets()
    {
        SafeLoad("Tilesets.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            Autotiles.AddRange(new Autotile[Ruby.Array.Length(data) * 7]);
            Tilesets.Add(null);
            for (int i = 0; i < Ruby.Array.Length(data); i++)
            {
                IntPtr tileset = Ruby.Array.Get(data, i);
                if (tileset != Ruby.Nil)
                {
                    Tilesets.Add(new Tileset(tileset));
                }
            }
            Ruby.Unpin(data);
        });
    }

    private static void SaveTilesets()
    {
        SafeSave("Tilesets.rxdata", File =>
        {
            IntPtr tilesets = Ruby.Array.Create();
            Ruby.Pin(tilesets);
            foreach (Tileset tileset in Tilesets)
            {
                if (tileset == null) Ruby.Array.Push(tilesets, Ruby.Nil);
                else
                {
                    IntPtr tilesetdata = tileset.Save();
                    Ruby.Array.Push(tilesets, tilesetdata);
                }
            }
            Ruby.Marshal.Dump(tilesets, File);
            Ruby.Unpin(tilesets);
        });
    }

    /// <summary>
    /// Returns a list of map files in the given folder.
    /// </summary>
    private static List<(string, int)> GetMapIDs(string path)
    {
        List<(string, int)> Filenames = new List<(string, int)>();
        foreach (string file in Directory.GetFiles(path))
        {
            string realfile = file;
            while (realfile.Contains('\\')) realfile = realfile.Replace('\\', '/');
            string name = realfile.Split('/').Last();
            Match match = Regex.Match(name, @"Map(\d+).rxdata");
            if (match.Success)
                Filenames.Add((name, Convert.ToInt32(match.Groups[1].Value)));
        }
        // Subdirectories
        //foreach (string dir in Directory.GetDirectories(path))
        //{
        //    string realdir = dir;
        //    while (realdir.Contains('\\')) realdir = realdir.Replace('\\', '/');
        //    Filenames.AddRange(GetMapIDs(realdir));
        //}
        return Filenames;
    }

    private static void LoadMaps()
    {
        SafeLoad("MapInfos.rxdata", InfoFile =>
        {
            IntPtr mapinfo = Ruby.Marshal.Load(InfoFile);
            Ruby.Pin(mapinfo);
            List<(string, int)> Filenames = GetMapIDs(DataPath);
            int total = Filenames.Count;
            int count = 0;
            foreach ((string name, int id) tuple in Filenames)
            {
                SafeLoad(tuple.name, MapFile =>
                {
                    IntPtr mapdata = Ruby.Marshal.Load(MapFile);
                    Ruby.Pin(mapdata);
                    int id = tuple.id;
                    Map map = new Map(id, mapdata, Ruby.Hash.Get(mapinfo, Ruby.Integer.ToPtr(id)));
                    Maps[map.ID] = map;
                    Ruby.Unpin(mapdata);
                    count++;
                    SetLoadProgress(count / (float) total);
                });
                if (StopLoading) break;
            }
            Ruby.Unpin(mapinfo);
            Editor.AssignOrderToNewMaps();
        });
    }

    private static void SaveMaps()
    {
        (bool Success, string Error) = SafeSave("MapInfos.rxdata", InfoFile =>
        {
            IntPtr mapinfos = Ruby.Hash.Create();
            Ruby.Pin(mapinfos);
            foreach (Map map in Maps.Values)
            {
                SafeSave($"Map{Utilities.Digits(map.ID, 3)}.rxdata", MapFile =>
                {
                    IntPtr mapinfo = Ruby.Funcall(Compatibility.RMXP.MapInfo.Class, "new");
                    Ruby.Hash.Set(mapinfos, Ruby.Integer.ToPtr(map.ID), mapinfo);
                    Ruby.SetIVar(mapinfo, "@name", Ruby.String.ToPtr(map.Name));
                    Ruby.SetIVar(mapinfo, "@parent_id", Ruby.Integer.ToPtr(map.ParentID));
                    Ruby.SetIVar(mapinfo, "@order", Ruby.Integer.ToPtr(map.Order));
                    Ruby.SetIVar(mapinfo, "@expanded", map.Expanded ? Ruby.True : Ruby.False);
                    Ruby.SetIVar(mapinfo, "@scroll_x", Ruby.Integer.ToPtr(map.ScrollX));
                    Ruby.SetIVar(mapinfo, "@scroll_y", Ruby.Integer.ToPtr(map.ScrollY));

                    IntPtr mapdata = map.Save();
                    Ruby.Marshal.Dump(mapdata, MapFile);
                });
                if (StopLoading) break;
            }
            Ruby.Marshal.Dump(mapinfos, InfoFile);
            Ruby.Unpin(mapinfos);
        });
        if (Success)
        {
            // Delete all maps that are not part of of the data anymore
            // At least, only if saving all the other maps and info was a success.
            foreach ((string filename, int id) map in GetMapIDs(DataPath))
            {
                try
                {
                    if (!Maps.ContainsKey(map.id)) File.Delete(DataPath + "/" + map.filename);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to delete '{DataPath}/{map.filename}'.");
                }
            }
        }
    }

    private static void LoadSystem()
    {
        SafeLoad("System.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            System = new System(data);
            Ruby.Unpin(data);
        });
    }

    private static void SaveSystem()
    {
        System.EditMapID = Editor.ProjectSettings.LastMapID;
        SafeSave("System.rxdata", File =>
        {
            IntPtr data = System.Save();
            Ruby.Pin(data);
            Ruby.Marshal.Dump(data, File);
            Ruby.Unpin(data);
        });
    }

    private static void LoadCommonEvents()
    {
        SafeLoad("CommonEvents.rxdata", File =>
        {
            IntPtr list = Ruby.Marshal.Load(File);
            Ruby.Pin(list);
            for (int i = 1; i < Ruby.Array.Length(list); i++)
            {
                CommonEvent ce = new CommonEvent(Ruby.Array.Get(list, i));
                CommonEvents.Add(ce);
            }
            Ruby.Unpin(list);
        });
    }

    private static void SaveCommonEvents()
    {
        SafeSave("CommonEvents.rxdata", File =>
        {
            IntPtr list = Ruby.Array.Create();
            Ruby.Pin(list);
            for (int i = 0; i < CommonEvents.Count; i++)
            {
                Ruby.Array.Set(list, i + 1, CommonEvents[i].Save());
            }
            Ruby.Marshal.Dump(list, File);
            Ruby.Unpin(list);
        });
    }

    private static void LoadScripts()
    {
        if (Directory.Exists(DataPath + "/Scripts"))
            LoadScriptsExternal();
        else LoadScriptsRXDATA();
    }

    private static void LoadScriptsExternal()
    {
        List<(string, string)>? GetScripts(string Path, int Depth)
        {
            List<(string, string)>? Files = new List<(string, string)>();
            foreach (string File in Directory.GetFiles(Path))
            {
                try
                {
                    StreamReader sr = new StreamReader(global::System.IO.File.OpenRead(File));
                    string filename = global::System.IO.Path.GetFileNameWithoutExtension(File);
                    Match m = Regex.Match(filename, @"^\d+_(.*)$");
                    if (!m.Success) continue;
                    filename = m.Groups[1].Value;
                    Files.Add((filename, sr.ReadToEnd()));
                    sr.Close();
                }
                catch (Exception ex)
                {
                    LoadError("Scripts", ex.Message + "\n\n" + ex.StackTrace);
                    return null;
                }
            }
            foreach (string Directory in Directory.GetDirectories(Path))
            {
                List<(string, string)>? DirFiles = GetScripts(Directory, Depth + 1);
                if (DirFiles == null) return null;
                if (Depth == 0) Files.Add(("==================", ""));
                else Files.Add(("", ""));
                string DirName = global::System.IO.Path.GetFileName(Directory);
                Match m = Regex.Match(DirName, @"^\d+_(.*)$");
                if (!m.Success) continue;
                string SectionName = m.Groups[1].Value;
                Files.Add(($"[[ {SectionName} ]]", ""));
                Files.AddRange(DirFiles);
            }
            return Files;
        }
        List<(string, string)>? Files = GetScripts(DataPath + "/Scripts", 0);
        if (Files == null) return;
        if (Files.Count == 0)
        {
            LoadScriptsRXDATA();
            return;
        }
        foreach ((string Name, string Code) in Files)
        {
            Script Script = new Script();
            Script.Name = Name;
            Script.Content = Code;
            Scripts.Add(Script);
        }
        UsesExternalScripts = true;
    }

    private static void LoadScriptsRXDATA()
    {
        UsesExternalScripts = false;
        SafeLoad("Scripts.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            for (int i = 0; i < Ruby.Array.Length(data); i++)
            {
                IntPtr script = Ruby.Array.Get(data, i);
                Scripts.Add(new Script(script));
            }
            Ruby.Unpin(data);
            bool Inject = false;
            // Injects code at the top of the script list
            if (Inject)
            {
                string startcode = Utilities.GetInjectedCodeStart();
                if (Scripts[0].Name != "RPG Studio MK1")
                {
                    if (!string.IsNullOrEmpty(startcode))
                    {
                        Script script = new Script();
                        script.Name = "RPG Studio MK1";
                        script.Content = startcode;
                        Scripts.Insert(0, script);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(startcode)) Scripts.RemoveAt(0);
                    else Scripts[0].Content = startcode;
                }
                // Injects code at the bottom of the script list, above Main
                string maincode = Utilities.GetInjectedCodeAboveMain();
                if (Scripts.Count < 3 || Scripts[Scripts.Count - 2].Name != "RPG Studio MK2")
                {
                    if (!string.IsNullOrEmpty(maincode))
                    {
                        Script script = new Script();
                        script.Name = "RPG Studio MK2";
                        script.Content = maincode;
                        Scripts.Insert(Scripts.Count - 1, script);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(maincode)) Scripts.RemoveAt(Scripts.Count - 2);
                    else Scripts[Scripts.Count - 2].Content = maincode;
                }
            }
            // Find Essentials version
            for (int i = 0; i < Scripts.Count; i++)
            {
                Script s = Scripts[i];
                Match m = Regex.Match(s.Content, "module Essentials[\t\r\n ]*VERSION[\t\r\n ]*=[\t\r\n ]*\"(.*)\"");
                if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value)) // v19, v19.1, v20, etc.
                {
                    EssentialsVersion = m.Groups[1].Value switch
                    {
                        "19" => EssentialsVersion.v19,
                        "19.1" => EssentialsVersion.v19_1,
                        "20" => EssentialsVersion.v20,
                        "20.1" => EssentialsVersion.v20_1,
                        _ => EssentialsVersion.Unknown
                    };
                    break;
                }
                m = Regex.Match(s.Content, "(ESSENTIALS_VERSION|ESSENTIALSVERSION)[\t\r\n ]*=[\t\r\n ]*\"(.*)\"");
                if (m.Success && !string.IsNullOrEmpty(m.Groups[2].Value)) // v17, v17.1, v17.2, v18, v18.1
                {
                    EssentialsVersion = m.Groups[2].Value switch
                    {
                        "17" => EssentialsVersion.v17,
                        "17.1" => EssentialsVersion.v17_1,
                        "17.2" => EssentialsVersion.v17_2,
                        "18" => EssentialsVersion.v18,
                        "18.1" => EssentialsVersion.v18_1,
                        _ => EssentialsVersion.Unknown
                    };
                    break;
                }
            }
        });
    }

    private static void SaveScripts()
    {
        if (UsesExternalScripts) SaveScriptsExternal();
        else SaveScriptsRXDATA();
    }

    private static void SaveScriptsExternal()
    {
        if (!Directory.Exists(DataPath + "/Scripts")) Directory.CreateDirectory(DataPath + "/Scripts");
        else
        {
            // Delete all .rb files and all (\d+)_* folders
            ClearScriptFolder(DataPath + "/Scripts", false);
        }
        string? FirstParent = null;
        string? SecondParent = null;
        int MainCount = 0;
        int SubCount = 0;
        List<(string, int)> SubFolderCounts = new List<(string, int)>();
        List<(string, string, int)> Tracker = new List<(string, string, int)>(); // First Parent, Second Parent, No. Files
        foreach (Script script in Scripts)
        {
            if (script.Name == "==================")
            {
                FirstParent = null;
                SecondParent = null;
                SubCount = 0;
            }
            else
            {
                Match m = Regex.Match(script.Name, @"\[\[ (.*) \]\]");
                if (m.Success)
                {
                    if (FirstParent == null)
                    {
                        MainCount++;
                        if (m.Groups[1].Value == "Main") FirstParent = "999_" + m.Groups[1].Value;
                        else FirstParent = Utilities.Digits(MainCount, 3) + "_" + m.Groups[1].Value;
                    }
                    else
                    {
                        SubCount++;
                        SecondParent = Utilities.Digits(SubCount, 3) + "_" + m.Groups[1].Value;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(script.Name) && string.IsNullOrEmpty(script.Content)) continue; // We don't write scripts without a name and without content
                    string ScriptPath = null;
                    if (FirstParent == null) ScriptPath = DataPath + "/Scripts";
                    else if (SecondParent == null) ScriptPath = DataPath + "/Scripts/" + FirstParent;
                    else ScriptPath = DataPath + "/Scripts/" + FirstParent + "/" + SecondParent;
                    Directory.CreateDirectory(ScriptPath);
                    (string First, string Second, int Count)? result = Tracker.Find(t => t.Item1 == FirstParent && t.Item2 == SecondParent);
                    int FileCount = result == null ? 0 : result.Value.Count;
                    if (result != null) Tracker.Remove(((string, string, int)) result);
                    FileCount += 1;
                    Tracker.Add((FirstParent, SecondParent, FileCount));
                    if (script.Name == "Main") FileCount = 999;
                    ScriptPath += $"/{Utilities.Digits(FileCount, 3)}_{script.Name}.rb";
                    File.WriteAllText(ScriptPath, script.Content);
                }
            }
        }
    }

    private static void ClearScriptFolder(string Path, bool Delete)
    {
        foreach (string file in Directory.GetFiles(Path))
        {
            if (file.EndsWith(".rb")) File.Delete(file);
        }
        foreach (string folder in Directory.GetDirectories(Path))
        {
            if (Regex.IsMatch(folder, @"\d+_.*$"))
            {
                ClearScriptFolder(folder, true);
                if (!Utilities.DoesDirectoryHaveAnyFiles(folder)) Directory.Delete(folder);
            }
        }
    }

    private static void SaveScriptsRXDATA()
    {
        SafeSave("Scripts.rxdata", File =>
        {
            IntPtr scripts = Ruby.Array.Create();
            Ruby.Pin(scripts);
            foreach (Script script in Scripts)
            {
                IntPtr scriptdata = script.Save();
                Ruby.Array.Push(scripts, scriptdata);
            }
            Ruby.Marshal.Dump(scripts, File);
            Ruby.Unpin(scripts);
        });
    }

    private static Encoding win1252;

    private static void LoadGameINI()
    {
        if (win1252 == null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            win1252 = Encoding.GetEncoding("windows-1252");
        }
        string data = File.ReadAllText(ProjectPath + "/Game.ini", win1252);
        Match m = Regex.Match(data, @"Title=(.*)\n"); // \r will be included in the match, so we trim that out.
        if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value.Trim()))
        {
            Editor.ProjectSettings.ProjectName = m.Groups[1].Value.Trim();
        }
    }

    private static void SaveGameINI()
    {
        string data = File.ReadAllText(ProjectPath + "/Game.ini", win1252);
        data = Regex.Replace(data, @"Title=.*\n", $"Title={Editor.ProjectSettings.ProjectName}{Environment.NewLine}");
        File.WriteAllText(ProjectPath + "/Game.ini", data, win1252);
    }

    public static bool EssentialsAtLeast(EssentialsVersion Version)
    {
        return EssentialsVersion >= Version;
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