using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Dir"), "chdir", Ruby.String.ToPtr(DataPath));
        }

        public static void LoadGameData()
        {
            Initialize();
            //LoadSpecies();
            LoadTilesets();
            //LoadAutotiles();
            //LoadMaps(); // TODO: Event commands/conditions
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

        public static void LoadSpecies()
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

        public static void SaveSpecies()
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

        public static void LoadTilesets()
        {
            IntPtr file = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(DataPath + "/Tilesets.rxdata"), Ruby.String.ToPtr("rb"));
            IntPtr data = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", file);
            Ruby.Pin(data);
            Ruby.Funcall(file, "close");
            long count = Ruby.Integer.FromPtr(Ruby.Funcall(data, "length"));
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
            /*StreamReader sr = new StreamReader(File.OpenRead(DataPath + "/tilesets.mkd"));
            string content = sr.ReadToEnd();
            sr.Close();
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            if ((string) data[":type"] != ":tilesets")
            {
                throw new Exception("Invalid data type for tilesets.mkd - Expected to contain tileset data but found " + data[":type"] + ".");
            }
            List<object> AllTilests = ((JArray) data[":data"]).ToObject<List<object>>();
            for (int i = 0; i < AllTilests.Count; i++)
            {
                if (AllTilests[i] == null) Tilesets.Add(null);
                else Tilesets.Add(new Tileset(((JObject)AllTilests[i]).ToObject<Dictionary<string, object>>()));
            }
            int MaxID = Tilesets.Count;
            int Missing = Editor.ProjectSettings.TilesetCapacity - MaxID + 1;
            for (int i = 0; i < Missing; i++) Tilesets.Add(null);*/
        }

        public static void SaveTilesets()
        {
            Dictionary<string, object> Main = new Dictionary<string, object>();
            Main[":type"] = ":tilesets";
            List<object> list = new List<object>();
            for (int i = 0; i < Tilesets.Count; i++)
            {
                if (Tilesets[i] == null) list.Add(null);
                else list.Add(Tilesets[i].ToJSON());
            }
            Main[":data"] = list;
            string jsonstring = JsonConvert.SerializeObject(Main);
            if (File.Exists(DataPath + "/tilesets.mkd")) File.Delete(DataPath + "/tilesets.mkd");
            StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/tilesets.mkd"));
            sw.Write(jsonstring);
            sw.Close();
        }

        public static void LoadAutotiles()
        {
            StreamReader sr = new StreamReader(File.OpenRead(DataPath + "/autotiles.mkd"));
            string content = sr.ReadToEnd();
            sr.Close();
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            if ((string) data[":type"] != ":autotiles")
            {
                throw new Exception("Invalid data type for autotiles.mkd - Expected to contain autotile data but found " + data[":type"] + ".");
            }
            List<object> AllAutotiles = ((JArray) data[":data"]).ToObject<List<object>>();
            for (int i = 0; i < AllAutotiles.Count; i++)
            {
                if (AllAutotiles[i] == null) Autotiles.Add(null);
                else Autotiles.Add(new Autotile(((JObject) AllAutotiles[i]).ToObject<Dictionary<string, object>>()));
            }
            int MaxID = Autotiles.Count;
            int Missing = Editor.ProjectSettings.AutotileCapacity - MaxID + 1;
            for (int i = 0; i < Missing; i++) Autotiles.Add(null);
        }

        public static void SaveAutotiles()
        {
            Dictionary<string, object> Main = new Dictionary<string, object>();
            Main[":type"] = ":autotiles";
            List<object> list = new List<object>();
            for (int i = 0; i < Autotiles.Count; i++)
            {
                if (Autotiles[i] == null) list.Add(null);
                else list.Add(Autotiles[i].ToJSON());
            }
            Main[":data"] = list;
            string jsonstring = JsonConvert.SerializeObject(Main);
            if (File.Exists(DataPath + "/autotiles.mkd")) File.Delete(DataPath + "/autotiles.mkd");
            StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/autotiles.mkd"));
            sw.Write(jsonstring);
            sw.Close();
        }

        /// <summary>
        /// Returns a list of map files in the given folder.
        /// </summary>
        public static List<string> GetMapIDs(string path)
        {
            List<string> Filenames = new List<string>();
            foreach (string file in Directory.GetFiles(path))
            {
                string realfile = file;
                while (realfile.Contains('\\')) realfile = realfile.Replace('\\', '/');
                string name = realfile.Split('/').Last();
                if (name.StartsWith("map") && name.EndsWith(".mkd"))
                {
                    name = name.Substring(3, name.Length - 7);
                    int id;
                    bool valid = int.TryParse(name, out id);
                    if (valid) Filenames.Add(realfile);
                }
            }
            // Subdirectories
            foreach (string dir in Directory.GetDirectories(path))
            {
                string realdir = dir;
                while (realdir.Contains('\\')) realdir = realdir.Replace('\\', '/');
                Filenames.AddRange(GetMapIDs(realdir));
            }
            return Filenames;
        }

        public static void LoadMaps()
        {
            List<string> Filenames = GetMapIDs(DataPath + "/maps");
            foreach (string filename in Filenames)
            {
                StreamReader sr = new StreamReader(File.OpenRead(filename));
                string content = sr.ReadToEnd();
                sr.Close();
                Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                if ((string) data[":type"] != ":map")
                {
                    throw new Exception("Invalid data type for '" + filename + "' - Expected to contain map data but found " + data[":type"] + ".");
                }
                Map map = new Map(((JObject) data[":data"]).ToObject<Dictionary<string, object>>());
                Maps[map.ID] = map;
            }
        }

        public static void SaveMaps()
        {
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
            }
        }
    }
}
