using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public static void ClearProjectData()
    {
        ProjectPath = null;
        ProjectFilePath = null;
        DataPath = null;
        Maps.Clear();
        Tilesets.Clear();
        Species.Clear();
        System = null;
    }

    private static void Initialize()
    {
        Compatibility.RMXP.Setup();
    }

    public static IEnumerable<float> LoadGameData()
    {
        Initialize();
        LoadTilesets();
        foreach (float f in LoadMaps())
        {
            yield return f;
        }
        LoadSystem();
        LoadCommonEvents();
        LoadScripts();
        //LoadSpecies();
    }

    public static void SaveGameData()
    {
        SaveTilesets();
        SaveMaps();
        SaveSystem();
        SaveCommonEvents();
        SaveScripts();
        //SaveSpecies();
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

    private static void LoadSpecies()
    {
        //StreamReader sr = new StreamReader(File.OpenRead(DataPath + "/species.mkd"));
        //string content = sr.ReadToEnd();
        //sr.Close();
        //Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
        //if ((string) data[":type"] != ":species")
        //{
        //    throw new Exception("Invalid data type for species.mkd - Expected to contain species data but found " + data[":type"] + ".");
        //}
        //Dictionary<string, object> AllSpecies = ((JObject) data[":data"]).ToObject<Dictionary<string, object>>();
        //foreach (string key in AllSpecies.Keys)
        //{
        //    Species s = new Species(((JObject) AllSpecies[key]).ToObject<Dictionary<string, object>>()); ;
        //    Species[s.IntName] = s;
        //}
    }

    private static void SaveSpecies()
    {
        //Dictionary<string, object> Main = new Dictionary<string, object>();
        //Main[":type"] = ":species";
        //Dictionary<string, object> list = new Dictionary<string, object>();
        //foreach (KeyValuePair<string, Species> kvp in Species)
        //{
        //    list[":" + kvp.Key] = kvp.Value.ToJSON();
        //}
        //Main[":data"] = list;
        //string jsonstring = JsonConvert.SerializeObject(Main);
        //if (File.Exists(DataPath + "/species.mkd")) File.Delete(DataPath + "/species.mkd");
        //StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/species.mkd"));
        //sw.Write(jsonstring);
        //sw.Close();
    }

    private static void LoadTilesets()
    {
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/Tilesets.rxdata"), Ruby.String.ToPtr("rb"));
        IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", file);
        Ruby.Pin(data);
        Ruby.Funcall(file, "close");
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
            if (tileset == null) Ruby.Funcall(tilesets, "push", Ruby.Nil);
            else
            {
                IntPtr tilesetdata = tileset.Save();
                Ruby.Funcall(tilesets, "push", tilesetdata);
            }
        }
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/Tilesets.rxdata"), Ruby.String.ToPtr("wb"));
        Ruby.Pin(file);
        IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "dump", tilesets);
        Ruby.Funcall(file, "write", data);
        Ruby.Funcall(file, "close");
        Ruby.Unpin(tilesets);
        Ruby.Unpin(file);
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
        IntPtr mapinfofile = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/MapInfos.rxdata"), Ruby.String.ToPtr("rb"));
        IntPtr mapinfo = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", mapinfofile);
        Ruby.Pin(mapinfo);
        Ruby.Funcall(mapinfofile, "close");
        List<(string, int)> Filenames = GetMapIDs(DataPath);
        int total = Filenames.Count;
        int count = 0;
        foreach ((string name, int id) tuple in Filenames)
        {
            IntPtr mapfile = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/" + tuple.name), Ruby.String.ToPtr("rb"));
            IntPtr mapdata = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", mapfile);
            Ruby.Pin(mapdata);
            int id = tuple.id;
            Ruby.Funcall(mapfile, "close");
            Map map = new Map(id, mapdata, Ruby.Hash.Get(mapinfo, Ruby.Integer.ToPtr(id)));
            Maps[map.ID] = map;
            Ruby.Unpin(mapdata);
            count++;
            yield return count / (float)total;
        }
        Ruby.Unpin(mapinfo);
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
            IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + $"/Map{Utilities.Digits(map.ID, 3)}.rxdata"), Ruby.String.ToPtr("wb"));
            Ruby.Pin(file);
            IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "dump", mapdata);
            Ruby.Funcall(file, "write", data);
            Ruby.Funcall(file, "close");
            Ruby.Unpin(file);
        }
        IntPtr mapinfosfile = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + $"/MapInfos.rxdata"), Ruby.String.ToPtr("wb"));
        Ruby.Pin(mapinfosfile);
        IntPtr mapinfosdata = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "dump", mapinfos);
        Ruby.Funcall(mapinfosfile, "write", mapinfosdata);
        Ruby.Funcall(mapinfosfile, "close");
        Ruby.Unpin(mapinfosfile);
        Ruby.Unpin(mapinfos);
        // Delete all maps that are not part of of the data anymore
        foreach ((string filename, int id) map in GetMapIDs(DataPath))
        {
            if (!Maps.ContainsKey(map.id)) File.Delete(DataPath + "/" + map.filename);
        }
    }

    private static void LoadSystem()
    {
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/System.rxdata"), Ruby.String.ToPtr("rb"));
        IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", file);
        Ruby.Pin(data);
        Ruby.Funcall(file, "close");
        System = new System(data);
        Ruby.Unpin(data);
    }

    private static void SaveSystem()
    {
        System.EditMapID = Editor.ProjectSettings.LastMapID;
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/System.rxdata"), Ruby.String.ToPtr("wb"));
        Ruby.Pin(file);
        IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "dump", System.Save());
        Ruby.Funcall(file, "write", data);
        Ruby.Funcall(file, "close");
        Ruby.Unpin(file);
    }

    private static void LoadCommonEvents()
    {
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/CommonEvents.rxdata"), Ruby.String.ToPtr("rb"));
        IntPtr list = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", file);
        Ruby.Pin(list);
        Ruby.Funcall(file, "close");
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
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/CommonEvents.rxdata"), Ruby.String.ToPtr("wb"));
        Ruby.Pin(file);
        IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "dump", list);
        Ruby.Funcall(file, "write", data);
        Ruby.Funcall(file, "close");
        Ruby.Unpin(file);
        Ruby.Unpin(list);
    }

    private static void LoadScripts()
    {
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/Scripts.rxdata"), Ruby.String.ToPtr("rb"));
        IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", file);
        Ruby.Pin(data);
        Ruby.Funcall(file, "close");
        for (int i = 0; i < Ruby.Array.Length(data); i++)
        {
            IntPtr script = Ruby.Array.Get(data, i);
            Scripts.Add(new Script(script));
        }
        Ruby.Unpin(data);
    }

    private static void SaveScripts()
    {
        IntPtr scripts = Ruby.Array.Create();
        Ruby.Pin(scripts);
        foreach (Script script in Scripts)
        {
            IntPtr scriptdata = script.Save();
            Ruby.Funcall(scripts, "push", scriptdata);
        }
        IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/Scripts.rxdata"), Ruby.String.ToPtr("wb"));
        Ruby.Pin(file);
        IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "dump", scripts);
        Ruby.Funcall(file, "write", data);
        Ruby.Funcall(file, "close");
        Ruby.Unpin(scripts);
        Ruby.Unpin(file);
    }
}
