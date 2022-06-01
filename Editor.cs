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
        Widget.ShowWidgetOutlines = !Widget.ShowWidgetOutlines;
        //ProgressWindow pw = new ProgressWindow("Testing", "Testing animation...", false);
        //pw.SetProgress(1);
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
    public static void CloseProject(bool GoToHomeScreen = true)
    {
        if (!InProject) return;
        if (GoToHomeScreen)
        {
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
        }
        Data.ClearProjectData();
        ClearProjectData();
    }

    /// <summary>
    /// Closes and reopens the project.
    /// </summary>
    public static void ReloadProject()
    {
        string projectfile = Data.ProjectFilePath;
        CloseProject(false);
        Data.SetProjectPath(projectfile);
        MainWindow.CreateEditor();
        MakeRecentProject();
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
        NewProjectWindow window = new NewProjectWindow();
        window.OnClosed += _ =>
        {
            if (window.PressedOK)
            {
                CreateNewProject(window.Name, window.Kit, window.Folder);
            }
        };
    }

    public static void CreateNewProject(string ProjectName, (string Name, string Download) Kit, string Folder)
    {
        // Make the desktop the current directory, in case a relative path was specified.
        string OldCurrentDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

        // Create the folder that was specified if it did not already exist
        Folder = Path.Combine(Folder, Utilities.LegalizeFilename(ProjectName)); 

        if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
        // Turn the (potentially) local folder name to an absolute one for further use
        Folder = Path.GetFullPath(Folder);
        // Restore current directory to what it was before
        Directory.SetCurrentDirectory(OldCurrentDirectory);

        // The kit we've chosen has been downloaded in the past, so we can copy that to our game folder.
        if (Utilities.KitExists(Kit.Name))
        {
            CopyStep();
        }
        else
        {
            // The kit has not been downloaded before, so we download it now.
            string Filename = Path.Combine("Kits", Kit.Name + ".zip");
            if (!Directory.Exists("Kits")) Directory.CreateDirectory("Kits");
            FileDownloaderWindow window = new FileDownloaderWindow(Kit.Download, Filename, "Downloading kit...");
            window.OnFinished += _ => 
            {
                CopyStep();
            };
        }

        void CopyStep()
        {
            ProgressWindow window = new ProgressWindow("Copying", "Copying files...");
            Graphics.Update();
            Utilities.CopyKit(Kit.Name, Folder, delegate (ObjectEventArgs e)
            {
                if (Graphics.CanUpdate()) Graphics.Update();
                else return;
                window.SetProgress((float) e.Object);
                if ((float) e.Object == 1)
                {
                    CloseProject(false);
                    Data.SetProjectPath(Path.Combine(Folder, "Game.rxproj"));
                    MainWindow.CreateEditor();
                    MakeRecentProject();
                }
            });
        }
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
            string path = GeneralSettings.RecentFiles[0].ProjectFile;
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
        string result = of.ChooseFile();
        if (result != null)
        {
            if (!result.EndsWith(".rxproj"))
                new MessageBox("Error", "Invalid project file.", ButtonType.OK, IconType.Error);
            else
            {
                CloseProject(false);
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
        MakeRecentProject();
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
        SaveProject();
        GameRunner.Start();
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
    public static void SetMode(EditorMode Mode, bool Force = false, MapMode? MapMode = null, DatabaseMode? DatabaseMode = null)
    {
        if (!Force && Editor.Mode == Mode)
        {
            if (Mode == EditorMode.Mapping && MapMode != null && MainWindow.MapWidget.MapViewer.Mode != MapMode) SetMappingSubmode((MapMode)MapMode);
            else if (Mode == EditorMode.Database && DatabaseMode != null && MainWindow.DatabaseWidget.Mode != DatabaseMode) SetDatabaseSubmode((DatabaseMode) DatabaseMode);
            return;
        }

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
                SetMappingMode(MapMode ?? ProjectSettings.LastMappingSubmode);
                break;
            case EditorMode.Scripting:
                // Select Scripting Mode
                MainWindow.ToolBar.ScriptingMode.SetSelected(true, Force);
                break;
            case EditorMode.Database:
                // Select Database mode
                SetDatabaseMode(DatabaseMode ?? ProjectSettings.LastDatabaseSubmode);
                break;
        }
        MainWindow.MainGridLayout.UpdateLayout();
        MainWindow.StatusBar.Refresh();
        MainWindow.ToolBar.Refresh();
    }

    private static void SetMappingMode(MapMode Submode)
    {
        MainWindow.ToolBar.MappingMode.SetSelected(true);
        if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();

        MainWindow.MainEditorWidget = new MappingWidget(MainWindow.MainGridLayout);
        MainWindow.MainEditorWidget.SetGridRow(3);
        // Set list of maps & initial map
        MainWindow.MapWidget.MapSelectPanel.PopulateList();
        int mapid = ProjectSettings.LastMapID;
        int lastlayer = ProjectSettings.LastLayer;
        MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps.ContainsKey(mapid) ? Data.Maps[mapid] : Data.Maps.Values.First());
        MainWindow.MapWidget.SetSelectedLayer(lastlayer);
        MainWindow.MapWidget.SetZoomFactor(ProjectSettings.LastZoomFactor);

        MainWindow.UI.SetSelectedWidget(MainWindow.MapWidget.MapViewer);
        SetMappingSubmode(Submode);
    }

    public static void SetMappingSubmode(MapMode Submode)
    {
        MainWindow.MapWidget.SetMode(Submode);
    }

    private static void SetDatabaseMode(DatabaseMode Submode, bool Force = false)
    {
        MainWindow.ToolBar.DatabaseMode.SetSelected(true, Force);
        if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();

        MainWindow.MainEditorWidget = new DatabaseWidget(MainWindow.MainGridLayout);
        MainWindow.MainEditorWidget.SetGridRow(3);
        SetDatabaseSubmode(Submode);
    }

    public static void SetDatabaseSubmode(DatabaseMode Submode)
    {
        MainWindow.DatabaseWidget.SetMode(Submode);
    }

    /// <summary>
    /// Opens the Help window.
    /// </summary>
    public static void OpenHelpWindow()
    {
        new MessageBox("Help",
            "As there is no built-in wiki or documentation yet, please direct any questions to the official Discord server or Twitter account.");
    }

    /// <summary>
    /// Open the About window.
    /// </summary>
    public static void OpenAboutWindow()
    {
        new MessageBox("About RPG Studio MK",
            "This program is intended to be an editor for games made with the MK Starter Kit.\n" +
            "It was created by Marin, with additional support of various other individuals.\n" +
            "\n" +
            "Please turn to the GitHub page for a full credits list."
        );
    }

    /// <summary>
    /// Open the Legal window.
    /// </summary>
    public static void OpenLegalWindow()
    {
        new MessageBox("Legal",
            "Copyright © 2021 Marijn Herrebout\n\n" +
            "RPG Studio MK is licensed under the GNU General Public License v3+, referred to as GPLv3+.\n\n" +
            "You may view the details of this license from the file titled LICENSE in the program's root folder.\nIf not, please view https://www.gnu.org/licenses/gpl-3.0.html."
        );
    }

    /// <summary>
    /// Opens a window allowing you to rename the game.
    /// </summary>
    public static void RenameGame()
    {
        GenericTextBoxWindow win = new GenericTextBoxWindow("Title", "Title:", ProjectSettings.ProjectName, true);
        win.OnClosed += _ =>
        {
            if (!win.Apply) return;
            ProjectSettings.ProjectName = win.Value;
        };
    }

    /// <summary>
    /// Changes the view mode of event boxes in the map viewer.
    /// </summary>
    /// <param name="ViewMode">The new view mode of event boxes.</param>
    public static void SetEventGraphicViewMode(EventGraphicViewMode ViewMode)
    {
        ProjectSettings.EventGraphicViewMode = ViewMode;
        if (Mode == EditorMode.Mapping && (MainWindow.MapWidget.MapViewer.Mode == MapMode.Events || MainWindow.MapWidget.MapViewer.Mode == MapMode.Tiles && ProjectSettings.ShowEventBoxesInTilesSubmode))
            MainWindow.MapWidget.MapViewer.UpdateEventBoxesViewMode();
    }

    /// <summary>
    /// Sets whether event boxes are visible in the Tiles submode.
    /// </summary>
    /// <param name="Visible">The visiblity of event boxes.</param>
    public static void SetEventBoxVisibilityInTiles(bool Visible)
    {
        ProjectSettings.ShowEventBoxesInTilesSubmode = Visible;
        if (Mode == EditorMode.Mapping && MainWindow.MapWidget.MapViewer.Mode == MapMode.Tiles)
        {
            if (Visible) MainWindow.MapWidget.MapViewer.ShowEventBoxes();
            else MainWindow.MapWidget.MapViewer.HideEventBoxes();
        }
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
            throw new Exception("No Game.rxproj was found.");
        }
        while (path.Contains('\\')) path = path.Replace('\\', '/');
        for (int i = 0; i < GeneralSettings.RecentFiles.Count; i++)
        {
            if (GeneralSettings.RecentFiles[i].ProjectFile == path) // Project file paths match - same project
            {
                // Remove and still add to update the ordering in the list
                GeneralSettings.RecentFiles.RemoveAt(i);
            }
        }
        GeneralSettings.RecentFiles.Add((ProjectSettings.ProjectName, path));
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
            try
            {
                GeneralSettings = formatter.Deserialize(stream) as GeneralSettings;
                GeneralSettings.Update();
            }
            catch (SerializationException)
            {
                GeneralSettings = new GeneralSettings();
            }
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
            ProjectSettings.Update();
            stream.Close();
        }
        else
        {
            ProjectSettings = new ProjectSettings();
        }
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
        UnsavedChanges = false;
    }

    /// <summary>
    /// Called every tick for logic updates.
    /// </summary>
    public static void Update()
    {
        GameRunner.Update();
    }
}

