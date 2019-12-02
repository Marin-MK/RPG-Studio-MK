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

        public Grid MainGridLayout;
        public MenuBar MenuBar;
        public StatusBar StatusBar;
        public ToolBar ToolBar;
        public HomeScreen HomeScreen;

        public MainEditorWindow(string[] args)
        {
            Editor.LoadGeneralSettings();
            Utilities.Initialize();

            this.SetText("RPG Studio MK");
            this.SetMinimumSize(600, 400);
            this.Initialize();

            using (Bitmap b = new Bitmap(9, 14)) // Set cursor
            {
                Color gray = new Color(55, 51, 55);
                Color white = Color.WHITE;
                b.Unlock();
                b.DrawLine(0, 0, 0, 12, gray);
                b.DrawLine(1, 0, 8, 7, gray);
                b.SetPixel(8, 8, gray);
                b.SetPixel(8, 9, gray);
                b.SetPixel(7, 9, gray);
                b.SetPixel(6, 9, gray);
                b.SetPixel(6, 10, gray);
                b.SetPixel(7, 11, gray);
                b.SetPixel(7, 12, gray);
                b.SetPixel(7, 13, gray);
                b.SetPixel(6, 13, gray);
                b.SetPixel(5, 13, gray);
                b.SetPixel(4, 12, gray);
                b.SetPixel(4, 11, gray);
                b.SetPixel(3, 10, gray);
                b.SetPixel(2, 11, gray);
                b.SetPixel(1, 12, gray);
                b.DrawLine(1, 1, 1, 11, white);
                b.DrawLine(2, 2, 2, 10, white);
                b.DrawLine(3, 3, 3, 9, white);
                b.DrawLine(4, 4, 4, 10, white);
                b.DrawLine(5, 5, 5, 12, white);
                b.DrawLine(6, 6, 6, 8, white);
                b.SetPixel(7, 7, white);
                b.SetPixel(7, 8, white);
                b.SetPixel(6, 11, white);
                b.SetPixel(6, 12, white);
                b.Lock();
                Input.SetCursor(b);
            }

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
                        new MenuItem("New"),
                        new MenuItem("Open") { Shortcut = "Ctrl+O" },
                        new MenuItem("Save") { Shortcut = "Ctrl+S" },
                        new MenuSeparator(),
                        new MenuItem("Close Project"),
                        new MenuItem("Exit Editor")
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

            MainEditorWidget mew = new MainEditorWidget(MainGridLayout);
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
            int id;
            if (Editor.ProjectSettings.MapOrder[0] is List<object>) id = (int)((List<object>) Editor.ProjectSettings.MapOrder[0])[0];
            else id = (int) Editor.ProjectSettings.MapOrder[0];
            mew.mst.SetMap(Data.Maps[id]);

            MainGridLayout.UpdateLayout();

            StatusBar.SetVisible(true);
            ToolBar.SetVisible(true);
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
