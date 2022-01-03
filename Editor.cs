using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;
using System.IO.Compression;

namespace RPGStudioMK;

public static class Editor
{
    /// <summary>
    /// The time at which the editor was opened.
    /// </summary>
    public static DateTime TimeOpened = DateTime.Now;

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
    public static List<Undo.BaseUndoAction> UndoList = new List<Undo.BaseUndoAction>();

    /// <summary>
    /// Contains the list of recent actions that you undid that you can redo.
    /// </summary>
    public static List<Undo.BaseUndoAction> RedoList = new List<Undo.BaseUndoAction>();

    /// <summary>
    /// Event that is called after the editor has undone the latest action.
    /// </summary>
    public static BaseEvent OnUndoing;

    /// <summary>
    /// Whether or not undo/redo is currently usable. Disable while drawing tiles in map editor, for instance.
    /// </summary>
    public static bool CanUndo = true;

    /// <summary>
    /// A list of maps that were deleted this sessions. Allows you to restore a map.
    /// </summary>
    public static List<Map> DeletedMaps = new List<Map>();

    /// <summary>
    /// The currently active mode of the editor.
    /// </summary>
    public static EditorMode Mode;

    /// <summary>
    /// Whether the editor is currently undoing a change.
    /// </summary>
    public static bool Undoing = false;

    /// <summary>
    /// Whether the editor is currently redoing a change.
    /// </summary>
    public static bool Redoing = false;


    /// <summary>
    /// Debug method for quickly testing a piece of functionality.
    /// </summary>
    public static void Test()
    {
        if (Program.ReleaseMode) return;
    }

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

    public static void WIP()
    {
        new MessageBox("WIP", "WIP", ButtonType.OK, IconType.Error);
    }

    /// <summary>
    /// Undoes the latest change you made.
    /// </summary>
    public static void Undo(bool Internal = false)
    {
        if (UndoList.Count > 0 && (CanUndo || Internal) && !Input.TextInputActive())
        {
            Undoing = true;
            UndoList[UndoList.Count - 1].RevertTo(false);
            if (!Internal) OnUndoing?.Invoke(new BaseEventArgs());
            MainWindow.ToolBar.Undo.SetEnabled(UndoList.Count > 0);
            MainWindow.ToolBar.Redo.SetEnabled(RedoList.Count > 0);
            Undoing = false;
        }
    }

