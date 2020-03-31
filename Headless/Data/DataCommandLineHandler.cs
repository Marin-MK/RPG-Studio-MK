using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor
{
    public class DataCommandLineHandler : CommandLineHandler
    {
        public DataCommandLineHandler()
        {
            BaseCommand = "data ";
            Register(new Command("--help", Help, "Shows this help menu.", "-h"));
            Register(new Command("tileset", Tileset, "Performs operations on tilesets."));
            Initialize();
        }

        public bool Tileset()
        {
            if (CurrentArgs.Count == 1)
            {
                Console.WriteLine("For information about the 'tileset' command, please use 'data tileset --help'.");
            }
            else
            {
                TilesetCommandLineHandler cli = new TilesetCommandLineHandler();
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false;
            }
            return true;
        }
    }
}
