using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Game;

public static class Data
{
    public static string ProjectPath;
    public static string ProjectFilePath;
    public static string DataPath;

    public static Dictionary<int, Map> Maps = new Dictionary<int, Map>();
    public static List<Tileset> Tilesets = new List<Tileset>();
    public static List<Autotile> Autotiles = new List<Autotile>();
    public static List<CommonEvent> CommonEvents = new List<CommonEvent>();
    public static Dictionary<string, Species> Species = new Dictionary<string, Species>();
    public static List<Script> Scripts = new List<Script>();
    public static System System;

    public static EssentialsVersion EssentialsVersion = EssentialsVersion.Unknown;

    public static void ClearProjectData()
    {
        ProjectPath = null;
        ProjectFilePath = null;
        DataPath = null;
        Maps.Clear();
        Tilesets.Clear();
        Autotiles.Clear();
        CommonEvents.Clear();
        Species.Clear();
        Scripts.Clear();
        System = null;
    }

    public static IEnumerable<float> LoadGameData()
    {
        Compatibility.RMXP.Setup();
        LoadTilesets();
        LoadScripts();
        foreach (float f in LoadMaps())
        {
            yield return f;
        }
        LoadSystem();
        LoadCommonEvents();
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
        Editor.ProjectFilePath = Data.ProjectFilePath;
    }

    private static void LoadTilesets()
    {
        IntPtr file = Ruby.File.Open(DataPath + "/Tilesets.rxdata", "rb");
        IntPtr data = Ruby.Marshal.Load(file);
        Ruby.Pin(data);
        Ruby.File.Close(file);
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
    }

    private static void SaveTilesets()
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
        IntPtr file = Ruby.File.Open(DataPath + "/Tilesets.rxdata", "wb");
        Ruby.Marshal.Dump(tilesets, file);
        Ruby.File.Close(file);
        Ruby.Unpin(tilesets);
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

    private static IEnumerable<float> LoadMaps()
    {
        IntPtr mapinfofile = Ruby.File.Open(DataPath + "/MapInfos.rxdata", "rb");
        IntPtr mapinfo = Ruby.Marshal.Load(mapinfofile);
        Ruby.Pin(mapinfo);
        Ruby.File.Close(mapinfofile);
        List<(string, int)> Filenames = GetMapIDs(DataPath);
        int total = Filenames.Count;
        int count = 0;
        foreach ((string name, int id) tuple in Filenames)
        {
            IntPtr mapfile = Ruby.File.Open(DataPath + "/" + tuple.name, "rb");
            IntPtr mapdata = Ruby.Marshal.Load(mapfile);
            Ruby.Pin(mapdata);
            int id = tuple.id;
            Ruby.File.Close(mapfile);
            Map map = new Map(id, mapdata, Ruby.Hash.Get(mapinfo, Ruby.Integer.ToPtr(id)));
            Maps[map.ID] = map;
            Ruby.Unpin(mapdata);
            count++;
            yield return count / (float) total;
        }
        Ruby.Unpin(mapinfo);
        Editor.AssignOrderToNewMaps();
    }

    private static void SaveMaps()
    {
        IntPtr mapinfos = Ruby.Hash.Create();
        Ruby.Pin(mapinfos);
        foreach (Map map in Maps.Values)
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
            IntPtr file = Ruby.File.Open(DataPath + $"/Map{Utilities.Digits(map.ID, 3)}.rxdata", "wb");
            Ruby.Marshal.Dump(mapdata, file);
            Ruby.File.Close(file);
        }
        IntPtr mapinfosfile = Ruby.File.Open(DataPath + $"/MapInfos.rxdata", "wb");
        Ruby.Marshal.Dump(mapinfos, mapinfosfile);
        Ruby.File.Close(mapinfosfile);
        Ruby.Unpin(mapinfos);
        // Delete all maps that are not part of of the data anymore
        foreach ((string filename, int id) map in GetMapIDs(DataPath))
        {
            if (!Maps.ContainsKey(map.id)) File.Delete(DataPath + "/" + map.filename);
        }
    }

    private static void LoadSystem()
    {
        IntPtr file = Ruby.File.Open(DataPath + "/System.rxdata", "rb");
        IntPtr data = Ruby.Marshal.Load(file);
        Ruby.Pin(data);
        Ruby.File.Close(file);
        System = new System(data);
        Ruby.Unpin(data);
    }

    private static void SaveSystem()
    {
        System.EditMapID = Editor.ProjectSettings.LastMapID;
        IntPtr data = System.Save();
        Ruby.Pin(data);
        IntPtr file = Ruby.File.Open(DataPath + "/System.rxdata", "wb");
        Ruby.Marshal.Dump(data, file);
        Ruby.File.Close(file);
        Ruby.Unpin(data);
    }

    private static void LoadCommonEvents()
    {
        IntPtr file = Ruby.File.Open(DataPath + "/CommonEvents.rxdata", "rb");
        IntPtr list = Ruby.Marshal.Load(file);
        Ruby.Pin(list);
        Ruby.File.Close(file);
        for (int i = 1; i < Ruby.Array.Length(list); i++)
        {
            CommonEvent ce = new CommonEvent(Ruby.Array.Get(list, i));
            CommonEvents.Add(ce);
        }
        Ruby.Unpin(list);
    }

    private static void SaveCommonEvents()
    {
        IntPtr list = Ruby.Array.Create();
        Ruby.Pin(list);
        for (int i = 0; i < CommonEvents.Count; i++)
        {
            Ruby.Array.Set(list, i + 1, CommonEvents[i].Save());
        }
        IntPtr file = Ruby.File.Open(DataPath + "/CommonEvents.rxdata", "wb");
        Ruby.Marshal.Dump(list, file);
        Ruby.File.Close(file);
        Ruby.Unpin(list);
    }

    private static void LoadScripts()
    {
        IntPtr file = Ruby.File.Open(DataPath + "/Scripts.rxdata", "rb");
        IntPtr data = Ruby.Marshal.Load(file);
        Ruby.Pin(data);
        Ruby.File.Close(file);
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
    }

    private static void SaveScripts()
    {
        IntPtr scripts = Ruby.Array.Create();
        Ruby.Pin(scripts);
        foreach (Script script in Scripts)
        {
            IntPtr scriptdata = script.Save();
            Ruby.Array.Push(scripts, scriptdata);
        }
        IntPtr file = Ruby.File.Open(DataPath + "/Scripts.rxdata", "wb");
        Ruby.Marshal.Dump(scripts, file);
        Ruby.File.Close(file);
        Ruby.Unpin(scripts);
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