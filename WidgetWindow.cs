using System;
using System.Collections.Generic;
using System.IO;
using MKEditor.Data;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class WidgetWindow : Window
    {
        public UIManager UI;
        public bool Blocked = false;

        public WidgetWindow()
        {
            GameData.Initialize("D:\\Desktop\\MK\\MK\\data");

            Widget.Setup();

            this.SetSize(1080, 720);
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
                Graphics.SetCursor(b);
            }
            this.UI = new UIManager(this);

            Grid layout = new Grid(this);
            layout.SetRows(
                new GridSize(32, Unit.Pixels),
                new GridSize(63, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1)
            );
            layout.SetColumns(
                new GridSize(224, Unit.Pixels),
                new GridSize(1),
                new GridSize(293, Unit.Pixels)
            );


            // Header + Menubar
            MenuBar menu = new MenuBar(layout)
                .SetBackgroundColor(28, 50, 73)
                .SetGrid(0, 0, 0, 2) as MenuBar;
            menu.SetItems(new List<MenuItem>()
            {
                new MenuItem("File")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("New"),
                        new MenuItem("Open") { Shortcut = "Ctrl+O" },
                        new MenuItem("Save") { Shortcut = "Ctrl+S" },
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
                        new MenuItem("Focus Selected Layer"),
                        new MenuItem("Show Grid"),
                        new MenuItem("Zoom 1:1"),
                        new MenuItem("Zoom 1:2"),
                        new MenuItem("Zoom 1:4")
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
            Widget w = new Widget(layout)
                .SetBackgroundColor(10, 23, 37)
                .SetGrid(1, 1, 0, 2);


            // Blue 1px separator
            new Widget(layout)
                .SetBackgroundColor(79, 108, 159)
                .SetGrid(2, 2, 0, 2);


            // Left sidebar
            MapSelectTab mst = new MapSelectTab(layout);
            mst.SetGrid(3, 0);


            // Right sidebar
            Grid rightcontainer = new Grid(layout).SetGrid(3, 2) as Grid;
            rightcontainer.SetRows(new GridSize(2), new GridSize(1));
            rightcontainer.SetColumns(new GridSize(1));
            rightcontainer.SetBackgroundColor(40, 44, 52);


            // Tileset part of right sidebar
            TilesetTab tt = new TilesetTab(rightcontainer);

            // Layers part of right sidebar
            LayersTab lt = new LayersTab(rightcontainer);
            lt.SetGrid(1, 0);


            // Center map viewer
            MapViewer mv = new MapViewer(layout);
            mv.SetGrid(3, 1);



            // Link the UI pieces together
            mv.LayersTab = lt;
            mv.TilesetTab = tt;
            lt.TilesetTab = tt;
            lt.MapViewer = mv;
            tt.LayersTab = lt;
            tt.MapViewer = mv;
            mst.MapViewer = mv;

            // Set initial map
            Map map = null;
            foreach (Map m in GameData.Maps.Values) { map = m; break; }
            mv.SetMap(map);

            // TEMP: Create map properties window
            //new MapPropertiesWindow(this);

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
        }

        private void Tick(object sender, EventArgs e)
        {
            this.UI.Update();
        }
    }
}
