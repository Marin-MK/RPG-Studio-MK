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
        public static List<Map> Maps = new List<Map>();
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
            GameData.Exec($"$species = nil");
        }

        public static void LoadTilesets()
        {
            LoadFile($"tilesets.mkd", "$tilesets");
            int tilesets = GameData.Exec($"$tilesets.size");
            for (int i = 0; i < tilesets; i++)
            {
                bool nil = GameData.Exec($"$tilesets[{i}].nil?");
                if (nil) Tilesets.Add(null);
                else
                {
                    Tilesets.Add(new Tileset($"$tilesets[{i}]"));
                }
            }
        }
    }
}
