using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewerTiles : MapViewerBase
    {
        public TilesetPanel TilesetPanel;
        public LayerPanel LayerPanel;

        public Point OriginPoint;
        public Location CursorOrigin;
        public List<TileData> TileDataList = new List<TileData>();
        public int CursorWidth = 0;
        public int CursorHeight = 0;
        public bool SelectionOnMap = false;
        public int SelectionX = -1;
        public int SelectionY = -1;
        public int SelectionWidth = 0;
        public int SelectionHeight = 0;

        public CursorWidget Cursor;

        public int TopLeftX
        {
            get
            {
                int x = MapTileX;
                if (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight) x -= CursorWidth;
                return x;
            }
        }
        public int TopLeftY
        {
            get
            {
                int y = MapTileY;
                if (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight) y -= CursorHeight;
                return y;
            }
        }

        public SelectionBackground SelectionBackground;

        public MapViewerTiles(object Parent, string Name = "mapViewerTiles")
            : base(Parent, Name)
        {
            GridLayout.Columns.Add(new GridSize(288, Unit.Pixels));
            GridLayout.UpdateContainers();

            // Right sidebar
            Grid sidebargrid = new Grid(GridLayout);
            sidebargrid.SetGrid(0, 1, 2, 2);
            sidebargrid.SetRows(new GridSize(5), new GridSize(2, Unit.Pixels), new GridSize(2));
            sidebargrid.SetColumns(new GridSize(1));

            // Tileset part of right sidebar
            TilesetPanel = new TilesetPanel(sidebargrid);
            TilesetPanel.SetBackgroundColor(28, 50, 73);
            TilesetPanel.MapViewer = this;
            (this.Parent.Parent.Parent.Parent.Parent as MappingWidget).TilesetPanel = TilesetPanel;

            // Inner right sidebar divider
            Widget InnerRightSidebarDivider = new Widget(sidebargrid);
            InnerRightSidebarDivider.SetBackgroundColor(10, 23, 37);
            InnerRightSidebarDivider.SetGridRow(1);

            // Layers part of right sidebar
            LayerPanel = new LayerPanel(sidebargrid);
            LayerPanel.SetBackgroundColor(28, 50, 73);
            LayerPanel.SetGridRow(2);
            LayerPanel.MapViewer = this;
            (this.Parent.Parent.Parent.Parent.Parent as MappingWidget).LayerPanel = LayerPanel;

            SelectionBackground = new SelectionBackground(MainContainer);
            SelectionBackground.SetZIndex(2);

            Cursor = new CursorWidget(MainContainer);
            Cursor.ConsiderInAutoScrollCalculation = false;
            Cursor.SetZIndex(1);

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.ESCAPE), new EventHandler<EventArgs>(CancelSelection))
            });
        }

        public override void SetMap(Map Map)
        {
            this.Map = Map;
            LayerPanel.CreateLayers();
            TilesetPanel.SetTilesets(Map.TilesetIDs);
            TilesetPanel.SelectTile(new TileData() { TileType = TileType.Tileset, Index = 0, ID = 0 });
            base.SetMap(Map);
        }

        public override void PositionMap()
        {
            base.PositionMap();
            UpdateSelection();
        }

        public void CreateNewLayer(int Index, Game.Layer LayerData)
        {
            MapWidget.CreateNewLayer(Index, LayerData);
        }

        public void DeleteLayer(int Index)
        {
            MapWidget.DeleteLayer(Index);
        }

        public void SwapLayers(int Index1, int Index2)
        {
            MapWidget.SwapLayers(Index1, Index2);
        }

        public void CancelSelection(object sender, EventArgs e)
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
            if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.Dragging || MainContainer.HScrollBar.Hovering)) return;
            if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.Dragging || MainContainer.VScrollBar.Hovering)) return;
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

            // Input handling
            if (Left)
            {
                if (TilesetPanel.SelectButton.Selected) // Selection tool
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
                else // Pencil tool
                {
                    int Layer = LayerPanel.SelectedLayer;
                    if (TileDataList.Count == 0)
                    {
                        if (TilesetPanel.EraserButton.Selected) TileDataList.Add(null);
                        else
                        {
                            TilesetPanel.SelectTile(null);
                            TileDataList.Add(null);
                            throw new Exception($"The tile data list is empty, but the eraser tool is not selected.\nCan't find tiles to draw with.");
                        }
                    }
                    MapWidget.DrawTiles(oldx, oldy, newx, newy, Layer);
                }
            }
            else if (Right)
            {
                if (TilesetPanel.SelectButton.Selected) // Selection tool
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
                        TilesetPanel.SelectTile(null);
                    else
                    {
                        TileData tile = Map.Layers[Layer].Tiles[MapTileIndex];
                        if (tile == null) TilesetPanel.SelectTile(null);
                        else TilesetPanel.SelectTile(tile);
                    }
                }
                else
                {
                    SelectionOnMap = true;
                    TilesetPanel.EraserButton.SetSelected(false);
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

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!MiddleMouseScrolling)
            {
                LastMouseX = e.X;
                LastMouseY = e.Y;
                int oldmousex = RelativeMouseX;
                int oldmousey = RelativeMouseY;
                // Cursor placement
                if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.Dragging || MainContainer.HScrollBar.Hovering)) return;
                if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.Dragging || MainContainer.VScrollBar.Hovering)) return;
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
                if (!Editor.MainWindow.MapWidget.MapViewerTiles.TilesetPanel.SelectButton.Selected) Cursor.SetVisible(true);
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

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if ((e.LeftButton != e.OldLeftButton && e.LeftButton ||
                e.RightButton != e.OldRightButton && e.RightButton) &&
                MainContainer.WidgetIM.Hovering)
                UpdateTilePlacement(RelativeMouseX, RelativeMouseY, RelativeMouseX, RelativeMouseY);
        }

        public override void MouseUp(object sender, MouseEventArgs e)
        {
            base.MouseUp(sender, e);
            if (!e.LeftButton && !e.RightButton) OriginPoint = null;
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            
        }
    }
}
