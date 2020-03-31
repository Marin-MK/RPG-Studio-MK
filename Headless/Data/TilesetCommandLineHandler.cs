using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor
{
    public class TilesetCommandLineHandler : CommandLineHandler
    {
        public TilesetCommandLineHandler()
        {
            BaseCommand = "data tileset ";
            Register(new Command("--help", Help, "Shows this help menu.", "-h"));
            Register(new Command("list", List, "Shows a list of all tilesets."));
            Register(new Command("info", Info, "Shows basic information about a tileset."));
            Register(new Command("show", Show, "Shows the tileset's per-tile properties."));
            Initialize();
        }

        public bool List()
        {
            for (int i = 1; i < Editor.ProjectSettings.TilesetCapacity; i++)
            {
                Console.Write($"#{Utilities.Digits(i, 3)}: ");
                if (i < Game.Data.Tilesets.Count && Game.Data.Tilesets[i] != null)
                {
                    Console.WriteLine(Game.Data.Tilesets[i].Name);
                }
                else Console.WriteLine();
            }
            return true;
        }

        public bool Info()
        {
            if (CurrentArgs.Count == 1)
            {
                Console.WriteLine("For information about the 'info' command, please use 'data tileset info --help'.");
            }
            else
            {
                InfoCommandLineHandler cli = new InfoCommandLineHandler();
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false;
            }
            return true;
        }

        public class InfoCommandLineHandler : CommandLineHandler
        {
            public int TilesetID = -1;

            public InfoCommandLineHandler()
            {
                Register(new Command("--help", Help, "Shows this help menu.", "-h"));
                Register(new Command("--id", TilesetIDFunc, "Specifies the tileset to fetch its info."));
                Initialize();
            }

            public override bool Finalize()
            {
                if (!Success) return false;
                if (TilesetID == -1) return Error("Please specify a tileset ID with the '--id' parameter.");
                Console.WriteLine($"Tileset #{Utilities.Digits(TilesetID, 3)}");
                Game.Tileset tileset = Game.Data.Tilesets[TilesetID];
                Console.WriteLine($"Name: {tileset?.Name}");
                Console.WriteLine($"Graphic Name: {tileset?.GraphicName}");
                return true;
            }

            public bool TilesetIDFunc()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the ID of the tileset.");
                try
                {
                    TilesetID = Convert.ToInt32(NextArg);
                    if (TilesetID < 1 || TilesetID >= Editor.ProjectSettings.TilesetCapacity)
                    {
                        return Error($"No tileset could be found with ID #{TilesetID}.");
                    }
                }
                catch (Exception)
                {
                    return Error("Invalid integer.");
                }
                return true;
            }
        }

        public bool Show()
        {
            if (CurrentArgs.Count == 1)
            {
                Console.WriteLine("For information about the 'show' command, please use 'data tileset show --help'.");
            }
            else
            {
                ShowCommandLineHandler cli = new ShowCommandLineHandler();
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false;
            }
            return true;
        }

        public class ShowCommandLineHandler : CommandLineHandler
        {
            public int TilesetID = -1;
            public bool Passability = false;
            public bool Priority = false;

            public ShowCommandLineHandler()
            {
                Register(new Command("--help", Help, "Shows this help menu.", "-h"));
                Register(new Command("--id", TilesetIDFunc, "Specifies the tileset to show."));
                Register(new Command("--passability", PassabilityFunc, "Shows the tileset's passability data."));
                Register(new Command("--priority", PriorityFunc, "Shows the tileset's priority data."));
                Initialize();
            }

            public override bool Finalize()
            {
                if (!Success) return false;
                if (TilesetID == -1) return Error("Please specify a tileset ID with the '--id' parameter.");
                Game.Tileset tileset = Game.Data.Tilesets[TilesetID];
                if (tileset == null)
                {
                    Console.WriteLine($"Tileset #{Utilities.Digits(TilesetID, 3)} is empty.");
                    return true;
                }
                if (!Passability && !Priority) return Error("Please use one of the following flags: '--passability', '--priority'");
                if (Passability)
                {
                    Console.WriteLine("\nPassability data");
                    Console.WriteLine("0: Impassable");
                    Console.WriteLine("1: Passable Down");
                    Console.WriteLine("2: Passable Left");
                    Console.WriteLine("4: Passable Right");
                    Console.WriteLine("8: Passable Up");
                    for (int i = 0; i < tileset.Passabilities.Count; i++)
                    {
                        if (i % 8 == 0)
                        {
                            Console.WriteLine();
                            Console.Write($"#{Utilities.Digits(i, 3)}: ");
                        }
                        Console.Write(Utilities.Digits((int) tileset.Passabilities[i], 2) + " ");
                    }
                    Console.WriteLine();
                }
                if (Priority)
                {
                    Console.WriteLine("\nPriority data");
                    for (int i = 0; i < tileset.Passabilities.Count; i++)
                    {
                        if (i % 8 == 0)
                        {
                            if (i > 0) Console.WriteLine();
                            Console.Write($"#{Utilities.Digits(i, 3)}: ");
                        }
                        if (tileset.Priorities[i] == null) Console.Write("00 ");
                        else Console.Write(Utilities.Digits((int) tileset.Priorities[i], 2) + " ");
                    }
                    Console.WriteLine();
                }
                return true;
            }

            public bool TilesetIDFunc()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the ID of the tileset.");
                try
                {
                    TilesetID = Convert.ToInt32(NextArg);
                    if (TilesetID < 1 || TilesetID >= Editor.ProjectSettings.TilesetCapacity)
                    {
                        return Error($"No tileset could be found with ID #{TilesetID}.");
                    }
                }
                catch (Exception)
                {
                    return Error("Invalid integer.");
                }
                return true;
            }

            public bool PassabilityFunc()
            {
                this.Passability = true;
                return true;
            }

            public bool PriorityFunc()
            {
                this.Priority = true;
                return true;
            }
        }
    }
}
