using System;
using System.Collections.Generic;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class WidgetWindow : Window
    {
        public UIManager UI;

        public WidgetWindow()
        {
            this.SetSize(800, 400);

            this.Initialize();

            this.SetBackgroundColor(240, 240, 240);

            this.UI = new UIManager(this);

            Grid layout = new Grid(this);
            layout.SetRows(
                new GridSize(37, Unit.Pixels),
                new GridSize(33, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1)
            );
            layout.SetColumns(
                new GridSize(234, Unit.Pixels),
                new GridSize(1),
                new GridSize(333, Unit.Pixels)
            );

            new Widget(layout)
                .SetBackgroundColor(40, 44, 52)
                .SetGrid(0, 0, 0, 2);

            new Widget(layout)
                .SetBackgroundColor(27, 28, 32)
                .SetGrid(1, 1, 0, 2);

            new Widget(layout)
                .SetBackgroundColor(255, 191, 31)
                .SetGrid(2, 2, 0, 2);

            new Widget(layout)
                .SetBackgroundColor(27, 28, 32)
                .SetGrid(3, 0);

            Grid rightcontainer = new Grid(layout).SetGrid(3, 2) as Grid;
            rightcontainer.SetRows(new GridSize(2), new GridSize(4, Unit.Pixels), new GridSize(1));
            rightcontainer.SetColumns(new GridSize(1));
            rightcontainer.SetBackgroundColor(40, 44, 52);

            MKD.Tileset tileset = MKD.Tileset.GetTileset();
            TilesetTab tt = new TilesetTab(rightcontainer);
            tt.SetBackgroundColor(27, 28, 32);
            tt.SetTileset(tileset);

            LayerTab lt = new LayerTab(rightcontainer);
            lt.SetGrid(2, 0);
            lt.SetBackgroundColor(27, 28, 32);

            MKD.Map map = MKD.Map.CreateTemp();
            MapViewer mv = new MapViewer(layout);
            mv.SetBackgroundColor(40, 44, 52);
            mv.SetGrid(3, 1);
            mv.SetMap(map);

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
