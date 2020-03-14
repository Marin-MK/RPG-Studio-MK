using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using ODL;
using RubyDotNET;
using MKEditor.Widgets;
using static RubyDotNET.RubyArray;

namespace MKEditor
{
    public static class Editor
    {
        /// <summary>
        /// Determines whether the user should be warned of unsaved changed before closing.
        /// </summary>
        public static bool UnsavedChanges = false;

        private static Platform? _platform;
        /// <summary>
        /// The current OS.
        /// </summary>
        public static Platform Platform
        {
            get
            {
                if (_platform != null) return (Platform) _platform;
                string p = SDL2.SDL.SDL_GetPlatform();
                if (p == "Windows") _platform = Platform.Windows;
                if (p == "Linux") _platform = Platform.Linux;
                if (p == "Mac OS X") _platform = Platform.MacOS;
                if (p == "iOS") _platform = Platform.IOS;
                if (p == "Android") _platform = Platform.Android;
                return (Platform) _platform;
            }
        }

        /// <summary>
        /// The main Window object for the editor.
        /// </summary>
        public static MainEditorWindow MainWindow;

        /// <summary>
        /// Whether the user is currently has a project open.
        /// </summary>
        public static bool InProject { get { return !string.IsNullOrEmpty(ProjectFilePath); } }

        /// <summary>
        /// The path to the current project's project file.
        /// </summary>
        public static string ProjectFilePath;

        /// <summary>
        /// Settings specific to the currently opened project.
        /// </summary>
        public static ProjectSettings ProjectSettings;

        /// <summary>
        /// General settings for the editor as a whole.
        /// </summary>
        public static GeneralSettings GeneralSettings;



        /// <summary>
        /// Returns the displayed string for the current editor version.
        /// </summary>
        public static string GetVersionString()
        {
            // Changed in Project Settings -> Package -> Package Version (stored in .csproj)
            Assembly assembly = Assembly.GetExecutingAssembly();
            string Version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            if (string.IsNullOrEmpty(Version)) Version = "0.0.1";
            string VersionName = "Version";
            if (Version[0] == '0') VersionName = "Alpha";
            return VersionName + " " + Version;
        }

        /// <summary>
        /// Returns the OperatingSystem object that corresponds with the in-use OS.
        /// </summary>
        public static OperatingSystem GetOperatingSystem()
        {
            return Environment.OSVersion;
        }

        /// <summary>
        /// Initializes Ruby variables to allow Ruby methods to be called.
        /// </summary>
        public static void InitializeRuby()
        {
            if (Internal.Initialized) return;
            Internal.Initialize();

            RubyDotNET.Module mRPG = new RubyDotNET.Module("RPG");
            Table.CreateClass();
            RubyDotNET.Tone.CreateClass();
            RubyDotNET.Color.CreateClass();
            AudioFile.CreateClass();
            Map.CreateClass();
            EventCommand.CreateClass();
            MoveCommand.CreateClass();
            MoveRoute.CreateClass();
            Event.CreateClass();
            Page.CreateClass();
            Condition.CreateClass();
            Graphic.CreateClass();

            Animation.CreateClass();
            Frame.CreateClass();
            Timing.CreateClass();

            RPGSystem.CreateClass();
            Words.CreateClass();
            TestBattler.CreateClass();

            Tileset.CreateClass();

            CommonEvent.CreateClass();

            MapInfo.CreateClass();
        }

        /// <summary>
        /// Closes the currently active project, if existent.
        /// </summary>
        public static void CloseProject()
        {
            if (!InProject) return;
            if (MainWindow.MainEditorWidget != null) MainWindow.MainEditorWidget.Dispose();
            MainWindow.MainEditorWidget = null;
            MainWindow.StatusBar.SetVisible(false);
            MainWindow.ToolBar.SetVisible(false);
            MainWindow.HomeScreen = new HomeScreen(MainWindow.MainGridLayout);
            MainWindow.HomeScreen.SetGridRow(3);
            MainWindow.MainGridLayout.Rows[1] = new GridSize(0, Unit.Pixels);
            MainWindow.MainGridLayout.Rows[4] = new GridSize(0, Unit.Pixels);
            MainWindow.MainGridLayout.Rows[5] = new GridSize(0, Unit.Pixels);
            MainWindow.MainGridLayout.UpdateContainers();
            MainWindow.MainGridLayout.UpdateLayout();
            Game.Data.ClearProjectData();
            ClearProjectData();
        }

