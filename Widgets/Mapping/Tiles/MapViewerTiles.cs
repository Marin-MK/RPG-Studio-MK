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
            SelectionBackground.SetZIndex(2);

            Cursor = new CursorWidget(MainContainer);
            Cursor.ConsiderInAutoScrollCalculation = false;
            Cursor.SetZIndex(6);

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.ESCAPE), CancelSelection)
            });
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
            if (SelectionX != -1 || SelectionY != -1 || SelectionWidth != 0 || SelectionHeight != 0 || SelectionBackground.Visible)
            {
                SelectionX = -1;
                SelectionY = -1;
                SelectionWidth = 0;
                SelectionHeight = 0;
                SelectionBackground.SetVisible(false);
            }
        }

        public void UpdateSelection()
        {
            if (SelectionX >= 0 && SelectionY >= 0 && SelectionWidth > 0 && SelectionHeight > 0)
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

        public void UpdateTilePlacement(int oldx = -1, int oldy = -1, int newx = -1, int newy = -1)
        {
            if (!MainContainer.WidgetIM.Hovering) return;
            if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.SliderDragging || MainContainer.HScrollBar.SliderHovering)) return;
            if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.SliderDragging || MainContainer.VScrollBar.SliderHovering)) return;
            bool Left = WidgetIM.ClickedLeftInArea == true;
            bool Right = WidgetIM.ClickedRightInArea == true;

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
            if (Left)
            {
                if (TilesPanel.SelectButton.Selected) // Selection tool
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
                else // Draw tool
                {
                    int Layer = LayerPanel.SelectedLayer;
                    if (TileDataList.Count == 0)
                    {
                        if (TilesPanel.EraserButton.Selected) TileDataList.Add(null);
                        else
                        {
                            TilesPanel.SelectTile(null);
                            TileDataList.Add(null);
                            throw new Exception($"The tile data list is empty, but the eraser tool is not selected.\nCan't find tiles to draw with.");
                        }
                    }
                    List<Point> points = MapWidget.GetTilesFromMouse(oldx, oldy, newx, newy);
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
                        if (TilesPanel.RectButton.Selected &&
                            TileGroupUndoAction.GetLatest() != null && !TileGroupUndoAction.GetLatest().Ready &&
                            MoveDirection != CursorDirectionFromOrigin)
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
                            Editor.CanUndo = true;
                            Editor.Undo();
                            Editor.CanUndo = false;
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
                        Console.WriteLine($"Drawing {points.Count} tiles.");
                        MapWidget.DrawTiles(points, Layer);
                    }
                }
            }
            else if (Right)
            {
                if (TilesPanel.SelectButton.Selected) // Selection tool
                {
                    if (SelectionX != -1 || SelectionY != -1 || SelectionWidth != 0 || SelectionHeight != 0)
                    {
                        SelectionX = -1;
                        SelectionY = -1;
                        SelectionWidth = 0;
                        SelectionHeight = 0;
                        UpdateSelection();
                    }
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
                    TilesPanel.EraserButton.SetSelected(false);
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
                if (Editor.MainWindow.MapWidget != null && !TilesPanel.SelectButton.Selected)
                {
                    if (!TilesPanel.RectButton.Selected || OriginPoint == null) Cursor.SetVisible(true);
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
                    UpdateTilePlacement(oldmousex, oldmousey, RelativeMouseX, RelativeMouseY);
                }
            }
            LastMouseX = e.X;
            LastMouseY = e.Y;
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
                if (TilesPanel.RectButton.Selected && e.LeftButton && e.LeftButton != e.OldLeftButton) Cursor.SetVisible(false);
                UpdateTilePlacement(RelativeMouseX, RelativeMouseY, RelativeMouseX, RelativeMouseY);
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
                    if (TilesPanel.RectButton.Selected && !Cursor.Visible) Cursor.SetVisible(true);
                }
            }
            if (!e.LeftButton && !e.RightButton) OriginPoint = null;
        }
    }
}
