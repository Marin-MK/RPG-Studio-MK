using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronRuby;
using Microsoft.Scripting.Hosting;

namespace MKEditor.Data
{
    public static class GameData
    {
        public static ScriptEngine Engine;
        public static string DataPath;
        public static Dictionary<int, Map> Maps = new Dictionary<int, Map>();
        public static List<Tileset> Tilesets = new List<Tileset>();
        public static Dictionary<string, Species> Species = new Dictionary<string, Species>();

        public static void Initialize(string DataPath)
        {
            while (DataPath.Contains('\\')) DataPath = DataPath.Replace('\\', '/');
            GameData.DataPath = DataPath;
            Engine = Ruby.CreateEngine();
            Engine.Execute(Utilities.GetRubyRequirements());
            LoadSpecies();
            LoadTilesets();
            LoadMaps(); // TODO: Event commands/conditions
            // TODO: Map Connections
        }

        public static void LoadFile(string Filename, string GlobalVar)
        {
            while (Filename.Contains('\\')) Filename = Filename.Replace('\\', '/');
            Engine.Execute($"{GlobalVar} = FileUtils.load_data(\"{DataPath}/{Filename}\")");
        }

        public static dynamic Exec(string Code)
        {
            return Engine.Execute(Code);
        }

        public static void LoadSpecies()
        {
            LoadFile("species.mkd", "$species");
            List<string> keys = Array.ConvertAll((object[]) Exec($"$species.keys").ToArray(), x => x.ToString()).ToList();
            foreach (string key in keys)
            {
                Species[key] = new Species($"$species[:{key}]");
            }
            Exec($"$species = nil");
        }

        public static void LoadTilesets()
        {
            LoadFile($"tilesets.mkd", "$tilesets");
            int tilesets = Exec($"$tilesets.size");
            for (int i = 0; i < tilesets; i++)
            {
                bool nil = Exec($"$tilesets[{i}].nil?");
                if (nil) Tilesets.Add(null);
                else
                {
                    Tilesets.Add(new Tileset($"$tilesets[{i}]"));
                }
            }
            Exec("$tilesets = nil");
        }

        public static void LoadMaps()
        {
            List<int> maps = Array.ConvertAll(
                (object[]) Exec(
                    $"Dir.glob(\"{DataPath}/maps/*\")" +
                    $".select {{ |f| f =~ /#{{\"{DataPath}/maps/\"}}map\\d+.mkd/ }}" +
                    $".map {{ |f| f.sub(\"{DataPath}/maps/map\", \"\")" +
                                 $".sub(/.mkd/, \"\")" +
                    $"}}").ToArray(),
                x => Convert.ToInt32(x.ToString())
            ).ToList();
            for (int i = 0; i < maps.Count; i++)
            {
                string n = maps[i].ToString();
                if (n.Length == 1) n = '0' + n;
                if (n.Length == 2) n = '0' + n;
                LoadFile($"maps/map{n}.mkd", $"$map{n}");
                Maps[maps[i]] = new Map($"$map{n}");
            }
        }
    }
}