        /// <summary>
        /// Sets in motion the process of importing maps.
        /// </summary>
        public static void ImportMaps()
        {
            OpenFile of = new OpenFile();
            of.SetFilters(new List<FileFilter>()
            {
                new FileFilter("RPG Maker XP Map", "rxdata")
            });
            of.SetTitle("Pick map(s)");
            of.SetAllowMultiple(true);
            object ret = of.Show();
            List<string> Files = new List<string>();
            if (ret is string) Files.Add(ret as string);
            else if (ret is List<string>) Files = ret as List<string>;
            else return; // No files picked
            InitializeRuby();
            string[] folders = Files[0].Split('\\');
            string parent = "";
            string root = "";
            for (int i = 0; i < folders.Length - 1; i++)
            {
                parent += folders[i];
                if (i != folders.Length - 2) root += folders[i];
                if (i != folders.Length - 2) parent += '\\';
                if (i != folders.Length - 3) root += '\\';
            }
            List<string> Names = new List<string>();
            foreach (string f in Files)
            {
                string[] l = f.Split('\\').Last().Split('.');
                string n = "";
                for (int i = 0; i < l.Length - 1; i++)
                {
                    n += l[i];
                    if (i != l.Length - 2) n += '.';
                }
                Names.Add(n);
            }
            // Load MapInfos.rxdata
            RubyFile infofile = RubyFile.Open(parent + "\\MapInfos.rxdata", "rb");
            RubyHash infos = RubyMarshal.Load<RubyHash>(infofile);
            RubyArray keys = infos.Keys;
            infofile.Close();
            infofile.Free();
            // Load Tilesets.rxdata
            RubyFile tilesetfile = RubyFile.Open(parent + "\\Tilesets.rxdata", "rb");
            RubyArray tilesets = RubyMarshal.Load<RubyArray>(tilesetfile);
            tilesetfile.Close();
            tilesetfile.Free();
            Action<int> ImportMap = null;
            ImportMap = delegate (int MapIndex)
            {
                // Convert rxdata (Ruby) to mkd (C#)
                string MapName = Names[MapIndex];
                string file = Files[MapIndex];
                while (file.Contains('\\')) file = file.Replace('\\', '/');
                // Load Map.rxdata
                RubyFile f = RubyFile.Open(file, "rb");
                RubyDotNET.Map map = RubyMarshal.Load<RubyDotNET.Map>(f);
                f.Close();
                f.Free();
                int id = Convert.ToInt32(file.Substring(file.Length - 10, 3));
                // Link Map with its MapInfo
                MapInfo info = null;
                keys.EachWithIndex((obj, idx) =>
                {
                    if (obj.Pointer != Internal.QNil && obj.Convert<RubyInt>().ToInt32() == id)
                        info = infos[keys[idx]].Convert<MapInfo>();
                });
                if (info == null)
                    throw new Exception($"No MapInfo could be found for map ({MapName}).");
                Game.Map data = new Game.Map();
                data.ID = GetFreeMapID();
                data.DisplayName = info.Name.ToString();
                data.DevName = data.DisplayName;
                data.Width = map.Width.ToInt32();
                data.Height = map.Height.ToInt32();
                RubyDotNET.Tileset tileset = tilesets[map.TilesetID.ToInt32()].Convert<RubyDotNET.Tileset>();
                string tilesetname = tileset.Name.ToString();
                Action cont = null;
                Game.Tileset existing = Game.Data.Tilesets.Find(t => t != null && (t.Name == tilesetname || t.GraphicName == tilesetname));
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
                box.OnButtonPressed += delegate (object sender, EventArgs e)
                {
                    if (Options[box.Result] == "Use this") // Use the matched tileset
                    {
                        data.TilesetIDs = new List<int>() { existing.ID };
                        cont();
                    }
                    else if (Options[box.Result] == "Pick other") // Pick other tileset
                    {
                        TilesetPicker picker = new TilesetPicker(null, MainWindow);
                        picker.OnClosed += delegate (object sender, EventArgs e)
                        {
                            if (picker.ChosenTilesetID > 0) // Chose tileset
                            {
                                data.TilesetIDs = new List<int>() { picker.ChosenTilesetID };
                                cont();
                            }
                            else // Didn't pick tileset; cancel importing
                            {
                                data = null;
                                tileset.Free();
                                map.Free();
                                MessageBox b = new MessageBox("Warning", $"Importing Map ({MapName})...\n\nAs no tileset was chosen, this map will not be imported.", IconType.Warning);
                                b.OnButtonPressed += delegate (object sender, EventArgs e)
                                {
                                    if (MapIndex < Files.Count - 1) ImportMap(MapIndex + 1);
                                };
                            }
                        };
                    }
                    else if (Options[box.Result] == "Import it") // Import the tileset
                    {
                        string filename = root + "\\Graphics\\Tilesets\\" + tileset.TilesetName.ToString() + ".png";
                        if (!File.Exists(filename)) // Graphic doesn't exist
                        {
                            MessageBox b = new MessageBox("Error", $"Importing Map ({MapName})...\n\nThe tileset graphic could not be found. The tileset cannot be imported, and thus this map will not be imported.", IconType.Error);
                            b.OnButtonPressed += delegate (object sender, EventArgs e)
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
                                b.OnButtonPressed += delegate (object sender, EventArgs e)
                                {
                                    if (MapIndex < Files.Count - 1) ImportMap(MapIndex + 1);
                                };
                            }
                            else
                            {
                                string destination = Game.Data.ProjectPath + "/gfx/tilesets/";
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
                                set.ID = GetFreeTilesetID();
                                int tilecount = 8 * bmp.Height / 32;
                                set.Passabilities = new List<Game.Passability>();
                                set.Priorities = new List<int?>();
                                set.Tags = new List<int?>();
                                for (int i = 0; i < tilecount; i++)
                                {
                                    set.Passabilities.Add(Game.Passability.All);
                                    set.Priorities.Add(0);
                                    set.Tags.Add(null);
                                }
                                Game.Data.Tilesets[set.ID] = set;
                                set.CreateBitmap();
                                if (MainWindow.DatabaseWidget != null)
                                {
                                    MainWindow.DatabaseWidget.DBDataList.RefreshList();
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

                    data.Layers = new List<Game.Layer>();

                    bool RemovedAutotiles = false;
                    bool RemovedEvents = map.Events.Length > 0;

                    Table Tiles = map.Data.Convert<Table>();
                    int XSize = Tiles.XSize.ToInt32();
                    int YSize = Tiles.YSize.ToInt32();
                    int ZSize = Tiles.ZSize.ToInt32();
                    for (int z = 0; z < ZSize; z++)
                    {
                        Game.Layer layer = new Game.Layer($"Layer {z + 1}");
                        for (int y = 0; y < YSize; y++)
                        {
                            for (int x = 0; x < XSize; x++)
                            {
                                int idx = x + y * XSize + z * XSize * YSize;
                                int tileid = Tiles.Data[idx].Convert<RubyInt>().ToInt32();
                                if (tileid < 384) RemovedAutotiles = true;
                                if (tileid == 0) layer.Tiles.Add(null);
                                else layer.Tiles.Add(new Game.TileData() { TileType = Game.TileType.Tileset, Index = 0, ID = tileid - 384 });
                            }
                        }
                        data.Layers.Add(layer);
                    }

                    map.Free();
                    tileset.Free();

                    AddMap(data);

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
                        if (ProjectSettings.LastMode != "MAPPING") options.Add("Go to Map");
                        options.Add("OK");
                        MessageBox box = new MessageBox(Title, Msg, options, IconType.Info);
                        box.OnButtonPressed += delegate (object sender, EventArgs e)
                        {
                            if (options[box.Result] == "Go to Map") // Go to map
                            {
                                SetMode("MAPPING");
                                MainWindow.MapWidget.MapSelectPanel.SetMap(data);
                            }
                        };
                        infos.Free();
                        keys.Free();
                    }
                });
            };
            ImportMap(0);
        }

        /// <summary>
        /// Starts or stops all map animations.
        /// </summary>
        public static void ToggleMapAnimations()
        {
            GeneralSettings.ShowMapAnimations = !GeneralSettings.ShowMapAnimations;
            MainWindow.MapWidget.SetMapAnimations(GeneralSettings.ShowMapAnimations);
        }

        /// <summary>
        /// Returns the first unused map ID for the current project.
        /// </summary>
        /// <returns></returns>
        public static int GetFreeMapID()
        {
            int i = 1;
            while (true)
            {
                if (!Game.Data.Maps.ContainsKey(i))
                {
                    return i;
                }
                i++;
            }
        }

        /// <summary>
        /// Returns the first unused tileset ID for the current project.
        /// </summary>
        /// <returns></returns>
        public static int GetFreeTilesetID()
        {
            int i = 1;
            while (true)
            {
                if (Game.Data.Tilesets[i] == null)
                {
                    return i;
                }
                i++;
            }
        }

        /// <summary>
        /// Adds a Map to the map list.
        /// </summary>
        /// <param name="Map">The new Map object.</param>
        /// <param name="ParentID">The ID of the parent map.</param>
        public static void AddMap(Game.Map Map, int ParentID = 0)
        {
            Game.Data.Maps.Add(Map.ID, Map);
            if (ParentID != 0) AddIDToMap(ProjectSettings.MapOrder, ParentID, Map.ID);
            else ProjectSettings.MapOrder.Add(Map.ID);
            TreeNode node = new TreeNode() { Name = Map.DevName, Object = Map.ID };
            if (MainWindow.MapWidget != null)
            {
                TreeView mapview = MainWindow.MapWidget.MapSelectPanel.mapview;
                if (mapview.HoveringNode != null)
                {
                    mapview.HoveringNode.Nodes.Add(node);
                    mapview.HoveringNode.Collapsed = false;
                }
                else
                {
                    mapview.Nodes.Add(node);
                }
                mapview.SetSelectedNode(node);
            }
        }

        /// <summary>
        /// Adss a Map to the project's map order.
        /// </summary>
        /// <param name="collection">The collection to add the map to.</param>
        /// <param name="ParentID">The parent ID of the map.</param>
        /// <param name="ChildID">The child ID of the map.</param>
        public static bool AddIDToMap(List<object> collection, int ParentID, int ChildID)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                object o = collection[i];
                if (o is bool) continue;
                else if (o is int)
                {
                    if ((int)o == ParentID)
                    {
                        if (i == 0) // Already in this parent's node list
                            collection.Add(ChildID);
                        else // Create new node list
                            collection[i] = new List<object>() { ParentID, false, ChildID };
                        return true;
                    }
                }
                else
                {
                    if (AddIDToMap((List<object>)o, ParentID, ChildID)) break;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a new, blank project.
        /// </summary>
        public static void NewProject()
        {
            new MessageBox("Oops!", "This feature has not been implemented yet.\nTo get started, please use the \"Open Project\" feature and choose the MK Starter Kit.", IconType.Error);
        }

        /// <summary>
        /// Allows the user to pick a project file.
        /// </summary>
        public static void OpenProject()
        {
            OpenFile of = new OpenFile();
            of.SetFilters(new List<FileFilter>()
            {
                new FileFilter("MK Project File", "mkproj")
            });
            string lastfolder = "";
            if (GeneralSettings.RecentFiles.Count > 0)
            {
                string path = GeneralSettings.RecentFiles[0][1];
                while (path.Contains("/")) path = path.Replace("/", "\\");
                List<string> folders = path.Split('\\').ToList();
                for (int i = 0; i < folders.Count - 1; i++)
                {
                    lastfolder += folders[i];
                    if (i != folders.Count - 2) lastfolder += "\\";
                }
            }
            of.SetInitialDirectory(lastfolder);
            of.SetTitle("Choose a project file...");
            string result = of.Show() as string;
            if (!string.IsNullOrEmpty(result))
            {
                CloseProject();
                Game.Data.SetProjectPath(result);
                MainWindow.CreateEditor();
                MakeRecentProject();
            }
        }

        /// <summary>
        /// Saves the current project.
        /// </summary>
        public static void SaveProject()
        {
            if (!InProject) return;
            MainWindow.StatusBar.QueueMessage("Saving project...");
            Graphics.UpdateGraphics(); // Overrides default Logic/Visual update loop by immediately updating just the graphics.
            DateTime t1 = DateTime.Now;
            DumpProjectSettings();
            Game.Data.SaveTilesets();
            Game.Data.SaveAutotiles();
            Game.Data.SaveMaps();
            Game.Data.SaveSpecies();
            UnsavedChanges = false;
            long time = (long) Math.Round((DateTime.Now - t1).TotalMilliseconds);
            MainWindow.StatusBar.QueueMessage($"Saved project ({time}ms)", true);
        }

        /// <summary>
        /// Runs the current project.
        /// </summary>
        public static void StartGame()
        {
            MainWindow.StatusBar.QueueMessage("Game starting...", true);
            Process.Start(Game.Data.ProjectPath + "/mkxp.exe");
        }

        /// <summary>
        /// Opens the game folder corresponding with the current project.
        /// </summary>
        public static void OpenGameFolder()
        {
            Utilities.OpenFolder(Game.Data.ProjectPath);
        }

        /// <summary>
        /// Quits the editor entirely.
        /// </summary>
        public static void ExitEditor()
        {
            MainWindow.Dispose();
        }

        /// <summary>
        /// Changes the active mode of the editor.
        /// </summary>
        /// <param name="Mode">The mode to switch to. MAPPING, EVENTING, SCRIPTING or DATABASE.</param>
        /// <param name="Force">Whether or not to force a full redraw.</param>
        public static void SetMode(string Mode, bool Force = false)
        {
            if (Mode == ProjectSettings.LastMode && !Force) return;
            if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();
            MainWindow.MainEditorWidget = null;

            string OldMode = ProjectSettings.LastMode;
            ProjectSettings.LastMode = Mode;

            MainWindow.StatusBar.SetVisible(true);
            MainWindow.ToolBar.SetVisible(true);

            if (Mode == "MAPPING") // Select Mapping mode
            {
                MainWindow.ToolBar.MappingMode.SetSelected(true, Force);

                MainWindow.MainEditorWidget = new MappingWidget(MainWindow.MainGridLayout);
                MainWindow.MainEditorWidget.SetGridRow(3);

                // Set list of maps & initial map
                MainWindow.MapWidget.MapSelectPanel.PopulateList(Editor.ProjectSettings.MapOrder, true);

                int mapid = ProjectSettings.LastMapID;
                if (!Game.Data.Maps.ContainsKey(mapid))
                {
                    if (ProjectSettings.MapOrder[0] is List<object>) mapid = (int)((List<object>) ProjectSettings.MapOrder[0])[0];
                    else mapid = (int) ProjectSettings.MapOrder[0];
                }
                int lastlayer = ProjectSettings.LastLayer;
                MainWindow.MapWidget.MapSelectPanel.SetMap(Game.Data.Maps[mapid]);

                MainWindow.MapWidget.SetSelectedLayer(lastlayer);
                MainWindow.MapWidget.SetZoomFactor(ProjectSettings.LastZoomFactor);
                MainWindow.MapWidget.SetSubmode(ProjectSettings.LastMappingSubmode);
            }
            else if (OldMode == "MAPPING") // Deselct Mapping mode
            {
                
            }
            if (Mode == "EVENTING") // Select Eventing mode
            {
                MainWindow.ToolBar.EventingMode.SetSelected(true, Force);
            }
            else if (OldMode == "EVENTING") // Deselect Eventing mode
            {

            }
            if (Mode == "SCRIPTING") // Select Scripting mode
            {
                MainWindow.ToolBar.ScriptingMode.SetSelected(true, Force);
            }
            else if (OldMode == "SCRIPTING") // Deselect Script mode
            {

            }
            if (Mode == "DATABASE") // Select Database mode
            {
                MainWindow.ToolBar.DatabaseMode.SetSelected(true, Force);
                MainWindow.MainEditorWidget = new DatabaseWidget(MainWindow.MainGridLayout);
                MainWindow.MainEditorWidget.SetGridRow(3);
            }
            else if (OldMode == "DATABASE") // Deselect Database mode
            {

            }
            MainWindow.MainGridLayout.UpdateLayout();
            MainWindow.StatusBar.Refresh();
            MainWindow.ToolBar.Refresh();
        }

        /// <summary>
        /// Adds the current project to the list of recently opened projects.
        /// </summary>
        public static void MakeRecentProject()
        {
            for (int i = 0; i < GeneralSettings.RecentFiles.Count; i++)
            {
                if (GeneralSettings.RecentFiles[i][1] == ProjectFilePath) // Project file paths match - same project
                {
                    GeneralSettings.RecentFiles.RemoveAt(i);
                }
            }
            GeneralSettings.RecentFiles.Add(new List<string>() { ProjectSettings.ProjectName, ProjectFilePath });
        }

        /// <summary>
        /// Saves the editor's general settings.
        /// </summary>
        public static void DumpGeneralSettings()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("editor.mkd", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, GeneralSettings);
            stream.Close();
        }

        /// <summary>
        /// Loads the editor's general settings.
        /// </summary>
        public static void LoadGeneralSettings()
        {
            if (File.Exists("editor.mkd"))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("editor.mkd", FileMode.Open, FileAccess.Read);
                GeneralSettings = formatter.Deserialize(stream) as GeneralSettings;
                stream.Close();
            }
            else
            {
                GeneralSettings = new GeneralSettings();
            }
            if (GeneralSettings.LastWidth < MainWindow.MinimumSize.Width) GeneralSettings.LastWidth = MainWindow.MinimumSize.Width;
            if (GeneralSettings.LastHeight < MainWindow.MinimumSize.Height) GeneralSettings.LastHeight = MainWindow.MinimumSize.Height;
            if (GeneralSettings.LastX < 0) GeneralSettings.LastX = 0;
            if (GeneralSettings.LastY < 0) GeneralSettings.LastY = 0;
        }

