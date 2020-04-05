using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using ODL;
using MKEditor.Game;
using MKEditor.Widgets;

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
        /// Contains the list of recent actions that you made that you can undo.
        /// </summary>
        public static List<BaseUndoAction> MapUndoList = new List<BaseUndoAction>();

        /// <summary>
        /// Contains the list of recent actions that you undid that you can redo.
        /// </summary>
        public static List<BaseUndoAction> MapRedoList = new List<BaseUndoAction>();

        /// <summary>
        /// Whether or not undo/redo is currently usable. Disable while drawing tiles in map editor, for instance.
        /// </summary>
        public static bool CanUndo = true;

        /// <summary>
        /// A list of maps that were deleted this sessions. Allows you to restore a map.
        /// </summary>
        public static List<Map> DeletedMaps = new List<Map>();



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
        /// Undoes the latest change you made.
        /// </summary>
        public static void Undo()
        {
            if (MapUndoList.Count > 0 && CanUndo) MapUndoList[MapUndoList.Count - 1].RevertTo(false);
        }

        /// <summary>
        /// Redoes the latest change that you undid.
        /// </summary>
        public static void Redo()
        {
            if (MapRedoList.Count > 0 && CanUndo) MapRedoList[MapRedoList.Count - 1].RevertTo(true);
        }

        /// <summary>
        /// Initializes Ruby variables to allow Ruby methods to be called.
        /// </summary>
        public static void InitializeRuby()
        {
            if (RubyDotNET.Internal.Initialized) return;
            RubyDotNET.Internal.Initialize();

            RubyDotNET.Module mRPG = new RubyDotNET.Module("RPG");
            RubyDotNET.Table.CreateClass();
            RubyDotNET.Tone.CreateClass();
            RubyDotNET.Color.CreateClass();
            RubyDotNET.AudioFile.CreateClass();
            RubyDotNET.Map.CreateClass();
            RubyDotNET.EventCommand.CreateClass();
            RubyDotNET.MoveCommand.CreateClass();
            RubyDotNET.MoveRoute.CreateClass();
            RubyDotNET.Event.CreateClass();
            RubyDotNET.Page.CreateClass();
            RubyDotNET.Condition.CreateClass();
            RubyDotNET.Graphic.CreateClass();

            RubyDotNET.Animation.CreateClass();
            RubyDotNET.Frame.CreateClass();
            RubyDotNET.Timing.CreateClass();

            RubyDotNET.RPGSystem.CreateClass();
            RubyDotNET.Words.CreateClass();
            RubyDotNET.TestBattler.CreateClass();

            RubyDotNET.Tileset.CreateClass();

            RubyDotNET.CommonEvent.CreateClass();

            RubyDotNET.MapInfo.CreateClass();
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
            Data.ClearProjectData();
            ClearProjectData();
        }

        /// <summary>
        /// Closes and reopens the project.
        /// </summary>
        public static void ReloadProject()
        {
            string projectfile = Data.ProjectFilePath;
            CloseProject();
            Data.SetProjectPath(projectfile);
            MainWindow.CreateEditor();
            MakeRecentProject();
        }

        /// <summary>
        /// Sets in motion the process of importing maps.
        /// </summary>
        public static void ImportMaps()
        {
            OpenFileDialog of = new OpenFileDialog();
            of.SetFilter(new FileFilter("RPG Maker XP Map", "rxdata"));
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
            RubyDotNET.RubyFile infofile = RubyDotNET.RubyFile.Open(parent + "\\MapInfos.rxdata", "rb");
            RubyDotNET.RubyHash infos = RubyDotNET.RubyMarshal.Load<RubyDotNET.RubyHash>(infofile);
            RubyDotNET.RubyArray keys = infos.Keys;
            infofile.Close();
            infofile.Free();
            // Load Tilesets.rxdata
            RubyDotNET.RubyFile tilesetfile = RubyDotNET.RubyFile.Open(parent + "\\Tilesets.rxdata", "rb");
            RubyDotNET.RubyArray tilesets = RubyDotNET.RubyMarshal.Load<RubyDotNET.RubyArray>(tilesetfile);
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
                RubyDotNET.RubyFile f = RubyDotNET.RubyFile.Open(file, "rb");
                RubyDotNET.Map map = RubyDotNET.RubyMarshal.Load<RubyDotNET.Map>(f);
                f.Close();
                f.Free();
                int id = Convert.ToInt32(file.Substring(file.Length - 10, 3));
                // Link Map with its MapInfo
                RubyDotNET.MapInfo info = null;
                keys.EachWithIndex((obj, idx) =>
                {
                    if (obj.Pointer != RubyDotNET.Internal.QNil && obj.Convert<RubyDotNET.RubyInt>().ToInt32() == id)
                        info = infos[keys[idx]].Convert<RubyDotNET.MapInfo>();
                });
                if (info == null)
                    throw new Exception($"No MapInfo could be found for map ({MapName}).");
                Map data = new Map();
                data.ID = GetFreeMapID();
                data.DisplayName = info.Name.ToString();
                data.DevName = data.DisplayName;
                data.Width = map.Width.ToInt32();
                data.Height = map.Height.ToInt32();
                RubyDotNET.Tileset tileset = tilesets[map.TilesetID.ToInt32()].Convert<RubyDotNET.Tileset>();
                string tilesetname = tileset.Name.ToString();
                Action cont = null;
                Tileset existing = Data.Tilesets.Find(t => t != null && (t.Name == tilesetname || t.GraphicName == tilesetname));
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
                                tileset.Free();
                                map.Free();
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
                        string filename = root + "\\Graphics\\Tilesets\\" + tileset.TilesetName.ToString() + ".png";
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
                                Tileset set = new Tileset();
                                set.Name = tileset.Name.ToString();
                                set.GraphicName = name;
                                set.ID = GetFreeTilesetID();
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

                    data.Layers = new List<Layer>();

                    bool RemovedAutotiles = false;
                    bool RemovedEvents = map.Events.Length > 0;

                    RubyDotNET.Table Tiles = map.Data.Convert<RubyDotNET.Table>();
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
                                int tileid = Tiles.Data[idx].Convert<RubyDotNET.RubyInt>().ToInt32();
                                if (tileid < 384) RemovedAutotiles = true;
                                if (tileid == 0) layer.Tiles.Add(null);
                                else layer.Tiles.Add(new TileData() { TileType = TileType.Tileset, Index = 0, ID = tileid - 384 });
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
                        box.OnButtonPressed += delegate (BaseEventArgs e)
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
        /// Allows the user to restore a map that was deleted during this session.
        /// </summary>
        public static void RestoreMap()
        {
            if (DeletedMaps.Count == 0)
            {
                new MessageBox("Info", "You have not deleted any maps during this session. If you delete a map, it will be available for restoration here for the rest of the session, or until you clear the deleted map cache.", IconType.Info);
            }
            else
            {
                MapPicker picker = new MapPicker(DeletedMaps, "Restore a Map", false);
                picker.OnClosed += delegate (BaseEventArgs e)
                {
                    if (picker.ChosenMap != null)
                    {
                        bool NewID = false;
                        Map Map = picker.ChosenMap;
                        if (Data.Maps.ContainsKey(picker.ChosenMap.ID)) // ID in use, generate new ID
                        {
                            Map.ID = GetFreeMapID();
                            NewID = true;
                        }
                        DeletedMaps.Remove(Map);
                        AddMap(Map);
                        if (NewID)
                        {
                            new MessageBox("Warning", "The ID of the restored map was already in use. It has been assigned a new ID. This may affect map connections or transfer commands.", IconType.Warning);
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Clears the map cache for deleted but restore-able maps.
        /// </summary>
        public static void ClearMapCache()
        {
            MessageBox box = new MessageBox("Warning", "You are about to clear the internal deleted map cache. " +
                "This means that all maps that you have deleted during this session will be permanently lost. " +
                "Would you like to continue and clear the cache?", ButtonType.YesNoCancel, IconType.Warning);
            box.OnClosed += delegate (BaseEventArgs e)
            {
                if (box.Result == 0) // Yes
                {
                    DeletedMaps.Clear();
                    new MessageBox("Info", "The deleted map cache has been cleared.", ButtonType.OK, IconType.Info);
                }
            };
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
        /// Shows or hides the map grid overlay.
        /// </summary>
        public static void ToggleGrid()
        {
            GeneralSettings.ShowGrid = !GeneralSettings.ShowGrid;
            MainWindow.MapWidget.SetGridVisibility(GeneralSettings.ShowGrid);
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
                if (!Data.Maps.ContainsKey(i))
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
                if (Data.Tilesets[i] == null)
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
        public static void AddMap(Map Map, int ParentID = 0)
        {
            Data.Maps.Add(Map.ID, Map);
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
        /// Adds a Map to the project's map order.
        /// </summary>
        /// <param name="collection">The collection to add the map to.</param>
        /// <param name="ParentID">The parent ID of the map.</param>
        /// <param name="ChildID">The child ID of the map.</param>
        public static bool AddIDToMap(List<object> collection, int ParentID, object ChildData, bool first = true)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                object o = collection[i];
                if (o is bool) continue;
                else if (o is int)
                {
                    if ((int) o == ParentID)
                    {
                        if (i == 0 && !first) // Already in this parent's node list
                            collection.Add(ChildData);
                        else // Create new node list
                            collection[i] = new List<object>() { ParentID, false, ChildData };
                        return true;
                    }
                }
                else
                {
                    if (AddIDToMap((List<object>) o, ParentID, ChildData, false)) break;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes and returns the object in the map order corresponding with the map ID.
        /// </summary>
        /// <param name="MapID">The ID of the map to return.</param>
        public static object RemoveIDFromOrder(List<object> parentcollection, List<object> collection, int MapID)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                object o = collection[i];
                if (o is bool) continue;
                if (o is List<object>)
                {
                    object ret = RemoveIDFromOrder(collection, (List<object>) o, MapID);
                    if (ret != null) return ret;
                }
                else if (i == 0 && (int) o == MapID)
                {
                    if (parentcollection == null)
                    {
                        collection.RemoveAt(i);
                        return MapID;
                    }
                    parentcollection.Remove(collection);
                    return collection;
                }
                else if ((int) o == MapID)
                {
                    if (collection.Count == 3 && i == 2 && parentcollection != null)
                    {
                        int Idx = parentcollection.IndexOf(collection);
                        parentcollection.Remove(collection);
                        parentcollection.Insert(Idx, collection[0]);
                    }
                    collection.RemoveAt(i);
                    return o;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if the parent map has the child map as a child.
        /// </summary>
        public static bool MapIsChildMap(List<object> collection, int ParentID, int ChildID, bool First = true, bool FoundParent = false)
        {
            for (int i = (First ? 0 : 2); i < collection.Count; i++)
            {
                object o = collection[i];
                if (o is int)
                {
                    if ((int) o == ParentID) return false;
                    if ((int) o == ChildID) return FoundParent;
                }
                else if (o is List<object>)
                {
                    List<object> newlist = (List<object>) o;
                    if ((int) newlist[0] == ParentID) return MapIsChildMap(newlist, ParentID, ChildID, false, true);
                    else if (MapIsChildMap(newlist, ParentID, ChildID, false, false)) return true;
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
            OpenFileDialog of = new OpenFileDialog();
            of.SetFilter(new FileFilter("MK Project File", "mkproj"));
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
                if (!result.EndsWith(".mkproj"))
                    new MessageBox("Error", "Invalid project file.", ButtonType.OK, IconType.Error);
                else
                {
                    CloseProject();
                    Data.SetProjectPath(result);
                    MainWindow.CreateEditor();
                    MakeRecentProject();
                }
            }
        }

        /// <summary>
        /// Saves the current project.
        /// </summary>
        public static void SaveProject()
        {
            if (!InProject) return;
            if (MainWindow != null)
            {
                MainWindow.StatusBar.QueueMessage("Saving project...");
                Graphics.UpdateGraphics(); // Overrides default Logic/Visual update loop by immediately updating just the graphics.
            }
            DateTime t1 = DateTime.Now;
            DumpProjectSettings();
            Data.SaveTilesets();
            Data.SaveAutotiles();
            Data.SaveMaps();
            Data.SaveSpecies();
            UnsavedChanges = false;
            if (MainWindow != null)
            {
                long time = (long) Math.Round((DateTime.Now - t1).TotalMilliseconds);
                MainWindow.StatusBar.QueueMessage($"Saved project ({time}ms)", true);
            }
        }

        /// <summary>
        /// Runs the current project.
        /// </summary>
        public static void StartGame()
        {
            MainWindow.StatusBar.QueueMessage("Game starting...", true);
            Process.Start(Data.ProjectPath + "/mkxp.exe");
        }

        /// <summary>
        /// Opens the game folder corresponding with the current project.
        /// </summary>
        public static void OpenGameFolder()
        {
            Utilities.OpenFolder(Data.ProjectPath);
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
                List<TreeNode> Nodes = MainWindow.MapWidget.MapSelectPanel.PopulateList(Editor.ProjectSettings.MapOrder, true);
                GenerateMapOrder(Nodes);

                int mapid = ProjectSettings.LastMapID;
                if (!Data.Maps.ContainsKey(mapid))
                {
                    if (ProjectSettings.MapOrder[0] is List<object>) mapid = (int)((List<object>) ProjectSettings.MapOrder[0])[0];
                    else mapid = (int) ProjectSettings.MapOrder[0];
                }
                int lastlayer = ProjectSettings.LastLayer;
                MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[mapid]);

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
        /// Generates a new MapOrder list based on the existing nodes in the map list.
        /// </summary>
        public static List<object> GenerateMapOrder(List<TreeNode> Nodes, bool Recursive = false)
        {
            List<object> List = new List<object>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                TreeNode n = Nodes[i];
                if (n.Nodes.Count > 0)
                {
                    List<object> sublist = new List<object>() { (int) n.Object, n.Collapsed };
                    sublist.AddRange(GenerateMapOrder(n.Nodes, true));
                    List.Add(sublist);
                }
                else
                {
                    List.Add((int) n.Object);
                }
            }
            if (!Recursive) // First call
            {
                Editor.ProjectSettings.MapOrder = List;
            }
            return List;
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
            if (MainWindow != null && GeneralSettings.LastWidth < MainWindow.MinimumSize.Width) GeneralSettings.LastWidth = MainWindow.MinimumSize.Width;
            if (MainWindow != null && GeneralSettings.LastHeight < MainWindow.MinimumSize.Height) GeneralSettings.LastHeight = MainWindow.MinimumSize.Height;
            if (GeneralSettings.LastX < 0) GeneralSettings.LastX = 0;
            if (GeneralSettings.LastY < 0) GeneralSettings.LastY = 0;
        }

        /// <summary>
        /// Saves the current project's settings.
        /// </summary>
        public static void DumpProjectSettings()
        {
            // Saves the assembly version into the project file.
            ProjectSettings.SavedVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
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
            MapUndoList.Clear();
            MapRedoList.Clear();
            DeletedMaps.Clear();
            UnsavedChanges = false;
        }
    }

    [Serializable]
    public class ProjectSettings
    {
        /// <summary>
        /// The last-used version of the editor to save the project. Can be used to programmatically port old data formats to new formats upon an update.
        /// </summary>
        public FileVersionInfo SavedVersion;
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
        /// <summary>
        /// Whether to show the map grid overlay.
        /// </summary>
        public bool ShowGrid = true;
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
