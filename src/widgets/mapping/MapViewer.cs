﻿using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class MapViewer : Widget
{
    public Map Map;
    public MapMode? Mode;

    public Grid GridLayout;

    public int RelativeMouseX = 0;
    public int RelativeMouseY = 0;
    public int MapTileX = 0;
    public int MapTileY = 0;

    public bool MiddleMouseScrolling = false;
    public int LastMouseX = 0;
    public int LastMouseY = 0;

    public bool UsingLeft = false;
    public bool UsingRight = false;

    public virtual int TopLeftX
    {
        get
        {
            int x = MapTileX;
            if (Mode == MapMode.Tiles && (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight)) x -= CursorWidth;
            return x;
        }
    }
    public virtual int TopLeftY
    {
        get
        {
            int y = MapTileY;
            if (Mode == MapMode.Tiles && (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight)) y -= CursorHeight;
            return y;
        }
    }
    public int CursorWidth = 0;
    public int CursorHeight = 0;

    public double ZoomFactor = 1.0;

    public int Depth = 10;

    public Container MainContainer;
    public MapImageWidget MapWidget;
    public Widget DummyWidget;
    public VignetteFade Fade;
    public Container HScrollContainer;
    public Container VScrollContainer;
    public Widget SidebarWidgetTiles;
    public Widget SidebarWidgetEvents;
    public CursorWidget Cursor;

    Container NoMapContainer;

    HintWindow HintWindow;

    public MapViewer(IContainer Parent) : base(Parent)
    {
        this.SetBackgroundColor(28, 50, 73);
        this.OnWidgetSelected += WidgetSelected;

        GridLayout = new Grid(this);
        GridLayout.SetColumns(
            new GridSize(1),
            new GridSize(10, Unit.Pixels),
            new GridSize(288, Unit.Pixels)
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
        HScrollBar.SetZIndex(1);
        HScrollBar.SetValue(0.5);
        HScrollBar.SetHDocked(true);
        HScrollBar.SetPadding(1, 2, 1, 0);
        HScrollBar.OnValueChanged += delegate (BaseEventArgs e)
        {
            if (Editor.MainWindow.MapWidget != null) Editor.MainWindow.MapWidget.SetHorizontalScroll(HScrollBar.Value);
            MouseMoving(Graphics.LastMouseEvent);
        };
        VScrollContainer = new Container(GridLayout);
        VScrollContainer.SetGridColumn(1);
        VScrollBar VScrollBar = new VScrollBar(VScrollContainer);
        VScrollBar.SetZIndex(1);
        VScrollBar.SetValue(0.5);
        VScrollBar.SetVDocked(true);
        VScrollBar.SetPadding(2, 1, 0, 1);
        VScrollBar.OnValueChanged += delegate (BaseEventArgs e)
        {
            if (Editor.MainWindow.MapWidget != null) Editor.MainWindow.MapWidget.SetVerticalScroll(VScrollBar.Value);
            MouseMoving(Graphics.LastMouseEvent);
        };

        MainContainer.SetHScrollBar(HScrollBar);
        MainContainer.SetVScrollBar(VScrollBar);

        Fade = new VignetteFade(MainContainer);
        Fade.ConsiderInAutoScrollCalculation = Fade.ConsiderInAutoScrollPositioningX = Fade.ConsiderInAutoScrollPositioningY = false;
        Fade.SetDocked(true);
        Fade.SetZIndex(9);

        Cursor = new CursorWidget(MainContainer);
        Cursor.ConsiderInAutoScrollCalculation = false;
        Cursor.SetZIndex(8);

        ConstructorTiles();
        ConstructorEvents();
        SidebarWidgetTiles.SetVisible(true);
        SidebarWidgetEvents.SetVisible(false);

        HintWindow = new HintWindow(MainContainer);
        HintWindow.ConsiderInAutoScrollCalculation = HintWindow.ConsiderInAutoScrollPositioningX = HintWindow.ConsiderInAutoScrollPositioningY = false;
        HintWindow.SetBottomDocked(true);
        HintWindow.SetPadding(-3, 0, 0, -9);
        HintWindow.SetZIndex(10);
        HintWindow.SetVisible(false);

        MapWidget = new MapImageWidget(MainContainer);
        MapWidget.MapViewer = this;
        MapWidget.SetZIndex(3);
        MapWidget.SetGridVisibility(Editor.GeneralSettings.ShowGrid);
        MapWidget.SetMapAnimations(Editor.GeneralSettings.ShowMapAnimations);

        RegisterShortcuts(new List<Shortcut>()
        {
            /* Tiles mode */
            new Shortcut(this, new Key(Keycode.ESCAPE), CancelSelection, false, e => e.Value = Mode == MapMode.Tiles),
            new Shortcut(this, new Key(Keycode.A, Keycode.CTRL), SelectAll, false, e => e.Value = Mode == MapMode.Tiles),
            new Shortcut(this, new Key(Keycode.Q, Keycode.SHIFT), SetDrawModePencil, false, e => e.Value = Mode == MapMode.Tiles),
            new Shortcut(this, new Key(Keycode.W, Keycode.SHIFT), SetDrawModeRectangle, false, e => e.Value = Mode == MapMode.Tiles),
            new Shortcut(this, new Key(Keycode.E, Keycode.SHIFT), SetDrawModeEllipse, false, e => e.Value = Mode == MapMode.Tiles),
            new Shortcut(this, new Key(Keycode.R, Keycode.SHIFT), SetDrawModeBucket, false, e => e.Value = Mode == MapMode.Tiles),
            new Shortcut(this, new Key(Keycode.T, Keycode.SHIFT), SetDrawModeEraser, false, e => e.Value = Mode == MapMode.Tiles),
            new Shortcut(this, new Key(Keycode.Y, Keycode.SHIFT), SetDrawModeSelection, false, e => e.Value = Mode == MapMode.Tiles),

            /* Events mode */
            new Shortcut(this, new Key(Keycode.ENTER), _ => OpenOrCreateEventCursorIsOver(), false, e => e.Value = Mode == MapMode.Events),
            new Shortcut(this, new Key(Keycode.LEFT), _ => MoveCursorLeft(), false, e => e.Value = Mode == MapMode.Events),
            new Shortcut(this, new Key(Keycode.RIGHT), _ => MoveCursorRight(), false, e => e.Value = Mode == MapMode.Events),
            new Shortcut(this, new Key(Keycode.UP), _ => MoveCursorUp(), false, e => e.Value = Mode == MapMode.Events),
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveCursorDown(), false, e => e.Value = Mode == MapMode.Events),
            new Shortcut(this, new Key(Keycode.DELETE), _ => DeleteEventCursorIsOver(), false, e => e.Value = Mode == MapMode.Events)
        });
    }

    private partial void ConstructorTiles();
    private partial void ConstructorEvents();

    public void SetMode(MapMode Mode)
    {
        if (this.Mode == Mode) return;
        this.Mode = Mode;
        Editor.ProjectSettings.LastMappingSubmode = (MapMode) this.Mode;
        SidebarWidgetTiles.SetVisible(this.Mode == MapMode.Tiles);
        SidebarWidgetEvents.SetVisible(this.Mode == MapMode.Events);
        if (this.Mode == MapMode.Events || this.Mode == MapMode.Tiles && Editor.ProjectSettings.ShowEventBoxesInTilesSubmode) ShowEventBoxes();
        else HideEventBoxes();
        MapTileX = -1;
        MapTileY = -1;
        CursorWidth = CursorHeight = 0;
        Cursor.SetVisible(false);
        Editor.MainWindow.MapWidget.SubmodePicker.SelectTab((int) Mode);
        ClearMenus();
        //RegisterMenuTiles();
        RegisterMenuEvents();
    }

    private void ClearMenus()
    {
        MapWidget.SetContextMenuList(new List<IMenuItem>());
    }

    private partial void RegisterMenuEvents();

    public virtual void SetZoomFactor(double factor, bool FromStatusBar = false)
    {
        int oldmapx = MapWidget.Position.X;
        int oldmapy = MapWidget.Position.Y;
        int oldrx = LastMouseX - MainContainer.Viewport.X;
        int oldry = LastMouseY - MainContainer.Viewport.Y;
        int oldscrolledx = MainContainer.ScrolledX;
        int oldscrolledy = MainContainer.ScrolledY;
        double factordiff = factor / this.ZoomFactor;
        this.ZoomFactor = factor;
        Editor.ProjectSettings.LastZoomFactor = factor;
        MapWidget.SetZoomFactor(factor);
        if (!FromStatusBar) Editor.MainWindow.StatusBar.ZoomControl.SetZoomFactor(factor, true);
        PositionMap();
        MouseMoving(Graphics.LastMouseEvent);

        if (!MainContainer.Mouse.Inside) // Using control scrolling, so base it off where the user is looking
        {
            // Using the zoom buttons, so take the relative coordinates to be the middle of the screen
            oldrx = MainContainer.Size.Width / 2;
            oldry = MainContainer.Size.Height / 2;
        }

        int tx = oldrx + oldscrolledx - oldmapx;
        int totalw = MainContainer.MaxChildWidth - MainContainer.Viewport.Width;
        int x = MapWidget.Position.X;
        double addx = tx * factordiff;
        double scrollx = x + addx;
        scrollx -= oldrx;
        MainContainer.HScrollBar.SetValue(scrollx / totalw);

        int ty = oldry + oldscrolledy - oldmapy;
        int totalh = MainContainer.MaxChildHeight - MainContainer.Viewport.Height;
        int y = MapWidget.Position.Y;
        double addy = ty * factordiff;
        double scrolly = y + addy;
        scrolly -= oldry;
        MainContainer.VScrollBar.SetValue(scrolly / totalh);
    }

    public void UpdateCursorPosition()
    {
        int offsetx = 0;
        int offsety = 0;
        if (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight)
            offsetx = (int)Math.Round(32 * CursorWidth * ZoomFactor);
        if (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight)
            offsety = (int)Math.Round(32 * CursorHeight * ZoomFactor);
        Cursor.SetPosition(
            MapWidget.Position.X + (int)Math.Round(32 * MapTileX * ZoomFactor) - offsetx - 7,
            MapWidget.Position.Y + (int)Math.Round(32 * MapTileY * ZoomFactor) - offsety - 7
        );
        Cursor.SetSize(
            (int)Math.Round(32 * ZoomFactor) * (CursorWidth + 1) + 14,
            (int)Math.Round(32 * ZoomFactor) * (CursorHeight + 1) + 14
        );
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (Size.Width == 50 && Size.Height == 50) return;
        PositionMap();

        (Sprites["hslider"].Bitmap as SolidBitmap).SetSize(HScrollContainer.Size);
        Sprites["hslider"].Y = MainContainer.Size.Height + 1;

        (Sprites["vslider"].Bitmap as SolidBitmap).SetSize(VScrollContainer.Size);
        Sprites["vslider"].X = MainContainer.Size.Width + 1;

        Sprites["block"].X = Sprites["vslider"].X - 1;
        Sprites["block"].Y = Sprites["hslider"].Y - 1;
    }

    public virtual void SetMapAnimations(bool MapAnimations)
    {
        MapWidget.SetMapAnimations(MapAnimations);
    }

    public virtual void SetGridVisibility(bool GridVisibility)
    {
        MapWidget.SetGridVisibility(GridVisibility);
    }

    public virtual void SetMap(Map Map)
    {
        this.Map = Map;

        MapWidget.SetMap(Map);
        MapWidget.SetVisible(Map != null);
        Cursor.SetVisible(Map != null && MainContainer.Mouse.Inside);

        CancelSelection(new BaseEventArgs());
        LayerPanel.CreateLayers();
        TilesPanel.SetMap(Map);
        TilesPanel.SelectTile(new TileData() { TileType = TileType.Tileset, Index = 0, ID = 0 });

        if (Mode == MapMode.Events)
        {
            MapTileX = -1;
            MapTileY = -1;
            CursorWidth = 0;
            CursorHeight = 0;
            Cursor.SetVisible(false);
        }
        EventsPanel.SetMap(Map);
        DrawEvents();
        if (Mode != MapMode.Events && (Mode != MapMode.Tiles || !Editor.ProjectSettings.ShowEventBoxesInTilesSubmode)) HideEventBoxes();

        Editor.MainWindow.StatusBar.SetMap(Map);
        PositionMap();
        MainContainer.HScrollBar.SetValue(0.5);
        MainContainer.VScrollBar.SetValue(0.5);

        if (Map == null)
        {
            NoMapContainer = new Container(MainContainer);
            Label NoMapsLabel = new Label(NoMapContainer);
            NoMapsLabel.SetFont(Fonts.Paragraph);
            NoMapsLabel.SetText("You don't have any maps yet. Get started by creating one now.");
            NoMapsLabel.RedrawText(true);
            NoMapContainer.SetSize(NoMapsLabel.Size.Width, 64);
            Button CreateMapButton = new Button(NoMapContainer);
            CreateMapButton.SetWidth(128);
            CreateMapButton.SetPosition(NoMapContainer.Size.Width / 2 - CreateMapButton.Size.Width / 2, 30);
            CreateMapButton.SetText("Create Map");
            CreateMapButton.OnClicked += _ =>
            {
                // TODO
                //Editor.MainWindow.MapWidget.MapSelectPanel.NewMap(new BaseEventArgs());
            };
            PositionMap();
        }
        else
        {
            NoMapContainer?.Dispose();
            NoMapContainer = null;
        }
    }

    public virtual void PositionMap()
    {
        if (Map == null)
        {
            MapWidget.SetPosition(0, 0);
            MapWidget.SetSize(1, 1);
            DummyWidget.SetSize(MainContainer.Size);
            NoMapContainer?.SetPosition(MainContainer.Size.Width / 2 - NoMapContainer.Size.Width / 2, MainContainer.Size.Height / 2 - NoMapContainer.Size.Height / 2);
            return;
        }
        int w = (int)Math.Round(Map.Width * 32d * ZoomFactor);
        int h = (int)Math.Round(Map.Height * 32d * ZoomFactor);
        int minx = MainContainer.Size.Width / 2 - w / 2;
        int miny = MainContainer.Size.Height / 2 - h / 2;
        if (minx - 12 * 32d * ZoomFactor < 0) minx = (int)Math.Round(12 * 32d * ZoomFactor);
        if (miny - 12 * 32d * ZoomFactor < 0) miny = (int)Math.Round(12 * 32d * ZoomFactor);
        int x = minx;
        int y = miny;
        /*foreach (MapConnection c in Map.Connections)
        {
            int leftx = (int) Math.Round((-c.RelativeX + 2) * 32d * ZoomFactor);
            int rightx = (int) Math.Round((c.RelativeX - Map.Width + Data.Maps[c.MapID].Width + 2) * 32d * ZoomFactor);
            int uppery = (int) Math.Round((-c.RelativeY + 2) * 32d * ZoomFactor);
            int lowery = (int) Math.Round((c.RelativeY - Map.Height + Data.Maps[c.MapID].Height + 2) * 32d * ZoomFactor);
            x = Math.Max(x, Math.Max(leftx, rightx));
            y = Math.Max(y, Math.Max(uppery, lowery));
        }*/
        MapWidget.SetPosition(x, y);
        MapWidget.SetSize(w, h);
        DummyWidget.SetSize(2 * x + w, 2 * y + h);
        MainContainer.UpdateBounds();
        MainContainer.UpdateAutoScroll();

        if (Mode == MapMode.Tiles) UpdateSelection();
        if (Mode == MapMode.Events || Mode == MapMode.Tiles && Editor.ProjectSettings.ShowEventBoxesInTilesSubmode) RepositionEvents();
    }

    private partial void MouseMovingTiles(MouseEventArgs e);
    private partial void MouseMovingEvents(MouseEventArgs e);

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (MiddleMouseScrolling && e.MiddleButton)
        {
            int dx = LastMouseX - e.X;
            int dy = LastMouseY - e.Y;
            MainContainer.ScrolledX += dx;
            MainContainer.ScrolledY += dy;

            MainContainer.ScrolledX = Math.Clamp(MainContainer.ScrolledX, 0, Math.Max(0, MainContainer.MaxChildWidth - MainContainer.Viewport.Width));
            MainContainer.ScrolledY = Math.Clamp(MainContainer.ScrolledY, 0, Math.Max(0, MainContainer.MaxChildHeight - MainContainer.Viewport.Height));

            MainContainer.UpdateAutoScroll();
            if (Editor.MainWindow.MapWidget != null)
            {
                Editor.MainWindow.MapWidget.SetHorizontalScroll(MainContainer.HScrollBar.Value);
                Editor.MainWindow.MapWidget.SetVerticalScroll(MainContainer.VScrollBar.Value);
            }
        }
        MouseMovingTiles(e);
        MouseMovingEvents(e);
        LastMouseX = e.X;
        LastMouseY = e.Y;
    }

    private partial void MouseDownTiles(MouseEventArgs e);
    private partial void MouseDownEvents(MouseEventArgs e);

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        // Update position - to make sure you're drawing where the mouse is, not where the cursor is
        // (the cursor will also follow the mouse with this call if they're not aligned (which they should be))
        MouseMoving(e);
        if (MainContainer.Mouse.Inside)
        {
            if (Mouse.LeftMouseTriggered) UsingLeft = true;
            if (Mouse.RightMouseTriggered) UsingRight = true;
            if (Mouse.MiddleMouseTriggered)
            {
                Input.SetCursor(CursorType.SizeAll);
                this.MiddleMouseScrolling = true;
                LastMouseX = e.X;
                LastMouseY = e.Y;
                Input.CaptureMouse();
            }
        }
        MouseDownTiles(e);
        MouseDownEvents(e);
    }

    private partial void MouseUpTiles(MouseEventArgs e);
    private partial void MouseUpEvents(MouseEventArgs e);

    public override void MouseUp(MouseEventArgs e)
    {
        if (Mouse.Accessible) MouseMoving(e);
        base.MouseUp(e);
        if (Mouse.LeftMouseReleased) UsingLeft = false;
        if (Mouse.RightMouseReleased) UsingRight = false;
        if (Mouse.MiddleMouseReleased)
        {
            Input.SetCursor(CursorType.Arrow);
            this.MiddleMouseScrolling = false;
            Input.ReleaseMouse();
        }
        MouseUpTiles(e);
        MouseUpEvents(e);
    }

    public override void MouseWheel(MouseEventArgs e)
    {
        base.MouseWheel(e);
        if (!Input.Press(Keycode.CTRL) || !MainContainer.Mouse.Inside) return;
        if (e.WheelY > 0) Editor.MainWindow.StatusBar.ZoomControl.IncreaseZoom();
        else Editor.MainWindow.StatusBar.ZoomControl.DecreaseZoom();
    }

    private partial void UpdateTiles();

    public override void Update()
    {
        base.Update();
        if (!SelectedWidget) return;
        if (Input.Press(Keycode.CTRL))
        {
            if (Input.Trigger(Keycode.C)) // Copy
            {
                Copy();
            }
            else if (Input.Trigger(Keycode.X)) // Cut
            {
                Cut();
            }
            else if (Input.Trigger(Keycode.V)) // Paste
            {
                Paste();
            }
        }
        UpdateTiles();
    }

    private partial void CopyTiles(bool Cut);
    private partial void CutTiles();
    private partial void PasteTiles();
    private partial void CopyEvents(bool Cut);
    private partial void CutEvents();
    private partial void PasteEvents();

    public void Copy()
    {
        if (Mode == MapMode.Tiles) CopyTiles(false);
        else if (Mode == MapMode.Events) CopyEvents(false);
    }

    public void Cut()
    {
        if (Mode == MapMode.Tiles) CutTiles();
        else if (Mode == MapMode.Events) CutEvents();
    }

    public void Paste()
    {
        if (Mode == MapMode.Tiles) PasteTiles();
        else if (Mode == MapMode.Events) PasteEvents();
    }

    public void SetHint(string HintText)
    {
        HintWindow.SetText(HintText);
    }

    public void DrawTile(int X, int Y, int Layer, TileData Tile, TileData OldTile, bool ForceUpdateNearbyAutotiles = false, bool MakeNeighboursUndoable = true)
    {
        MapWidget.DrawTile(X, Y, Layer, Tile, OldTile, ForceUpdateNearbyAutotiles, MakeNeighboursUndoable);
    }

    public bool IsLayerLocked(int Layer)
    {
        return MapWidget.IsLayerLocked(Layer);
    }

    public void SetLayerLocked(int Layer, bool Locked)
    {
        MapWidget.SetLayerLocked(Layer, Locked);
    }
}