[Serializable]
public class ProjectSettings
{
    public Dictionary<string, object> RawData;

    public ProjectSettings()
    {
        RawData = new Dictionary<string, object>();
        RawData.Add("SAVED_VERSION", "1.0.0");
        RawData.Add("PROJECT_NAME", "Untitled Game");
        RawData.Add("LAST_MODE", EditorMode.Mapping);
        RawData.Add("LAST_MAPPING_SUBMODE", MapMode.Tiles);
        RawData.Add("LAST_DATABASE_SUBMODE", DatabaseMode.Tilesets);
        RawData.Add("LAST_MAP_ID", 1);
        RawData.Add("LAST_LAYER", 1);
        RawData.Add("LAST_ZOOM_FACTOR", 1d);
        RawData.Add("TILESET_CAPACITY", 25);
        RawData.Add("AUTOTILE_CAPACITY", 25);
        RawData.Add("EVENT_GRAPHIC_VIEW_MODE", EventGraphicViewMode.BoxAndGraphic);
        RawData.Add("SHOW_EVENT_BOXES_IN_TILES", false);
    }

    /// <summary>
    /// Perform any version upgrades or updates here.
    /// </summary>
    public void Update()
    {
        
    }

    /// <summary>
    /// The last-used version of the editor to save the project. Can be used to programmatically port old data formats to new formats upon an update.
    /// </summary>
    public string SavedVersion
    {
        get => (string) RawData["SAVED_VERSION"];
        set => RawData["SAVED_VERSION"] = value;
    }
    /// <summary>
    /// The name of the project.
    /// </summary>
    public string ProjectName
    {
        get => (string) RawData["PROJECT_NAME"];
        set => RawData["PROJECT_NAME"] = value;
    }
    /// <summary>
    /// The last-active mode of the project.
    /// </summary>
    public EditorMode LastMode
    {
        get => (EditorMode) RawData["LAST_MODE"];
        set => RawData["LAST_MODE"] = value;
    }
    /// <summary>
    /// The last-active submode within the Mapping mode.
    /// </summary>
    public MapMode LastMappingSubmode
    {
        get => (MapMode) RawData["LAST_MAPPING_SUBMODE"];
        set => RawData["LAST_MAPPING_SUBMODE"] = value;
    }
    /// <summary>
    /// The last-active submode within the Database mode.
    /// </summary>
    public DatabaseMode LastDatabaseSubmode
    {
        get => (DatabaseMode) RawData["LAST_DATABASE_SUBMODE"];
        set => RawData["LAST_DATABASE_SUBMODE"] = value;
    }
    /// <summary>
    /// The last selected Map.
    /// </summary>
    public int LastMapID
    {
        get => (int) RawData["LAST_MAP_ID"];
        set => RawData["LAST_MAP_ID"] = value;
    }
    /// <summary>
    /// The last selected layer.
    /// </summary>
    public int LastLayer
    {
        get => (int) RawData["LAST_LAYER"];
        set => RawData["LAST_LAYER"] = value;
    }
    /// <summary>
    /// The last zoom factor.
    /// </summary>
    public double LastZoomFactor
    {
        get => (double) RawData["LAST_ZOOM_FACTOR"];
        set => RawData["LAST_ZOOM_FACTOR"] = value;
    }
    /// <summary>
    /// The maximum tileset capacity.
    /// </summary>
    public int TilesetCapacity
    {
        get => (int) RawData["TILESET_CAPACITY"];
        set => RawData["TILESET_CAPACITY"] = value;
    }
    /// <summary>
    /// The maximum autotile capacity.
    /// </summary>
    public int AutotileCapacity
    {
        get => (int) RawData["AUTOTILE_CAPACITY"];
        set => RawData["AUTOTILE_CAPACITY"] = value;
    }

