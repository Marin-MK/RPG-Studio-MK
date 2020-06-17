using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RPGStudioMK
{
    public class MapEditCommandLineHandler : CommandLineHandler
    {
        public bool Create;
        public int MapID = -1;
        public int Width = -1;
        public int Height = -1;
        public string DevName;
        public string DisplayName;
        public int ParentID = -1;

        public MapEditCommandLineHandler(bool Create = false)
        {
            this.Create = Create;
            BaseCommand = Create ? "map create " : "map edit ";
            Register(new Command("--help", Help, "Shows this help menu.", "-h"));
            if (!Create) Register(new Command("--id", ID, "Specifies which map to edit. Mandatory."));
            Register(new Command("--size", Size, "Sets the size of the map."));
            Register(new Command("--dev-name", DevNameFunc, "Sets the development name of the map."));
            Register(new Command("--display-name", DisplayNameFunc, "Sets the display name of the map."));
            Register(new Command("--name", Name, "Sets both display and development name of the map."));
            Register(new Command("--parent-id", ParentIDFunc, "Sets the parent map ID, used for displaying the map list."));
            Register(new Command("--tilesets", TilesetsFunc, "Sets the tilesets used by the map."));
            Register(new Command("--autotiles", AutotilesFunc, "Sets the autotiles used by the map."));
            Initialize();
        }

        public override bool Finalize()
        {
            if (Create)
            {
                if (!Success && cli == null) return false;
                if (Width == -1 || Height == -1) return Error("You must specify the map's size using the '--size' parameter.");
                if (DevName == null) return Error("You must specify the map's development name using the '--dev-name' or '--name' parameter.");
                if (DisplayName == null) return Error("You must specify the map's display name using the '--display-name' or '--name' parameter.");
                if (cli != null && !cli.Success) return false;
                Game.Map Map = new Game.Map();
                Map.ID = Editor.GetFreeMapID();
                Map.DevName = DevName;
                Map.DisplayName = DisplayName;
                Map.SetSize(Width, Height);
                Game.Data.Maps[Map.ID] = Map;
                if (ParentID != -1) Editor.AddIDToMap(Editor.ProjectSettings.MapOrder, ParentID, Map.ID);
                else Editor.ProjectSettings.MapOrder.Add(Map.ID);
                if (cli == null)
                {
                    Map.TilesetIDs = new List<int>() { 1 };
                    Map.AutotileIDs = new List<int>();
                }
                else
                {
                    if (cli is TilesetsCommandLineHandler) ((TilesetsCommandLineHandler) cli).IDs = new List<int>();
                    else ((AutotilesCommandLineHandler) cli).IDs = new List<int>();
                    if (cli is TilesetsCommandLineHandler) ((TilesetsCommandLineHandler) cli).MapID = Map.ID;
                    else ((AutotilesCommandLineHandler) cli).MapID = Map.ID;
                    cli.Finalize();
                }
                Console.WriteLine($"Created a new map with id #{Map.ID}.");
            }
            else
            {
                if (cli != null)
                {
                    if (!cli.Success) return false;
                    int CliMapID = -1;
                    if (cli is TilesetsCommandLineHandler) CliMapID = ((TilesetsCommandLineHandler) cli).MapID;
                    else CliMapID = ((AutotilesCommandLineHandler) cli).MapID;
                    if (CliMapID == -1 && MapID == -1) return Error("You must specify a map ID using the '--id' parameter.");
                    else if (CliMapID != -1) MapID = CliMapID;
                }
                else if (!Success && MapID == -1) return false;
                if (MapID == -1) return Error("You must specify a map ID using the '--id' parameter.");
                Game.Map Map = Game.Data.Maps[MapID];
                if (cli != null)
                {
                    if (cli is TilesetsCommandLineHandler) ((TilesetsCommandLineHandler) cli).IDs = Map.TilesetIDs;
                    else ((AutotilesCommandLineHandler) cli).IDs = Map.AutotileIDs;
                    if (cli is TilesetsCommandLineHandler) ((TilesetsCommandLineHandler) cli).MapID = Map.ID;
                    else ((AutotilesCommandLineHandler) cli).MapID = Map.ID;
                    if (!cli.Finalize())
                    {
                        if (cli is TilesetsCommandLineHandler) Map.TilesetIDs = ((TilesetsCommandLineHandler) cli).OldIDs;
                        else Map.AutotileIDs = ((AutotilesCommandLineHandler) cli).OldIDs;
                        return false;
                    }
                }
                if (ParentID != -1 && MapID != -1 && ParentID == MapID) return Error($"Cannot make map #{MapID} a child of itself.");
                if (ParentID > 0)
                {
                    if (Editor.MapIsChildMap(Editor.ProjectSettings.MapOrder, Map.ID, ParentID))
                    {
                        return Error($"Map #{ParentID} is a child map of map #{Map.ID}, so map #{Map.ID} can't also be a child of map #{ParentID}.");
                    }
                    else
                    {
                        object o = Editor.RemoveIDFromOrder(null, Editor.ProjectSettings.MapOrder, Map.ID);
                        Editor.AddIDToMap(Editor.ProjectSettings.MapOrder, ParentID, o);
                    }
                }
                else if (ParentID == 0)
                {
                    object o = Editor.RemoveIDFromOrder(null, Editor.ProjectSettings.MapOrder, Map.ID);
                    Editor.ProjectSettings.MapOrder.Add(o);
                }
                if (!string.IsNullOrEmpty(DevName)) Map.DevName = DevName;
                if (!string.IsNullOrEmpty(DisplayName)) Map.DisplayName = DisplayName;
                if (Width != -1 && Height != -1)
                {
                    Map.Resize(Map.Width, Width, Map.Height, Height);
                }
                Console.WriteLine($"Map #{Map.ID} has been edited successfully.");
            }
            return true;
        }

        public bool ID()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the ID of the map.");
            try
            {
                MapID = Convert.ToInt32(NextArg);
                if (MapID < 1 || !Game.Data.Maps.ContainsKey(MapID))
                {
                    return Error($"No map could be found with ID #{MapID}.");
                }
            }
            catch (Exception)
            {
                return Error("Invalid integer.");
            }
            return true;
        }

        public bool Size()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the size of the map.");
            if (!Regex.IsMatch(NextArg, @"\b\d+x\d+\b")) return Error("Map size format must be 'INTxINT'.");
            Width = Convert.ToInt32(NextArg.Split('x')[0]);
            Height = Convert.ToInt32(NextArg.Split('x')[1]);
            if (Width < 1 || Width >= 256) return Error("The map width must be between 1 and 255.");
            if (Height < 1 || Height >= 256) return Error("The map height must be between 1 and 255.");
            return true;
        }

        public bool DevNameFunc()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the development name of the map.");
            this.DevName = NextArg;
            return true;
        }

        public bool DisplayNameFunc()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the display name of the map.");
            this.DisplayName = NextArg;
            return true;
        }

        public bool Name()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the display name of the map.");
            this.DisplayName = this.DevName = NextArg;
            return true;
        }

        public bool ParentIDFunc()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the ID of the parent map.");
            try
            {
                ParentID = Convert.ToInt32(NextArg);
                if (ParentID < 1) ParentID = 0;
                if (ParentID > 0 && !Game.Data.Maps.ContainsKey(ParentID))
                {
                    return Error($"No map could be found with ID #{ParentID}.");
                }
            }
            catch (Exception)
            {
                return Error("Invalid integer.");
            }
            return true;
        }

        CommandLineHandler cli;

        public bool TilesetsFunc()
        {
            if (CurrentArgs.Count == 1)
            {
                Console.WriteLine("For information about the '--tilesets' parameter, please use 'map edit --tilesets --help'.");
                return false;
            }
            else
            {
                cli = new TilesetsCommandLineHandler(this.Create);
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false;
            }
        }

        public class TilesetsCommandLineHandler : CommandLineHandler
        {
            public int AddID = -1;
            public int RemoveID = -1;
            public int ReplaceID1 = -1;
            public int ReplaceID2 = -1;
            public List<int> SetList;
            public bool Confirm = false;
            public bool Clear = false;
            public List<int> IDs;
            public List<int> OldIDs;
            public int MapID = -1;

            public TilesetsCommandLineHandler(bool Create)
            {
                Register(new Command("--help", Help, "Shows this help menu.", "-h"));
                Register(new Command("--add", Add, "Appends a tileset to the tileset list."));
                Register(new Command("--remove", Remove, "Removes the tileset with the specified ID - not index."));
                if (Create) this.Confirm = true;
                else Register(new Command("--confirm", ConfirmFunc, "Confirms potentially dangerous operations.", "-c"));
                Register(new Command("--set", Set, "Sets the new list of tilesets."));
                Register(new Command("--replace", Replace, "Replaces one tileset with another one."));
                Register(new Command("--id", IDFunc, "Specifies which map to edit."));
                Register(new Command("--clear", ClearFunc, "Cleras the tileset list."));
                Initialize();
            }

            public override bool Finalize()
            {
                if (IDs != null)
                {
                    OldIDs = new List<int>(IDs);
                    if (!Confirm && (RemoveID != -1 || SetList != null))
                        return Error("You are about to alter the list of tilesets of this map. " +
                            "If the map is using tiles from tilesets that are to be deleted, these tiles will be deleted too. " +
                            "Repeat the command with '--confirm' appended to confirm your choice.");
                    if (AddID != -1 && IDs.Contains(AddID))
                        return Error($"The tileset you tried to add, #{AddID}, is already present in the tileset list.");
                    if (RemoveID != -1 && !IDs.Contains(RemoveID))
                        return Error($"The tileset you tried to remove, #{RemoveID} is not present in the tileset list.");
                    if (ReplaceID1 != -1 && !IDs.Contains(ReplaceID1))
                        return Error($"The tileset you tried to replace, #{ReplaceID1} is not present in the tileset list.");
                    if (ReplaceID2 != -1 && IDs.Contains(ReplaceID2))
                        return Error($"The tileset you tried to replace with, #{ReplaceID2}, is already present in the tileset list.");
                    if (AddID != -1 && RemoveID != -1 || AddID != -1 && ReplaceID1 != -1 || RemoveID != -1 && ReplaceID1 != -1 ||
                        AddID != -1 && SetList != null || RemoveID != -1 && SetList != null || ReplaceID1 != -1 && SetList != null ||
                        AddID != -1 && Clear || RemoveID != -1 && Clear || ReplaceID1 != -1 && Clear || SetList != null && Clear)
                        return Error("Only one add/remove/replace/set/clear operation is allowed at a time on the tileset list.");
                    if (AddID != -1)
                    {
                        IDs.Add(AddID);
                    }
                    else if (RemoveID != -1)
                    {
                        IDs.Remove(RemoveID);
                    }
                    else if (ReplaceID1 != -1)
                    {
                        int Index = IDs.IndexOf(ReplaceID1);
                        IDs.RemoveAt(Index);
                        IDs.Insert(Index, ReplaceID2);
                    }
                    else if (SetList != null)
                    {
                        IDs = new List<int>(SetList);
                    }
                    else if (Clear)
                    {
                        IDs.Clear();
                    }
                    Game.Map Map = Game.Data.Maps[MapID];
                    for (int layer = 0; layer < Map.Layers.Count; layer++)
                    {
                        for (int i = 0; i < Map.Layers[layer].Tiles.Count; i++)
                        {
                            Game.TileData data = Map.Layers[layer].Tiles[i];
                            if (data == null) continue;
                            if (data.TileType == Game.TileType.Autotile) continue;
                            int TilesetIndex = data.Index;
                            int TilesetID = OldIDs[TilesetIndex];
                            if (IDs.Contains(TilesetID)) data.Index = IDs.IndexOf(TilesetID);
                            else Map.Layers[layer].Tiles[i] = null;
                        }
                    }
                    Map.TilesetIDs = IDs;
                }
                return true;
            }

            public bool ConfirmFunc()
            {
                Confirm = true;
                return true;
            }

            private int ParseTilesetID()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg))
                {
                    Error("Please specify the ID of the tileset to add.");
                    return -1;
                }
                try
                {
                    int ID = Convert.ToInt32(NextArg);
                    if (ID < 1 || ID >= Editor.ProjectSettings.TilesetCapacity)
                    {
                        Error($"No tileset could be found with ID #{ID}");
                        return -1;
                    }
                    return ID;
                }
                catch (Exception)
                {
                    Error("Invalid integer.");
                    return -1;
                }
            }

            public bool IDFunc()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the map ID using the '--id' parameter");
                try
                {
                    MapID = Convert.ToInt32(NextArg);
                    if (!Game.Data.Maps.ContainsKey(MapID)) return Error($"No map could be found with id #{MapID}.");
                }
                catch (Exception)
                {
                    return Error("Invalid integer.");
                }
                return true;
            }

            public bool Add()
            {
                int TilesetID = ParseTilesetID();
                if (TilesetID != -1) this.AddID = TilesetID;
                else return false;
                return true;
            }

            public bool Remove()
            {
                int TilesetID = ParseTilesetID();
                if (TilesetID != -1) this.RemoveID = TilesetID;
                else return false;
                return true;
            }

            public bool Replace()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the ID of the tileset to replace.");
                if (!Regex.IsMatch(NextArg, @"\b\d+,\d+\b"))
                    return Error("The format for the '--replace' parameter argument must be 'ID_TO_REPLACE,NEW_ID'");
                int ID1 = Convert.ToInt32(NextArg.Split(',')[0]);
                int ID2 = Convert.ToInt32(NextArg.Split(',')[1]);
                if (ID1 < 1 || ID1 >= Editor.ProjectSettings.TilesetCapacity) return Error("The ID to be replace is not a valid tileset ID.");
                if (ID2 < 1 || ID2 >= Editor.ProjectSettings.TilesetCapacity) return Error("The ID to replace with is not a valid tileset ID.");
                ReplaceID1 = ID1;
                ReplaceID2 = ID2;
                return true;
            }

            public bool Set()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify a list of tile IDs.");
                if (!Regex.IsMatch(NextArg, @"^\d+(,\d+)*$"))
                    return Error("The format for the '--set' parameter argument must be 'INT,INT,INT,...'");
                List<string> Ints = NextArg.Split(',').ToList();
                SetList = new List<int>();
                foreach (string s in Ints)
                {
                    int id = Convert.ToInt32(s);
                    if (id < 1 || id >= Editor.ProjectSettings.TilesetCapacity) return Error($"Could not find a tileset with id #{id}.");
                    if (SetList.Contains(id)) return Error($"Duplicate tileset IDs: #{id}.");
                    SetList.Add(id);
                }
                return true;
            }

            public bool ClearFunc()
            {
                Clear = true;
                return true;
            }
        }

        public bool AutotilesFunc()
        {
            if (CurrentArgs.Count == 1)
            {
                Console.WriteLine("For information about the '--autotiles' parameter, please use 'map edit --autotiles --help'.");
                return false;
            }
            else
            {
                cli = new AutotilesCommandLineHandler(this.Create);
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false;
            }
        }

        public class AutotilesCommandLineHandler : CommandLineHandler
        {
            public int AddID = -1;
            public int RemoveID = -1;
            public int ReplaceID1 = -1;
            public int ReplaceID2 = -1;
            public List<int> SetList;
            public bool Confirm = false;
            public bool Clear = false;
            public List<int> IDs;
            public List<int> OldIDs;
            public int MapID = -1;

            public AutotilesCommandLineHandler(bool Create)
            {
                Register(new Command("--help", Help, "Shows this help menu.", "-h"));
                Register(new Command("--add", Add, "Appends an autotile to the autotile list."));
                Register(new Command("--remove", Remove, "Removes the autotile with the specified ID - not index."));
                if (Create) this.Confirm = true;
                else Register(new Command("--confirm", ConfirmFunc, "Confirms potentially dangerous operations.", "-c"));
                Register(new Command("--set", Set, "Sets the new list of autotiles."));
                Register(new Command("--replace", Replace, "Replaces one autotile with another one."));
                Register(new Command("--id", IDFunc, "Specifies which map to edit."));
                Register(new Command("--clear", ClearFunc, "Cleras the autotile list."));
                Initialize();
            }

            public override bool Finalize()
            {
                if (IDs != null)
                {
                    OldIDs = new List<int>(IDs);
                    if (!Confirm && (RemoveID != -1 || SetList != null))
                        return Error("You are about to alter the list of autotiles of this map. " +
                            "If the map is using tiles from autotiles that are to be deleted, these tiles will be deleted too. " +
                            "Repeat the command with '--confirm' appended to confirm your choice.");
                    if (AddID != -1 && IDs.Contains(AddID))
                        return Error($"The autotile you tried to add, #{AddID}, is already present in the autotile list.");
                    if (RemoveID != -1 && !IDs.Contains(RemoveID))
                        return Error($"The autotile you tried to remove, #{RemoveID} is not present in the autotile list.");
                    if (ReplaceID1 != -1 && !IDs.Contains(ReplaceID1))
                        return Error($"The autotile you tried to replace, #{ReplaceID1} is not present in the autotile list.");
                    if (ReplaceID2 != -1 && IDs.Contains(ReplaceID2))
                        return Error($"The autotile you tried to replace with, #{ReplaceID2}, is already present in the autotile list.");
                    if (AddID != -1 && RemoveID != -1 || AddID != -1 && ReplaceID1 != -1 || RemoveID != -1 && ReplaceID1 != -1 ||
                        AddID != -1 && SetList != null || RemoveID != -1 && SetList != null || ReplaceID1 != -1 && SetList != null ||
                        AddID != -1 && Clear || RemoveID != -1 && Clear || ReplaceID1 != -1 && Clear || SetList != null && Clear)
                        return Error("Only one add/remove/replace/set/clear operation is allowed at a time on the autotile list.");
                    if (AddID != -1)
                    {
                        IDs.Add(AddID);
                    }
                    else if (RemoveID != -1)
                    {
                        IDs.Remove(RemoveID);
                    }
                    else if (ReplaceID1 != -1)
                    {
                        int Index = IDs.IndexOf(ReplaceID1);
                        IDs.RemoveAt(Index);
                        IDs.Insert(Index, ReplaceID2);
                    }
                    else if (SetList != null)
                    {
                        IDs = new List<int>(SetList);
                    }
                    else if (Clear)
                    {
                        IDs.Clear();
                    }
                    Game.Map Map = Game.Data.Maps[MapID];
                    for (int layer = 0; layer < Map.Layers.Count; layer++)
                    {
                        for (int i = 0; i < Map.Layers[layer].Tiles.Count; i++)
                        {
                            Game.TileData data = Map.Layers[layer].Tiles[i];
                            if (data == null) continue;
                            if (data.TileType == Game.TileType.Tileset) continue;
                            int AutotileIndex = data.Index;
                            int AutotileID = OldIDs[AutotileIndex];
                            if (IDs.Contains(AutotileID)) data.Index = IDs.IndexOf(AutotileID);
                            else Map.Layers[layer].Tiles[i] = null;
                        }
                    }
                    Map.AutotileIDs = IDs;
                }
                return true;
            }

            public bool ConfirmFunc()
            {
                Confirm = true;
                return true;
            }

            private int ParseAutotileID()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg))
                {
                    Error("Please specify the ID of the autotile to add.");
                    return -1;
                }
                try
                {
                    int ID = Convert.ToInt32(NextArg);
                    if (ID < 1 || ID >= Editor.ProjectSettings.AutotileCapacity)
                    {
                        Error($"No autotile could be found with ID #{ID}");
                        return -1;
                    }
                    return ID;
                }
                catch (Exception)
                {
                    Error("Invalid integer.");
                    return -1;
                }
            }

            public bool IDFunc()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the map ID using the '--id' parameter");
                try
                {
                    MapID = Convert.ToInt32(NextArg);
                    if (!Game.Data.Maps.ContainsKey(MapID)) return Error($"No map could be found with id #{MapID}.");
                }
                catch (Exception)
                {
                    return Error("Invalid integer.");
                }
                return true;
            }

            public bool Add()
            {
                int AutotileID = ParseAutotileID();
                if (AutotileID != -1) this.AddID = AutotileID;
                else return false;
                return true;
            }

            public bool Remove()
            {
                int AutotileID = ParseAutotileID();
                if (AutotileID != -1) this.RemoveID = AutotileID;
                else return false;
                return true;
            }

            public bool Replace()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify the ID of the autotile to replace.");
                if (!Regex.IsMatch(NextArg, @"\b\d+,\d+\b"))
                    return Error("The format for the '--replace' parameter argument must be 'ID_TO_REPLACE,NEW_ID'");
                int ID1 = Convert.ToInt32(NextArg.Split(',')[0]);
                int ID2 = Convert.ToInt32(NextArg.Split(',')[1]);
                if (ID1 < 1 || ID1 >= Editor.ProjectSettings.AutotileCapacity) return Error("The ID to be replace is not a valid autotile ID.");
                if (ID2 < 1 || ID2 >= Editor.ProjectSettings.AutotileCapacity) return Error("The ID to replace with is not a valid autotile ID.");
                ReplaceID1 = ID1;
                ReplaceID2 = ID2;
                return true;
            }

            public bool Set()
            {
                string NextArg = GetNextArg(true);
                if (string.IsNullOrEmpty(NextArg)) return Error("Please specify a list of tile IDs.");
                if (!Regex.IsMatch(NextArg, @"^\d+(,\d+)*$"))
                    return Error("The format for the '--set' parameter argument must be 'INT,INT,INT,...'");
                List<string> Ints = NextArg.Split(',').ToList();
                SetList = new List<int>();
                foreach (string s in Ints)
                {
                    int id = Convert.ToInt32(s);
                    if (id < 1 || id >= Editor.ProjectSettings.AutotileCapacity) return Error($"Could not find an autotile with id #{id}.");
                    if (SetList.Contains(id)) return Error($"Duplicate autotile IDs: #{id}.");
                    SetList.Add(id);
                }
                return true;
            }

            public bool ClearFunc()
            {
                Clear = true;
                return true;
            }
        }
    }
}
