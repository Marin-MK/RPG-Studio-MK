using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static void LoadGameData()
        {
            LoadSpecies();
            LoadTilesets();
            LoadAutotiles();
            LoadMaps(); // TODO: Event commands/conditions
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
            if (File.Exists(DataPath + "/species.mkd")) File.Delete(DataPath + "/species.mkd");
            StreamWriter sw = new StreamWriter(File.OpenWrite(DataPath + "/species.mkd"));
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
                else Tilesets.Add(new Tileset(((JObject)AllTilests[i]).ToObject<Dictionary<string, object>>()));
            }
            int MaxID = Tilesets.Count;
            int Missing = Editor.ProjectSettings.TilesetCapacity - MaxID + 1;
            for (int i = 0; i < Missing; i++) Tilesets.Add(null);
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
            SaveMapConnections();
        }

        public static void SaveMapConnections()
        {
            List<int> SeenMaps = new List<int>();
            MapConnection mc = new MapConnection();
            foreach (KeyValuePair<int, Map> kvp in Maps)
            {
                if (SeenMaps.Contains(kvp.Value.ID)) continue;
                SeenMaps.Add(kvp.Value.ID);
                // Loads all connections (and all those connections, etc) of one map (= one connection system)
                List<int> Comprising = new List<int>();
                LocateConnections(kvp.Value, SeenMaps, Comprising);
                if (Comprising.Count == 0) continue;
                int LowestX = 0;
                int LowestY = 0;
                foreach (int MapID in Comprising)
                {
                    if (Maps[MapID].SaveX < LowestX) LowestX = Maps[MapID].SaveX;
                    if (Maps[MapID].SaveY < LowestY) LowestY = Maps[MapID].SaveY;
                }
                LowestX = Math.Abs(LowestX);
                LowestY = Math.Abs(LowestY);
                Dictionary<List<int>, int> sys = new Dictionary<List<int>, int>();
                foreach (int MapID in Comprising)
                {
                    sys.Add(new List<int>() { Maps[MapID].SaveX + LowestX, Maps[MapID].SaveY + LowestY }, MapID);
                    Maps[MapID].SaveX = Maps[MapID].SaveY = 0;
                }
                sys.Add(new List<int>() { kvp.Value.SaveX + LowestX, kvp.Value.SaveY + LowestY }, kvp.Key);
                kvp.Value.SaveX = kvp.Value.SaveY = 0;
                mc.Maps.Add(sys);
            }
            Dictionary<string, object> Main = new Dictionary<string, object>();
            Main[":type"] = ":map_connections";
            Main[":data"] = mc.ToJSON();
            string jsonstring = JsonConvert.SerializeObject(Main);
            string file = DataPath + "/maps/connections.mkd";
            if (File.Exists(DataPath + "/maps/connections.mkd")) File.Delete(DataPath + "/maps/connections.mkd");
            StreamWriter sw = new StreamWriter(File.OpenWrite(file));
            sw.Write(jsonstring);
            sw.Close();
        }

        public static void LocateConnections(Map Map, List<int> SeenMaps, List<int> Comprising)
        {
            foreach (KeyValuePair<string, List<Connection>> kvp in Map.Connections)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Map m = Maps[kvp.Value[i].MapID];
                    if (SeenMaps.Contains(m.ID)) continue;
                    switch (kvp.Key)
                    {
                        case ":north":
                            m.SaveX = Map.SaveX + kvp.Value[i].Offset;
                            m.SaveY = Map.SaveY - m.Height;
                            break;
                        case ":east":
                            m.SaveX = Map.SaveX + Map.Width;
                            m.SaveY = Map.SaveY + kvp.Value[i].Offset;
                            break;
                        case ":south":
                            m.SaveX = Map.SaveX + kvp.Value[i].Offset;
                            m.SaveY = Map.SaveY + Map.Height;
                            break;
                        case ":west":
                            m.SaveX = Map.SaveX - m.Width;
                            m.SaveY = Map.SaveY + kvp.Value[i].Offset;
                            break;
                    }
                    SeenMaps.Add(m.ID);
                    Comprising.Add(m.ID);
                    LocateConnections(m, SeenMaps, Comprising);
                }
            }
        }
    }
}
