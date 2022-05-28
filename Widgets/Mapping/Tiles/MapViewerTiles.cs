using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public partial class MapViewer
{
    public TilesPanel TilesPanel;
    public LayerPanel LayerPanel;

    public Point OriginPoint;
    public Location CursorOrigin;
    public List<TileData> TileDataList = new List<TileData>();
    public bool SelectionOnMap = false;
    public int SelectionX = -1;
    public int SelectionY = -1;
    public int SelectionWidth = 0;
    public int SelectionHeight = 0;

    // Used to determine whether an undo is needed for rectangle drawing
    Location MoveDirection = Location.TopLeft;
    Location CursorDirectionFromOrigin = Location.TopLeft;

    bool IgnoreLeftButton = false;
    bool IgnoreRightButton = false;

    bool CursorInActiveSelection = false;
    bool MovingSelection = false;
    bool SelectionStartUndoLast = false;
    bool SelectionFromPaste = false;
    Point SelectionMouseOrigin;
    Point SelectionTileOrigin;
    MapSelection SelectionTiles;

    public SelectionBackground SelectionBackground;

    private partial void ConstructorTiles()
    {
        /**
         * Create the Tiles mode widgets
         */

        // Right sidebar grid
        Grid sidebargrid = new Grid(GridLayout);
        sidebargrid.SetGrid(0, 1, 2, 2);
        sidebargrid.SetRows(new GridSize(5), new GridSize(2, Unit.Pixels), new GridSize(2));
        sidebargrid.SetColumns(new GridSize(1));
        sidebargrid.SetVisible(Visible);
        SidebarWidgetTiles = sidebargrid;

        // Tileset part of right sidebar
        TilesPanel = new TilesPanel(sidebargrid);
        TilesPanel.SetBackgroundColor(28, 50, 73);
        TilesPanel.MapViewer = this;
        Editor.MainWindow.MapWidget.TilesPanel = TilesPanel;

        // Inner right sidebar divider
        Widget InnerRightSidebarDivider = new Widget(sidebargrid);
        InnerRightSidebarDivider.SetBackgroundColor(10, 23, 37);
        InnerRightSidebarDivider.SetGridRow(1);

        // Layers part of right sidebar
        LayerPanel = new LayerPanel(sidebargrid);
        LayerPanel.SetBackgroundColor(28, 50, 73);
        LayerPanel.SetGridRow(2);
        LayerPanel.MapViewer = this;
        Editor.MainWindow.MapWidget.LayerPanel = LayerPanel;

        SelectionBackground = new SelectionBackground(MainContainer);
        SelectionBackground.SetZIndex(7);

        Editor.OnUndoing += delegate (BaseEventArgs e)
        {
            if (Mode == MapMode.Tiles && SelectionWidth != 0 && SelectionHeight != 0) CancelSelection(e);
        };
    }

    public void SetDrawModePencil(BaseEventArgs e)
    {
        TilesPanel.DrawTool = DrawTools.Pencil;
    }

    public void SetDrawModeBucket(BaseEventArgs e)
    {
        TilesPanel.DrawTool = DrawTools.Bucket;
    }

    public void SetDrawModeEllipse(BaseEventArgs e)
    {
        TilesPanel.DrawTool = DrawTools.EllipseFilled;
    }

    public void SetDrawModeRectangle(BaseEventArgs e)
    {
        TilesPanel.DrawTool = DrawTools.RectangleFilled;
    }

    public void SetDrawModeSelection(BaseEventArgs e)
    {
        TilesPanel.DrawTool = DrawTools.SelectionActiveLayer;
    }

    public void SetDrawModeEraser(BaseEventArgs e)
    {

        TilesPanel.Erase = !TilesPanel.Erase;
    }

    public void CreateNewLayer(int Index, Layer LayerData, bool IsUndoAction = false)
    {
        MapWidget.CreateNewLayer(Index, LayerData, IsUndoAction);
    }

    public void DeleteLayer(int Index, bool IsUndoAction = false)
    {
        MapWidget.DeleteLayer(Index, IsUndoAction);
    }

    public void SwapLayers(int Index1, int Index2)
    {
        MapWidget.SwapLayers(Index1, Index2);
    }

    public void CancelSelection(BaseEventArgs e)
    {
        if (SelectionWidth != 0 || SelectionHeight != 0 || SelectionBackground.Visible)
        {
            SelectionX = -1;
            SelectionY = -1;
            SelectionWidth = 0;
            SelectionHeight = 0;
            SelectionBackground.SetVisible(false);
        }
        SetCursorInActiveSelection(false);
        this.MovingSelection = false;
        this.CursorInActiveSelection = false;
        this.IgnoreLeftButton = false;
        this.MovingSelection = false;
        this.SelectionStartUndoLast = false;
        this.SelectionFromPaste = false;
    }

    public void SelectAll(BaseEventArgs e)
    {
        if (TilesPanel.DrawTool != DrawTools.SelectionActiveLayer && TilesPanel.DrawTool != DrawTools.SelectionAllLayers) return;
        SelectionX = 0;
        SelectionY = 0;
        SelectionWidth = Map.Width;
        SelectionHeight = Map.Height;
        UpdateSelection();
        MouseMoving(Graphics.LastMouseEvent);
    }

    private void SetCursorInActiveSelection(bool Value)
    {
        this.CursorInActiveSelection = Value;
        if (Value) Input.SetCursor(odl.SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
        else Input.SetCursor(odl.SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
    }

    public void UpdateSelection()
    {
        if (SelectionWidth > 0 && SelectionHeight > 0)
        {
            SelectionBackground.SetVisible(true);
            SelectionBackground.SetPosition(
                (int)Math.Round(MapWidget.Position.X + SelectionX * 32 * this.ZoomFactor),
                (int)Math.Round(MapWidget.Position.Y + SelectionY * 32 * this.ZoomFactor)
            );
            SelectionBackground.SetSize(
                (int)Math.Round(SelectionWidth * 32 * this.ZoomFactor + 1),
                (int)Math.Round(SelectionHeight * 32 * this.ZoomFactor + 1)
            );
        }
        else SelectionBackground.SetVisible(false);
    }

    public void UpdateTilePlacement(MouseEventArgs e, int oldx = -1, int oldy = -1, int newx = -1, int newy = -1)
    {
        if (!MainContainer.Mouse.Inside) return;
        if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.SliderDragging || MainContainer.HScrollBar.SliderHovering)) return;
        if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.SliderDragging || MainContainer.VScrollBar.SliderHovering)) return;
        bool Left = Mouse.LeftStartedInside && Mouse.LeftMousePressed;
        bool Right = Mouse.RightStartedInside && Mouse.RightMousePressed;

        if (Left || Right)
        {
            if (OriginPoint == null)
                OriginPoint = new Point(
                    (int)Math.Floor(oldx / 32d),
                    (int)Math.Floor(oldy / 32d)
                );
        }

        if (OriginPoint != null && MapTileX != OriginPoint.X && MapTileY != OriginPoint.Y)
        {
            if (MapTileX > OriginPoint.X) // Right
            {
                if (MapTileY > OriginPoint.Y) CursorDirectionFromOrigin = Location.BottomRight;
                else CursorDirectionFromOrigin = Location.TopRight;
            }
            else // Left
            {
                if (MapTileY > OriginPoint.Y) CursorDirectionFromOrigin = Location.BottomLeft;
                else CursorDirectionFromOrigin = Location.TopLeft;
            }
        }

        // Input handling
        if (Left && !IgnoreLeftButton)
        {
            if (TilesPanel.DrawTool == DrawTools.SelectionActiveLayer || TilesPanel.DrawTool == DrawTools.SelectionAllLayers) // Selection tool
            {
                if (MovingSelection)
                {
                    int mx = (int)Math.Round(newx / 32d);
                    int my = (int)Math.Round(newy / 32d);
                    int oldselx = SelectionX;
                    int oldsely = SelectionY;
                    if (SelectionMouseOrigin.X + mx - OriginPoint.X != oldselx || SelectionMouseOrigin.Y + my - OriginPoint.Y != oldsely)
                    {
                        // If we're continuing moving a selection, we start by undoing the last (ready!) selection in
                        // order to smoothly continue moving the selection.
                        if (this.SelectionStartUndoLast && Undo.TileGroupUndoAction.GetLatestAll() != null && Undo.TileGroupUndoAction.GetLatestAll().PartOfSelection ||
                            !this.SelectionStartUndoLast && Undo.TileGroupUndoAction.GetLatest() != null && !Undo.TileGroupUndoAction.GetLatest().Ready)
                        {
                            Undo.TileGroupUndoAction.GetLatestAll().Ready = true;
                            Editor.Undo(true);
                            this.SelectionStartUndoLast = false;
                        }
                        bool All = TilesPanel.DrawTool == DrawTools.SelectionAllLayers;
                        Undo.TileGroupUndoAction.Create(Map.ID, true);
                        for (int Layer = All ? 0 : LayerPanel.SelectedLayer; Layer < (All ? Map.Layers.Count : LayerPanel.SelectedLayer + 1); Layer++)
                        {
                            MapWidget.SetLayerLocked(Layer, false);
                            if (!SelectionFromPaste)
                            {
                                for (int y = 0; y < SelectionHeight; y++)
                                {
                                    for (int x = 0; x < SelectionWidth; x++)
                                    {
                                        if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) continue;
                                        int idx = SelectionTileOrigin.X + x + (SelectionTileOrigin.Y + y) * Map.Width;
                                        TileData oldtile = Map.Layers[Layer].Tiles[idx];
                                        Map.Layers[Layer].Tiles[idx] = null;
                                        MapWidget.DrawTile(SelectionTileOrigin.X + x, SelectionTileOrigin.Y + y, Layer, null, oldtile, true);
                                        Undo.TileGroupUndoAction.AddToLatest(idx, Layer, null, oldtile);
                                    }
                                }
                            }
                        }
                        SelectionX = SelectionMouseOrigin.X + mx - OriginPoint.X;
                        SelectionY = SelectionMouseOrigin.Y + my - OriginPoint.Y;
                        for (int Layer = All ? 0 : LayerPanel.SelectedLayer; Layer < (All ? Map.Layers.Count : LayerPanel.SelectedLayer + 1); Layer++)
                        {
                            for (int y = SelectionY; y < SelectionY + SelectionHeight; y++)
                            {
                                for (int x = SelectionX; x < SelectionX + SelectionWidth; x++)
                                {
                                    if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) continue;
                                    int tileidx = x + y * Map.Width;
                                    TileData oldtile = Map.Layers[Layer].Tiles[tileidx];
                                    int listidx = (x - SelectionX) + (y - SelectionY) * SelectionWidth;
                                    TileData tile = SelectionTiles.GetTile(Layer, listidx);
                                    Map.Layers[Layer].Tiles[tileidx] = tile;
                                    MapWidget.DrawTile(x, y, Layer, tile, oldtile, true);
                                    Undo.TileGroupUndoAction.AddToLatest(tileidx, Layer, tile, oldtile);
                                }
                            }
                            MapWidget.SetLayerLocked(Layer, true);
                        }
                        Editor.CanUndo = false;
                        UpdateSelection();
                    }
                }
                else
                {
                    int sx = OriginPoint.X < MapTileX ? OriginPoint.X : MapTileX;
                    int ex = OriginPoint.X < MapTileX ? MapTileX : OriginPoint.X;
                    int sy = OriginPoint.Y < MapTileY ? OriginPoint.Y : MapTileY;
                    int ey = OriginPoint.Y < MapTileY ? MapTileY : OriginPoint.Y;
                    if (sx < 0) sx = 0;
                    else if (sx >= Map.Width) sx = Map.Width - 1;
                    if (sy < 0) sy = 0;
                    else if (sy >= Map.Height) sy = Map.Height - 1;
                    if (ex < 0) ex = 0;
                    else if (ex >= Map.Width) ex = Map.Width - 1;
                    if (ey < 0) ey = 0;
                    else if (ey >= Map.Height) ey = Map.Height - 1;
                    SelectionX = sx;
                    SelectionY = sy;
                    SelectionWidth = ex - sx + 1;
                    SelectionHeight = ey - sy + 1;
                    UpdateSelection();
                }
            }
            else // A tile-drawing tool
            {
                int Layer = LayerPanel.SelectedLayer;
                if (TileDataList.Count == 0)
                {
                    if (TilesPanel.Erase) TileDataList.Add(null);
                    else
                    {
                        TilesPanel.SelectTile(null);
                        TileDataList.Add(null);
                        throw new Exception($"The tile data list is empty, but the eraser tool is not selected.\nCan't find tiles to draw with.");
                    }
                }
                List<Point> points = MapWidget.GetTilesFromMouse(oldx, oldy, newx, newy, Layer);
                if (points.Count > 0)
                {
                    // Consider the screen split in 4 quadrants, meeting at the origin point.
                    // If you're drawing in one quadrant, and you go further into that direction,
                    // all tiles you draw will overlap and thus you do not need to undo those tiles.
                    // However, if you're in one quadrant, and changing direction to go to the origin or
                    // another quadrant, there are tiles further into the quadrant that you have already
                    // drawn, but that need to be undone.
                    // Thus you can say, redraw/undo the tiles if the quadrant you're in w.r.t the origin is not equal to
                    // the quadrant your mouse is moving to.
                    // This drastically increases performance over redrawing every single time you move a tile.
                    if (Undo.TileGroupUndoAction.GetLatest() != null && !Undo.TileGroupUndoAction.GetLatest().Ready &&
                        (TilesPanel.DrawTool == DrawTools.RectangleFilled && MoveDirection != CursorDirectionFromOrigin ||
                        TilesPanel.DrawTool == DrawTools.RectangleOutline ||
                        TilesPanel.DrawTool == DrawTools.EllipseFilled ||
                        TilesPanel.DrawTool == DrawTools.EllipseOutline)) // Ellipse tool redraws every tile movement regardless of quadrant/direction
                    {
                        // We have to undo the last draw, at least the part that does not overlap with our new to-be-drawn area.
                        List<Undo.TileGroupUndoAction.TileChange> changes = new List<Undo.TileGroupUndoAction.TileChange>();
                        // For all tiles that are both in the to-be-drawn area and in the current undo group,
                        // Take them out of the undo and draw group, and then undo the rest (non-drawn area)
                        // Then put the to-be-drawn area back in a new tile undo group for the next draw.
                        Undo.TileGroupUndoAction action = Undo.TileGroupUndoAction.GetLatest();
                        for (int i = 0; i < action.Tiles.Count; i++)
                        {
                            Point point = points.Find(p => p.X + p.Y * Map.Width == action.Tiles[i].MapPosition && action.Tiles[i].Layer == Layer);
                            if (point != null && point.X >= 0 && point.Y >= 0 && point.X < Map.Width && point.Y < Map.Height)
                            {
                                Undo.TileGroupUndoAction.TileChange tc = action.Tiles[i];
                                changes.Add(tc);
                                action.Tiles.RemoveAt(i);
                                i--;
                            }
                        }
                        action.Ready = true;
                        Editor.Undo(true);
                        Undo.TileGroupUndoAction.Create(Map.ID);
                        Undo.TileGroupUndoAction.GetLatest().Tiles.AddRange(changes);
                    }
                    else
                    {
                        // Our draw area has a large overlapping area, so we take out all the tiles now
                        // to prevent further processing.
                        // Since these tiles would draw with the exact same time, it does not have any 
                        // visual impact, but we can separate these tiles out faster than if we
                        // proceed with the draw procedure.
                        Undo.TileGroupUndoAction action = Undo.TileGroupUndoAction.GetLatest();
                        if (action != null)
                        {
                            for (int i = 0; i < points.Count; i++)
                            {
                                if (points[i].X >= 0 && points[i].Y >= 0 &&
                                    points[i].X < Map.Width && points[i].Y < Map.Height &&
                                    action.Tiles.Exists(g => g.MapPosition == points[i].X + points[i].Y * Map.Width && g.Layer == Layer && !g.FromAutotile))
                                {
                                    points.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                    MapWidget.DrawTiles(points, Layer);
                }
            }
        }
        else if (Right && !IgnoreRightButton)
        {
            if (TilesPanel.DrawTool == DrawTools.SelectionActiveLayer || TilesPanel.DrawTool == DrawTools.SelectionAllLayers) // Selection tool
            {
                CancelSelection(new BaseEventArgs());
            }
            int Layer = LayerPanel.SelectedLayer;
            TileDataList.Clear();

            int OriginDiffX = MapTileX - OriginPoint.X;
            int OriginDiffY = MapTileY - OriginPoint.Y;
            CursorOrigin = Location.BottomRight;
            if (OriginDiffX < 0)
            {
                OriginDiffX = -OriginDiffX;
                CursorOrigin = Location.BottomLeft;
            }
            if (OriginDiffY < 0)
            {
                OriginDiffY = -OriginDiffY;
                if (CursorOrigin == Location.BottomLeft) CursorOrigin = Location.TopLeft;
                else CursorOrigin = Location.TopRight;
            }
            CursorWidth = OriginDiffX;
            CursorHeight = OriginDiffY;
            UpdateCursorPosition();

            if (CursorWidth == 0 && CursorHeight == 0)
            {
                int MapTileIndex = MapTileY * Map.Width + MapTileX;
                if (MapTileX < 0 || MapTileX >= Map.Width || MapTileY < 0 || MapTileY >= Map.Height)
                    TilesPanel.SelectTile(null);
                else
                {
                    TileData tile = Map.Layers[Layer].Tiles[MapTileIndex];
                    if (tile == null) TilesPanel.SelectTile(null);
                    else TilesPanel.SelectTile(tile);
                }
            }
            else
            {
                SelectionOnMap = true;
                TilesPanel.Erase = false;
                int sx = OriginPoint.X < MapTileX ? OriginPoint.X : MapTileX;
                int ex = OriginPoint.X < MapTileX ? MapTileX : OriginPoint.X;
                int sy = OriginPoint.Y < MapTileY ? OriginPoint.Y : MapTileY;
                int ey = OriginPoint.Y < MapTileY ? MapTileY : OriginPoint.Y;
                TileDataList.Clear();
                for (int y = sy; y <= ey; y++)
                {
                    for (int x = sx; x <= ex; x++)
                    {
                        int index = y * Map.Width + x;
                        if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height)
                            TileDataList.Add(null);
                        else
                        {
                            TileData tile = Map.Layers[Layer].Tiles[index];
                            TileDataList.Add(tile);
                        }
                    }
                }
            }
        }
        else
        {
            // Just in case no MouseUp event was ever received, in case of long computation times for, say, bucket tool
            if (!Editor.CanUndo)
            {
                Editor.CanUndo = true;
                Undo.TileGroupUndoAction.GetLatest().Ready = true;
                if ((TilesPanel.DrawTool == DrawTools.RectangleFilled || TilesPanel.DrawTool == DrawTools.RectangleOutline ||
                     TilesPanel.DrawTool == DrawTools.EllipseFilled || TilesPanel.DrawTool == DrawTools.EllipseOutline) &&
                    !Cursor.Visible) Cursor.SetVisible(true);
            }
            OriginPoint = null;
        }
    }

    public void SetLayerVisible(int layerindex, bool Visible)
    {
        MapWidget.SetLayerVisible(layerindex, Visible);
    }

    private void StartMovingSelection()
    {
        this.SelectionMouseOrigin = new Point(SelectionX, SelectionY);
        if (this.MovingSelection)
        {
            // If we're already moving a selection, we've released our mouse and placed the selection somewhere.
            // We can move it again though, and if we undo the last tile action, we can move the selection
            // and place back the original tiles in place of the current selection location.
            this.SelectionStartUndoLast = true;
            return;
        }
        this.MovingSelection = true;
        SelectionTiles = new MapSelection();
        this.SelectionTileOrigin = new Point(SelectionX, SelectionY);
        bool All = TilesPanel.DrawTool == DrawTools.SelectionAllLayers;
        for (int Layer = All ? 0 : LayerPanel.SelectedLayer; Layer < (All ? Map.Layers.Count : LayerPanel.SelectedLayer + 1); Layer++)
        {
            MapWidget.SetLayerLocked(Layer, false);
            for (int y = 0; y < SelectionHeight; y++)
            {
                for (int x = 0; x < SelectionWidth; x++)
                {
                    int idx = SelectionX + x + (SelectionY + y) * Map.Width;
                    TileData tile = Map.Layers[Layer].Tiles[idx];
                    SelectionTiles.AddTile(Layer, tile);
                }
            }
            MapWidget.SetLayerLocked(Layer, true);
        }
    }

    private void StopMovingSelection()
    {
        this.MovingSelection = false;
        this.SelectionMouseOrigin = null;
        this.SelectionTileOrigin = null;
        this.SelectionStartUndoLast = false;
        this.SelectionFromPaste = false;
        SelectionTiles = null;
    }

    private partial void CopyTiles(bool Cut = false)
    {
        if ((TilesPanel.DrawTool == DrawTools.SelectionActiveLayer || TilesPanel.DrawTool == DrawTools.SelectionAllLayers) &&
            SelectionWidth != 0 && SelectionHeight != 0)
        {
            bool All = TilesPanel.DrawTool == DrawTools.SelectionAllLayers;
            MapSelection sel = new MapSelection();
            sel.Width = SelectionWidth;
            sel.Height = SelectionHeight;
            if (Cut)
            {
                if (All) for (int i = 0; i < Map.Layers.Count; i++) MapWidget.SetLayerLocked(i, false);
                else MapWidget.SetLayerLocked(LayerPanel.SelectedLayer, false);
                Undo.TileGroupUndoAction.Create(Map.ID);
            }
            for (int Layer = All ? 0 : LayerPanel.SelectedLayer; Layer < (All ? Map.Layers.Count : LayerPanel.SelectedLayer + 1); Layer++)
            {
                for (int y = 0; y < SelectionHeight; y++)
                {
                    for (int x = 0; x < SelectionWidth; x++)
                    {
                        int idx = SelectionX + x + (SelectionY + y) * Map.Width;
                        TileData tile = Map.Layers[Layer].Tiles[idx];
                        sel.AddTile(Layer, tile);
                        if (Cut)
                        {
                            Map.Layers[Layer].Tiles[idx] = null;
                            MapWidget.DrawTile(SelectionX + x, SelectionY + y, Layer, null, tile, true);
                            Undo.TileGroupUndoAction.AddToLatest(idx, Layer, null, tile);
                        }
                    }
                }
            }
            Utilities.SetClipboard(sel, BinaryData.MAP_SELECTION);
            if (Cut)
            {
                if (All) for (int i = 0; i < Map.Layers.Count; i++) MapWidget.SetLayerLocked(i, true);
                else MapWidget.SetLayerLocked(LayerPanel.SelectedLayer, true);
                Undo.TileGroupUndoAction.GetLatest().Ready = true;
            }
        }
    }

    private partial void CutTiles()
    {
        CopyTiles(true);
        CancelSelection(new BaseEventArgs());
    }

    private partial void PasteTiles()
    {
        if (!Utilities.IsClipboardValidBinary(BinaryData.MAP_SELECTION)) return;
        if (TilesPanel.DrawTool != DrawTools.SelectionActiveLayer) TilesPanel.DrawTool = DrawTools.SelectionActiveLayer;
        CancelSelection(new BaseEventArgs());
        MapSelection group = Utilities.GetClipboard<MapSelection>();
        int origtilewidth = group.Width;
        int sx = MapTileX;
        if (sx < 0) sx = 0;
        else if (sx + group.Width >= Map.Width)
        {
            sx = Map.Width - group.Width;
            if (sx < 0)
            {
                group.Width += sx;
                sx = 0;
            }
        }
        int sy = MapTileY;
        if (sy < 0) sy = 0;
        else if (sy + group.Height >= Map.Height)
        {
            sy = Map.Height - group.Height;
            if (sy < 0)
            {
                group.Height += sy;
                sy = 0;
            }
        }
        SelectionX = sx;
        SelectionY = sy;
        SelectionWidth = group.Width;
        SelectionHeight = group.Height;
        UpdateSelection();
        Undo.TileGroupUndoAction.Create(Map.ID, true);
        foreach (int Layer in group.LayerSelections.Keys)
        {
            MapWidget.SetLayerLocked(Layer, false);
            for (int y = 0; y < SelectionHeight; y++)
            {
                for (int x = 0; x < SelectionWidth; x++)
                {
                    int tileidx = SelectionX + x + (SelectionY + y) * Map.Width;
                    int selidx = x + y * origtilewidth;
                    TileData tile = group.LayerSelections[Layer].Tiles[selidx];
                    TileData oldtile = Map.Layers[Layer].Tiles[tileidx];
                    Map.Layers[Layer].Tiles[tileidx] = tile;
                    Undo.TileGroupUndoAction.AddToLatest(tileidx, Layer, tile, oldtile);
                    MapWidget.DrawTile(SelectionX + x, SelectionY + y, Layer, tile, oldtile, true);
                }
            }
            MapWidget.SetLayerLocked(Layer, true);
        }
        MouseMoving(Graphics.LastMouseEvent);
        Undo.TileGroupUndoAction.GetLatest().Ready = true;
        this.SelectionStartUndoLast = true;
        this.SelectionFromPaste = true;
    }

    private partial void MouseMovingTiles(MouseEventArgs e)
    {
        if (Mode == MapMode.Tiles)
        {
            if (TilesPanel.UsingLeft || TilesPanel.UsingRight || LayerPanel.UsingLeft || LayerPanel.UsingRight) return;
            if (e.X != LastMouseX || e.Y != LastMouseY)
            {
                if (e.X >= LastMouseX) // Right
                {
                    if (e.Y >= LastMouseY) MoveDirection = Location.BottomRight;
                    else MoveDirection = Location.TopRight;
                }
                else // Left
                {
                    if (e.Y >= LastMouseY) MoveDirection = Location.BottomLeft;
                    else MoveDirection = Location.TopLeft;
                }
            }
            if (!MiddleMouseScrolling)
            {
                int oldmousex = RelativeMouseX;
                int oldmousey = RelativeMouseY;
                // Cursor placement
                if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.SliderDragging || MainContainer.HScrollBar.SliderHovering)) return;
                if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.SliderDragging || MainContainer.VScrollBar.SliderHovering)) return;
                int rx = e.X - MapWidget.Viewport.X;
                int ry = e.Y - MapWidget.Viewport.Y;
                if (e.X < MainContainer.Viewport.X || e.Y < MainContainer.Viewport.Y ||
                    e.X >= MainContainer.Viewport.X + MainContainer.Viewport.Width ||
                    e.Y >= MainContainer.Viewport.Y + MainContainer.Viewport.Height) // Off the Map Viewer area
                {
                    if (Cursor.Visible) Cursor.SetVisible(false);
                    MapTileX = -1;
                    MapTileY = -1;
                    RelativeMouseX = -1;
                    RelativeMouseY = -1;
                    return;
                }
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
                int tilex = (int)Math.Floor(rx / (32d * ZoomFactor));
                int tiley = (int)Math.Floor(ry / (32d * ZoomFactor));
                if (Editor.MainWindow.MapWidget != null && TilesPanel.DrawTool != DrawTools.SelectionActiveLayer && TilesPanel.DrawTool != DrawTools.SelectionAllLayers)
                {
                    if ((TilesPanel.DrawTool != DrawTools.RectangleFilled && TilesPanel.DrawTool != DrawTools.RectangleOutline &&
                         TilesPanel.DrawTool != DrawTools.EllipseFilled && TilesPanel.DrawTool != DrawTools.EllipseOutline) ||
                         OriginPoint == null) Cursor.SetVisible(true);
                }
                RelativeMouseX = tilex * 32;
                RelativeMouseY = tiley * 32;
                MapTileX = tilex;
                MapTileY = tiley;
                UpdateCursorPosition();
                if (oldmousex != RelativeMouseX || oldmousey != RelativeMouseY)
                {
                    UpdateTilePlacement(e, oldmousex, oldmousey, RelativeMouseX, RelativeMouseY);
                }
                if (TilesPanel.DrawTool == DrawTools.SelectionActiveLayer || TilesPanel.DrawTool == DrawTools.SelectionAllLayers)
                {
                    if (SelectionWidth != 0 && SelectionHeight != 0 && (!e.LeftButton || e.LeftButton && e.LeftButton != e.OldLeftButton))
                    {
                        if (MapTileX >= SelectionX && MapTileX < SelectionX + SelectionWidth &&
                            MapTileY >= SelectionY && MapTileY < SelectionY + SelectionHeight)
                        {
                            SetCursorInActiveSelection(true);
                        }
                        else
                        {
                            SetCursorInActiveSelection(false);
                        }
                    }
                    else
                    {
                        SetCursorInActiveSelection(false);
                    }
                }
            }
        }
    }

    private partial void MouseDownTiles(MouseEventArgs e)
    {
        if (Mode == MapMode.Tiles)
        {
            if (TilesPanel.UsingLeft || TilesPanel.UsingRight || LayerPanel.UsingLeft || LayerPanel.UsingRight) return;
            if (Mouse.LeftMouseTriggered && !IgnoreLeftButton)
            {
                IgnoreLeftButton = false;
                IgnoreRightButton = true;
            }
            else if (Mouse.RightMouseTriggered && !IgnoreRightButton)
            {
                IgnoreLeftButton = true;
                IgnoreRightButton = false;
            }
            if ((Mouse.LeftMouseTriggered || Mouse.RightMouseTriggered) && MainContainer.Mouse.Inside)
            {
                if ((TilesPanel.DrawTool == DrawTools.RectangleFilled || TilesPanel.DrawTool == DrawTools.RectangleOutline ||
                        TilesPanel.DrawTool == DrawTools.EllipseFilled || TilesPanel.DrawTool == DrawTools.EllipseOutline) &&
                    e.LeftButton && e.LeftButton != e.OldLeftButton && !IgnoreLeftButton) Cursor.SetVisible(false);
                if (SelectionWidth != 0 || SelectionHeight != 0)
                {
                    if (this.CursorInActiveSelection) // Start moving selection
                    {
                        StartMovingSelection();
                    }
                    else // Cancel selection
                    {
                        StopMovingSelection();
                        CancelSelection(new BaseEventArgs());
                        this.IgnoreLeftButton = true;
                    }
                }
                UpdateTilePlacement(e, RelativeMouseX, RelativeMouseY, RelativeMouseX, RelativeMouseY);
            }
        }
    }

    private partial void MouseUpTiles(MouseEventArgs e)
    {
        if (Mode == MapMode.Tiles)
        {
            if (e.LeftButton != e.OldLeftButton)
            {
                if (!Editor.CanUndo && Undo.TileGroupUndoAction.GetLatest() != null)
                {
                    Editor.CanUndo = true;
                    Undo.TileGroupUndoAction.GetLatest().Ready = true;
                    if ((TilesPanel.DrawTool == DrawTools.RectangleFilled || TilesPanel.DrawTool == DrawTools.RectangleOutline ||
                         TilesPanel.DrawTool == DrawTools.EllipseFilled || TilesPanel.DrawTool == DrawTools.EllipseOutline) &&
                        !Cursor.Visible) Cursor.SetVisible(true);
                }
            }
            if (!e.LeftButton && !e.RightButton)
            {
                OriginPoint = null;
                IgnoreLeftButton = false;
                IgnoreRightButton = false;
            }
        }
    }
}