    /// <summary>
    /// Redoes the latest change that you undid.
    /// </summary>
    public static void Redo()
    {
        if (RedoList.Count > 0 && CanUndo && !Input.TextInputActive())
        {
            Redoing = true;
            RedoList[RedoList.Count - 1].RevertTo(true);
            MainWindow.ToolBar.Undo.SetEnabled(UndoList.Count > 0);
            MainWindow.ToolBar.Redo.SetEnabled(RedoList.Count > 0);
            Redoing = false;
        }
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
    /// Increments the order of all maps higher than a specific order, essentially moving them all down by one.
    /// </summary>
    /// <param name="Order">The order from where to start shifting.</param>
    public static void IncrementMapOrderFrom(int Order)
    {
        foreach (Map map in Data.Maps.Values)
        {
            if (map.Order >= Order) map.Order++;
        }
    }

    /// <summary>
    /// Decrements the order of all maps higher than a specific order, essentially moving them all up by one.
    /// </summary>
    /// <param name="Order">The order from where to start shifting.</param>
    public static void DecrementMapOrderFrom(int Order)
    {
        foreach (Map map in Data.Maps.Values)
        {
            if (map.Order >= Order) map.Order--;
        }
    }

    /// <summary>
    /// Get the highest order value in a node and its children.
    /// </summary>
    /// <param name="Node">The node to look within.</param>
    /// <returns>The highest order within the node.</returns>
    public static int GetHighestMapOrder(TreeNode Node)
    {
        int max = Data.Maps[(int)Node.Object].Order;
        foreach (TreeNode Child in Node.Nodes)
        {
            int childmax = GetHighestMapOrder(Child);
            if (childmax > max) max = childmax;
        }
        return max;
    }

    /// <summary>
    /// Updates the map data parent/order fields based on the node structure.
    /// </summary>
    /// <param name="Nodes">The list of nodes to update the order of.</param>
    /// <param name="start">The integer to start counting at.</param>
    public static int UpdateOrder(List<TreeNode> Nodes, int start = 1)
    {
        int order = start;
        for (int i = 0; i < Nodes.Count; i++)
        {
            TreeNode n = Nodes[i];
            // Set order
            Data.Maps[(int)n.Object].Order = order;
            if (start == 1) // First call; set parent to 0
            {
                Data.Maps[(int)n.Object].ParentID = 0;
            }
            order++;
            order = UpdateOrder(n.Nodes, order);
            foreach (TreeNode child in n.Nodes)
            {
                // Set parent
                Data.Maps[(int)child.Object].ParentID = (int)n.Object;
            }
        }
        return order;
    }

    /// <summary>
    /// Reorganises the order values to never skip any values.
    /// </summary>
    public static void OptimizeOrder()
    {
        List<(int, int)> list = Data.Maps.Values.Select(m => (m.ID, m.Order)).ToList();
        list.Sort(((int, int) t1, (int, int) t2) =>
        {
            return t1.Item2.CompareTo(t2.Item2);
        });
        OptimizeOrderInternal(list);
    }

    private static void OptimizeOrderInternal(List<(int MapID, int Order)> Orders)
    {
        for (int i = 0; i < Orders.Count; i++)
        {
            int mapid = Orders[i].MapID;
            if (i == 0)
            {
                if (Orders[i].Order > 1)
                {
                    Data.Maps[mapid].Order = 1;
                    Orders[i] = (mapid, 1);
                }
            }
            else
            {
                int diff = Orders[i].Order - Orders[i - 1].Order;
                if (diff > 1)
                {
                    Data.Maps[mapid].Order -= diff - 1;
                    Orders[i] = (mapid, Data.Maps[mapid].Order);
                }
            }
        }
    }

    /// <summary>
    /// Adds a Map to the map list.
    /// </summary>
    /// <param name="Map">The new Map object.</param>
    /// <param name="ParentID">The ID of the parent map, or 0 if adding to bottom.</param>
    public static void AddMap(Map Map, int ParentID = 0)
    {
        Map.ParentID = ParentID;
        Data.Maps.Add(Map.ID, Map);
        TreeNode node = new TreeNode() { Name = Map.ToString(), Object = Map.ID };
        if (MainWindow.MapWidget != null)
        {
            TreeView mapview = MainWindow.MapWidget.MapSelectPanel.mapview;
            if (mapview.HoveringNode != null)
            {
                if ((int)mapview.HoveringNode.Object != ParentID) throw new Exception("Adding map to wrong node.");
                int maxorder = GetHighestMapOrder(mapview.HoveringNode);
                if (Data.Maps.Values.Any(m => m.Order == maxorder + 1)) IncrementMapOrderFrom(maxorder + 1);
                if (Data.Maps.Values.Any(m => m.Order == maxorder + 1)) throw new Exception("Error creating unique order");
                Map.Order = maxorder + 1;
                Map.ParentID = (int)mapview.HoveringNode.Object;
                mapview.HoveringNode.Nodes.Add(node);
                mapview.HoveringNode.Collapsed = false;
                Data.Maps[(int)mapview.HoveringNode.Object].Expanded = true;
            }
            else
            {
                int max = 0;
                foreach (Map m in Data.Maps.Values) if (m.Order > max) max = m.Order;
                Map.Order = max + 1;
                mapview.Nodes.Add(node);
            }
            mapview.SetSelectedNode(node);
        }
        else
        {
            throw new NotImplementedException();
        }
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
        of.SetFilter(new FileFilter("RMXP Project", "rxproj"));
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
            if (!result.EndsWith(".rxproj"))
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
        Stopwatch s = new Stopwatch();
        s.Start();
        DumpProjectSettings();
        Data.SaveGameData();
        UnsavedChanges = false;
        if (MainWindow != null)
        {
            s.Stop();
            MainWindow.StatusBar.QueueMessage($"Saved project ({s.ElapsedMilliseconds}ms)", true);
        }
    }

    public static void MakeGame()
    {
        // That's your job.
    }

    /// <summary>
    /// Runs the current project.
    /// </summary>
    public static void StartGame()
    {
        //peridot.Program.EmbedGame(MainWindow, Data.ProjectPath);
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
    /// <param name="Mode">The mode to switch to. MAPPING, SCRIPTING or DATABASE.</param>
    /// <param name="Force">Whether or not to force a full redraw.</param>
    public static void SetMode(EditorMode Mode, bool Force = false)
    {
        if (Mode == Editor.Mode && !Force) return;

        EditorMode OldMode = ProjectSettings.LastMode;
        ProjectSettings.LastMode = Mode;
        Editor.Mode = Mode;

        MainWindow.StatusBar.SetVisible(true);
        MainWindow.ToolBar.SetVisible(true);

        // Perform any actions upon deselection of a mode.
        switch (OldMode)
        {
            case EditorMode.Mapping:
                break;
            case EditorMode.Scripting:
                break;
            case EditorMode.Database:
                break;
        }

        if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();
        MainWindow.MainEditorWidget = null;

        // Perform any actions upon selection of a mode.
        switch (Mode)
        {
            case EditorMode.Mapping:
                // Select Mapping mode
                SetMappingMode(Force);
                break;
            case EditorMode.Scripting:
                // Select Scripting Mode
                MainWindow.ToolBar.ScriptingMode.SetSelected(true, Force);
                break;
            case EditorMode.Database:
                // Select Database mode
                SetDatabaseMode(ProjectSettings.LastDatabaseSubmode);
                break;
        }
        MainWindow.MainGridLayout.UpdateLayout();
        MainWindow.StatusBar.Refresh();
        MainWindow.ToolBar.Refresh();
    }

    public static void SetMappingMode(bool Force = false)
    {
        MainWindow.ToolBar.MappingMode.SetSelected(true, Force);

        MainWindow.MainEditorWidget = new MappingWidget(MainWindow.MainGridLayout);
        MainWindow.MainEditorWidget.SetGridRow(3);
        // Set list of maps & initial map
        MainWindow.MapWidget.MapSelectPanel.PopulateList();
        int mapid = ProjectSettings.LastMapID;
        int lastlayer = ProjectSettings.LastLayer;
        MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps.ContainsKey(mapid) ? Data.Maps[mapid] : Data.Maps.Values.First());
        MainWindow.MapWidget.SetSelectedLayer(lastlayer);
        MainWindow.MapWidget.SetZoomFactor(ProjectSettings.LastZoomFactor);
    }

    public static void SetDatabaseMode(DatabaseMode Submode, bool Force = false)
    {
        MainWindow.ToolBar.DatabaseMode.SetSelected(true, Force);
        if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();
        MainWindow.MainEditorWidget = new DatabaseWidget(MainWindow.MainGridLayout);
        MainWindow.MainEditorWidget.SetGridRow(3);
        MainWindow.DatabaseWidget.SetMode(Submode);
    }

    public static void SetDatabaseSubmode(DatabaseMode Submode)
    {
        MainWindow.DatabaseWidget.SetMode(Submode);
    }

    /// <summary>
    /// Adds the current project to the list of recently opened projects.
    /// </summary>
    public static void MakeRecentProject()
    {
        string path = null;
        foreach (string file in Directory.GetFiles(Data.ProjectPath))
        {
            if (file.EndsWith(".rxproj"))
            {
                path = Path.GetFullPath(file);
                break;
            }
        }
        if (path == null)
        {

            return;
        }
        while (path.Contains('\\')) path = path.Replace('\\', '/');
        for (int i = 0; i < GeneralSettings.RecentFiles.Count; i++)
        {
            if (GeneralSettings.RecentFiles[i][1] == path) // Project file paths match - same project
            {
                // Remove and still add to update the ordering in the list
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
        GeneralSettings.SecondsUsed += (int)Math.Floor((DateTime.Now - TimeOpened).TotalSeconds);
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
        Stream stream = new FileStream(Data.ProjectPath + "/project.mkproj", FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, ProjectSettings);
        stream.Close();
    }

    /// <summary>
    /// Loads the current project's settings.
    /// </summary>
    public static void LoadProjectSettings()
    {
        if (File.Exists(Data.ProjectPath + "/project.mkproj"))
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Data.ProjectPath + "/project.mkproj", FileMode.Open, FileAccess.Read);
            ProjectSettings = formatter.Deserialize(stream) as ProjectSettings;
            stream.Close();
        }
        else
        {
            ProjectSettings = new ProjectSettings();
        }
        if (ProjectSettings.LastZoomFactor == 0) ProjectSettings.LastZoomFactor = 1;
        if (ProjectSettings.ProjectName.Length == 0) ProjectSettings.ProjectName = "Untitled Game";
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
        UndoList.Clear();
        RedoList.Clear();
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
    /// The name of the project.
    /// </summary>
    public string ProjectName = "Untitled Game";
    /// <summary>
    /// The last-active mode of the project.
    /// </summary>
    public EditorMode LastMode = EditorMode.Mapping;
    /// <summary>
    /// The last-active submode within the Mapping mode.
    /// </summary>
    public string LastMappingSubmode = "TILES";
    /// <summary>
    /// The last-active submode within the Database mode.
    /// </summary>
    public DatabaseMode LastDatabaseSubmode = DatabaseMode.Tilesets;
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
    /// <summary>
    /// Whether the user prefers to use the rectangle tool including filling.
    /// </summary>
    public bool PreferRectangleFill = false;
    /// <summary>
    /// Whether the user prefers to use the ellipse tool including filling.
    /// </summary>
    public bool PreferEllipseFill = false;
    /// <summary>
    /// Whether the user prefers to use the selection tool for selecting multiple layers.
    /// </summary>
    public bool PreferSelectionAll = false;
    /// <summary>
    /// The total number of seconds the program has ever been open.
    /// </summary>
    public int SecondsUsed = 0;
}

public enum EditorMode
{
    Mapping,
    Scripting,
    Database
}