    /// <summary>
    /// The view mode of event graphics.
    /// </summary>
    public EventGraphicViewMode EventGraphicViewMode
    {
        get => (EventGraphicViewMode) RawData["EVENT_GRAPHIC_VIEW_MODE"];
        set => RawData["EVENT_GRAPHIC_VIEW_MODE"] = value;
    }

    /// <summary>
    /// Whether to show event boxes in the TIles submode
    /// </summary>
    public bool ShowEventBoxesInTilesSubmode
    {
        get => (bool) RawData["SHOW_EVENT_BOXES_IN_TILES"];
        set => RawData["SHOW_EVENT_BOXES_IN_TILES"] = value;
    }
}

[Serializable]
public class GeneralSettings
{
    public Dictionary<string, object> RawData;

    public GeneralSettings()
    {
        RawData = new Dictionary<string, object>();
        RawData.Add("WAS_MAXIMIZED", true);
        RawData.Add("LAST_WIDTH", 600);
        RawData.Add("LAST_HEIGHT", 600);
        RawData.Add("LAST_X", 50);
        RawData.Add("LAST_Y", 50);
        RawData.Add("RECENT_FILES", new List<(string, string)>());
        RawData.Add("SHOW_MAP_ANIMATIONS", true);
        RawData.Add("SHOW_GRID", true);
        RawData.Add("PREFER_RECTANGLE_FILL", false);
        RawData.Add("PREFER_ELLIPSE_FILL", false);
        RawData.Add("PREFER_SELECTION_ALL", false);
        RawData.Add("SECONDS_USED", 0);
    }

