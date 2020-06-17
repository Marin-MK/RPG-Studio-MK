using System;
using System.Collections.Generic;
using System.Text;

namespace RPGStudioMK
{
    public class MapCommandLineHandler : CommandLineHandler
    {
        public MapCommandLineHandler()
        {
            BaseCommand = "map ";
            Register(new Command("--help", Help, "Shows this help menu.", "-h"));
            Register(new Command("edit", Edit, "Edit an existing map."));
            Register(new Command("create", Create, "Create a new map."));
            Register(new Command("info", Info, "Displays information about a map."));
            Register(new Command("list", List, "Shows a list of all maps."));
            Register(new Command("destroy", Destroy, "Delete an existing map."));
            Initialize();
        }

        public bool Edit()
        {
            if (ArgIndex >= CurrentArgs.Count - 1)
            {
                Console.WriteLine("For information about the 'edit' command, please use 'map edit --help'.");
            }
            else
            {
                MapEditCommandLineHandler cli = new MapEditCommandLineHandler();
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false; // Return false to cancel the rest of the parsing
            }
            return true;
        }

        public bool Create()
        {
            if (ArgIndex >= CurrentArgs.Count - 1)
            {
                Console.WriteLine("For information about the 'create' command, please use 'map create --help'.");
            }
            else
            {
                MapEditCommandLineHandler cli = new MapEditCommandLineHandler(true);
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false; // Return false to cancel the rest of the parsing
            }
            return true;
        }

        private void PrintObject(List<object> list, int Indent, bool first = false)
        {
            string IndentStr = "";
            for (int i = 0; i < Indent; i++) IndentStr += " ";
            for (int i = (first ? 0 : 2); i < list.Count; i++)
            {
                if (list[i] is int)
                {
                    if (!Game.Data.Maps.ContainsKey((int) list[i])) continue;
                    Console.WriteLine($"{IndentStr}#{Utilities.Digits((int) list[i], 3)}: {Game.Data.Maps[(int) list[i]].DevName}");
                    Game.Data.Maps[(int) list[i]].Added = true;
                }
                else
                {
                    List<object> newlist = (List<object>) list[i];
                    if (!Game.Data.Maps.ContainsKey((int) newlist[0]))
                    {
                        PrintObject(newlist, Indent);
                    }
                    else
                    {
                        Console.WriteLine($"{IndentStr}#{Utilities.Digits((int) newlist[0], 3)}: {Game.Data.Maps[(int) newlist[0]].DevName}");
                        Game.Data.Maps[(int) newlist[0]].Added = true;
                        PrintObject(newlist, Indent + 4);
                    }
                }
            }
            if (first)
            {
                foreach (KeyValuePair<int, Game.Map> kvp in Game.Data.Maps)
                {
                    if (!kvp.Value.Added)
                    {
                        Console.WriteLine($"#{Utilities.Digits(kvp.Key, 3)}: {kvp.Value.DevName}");
                        Editor.ProjectSettings.MapOrder.Add(kvp.Key);
                    }
                }
            }
        }

        public bool List()
        {
            PrintObject(Editor.ProjectSettings.MapOrder, 0, true);
            return true;
        } 

        public bool Info()
        {
            if (ArgIndex >= CurrentArgs.Count - 1)
            {
                Console.WriteLine("For information about the 'info' command, please use 'map info --help'.");
            }
            else
            {
                InfoCommandLineHandler cli = new InfoCommandLineHandler();
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false; // Return false to cancel the rest of the parsing
            }
            return true;
        }

        public class InfoCommandLineHandler : CommandLineHandler
        {
            public int ID = -1;

            public InfoCommandLineHandler()
            {
                Register(new Command("--help", Help, "Shows this help menu.", "-h"));
                Register(new Command("--id", IDFunc, "Specifies the map ID."));
                Initialize();
            }

            public override bool Finalize()
            {
                if (ID == -1) return Error("Please specify a map ID using the '--id' parameter.");
                Game.Map Map = Game.Data.Maps[ID];
                Console.WriteLine($"Map #{Utilities.Digits(Map.ID, 3)}");
                Console.WriteLine($"Development Name: {Map.DevName}");
                Console.WriteLine($"Display Name: {Map.DisplayName}");
                Console.WriteLine($"Size: {Map.Width}x{Map.Height}");
                Console.Write("Tilesets: ");
                for (int i = 0; i < Map.TilesetIDs.Count; i++)
                {
                    Console.Write(Map.TilesetIDs[i]);
                    if (i != Map.TilesetIDs.Count - 1) Console.Write(", ");
                }
                Console.WriteLine();
                Console.Write("Autotiles: ");
                for (int i = 0; i < Map.AutotileIDs.Count; i++)
                {
                    Console.Write(Map.AutotileIDs[i]);
                    if (i != Map.AutotileIDs.Count - 1) Console.Write(", ");
                }
                Console.WriteLine();
                return true;
            }

            public bool IDFunc()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the ID of the tileset to add.");
                try
                {
                    ID = Convert.ToInt32(NextArg);
                    if (!Game.Data.Maps.ContainsKey(ID)) return Error($"No map could be found with ID #{ID}");
                }
                catch (Exception)
                {
                    return Error("Invalid integer.");
                }
                return true;
            }
        }

        public bool Destroy()
        {
            return true;
        }
    }
}
