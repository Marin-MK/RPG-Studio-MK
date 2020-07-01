using System;
using System.Collections.Generic;
using System.Linq;
using amethyst;
using odl;
using rubydotnet;
using RPGStudioMK.Widgets;
using RPGStudioMK.Game;
using System.IO;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static void Setup()
        {
            Ruby.Initialize();
            RPG.Create();
            Table.Create();
            Tone.Create();
            Map.Create();
            AudioFile.Create();
            Event.Create();
            Page.Create();
            EventCommand.Create();
            Condition.Create();
            MoveRoute.Create();
            MoveCommand.Create();
            Graphic.Create();
            MapInfo.Create();
            Tileset.Create();
        }

        public static void Cleanup()
        {
            Ruby.Reset();
        }

        public static void ImportMaps()
        {
            Setup();
            OpenFileDialog of = new OpenFileDialog();
            of.SetFilter(new FileFilter("RPG Maker XP Map", "rxdata"));
            of.SetTitle("Pick map(s)");
            of.SetAllowMultiple(true);
            object ret = of.Show();
            List<string> Files = new List<string>();
            if (ret is string) Files.Add(ret as string);
            else if (ret is List<string>) Files = ret as List<string>;
            else return; // No files picked
            for (int i = 0; i < Files.Count; i++)
            {
                while (Files[i].Contains('\\')) Files[i] = Files[i].Replace('\\', '/');
            }
            string[] folders = Files[0].Split('/');
            string parent = "";
            string root = "";
            for (int i = 0; i < folders.Length - 1; i++)
            {
                parent += folders[i];
                if (i != folders.Length - 2) root += folders[i];
                if (i != folders.Length - 2) parent += '/';
                if (i != folders.Length - 3) root += '/';
            }
            List<string> Names = new List<string>();
            foreach (string f in Files)
            {
                string[] l = f.Split('/').Last().Split('.');
                string n = "";
                for (int i = 0; i < l.Length - 1; i++)
                {
                    n += l[i];
                    if (i != l.Length - 2) n += '.';
                }
                Names.Add(n);
            }
            // Load MapInfos.rxdata
            Ruby.File infofile = new Ruby.File(parent + "/MapInfos.rxdata", "rb");
            Ruby.Hash infos = Ruby.Marshal.Load(infofile).Convert<Ruby.Hash>();
            infos.Pin();
            Ruby.Array keys = infos.Keys;
            keys.Pin();
            infofile.Close();
            // Load Tilesets.rxdata
            Ruby.File tilesetfile = new Ruby.File(parent + "/Tilesets.rxdata", "rb");
            Ruby.Array tilesets = Ruby.Marshal.Load(tilesetfile).Convert<Ruby.Array>();
            tilesets.Pin();
            tilesetfile.Close();
            Action<int> ImportMap = null;
            ImportMap = delegate (int MapIndex)
            {
                // Convert rxdata (Ruby) to mkd (C#)
                string MapName = Names[MapIndex];
                string file = Files[MapIndex];
                while (file.Contains('\\')) file = file.Replace('\\', '/');
                // Load Map.rxdata
                Ruby.File f = new Ruby.File(file, "rb");
                Map map = Ruby.Marshal.Load(f).Convert<Map>();
                map.Pin();
                f.Close();
                int id = Convert.ToInt32(file.Substring(file.Length - 10, 3));
                // Link Map with its MapInfo
                MapInfo info = null;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i] != Ruby.Nil && keys[i].Convert<Ruby.Integer>() == id)
                    {
                        info = infos[keys[i]].Convert<MapInfo>();
                    }
                }
                if (info == null) throw new Exception($"No MapInfo could be found for map ({MapName}).");
                Game.Map data = new Game.Map();
                data.ID = Editor.GetFreeMapID();
                data.DisplayName = info.Name.ToString();
                data.DevName = data.DisplayName;
                data.Width = map.Width.ToInt32();
                data.Height = map.Height.ToInt32();
                Tileset tileset = tilesets[map.TilesetID].Convert<Tileset>();
                tileset.Pin();
                string tilesetname = tileset.Name.ToString();
                Action cont = null;
                Game.Tileset existing = Data.Tilesets.Find(t => t != null && (t.Name == tilesetname || t.GraphicName == tilesetname));
                bool exist = existing != null;
                string message = $"Importing Map ({MapName})...\n\n";
                if (exist) message += "The tileset of the imported map has the same name as an already-defined tileset in " +
                    $"the database ({existing.Name}).\n" +
                    "Would you like to use this tileset, choose a different one, or import it?";
                else message += $"No tilesets similar to the one used in the imported map ({tileset.Name}) could be found.\n" +
                    "Would you like to pick an existing tileset, or import it?";
                List<string> Options = new List<string>();
                if (exist) Options.Add("Use this");
                Options.Add("Pick other");
                Options.Add("Import it");
                MessageBox box = new MessageBox("Importing Map", message, Options);
                box.OnButtonPressed += delegate (BaseEventArgs e)
                {
                    if (Options[box.Result] == "Use this") // Use the matched tileset
                    {
                        data.TilesetIDs = new List<int>() { existing.ID };
                        cont();
                    }
                    else if (Options[box.Result] == "Pick other") // Pick other tileset
                    {
                        TilesetPicker picker = new TilesetPicker(null);
                        picker.OnClosed += delegate (BaseEventArgs e)
                        {
                            if (picker.ChosenTilesetID > 0) // Chose tileset
                            {
                                data.TilesetIDs = new List<int>() { picker.ChosenTilesetID };
                                cont();
                            }
                            else // Didn't pick tileset; cancel importing
                            {
                                data = null;
                                tileset.Unpin();
                                map.Unpin();
                                MessageBox b = new MessageBox("Warning", $"Importing Map ({MapName})...\n\nAs no tileset was chosen, this map will not be imported.", IconType.Warning);
                                b.OnButtonPressed += delegate (BaseEventArgs e)
                                {
                                    if (MapIndex < Files.Count - 1) ImportMap(MapIndex + 1);
                                };
                            }
                        };
                    }
                    else if (Options[box.Result] == "Import it") // Import the tileset
                    {
                        string filename = root + "/Graphics/Tilesets/" + tileset.TilesetName.ToString() + ".png";
                        if (!File.Exists(filename)) // Graphic doesn't exist
                        {
                            MessageBox b = new MessageBox("Error", $"Importing Map ({MapName})...\n\nThe tileset graphic could not be found. The tileset cannot be imported, and thus this map will not be imported.", IconType.Error);
                            b.OnButtonPressed += delegate (BaseEventArgs e)
                            {
                                if (MapIndex < Files.Count - 1) ImportMap(MapIndex + 1);
                            };
                        }
                        else // Graphic does exist
                        {
                            Bitmap bmp = new Bitmap(filename);
                            int pw = bmp.Width / 32 * 33;
                            int ph = bmp.Height / 32 * 33;
                            if (pw > Graphics.MaxTextureSize.Width || ph > Graphics.MaxTextureSize.Height)
                            {
                                MessageBox b = new MessageBox("Error",
                                    $"Importing Map ({MapName})...\n\n" +
                                    $"The formatted tileset will exceed the maximum texture size ({Graphics.MaxTextureSize.Width},{Graphics.MaxTextureSize.Height}).\n" +
                                    "This map will not be imported."
                                );
                                b.OnButtonPressed += delegate (BaseEventArgs e)
                                {
                                    if (MapIndex < Files.Count - 1) ImportMap(MapIndex + 1);
                                };
                            }
                            else
                            {
                                string destination = Data.ProjectPath + "/gfx/tilesets/";
                                string name = tileset.TilesetName.ToString();
                                if (File.Exists(destination + tileset.TilesetName.ToString() + ".png"))
                                {
                                    int i = 0;
                                    do
                                    {
                                        i++;
                                    } while (File.Exists(destination + tileset.TilesetName.ToString() + " (" + i.ToString() + ").png"));
                                    destination += tileset.TilesetName.ToString() + " (" + i.ToString() + ").png";
                                    name += " (" + i.ToString() + ")";
                                }
                                else destination += tileset.TilesetName.ToString() + ".png";
                                File.Copy(filename, destination);
                                Game.Tileset set = new Game.Tileset();
                                set.Name = tileset.Name.ToString();
                                set.GraphicName = name;
                                set.ID = Editor.GetFreeTilesetID();
                                int tilecount = 8 * bmp.Height / 32;
                                set.Passabilities = new List<Passability>();
                                set.Priorities = new List<int?>();
                                set.Tags = new List<int?>();
                                for (int i = 0; i < tilecount; i++)
                                {
                                    set.Passabilities.Add(Passability.All);
                                    set.Priorities.Add(0);
                                    set.Tags.Add(null);
                                }
                                Data.Tilesets[set.ID] = set;
                                set.CreateBitmap();
                                if (Editor.MainWindow.DatabaseWidget != null)
                                {
                                    Editor.MainWindow.DatabaseWidget.DBDataList.RefreshList();
                                }
                                data.TilesetIDs = new List<int>() { set.ID };
                                cont();
                            }
                        }
                    }
                };
                // Called whenever a choice has been made for tileset importing.
                cont = new Action(delegate ()
                {
                    if (data.TilesetIDs == null || data.TilesetIDs.Count == 0) // Should not be reachable
                        throw new Exception("Cannot continue without a tileset.");

                    data.Layers = new List<Layer>();

                    bool RemovedAutotiles = false;
                    bool RemovedEvents = map.Events.Length > 0;

                    Table Tiles = map.Data;
                    int XSize = Tiles.XSize.ToInt32();
                    int YSize = Tiles.YSize.ToInt32();
                    int ZSize = Tiles.ZSize.ToInt32();
                    for (int z = 0; z < ZSize; z++)
                    {
                        Layer layer = new Layer($"Layer {z + 1}");
                        for (int y = 0; y < YSize; y++)
                        {
                            for (int x = 0; x < XSize; x++)
                            {
                                int idx = x + y * XSize + z * XSize * YSize;
                                int tileid = Tiles[idx].Convert<Ruby.Integer>();
                                if (tileid < 384) RemovedAutotiles = true;
                                if (tileid == 0) layer.Tiles.Add(null);
                                else layer.Tiles.Add(new TileData() { TileType = TileType.Tileset, Index = 0, ID = tileid - 384 });
                            }
                        }
                        data.Layers.Add(layer);
                    }

                    map.Unpin();
                    tileset.Unpin();

                    Editor.AddMap(data);

                    if (MapIndex < Files.Count - 1)
                    {
                        ImportMap(MapIndex + 1);
                    }
                    else
                    {
                        string Title = "Warning";
                        string Msg = "";
                        if (Files.Count > 1) Msg = "The maps were imported successfully";
                        else Msg = "The map was imported successfully";
                        if (RemovedEvents && RemovedAutotiles) Msg += ", but all events and autotiles have been deleted as these have not yet been implemented in RPG Studio MK.";
                        else if (RemovedEvents) Msg += ", but all events have been deleted as these have not yet been implemented in RPG Studio MK.";
                        else if (RemovedAutotiles) Msg += ", but all autotiles have been deleted as these have not yet been implemented in RPG Studio MK.";
                        else
                        {
                            Title = "Success";
                            Msg += ".";
                        }
                        List<string> options = new List<string>();
                        if (Editor.ProjectSettings.LastMode != "MAPPING") options.Add("Go to Map");
                        options.Add("OK");
                        MessageBox box = new MessageBox(Title, Msg, options, IconType.Info);
                        box.OnButtonPressed += delegate (BaseEventArgs e)
                        {
                            if (options[box.Result] == "Go to Map") // Go to map
                            {
                                Editor.SetMode("MAPPING");
                                Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(data);
                            }
                        };
                        keys.Unpin();
                        infos.Unpin();
                        tilesets.Unpin();
                        Cleanup();
                    }
                });
            };
            ImportMap(0);
        }
    }
}
