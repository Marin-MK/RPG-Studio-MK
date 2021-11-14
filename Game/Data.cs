using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public static class Data
    {
        public static string ProjectPath;
        public static string ProjectFilePath;
        public static string DataPath;

        public static Dictionary<int, Map> Maps = new Dictionary<int, Map>();
        public static List<Tileset> Tilesets = new List<Tileset>();
        public static List<Autotile> Autotiles = new List<Autotile>();
        public static Dictionary<string, Species> Species = new Dictionary<string, Species>();

        public static void ClearProjectData()
        {
            ProjectPath = null;
            ProjectFilePath = null;
            DataPath = null;
            Maps.Clear();
            Tilesets.Clear();
            Species.Clear();
        }

        private static void Initialize()
        {
            Compatibility.RMXP.Setup();
        }

        public static void LoadGameData()
        {
            Initialize();
            //LoadSpecies();
            LoadTilesets();
            LoadMaps();
        }

        public static void SaveGameData()
        {
            SaveTilesets();
            //SaveMaps();
            //SaveSpecies();
        }

        public static void SetProjectPath(string ProjectFilePath)
        {
            string path = ProjectFilePath;
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
            Data.ProjectFilePath = path + "/settings.mkproj";
            Editor.ProjectFilePath = Data.ProjectFilePath;
        }

        private static void LoadSpecies()
        {
            StreamReader sr = new StreamReader(File.OpenRead(DataPath + "/species.mkd"));
            string content = sr.ReadToEnd();
            sr.Close();
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            if ((string) data[":type"] != ":species")
            {
                throw new Exception("Invalid data type for species.mkd - Expected to contain species data but found " + data[":type"] + ".");
            }
            Dictionary<string, object> AllSpecies = ((JObject) data[":data"]).ToObject<Dictionary<string, object>>();
            foreach (string key in AllSpecies.Keys)
            {
                Species s = new Species(((JObject) AllSpecies[key]).ToObject<Dictionary<string, object>>()); ;
                Species[s.IntName] = s;
            }
        }

        private static void SaveSpecies()
        {
            Dictionary<string, object> Main = new Dictionary<string, object>();
            Main[":type"] = ":species";
            Dictionary<string, object> list = new Dictionary<string, object>();
            foreach (KeyValuePair<string, Species> kvp in Species)
            {
                list[":" + kvp.Key] = kvp.Value.ToJSON();
            }
            Main[":data"] = list;
            string jsonstring = JsonConvert.SerializeObject(Main);
            if (File.Exists(DataPath + "/species.mkd")) File.Delete(DataPath + "/species.mkd");
            StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/species.mkd"));
            sw.Write(jsonstring);
            sw.Close();
        }
        
        private static void LoadTilesets()
        {
            IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/Tilesets.rxdata"), Ruby.String.ToPtr("rb"));
            IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", file);
            Ruby.Pin(data);
            Ruby.Funcall(file, "close");
            long count = Ruby.Integer.FromPtr(Ruby.Funcall(data, "length"));
            Autotiles.AddRange(new Autotile[count * 7]);
            Tilesets.Add(null);
            for (int i = 0; i < count; i++)
            {
                IntPtr tileset = Ruby.Array.Get(data, i);
                Ruby.Pin(tileset);
                if (tileset != Ruby.Nil)
                {
                    Tilesets.Add(new Tileset(tileset));
                }
                Ruby.Unpin(tileset);
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
                    IntPtr tilesetbin = tileset.Save();
                    Ruby.Funcall(tilesets, "push", tilesetbin);
                }
            }
            IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/Tilesets2.rxdata"), Ruby.String.ToPtr("wb"));
            IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "dump", tilesets);
            Ruby.Funcall(file, "write", data);
            Ruby.Funcall(file, "close");
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
            IntPtr mapinfofile = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/MapInfos.rxdata"), Ruby.String.ToPtr("rb"));
            IntPtr mapinfo = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", mapinfofile);
            Ruby.Pin(mapinfo);
            Ruby.Funcall(mapinfofile, "close");
            List<(string, int)> Filenames = GetMapIDs(DataPath);
            foreach ((string name, int id) tuple in Filenames)
            {
                IntPtr mapfile = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/" + tuple.name), Ruby.String.ToPtr("rb"));
                IntPtr mapdata = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", mapfile);
                Ruby.Pin(mapdata);
                int id = tuple.id;
                Ruby.Funcall(mapfile, "close");
                Map map = new Map(id, mapdata, Ruby.Hash.Get(mapinfo, Ruby.Integer.ToPtr(id)));
                Maps[map.ID] = map;
            }
        }

        private static void SaveMaps()
        {
            /*
            // Delete all old map files
            foreach (string map in GetMapIDs(DataPath + "/maps"))
            {
                File.Delete(map);
            }
            // And create the new map files
            foreach (KeyValuePair<int, Map> kvp in Maps)
            {
                Dictionary<string, object> Main = new Dictionary<string, object>();
                Main[":type"] = ":map";
                Main[":data"] = kvp.Value.ToJSON();
                string jsonstring = JsonConvert.SerializeObject(Main);
                string file = DataPath + "/maps/map" + Utilities.Digits(kvp.Value.ID, 3) + ".mkd";
                StreamWriter sw = new StreamWriter(File.OpenWrite(file));
                sw.Write(jsonstring);
                sw.Close();
            }*/
            throw new NotImplementedException();
        }
    }
}
