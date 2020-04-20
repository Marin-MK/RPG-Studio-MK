using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewerBase : Widget
    {
        public Map Map;

        public Grid GridLayout;

        public int RelativeMouseX = 0;
        public int RelativeMouseY = 0;
        public int MapTileX = 0;
        public int MapTileY = 0;

        public bool MiddleMouseScrolling = false;
        public int LastMouseX = 0;
        public int LastMouseY = 0;

        public double ZoomFactor = 1.0;

        public List<MapConnectionWidget> ConnectionWidgets = new List<MapConnectionWidget>();

        public int Depth = 10;

        public Container MainContainer;
        public MapImageWidget MapWidget;
        public Widget DummyWidget;
        public VignetteFade Fade;
        public Container HScrollContainer;
        public Container VScrollContainer;

        public MapViewerBase(IContainer Parent) : base(Parent)
        {
            this.SetBackgroundColor(28, 50, 73);
            this.OnWidgetSelected += WidgetSelected;

            GridLayout = new Grid(this);
            GridLayout.SetColumns(
                new GridSize(1),
                new GridSize(11, Unit.Pixels)
            );
            GridLayout.SetRows(
                new GridSize(1),
                new GridSize(11, Unit.Pixels)
            );
            MainContainer = new Container(GridLayout);
            MainContainer.HAutoScroll = MainContainer.VAutoScroll = true;
            DummyWidget = new Widget(MainContainer);
            Sprites["hslider"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width - 13, 11, new Color(10, 23, 37)));
            Sprites["vslider"] = new Sprite(this.Viewport, new SolidBitmap(11, Size.Height - 13, new Color(10, 23, 37)));
            Sprites["block"] = new Sprite(this.Viewport, new SolidBitmap(12, 12, new Color(64, 104, 146)));
            HScrollContainer = new Container(GridLayout);
            HScrollContainer.SetGridRow(1);
            HScrollBar HScrollBar = new HScrollBar(HScrollContainer);
            HScrollBar.SetPosition(1, 2);
            HScrollBar.SetZIndex(1);
            HScrollBar.SetValue(0.5);
            HScrollBar.OnValueChanged += delegate (BaseEventArgs e)
            {
                Editor.MainWindow.MapWidget.SetHorizontalScroll(HScrollBar.Value);
                MouseMoving(Graphics.LastMouseEvent);
            };
            VScrollContainer = new Container(GridLayout);
            VScrollContainer.SetGridColumn(1);
            VScrollBar VScrollBar = new VScrollBar(VScrollContainer);
            VScrollBar.SetPosition(2, 1);
            VScrollBar.SetZIndex(1);
            VScrollBar.SetValue(0.5);
            VScrollBar.OnValueChanged += delegate (BaseEventArgs e)
            {
                Editor.MainWindow.MapWidget.SetVerticalScroll(VScrollBar.Value);
                MouseMoving(Graphics.LastMouseEvent);
            };

            MainContainer.SetHScrollBar(HScrollBar);
            MainContainer.SetVScrollBar(VScrollBar);

            Fade = new VignetteFade(MainContainer);
            Fade.ConsiderInAutoScrollCalculation = Fade.ConsiderInAutoScrollPositioning = false;
            Fade.SetZIndex(7);
        }

        public virtual void SetZoomFactor(double factor, bool FromStatusBar = false)
        {
            this.ZoomFactor = factor;
            Editor.ProjectSettings.LastZoomFactor = factor;
            MapWidget.SetZoomFactor(factor);
            if (!FromStatusBar) Editor.MainWindow.StatusBar.ZoomControl.SetZoomFactor(factor, true);
            ConnectionWidgets.ForEach(w => w.SetZoomFactor(factor));
            PositionMap();
            MouseMoving(Graphics.LastMouseEvent);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            GridLayout.SetSize(this.Size);
            PositionMap();

            (Sprites["hslider"].Bitmap as SolidBitmap).SetSize(HScrollContainer.Size);
            Sprites["hslider"].Y = MainContainer.Size.Height + 1;

            (Sprites["vslider"].Bitmap as SolidBitmap).SetSize(VScrollContainer.Size);
            Sprites["vslider"].X = MainContainer.Size.Width + 1;

            Sprites["block"].X = Sprites["vslider"].X - 1;
            Sprites["block"].Y = Sprites["hslider"].Y - 1;

            MainContainer.HScrollBar.SetWidth(HScrollContainer.Size.Width - 2);
            MainContainer.VScrollBar.SetHeight(VScrollContainer.Size.Height - 2);
            
            Fade.SetSize(MainContainer.Size);
        }

        public virtual void SetMap(Map Map)
        {
            this.Map = Map;
            Editor.MainWindow.StatusBar.SetMap(Map);
            RedrawConnectedMaps();
            PositionMap();
            if (MainContainer.HScrollBar != null) MainContainer.HScrollBar.SetValue(0.5);
            if (MainContainer.VScrollBar != null) MainContainer.VScrollBar.SetValue(0.5);
            UpdateConnectionPositions();
            PositionMap();
        }

        public virtual void PositionMap()
        {
            if (Editor.MainWindow.MapWidget.Submodes.SelectedIndex != -1 && this != Editor.MainWindow.MapWidget.ActiveMapViewer) return;
            int w = (int) Math.Round(Map.Width * 32d * ZoomFactor);
            int h = (int) Math.Round(Map.Height * 32d * ZoomFactor);
            int minx = MainContainer.Size.Width / 2 - w / 2;
            int miny = MainContainer.Size.Height / 2 - h / 2;
            if (minx - 12 * 32d * ZoomFactor < 0) minx = (int) Math.Round(12 * 32d * ZoomFactor);
            if (miny - 12 * 32d * ZoomFactor < 0) miny = (int) Math.Round(12 * 32d * ZoomFactor);
            int x = 0;
            int y = 0;
            foreach (MapConnection c in Map.Connections)
            {
                int leftx = (int) Math.Round((-c.RelativeX + 2) * 32d * ZoomFactor);
                int rightx = (int) Math.Round((c.RelativeX - Map.Width + Data.Maps[c.MapID].Width + 2) * 32d * ZoomFactor);
                int uppery = (int) Math.Round((-c.RelativeY + 2) * 32d * ZoomFactor);
                int lowery = (int) Math.Round((c.RelativeY - Map.Height + Data.Maps[c.MapID].Height + 2) * 32d * ZoomFactor);
                x = Math.Max(x, Math.Max(leftx, rightx));
                y = Math.Max(y, Math.Max(uppery, lowery));
            }
            x = Math.Max(x, minx);
            y = Math.Max(y, miny);
            MapWidget.SetPosition(x, y);
            MapWidget.SetSize(w, h);
            UpdateConnectionPositions();
            DummyWidget.SetSize(2 * x + w, 2 * y + h);
            MainContainer.UpdateAutoScroll();
            MainContainer.HScrollBar.SetValue(0.5);
            MainContainer.VScrollBar.SetValue(0.5);
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            if (MiddleMouseScrolling && e.MiddleButton)
            {
                int dx = LastMouseX - e.X;
                int dy = LastMouseY - e.Y;
                MainContainer.ScrolledX += dx;
                MainContainer.ScrolledY += dy;
                LastMouseX = e.X;
                LastMouseY = e.Y;

                MainContainer.ScrolledX = Math.Max(0, Math.Min(MainContainer.ScrolledX, MainContainer.MaxChildWidth - MainContainer.Viewport.Width));
                MainContainer.ScrolledY = Math.Max(0, Math.Min(MainContainer.ScrolledY, MainContainer.MaxChildHeight - MainContainer.Viewport.Height));

                MainContainer.UpdateAutoScroll();
                Editor.MainWindow.MapWidget.SetHorizontalScroll(MainContainer.HScrollBar.Value);
                Editor.MainWindow.MapWidget.SetVerticalScroll(MainContainer.VScrollBar.Value);
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            // Update position - to make sure you're drawing where the mouse is, not where the cursor is
            // (the cursor will also follow the mouse with this call if they're not aligned (which they should be))
            MouseMoving(e);
            if (e.MiddleButton != e.OldMiddleButton && e.MiddleButton && WidgetIM.Hovering)
            {
                Input.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
                this.MiddleMouseScrolling = true;
                LastMouseX = e.X;
                LastMouseY = e.Y;
                Input.CaptureMouse();
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            if (WidgetIM.Ready() && IsVisible() && WidgetIM.WidgetAccessible()) MouseMoving(e);
            base.MouseUp(e);
            if (e.MiddleButton != e.OldMiddleButton && !e.MiddleButton)
            {
                Input.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
                this.MiddleMouseScrolling = false;
                Input.ReleaseMouse();
            }
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            base.MouseWheel(e);
            if (!Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LCTRL) && !Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RCTRL)) return;
            if (e.WheelY > 0) Editor.MainWindow.StatusBar.ZoomControl.IncreaseZoom();
            else Editor.MainWindow.StatusBar.ZoomControl.DecreaseZoom();
        }

        public virtual void UpdateConnectionPositions()
        {
            for (int i = 0; i < ConnectionWidgets.Count; i++)
            {
                MapConnectionWidget mcw = ConnectionWidgets[i];
                mcw.SetZoomFactor(ZoomFactor);
                mcw.UpdateSize();
                mcw.SetPosition(MapWidget.Position.X + (int) Math.Round(mcw.RelativeX * 32d * ZoomFactor),
                    MapWidget.Position.Y + (int) Math.Round(mcw.RelativeY * 32d * ZoomFactor));
            }
        }

        public virtual void RedrawConnectedMaps()
        {
            foreach (MapConnectionWidget mcw in ConnectionWidgets) mcw.Dispose();
            ConnectionWidgets.Clear();
            MapWidget.Rect = new Rect(0, 0, Map.Width, Map.Height);
            foreach (MapConnection c in Map.Connections)
            {
                Map map = Data.Maps[c.MapID];
                MapConnectionWidget mcw = new MapConnectionWidget(MainContainer);
                mcw.LoadLayers(map, c.RelativeX, c.RelativeY);
                ConnectionWidgets.Add(mcw);
            }
        }
    }
}
