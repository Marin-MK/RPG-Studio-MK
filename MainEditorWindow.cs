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

        public MainEditorWidget MainEditorWidget;
        public Grid MainGridLayout;
        public MenuBar MenuBar;
        public StatusBar StatusBar;
        public ToolBar ToolBar;
        public HomeScreen HomeScreen;

        public MainEditorWindow(string[] args)
        {
            this.SetMinimumSize(600, 400);
            this.SetText("RPG Studio MK");
            this.Initialize();

            this.OnClosing += delegate (object sender, CancelEventArgs e)
            {
                if (!Editor.InProject)
                {
                    // Save window upon top-right Exit button
                    //e.Cancel = true;
                    //EnsureSaved(Dispose);
                }
            };

            this.UI = new UIManager(this);

            // Widgets may now be created

            Editor.MainWindow = this;
            Editor.LoadGeneralSettings();
            Utilities.Initialize();

            #region Grid
            MainGridLayout = new Grid(this);
            MainGridLayout.SetSize(Width, Height);
            /* 0 m m m m m m m m m m m m m
             * 1 t t t t t t t t t t t t t
             * 2 - - - - - - - - - - - - -
             * 3 a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             *   a a a a a a a a a a a a a
             * 4 - - - - - - - - - - - - -
             * 5 s s s s s s s s s s s s s
             * m => menubar
             * t => toolbar
             * a => main editor area
             * s => statusbar
             * - => divider*/
            MainGridLayout.SetRows(
                new GridSize(32, Unit.Pixels),
                new GridSize(31, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(26, Unit.Pixels)
            );

            #endregion
            #region Menubar + Toolbar
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
                        /*new MenuItem("Import Maps")
                        {
                            HelpText = "Import Maps made with RPG Maker XP.",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { Editor.ImportMaps(); },
                            IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Editor.InProject; }
                        },*/
                        new MenuItem("New")
                        {
                            HelpText = "Create a new project.",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(Editor.NewProject); }
                        },
                        new MenuItem("Open")
                        {
                            HelpText = "Open an existing project.",
                            Shortcut = "Ctrl+O",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(Editor.OpenProject); }
                        },
                        new MenuItem("Save")
                        {
                            HelpText = "Save all changes in the current project.",
                            Shortcut = "Ctrl+S",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { Editor.SaveProject(); },
                            IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Editor.InProject; }
                        },
                        new MenuSeparator(),
                        new MenuItem("Close Project")
                        {
                            HelpText = "Close this project and return to the welcome screen.",
                            IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Editor.InProject; },
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(Editor.CloseProject); }
                        },
                        new MenuItem("Exit Editor")
                        {
                            HelpText = "Close this project and quit the program.",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { EnsureSaved(Editor.ExitEditor); }
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
                        {
                            HelpText = "Toggles the visibility of the map grid.\nCurrently unavailable."
                        }
                    }
                },
                new MenuItem("Game")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Play Game")
                        {
                            Shortcut = "F12",
                            HelpText = "Play the game.",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { Editor.StartGame(); },
                            IsClickable = delegate (object sender, ConditionEventArgs e ) { e.ConditionValue = Editor.InProject; }
                        },
                        new MenuItem("Open Game Folder")
                        {
                            HelpText = "Opens the file explorer and navigates to the project folder.",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { Editor.OpenGameFolder(); },
                            IsClickable = delegate (object sender, ConditionEventArgs e ) { e.ConditionValue = Editor.InProject; }
                        }
                    }
                },
                new MenuItem("Help")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Help")
                        {
                            Shortcut = "F1",
                            HelpText = "Opens the help window.",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { OpenHelpWindow(); }
                        },
                        new MenuItem("About RPG Studio MK")
                        {
                            HelpText = "Shows information about this program.",
                            OnLeftClick = delegate (object sender, MouseEventArgs e) { OpenAboutWindow(); }
                        }
                    }
                }
            });


            // Toolbar (modes, icons, etc)
            ToolBar = new ToolBar(MainGridLayout);
            ToolBar.SetGridRow(1);
            #endregion
            #region Dividers
            // Blue 1px separator
            Widget Blue1pxSeparator = new Widget(MainGridLayout);
            Blue1pxSeparator.SetBackgroundColor(DividerColor);
            Blue1pxSeparator.SetGridRow(2);

            // Status bar divider
            Widget StatusBarDivider = new Widget(MainGridLayout);
            StatusBarDivider.SetBackgroundColor(DividerColor);
            StatusBarDivider.SetGridRow(4);
            #endregion
            #region Statusbar
            // Status bar
            StatusBar = new StatusBar(MainGridLayout);
            StatusBar.SetGridRow(5);
            ToolBar.StatusBar = StatusBar;
            #endregion

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

            #region Events
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
            #endregion

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

            MainEditorWidget = new MainEditorWidget(MainGridLayout);
            MainEditorWidget.SetGridRow(3);

            // Link the UI pieces together
            MainEditorWidget.mv.LayersTab = MainEditorWidget.lt;
            MainEditorWidget.mv.TilesetTab = MainEditorWidget.tt;
            MainEditorWidget.mv.ToolBar = ToolBar;
            MainEditorWidget.mv.StatusBar = StatusBar;

            MainEditorWidget.lt.TilesetTab = MainEditorWidget.tt;
            MainEditorWidget.lt.MapViewer = MainEditorWidget.mv;

            MainEditorWidget.tt.LayersTab = MainEditorWidget.lt;
            MainEditorWidget.tt.MapViewer = MainEditorWidget.mv;
            MainEditorWidget.tt.ToolBar = ToolBar;

            MainEditorWidget.mst.MapViewer = MainEditorWidget.mv;

            ToolBar.MapViewer = MainEditorWidget.mv;
            ToolBar.TilesetTab = MainEditorWidget.tt;
            MainEditorWidget.mst.StatusBar = StatusBar;

            StatusBar.MapViewer = MainEditorWidget.mv;

            // Set list of maps & initial map
            MainEditorWidget.mst.PopulateList(Editor.ProjectSettings.MapOrder, true);

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
            MainEditorWidget.mst.SetMap(Data.Maps[mapid]);

            MainEditorWidget.lt.SetSelectedLayer(lastlayer);

            MainEditorWidget.mv.SetZoomFactor(Editor.ProjectSettings.LastZoomFactor);
        }

        public void OpenHelpWindow()
        {
            new MessageBox("Help",
                "As there is no built-in wiki or documentation yet, please direct any questions to the official Discord server or Twitter account.");
        }
        
        public void OpenAboutWindow()
        {
            new MessageBox("About RPG Studio MK",
                "This program is intended to be an editor for games made with the MK Starter Kit.\n" +
                "It was created by Marin, with additional support of various other individuals.\n" +
                "\n" +
                "Please turn to the GitHub page for a full credits list."
            );
        }

        public void EnsureSaved(Action Function)
        {
            if (!Editor.UnsavedChanges)
            {
                Function();
                return;
            }
            MessageBox box = new MessageBox("Warning", "The game contains unsaved changed. Are you sure you would like to proceed? All unsaved changes will be lost.",
                new List<string>() { "Save", "Continue", "Cancel" });
            box.OnButtonPressed += delegate (object sender, EventArgs e)
            {
                if (box.Result == 0) // Save
                {
                    Editor.SaveProject();
                    Function();
                }
                else if (box.Result == 1) // Continue
                {
                    Function();
                }
            };
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
