using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;
using System.Threading.Tasks;
using System.Threading;
using amethyst.Animations;
using System.Security.AccessControl;
using RPGStudioMK.Utility;

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
    public static bool InProject { get { return !string.IsNullOrEmpty(Data.ProjectFilePath); } }

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
    /// The absolute path to the application's data folder.
    /// </summary>
    public static string AppDataFolder => Path.Combine(MKUtils.MKUtils.AppDataFolder, "RPG Studio MK");

    /// <summary>
    /// The absolute path to the installed kits folder.
    /// </summary>
    public static string KitsFolder => Path.Combine(AppDataFolder, "Kits");

    /// <summary>
    /// The absolute path to the general settings file of the program.
    /// </summary>
    public static string SettingsFilePath => Path.Combine(AppDataFolder, "editor.mkd");

    /// <summary>
    /// Debug method for quickly testing a piece of functionality.
    /// </summary>
    public static void Test()
    {
        if (Program.ReleaseMode) return;

        Widget.ShowWidgetOutlines = !Widget.ShowWidgetOutlines;

        //ColorPickerWindow win = new ColorPickerWindow(Color.WHITE, false);

        //PopupWindow win = new PopupWindow();
        //win.SetSize(600, 600);
        //win.Center();

        //var root = new OptimizedNode("Root");
        //var one = new OptimizedNode("One");
        //var two = new OptimizedNode("Two");
        //var three = new OptimizedNode("Three");
        //var four = new OptimizedNode("Four");
        //var five = new OptimizedNode("Five");
        //var six = new OptimizedNode("Six");
        //var seven = new OptimizedNode("Seven");
        //var eight = new OptimizedNode("Eight");
        //var nine = new OptimizedNode("Nine");
        //var ten = new OptimizedNode("Ten");
        //var eleven = new OptimizedNode("Eleven");
        //var twelve = new OptimizedNode("Twelve");
        //var thirteen = new OptimizedNode("Thirteen");
        //var fourteen = new OptimizedNode("Fourteen");
        //var fifteen = new OptimizedNode("Fifteen");
        //var sixteen = new OptimizedNode("Sixteen");
        //var seventeen = new OptimizedNode("Seventeen");
        //var abc = new OptimizedNode("abc");
        //var def = new OptimizedNode("def");
        //var ghi = new OptimizedNode("ghi");

        //nine.AddChild(ten);
        //nine.AddChild(eleven);

        //seven.AddChild(eight);
        //seven.AddChild(nine);
        //seven.AddChild(twelve);

        //four.AddChild(five);
        //two.AddChild(three);
        //two.AddChild(four);
        //two.AddChild(six);

        //thirteen.AddChild(fourteen);
        //fourteen.AddChild(abc);
        //fourteen.AddChild(def);
        //def.AddChild(ghi);
        //ghi.AddChild(new OptimizedNode("jkl"));
        //def.AddChild(new OptimizedNode("mno"));

        //one.AddChild(two);
        //one.AddChild(seven);
        //one.AddChild(thirteen);
        //one.AddChild(new OptimizedNode("pqr"));
        //one.AddChild(new OptimizedNode("Some rather long text, so that we can properly test the horizontal scrolling capabilities of the system."));

        //sixteen.AddChild(seventeen);

        //root.AddChild(one);
        //root.AddChild(new OptimizedNodeSeparator(8));
        //root.AddChild(fifteen);
        //root.AddChild(new OptimizedNodeSeparator(32));
        //root.AddChild(sixteen);

        //OptimizedTreeView tree = new OptimizedTreeView(win);
        //tree.SetDocked(true);
        //tree.SetPadding(40);
        //tree.SetBackgroundColor(10, 23, 37);
        //tree.SetExtraXScrollArea(40);
        //tree.SetExtraYScrollArea(40);
        //tree.SetRootNode(root);

        //win.CreateButton("OK", _ => win.Close());

        //Map Map = MainWindow.MapWidget?.Map;
        //if (Map == null) return;
        //MapAfterEffectsWindow maew = new MapAfterEffectsWindow(Map);
    }

    /// <summary>
    /// Returns the displayed string for the current editor version.
    /// </summary>
    public static string GetVersionString()
    {
        string VersionName = "Version";
        if (!string.IsNullOrEmpty(Program.CurrentVersion) && Program.CurrentVersion[0] == '0') VersionName = "Alpha";
        return VersionName + " " + Program.CurrentVersion;
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
    /// Opens a window containing all undoable changes.
    /// </summary>
    public static void ShowUndoHistory()
    {
        UndoListWindow win = new UndoListWindow(UndoList, false);
        win.OnClosed += _ =>
        {
            if (!win.Apply) return;
            win.ActionToRevertTo.RevertToLogical(false);
            SetMode(Mode, true);
        };
    }

    /// <summary>
    /// Opens a window containing all redoable changes.
    /// </summary>
    public static void ShowRedoHistory()
    {
        UndoListWindow win = new UndoListWindow(RedoList, true);
        win.OnClosed += _ =>
        {
            if (!win.Apply) return;
            win.ActionToRevertTo.RevertToLogical(true);
            SetMode(Mode, true);
        };
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
        string projectfile = Data.ProjectRMXPGamePath;
        CloseProject(false);
        Data.SetProjectPath(projectfile);
        if (MainWindow.CreateEditor()) MakeRecentProject();
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
    /// Gets the highest order value in the whole map tree overall.
    /// </summary>
    /// <returns>The highest order within all existing maps, i.e. the order value of the bottom-most map.</returns>
    public static int GetHighestMapOrder()
    {
        int max = 0;
        foreach (Map map in Data.Maps.Values)
        {
            if (map.Order > max) max = map.Order;
        }
        return max;
    }

    /// <summary>
    /// Assigns order values to externally added maps.
    /// </summary>
    public static void AssignOrderToNewMaps()
    {
        foreach (KeyValuePair<int, Map> kvp in Data.Maps)
        {
            if (kvp.Value.Order == -1)
            {
                kvp.Value.Order = GetHighestMapOrder() + 1;
            }
        }
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

    public static void DeleteProject()
    {
        string ProjectPath = Data.ProjectPath;
        string ProjectRMXPGamePath = Data.ProjectRMXPGamePath;
        GeneralSettings.RecentFiles.RemoveAll(rf => rf[1] == ProjectRMXPGamePath);
        CloseProject();
        try
        {
            Directory.Delete(ProjectPath, true);
        }
        catch (Exception ex)
        {
            new MessageBox("Error", $"Something went wrong while trying to delete the project.\n\n{ex.Message}\n{ex.StackTrace}", ButtonType.OK, IconType.Error);
        }
    }

    public static void CreateNewProject(string ProjectName, (string Name, string Download) Kit, string Folder)
    {
        // Make the desktop the current directory, in case a relative path was specified.
        string OldCurrentDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

        // Create the folder that was specified if it did not already exist
        Folder = Path.Combine(Folder, Utilities.LegalizeFilename(ProjectName));

        if (Directory.Exists(Folder))
        {
            // Restore current directory to what it was before to load the message box icons
            Directory.SetCurrentDirectory(OldCurrentDirectory);
            MessageBox win = new MessageBox("Warning", $"The folder you are trying to create a new project in, '{Folder}' already exists. Are you sure you want to continue creating a project here? This may overwrite other files.", ButtonType.YesNoCancel, IconType.Warning);
            win.OnClosed += _ =>
            {
                if (win.Result != 0) return;
                // Set the directory back to the new project folder
                Directory.SetCurrentDirectory(Folder);
                ContinueAfterCreatingFolder();
            };
        }
        else
        {
            Directory.CreateDirectory(Folder);
            ContinueAfterCreatingFolder();
        }

        void ContinueAfterCreatingFolder()
        {
            // Turn the (potentially) local folder name to an absolute one for further use
            Folder = Path.GetFullPath(Folder);
            // Restore current directory to what it was before to load the message box icons
            Directory.SetCurrentDirectory(OldCurrentDirectory);

            // The kit we've chosen has been downloaded in the past, so we can copy that to our game folder.
            if (Utilities.KitExists(Kit.Name))
            {
                CopyStep();
            }
            else
            {
                // The kit has not been downloaded before, so we download it now.
                string Filename = Path.Combine(KitsFolder, Kit.Name + ".zip");
                if (!Directory.Exists(KitsFolder)) Directory.CreateDirectory(KitsFolder);
                FileDownloaderWindow window = new FileDownloaderWindow(Kit.Download, Filename, "Downloading kit...");
                window.Download();
                FileInfo fi = new FileInfo(Filename);
                if (fi.Length <= 0)
                {
                    new MessageBox("Error", "Something went wrong while downloading the kit. The downloaded file is empty.", ButtonType.OK, IconType.Error);
                    Logger.Error("The downloaded kit has a file size of 0. This is likely an indication that something went wrong during the downloading phase.");
                    return;
                }
                CopyStep();
            }
        }

        async Task CopyStep()
        {
            ProgressWindow window = new ProgressWindow("Copying", "Copying files...", true, true, true, true);
            CancellationTokenSource src = new CancellationTokenSource();
            window.OnCancelled += () =>
            {
                src.Cancel();
            };
            window.OnFinished += () => Graphics.Schedule(() =>
            {
                CloseProject(false);
                Data.SetProjectPath(Path.Combine(Folder, "Game.rxproj"));
                string mkprojPath = Path.Combine(Folder, "project.mkproj");
                if (MainWindow.CreateEditor())
                {
                    MakeRecentProject();
                    if (!File.Exists(mkprojPath))
                    {
                        DumpProjectSettings();
                    }
                }
            });
            window.SetProgress(0f);
            Stopwatch stopwatch = Stopwatch.StartNew();
            int updateFrequency = 16; // Update the screen every x ms
            await Utilities.CopyKit(Kit.Name, Folder, src, e =>
            {
                if (stopwatch.ElapsedMilliseconds > updateFrequency || e == 1) 
                {
                    stopwatch.Restart();
                    Graphics.Schedule(() => {
                        if (!src.IsCancellationRequested) window.SetProgress(e);
                    });
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
            string path = GeneralSettings.RecentFiles[0][1]; // Project file
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
                if (MainWindow.CreateEditor()) MakeRecentProject();
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
            Graphics.UpdateGraphics();
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
                SetScriptingMode();
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
        MainWindow.MapWidget.SetMap(Data.Maps.ContainsKey(mapid) ? Data.Maps[mapid] : Data.Maps.Count == 0 ? null : Data.Maps.Values.First());
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
        MainWindow.DatabaseWidget.SetMode(Submode, true);
    }

    public static void SetScriptingMode()
    {
        MainWindow.ToolBar.ScriptingMode.SetSelected(true);
        if (MainWindow.MainEditorWidget != null && !MainWindow.MainEditorWidget.Disposed) MainWindow.MainEditorWidget.Dispose();

        MainWindow.MainEditorWidget = new ScriptingWidget(MainWindow.MainGridLayout);
        MainWindow.MainEditorWidget.SetGridRow(3);
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
            "This program is intended to be an editor for games made with Pokémon Essentials.\n" +
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
            "Copyright © 2023 Marijn Herrebout\n\n" +
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
        GeneralSettings.SecondsUsed += (int) Math.Floor((DateTime.Now - TimeOpened).TotalSeconds);
        if (!Directory.Exists(AppDataFolder)) Directory.CreateDirectory(AppDataFolder);
        Logger.WriteLine("Saving general settings to {0}...", SettingsFilePath);
        Stream stream = new FileStream(SettingsFilePath, FileMode.Create, FileAccess.Write);
        Utilities.WriteSerializationID(stream, 0);
        Utilities.SerializeStream(stream, GeneralSettings.RawData);
        stream.Close();
    }

    /// <summary>
    /// Loads the editor's general settings.
    /// </summary>
    public static void LoadGeneralSettings()
    {
        if (File.Exists(SettingsFilePath))
        {
            Logger.WriteLine("Loading general settings from {0}...", SettingsFilePath);
            Stream stream = new FileStream(SettingsFilePath, FileMode.Open, FileAccess.Read);
            Utilities.ReadSerializationID(stream, 0);
            var dict = Utilities.DeserializeStream<Dictionary<string, object>>(stream);
            try 
            {
                GeneralSettings = new GeneralSettings(dict);
                GeneralSettings.Update(); 
            }
            catch (Exception ex)
            {
                Logger.Error(ex); 
                Logger.WriteLine("Creating new backup general settings");
                GeneralSettings = new GeneralSettings();
            }
            stream.Close();
        }
        else
        {
            Logger.WriteLine("Creating new general settings ({0} did not exist)", SettingsFilePath);
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
        // Saves the version into the project file.
        ProjectSettings.SavedVersion = Program.CurrentVersion;
        Logger.WriteLine("Saving project settings to {0}...", Data.ProjectPath + "/project.mkproj");
        Stream stream = new FileStream(Data.ProjectPath + "/project.mkproj", FileMode.Create, FileAccess.Write);
        Utilities.WriteSerializationID(stream, 0);
        Utilities.SerializeStream(stream, ProjectSettings.RawData);
        stream.Close();
    }

    /// <summary>
    /// Loads the current project's settings.
    /// </summary>
    public static void LoadProjectSettings()
    {
        if (File.Exists(Data.ProjectPath + "/project.mkproj"))
        {
            Logger.WriteLine("Loading project settings from {0}...", Data.ProjectPath + "/project.mkproj");
            Stream stream = new FileStream(Data.ProjectPath + "/project.mkproj", FileMode.Open, FileAccess.Read);
            Utilities.ReadSerializationID(stream, 0);
            var dict = Utilities.DeserializeStream<Dictionary<string, object>>(stream);
            try 
            {
                ProjectSettings = new ProjectSettings(dict); 
                ProjectSettings.Update();
            }
            catch (Exception ex) 
            {
                Logger.Error(ex);
                Logger.WriteLine("Creating new backup project settings"); 
                ProjectSettings = new ProjectSettings(); 
            }
            stream.Close();
        }
        else
        {
            Logger.WriteLine("Creating new project settings ({0} did not exist)", Data.ProjectPath + "/project.mkproj");
            ProjectSettings = new ProjectSettings();
        }
    }

    /// <summary>
    /// Clears settings related to the current project. Usually only called after saving and closing a project.
    /// </summary>
    public static void ClearProjectData()
    {
        Logger.WriteLine("Clearing project data...");
        ProjectSettings = null;
        UndoList.Clear();
        RedoList.Clear();
        UnsavedChanges = false;
    }

    public static void AskToUpdate()
    {
        string updaterPath = Path.Combine(MKUtils.MKUtils.ProgramFilesPath, "MK", "Core");
        string updaterFile = Path.Combine(updaterPath, "updater.exe");
        bool hasUpdater = File.Exists(updaterFile);
        if (hasUpdater)
        {
            Logger.WriteLine("Prompt update from {0} to {1}", Program.CurrentVersion, Program.LatestVersion);
            MessageBox win = new MessageBox("Updater", $"An update for RPG Studio MK is available. Would you like to automatically install this update?\nCurrent version: {Program.CurrentVersion}\nLatest version: {Program.LatestVersion}", ButtonType.YesNo, IconType.Info);
            win.OnClosed += _ =>
            {
                if (win.Result == 0) // Yes
                {
                    // Open updater
                    // Close program
                    Logger.WriteLine("Closing editor...");
                    Editor.ExitEditor();
                    Directory.SetCurrentDirectory(updaterPath);
                    Logger.WriteLine("Launching updater...");
                    Process proc = new Process();
                    proc.StartInfo = new ProcessStartInfo("cmd");
                    proc.StartInfo.ArgumentList.Add("/c");
                    proc.StartInfo.ArgumentList.Add("updater.exe");
                    proc.StartInfo.ArgumentList.Add("--automatic-update");
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                }
            };
        }
        else
        {
            Logger.WriteLine("Editor found an update from {0} to {1}, but the updater does not exist at {2}.", Program.CurrentVersion, Program.LatestVersion, updaterFile);
            new MessageBox("Updater", $"An update for RPG Studio MK is available. Please run the original RPG Studio MK installer again to install the new version and the automatic updater.\nCurrent version: {Program.CurrentVersion}\nLatest version: {Program.LatestVersion}", ButtonType.OK, IconType.Info);
        }
        Program.PromptedUpdate = true;
    }

    /// <summary>
    /// Called every tick for logic updates.
    /// </summary>
    public static void Update()
    {
        GameRunner.Update();
        if (Input.Trigger(Keycode.F2))
        {
            Graphics.ShowFrames = !Graphics.ShowFrames;
        }
        if (Program.UpdateAvailable && !Program.PromptedUpdate)
        {
            AskToUpdate();
        }
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

public enum EventGraphicViewMode
{
    BoxOnly,
    BoxAndGraphic,
    BoxAndCroppedGraphic,
    GraphicOnly,
    CroppedGraphicOnly
}