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

        public Container MainContainer;
        public MapImageWidget MapWidget;
        public Widget DummyWidget;
        public VignetteFade Fade;
        public Container HScrollContainer;
        public Container VScrollContainer;

        public MapViewerBase(object Parent, string Name = "mapViewer")
            : base(Parent, Name)
        {
            this.SetBackgroundColor(28, 50, 73);
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
            this.WidgetIM.OnMouseUp += MouseUp;
            this.WidgetIM.OnMouseWheel += MouseWheel;
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
            HScrollBar = new HScrollBar(HScrollContainer);
            HScrollBar.SetPosition(1, 2);
            HScrollBar.SetZIndex(1);
            HScrollBar.OnValueChanged += delegate (object sender, EventArgs e)
            {
                if (Graphics.LastMouseEvent != null) MouseMoving(sender, Graphics.LastMouseEvent);
            };
            VScrollContainer = new Container(GridLayout);
            VScrollContainer.SetGridColumn(1);
            VScrollBar = new VScrollBar(VScrollContainer);
            VScrollBar.SetPosition(2, 1);
            VScrollBar.SetZIndex(1);
            VScrollBar.OnValueChanged += delegate (object sender, EventArgs e)
            {
                if (Graphics.LastMouseEvent != null) MouseMoving(sender, Graphics.LastMouseEvent);
            };

            MainContainer.SetHScrollBar(HScrollBar);
            MainContainer.SetVScrollBar(VScrollBar);

            Fade = new VignetteFade(MainContainer);
            Fade.ConsiderInAutoScrollCalculation = Fade.ConsiderInAutoScrollPositioning = false;
        }

        public virtual void SetZoomFactor(double factor, bool FromStatusBar = false)
        {
            this.ZoomFactor = factor;
            Editor.ProjectSettings.LastZoomFactor = factor;
            MapWidget.SetZoomFactor(factor);
            if (!FromStatusBar) Editor.MainWindow.StatusBar.ZoomControl.SetZoomFactor(factor, true);
            PositionMap();
            MouseMoving(null, Graphics.LastMouseEvent);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            GridLayout.SetSize(this.Size);
            PositionMap();

            (Sprites["hslider"].Bitmap as SolidBitmap).SetSize(HScrollContainer.Size);
            Sprites["hslider"].Y = MainContainer.Size.Height + 1;

            (Sprites["vslider"].Bitmap as SolidBitmap).SetSize(VScrollContainer.Size);
            Sprites["vslider"].X = MainContainer.Size.Width + 1;

            Sprites["block"].X = Sprites["vslider"].X - 1;
            Sprites["block"].Y = Sprites["hslider"].Y - 1;

            HScrollBar.SetWidth(HScrollContainer.Size.Width - 2);
            VScrollBar.SetHeight(VScrollContainer.Size.Height - 2);
            
            Fade.SetSize(MainContainer.Size);
        }

        public virtual void SetMap(Map Map)
        {
            this.Map = Map;
            Editor.MainWindow.StatusBar.SetMap(Map);
            PositionMap();
            if (MainContainer.HScrollBar != null) MainContainer.HScrollBar.SetValue(0.5);
            if (MainContainer.VScrollBar != null) MainContainer.VScrollBar.SetValue(0.5);
        }

        public virtual void PositionMap()
        {
            int x = 0;
            if (Map.Width * 32 * ZoomFactor < MainContainer.Viewport.Width)
            {
                x = MainContainer.Viewport.Width / 2 - (int) Math.Round(Map.Width * 32 * ZoomFactor / 2d);
            }
            else
            {
                x = MainContainer.Viewport.Width / 4;
            }
            int y = 0;
            if (Map.Height * 32 * ZoomFactor < MainContainer.Viewport.Height)
            {
                y = MainContainer.Viewport.Height / 2 - (int) Math.Round(Map.Height * 32 * ZoomFactor / 2d);
            }
            else
            {
                y = MainContainer.Viewport.Height / 4;
            }
            MapWidget.SetPosition(x, y);
            MapWidget.SetSize((int) Math.Round(Map.Width * 32 * ZoomFactor), (int) Math.Round(Map.Height * 32 * ZoomFactor));
            DummyWidget.SetSize(2 * x + MapWidget.Size.Width, 2 * y + MapWidget.Size.Height);
            if (Map.Width * 32 * ZoomFactor >= Viewport.Width || Map.Height * 32 * ZoomFactor >= Viewport.Height)
            {
                MainContainer.HScrollBar.SetValue(0.5);
                MainContainer.VScrollBar.SetValue(0.5);
            }
            MainContainer.UpdateAutoScroll();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
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
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            // Update position - to make sure you're drawing where the mouse is, not where the cursor is
            // (the cursor obviously follows the mouse with this call if they're not aligned (which they should be))
            MouseMoving(sender, e);
            if (e.MiddleButton != e.OldMiddleButton && e.MiddleButton)
            {
                if (WidgetIM.Hovering)
                {
                    Input.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
                    this.MiddleMouseScrolling = true;
                    LastMouseX = e.X;
                    LastMouseY = e.Y;
                    Input.CaptureMouse();
                }
            }
        }

        public override void MouseUp(object sender, MouseEventArgs e)
        {
            MouseMoving(sender, e);
            base.MouseUp(sender, e);
            if (e.MiddleButton != e.OldMiddleButton && !e.MiddleButton)
            {
                Input.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
                this.MiddleMouseScrolling = false;
                Input.ReleaseMouse();
            }
        }

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            base.MouseWheel(sender, e);
            if (!Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LCTRL) && !Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RCTRL)) return;
            if (e.WheelY > 0) Editor.MainWindow.StatusBar.ZoomControl.IncreaseZoom();
            else Editor.MainWindow.StatusBar.ZoomControl.DecreaseZoom();
        }
    }
}
