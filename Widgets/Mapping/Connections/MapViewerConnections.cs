﻿using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewerConnections : MapViewerBase
    {
        Map HoveringMap;

        public ConnectionsPanel ConnectionsPanel;

        public Dictionary<int, ColoredBox> Overlays = new Dictionary<int, ColoredBox>();
        public List<Widget> TileOverlaps = new List<Widget>();

        public MapViewerConnections(object Parent, string Name = "mapViewerConnections")
            : base(Parent, Name)
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
            MouseMoving(null, Graphics.LastMouseEvent);
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
                mcw.Outline.SetZIndex(3);
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

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
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
                overlapwidget.SetZIndex(2);
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

        public MapConnectionWidget(object Parent, string Name = "mapConnectionWidget")
            : base(Parent, Name)
        {
            this.GridBackground.SetVisible(false);
            this.SetDarkOverlay(192);
        }

        public override void RedrawLayers()
        {
            foreach (string s in this.Sprites.Keys)
            {
                if (s != "_bg" && s != "dark") this.Sprites[s].Dispose();
            }
            // Create layers
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()] = new Sprite(this.Viewport);
                this.Sprites[i.ToString()].Z = i * 2;
                this.Sprites[i.ToString()].Visible = MapData.Layers[i].Visible;
            }
            List<Bitmap> bmps = GetBitmaps(MapData.ID, 0, 0, MapData.Width, MapData.Height);
            for (int i = 0; i < bmps.Count; i++) Sprites[i.ToString()].Bitmap = bmps[i];
            // Zoom layers
            SetZoomFactor(ZoomFactor);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (Outline != null) Outline.SetSize(this.Size);
        }

        public override void PositionChanged(object sender, EventArgs e)
        {
            base.PositionChanged(sender, e);
            if (Outline != null) Outline.SetPosition(this.Position);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Outline != null) Outline.Dispose();
        }
    }
}
