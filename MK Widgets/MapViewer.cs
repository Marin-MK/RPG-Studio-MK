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

        MouseEventArgs LastMouseEvent;

        public MapViewer(object Parent, string Name = "mapViewer")
            : base(Parent, Name)
        {
            this.SetBackgroundColor(28, 50, 73);
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
            this.WidgetIM.OnMouseUp += MouseUp;
            this.OnWidgetSelect += WidgetSelect;
            MainContainer = new Container(this);
            MainContainer.SetPosition(1, 0);
            MainContainer.HAutoScroll = MainContainer.VAutoScroll = true;
            MainContainer.OnScrolling += delegate (object sender, EventArgs e)
            {
                if (LastMouseEvent != null) this.MouseMoving(sender, LastMouseEvent);
            };
            Cursor = new CursorWidget(MainContainer);
            Cursor.ConsiderInAutoScroll = false;
            MapWidget = new MapImageWidget(MainContainer);
            Sprites["topleft"] = new Sprite(this.Viewport);
            Sprites["topright"] = new Sprite(this.Viewport);
            Sprites["bottomleft"] = new Sprite(this.Viewport);
            Sprites["bottomright"] = new Sprite(this.Viewport);
            Sprites["left"] = new Sprite(this.Viewport);
            Sprites["top"] = new Sprite(this.Viewport);
            Sprites["right"] = new Sprite(this.Viewport);
            Sprites["bottom"] = new Sprite(this.Viewport);
            Sprites["hslider"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width - 13, 11, new Color(10, 23, 37)));
            Sprites["hslider"].X = 1;
            Sprites["vslider"] = new Sprite(this.Viewport, new SolidBitmap(11, Size.Height - 12, new Color(10, 23, 37)));
            HScrollBar = new HScrollBar(this);
            HScrollBar.SetPosition(2, Size.Height - 9);
            HScrollBar.SetSize(Size.Width - 15, 8);
            MainContainer.SetHScrollBar(HScrollBar);
            VScrollBar = new VScrollBar(this);
            VScrollBar.SetPosition(Size.Width - 9, 1);
            VScrollBar.SetSize(8, Size.Height - 14);
            MainContainer.SetVScrollBar(VScrollBar);
            UpdateSize();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            MainContainer.SetSize(this.Size.Width - 12, this.Size.Height - 11);
            PositionMap();
            UpdateSize();
            if (MainContainer.HScrollBar != null) MainContainer.HScrollBar.SetValue(0.5);
            if (MainContainer.VScrollBar != null) MainContainer.VScrollBar.SetValue(0.5);
        }

        private void UpdateSize()
        {
            Bitmap b;
            if (Sprites["topleft"].Bitmap == null)
            {
                b = new Bitmap(14, 12);
                #region Draw Corner
                b.Unlock();
                b.FillRect(0, 0, 14, 12, new Color(22, 40, 58));
                b.SetPixel(3, 11, new Color(22, 40, 59));
                b.SetPixel(4, 9, new Color(22, 40, 59));
                b.SetPixel(4, 8, new Color(22, 40, 59));
                b.SetPixel(4, 7, new Color(22, 40, 59));
                b.SetPixel(4, 8, new Color(22, 40, 59));
                b.SetPixel(5, 6, new Color(22, 40, 59));
                b.SetPixel(6, 5, new Color(22, 40, 59));
                b.SetPixel(7, 4, new Color(22, 40, 59));
                b.SetPixel(8, 4, new Color(22, 40, 59));
                b.SetPixel(9, 4, new Color(22, 40, 59));
                b.SetPixel(11, 3, new Color(22, 40, 59));
                b.SetPixel(12, 3, new Color(22, 40, 59));
                b.SetPixel(13, 3, new Color(22, 40, 59));

                b.SetPixel(4, 11, new Color(23, 41, 60));
                b.SetPixel(4, 10, new Color(23, 41, 60));
                b.SetPixel(5, 8, new Color(23, 41, 60));
                b.SetPixel(5, 7, new Color(23, 41, 60));
                b.SetPixel(6, 6, new Color(23, 41, 60));
                b.SetPixel(7, 5, new Color(23, 41, 60));
                b.SetPixel(5, 8, new Color(23, 41, 60));
                b.SetPixel(8, 5, new Color(23, 41, 60));
                b.SetPixel(10, 4, new Color(23, 41, 60));
                b.SetPixel(11, 4, new Color(23, 41, 60));
                b.SetPixel(12, 4, new Color(23, 41, 60));
                b.SetPixel(13, 4, new Color(23, 41, 60));

                b.SetPixel(5, 9, new Color(23, 41, 61));
                b.SetPixel(9, 5, new Color(23, 41, 61));

                b.SetPixel(5, 10, new Color(23, 42, 61));
                b.SetPixel(6, 7, new Color(23, 42, 61));
                b.SetPixel(7, 6, new Color(23, 42, 61));
                b.SetPixel(10, 5, new Color(23, 42, 61));

                b.SetPixel(5, 11, new Color(23, 42, 62));
                b.SetPixel(6, 8, new Color(23, 42, 62));
                b.SetPixel(8, 6, new Color(23, 42, 62));
                b.SetPixel(11, 5, new Color(23, 42, 62));
                b.SetPixel(12, 5, new Color(23, 42, 62));
                b.SetPixel(13, 5, new Color(23, 42, 62));

                b.SetPixel(7, 7, new Color(24, 43, 62));

                b.SetPixel(6, 9, new Color(24, 43, 63));
                b.SetPixel(9, 6, new Color(24, 43, 63));

                b.SetPixel(6, 10, new Color(24, 43, 64));
                b.SetPixel(10, 6, new Color(24, 43, 64));

                b.SetPixel(6, 11, new Color(24, 44, 64));
                b.SetPixel(7, 8, new Color(24, 44, 64));
                b.SetPixel(8, 7, new Color(24, 44, 64));
                b.SetPixel(11, 6, new Color(24, 44, 64));
                b.SetPixel(12, 6, new Color(24, 44, 64));
                b.SetPixel(13, 6, new Color(24, 44, 64));

                b.SetPixel(7, 9, new Color(25, 45, 65));
                b.SetPixel(9, 7, new Color(25, 45, 65));

                b.SetPixel(7, 10, new Color(25, 45, 66));
                b.SetPixel(8, 8, new Color(25, 45, 66));
                b.SetPixel(10, 7, new Color(25, 45, 66));

                b.SetPixel(7, 11, new Color(25, 46, 67));
                b.SetPixel(11, 7, new Color(25, 46, 67));

                b.SetPixel(12, 7, new Color(26, 46, 67));
                b.SetPixel(13, 7, new Color(26, 46, 67));

                b.SetPixel(8, 9, new Color(26, 46, 68));
                b.SetPixel(9, 8, new Color(26, 46, 68));

                b.SetPixel(8, 11, new Color(26, 47, 69));
                b.SetPixel(8, 10, new Color(26, 47, 69));
                b.SetPixel(9, 9, new Color(26, 47, 69));
                b.SetPixel(10, 8, new Color(26, 47, 69));
                b.SetPixel(11, 8, new Color(26, 47, 69));

                b.SetPixel(9, 10, new Color(27, 48, 70));
                b.SetPixel(10, 9, new Color(27, 48, 70));
                b.SetPixel(12, 8, new Color(27, 48, 70));

                b.SetPixel(13, 8, new Color(27, 48, 70));

                b.SetPixel(9, 11, new Color(27, 49, 71));
                b.SetPixel(10, 10, new Color(27, 49, 71));
                b.SetPixel(11, 9, new Color(27, 49, 71));
                b.SetPixel(12, 9, new Color(27, 49, 71));
                b.SetPixel(13, 9, new Color(27, 49, 71));

                b.SetPixel(10, 11, new Color(27, 49, 72));
                b.SetPixel(11, 11, new Color(27, 49, 72));
                b.SetPixel(11, 10, new Color(27, 49, 72));
                b.SetPixel(12, 10, new Color(27, 49, 72));
                b.SetPixel(13, 10, new Color(27, 49, 72));

                b.SetPixel(12, 11, new Color(28, 50, 73));
                b.SetPixel(13, 11, new Color(28, 50, 73));
                b.Lock();
                #endregion
                Sprites["topleft"].Bitmap = b;
                Sprites["topright"].Bitmap = b;
                Sprites["topright"].MirrorX = true;
                Sprites["bottomleft"].Bitmap = b;
                Sprites["bottomleft"].MirrorY = true;
                Sprites["bottomright"].Bitmap = b;
                Sprites["bottomright"].MirrorX = true;
                Sprites["bottomright"].MirrorY = true;

                Bitmap side = new Bitmap(14, 1);
                #region Side Gradient
                side.Unlock();
                side.SetPixel(0, 0, new Color(22, 40, 58));
                side.SetPixel(1, 0, new Color(22, 40, 58));
                side.SetPixel(2, 0, new Color(22, 40, 58));
                side.SetPixel(3, 0, new Color(22, 40, 59));
                side.SetPixel(4, 0, new Color(23, 41, 60));
                side.SetPixel(5, 0, new Color(23, 42, 62));
                side.SetPixel(6, 0, new Color(24, 44, 64));
                side.SetPixel(7, 0, new Color(26, 46, 67));
                side.SetPixel(8, 0, new Color(27, 48, 70));
                side.SetPixel(9, 0, new Color(27, 49, 71));
                side.SetPixel(10, 0, new Color(27, 49, 72));
                side.SetPixel(11, 0, new Color(28, 50, 73));
                side.SetPixel(12, 0, new Color(28, 50, 73));
                side.SetPixel(13, 0, new Color(28, 50, 73));
                side.Lock();
                #endregion
                Sprites["left"].Bitmap = side;
                Sprites["top"].Bitmap = side;
                Sprites["top"].Angle = 90;
                Sprites["right"].Bitmap = side;
                Sprites["right"].MirrorX = true;
                Sprites["bottom"].Bitmap = side;
                Sprites["bottom"].Angle = -90;

                Sprites["borderline"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(10, 23, 37)));
            }
            else b = Sprites["topleft"].Bitmap as Bitmap;
            Sprites["topleft"].X = 1;
            Sprites["topright"].X = Size.Width - b.Width - 11;
            Sprites["bottomleft"].X = 1;
            Sprites["bottomleft"].Y = Size.Height - b.Height - 11;
            Sprites["bottomright"].X = Size.Width - b.Width - 11;
            Sprites["bottomright"].Y = Size.Height - b.Height - 11;
            Sprites["left"].X = 1;
            Sprites["left"].Y = b.Height;
            Sprites["left"].ZoomY = Size.Height - 11 - 2 * b.Height;
            Sprites["top"].X = Size.Width - b.Width - 11;
            Sprites["top"].ZoomY = Size.Width - 12 - 2 * b.Width;
            Sprites["right"].X = Size.Width - b.Width - 11;
            Sprites["right"].Y = Sprites["left"].Y;
            Sprites["right"].ZoomY = Sprites["left"].ZoomY;
            Sprites["bottom"].X = 1 + b.Width;
            Sprites["bottom"].Y = Size.Height - 11;
            Sprites["bottom"].ZoomY = Sprites["top"].ZoomY;

            Sprites["borderline"].Bitmap.Unlock();
            (Sprites["borderline"].Bitmap as SolidBitmap).SetSize(1, Size.Height);
            Sprites["borderline"].Bitmap.Lock();

            Sprites["hslider"].Bitmap.Unlock();
            (Sprites["hslider"].Bitmap as SolidBitmap).SetSize(Size.Width - 13, 11);
            Sprites["hslider"].Bitmap.Lock();
            Sprites["hslider"].Y = Size.Height - 11;

            Sprites["vslider"].Bitmap.Unlock();
            (Sprites["vslider"].Bitmap as SolidBitmap).SetSize(11, Size.Height - 12);
            Sprites["vslider"].Bitmap.Lock();
            Sprites["vslider"].X = Size.Width - 11;

            HScrollBar.SetPosition(2, Size.Height - 9);
            HScrollBar.SetSize(Size.Width - 15, 8);
            VScrollBar.SetPosition(Size.Width - 9, 1);
            VScrollBar.SetSize(8, Size.Height - 14);
        }

        public void SetMap(Data.Map Map)
        {
            this.Map = Map;
            TilesetTab.SetTilesets(Map.TilesetIDs);
            this.CreateLayerBitmaps();
            this.LayersTab.CreateLayers();
            TilesetTab.SelectTile(new Data.TileData() { TilesetIndex = 0, TileID = 0 });
            if (MainContainer.HScrollBar != null) MainContainer.HScrollBar.SetValue(0.5);
            if (MainContainer.VScrollBar != null) MainContainer.VScrollBar.SetValue(0.5);
        }

        public void RedrawLayers()
        {
            MapWidget.RedrawLayers();
        }

        public void CreateLayerBitmaps()
        {
            MapWidget.LoadLayers(this.Map);
            PositionMap();
        }

        public void PositionMap()
        {
            int width = this.Map.Width * 32 + this.Viewport.Width / 2;
            if (width < this.Viewport.Width) width = MainContainer.Size.Width;
            int height = this.Map.Height * 32 + this.Viewport.Height / 2;
            if (height < this.Viewport.Height) height = MainContainer.Size.Height;
            int x = width / 2 - MapWidget.Size.Width / 2;
            int y = height / 2 - MapWidget.Size.Height / 2;
            int offsetx = 32 - x % 32;
            int offsety = 32 - y % 32;
            MapWidget.SetPosition(x, y);
            MainContainer.UpdateAutoScroll();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            LastMouseEvent = e;
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
            Cursor.SetPosition(MapWidget.Position.X + 32 * MapTileX - offsetx - 7, MapWidget.Position.Y + 32 * MapTileY - offsety - 7);
            Cursor.SetSize(32 * (CursorWidth + 1) + 14, 32 * (CursorHeight + 1) + 14);
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
            if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.Dragging || MainContainer.HScrollBar.Hovering)) return;
            if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.Dragging || MainContainer.VScrollBar.Hovering)) return;
            // Input handling
            if (WidgetIM.ClickedLeftInArea == true)
            {
                if (OriginDrawPoint == null) OriginDrawPoint = new Point((int) Math.Floor(oldx / 32d), (int) Math.Floor(oldy / 32d));
                int Layer = this.LayersTab.SelectedLayer;
                if (TileDataList.Count == 0)
                {
                    if (TilesetTab.EraserButton.Selected) TileDataList.Add(null);
                    else
                    {
                        throw new Exception($"The tile data list is empty, but the eraser tool is not selected.\nCan't find tiles to draw with.");
                    }
                }
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
            SetBackgroundColor(28, 50, 73);
            Sprites["grid"] = new Sprite(this.Viewport);
            Sprites["grid"].Z = 999999;
            this.MapViewer = this.Parent.Parent as MapViewer;
        }

        public void LoadLayers(Data.Map MapData)
        {
            this.MapData = MapData;
            SetBackgroundColor(73, 89, 109);
            this.SetSize(MapData.Width * 32, MapData.Height * 32);
            RedrawLayers();
            RedrawGrid();
        }

        public void RedrawGrid()
        {
            if (Sprites["grid"].Bitmap != null) Sprites["grid"].Bitmap.Dispose();
            Sprites["grid"].Bitmap = new Bitmap(MapData.Width * 32, MapData.Height * 32);
            Sprites["grid"].Bitmap.Unlock();
            Color c = new Color(0, 13, 26, 128);
            for (int y = 0; y < MapData.Height * 32; y++)
            {
                for (int x = 0; x < MapData.Width * 32; x++)
                {
                    bool draw = false;
                    if (x > 0 && x % 32 == 0 && y % 2 == 0) draw = true;
                    if (y > 0 && y % 32 == 0 && x % 2 == 0) draw = true;
                    if (draw)
                    {
                        Sprites["grid"].Bitmap.SetPixel(x, y, c);
                    }
                }
            }
            Sprites["grid"].Bitmap.Lock();
        }

        public void RedrawLayers()
        {
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
}
