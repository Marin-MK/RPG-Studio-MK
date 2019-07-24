using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewer : Widget
    {
        public Data.Map Map;
        public TilesetTab TilesetTab;
        public LayersTab LayersTab;

        public int RelativeMouseX = 0;
        public int RelativeMouseY = 0;
        public int MapTileX = 0;
        public int MapTileY = 0;
        public Location CursorOrigin;
        public List<Data.TileData> TileDataList = new List<Data.TileData>();
        public int CursorWidth = 0;
        public int CursorHeight = 0;
        public Point OriginDrawPoint;
        public bool SelectionOnMap = false;

        public Container MainContainer;
        public CursorWidget Cursor;
        public MapImageWidget MapWidget;
        public GridBackground GridBackground;

        MouseEventArgs LastMouseEvent;

        public MapViewer(object Parent, string Name = "mapViewer")
            : base(Parent, Name)
        {
            this.SetBackgroundColor(Color.BLACK);
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
            this.WidgetIM.OnMouseUp += MouseUp;
            this.OnWidgetSelect += WidgetSelect;
            MainContainer = new Container(this);
            MainContainer.Name = "mapViewerContainer";
            MainContainer.AutoScroll = true;
            MainContainer.OnScrolling += delegate (object sender, EventArgs e)
            {
                if (LastMouseEvent != null) this.MouseMoving(sender, LastMouseEvent);
            };
            Cursor = new CursorWidget(MainContainer);
            Cursor.ConsiderInAutoScroll = false;
            GridBackground = new GridBackground(MainContainer);
            MapWidget = new MapImageWidget(MainContainer);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            MainContainer.SetSize(this.Size);
            PositionMap();
            if (MainContainer.ScrollBarX != null) MainContainer.ScrollBarX.SetValue(0.5);
            if (MainContainer.ScrollBarY != null) MainContainer.ScrollBarY.SetValue(0.5);
        }

        public void SetMap(Data.Map Map)
        {
            this.Map = Map;
            TilesetTab.SetTilesets(Map.TilesetIDs);
            this.CreateLayerBitmaps();
            this.LayersTab.CreateLayers();
            if (MainContainer.ScrollBarX != null) MainContainer.ScrollBarX.SetValue(0.5);
            if (MainContainer.ScrollBarY != null) MainContainer.ScrollBarY.SetValue(0.5);
        }

        public void CreateLayerBitmaps()
        {
            MapWidget.LoadLayers(this.Map);
            PositionMap();
        }

        public void PositionMap()
        {
            int width = this.Map.Width * 32 + this.Viewport.Width / 2;
            if (width < this.Viewport.Width) width = this.Viewport.Width;
            int height = this.Map.Height * 32 + this.Viewport.Height / 2;
            if (height < this.Viewport.Height) height = this.Viewport.Height;
            int x = width / 2 - MapWidget.Size.Width / 2;
            int y = height / 2 - MapWidget.Size.Height / 2;
            int offsetx = 32 - x % 32;
            int offsety = 32 - y % 32;
            GridBackground.SetOffset(offsetx, offsety);
            GridBackground.SetSize(width, height);
            MapWidget.SetPosition(x, y);
            MainContainer.UpdateAutoScroll();
            if (MainContainer.ScrollBarX != null) MainContainer.ScrollBarX.WidgetIM.Priority = 1;
            if (MainContainer.ScrollBarY != null) MainContainer.ScrollBarY.WidgetIM.Priority = 1;
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            LastMouseEvent = e;
            int oldmousex = RelativeMouseX;
            int oldmousey = RelativeMouseY;
            // Cursor placement
            if (MainContainer.ScrollBarX != null && (MainContainer.ScrollBarX.Dragging || MainContainer.ScrollBarX.Hovering)) return;
            if (MainContainer.ScrollBarY != null && (MainContainer.ScrollBarY.Dragging || MainContainer.ScrollBarY.Hovering)) return;
            int rx = e.X - MapWidget.Viewport.X;
            int ry = e.Y - MapWidget.Viewport.Y;
            if (e.X < MainContainer.Viewport.X || e.Y < MainContainer.Viewport.Y ||
                e.X >= MainContainer.Viewport.X + MainContainer.Viewport.Width ||
                e.Y >= MainContainer.Viewport.Y + MainContainer.Viewport.Height) // Off the Map Viewer area
            {
                Cursor.SetVisible(false);
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
            int tilex = (int) Math.Floor(rx / 32d);
            int tiley = (int) Math.Floor(ry / 32d);
            Cursor.SetVisible(true);
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

        public void UpdateCursorPosition()
        {
            int offsetx = 0;
            int offsety = 0;
            if (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight)
                offsetx = 32 * CursorWidth;
            if (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight)
                offsety = 32 * CursorHeight;
            Cursor.SetPosition(MapWidget.Position.X + 32 * MapTileX - offsetx, MapWidget.Position.Y + 32 * MapTileY - offsety);
            Cursor.SetSize(32 * (CursorWidth + 1), 32 * (CursorHeight + 1));
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            // Update position - to make sure you're drawing where the mouse is, not where the cursor is
            // (the cursor obviously follows the mouse with this call if they're not aligned (which they should be))
            MouseMoving(sender, e);
            UpdateTilePlacement(RelativeMouseX, RelativeMouseY, RelativeMouseX, RelativeMouseY);
        }

        public override void MouseUp(object sender, MouseEventArgs e)
        {
            base.MouseUp(sender, e);
            if (!e.LeftButton && !e.RightButton) OriginDrawPoint = null;
        }

        public void UpdateTilePlacement(int oldx = -1, int oldy = -1, int newx = -1, int newy = -1)
        {
            if (MainContainer.ScrollBarX != null && (MainContainer.ScrollBarX.Dragging || MainContainer.ScrollBarX.Hovering)) return;
            if (MainContainer.ScrollBarY != null && (MainContainer.ScrollBarY.Dragging || MainContainer.ScrollBarY.Hovering)) return;
            if (MapTileX == -1 || MapTileY == -1) return;
            // Input handling
            if (WidgetIM.ClickedLeftInArea == true)
            {
                if (OriginDrawPoint == null) OriginDrawPoint = new Point((int) Math.Floor(oldx / 32d), (int) Math.Floor(oldy / 32d));
                int Layer = this.LayersTab.SelectedLayer;
                MapWidget.DrawTiles(oldx, oldy, newx, newy, Layer);
            }
            if (WidgetIM.ClickedRightInArea == true)
            {
                if (OriginDrawPoint == null) OriginDrawPoint = new Point((int) Math.Floor(oldx / 32d), (int) Math.Floor(oldy / 32d));
                int Layer = this.LayersTab.SelectedLayer;
                TileDataList.Clear();
                
                int OriginDiffX = MapTileX - OriginDrawPoint.X;
                int OriginDiffY = MapTileY - OriginDrawPoint.Y;
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
                        TilesetTab.EraserButton.SetSelected(true);
                    else
                    {
                        Data.TileData tile = Map.Layers[Layer].Tiles[MapTileIndex];
                        if (tile == null) TilesetTab.EraserButton.SetSelected(true);
                        else TilesetTab.SelectTile(tile);
                    }
                }
                else
                {
                    SelectionOnMap = true;
                    TilesetTab.EraserButton.SetSelected(false);
                    int sx = OriginDrawPoint.X < MapTileX ? OriginDrawPoint.X : MapTileX;
                    int ex = OriginDrawPoint.X < MapTileX ? MapTileX : OriginDrawPoint.X;
                    int sy = OriginDrawPoint.Y < MapTileY ? OriginDrawPoint.Y : MapTileY;
                    int ey = OriginDrawPoint.Y < MapTileY ? MapTileY : OriginDrawPoint.Y;
                    for (int y = sy; y <= ey; y++)
                    {
                        for (int x = sx; x <= ex; x++)
                        {
                            int index = y * Map.Width + x;
                            if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height)
                                TileDataList.Add(null);
                            else
                            {
                                Data.TileData tile = Map.Layers[Layer].Tiles[index];
                                TileDataList.Add(tile);
                            }
                        }
                    }
                    SelectionOnMap = true;
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }
    }

    public class MapImageWidget : Widget
    {
        public MapViewer MapViewer;
        public Data.Map MapData;

        public MapImageWidget(object Parent, string Name = "mapImageWidget")
            : base(Parent, Name)
        {
            SetBackgroundColor(Color.BLACK);
            Sprites["grid"] = new Sprite(this.Viewport);
            Sprites["grid"].Z = 999999;
            this.MapViewer = this.Parent.Parent as MapViewer;
        }

        public void LoadLayers(Data.Map MapData)
        {
            this.MapData = MapData;
            this.SetSize(MapData.Width * 32, MapData.Height * 32);
            if (Sprites["grid"].Bitmap != null) Sprites["grid"].Bitmap.Dispose();
            Sprites["grid"].Bitmap = new Bitmap(MapData.Width * 32, MapData.Height * 32);
            Sprites["grid"].Bitmap.Unlock();
            Color c = new Color(0, 0, 0, 64);
            for (int y = 0; y < MapData.Height * 32; y++)
            {
                for (int x = 0; x < MapData.Width * 32; x++)
                {
                    bool draw = false;
                    if (x > 0 && x < MapData.Width * 32 && x % 32 == 0) draw = true;
                    if (y > 0 && y < MapData.Height * 32 && y % 32 == 0) draw = true;
                    if (draw)
                    {
                        Sprites["grid"].Bitmap.SetPixel(x, y, c);
                    }
                }
            }
            Sprites["grid"].Bitmap.Lock();
            foreach (string s in this.Sprites.Keys)
            {
                if (s != "_bg" && s != "grid") this.Sprites[s].Dispose();
            }
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()] = new Sprite(this.Viewport, this.Size.Width, this.Size.Height);
                this.Sprites[i.ToString()].Z = i;
                this.Sprites[i.ToString()].Bitmap.Unlock();
            }
            // Iterate through all the layers
            for (int layer = 0; layer < MapData.Layers.Count; layer++)
            {
                Bitmap layerbmp = this.Sprites[layer.ToString()].Bitmap as Bitmap;
                // Iterate through all vertical tiles
                for (int y = 0; y < MapData.Height; y++)
                {
                    // Iterate through all horizontal tiles
                    for (int x = 0; x < MapData.Width; x++)
                    {
                        // Each individual tile
                        if (MapData.Layers[layer] == null || MapData.Layers[layer].Tiles == null ||
                            y * MapData.Width + x >= MapData.Layers[layer].Tiles.Count ||
                            MapData.Layers[layer].Tiles[y * MapData.Width + x] == null) continue;
                        int tileset_index = MapData.Layers[layer].Tiles[y * MapData.Width + x].TilesetIndex;
                        int tileset_id = MapData.TilesetIDs[tileset_index];
                        Bitmap tilesetimage = Data.GameData.Tilesets[tileset_id].TilesetBitmap;
                        int mapx = x * 32;
                        int mapy = y * 32;
                        int tile_id = MapData.Layers[layer].Tiles[y * MapData.Width + x].TileID;
                        int tilesetx = tile_id % 8;
                        int tilesety = (int) Math.Floor(tile_id / 8d);
                        layerbmp.Build(new Rect(mapx, mapy, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32));
                    }
                }
            }
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()].Bitmap.Lock();
            }
        }

        public void DrawTiles(int oldx, int oldy, int newx, int newy, int layer)
        {
            // Avoid drawing a line from top left to current tile
            if (oldx == -1 || oldy == -1)
            {
                oldx = newx;
                oldy = newy;
            }
            Point Origin = MapViewer.OriginDrawPoint;
            // This resets the two points tiles are drawn in between if the mouse has gone off the map (otherwise it'd draw a line between
            // the last point on the map and the current point on the map)
            bool blanktile = MapViewer.TilesetTab.EraserButton.Selected;
            this.Sprites[layer.ToString()].Bitmap.Unlock();
            bool line = !(oldx == newx && oldy == newy);
            List<Point> TempCoords = new List<Point>();
            if (line) // Draw tiles between several tiles - use simple line drawing algorithm to determine the tiles to draw on
            {
                int x1 = oldx;
                int y1 = oldy;
                int x2 = newx;
                int y2 = newy;
                for (int x = x1 > x2 ? x2 : x1; (x1 > x2) ? (x <= x1) : (x <= x2); x++)
                {
                    double fact = ((double) x - x1) / (x2 - x1);
                    int y = (int) Math.Round(y1 + ((y2 - y1) * fact));
                    if (y >= 0)
                    {
                        int tilex = (int) Math.Floor(x / 32d);
                        int tiley = (int) Math.Floor(y / 32d);
                        if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                    }
                }
                int sy = y1 > y2 ? y2 : y1;
                for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                {
                    double fact = ((double) y - y1) / (y2 - y1);
                    int x = (int) Math.Round(x1 + ((x2 - x1) * fact));
                    if (x >= 0)
                    {
                        int tilex = (int) Math.Floor(x / 32d);
                        int tiley = (int) Math.Floor(y / 32d);
                        if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                    }
                }
            }
            else // Just one singular tile
            {
                TempCoords.Add(new Point((int) Math.Floor(newx / 32d), (int) Math.Floor(newy / 32d)));
            }

            for (int i = 0; i < TempCoords.Count; i++)
            {
                // Both of these depend on the origin, but we need them both at the top left as we begin drawing there
                // and to be able to compare them to use in the modulus, they both have to be adjusted
                int MapTileX = TempCoords[i].X;
                int MapTileY = TempCoords[i].Y;
                int OriginX = MapViewer.OriginDrawPoint.X;
                int OriginY = MapViewer.OriginDrawPoint.Y;
                if (MapViewer.CursorOrigin == Location.TopRight || MapViewer.CursorOrigin == Location.BottomRight)
                {
                    MapTileX -= MapViewer.CursorWidth;
                    OriginX -= MapViewer.CursorWidth;
                }
                if (MapViewer.CursorOrigin == Location.BottomLeft || MapViewer.CursorOrigin == Location.BottomRight)
                {
                    MapTileY -= MapViewer.CursorHeight;
                    OriginY -= MapViewer.CursorHeight;
                }
                // MapTileX and MapTileY are now the top left no matter the origin point
                int SelArea = MapViewer.TileDataList.Count;

                int OriginDiffX = (OriginX - MapTileX) % (MapViewer.CursorWidth + 1);
                int OriginDiffY = (OriginY - MapTileY) % (MapViewer.CursorHeight + 1);

                for (int j = 0; j < SelArea; j++)
                {
                    bool Blank = blanktile;
                    int selx = j % (MapViewer.CursorWidth + 1);
                    if (OriginDiffX < 0) selx -= OriginDiffX;
                    if (OriginDiffX > 0) selx -= OriginDiffX;
                    if (selx < 0) selx += MapViewer.CursorWidth + 1;
                    selx %= MapViewer.CursorWidth + 1;
                    int sely = (int) Math.Floor((double) j / (MapViewer.CursorWidth + 1));
                    if (OriginDiffY < 0) sely -= OriginDiffY;
                    if (OriginDiffY > 0) sely -= OriginDiffY;
                    if (sely < 0) sely += MapViewer.CursorHeight + 1;
                    sely %= MapViewer.CursorHeight + 1;
                    Data.TileData tiledata = MapViewer.TileDataList[sely * (MapViewer.CursorWidth + 1) + selx];
                    int tileid = -1;
                    int tilesetindex = -1;
                    int tilesetx = -1;
                    int tilesety = -1;
                    if (tiledata != null)
                    {
                        tileid = tiledata.TileID;
                        tilesetindex = tiledata.TilesetIndex;
                        tilesetx = tileid % 8;
                        tilesety = (int) Math.Floor(tileid / 8d);
                    }
                    else Blank = true;

                    int actualx = MapTileX + (j % (MapViewer.CursorWidth + 1));
                    int actualy = MapTileY + (int) Math.Floor((double) j / (MapViewer.CursorWidth + 1));

                    int MapPosition = actualy * MapData.Width + actualx;
                    if (actualx < 0 || actualx >= MapData.Width || actualy < 0 || actualy >= MapData.Height) continue;
                    Data.TileData olddata = MapData.Layers[layer].Tiles[MapPosition];
                    if (Blank)
                    {
                        MapData.Layers[layer].Tiles[MapPosition] = null;
                    }
                    else
                    {
                        MapData.Layers[layer].Tiles[MapPosition] = new Data.TileData
                        {
                            TilesetIndex = tilesetindex,
                            TileID = tileid
                        };
                    }
                    
                    if (olddata != MapData.Layers[layer].Tiles[MapPosition])
                    {
                        this.Sprites[layer.ToString()].Bitmap.FillRect(actualx * 32, actualy * 32, 32, 32, Color.ALPHA);
                        if (!Blank)
                        {
                            this.Sprites[layer.ToString()].Bitmap.Build(
                                actualx * 32, actualy * 32,
                                Data.GameData.Tilesets[MapData.TilesetIDs[tilesetindex]].TilesetBitmap,
                                new Rect(tilesetx * 32, tilesety * 32, 32, 32)
                            );
                        }
                    }
                }
            }

            this.Sprites[layer.ToString()].Bitmap.Lock();
        }
    }

    public class GridBackground : Widget
    {
        public Bitmap GridBitmap;
        public int OffsetX;
        public int OffsetY;

        public GridBackground(object Parent, string Name = "gridBackground")
            : base(Parent, Name)
        {
            GridBitmap = new Bitmap(32, 32);
            GridBitmap.Unlock();
            #region Draw Grid pattern
            Bitmap b = GridBitmap;
            /*Color Corner = new Color(102, 104, 111);
            Color CornerSmall = new Color(82, 84, 91);
            Color Outer = new Color(75, 79, 86);
            Color Inner = new Color(72, 76, 84);
            Color Fill = new Color(69, 73, 81);
            b.SetPixel(0, 0, Corner);
            b.SetPixel(1, 0, Corner);
            b.SetPixel(2, 0, Corner);
            b.SetPixel(0, 1, Corner);
            b.SetPixel(0, 2, Corner);
            b.SetPixel(1, 1, CornerSmall);
            b.SetPixel(2, 1, CornerSmall);
            b.SetPixel(1, 2, CornerSmall);
            b.DrawLine(3, 0, 28, 0, Outer);
            b.DrawLine(3, 1, 28, 1, Inner);
            b.SetPixel(29, 0, Corner);
            b.SetPixel(30, 0, Corner);
            b.SetPixel(31, 0, Corner);
            b.SetPixel(31, 1, Corner);
            b.SetPixel(31, 2, Corner);
            b.SetPixel(30, 1, CornerSmall);
            b.SetPixel(29, 1, CornerSmall);
            b.SetPixel(30, 2, CornerSmall);
            b.DrawLine(31, 3, 31, 28, Outer);
            b.DrawLine(30, 3, 30, 28, Inner);
            b.SetPixel(31, 29, Corner);
            b.SetPixel(31, 30, Corner);
            b.SetPixel(31, 31, Corner);
            b.SetPixel(30, 31, Corner);
            b.SetPixel(29, 31, Corner);
            b.SetPixel(30, 29, CornerSmall);
            b.SetPixel(30, 30, CornerSmall);
            b.SetPixel(29, 30, CornerSmall);
            b.DrawLine(28, 31, 3, 31, Outer);
            b.DrawLine(28, 30, 3, 30, Inner);
            b.SetPixel(0, 29, Corner);
            b.SetPixel(0, 30, Corner);
            b.SetPixel(0, 31, Corner);
            b.SetPixel(1, 31, Corner);
            b.SetPixel(2, 31, Corner);
            b.SetPixel(2, 30, CornerSmall);
            b.SetPixel(1, 30, CornerSmall);
            b.SetPixel(1, 29, CornerSmall);
            b.DrawLine(0, 28, 0, 3, Outer);
            b.DrawLine(1, 28, 1, 3, Inner);
            b.FillRect(2, 2, 28, 28, Fill);*/
            Color Cross = new Color(68, 71, 78);
            Color Line = new Color(48, 52, 59);
            Color Fill = new Color(40, 44, 52);
            b.SetPixel(0, 0, Cross);
            b.SetPixel(1, 0, Cross);
            b.SetPixel(2, 0, Cross);
            b.SetPixel(0, 1, Cross);
            b.SetPixel(0, 2, Cross);
            b.SetPixel(30, 0, Cross);
            b.SetPixel(31, 0, Cross);
            b.SetPixel(0, 30, Cross);
            b.SetPixel(0, 31, Cross);
            b.DrawLine(3, 0, 29, 0, Line);
            b.DrawLine(0, 3, 0, 29, Line);
            b.FillRect(1, 1, 31, 31, Fill);
            #endregion
            GridBitmap.Lock();
            Sprites["grid"] = new Sprite(this.Viewport);
            Sprites["grid"].Bitmap = GridBitmap;
            UpdatePositions();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            UpdatePositions();
        }

        public void SetOffset(int x, int y)
        {
            this.OffsetX = x;
            this.OffsetY = y;
        }

        public void UpdatePositions()
        {
            Sprites["grid"].MultiplePositions.Clear();
            int tilesx = (int) Math.Ceiling(this.Size.Width / 32d) + 1;
            int tilesy = (int) Math.Ceiling(this.Size.Height / 32d) + 1;
            for (int y = 0; y < tilesy; y++)
            {
                for (int x = 0; x < tilesx; x++)
                {
                    Sprites["grid"].MultiplePositions.Add(new Point(x * 32 - OffsetX, y * 32 - OffsetY));
                }
            }
        }
    }
}
