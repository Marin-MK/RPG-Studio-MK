using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using odl;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;
using amethyst;

namespace RPGStudioMK
{
    public static class Editor
    {
        /// <summary>
        /// Determines whether the user should be warned of unsaved changed before closing.
        /// </summary>
        public static bool UnsavedChanges = false;

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
            if (string.IsNullOrEmpty(Version)) Version = "0.0.0";
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

        public static void InitializeEditor()
        {
            
        }

        public static void WIP()
        {
            new MessageBox("WIP", "WIP", ButtonType.OK, IconType.Error);
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
            Compatibility.RMXP.ImportMaps();
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
            if (MainWindow.MapWidget != null) MainWindow.MapWidget.SetGridVisibility(GeneralSettings.ShowGrid);
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
        /// Returns the first unused event ID for the current map.
        /// </summary>
        /// <returns></returns>
        public static int GetFreeEventID(Map Map)
        {
            int i = 1;
            while (true)
            {
                if (!Map.Events.ContainsKey(i))
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
            peridot.Program.EmbedGame(MainWindow, Data.ProjectPath);
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

            string OldMode = ProjectSettings.LastMode;
            ProjectSettings.LastMode = Mode;

            MainWindow.StatusBar.SetVisible(true);
            MainWindow.ToolBar.SetVisible(true);

            if (Mode == "MAPPING") // Select Mapping mode
            {
                MainWindow.ToolBar.MappingMode.SetSelected(true, Force);

                Map SelectedMap = null;
                if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();
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
                MainWindow.MapWidget.MapSelectPanel.SetMap(SelectedMap?? Data.Maps[mapid]);
                MainWindow.MapWidget.SetSelectedLayer(lastlayer);
                MainWindow.MapWidget.SetZoomFactor(ProjectSettings.LastZoomFactor);
                MainWindow.MapWidget.SetSubmode(ProjectSettings.LastMappingSubmode);
            }
            else if (OldMode == "MAPPING") // Deselect Mapping mode
            {
                
            }
            if (Mode == "EVENTING") // Select Eventing mode
            {
                
            }
            else if (OldMode == "EVENTING") // Deselect Eventing mode
            {

            }
            if (Mode == "SCRIPTING") // Select Scripting mode
            {
                MainWindow.ToolBar.ScriptingMode.SetSelected(true, Force);

                if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();
                MainWindow.MainEditorWidget = null;
            }
            else if (OldMode == "SCRIPTING") // Deselect Script mode
            {

            }
            if (Mode == "DATABASE") // Select Database mode
            {
                MainWindow.ToolBar.DatabaseMode.SetSelected(true, Force);

                if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();
                MainWindow.MainEditorWidget = new DatabaseWidget(MainWindow.MainGridLayout);
                MainWindow.MainEditorWidget.SetGridRow(3);
                MainWindow.DatabaseWidget.SetMode("species");
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
            string path = ProjectFilePath;
            while (path.Contains('\\')) path = path.Replace('\\', '/');
            for (int i = 0; i < GeneralSettings.RecentFiles.Count; i++)
            {
                if (GeneralSettings.RecentFiles[i][1] == path) // Project file paths match - same project
                {
                    GeneralSettings.RecentFiles.RemoveAt(i);
                }
            }
            GeneralSettings.RecentFiles.Add(new List<string>() { ProjectSettings.ProjectName, path });
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
            ProjectSettings.SavedVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
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
            if (ProjectSettings.SwitchGroupCapacity == 0) ProjectSettings.SwitchGroupCapacity = 25;
            if (ProjectSettings.Switches == null) ProjectSettings.Switches = new List<GameSwitchGroup>();
            if (ProjectSettings.Switches.Count == 0) for (int i = 0; i < ProjectSettings.SwitchGroupCapacity; i++) ProjectSettings.Switches.Add(new GameSwitchGroup() { ID = i + 1 });
            if (ProjectSettings.VariableGroupCapacity == 0) ProjectSettings.VariableGroupCapacity = 25;
            if (ProjectSettings.Variables == null) ProjectSettings.Variables = new List<GameVariableGroup>();
            if (ProjectSettings.Variables.Count == 0) for (int i = 0; i < ProjectSettings.VariableGroupCapacity; i++) ProjectSettings.Variables.Add(new GameVariableGroup() { ID = i + 1 });
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
        public string SavedVersion;
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
        /// <summary>
        /// The maximum game switch group capacity.
        /// </summary>
        public int SwitchGroupCapacity = 25;
        /// <summary>
        /// The data related to game switches.
        /// </summary>
        public List<GameSwitchGroup> Switches = new List<GameSwitchGroup>();
        /// <summary>
        /// The maximum game variable group capacity.
        /// </summary>
        public int VariableGroupCapacity = 25;
        /// <summary>
        /// The data related to game variables.
        /// </summary>
        public List<GameVariableGroup> Variables = new List<GameVariableGroup>();
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
}
