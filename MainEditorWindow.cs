using System;
using System.Collections.Generic;
using MKEditor.Game;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class MainEditorWindow : Window
    {
        public UIManager UI;
        public bool Blocked = false;
        public IContainer ActiveWidget;
        public List<IContainer> Widgets = new List<IContainer>();

        private MainEditorWidget mew;
        public Grid MainGridLayout;
        public MenuBar MenuBar;
        public StatusBar StatusBar;
        public ToolBar ToolBar;
        public HomeScreen HomeScreen;

        public MainEditorWindow(string[] args)
        {
            Editor.MainWindow = this;
            Editor.LoadGeneralSettings();
            Utilities.Initialize();

            this.SetText("RPG Studio MK");
            this.SetMinimumSize(600, 400);
            this.Initialize();

            this.UI = new UIManager(this);

            // Widgets may now be created

            MainGridLayout = new Grid(this);
            MainGridLayout.SetSize(Width, Height);
            MainGridLayout.SetRows(
                new GridSize(32, Unit.Pixels),
                new GridSize(31, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(26, Unit.Pixels)
            );

            Color DividerColor = new Color(79, 108, 159);

            // Header + Menubar
            MenuBar = new MenuBar(MainGridLayout);
            MenuBar.SetBackgroundColor(28, 50, 73);
            MenuBar.SetGridRow(0);
            MenuBar.SetItems(new List<MenuItem>()
            {
                new MenuItem("File")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("New")
                        {
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(NewProject); }
                        },
                        new MenuItem("Open")
                        {
                            Shortcut = "Ctrl+O",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(OpenProject); }
                        },
                        new MenuItem("Save")
                        {
                            Shortcut = "Ctrl+S",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { Editor.SaveProject(); },
                            IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Editor.InProject; }
                        },
                        new MenuSeparator(),
                        new MenuItem("Close Project")
                        {
                            IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Editor.InProject; },
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(CloseProject); }
                        },
                        new MenuItem("Exit Editor")
                        {
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(ExitEditor); }
                        }
                    }
                },
                new MenuItem("Edit")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Cut"),
                        new MenuItem("Copy") { Shortcut = "Ctrl+C" },
                        new MenuItem("Paste") { Shortcut = "Ctrl+V" },
                        new MenuSeparator(),
                        new MenuItem("Undo") { Shortcut = "Ctrl+Z" },
                        new MenuItem("Redo") { Shortcut = "Ctrl+Y" },
                        new MenuSeparator(),
                        new MenuItem("Delete") { Shortcut = "Del" }
                    }
                },
                new MenuItem("View")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Show/Hide Grid")
                    }
                },
                new MenuItem("Game")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Play Game") { Shortcut = "F12" },
                        new MenuItem("Open Game Folder")
                    }
                },
                new MenuItem("Help")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Help") { Shortcut = "F1" },
                        new MenuItem("About MK Editor")
                    }
                }
            });


            // Toolbar (modes, icons, etc)
            ToolBar = new ToolBar(MainGridLayout);
            ToolBar.SetGridRow(1);


            // Blue 1px separator
            Widget Blue1pxSeparator = new Widget(MainGridLayout);
            Blue1pxSeparator.SetBackgroundColor(DividerColor);
            Blue1pxSeparator.SetGridRow(2);

            // Status bar divider
            Widget StatusBarDivider = new Widget(MainGridLayout);
            StatusBarDivider.SetBackgroundColor(DividerColor);
            StatusBarDivider.SetGridRow(4);

            // Status bar
            StatusBar = new StatusBar(MainGridLayout);
            StatusBar.SetGridRow(5);

            ToolBar.StatusBar = StatusBar;

            bool LoadHomeScreen = true;
            // If an argument was passed, load that project file and skip the home screen
            if (args.Length > 0 && args[0].Contains("project.mkproj"))
            {
                Data.SetProjectPath(args[0]);
                LoadHomeScreen = false;
            }

            if (LoadHomeScreen)
            {
                StatusBar.SetVisible(false);
                ToolBar.SetVisible(false);
                UI.SetBackgroundColor(10, 23, 37);
                HomeScreen = new HomeScreen(MainGridLayout);
                HomeScreen.SetGridRow(3);
            }
            else
            {
                CreateEditor();
                Editor.MakeRecentProject();
            }

            this.OnMouseDown += UI.MouseDown;
            this.OnMousePress += UI.MousePress;
            this.OnMouseUp += UI.MouseUp;
            this.OnMouseMoving += UI.MouseMoving;
            this.OnMouseWheel += UI.MouseWheel;
            this.OnTextInput += UI.TextInput;
            this.OnWindowResized += UI.WindowResized;
            this.OnTick += Tick;
            this.UI.Update();
            this.Start();

            if (Editor.GeneralSettings.WasMaximized) SDL2.SDL.SDL_MaximizeWindow(SDL_Window);
            else
            {
                SetPosition(Editor.GeneralSettings.LastX, Editor.GeneralSettings.LastY);
                SetSize(Editor.GeneralSettings.LastWidth, Editor.GeneralSettings.LastHeight);
                this.UI.WindowResized(null, new WindowEventArgs(Width, Height));
            }
        }

        public void CreateEditor()
        {
            if (HomeScreen != null) HomeScreen.Dispose();

            Editor.LoadProjectSettings();
            Data.LoadGameData();

            mew = new MainEditorWidget(MainGridLayout);
            mew.SetGridRow(3);

            // Link the UI pieces together
            mew.mv.LayersTab = mew.lt;
            mew.mv.TilesetTab = mew.tt;
            mew.mv.ToolBar = ToolBar;
            mew.mv.StatusBar = StatusBar;

            mew.lt.TilesetTab = mew.tt;
            mew.lt.MapViewer = mew.mv;

            mew.tt.LayersTab = mew.lt;
            mew.tt.MapViewer = mew.mv;
            mew.tt.ToolBar = ToolBar;

            mew.mst.MapViewer = mew.mv;

            ToolBar.MapViewer = mew.mv;
            ToolBar.TilesetTab = mew.tt;
            mew.mst.StatusBar = StatusBar;

            StatusBar.MapViewer = mew.mv;

            // Set list of maps & initial map
            mew.mst.PopulateList(Editor.ProjectSettings.MapOrder, true);

            MainGridLayout.UpdateLayout();

            StatusBar.SetVisible(true);
            ToolBar.SetVisible(true);

            int mapid = Editor.ProjectSettings.LastMapID;
            if (!Data.Maps.ContainsKey(mapid))
            {
                if (Editor.ProjectSettings.MapOrder[0] is List<object>) mapid = (int) ((List<object>)Editor.ProjectSettings.MapOrder[0])[0];
                else mapid = (int) Editor.ProjectSettings.MapOrder[0];
            }
            int lastlayer = Editor.ProjectSettings.LastLayer;
            mew.mst.SetMap(Data.Maps[mapid]);

            mew.lt.SetSelectedLayer(lastlayer);

            mew.mv.SetZoomFactor(Editor.ProjectSettings.LastZoomFactor);
        }

        public void EnsureSaved(Action Continue)
        {
            if (!Editor.UnsavedChanges)
            {
                Continue();
                return;
            }
            MessageBox box = new MessageBox("Warning", "The game contains unsaved changed. Are you sure you would like to proceed? All unsaved changes will be lost.",
                new List<string>() { "Save", "Continue", "Cancel" });
            box.OnButtonPressed += delegate (object sender, EventArgs e)
            {
                if (box.Result == 0) // Save
                {
                    Editor.SaveProject();
                    Continue();
                }
                else if (box.Result == 1)
                {
                    Continue();
                }
            };
        }

        public void NewProject()
        {
            CloseProject();
            Editor.NewProject();
        }

        public void OpenProject()
        {
            CloseProject();
            Editor.OpenProject();
        }

        public void CloseProject()
        {
            if (mew != null) mew.Dispose();
            mew = null;
            StatusBar.SetVisible(false);
            ToolBar.SetVisible(false);
            HomeScreen = new HomeScreen(MainGridLayout);
            HomeScreen.SetGridRow(3);
            MainGridLayout.UpdateLayout();
            Data.ClearProjectData();
            Editor.ClearProjectData();
        }

        public void ExitEditor()
        {
            this.Dispose();
        }

        public void SetActiveWidget(IContainer Widget)
        {
            this.ActiveWidget = Widget;
            if (!Widgets.Contains(Widget)) Widgets.Add(Widget);
            if (Graphics.LastMouseEvent is MouseEventArgs) Graphics.LastMouseEvent.Handled = true;
        }

        public void SetOverlayOpacity(byte Opacity)
        {
            TopSprite.Opacity = Opacity;
        }

        public void SetOverlayZIndex(int Z)
        {
            TopViewport.Z = Z;
        }

        private void Tick(object sender, EventArgs e)
        {
            this.UI.Update();
        }
    }
}
