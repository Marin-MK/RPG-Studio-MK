using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MiniMapViewer : Widget
{
    Container MainContainer;
    MapImageWidget MapWidget;
    CursorWidget CursorWidget;

    bool MiddleMouseScrolling;
    Point LastMousePos;

    public int MapTileX = -1;
    public int MapTileY = -1;
    public BaseEvent OnTileChanged;
    public BaseEvent OnTileConfirmed;

    public MiniMapViewer(IContainer Parent) : base(Parent)
    {
        GroupBoxWithScrollBars GroupBox = new GroupBoxWithScrollBars(this);
        GroupBox.SetDocked(true);

        MainContainer = new Container(GroupBox);
        MainContainer.SetDocked(true);
        MainContainer.SetPadding(2, 2, 15, 15);

        VScrollBar vs = new VScrollBar(GroupBox);
        vs.SetRightDocked(true);
        vs.SetVDocked(true);
        vs.SetPadding(0, 3, 1, 16);
        MainContainer.SetVScrollBar(vs);
        MainContainer.VAutoScroll = true;

        HScrollBar hs = new HScrollBar(GroupBox);
        hs.SetBottomDocked(true);
        hs.SetHDocked(true);
        hs.SetPadding(3, 0, 16, 1);
        MainContainer.SetHScrollBar(hs);
        MainContainer.HAutoScroll = true;

        CursorWidget = new CursorWidget(MainContainer);
        CursorWidget.ConsiderInAutoScrollCalculation = false;
        CursorWidget.SetSize(2 * 7 + 32, 2 * 7 + 32);
        CursorWidget.SetVisible(false);

        MapWidget = new MapImageWidget(MainContainer);
        MapWidget.SetGridVisibility(Editor.GeneralSettings.ShowGrid);
        MapWidget.SetMapAnimations(Editor.GeneralSettings.ShowMapAnimations);

        OnWidgetSelected += WidgetSelected;

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER), _ => OnTileConfirmed?.Invoke(new BaseEventArgs()), false, e => e.Value = CursorWidget.Visible)
        });
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (MiddleMouseScrolling && e.MiddleButton)
        {
            int dx = LastMousePos.X - e.X;
            int dy = LastMousePos.Y - e.Y;
            MainContainer.ScrolledX += dx;
            MainContainer.ScrolledY += dy;

            MainContainer.ScrolledX = Math.Clamp(MainContainer.ScrolledX, 0, Math.Max(0, MainContainer.MaxChildWidth - MainContainer.Viewport.Width));
            MainContainer.ScrolledY = Math.Clamp(MainContainer.ScrolledY, 0, Math.Max(0, MainContainer.MaxChildHeight - MainContainer.Viewport.Height));

            MainContainer.UpdateAutoScroll();
        }
        this.LastMousePos = new Point(e.X, e.Y);
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (MainContainer.Mouse.Inside)
        {
            if (Mouse.LeftMouseTriggered)
            {
                int rx = e.X - MainContainer.Viewport.X + MapWidget.LeftCutOff;
                int ry = e.Y - MainContainer.Viewport.Y + MapWidget.TopCutOff;
                int nx = (int) Math.Floor(rx / 32d);
                int ny = (int) Math.Floor(ry / 32d);
                SetCursorPosition(nx, ny, false);
            }
            else if (Mouse.MiddleMouseTriggered)
            {
                Input.SetCursor(CursorType.SizeAll);
                this.MiddleMouseScrolling = true;
                this.LastMousePos = new Point(e.X, e.Y);
                Input.CaptureMouse();
            }
        }
    }

    public override void DoubleLeftMouseDownInside(MouseEventArgs e)
    {
        base.DoubleLeftMouseDownInside(e);
        if (MainContainer.Mouse.Inside)
        {
            OnTileConfirmed?.Invoke(new BaseEventArgs());
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (Mouse.MiddleMouseReleased)
        {
            Input.SetCursor(CursorType.Arrow);
            this.MiddleMouseScrolling = false;
            Input.ReleaseMouse();
        }
    }

    public void SetMap(Map Map)
    {
        MapWidget.SetMap(Map);
        SetCursorPosition(0, 0);
    }

    public void SetGridVisibility(bool GridVisibility)
    {
        MapWidget.SetGridVisibility(GridVisibility);
    }

    public void SetMapAnimations(bool MapAnimations)
    {
        MapWidget.SetMapAnimations(MapAnimations);
    }

    public void SetCursorPosition(int TileX, int TileY, bool Center = true)
    {
        int OldMapTileX = MapTileX;
        int OldMapTileY = MapTileY;
        MapTileX = TileX;
        MapTileY = TileY;
        if (MapTileX != OldMapTileX || MapTileY != OldMapTileY)
        {
            if (Center)
            {
                int cx = (int) Math.Round((32 * MapTileX + 16) * MapWidget.ZoomFactor);
                int cy = (int) Math.Round((32 * MapTileY + 16) * MapWidget.ZoomFactor);
                int maxw = (int) Math.Round(MapWidget.MapData.Width * 32 * MapWidget.ZoomFactor);
                int maxh = (int) Math.Round(MapWidget.MapData.Height * 32 * MapWidget.ZoomFactor);
                int winw = MainContainer.Size.Width;
                int winh = MainContainer.Size.Height;
                MainContainer.ScrolledX = Math.Clamp(cx - winw / 2, 0, Math.Max(0, maxw - winw));
                MainContainer.ScrolledY = Math.Clamp(cy - winh / 2, 0, Math.Max(0, maxh - winh));
                MainContainer.UpdateAutoScroll();
            }
            CancelDoubleClick();
            UpdateCursorPosition();
            OnTileChanged?.Invoke(new BaseEventArgs());
        }
    }

    private void UpdateCursorPosition()
    {
        int x = -7 + 32 * MapTileX;
        int y = -7 + 32 * MapTileY;
        CursorWidget.SetPosition(x, y);
        CursorWidget.SetVisible(true);
    }
}
