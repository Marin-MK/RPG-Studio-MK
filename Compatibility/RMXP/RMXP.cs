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
            IntPtr infofile = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(parent + "/MapInfos.rxdata"), Ruby.String.ToPtr("rb"));
            IntPtr infos = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", infofile);
            Ruby.Pin(infos);
            IntPtr keys = Ruby.Funcall(infos, "keys");
            Ruby.Pin(keys);
            Ruby.Funcall(infofile, "close");
            // Load Tilesets.rxdata
            IntPtr tilesetfile = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(parent + "/Tilesets.rxdata"), Ruby.String.ToPtr("rb"));
            IntPtr tilesets = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", tilesetfile);
            Ruby.Pin(tilesets);
            Ruby.Funcall(tilesetfile, "close");
            Action<int> ImportMap = null;
            ImportMap = delegate (int MapIndex)
            {
                // Convert rxdata (Ruby) to mkd (C#)
                string MapName = Names[MapIndex];
                string file = Files[MapIndex];
                while (file.Contains('\\')) file = file.Replace('\\', '/');
                // Load Map.rxdata
                IntPtr f = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "File"), "open", Ruby.String.ToPtr(file), Ruby.String.ToPtr("rb"));
                IntPtr map = Ruby.Funcall(Ruby.GetConst(Ruby.Object.Class, "Marshal"), "load", f);
                Ruby.Pin(map);
                Ruby.Funcall(f, "close");
                int id = Convert.ToInt32(file.Substring(file.Length - 10, 3));
                // Link Map with its MapInfo
                IntPtr info = IntPtr.Zero;
                for (int i = 0; i < Ruby.Array.Length(keys); i++)
                {
                    if (Ruby.Array.Get(keys, i) == Ruby.Integer.ToPtr(id))
                    {
                        info = Ruby.Funcall(infos, "[]", Ruby.Array.Get(keys, i));
                    }
                }
                if (info == IntPtr.Zero) throw new Exception($"No MapInfo could be found for map ({MapName}).");
                Game.Map data = new Game.Map();
                data.ID = Editor.GetFreeMapID();
                data.DisplayName = MapInfo.Name(info);
                data.DevName = data.DisplayName;
                data.Width = Map.Width(map);
                data.Height = Map.Height(map);
                IntPtr tileset = Ruby.Array.Get(tilesets, Map.TilesetID(map));
                Ruby.Pin(tileset);
                string tilesetname = Tileset.Name(tileset);
                Action cont = null;
                Game.Tileset existing = Data.Tilesets.Find(t => t != null && (t.Name == tilesetname || t.GraphicName == tilesetname));
                bool exist = existing != null;
                string message = $"Importing Map ({MapName})...\n\n";
                if (exist) message += "The tileset of the imported map has the same name as an already-defined tileset in " +
                    $"the database ({existing.Name}).\n" +
                    "Would you like to use this tileset, choose a different one, or import it?";
                else message += $"No tilesets similar to the one used in the imported map ({Tileset.Name(tileset)}) could be found.\n" +
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
                                Ruby.Unpin(tileset);
                                Ruby.Unpin(map);
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
                        string filename = root + "/Graphics/Tilesets/" + Tileset.TilesetName(tileset) + ".png";
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
                                string name = Tileset.TilesetName(tileset);
                                if (File.Exists(destination + Tileset.TilesetName(tileset) + ".png"))
                                {
                                    int i = 0;
                                    do
                                    {
                                        i++;
                                    } while (File.Exists(destination + Tileset.TilesetName(tileset) + " (" + i.ToString() + ").png"));
                                    destination += Tileset.TilesetName(tileset) + " (" + i.ToString() + ").png";
                                    name += " (" + i.ToString() + ")";
                                }
                                else destination += Tileset.TilesetName(tileset) + ".png";
                                File.Copy(filename, destination);
                                Game.Tileset set = new Game.Tileset();
                                set.Name = Tileset.Name(tileset);
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
                    bool RemovedEvents = Ruby.Integer.FromPtr(Ruby.Funcall(Map.Events(map), "length")) > 0;

                    IntPtr Tiles = Map.Data(map);
                    int XSize = Table.XSize(Tiles);
                    int YSize = Table.YSize(Tiles);
                    int ZSize = Table.ZSize(Tiles);
                    IntPtr tiledata = Table.Data(Tiles);
                    for (int z = 0; z < ZSize; z++)
                    {
                        Layer layer = new Layer($"Layer {z + 1}");
                        for (int y = 0; y < YSize; y++)
                        {
                            for (int x = 0; x < XSize; x++)
                            {
                                int idx = x + y * XSize + z * XSize * YSize;
                                int tileid = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(tiledata, idx));
                                if (tileid < 384) RemovedAutotiles = true;
                                if (tileid == 0) layer.Tiles.Add(null);
                                else layer.Tiles.Add(new TileData() { TileType = TileType.Tileset, Index = 0, ID = tileid - 384 });
                            }
                        }
                        data.Layers.Add(layer);
                    }

                    Ruby.Unpin(map);
                    Ruby.Unpin(tileset);

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
                        Ruby.Unpin(keys);
                        Ruby.Unpin(infos);
                        Ruby.Unpin(tileset);
                        Cleanup();
                    }
                });
            };
            ImportMap(0);
        }
    }
}
