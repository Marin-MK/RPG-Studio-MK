using System;
using System.Collections.Generic;
using MKEditor.Game;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class MapViewerConnections : MapViewerBase
    {
        Map HoveringMap;

        public ConnectionsPanel ConnectionsPanel;

        public Dictionary<int, ColoredBox> Overlays = new Dictionary<int, ColoredBox>();
        public List<Widget> TileOverlaps = new List<Widget>();

        public MapViewerConnections(IContainer Parent) : base(Parent)
        {
            GridLayout.Columns.Add(new GridSize(288, Unit.Pixels));
            GridLayout.UpdateContainers();

            // Right sidebar
            ConnectionsPanel = new ConnectionsPanel(GridLayout);
            ConnectionsPanel.SetGrid(0, 1, 2, 2);
        }

        public override void PositionMap()
        {
            base.PositionMap();
            MouseMoving(Graphics.LastMouseEvent);
            TestForOverlap();
        }

        public void HoverMap(Map Map)
        {
            if (Map == HoveringMap) return;
            HoveringMap = Map;
        }

        public override void RedrawConnectedMaps()
        {
            base.RedrawConnectedMaps();
            foreach (MapConnectionWidget mcw in ConnectionWidgets)
            {
                mcw.Outline = new ColoredBox(MainContainer);
                mcw.Outline.SetZIndex(5); // 1 for map connections, 2 for the selected map connection, 3 for the main map, 4 for tile overlaps, so 5 for outlines.
                mcw.Outline.SetPosition(mcw.Position);
                mcw.Outline.SetSize(mcw.Size);
            }
        }

        public override void SetMap(Map Map)
        {
            base.SetMap(Map);
            ConnectionsPanel.SetMap(Map);
            TestForOverlap();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            if (this.Size.Width != 50 && this.Size.Height != 50) TestForOverlap();
        }

        public void TestForOverlap()
        {
            foreach (Widget overlapwidget in TileOverlaps) overlapwidget.Dispose();
            TileOverlaps.Clear();
            List<List<int>> overlaps = new List<List<int>>();
            foreach (MapConnectionWidget a in ConnectionWidgets)
            {
                foreach (MapConnectionWidget b in ConnectionWidgets)
                {
                    if (a == b) continue;
                    TestForOverlapBetweenMaps(a, b);
                }
                TestForOverlapBetweenMaps(a, MapWidget);
                TestForOverlapBetweenMaps(MapWidget, a);
            }
        }

        public void TestForOverlapBetweenMaps(MapImageWidget a, MapImageWidget b)
        {
            if (new Rect(a.Position, a.Size).Overlaps(new Rect(b.Position, b.Size)))
            {
                // Intersection of the two maps
                int nx = Math.Max(a.Position.X, b.Position.X);
                int ny = Math.Max(a.Position.Y, b.Position.Y);
                int nw = Math.Min(a.Position.X + a.Size.Width, b.Position.X + b.Size.Width) - nx;
                int nh = Math.Min(a.Position.Y + a.Size.Height, b.Position.Y + b.Size.Height) - ny;
                Widget overlapwidget = new Widget(MainContainer);
                overlapwidget.SetZIndex(4); // 1 for map connections, 2 for the selected map connection, 3 for the main map, so 4 for overlaps
                overlapwidget.SetPosition(nx, ny);
                overlapwidget.SetSize(nw, nh);
                overlapwidget.SetBackgroundColor(255, 22, 47, 103);
                TileOverlaps.Add(overlapwidget);
            }
        }
    }

    public class MapConnectionWidget : MapImageWidget
    {
        public ColoredBox Outline;

        public MapConnectionWidget(IContainer Parent) : base(Parent)
        {
            this.GridBackground.SetVisible(false);
            this.SetDarkOverlay(192);
            this.WidgetIM.SelectWithRightClick = false;
            this.WidgetIM.SelectWithMiddleClick = false;
            OnWidgetSelected += WidgetSelected;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && !e.Handled && e.LeftButton != e.OldLeftButton) // Ensure only one map connection is ever selected (saves an extra call when clicking an overlapping section)
            {
                e.Handled = true;
                Editor.MainWindow.MapWidget.MapViewerConnections.ConnectionsPanel.SelectMap(this.MapData);
            }
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            if (Outline != null) Outline.SetSize(this.Size);
        }

        public override void PositionChanged(BaseEventArgs e)
        {
            base.PositionChanged(e);
            if (Outline != null) Outline.SetPosition(this.Position);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Outline != null && !Outline.Disposed) Outline.Dispose();
        }
    }
}
