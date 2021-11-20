using System;
using System.Collections.Generic;
using RPGStudioMK.Game;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class MapViewerTiles : MapViewerBase
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

        bool CursorInActiveSelection = false;
        bool IgnoreLeftButton = false;
        bool MovingSelection = false;
        bool SelectionStartUndoLast = false;
        bool SelectionFromPaste = false;
        Point SelectionMouseOrigin;
        Point SelectionTileOrigin;
        List<TileData> SelectionTiles = new List<TileData>();

        public CursorWidget Cursor;

        public override int TopLeftX
        {
            get
            {
                int x = base.TopLeftX;
                if (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight) x -= CursorWidth;
                return x;
            }
        }
        public override int TopLeftY
        {
            get
            {
                int y = base.TopLeftY;
                if (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight) y -= CursorHeight;
                return y;
            }
        }

        public SelectionBackground SelectionBackground;

        public MapViewerTiles(IContainer Parent) : base(Parent)
        {
            GridLayout.Columns.Add(new GridSize(288, Unit.Pixels));
            GridLayout.UpdateContainers();

            // Right sidebar
            Grid sidebargrid = new Grid(GridLayout);
            sidebargrid.SetGrid(0, 1, 2, 2);
            sidebargrid.SetRows(new GridSize(5), new GridSize(2, Unit.Pixels), new GridSize(2));
            sidebargrid.SetColumns(new GridSize(1));

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

            Cursor = new CursorWidget(MainContainer);
            Cursor.ConsiderInAutoScrollCalculation = false;
            Cursor.SetZIndex(8);

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.ESCAPE), CancelSelection),
                new Shortcut(this, new Key(Keycode.A, Keycode.CTRL), SelectAll),
                new Shortcut(this, new Key(Keycode.Q), SetDrawModePencil),
                new Shortcut(this, new Key(Keycode.W), SetDrawModeRectangle),
                new Shortcut(this, new Key(Keycode.E), SetDrawModeEllipse),
                new Shortcut(this, new Key(Keycode.R), SetDrawModeBucket),
                new Shortcut(this, new Key(Keycode.T), SetDrawModeEraser),
                new Shortcut(this, new Key(Keycode.Y), SetDrawModeSelection)
            });

            Editor.OnUndoing += delegate (BaseEventArgs e)
            {
                if (SelectionWidth != 0 && SelectionHeight != 0) CancelSelection(e);
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
            TilesPanel.DrawTool = DrawTools.Ellipse;
        }

        public void SetDrawModeRectangle(BaseEventArgs e)
        {
            TilesPanel.DrawTool = DrawTools.Rectangle;
        }

        public void SetDrawModeSelection(BaseEventArgs e)
        {
            TilesPanel.DrawTool = DrawTools.Selection;
        }

        public void SetDrawModeEraser(BaseEventArgs e)
        {

            TilesPanel.Erase = !TilesPanel.Erase;
        }

        public override void SetMap(Map Map)
        {
            this.Map = Map;
            LayerPanel.CreateLayers();
            TilesPanel.SetMap(Map);
            TilesPanel.SelectTile(new TileData() { TileType = TileType.Tileset, Index = 0, ID = 0 });
            base.SetMap(Map);
        }

        public override void PositionMap()
        {
            base.PositionMap();
            UpdateSelection();
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
        }

        public void SelectAll(BaseEventArgs e)
        {
            if (TilesPanel.DrawTool != DrawTools.Selection) return;
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
                    (int) Math.Round(MapWidget.Position.X + SelectionX * 32 * this.ZoomFactor),
                    (int) Math.Round(MapWidget.Position.Y + SelectionY * 32 * this.ZoomFactor)
                );
                SelectionBackground.SetSize(
                    (int) Math.Round(SelectionWidth * 32 * this.ZoomFactor + 1),
                    (int) Math.Round(SelectionHeight * 32 * this.ZoomFactor + 1)
                );
            }
            else SelectionBackground.SetVisible(false);
        }

        public void UpdateTilePlacement(MouseEventArgs e, int oldx = -1, int oldy = -1, int newx = -1, int newy = -1)
        {
            if (!MainContainer.WidgetIM.Hovering) return;
            if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.SliderDragging || MainContainer.HScrollBar.SliderHovering)) return;
            if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.SliderDragging || MainContainer.VScrollBar.SliderHovering)) return;
            bool Left = WidgetIM.ClickedLeftInArea == true && e.LeftButton;
            bool Right = WidgetIM.ClickedRightInArea == true && e.RightButton;

            if (Left || Right)
            {
                if (OriginPoint == null)
                    OriginPoint = new Point(
                        (int) Math.Floor(oldx / 32d),
                        (int) Math.Floor(oldy / 32d)
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
                if (TilesPanel.DrawTool == DrawTools.Selection) // Selection tool
                {
                    if (MovingSelection)
                    {
                        int mx = (int) Math.Round(newx / 32d);
                        int my = (int) Math.Round(newy / 32d);
                        int oldselx = SelectionX;
                        int oldsely = SelectionY;
                        if (SelectionMouseOrigin.X + mx - OriginPoint.X != oldselx || SelectionMouseOrigin.Y + my - OriginPoint.Y != oldsely)
                        {
                            int Layer = LayerPanel.SelectedLayer;
                            // If we're continuing moving a selection, we start by undoing the last (ready!) selection in
                            // order to smoothly continue moving the selection.
                            if (this.SelectionStartUndoLast && TileGroupUndoAction.GetLatestAll() != null && TileGroupUndoAction.GetLatestAll().PartOfSelection ||
                                !this.SelectionStartUndoLast && TileGroupUndoAction.GetLatest() != null && !TileGroupUndoAction.GetLatest().Ready)
                            {
                                TileGroupUndoAction.GetLatestAll().Ready = true;
                                Editor.Undo(true);
                                this.SelectionStartUndoLast = false;
                            }
                            TileGroupUndoAction.Log(Map.ID, Layer, true);
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
                                        TileGroupUndoAction.AddToLatest(idx, null, oldtile);
                                    }
                                }
                            }
                            SelectionX = SelectionMouseOrigin.X + mx - OriginPoint.X;
                            SelectionY = SelectionMouseOrigin.Y + my - OriginPoint.Y;
                            UpdateSelection();
                            for (int y = SelectionY; y < SelectionY + SelectionHeight; y++)
                            {
                                for (int x = SelectionX; x < SelectionX + SelectionWidth; x++)
                                {
                                    if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) continue;
                                    int tileidx = x + y * Map.Width;
                                    TileData oldtile = Map.Layers[Layer].Tiles[tileidx];
                                    int listidx = (x - SelectionX) + (y - SelectionY) * SelectionWidth;
                                    TileData tile = SelectionTiles[listidx];
                                    Map.Layers[Layer].Tiles[tileidx] = tile;
                                    MapWidget.DrawTile(x, y, Layer, tile, oldtile, true);
                                    TileGroupUndoAction.AddToLatest(tileidx, tile, oldtile);
                                }
                            }
                            Editor.CanUndo = false;
                            MapWidget.SetLayerLocked(Layer, true);
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
                        if (TileGroupUndoAction.GetLatest() != null && !TileGroupUndoAction.GetLatest().Ready &&
                            (TilesPanel.DrawTool == DrawTools.Rectangle && MoveDirection != CursorDirectionFromOrigin ||
                            TilesPanel.DrawTool == DrawTools.Ellipse)) // Ellipse tool redraws every tile movement regardless of quadrant/direction
                        {
                            List<TileGroupUndoAction.TileChange> changes = new List<TileGroupUndoAction.TileChange>();
                            // For all tiles that are both in the to-be-drawn area and in the current undo group,
                            // Take them out of the undo and draw group, and then undo the rest (non-drawn area)
                            // Then put the to-be-drawn area back in a new tile undo group for the next draw.
                            TileGroupUndoAction action = TileGroupUndoAction.GetLatest();
                            for (int i = 0; i < action.Tiles.Count; i++)
                            {
                                Point point = points.Find(p => p.X + p.Y * Map.Width == action.Tiles[i].MapPosition);
                                if (point != null)
                                {
                                    points.Remove(point);
                                    TileGroupUndoAction.TileChange tc = action.Tiles[i];
                                    changes.Add(tc);
                                    action.Tiles.RemoveAt(i);
                                    i--;
                                }
                            }
                            action.Ready = true;
                            Editor.Undo(true);
                            TileGroupUndoAction.Log(Map.ID, Layer);
                            TileGroupUndoAction.GetLatest().Tiles.AddRange(changes);
                        }
                        else
                        {
                            // Our draw area has a large overlapping area, so we take out all the tiles now
                            // to prevent further processing.
                            // Since these tiles would draw with the exact same time, it does not have any 
                            // visual impact, but we can separate these tiles out faster than if we
                            // proceed with the draw procedure.
                            TileGroupUndoAction action = TileGroupUndoAction.GetLatest();
                            if (action != null)
                            {
                                for (int i = 0; i < points.Count; i++)
                                {
                                    if (action.Tiles.Exists(g => g.MapPosition == points[i].X + points[i].Y * Map.Width))
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
            else if (Right)
            {
                if (TilesPanel.DrawTool == DrawTools.Selection) // Selection tool
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
                    TileGroupUndoAction.GetLatest().Ready = true;
                    if ((TilesPanel.DrawTool == DrawTools.Rectangle || TilesPanel.DrawTool == DrawTools.Ellipse) &&
                        !Cursor.Visible) Cursor.SetVisible(true);
                }
                OriginPoint = null;
            }
        }

        public void SetLayerVisible(int layerindex, bool Visible)
        {
            MapWidget.SetLayerVisible(layerindex, Visible);
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
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
                int tilex = (int) Math.Floor(rx / (32d * ZoomFactor));
                int tiley = (int) Math.Floor(ry / (32d * ZoomFactor));
                if (Editor.MainWindow.MapWidget != null && TilesPanel.DrawTool != DrawTools.Selection)
                {
                    if ((TilesPanel.DrawTool != DrawTools.Ellipse && TilesPanel.DrawTool != DrawTools.Rectangle) ||
                         OriginPoint == null) Cursor.SetVisible(true);
                }
                int cx = tilex * 32;
                int cy = tiley * 32;
                RelativeMouseX = cx;
                RelativeMouseY = cy;
                cx += MapWidget.Position.X;
                cy += MapWidget.Position.Y;
                MapTileX = tilex;
                MapTileY = tiley;
                UpdateCursorPosition();
                if (oldmousex != RelativeMouseX || oldmousey != RelativeMouseY)
                {
                    UpdateTilePlacement(e, oldmousex, oldmousey, RelativeMouseX, RelativeMouseY);
                }
                if (TilesPanel.DrawTool == DrawTools.Selection)
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
            LastMouseX = e.X;
            LastMouseY = e.Y;
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
            SelectionTiles.Clear();
            this.SelectionTileOrigin = new Point(SelectionX, SelectionY);
            int Layer = LayerPanel.SelectedLayer;
            MapWidget.SetLayerLocked(Layer, false);
            for (int y = 0; y < SelectionHeight; y++)
            {
                for (int x = 0; x < SelectionWidth; x++)
                {
                    int idx = SelectionX + x + (SelectionY + y) * Map.Width;
                    TileData oldtile = Map.Layers[Layer].Tiles[idx];
                    SelectionTiles.Add(Map.Layers[Layer].Tiles[idx]);
                }
            }
            MapWidget.SetLayerLocked(Layer, true);
        }

        private void StopMovingSelection()
        {
            this.MovingSelection = false;
            this.SelectionMouseOrigin = null;
            this.SelectionTileOrigin = null;
            this.SelectionStartUndoLast = false;
            this.SelectionFromPaste = false;
            SelectionTiles.Clear();
        }

        public void UpdateCursorPosition()
        {
            int offsetx = 0;
            int offsety = 0;
            if (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight)
                offsetx = (int) Math.Round(32 * CursorWidth * ZoomFactor);
            if (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight)
                offsety = (int) Math.Round(32 * CursorHeight * ZoomFactor);
            Cursor.SetPosition(
                MapWidget.Position.X + (int) Math.Round(32 * MapTileX * ZoomFactor) - offsetx - 7,
                MapWidget.Position.Y + (int) Math.Round(32 * MapTileY * ZoomFactor) - offsety - 7
            );
            Cursor.SetSize(
                (int) Math.Round(32 * ZoomFactor) * (CursorWidth + 1) + 14,
                (int) Math.Round(32 * ZoomFactor) * (CursorHeight + 1) + 14
            );
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if ((e.LeftButton != e.OldLeftButton && e.LeftButton ||
                e.RightButton != e.OldRightButton && e.RightButton) &&
                MainContainer.WidgetIM.Hovering)
            {
                if ((TilesPanel.DrawTool == DrawTools.Rectangle || TilesPanel.DrawTool == DrawTools.Ellipse) &&
                    e.LeftButton && e.LeftButton != e.OldLeftButton) Cursor.SetVisible(false);
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

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (e.LeftButton != e.OldLeftButton)
            {
                if (!Editor.CanUndo)
                {
                    Editor.CanUndo = true;
                    TileGroupUndoAction.GetLatest().Ready = true;
                    if ((TilesPanel.DrawTool == DrawTools.Rectangle || TilesPanel.DrawTool == DrawTools.Ellipse) &&
                        !Cursor.Visible) Cursor.SetVisible(true);
                }
                IgnoreLeftButton = false;
            }
            if (!e.LeftButton && !e.RightButton) OriginPoint = null;
        }

        public override void Update()
        {
            base.Update();
            if (!SelectedWidget) return;
            if (Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_LCTRL) || Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_RCTRL))
            {
                if (Input.Trigger(odl.SDL2.SDL.SDL_Keycode.SDLK_c)) // Copy
                {
                    CopySelection();
                }
                else if (Input.Trigger(odl.SDL2.SDL.SDL_Keycode.SDLK_x)) // Cut
                {
                    CutSelection();
                }
                else if (Input.Trigger(odl.SDL2.SDL.SDL_Keycode.SDLK_v)) // Paste
                {
                    PasteSelection();
                }
            }
        }

        void CopySelection(bool DeleteTiles = false)
        {
            if (TilesPanel.DrawTool == DrawTools.Selection && SelectionWidth != 0 && SelectionHeight != 0)
            {
                TileGroup group = new TileGroup();
                group.Width = SelectionWidth;
                group.Height = SelectionHeight;
                if (DeleteTiles) MapWidget.SetLayerLocked(LayerPanel.SelectedLayer, false);
                for (int y = 0; y < SelectionHeight; y++)
                {
                    for (int x = 0; x < SelectionWidth; x++)
                    {
                        int idx = SelectionX + x + (SelectionY + y) * Map.Width;
                        TileData tile = Map.Layers[LayerPanel.SelectedLayer].Tiles[idx];
                        group.Tiles.Add(tile);
                        if (DeleteTiles)
                        {
                            Map.Layers[LayerPanel.SelectedLayer].Tiles[idx] = null;
                            MapWidget.DrawTile(SelectionX + x, SelectionY + y, LayerPanel.SelectedLayer, null, tile, true);
                        }
                    }
                }
                Utilities.SetClipboard(group, BinaryData.TILE_SELECTION);
                if (DeleteTiles) MapWidget.SetLayerLocked(LayerPanel.SelectedLayer, true);
            }
        }

        void CutSelection()
        {
            CopySelection(true);
        }

        void PasteSelection()
        {
            if (!Utilities.IsClipboardValidBinary(BinaryData.TILE_SELECTION)) return;
            if (TilesPanel.DrawTool != DrawTools.Selection) TilesPanel.DrawTool = DrawTools.Selection;
            CancelSelection(new BaseEventArgs());
            TileGroup group = Utilities.GetClipboard<TileGroup>();
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
            int Layer = LayerPanel.SelectedLayer;
            TileGroupUndoAction.Log(Map.ID, Layer, true);
            MapWidget.SetLayerLocked(Layer, false);
            for (int y = 0; y < SelectionHeight; y++)
            {
                for (int x = 0; x < SelectionWidth; x++)
                {
                    int tileidx = SelectionX + x + (SelectionY + y) * Map.Width;
                    int selidx = x + y * origtilewidth;
                    TileData tile = group.Tiles[selidx];
                    TileData oldtile = Map.Layers[Layer].Tiles[tileidx];
                    Map.Layers[Layer].Tiles[tileidx] = tile;
                    TileGroupUndoAction.AddToLatest(tileidx, tile, oldtile);
                    MapWidget.DrawTile(SelectionX + x, SelectionY + y, Layer, tile, oldtile, true);
                }
            }
            MouseMoving(Graphics.LastMouseEvent);
            TileGroupUndoAction.GetLatest().Ready = true;
            MapWidget.SetLayerLocked(Layer, true);
            this.SelectionStartUndoLast = true;
            this.SelectionFromPaste = true;
        }
    }
}
