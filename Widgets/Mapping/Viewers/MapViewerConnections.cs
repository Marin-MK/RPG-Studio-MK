using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewerConnections : MapViewerBase
    {
        public bool HoveringAbove = false;
        public bool HoveringLeft = false;
        public bool HoveringRight = false;
        public bool HoveringBelow = false;

        Widget Hover;

        public MapViewerConnections(object Parent, string Name = "mapViewerConnections")
            : base(Parent, Name)
        {
            Hover = new Widget(MainContainer);
            Hover.SetBackgroundColor(35, 61, 89);
            Hover.SetVisible(false);
            Hover.ConsiderInAutoScrollCalculation = false;
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            int rx = e.X - MapWidget.Viewport.X;
            int ry = e.Y - MapWidget.Viewport.Y;
            int movedx = MapWidget.Position.X - MapWidget.ScrolledPosition.X;
            int movedy = MapWidget.Position.Y - MapWidget.ScrolledPosition.Y;
            if (movedx >= MapWidget.Position.X)
            {
                movedx -= MapWidget.Position.X;
                rx += movedx;
            }
            if (movedy >= MapWidget.Position.Y)
            {
                movedy -= MapWidget.Position.Y;
                ry += movedy;
            }
            HoveringAbove = ry < 0;
            HoveringLeft = rx < 0;
            HoveringRight = rx >= MapWidget.Size.Width;
            HoveringBelow = ry >= MapWidget.Size.Height;
            if (!MainContainer.WidgetIM.Hovering)
            {
                HoveringAbove = HoveringLeft = HoveringRight = HoveringBelow = false;
            }
            ProcessHovering((int) Math.Floor(rx / 32d / ZoomFactor), (int) Math.Floor(ry / 32d / ZoomFactor));
        }

        public void ProcessHovering(int MouseX, int MouseY)
        {
            int x = 0;
            int y = 0;
            int w = MapWidget.Size.Width + MapWidget.Position.X * 2;
            int h = MapWidget.Size.Height + MapWidget.Position.Y * 2;
            if (HoveringAbove && !HoveringLeft && !HoveringRight)
            {
                h = MapWidget.Position.Y;
                int? BottomX = null;
                int? BottomX_Width = null;
                int? TopX = null;
                int? TopX_Width = null;
                foreach (Connection c in Map.Connections[":north"])
                {
                    Map m = Data.Maps[c.MapID];
                    int mx = c.Offset;
                    int mw = m.Width;
                    if (MouseX >= mx && MouseX < mx + mw)
                    {
                        // Selected map m
                        Hover.SetVisible(false);
                        HoverMap(m);
                        return;
                    }
                    else if (MouseX < mx)
                    {
                        if (BottomX == null || mx < BottomX)
                        {
                            BottomX = mx;
                            BottomX_Width = MapWidget.Position.X + (int) Math.Floor(mx * 32d * ZoomFactor);
                        }
                    }
                    else if (MouseX >= mx + mw)
                    {
                        TopX = MouseX;
                        TopX_Width = MapWidget.Position.X + (int) Math.Floor((mx + mw) * 32d * ZoomFactor);
                    }
                }
                foreach (Connection c in Map.Connections[":west"])
                {
                    if (c.Offset < 0 && TopX == null)
                    {
                        TopX = 0;
                        TopX_Width = MapWidget.Position.X;
                    }
                }
                foreach (Connection c in Map.Connections[":east"])
                {
                    if (c.Offset < 0 && BottomX == null)
                    {
                        BottomX = Map.Width;
                        BottomX_Width = MapWidget.Position.X + (int) Math.Floor(Map.Width * 32d * ZoomFactor);
                    }
                }
                if (BottomX != null && BottomX_Width != null)
                {
                    w = (int) BottomX_Width;
                }
                if (TopX != null && TopX_Width != null)
                {
                    x = (int) TopX_Width;
                    w -= x;
                }
            }
            else if (HoveringLeft && !HoveringAbove && !HoveringBelow)
            {
                w = MapWidget.Position.X;
                int? BottomY = null;
                int? BottomY_Height = null;
                int? TopY = null;
                int? TopY_Height = null;
                foreach (Connection c in Map.Connections[":west"])
                {
                    Map m = Data.Maps[c.MapID];
                    int my = c.Offset;
                    int mh = m.Height;
                    if (MouseY >= my && MouseY < my + mh)
                    {
                        // Selected map m
                        Hover.SetVisible(false);
                        HoverMap(m);
                        return;
                    }
                    else if (MouseY < my)
                    {
                        if (BottomY == null || my < BottomY)
                        {
                            BottomY = my;
                            BottomY_Height = MapWidget.Position.Y + (int) Math.Floor(my * 32d * ZoomFactor);
                        }
                    }
                    else if (MouseY >= my + mh)
                    {
                        TopY = MouseY;
                        TopY_Height = MapWidget.Position.Y + (int) Math.Floor((my + mh) * 32d * ZoomFactor);
                    }
                }
                foreach (Connection c in Map.Connections[":north"])
                {
                    if (c.Offset < 0 && TopY == null)
                    {
                        TopY = 0;
                        TopY_Height = MapWidget.Position.Y;
                    }
                }
                foreach (Connection c in Map.Connections[":south"])
                {
                    if (c.Offset < 0 && BottomY == null)
                    {
                        BottomY = Map.Height;
                        BottomY_Height = MapWidget.Position.Y + (int) Math.Floor(Map.Height * 32d * ZoomFactor);
                    }
                }
                if (BottomY != null && BottomY_Height != null)
                {
                    h = (int) BottomY_Height;
                }
                if (TopY != null && TopY_Height != null)
                {
                    y = (int) TopY_Height;
                    h -= y;
                }
            }
            else if (HoveringRight && !HoveringAbove && !HoveringBelow)
            {
                x = MapWidget.Position.X + MapWidget.Size.Width;
                w = MapWidget.Position.X;
                int? BottomY = null;
                int? BottomY_Height = null;
                int? TopY = null;
                int? TopY_Height = null;
                foreach (Connection c in Map.Connections[":east"])
                {
                    Map m = Data.Maps[c.MapID];
                    int my = c.Offset;
                    int mh = m.Height;
                    if (MouseY >= my && MouseY < my + mh)
                    {
                        // Selected map m
                        Hover.SetVisible(false);
                        HoverMap(m);
                        return;
                    }
                    else if (MouseY < my)
                    {
                        if (BottomY == null || my < BottomY)
                        {
                            BottomY = my;
                            BottomY_Height = MapWidget.Position.Y + (int) Math.Floor(my * 32d * ZoomFactor);
                        }
                    }
                    else if (MouseY >= my + mh)
                    {
                        TopY = MouseY;
                        TopY_Height = MapWidget.Position.Y + (int) Math.Floor((my + mh) * 32d * ZoomFactor);
                    }
                }
                foreach (Connection c in Map.Connections[":north"])
                {
                    if (c.Offset + Data.Maps[c.MapID].Width >= Map.Width && TopY == null)
                    {
                        TopY = 0;
                        TopY_Height = MapWidget.Position.Y;
                    }
                }
                foreach (Connection c in Map.Connections[":south"])
                {
                    if (c.Offset + Data.Maps[c.MapID].Width >= Map.Width && BottomY == null)
                    {
                        BottomY = Map.Height;
                        BottomY_Height = MapWidget.Position.Y + (int) Math.Floor(Map.Height * 32d * ZoomFactor);
                    }
                }
                if (BottomY != null && BottomY_Height != null)
                {
                    h = (int) BottomY_Height;
                }
                if (TopY != null && TopY_Height != null)
                {
                    y = (int) TopY_Height;
                    h -= y;
                }
            }
            else if (HoveringBelow && !HoveringLeft && !HoveringRight)
            {
                y = MapWidget.Position.Y + MapWidget.Size.Height;
                h = MapWidget.Position.Y;
                int? BottomX = null;
                int? BottomX_Width = null;
                int? TopX = null;
                int? TopX_Width = null;
                foreach (Connection c in Map.Connections[":south"])
                {
                    Map m = Data.Maps[c.MapID];
                    int mx = c.Offset;
                    int mw = m.Width;
                    if (MouseX >= mx && MouseX < mx + mw)
                    {
                        // Selected map m
                        Hover.SetVisible(false);
                        HoverMap(m);
                        return;
                    }
                    else if (MouseX < mx)
                    {
                        if (BottomX == null || mx < BottomX)
                        {
                            BottomX = mx;
                            BottomX_Width = MapWidget.Position.X + (int) Math.Floor(mx * 32d * ZoomFactor);
                        }
                    }
                    else if (MouseX >= mx + mw)
                    {
                        TopX = MouseX;
                        TopX_Width = MapWidget.Position.X + (int) Math.Floor((mx + mw) * 32d * ZoomFactor);
                    }
                }
                foreach (Connection c in Map.Connections[":west"])
                {
                    if (c.Offset + Data.Maps[c.MapID].Height >= Map.Height && TopX == null)
                    {
                        TopX = 0;
                        TopX_Width = MapWidget.Position.X;
                    }
                }
                foreach (Connection c in Map.Connections[":east"])
                {
                    if (c.Offset + Data.Maps[c.MapID].Height >= Map.Height && BottomX == null)
                    {
                        BottomX = Map.Width;
                        BottomX_Width = MapWidget.Position.X + (int) Math.Floor(Map.Width * 32d * ZoomFactor);
                    }
                }
                if (BottomX != null && BottomX_Width != null)
                {
                    w = (int) BottomX_Width;
                }
                if (TopX != null && TopX_Width != null)
                {
                    x = (int) TopX_Width;
                    w -= x;
                }
            }
            else if (!HoveringAbove && !HoveringLeft && !HoveringRight && !HoveringBelow || !Hover.WidgetIM.Hovering)
            {
                Hover.SetVisible(false);
                return;
            }
            else return;
            Hover.SetPosition(x, y);
            Hover.SetSize(w, h);
            Hover.SetVisible(true);
        }

        public override void SetZoomFactor(double factor, bool FromStatusBar = false)
        {
            base.SetZoomFactor(factor, FromStatusBar);
            MouseEventArgs e = Graphics.LastMouseEvent;
            int rx = e.X - MapWidget.Viewport.X;
            int ry = e.Y - MapWidget.Viewport.Y;
            int movedx = MapWidget.Position.X - MapWidget.ScrolledPosition.X;
            int movedy = MapWidget.Position.Y - MapWidget.ScrolledPosition.Y;
            if (movedx >= MapWidget.Position.X)
            {
                movedx -= MapWidget.Position.X;
                rx += movedx;
            }
            if (movedy >= MapWidget.Position.Y)
            {
                movedy -= MapWidget.Position.Y;
                ry += movedy;
            }
            HoveringAbove = HoveringLeft = HoveringRight = HoveringBelow = false;
            ProcessHovering((int) Math.Floor(rx / 32d / ZoomFactor), (int) Math.Floor(ry / 32d / ZoomFactor));
            MouseMoving(null, Graphics.LastMouseEvent);
        }

        public override void PositionMap()
        {
            base.PositionMap();
            MouseMoving(null, Graphics.LastMouseEvent);
        }

        public void HoverMap(Map map)
        {

        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
        }
    }

    public class MapConnectionWidget : MapImageWidget
    {
        public int MapID;
        public string Side;
        public int Offset;
        public int PixelOffset => (int) Math.Round(this.Offset * 32 * ZoomFactor);
        public int Depth;

        public MapConnectionWidget(object Parent, string Name = "mapConnectionWidget")
            : base(Parent, Name)
        {

        }

        public override void UpdateSize()
        {
            int Width = (int) Math.Round(MapData.Width * 32 * ZoomFactor);
            int Height = (int) Math.Round(MapData.Height * 32 * ZoomFactor);
            if (Side == ":north" || Side == ":south") Height = (int) Math.Round(Math.Min(Depth, MapData.Height) * 32 * ZoomFactor);
            else if (Side == ":east" || Side == ":west") Width = (int) Math.Round(Math.Min(Depth, MapData.Width) * 32 * ZoomFactor);
            this.SetSize(Width, Height);
        }

        public override void LoadLayers(Map MapData, string Side = "", int Offset = 0)
        {
            this.MapID = MapData.ID;
            this.MapData = MapData;
            this.Side = Side;
            this.Offset = Offset;
            UpdateSize();
            RedrawLayers();
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
            int SX = 0;
            int SY = 0;
            int Width = MapData.Width;
            int Height = MapData.Height;
            if (this.Side == ":north")
            {
                SY = MapData.Height - Depth;
                Height = Depth;
            }
            else if (this.Side == ":east")
            {
                Width = Depth;
            }
            else if (this.Side == ":south")
            {
                Height = Depth;
            }
            else if (this.Side == ":west")
            {
                SX = MapData.Width - Depth;
                Width = Depth;
            }
            List<Bitmap> bmps = GetBitmaps(MapData.ID, SX, SY, Width, Height);
            for (int i = 0; i < bmps.Count; i++) Sprites[i.ToString()].Bitmap = bmps[i];
            // Zoom layers
            SetZoomFactor(ZoomFactor);
        }
    }
}