        /// <summary>
        /// Saves the current project's settings.
        /// </summary>
        public static void DumpProjectSettings()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(ProjectFilePath, FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, ProjectSettings);
            stream.Close();
        }

        /// <summary>
        /// Loads the current project's settings.
        /// </summary>
        public static void LoadProjectSettings()
        {
            if (File.Exists(ProjectFilePath))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(ProjectFilePath, FileMode.Open, FileAccess.Read);
                ProjectSettings = formatter.Deserialize(stream) as ProjectSettings;
                stream.Close();
            }
            else
            {
                ProjectSettings = new ProjectSettings();
            }
            if (ProjectSettings.LastZoomFactor == 0) ProjectSettings.LastZoomFactor = 1;
            if (ProjectSettings.ProjectName.Length == 0) ProjectSettings.ProjectName = "Untitled Game";
            if (string.IsNullOrEmpty(ProjectSettings.LastMode)) ProjectSettings.LastMode = "MAPPING";
            if (string.IsNullOrEmpty(ProjectSettings.LastMappingSubmode)) ProjectSettings.LastMappingSubmode = "TILES";
            if (ProjectSettings.TilesetCapacity == 0) ProjectSettings.TilesetCapacity = 25;
            if (ProjectSettings.AutotileCapacity == 0) ProjectSettings.AutotileCapacity = 25;
        }

        /// <summary>
        /// Clears settings related to the current project. Usually only called after saving and closing a project.
        /// </summary>
        public static void ClearProjectData()
        {
            ProjectFilePath = null;
            ProjectSettings = null;
            UnsavedChanges = false;
        }
    }

    [Serializable]
    public class ProjectSettings
    {
        /// <summary>
        /// The hierarchy of maps as seen in the map list.
        /// </summary>
        public List<object> MapOrder = new List<object>();
        /// <summary>
        /// The name of the project.
        /// </summary>
        public string ProjectName = "Untitled Game";
        /// <summary>
        /// The last-active mode of the project.
        /// </summary>
        public string LastMode = "MAPPING";
        /// <summary>
        /// The last-active submode within the Mapping mode.
        /// </summary>
        public string LastMappingSubmode = "TILES";
        /// <summary>
        /// The last selected Map.
        /// </summary>
        public int LastMapID = 1;
        /// <summary>
        /// The last selected layer.
        /// </summary>
        public int LastLayer = 1;
        /// <summary>
        /// The last zoom factor.
        /// </summary>
        public double LastZoomFactor = 1;
        /// <summary>
        /// The maximum tileset capacity.
        /// </summary>
        public int TilesetCapacity = 25;
        /// <summary>
        /// The maximum autotile capacity.
        /// </summary>
        public int AutotileCapacity = 25;
    }

    [Serializable]
    public class GeneralSettings
    {
        /// <summary>
        /// Whether the editor window was maximized.
        /// </summary>
        public bool WasMaximized = true;
        /// <summary>
        /// The last width the editor window had when it was not maximized.
        /// </summary>
        public int LastWidth = 600;
        /// <summary>
        /// The last height the editor window had when it was not maximized.
        /// </summary>
        public int LastHeight = 600;
        /// <summary>
        /// The last X position the editor window had when it was not maximized.
        /// </summary>
        public int LastX = 50;
        /// <summary>
        /// The last Y position the editor window had when it was not maximized.
        /// </summary>
        public int LastY = 50;
        /// <summary>
        /// The list of recently opened projects. May contain old/invalid paths.
        /// </summary>
        public List<List<string>> RecentFiles = new List<List<string>>();
        /// <summary>
        /// Whether to play map animations.
        /// </summary>
        public bool ShowMapAnimations = true;
    }

    public enum Platform
    {
        Unknown,
        Windows,
        Linux,
        MacOS,
        IOS,
        Android
    }
}
