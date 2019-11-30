using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MKEditor.Game
{
    public static class Data
    {
        public static string ProjectPath;
        public static string ProjectFilePath;
        public static string DataPath;

        public static Dictionary<int, Map> Maps = new Dictionary<int, Map>();
        public static List<Tileset> Tilesets = new List<Tileset>();
        public static Dictionary<string, Species> Species = new Dictionary<string, Species>();

        public static void LoadGameData()
        {
            LoadSpecies();
            LoadTilesets();
            LoadMaps(); // TODO: Event commands/conditions
            // TODO: Map Connections
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
            ProjectPath = path;
            DataPath = path + "/data";
            ProjectFilePath = path + "/project.mkproj";
            Editor.ProjectFilePath = ProjectFilePath;
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
            StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/species_editor.mkd"));
            sw.Write(jsonstring);
            sw.Close();
        }

        public static void LoadTilesets()
        {
            StreamReader sr = new StreamReader(File.OpenRead(DataPath + "/tilesets.mkd"));
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
                else Tilesets.Add(new Tileset(((JObject) AllTilests[i]).ToObject<Dictionary<string, object>>()));
            }
            SaveTileset();
        }

        public static void SaveTileset()
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
            StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/tilesets_editor.mkd"));
            sw.Write(jsonstring);
            sw.Close();
        }

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
            SaveMaps();
        }

        public static void SaveMaps()
        {
            foreach (KeyValuePair<int, Map> kvp in Maps)
            {
                Dictionary<string, object> Main = new Dictionary<string, object>();
                Main[":type"] = ":map";
                Main[":data"] = kvp.Value.ToJSON();
                string jsonstring = JsonConvert.SerializeObject(Main);
                StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/maps/map" + Utilities.Digits(kvp.Value.ID, 3) + "_editor.mkd"));
                sw.Write(jsonstring);
                sw.Close();
            }
        }
    }
}
