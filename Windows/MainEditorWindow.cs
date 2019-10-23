using System;
using System.Collections.Generic;
using System.IO;
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

        public MainEditorWindow()
        {
            Editor.LoadGeneralSettings();
            Utilities.Initialize();
            Editor.LoadProjectSettings();

            this.SetText("MK Editor");
            this.SetMinimumSize(600, 400);
            this.Initialize();

            Data.Initialize();

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
                Graphics.SetCursor(b);
            }

            this.UI = new UIManager(this);

            // Widgets may now be created

            Grid layout = new Grid(this);
            layout.SetRows(
                new GridSize(32, Unit.Pixels),
                new GridSize(31, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(26, Unit.Pixels)
            );
            layout.SetColumns(
                new GridSize(222, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(283, Unit.Pixels)
            );

            Color DividerColor = new Color(79, 108, 159);

            // Header + Menubar
            MenuBar menu = new MenuBar(layout);
            menu.SetBackgroundColor(28, 50, 73);
            menu.SetGrid(0, 0, 0, 4);
            menu.SetItems(new List<MenuItem>()
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
            ToolBar toolbar = new ToolBar(layout);
            toolbar.SetGrid(1, 1, 0, 4);


            // Blue 1px separator
            Widget Blue1pxSeparator = new Widget(layout);
            Blue1pxSeparator.SetBackgroundColor(DividerColor);
            Blue1pxSeparator.SetGrid(2, 2, 0, 4);


            // Left sidebar
            MapSelectTab mst = new MapSelectTab(layout);
            mst.SetGrid(3, 0);

            // Left sidebar divider
            Widget LeftSidebarDivider = new Widget(layout);
            LeftSidebarDivider.SetBackgroundColor(79, 108, 159);
            LeftSidebarDivider.SetGrid(3, 3, 1, 1);

            // Right sidebar divider
            Widget RightSidebarDivider = new Widget(layout);
            RightSidebarDivider.SetBackgroundColor(DividerColor);
            RightSidebarDivider.SetGrid(3, 3, 3, 3);

            // Right sidebar
            Grid rightcontainer = new Grid(layout);
            rightcontainer.SetGrid(3, 4);
            rightcontainer.SetRows(new GridSize(5), new GridSize(1, Unit.Pixels), new GridSize(2));
            rightcontainer.SetColumns(new GridSize(1));
            rightcontainer.SetBackgroundColor(40, 44, 52);


            // Tileset part of right sidebar
            TilesetTab tt = new TilesetTab(rightcontainer);

            // Inner right sidebar divider
            Widget InnerRightSidebarDivider = new Widget(rightcontainer);
            InnerRightSidebarDivider.SetBackgroundColor(DividerColor);
            InnerRightSidebarDivider.SetGrid(1, 0);

            // Layers part of right sidebar
            LayersTab lt = new LayersTab(rightcontainer);
            lt.SetGrid(2, 0);


            // Center map viewer
            MapViewer mv = new MapViewer(layout);
            mv.SetGrid(3, 2);

            // Status bar divider
            Widget StatusBarDivider = new Widget(layout);
            StatusBarDivider.SetBackgroundColor(DividerColor);
            StatusBarDivider.SetGrid(4, 4, 0, 4);

            // Status bar
            StatusBar status = new StatusBar(layout);
            status.SetGrid(5, 5, 0, 4);



            // Link the UI pieces together
            mv.LayersTab = lt;
            mv.TilesetTab = tt;
            mv.ToolBar = toolbar;
            mv.StatusBar = status;

            lt.TilesetTab = tt;
            lt.MapViewer = mv;

            tt.LayersTab = lt;
            tt.MapViewer = mv;
            tt.ToolBar = toolbar;

            mst.MapViewer = mv;

            toolbar.MapViewer = mv;
            toolbar.TilesetTab = tt;
            toolbar.StatusBar = status;

            mst.StatusBar = status;

            status.MapViewer = mv;


            // Set list of maps & initial map
            mst.PopulateList(Editor.ProjectSettings.MapOrder, true);
            int id;
            if (Editor.ProjectSettings.MapOrder[0] is List<object>) id = (int) ((List<object>) Editor.ProjectSettings.MapOrder[0])[0];
            else id = (int) Editor.ProjectSettings.MapOrder[0];
            mst.SetMap(Data.Maps[id]);


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