    /// <summary>
    /// Perform any version upgrades or updates here.
    /// </summary>
    public void Update()
    {

    }

    /// <summary>
    /// Whether the editor window was maximized.
    /// </summary>
    public bool WasMaximized
    {
        get => (bool) RawData["WAS_MAXIMIZED"];
        set => RawData["WAS_MAXIMIZED"] = value;
    }
    /// <summary>
    /// The last width the editor window had when it was not maximized.
    /// </summary>
    public int LastWidth
    {
        get => (int) RawData["LAST_WIDTH"];
        set => RawData["LAST_WIDTH"] = value;
    }
    /// <summary>
    /// The last height the editor window had when it was not maximized.
    /// </summary>
    public int LastHeight
    {
        get => (int) RawData["LAST_HEIGHT"];
        set => RawData["LAST_HEIGHT"] = value;
    }
    /// <summary>
    /// The last X position the editor window had when it was not maximized.
    /// </summary>
    public int LastX
    {
        get => (int) RawData["LAST_X"];
        set => RawData["LAST_X"] = value;
    }
    /// <summary>
    /// The last Y position the editor window had when it was not maximized.
    /// </summary>
    public int LastY
    {
        get => (int) RawData["LAST_Y"];
        set => RawData["LAST_Y"] = value;
    }
    /// <summary>
    /// The list of recently opened projects. May contain old/invalid paths.
    /// </summary>
    public List<(string ProjectName, string ProjectFile)> RecentFiles
    {
        get => (List<(string, string)>) RawData["RECENT_FILES"];
        set => RawData["RECENT_FILES"] = value;
    }
    /// <summary>
    /// Whether to play map animations.
    /// </summary>
    public bool ShowMapAnimations
    {
        get => (bool) RawData["SHOW_MAP_ANIMATIONS"];
        set => RawData["SHOW_MAP_ANIMATIONS"] = value;
    }
    /// <summary>
    /// Whether to show the map grid overlay.
    /// </summary>
    public bool ShowGrid
    {
        get => (bool) RawData["SHOW_GRID"];
        set => RawData["SHOW_GRID"] = value;
    }
    /// <summary>
    /// Whether the user prefers to use the rectangle tool including filling.
    /// </summary>
    public bool PreferRectangleFill
    {
        get => (bool) RawData["PREFER_RECTANGLE_FILL"];
        set => RawData["PREFER_RECTANGLE_FILL"] = value;
    }
    /// <summary>
    /// Whether the user prefers to use the ellipse tool including filling.
    /// </summary>
    public bool PreferEllipseFill
    {
        get => (bool) RawData["PREFER_ELLIPSE_FILL"];
        set => RawData["PREFER_ELLIPSE_FILL"] = value;
    }
    /// <summary>
    /// Whether the user prefers to use the selection tool for selecting multiple layers.
    /// </summary>
    public bool PreferSelectionAll
    {
        get => (bool) RawData["PREFER_SELECTION_ALL"];
        set => RawData["PREFER_SELECTION_ALL"] = value;
    }
    /// <summary>
    /// The total number of seconds the program has ever been open.
    /// </summary>
    public int SecondsUsed
    {
        get => (int) RawData["SECONDS_USED"];
        set => RawData["SECONDS_USED"] = value;
    }
}

public enum EditorMode
{
    Mapping,
    Scripting,
    Database
}

public enum MapMode
{
    Tiles = 0,
    Events = 1
}

public enum DatabaseMode
{
    Species,
    Moves,
    Abilities,
    Items,
    TMs,
    Tilesets,
    Autotiles,
    Types,
    Trainers,
    Animations,
    System
}

public enum Direction
{
    Down  = 2,
    Left  = 4,
    Right = 6,
    Up    = 8
}