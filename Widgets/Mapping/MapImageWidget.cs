using System;
using System.Collections.Generic;
using odl;
using RPGStudioMK.Game;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class MapImageWidget : Widget
    {
        public int MapID;
        public int RelativeX;
        public int RelativeY;

        public GridBackground GridBackground;
        public MapViewerBase MapViewer;
        public Map MapData;
        public Rect Rect;

        public int AnimateCount = 0;
        public List<List<int>> AnimatedAutotiles = new List<List<int>>();

        public double ZoomFactor = 1.0;

        public MapImageWidget(IContainer Parent) : base(Parent)
        {
            SetBackgroundColor(73, 89, 109);
            this.MapViewer = this.Parent.Parent.Parent as MapViewerBase;
            this.GridBackground = new GridBackground(this);
            this.GridBackground.SetVisible(Editor.GeneralSettings.ShowGrid);
            Sprites["dark"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(0, 0, 0, 0)));
            Sprites["dark"].Z = 999999;
            SetTimer("frame", (long) Math.Round(1000 / 60d)); // 60 FPS
        }

        public void SetZoomFactor(double factor)
        {
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                Sprites[i.ToString()].ZoomX = factor;
                Sprites[i.ToString()].ZoomY = factor;
            }
            this.ZoomFactor = factor;
            GridBackground.SetTileSize((int) Math.Round(32 * this.ZoomFactor));
            UpdateSize();
        }

        public void SetDarkOverlay(byte Opacity)
        {
            (Sprites["dark"].Bitmap as SolidBitmap).SetColor(0, 0, 0, Opacity);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            GridBackground.SetSize(this.Size);
            (Sprites["dark"].Bitmap as SolidBitmap).SetSize(this.Size);
        }

        public void SetLayerVisible(int layerindex, bool Visible)
        {
            MapData.Layers[layerindex].Visible = Visible;
            Sprites[layerindex.ToString()].Visible = Visible;
        }

        public void CreateNewLayer(int Index, Layer LayerData, bool IsUndoAction = false)
        {
            for (int i = MapData.Layers.Count - 1; i >= Index; i--)
            {
                Sprite s = this.Sprites[i.ToString()] as Sprite;
                s.Z += 2;
                this.Sprites.Remove(i.ToString());
                this.Sprites.Add((i + 1).ToString(), s);
            }
            MapData.Layers.Insert(Index, LayerData);
            string key = Index.ToString();
            Sprites[key] = new Sprite(this.Viewport);
            Sprites[key].Bitmap = GetLayerBitmap(Index);
            Sprites[key].Z = Index;
            Sprites[key].ZoomX = Sprites[key].ZoomY = this.ZoomFactor;
            if (!IsUndoAction) LayerChangeUndoAction.Create(MapID, Index, MapData.Layers[Index], false);
        }

        public void DeleteLayer(int Index, bool IsUndoAction = false)
        {
            this.Sprites[Index.ToString()].Dispose();
            this.Sprites.Remove(Index.ToString());
            for (int y = 0; y < MapData.Height; y++)
            {
                for (int x = 0; x < MapData.Width; x++)
                {
                    TileData TileData = MapData.Layers[Index].Tiles[x + y * MapData.Width];
                    if (TileData == null) continue;
                    List<int> autotile = AnimatedAutotiles.Find(a => a[0] == Index && a[1] == x && a[2] == y);
                    if (autotile != null) AnimatedAutotiles.Remove(autotile);
                }
            }
            for (int i = Index + 1; i < MapData.Layers.Count; i++)
            {
                Sprite s = this.Sprites[i.ToString()] as Sprite;
                s.Z -= 2;
                this.Sprites.Remove(i.ToString());
                this.Sprites.Add((i - 1).ToString(), s);
            }
            if (!IsUndoAction) LayerChangeUndoAction.Create(MapID, Index, MapData.Layers[Index], true);
            MapData.Layers.RemoveAt(Index);
        }

        public override void Update()
        {
            base.Update();
            if (!WidgetIM.WidgetAccessible()) return;
            if (!IsVisible()) return;
            if (TimerPassed("frame"))
            {
                ResetTimer("frame");
                if (!Editor.GeneralSettings.ShowMapAnimations) return;
                List<int> UpdateLayers = new List<int>();
                AnimateCount++;
                foreach (List<int> data in AnimatedAutotiles)
                {
                    if (AnimateCount % Data.Autotiles[data[3]].AnimateSpeed == 0)
                    {
                        if (!UpdateLayers.Contains(data[0])) UpdateLayers.Add(data[0]);
                    }
                }
                for (int i = 0; i < UpdateLayers.Count; i++)
                {
                    this.Sprites[UpdateLayers[i].ToString()].Bitmap.Unlock();
                }
                foreach (List<int> data in AnimatedAutotiles)
                {
                    if (AnimateCount % Data.Autotiles[data[3]].AnimateSpeed == 0)
                        DrawAutotile(data[0], data[1], data[2], data[3], data[4], (int) Math.Floor((double) AnimateCount / Data.Autotiles[data[3]].AnimateSpeed));
                }
                for (int i = 0; i < UpdateLayers.Count; i++)
                {
                    this.Sprites[UpdateLayers[i].ToString()].Bitmap.Lock();
                }
            }
        }

        public void SwapLayers(int Index1, int Index2)
        {
            foreach (List<int> data in AnimatedAutotiles)
            {
                if (data[0] == Index1) data[0] = Index2;
                else if (data[0] == Index2) data[0] = Index1;
            }
            Sprite s1 = this.Sprites[Index1.ToString()] as Sprite;
            s1.Z = Index2 * 2;
            Sprite s2 = this.Sprites[Index2.ToString()] as Sprite;
            s2.Z = Index1 * 2;
            this.Sprites.Remove(Index1.ToString());
            this.Sprites.Remove(Index2.ToString());
            this.Sprites[Index1.ToString()] = s2;
            this.Sprites[Index2.ToString()] = s1;
            Layer l1 = MapData.Layers[Index1];
            MapData.Layers[Index1] = MapData.Layers[Index2];
            MapData.Layers[Index2] = l1;
        }

        public Bitmap GetLayerBitmap(int Layer)
        {
            Bitmap bmp = new Bitmap(MapData.Width * 32, MapData.Height * 32, 16 * 32, 16 * 32); // 16x16 tile chunks
            bmp.Unlock();
            // Iterate through all vertical tiles
            for (int y = 0; y < MapData.Height; y++)
            {
                // Iterate through all horizontal tiles
                for (int x = 0; x < MapData.Width; x++)
                {
                    // Draw each individual tile
                    if (MapData.Layers[Layer] == null || MapData.Layers[Layer].Tiles == null ||
                        y * MapData.Width + x >= MapData.Layers[Layer].Tiles.Count ||
                        MapData.Layers[Layer].Tiles[y * MapData.Width + x] == null) continue;
                    int mapx = x * 32;
                    int mapy = y * 32;
                    int tile_id = MapData.Layers[Layer].Tiles[y * MapData.Width + x].ID;
                    if (MapData.Layers[Layer].Tiles[y * MapData.Width + x].TileType == TileType.Tileset)
                    {
                        int tileset_index = MapData.Layers[Layer].Tiles[y * MapData.Width + x].Index;
                        int tileset_id = MapData.TilesetIDs[tileset_index];
                        Bitmap tilesetimage = Data.Tilesets[tileset_id].TilesetBitmap;
                        int tilesetx = tile_id % 8;
                        int tilesety = (int) Math.Floor(tile_id / 8d);
                        bmp.Build(new Rect(mapx, mapy, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32));
                    }
                    else if (MapData.Layers[Layer].Tiles[y * MapData.Width + x].TileType == TileType.Autotile)
                    {
                        int autotile_index = MapData.Layers[Layer].Tiles[y * MapData.Width + x].Index;
                        int autotile_id = MapData.AutotileIDs[autotile_index];
                        Autotile autotile = Data.Autotiles[autotile_id];
                        if (autotile.AnimateSpeed > 0) AnimatedAutotiles.Add(new List<int>() { Layer, x, y, autotile_id, tile_id });
                        Bitmap autotileimage = autotile.AutotileBitmap;
                        if (autotile.Format == AutotileFormat.Single)
                        {
                            int AnimX = 0;
                            bmp.Build(new Rect(mapx, mapy, 32, 32), autotileimage, new Rect(AnimX, 0, 32, 32));
                        }
                        else
                        {
                            int AnimX = 0;
                            List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][tile_id];
                            for (int i = 0; i < 4; i++)
                            {
                                bmp.Build(new Rect(mapx + 16 * (i % 2), mapy + 16 * (int) Math.Floor(i / 2d), 16, 16), autotileimage,
                                    new Rect(16 * (Tiles[i] % 6) + AnimX, 16 * (int) Math.Floor(Tiles[i] / 6d), 16, 16));
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid tile type.");
                    }
                }
            }
            bmp.Lock();
            return bmp;
        }

        public void SetMapAnimations(bool Animations)
        {
            if (!Animations)
            {
                List<int> UpdateLayers = new List<int>();
                AnimateCount++;
                foreach (List<int> data in AnimatedAutotiles)
                {
                    if (!UpdateLayers.Contains(data[0])) UpdateLayers.Add(data[0]);
                }
                for (int i = 0; i < UpdateLayers.Count; i++)
                {
                    this.Sprites[UpdateLayers[i].ToString()].Bitmap.Unlock();
                }
                foreach (List<int> data in AnimatedAutotiles)
                {
                    DrawAutotile(data[0], data[1], data[2], data[3], data[4], 0);
                }
                for (int i = 0; i < UpdateLayers.Count; i++)
                {
                    this.Sprites[UpdateLayers[i].ToString()].Bitmap.Lock();
                }
            }
        }

        public void SetGridVisibility(bool Visible)
        {
            GridBackground.SetVisible(Visible);
        }

        public virtual void UpdateSize()
        {
            int Width = (int) Math.Round(MapData.Width * 32 * ZoomFactor);
            int Height = (int) Math.Round(MapData.Height * 32 * ZoomFactor);
            this.SetSize(Width, Height);
        }

        public virtual void LoadLayers(Map MapData, int RelativeX = 0, int RelativeY = 0)
        {
            this.MapData = MapData;
            this.MapID = MapData.ID;
            this.RelativeX = RelativeX;
            this.RelativeY = RelativeY;
            UpdateSize();
            RedrawLayers();
        }

        public virtual void RedrawLayers()
        {
            foreach (string key in this.Sprites.Keys)
            {
                if (Utilities.IsNumeric(key))
                {
                    this.Sprites[key].Dispose();
                    this.Sprites.Remove(key);
                }
            }
            // Create layers
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()] = new Sprite(this.Viewport);
                this.Sprites[i.ToString()].Z = i * 2;
                this.Sprites[i.ToString()].Visible = MapData.Layers[i].Visible;
            }
            List<Bitmap> bmps = new List<Bitmap>();
            AnimatedAutotiles.Clear();
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                bmps.Add(GetLayerBitmap(i));
                Sprites[i.ToString()].Bitmap = bmps[i];
            }
            // Zoom layers
            SetZoomFactor(ZoomFactor);
        }

        public bool IsLayerLocked(int LayerIndex)
        {
            return this.Sprites[LayerIndex.ToString()].Bitmap.Locked;
        }

        public void SetLayerLocked(int LayerIndex, bool Locked)
        {
            ISprite s = this.Sprites[LayerIndex.ToString()];
            if (Locked) s.Bitmap.Lock();
            else s.Bitmap.Unlock();
        }

        public List<Point> GetTilesFromMouse(int oldx, int oldy, int newx, int newy, int layer)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            List<Point> Coords = new List<Point>();
            switch (MapViewer.TilesPanel.DrawTool)
            {
                case DrawTools.Pencil:
                    CalculateTilesPencil(Coords, oldx, oldy, newx, newy);
                    break;
                case DrawTools.RectangleFilled:
                    CalculateTilesRectangleFilled(Coords, newx, newy);
                    break;
                case DrawTools.RectangleOutline:
                    CalculateTilesRectangleOutline(Coords, newx, newy);
                    break;
                case DrawTools.EllipseFilled:
                    CalculateTilesEllipseFilled(Coords, newx, newy);
                    break;
                case DrawTools.EllipseOutline:
                    CalculateTilesEllipseOutline(Coords, newx, newy);
                    break;
                case DrawTools.Bucket:
                    CalculateTilesBucket(Coords, newx, newy, layer);
                    break;
            }
            return Coords;
        }

        void CalculateTilesBucket(List<Point> Coords, int newx, int newy, int layer)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            int x = (int) Math.Floor(newx / 32d);
            int y = (int) Math.Floor(newy / 32d);
            if (MapViewer.SelectionWidth != 0 && MapViewer.SelectionHeight != 0)
            {
                if (x < MapViewer.SelectionX || x >= MapViewer.SelectionX + MapViewer.SelectionWidth ||
                    y < MapViewer.SelectionY || y >= MapViewer.SelectionY + MapViewer.SelectionHeight)
                {
                    return;
                }
            }
            List<Point> tiles = Utilities.GetIdenticalConnected(MapData, layer, x, y);
            foreach (Point point in tiles)
            {
                Coords.Add(point);
            }
        }

        void CalculateTilesPencil(List<Point> Coords, int oldx, int oldy, int newx, int newy)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            // Avoid drawing a line from top left to current tile
            if (oldx == -1 || oldy == -1)
            {
                oldx = newx;
                oldy = newy;
            }
            if (oldx == newx && oldy == newy) // Don't bother with line calculations: it's only one single tile.
            {
                Coords.Add(new Point((int) Math.Floor(newx / 32d), (int) Math.Floor(newy / 32d)));
            }
            else
            {
                // Interpolates the new mouse position over the old position and adds all tiles
                // in the line so as to not skip any tiles in between the old and new mouse position.
                int x1 = oldx;
                int y1 = oldy;
                int x2 = newx;
                int y2 = newy;
                for (int x = x1 > x2 ? x2 : x1; (x1 > x2) ? (x <= x1) : (x <= x2); x++)
                {
                    double fact = ((double) x - x1) / (x2 - x1);
                    int y = (int) Math.Round(y1 + ((y2 - y1) * fact));
                    int tilex = (int) Math.Floor(x / 32d);
                    int tiley = (int) Math.Floor(y / 32d);
                    if (!Coords.Exists(c => c.X == tilex && c.Y == tiley)) Coords.Add(new Point(tilex, tiley));
                }
                int sy = y1 > y2 ? y2 : y1;
                for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                {
                    double fact = ((double) y - y1) / (y2 - y1);
                    int x = (int) Math.Round(x1 + ((x2 - x1) * fact));
                    int tilex = (int) Math.Floor(x / 32d);
                    int tiley = (int) Math.Floor(y / 32d);
                    if (!Coords.Exists(c => c.X == tilex && c.Y == tiley)) Coords.Add(new Point(tilex, tiley));
                }
            }
            // Determine how many tiles are part of the line.
            // We add coordinates after this if the cursor is bigger than 1x1,
            // thus we use this constant count.
            int count = Coords.Count;
            // Now for each recorded tile in the interpolated line, we add all tiles
            // that need to also be drawn if the cursor was bigger than 1x1.
            for (int i = 0; i < count; i++)
            {
                for (int cx = 0; cx <= MapViewer.CursorWidth; cx++)
                {
                    for (int cy = 0; cy <= MapViewer.CursorHeight; cy++)
                    {
                        if (cx == 0 && cy == 0) continue; // Equal to already-calculated top-left coord
                        int px = Coords[i].X;
                        int py = Coords[i].Y;
                        // If the origin is in the bottom/right, we want to count tiles down, not up (or we'd exceed the cursor bounds)
                        if (MapViewer.CursorOrigin == Location.TopRight || MapViewer.CursorOrigin == Location.BottomRight) px -= cx;
                        else px += cx;
                        if (MapViewer.CursorOrigin == Location.BottomLeft || MapViewer.CursorOrigin == Location.BottomRight) py -= cy;
                        else py += cy;
                        if (!Coords.Exists(c => c.X == px && c.Y == py)) Coords.Add(new Point(px, py));
                    }
                }
            }
        }

        void CalculateTilesEllipseFilled(List<Point> Coords, int newx, int newy)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            int x1 = MapViewer.OriginPoint.X * 32;
            int y1 = MapViewer.OriginPoint.Y * 32;
            int x2 = newx;
            int y2 = newy;
            if (Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_RSHIFT))
            {
                if (x1 < x2)
                {
                    if (y1 < y2)
                    {
                        if (x2 - x1 > y2 - y1) x2 = x1 + (y2 - y1);
                        else if (y2 - y1 > x2 - x1) y2 = y1 + (x2 - x1);
                    }
                    else if (y2 < y1)
                    {
                        if (x2 - x1 > y1 - y2) x2 = x1 + (y1 - y2);
                        else if (y1 - y2 > x2 - x1) y2 = y1 - (x2 - x1);
                    }
                }
                else if (x2 < x1)
                {
                    if (y1 < y2)
                    {
                        if (x1 - x2 > y2 - y1) x2 = x1 - (y2 - y1);
                        else if (y2 - y1 > x1 - x2) y2 = y1 + (x1 - x2);
                    }
                    else if (y2 < y1)
                    {
                        if (x1 - x2 > y1 - y2) x2 = x1 - (y1 - y2);
                        else if (y1 - y2 > x1 - x2) y2 = y1 - (x1 - x2);
                    }
                }
            }
            double cx = x1 / 2d + x2 / 2d;
            double cy = y1 / 2d + y2 / 2d;
            double a = cx - x1;
            double b = cy - y1;
            int ox1 = MapViewer.OriginPoint.X;
            int oy1 = MapViewer.OriginPoint.Y;
            int ox2 = (int) Math.Round(newx / 32d);
            int oy2 = (int) Math.Round(newy / 32d);
            if (a == 0 || b == 0)
            {
                for (int y = oy1 > oy2 ? oy2 : oy1; y <= (oy1 > oy2 ? oy1 : oy2); y++)
                {
                    for (int x = ox1 > ox2 ? ox2 : ox1; x <= (ox1 > ox2 ? ox1 : ox2); x++)
                    {
                        Coords.Add(new Point(x, y));
                    }
                }
                return;
            }
            for (int x = 0; a > 0 ? x <= a : x >= a; x += (a > 0 ? 1 : -1))
            {
                int ry = (int) Math.Round(b / a * Math.Sqrt(a * a - x * x));
                int tilex1 = (int) Math.Round((cx + x) / 32d);
                int tilex2 = (int) Math.Round((cx - x) / 32d);
                int tiley1 = (int) Math.Round((cy + ry) / 32d);
                int tiley2 = (int) Math.Round((cy - ry) / 32d);
                if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley1)) Coords.Add(new Point(tilex1, tiley1));
                if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley1)) Coords.Add(new Point(tilex2, tiley1));
                if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley2)) Coords.Add(new Point(tilex1, tiley2));
                if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley2)) Coords.Add(new Point(tilex2, tiley2));
            }
            List<Point> fillpoints = new List<Point>();
            int sx = (int) Math.Round((x1 > x2 ? x2 : x1) / 32d);
            int ex = (int) Math.Round((x1 > x2 ? x1 : x2) / 32d);
            for (int x = sx; x <= ex; x++)
            {
                List<int> ys = Coords.FindAll(p => p.X == x).ConvertAll<int>(p => p.Y);
                if (ys.Count >= 2)
                {
                    int miny = int.MaxValue;
                    foreach (int py in ys) if (py < miny) miny = py;
                    int maxy = int.MinValue;
                    foreach (int py in ys) if (py > maxy) maxy = py;
                    for (int y = miny; y < maxy; y++)
                    {
                        if (!fillpoints.Exists(p => p.X == x && p.Y == y)) fillpoints.Add(new Point(x, y));
                    }
                }
            }
            Coords.AddRange(fillpoints);
            if (Math.Abs(ox2 - ox1) >= 2 && Math.Abs(oy2 - oy1) >= 2)
                Coords.RemoveAll(c => c.X == ox1 && (c.Y == oy1 || c.Y == oy2) || c.X == ox2 && (c.Y == oy1 || c.Y == oy2));
        }

        void CalculateTilesEllipseOutline(List<Point> Coords, int newx, int newy)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            int x1 = MapViewer.OriginPoint.X * 32;
            int y1 = MapViewer.OriginPoint.Y * 32;
            int x2 = newx;
            int y2 = newy;
            if (Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_RSHIFT))
            {
                if (x1 < x2)
                {
                    if (y1 < y2)
                    {
                        if (x2 - x1 > y2 - y1) x2 = x1 + (y2 - y1);
                        else if (y2 - y1 > x2 - x1) y2 = y1 + (x2 - x1);
                    }
                    else if (y2 < y1)
                    {
                        if (x2 - x1 > y1 - y2) x2 = x1 + (y1 - y2);
                        else if (y1 - y2 > x2 - x1) y2 = y1 - (x2 - x1);
                    }
                }
                else if (x2 < x1)
                {
                    if (y1 < y2)
                    {
                        if (x1 - x2 > y2 - y1) x2 = x1 - (y2 - y1);
                        else if (y2 - y1 > x1 - x2) y2 = y1 + (x1 - x2);
                    }
                    else if (y2 < y1)
                    {
                        if (x1 - x2 > y1 - y2) x2 = x1 - (y1 - y2);
                        else if (y1 - y2 > x1 - x2) y2 = y1 - (x1 - x2);
                    }
                }
            }
            double cx = x1 / 2d + x2 / 2d;
            double cy = y1 / 2d + y2 / 2d;
            double a = cx - x1;
            double b = cy - y1;
            int ox1 = MapViewer.OriginPoint.X;
            int oy1 = MapViewer.OriginPoint.Y;
            int ox2 = (int) Math.Round(newx / 32d);
            int oy2 = (int) Math.Round(newy / 32d);
            if (a == 0 || b == 0)
            {
                for (int y = oy1 > oy2 ? oy2 : oy1; y <= (oy1 > oy2 ? oy1 : oy2); y++)
                {
                    for (int x = ox1 > ox2 ? ox2 : ox1; x <= (ox1 > ox2 ? ox1 : ox2); x++)
                    {
                        Coords.Add(new Point(x, y));
                    }
                }
                return;
            }
            for (int x = 0; a > 0 ? x <= a : x >= a; x += (a > 0 ? 1 : -1))
            {
                int ry = (int) Math.Round(b / a * Math.Sqrt(a * a - x * x));
                int tilex1 = (int) Math.Round((cx + x) / 32d);
                int tilex2 = (int) Math.Round((cx - x) / 32d);
                int tiley1 = (int) Math.Round((cy + ry) / 32d);
                int tiley2 = (int) Math.Round((cy - ry) / 32d);
                if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley1)) Coords.Add(new Point(tilex1, tiley1));
                if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley1)) Coords.Add(new Point(tilex2, tiley1));
                if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley2)) Coords.Add(new Point(tilex1, tiley2));
                if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley2)) Coords.Add(new Point(tilex2, tiley2));
            }
            for (int y = 0; b > 0 ? y <= b : y >= b; y += (b > 0 ? 1 : -1))
            {
                int rx = (int) Math.Round(a / b * Math.Sqrt(b * b - y * y));
                int tilex1 = (int) Math.Round((cx + rx) / 32d);
                int tilex2 = (int) Math.Round((cx - rx) / 32d);
                int tiley1 = (int) Math.Round((cy + y) / 32d);
                int tiley2 = (int) Math.Round((cy - y) / 32d);
                if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley1)) Coords.Add(new Point(tilex1, tiley1));
                if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley1)) Coords.Add(new Point(tilex2, tiley1));
                if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley2)) Coords.Add(new Point(tilex1, tiley2));
                if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley2)) Coords.Add(new Point(tilex2, tiley2));
            }
            if (Math.Abs(ox2 - ox1) >= 2 && Math.Abs(oy2 - oy1) >= 2)
                Coords.RemoveAll(c => c.X == ox1 && (c.Y == oy1 || c.Y == oy2) || c.X == ox2 && (c.Y == oy1 || c.Y == oy2));
        }

        void CalculateTilesRectangleFilled(List<Point> Coords, int newx, int newy)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            int x1 = MapViewer.OriginPoint.X;
            int y1 = MapViewer.OriginPoint.Y;
            int x2 = (int) Math.Floor(newx / 32d);
            int y2 = (int) Math.Floor(newy / 32d);
            if (Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_RSHIFT))
            {
                if (x1 < x2)
                {
                    if (y1 < y2)
                    {
                        if (x2 - x1 > y2 - y1) x2 = x1 + (y2 - y1);
                        else if (y2 - y1 > x2 - x1) y2 = y1 + (x2 - x1);
                    }
                    else if (y2 < y1)
                    {
                        if (x2 - x1 > y1 - y2) x2 = x1 + (y1 - y2);
                        else if (y1 - y2 > x2 - x1) y2 = y1 - (x2 - x1);
                    }
                }
                else if (x2 < x1)
                {
                    if (y1 < y2)
                    {
                        if (x1 - x2 > y2 - y1) x2 = x1 - (y2 - y1);
                        else if (y2 - y1 > x1 - x2) y2 = y1 + (x1 - x2);
                    }
                    else if (y2 < y1)
                    {
                        if (x1 - x2 > y1 - y2) x2 = x1 - (y1 - y2);
                        else if (y1 - y2 > x1 - x2) y2 = y1 - (x1 - x2);
                    }
                }
            }
            for (int x = x1 > x2 ? x2 : x1; (x1 > x2) ? (x <= x1) : (x <= x2); x++)
            {
                for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                {
                    Coords.Add(new Point(x, y));
                }
            }
        }

        void CalculateTilesRectangleOutline(List<Point> Coords, int newx, int newy)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            int x1 = MapViewer.OriginPoint.X;
            int y1 = MapViewer.OriginPoint.Y;
            int x2 = (int) Math.Floor(newx / 32d);
            int y2 = (int) Math.Floor(newy / 32d);
            if (Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_RSHIFT))
            {
                if (x1 < x2)
                {
                    if (y1 < y2)
                    {
                        if (x2 - x1 > y2 - y1) x2 = x1 + (y2 - y1);
                        else if (y2 - y1 > x2 - x1) y2 = y1 + (x2 - x1);
                    }
                    else if (y2 < y1)
                    {
                        if (x2 - x1 > y1 - y2) x2 = x1 + (y1 - y2);
                        else if (y1 - y2 > x2 - x1) y2 = y1 - (x2 - x1);
                    }
                }
                else if (x2 < x1)
                {
                    if (y1 < y2)
                    {
                        if (x1 - x2 > y2 - y1) x2 = x1 - (y2 - y1);
                        else if (y2 - y1 > x1 - x2) y2 = y1 + (x1 - x2);
                    }
                    else if (y2 < y1)
                    {
                        if (x1 - x2 > y1 - y2) x2 = x1 - (y1 - y2);
                        else if (y1 - y2 > x1 - x2) y2 = y1 - (x1 - x2);
                    }
                }
            }
            for (int x = x1 > x2 ? x2 : x1; (x1 > x2) ? (x <= x1) : (x <= x2); x++)
            {
                if (x == x1 || x == x2)
                {
                    for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                    {
                        Coords.Add(new Point(x, y));
                    }
                }
                else
                {
                    Coords.Add(new Point(x, y1));
                    Coords.Add(new Point(x, y2));
                }
            }
        }

        public void DrawTiles(List<Point> Coords, int layer)
        {
            MapViewerTiles MapViewer = (MapViewerTiles) this.MapViewer;
            SetLayerLocked(layer, false);
            for (int i = 0; i < Coords.Count; i++)
            {
                int MapTileX = Coords[i].X;
                int MapTileY = Coords[i].Y;
                if (MapTileX < 0 || MapTileX >= MapData.Width || MapTileY < 0 || MapTileY >= MapData.Height) continue;
                int OriginX = MapViewer.OriginPoint.X;
                int OriginY = MapViewer.OriginPoint.Y;
                if (MapViewer.CursorOrigin == Location.TopRight || MapViewer.CursorOrigin == Location.BottomRight)
                {
                    OriginX -= MapViewer.CursorWidth;
                }
                if (MapViewer.CursorOrigin == Location.BottomLeft || MapViewer.CursorOrigin == Location.BottomRight)
                {
                    OriginY -= MapViewer.CursorHeight;
                }
                // MapTileX and MapTileY are now the top left no matter the origin point

                int OriginDiffX = (MapTileX - OriginX) % (MapViewer.CursorWidth + 1);
                int OriginDiffY = (MapTileY - OriginY) % (MapViewer.CursorHeight + 1);

                int MapPosition = MapTileX + MapTileY * MapData.Width;
                if (MapViewer.SelectionWidth != 0 && MapViewer.SelectionHeight != 0 && MapViewer.SelectionBackground.Visible)
                {
                    // NOT within the selection
                    if (!(MapTileX >= MapViewer.SelectionX && MapTileX < MapViewer.SelectionX + MapViewer.SelectionWidth &&
                            MapTileY >= MapViewer.SelectionY && MapTileY < MapViewer.SelectionY + MapViewer.SelectionHeight))
                        continue;
                }

                int selx = OriginDiffX;
                if (selx < 0) selx = MapViewer.CursorWidth + 1 + selx;
                int sely = OriginDiffY;
                if (sely < 0) sely = MapViewer.CursorHeight + 1 + sely;

                bool Blank = MapViewer.TilesPanel.Erase;

                TileData tiledata = MapViewer.TileDataList[sely * (MapViewer.CursorWidth + 1) + selx];
                TileType tiletype = TileType.Tileset;
                int tileid = -1;
                int index = -1;
                if (tiledata != null)
                {
                    tiletype = tiledata.TileType;
                    tileid = tiledata.ID;
                    index = tiledata.Index;
                }
                else Blank = true;

                TileData OldTile = MapData.Layers[layer].Tiles[MapPosition];
                TileData NewTile = null;
                if (!Blank)
                {
                    NewTile = new TileData
                    {
                        TileType = tiletype,
                        Index = index,
                        ID = tileid
                    };
                }

                bool SameTile = true;
                if (OldTile == null && NewTile != null ||
                    OldTile != null && NewTile == null) SameTile = false;
                else if (OldTile != null && OldTile.TileType != NewTile.TileType) SameTile = false;
                else if (OldTile != null && OldTile.Index != NewTile.Index) SameTile = false;
                else if (OldTile != null && OldTile.TileType != TileType.Autotile && OldTile.ID != NewTile.ID) SameTile = false;

                if (!SameTile)
                {
                    MapData.Layers[layer].Tiles[MapPosition] = NewTile;
                    if (TileGroupUndoAction.GetLatest() == null || TileGroupUndoAction.GetLatest().Ready)
                    {
                        Editor.CanUndo = false;
                        TileGroupUndoAction.Log(MapID);
                    }
                    TileGroupUndoAction.AddToLatest(MapPosition, layer, NewTile, OldTile);
                    DrawTile(MapTileX, MapTileY, layer, NewTile, OldTile);
                }
            }
            SetLayerLocked(layer, true);
        }

        public void DrawTile(int X, int Y, int Layer, TileData Tile, TileData OldTile, bool ForceUpdateNearbyAutotiles = false)
        {
            SetZoomFactor(this.ZoomFactor);
            bool Blank = Tile == null;
            for (int k = 0; k < AnimatedAutotiles.Count; k++)
            {
                List<int> data = AnimatedAutotiles[k];
                if (data[0] == Layer && data[1] == X && data[2] == Y)
                {
                    AnimatedAutotiles.RemoveAt(k);
                    break;
                }
            }

            if (OldTile != null && OldTile.TileType == TileType.Autotile)
            {
                UpdateAutotiles(Layer, X, Y, OldTile.Index, true, true);
            }

            if (Blank)
            {
                this.Sprites[Layer.ToString()].Bitmap.FillRect(X * 32, Y * 32, 32, 32, Color.ALPHA);
            }
            else
            {
                this.Sprites[Layer.ToString()].Bitmap.FillRect(X * 32, Y * 32, 32, 32, Color.ALPHA);
                if (Tile.TileType == TileType.Tileset)
                {
                    Editor.UnsavedChanges = true;
                    this.Sprites[Layer.ToString()].Bitmap.Build(
                        X * 32, Y * 32,
                        Data.Tilesets[MapData.TilesetIDs[Tile.Index]].TilesetBitmap,
                        new Rect(32 * (Tile.ID % 8), 32 * (int) Math.Floor(Tile.ID / 8d), 32, 32)
                    );
                }
                else if (Tile.TileType == TileType.Autotile)
                {
                    if (Tile.ID != -1 && !ForceUpdateNearbyAutotiles) // Only draws
                    {
                        AnimatedAutotiles.Add(new List<int>() { Layer, X, Y, MapData.AutotileIDs[Tile.Index], Tile.ID });
                        int frame = Editor.GeneralSettings.ShowMapAnimations ? (int) Math.Floor((double) AnimateCount / Data.Autotiles[MapData.AutotileIDs[Tile.Index]].AnimateSpeed) : 0;
                        DrawAutotile(Layer, X, Y, MapData.AutotileIDs[Tile.Index], Tile.ID, frame);
                    }
                    else // Draws and updates
                    {
                        UpdateAutotiles(Layer, X, Y, Tile.Index, true, false);
                    }
                }
            }
        }

        public void DrawAutotile(int Layer, int X, int Y, int AutotileID, int TileID, int Frame)
        {
            Autotile autotile = Data.Autotiles[AutotileID];
            int AnimX = 0;
            if (autotile.Format == AutotileFormat.Single)
            {
                AnimX = (Frame * 32) % autotile.AutotileBitmap.Width;
                this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X, 32 * Y, 32, 32), autotile.AutotileBitmap,
                    new Rect(AnimX, 0, 32, 32));
            }
            else
            {
                AnimX = (Frame * 96) % autotile.AutotileBitmap.Width;
                List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][TileID];
                for (int i = 0; i < 4; i++)
                {
                    this.Sprites[Layer.ToString()].Bitmap.Build(
                        new Rect(
                            32 * X + 16 * (i % 2),
                            32 * Y + 16 * (int) Math.Floor(i / 2d),
                            16,
                            16
                        ),
                        autotile.AutotileBitmap,
                        new Rect(
                            16 * (Tiles[i] % 6) + AnimX,
                            16 * (int) Math.Floor(Tiles[i] / 6d),
                            16,
                            16
                        )
                    );
                }
            }
        }

        public void UpdateAutotiles(int Layer, int X, int Y, int AutotileIndex, bool CheckNeighbouring = false, bool DeleteTile = false)
        {
            TileData TileData = MapData.Layers[Layer].Tiles[X + Y * MapData.Width];
            List<Point> Connected = new List<Point>()
            {
                new Point(X - 1, Y - 1), new Point(X, Y - 1), new Point(X + 1, Y - 1),
                new Point(X - 1, Y    ),                      new Point(X + 1, Y),
                new Point(X - 1, Y + 1), new Point(X, Y + 1), new Point(X + 1, Y + 1)
            };
            bool NWauto = Connected[0].X >= 0 && Connected[0].X < MapData.Width &&
                          Connected[0].Y >= 0 && Connected[0].Y < MapData.Height &&
                          MapData.Layers[Layer].Tiles[Connected[0].X + Connected[0].Y * MapData.Width] != null &&
                          MapData.Layers[Layer].Tiles[Connected[0].X + Connected[0].Y * MapData.Width].TileType == TileType.Autotile;
            bool NW = NWauto && MapData.Layers[Layer].Tiles[Connected[0].X + Connected[0].Y * MapData.Width].Index == AutotileIndex;
            bool Nauto = Connected[1].X >= 0 && Connected[1].X < MapData.Width &&
                         Connected[1].Y >= 0 && Connected[1].Y < MapData.Height &&
                         MapData.Layers[Layer].Tiles[Connected[1].X + Connected[1].Y * MapData.Width] != null &&
                         MapData.Layers[Layer].Tiles[Connected[1].X + Connected[1].Y * MapData.Width].TileType == TileType.Autotile;
            bool N = Nauto && MapData.Layers[Layer].Tiles[Connected[1].X + Connected[1].Y * MapData.Width].Index == AutotileIndex;
            bool NEauto = Connected[2].X >= 0 && Connected[2].X < MapData.Width &&
                          Connected[2].Y >= 0 && Connected[2].Y < MapData.Height &&
                          MapData.Layers[Layer].Tiles[Connected[2].X + Connected[2].Y * MapData.Width] != null &&
                          MapData.Layers[Layer].Tiles[Connected[2].X + Connected[2].Y * MapData.Width].TileType == TileType.Autotile;
            bool NE = NEauto && MapData.Layers[Layer].Tiles[Connected[2].X + Connected[2].Y * MapData.Width].Index == AutotileIndex;
            bool Wauto = Connected[3].X >= 0 && Connected[3].X < MapData.Width &&
                         Connected[3].Y >= 0 && Connected[3].Y < MapData.Height &&
                         MapData.Layers[Layer].Tiles[Connected[3].X + Connected[3].Y * MapData.Width] != null &&
                         MapData.Layers[Layer].Tiles[Connected[3].X + Connected[3].Y * MapData.Width].TileType == TileType.Autotile;
            bool W = Wauto && MapData.Layers[Layer].Tiles[Connected[3].X + Connected[3].Y * MapData.Width].Index == AutotileIndex;
            bool Eauto = Connected[4].X >= 0 && Connected[4].X < MapData.Width &&
                         Connected[4].Y >= 0 && Connected[4].Y < MapData.Height &&
                         MapData.Layers[Layer].Tiles[Connected[4].X + Connected[4].Y * MapData.Width] != null &&
                         MapData.Layers[Layer].Tiles[Connected[4].X + Connected[4].Y * MapData.Width].TileType == TileType.Autotile;
            bool E = Eauto && MapData.Layers[Layer].Tiles[Connected[4].X + Connected[4].Y * MapData.Width].Index == AutotileIndex;
            bool SWauto = Connected[5].X >= 0 && Connected[5].X < MapData.Width &&
                          Connected[5].Y >= 0 && Connected[5].Y < MapData.Height &&
                          MapData.Layers[Layer].Tiles[Connected[5].X + Connected[5].Y * MapData.Width] != null &&
                          MapData.Layers[Layer].Tiles[Connected[5].X + Connected[5].Y * MapData.Width].TileType == TileType.Autotile;
            bool SW = SWauto && MapData.Layers[Layer].Tiles[Connected[5].X + Connected[5].Y * MapData.Width].Index == AutotileIndex;
            bool Sauto = Connected[6].X >= 0 && Connected[6].X < MapData.Width &&
                         Connected[6].Y >= 0 && Connected[6].Y < MapData.Height &&
                         MapData.Layers[Layer].Tiles[Connected[6].X + Connected[6].Y * MapData.Width] != null &&
                         MapData.Layers[Layer].Tiles[Connected[6].X + Connected[6].Y * MapData.Width].TileType == TileType.Autotile;
            bool S = Sauto && MapData.Layers[Layer].Tiles[Connected[6].X + Connected[6].Y * MapData.Width].Index == AutotileIndex;
            bool SEauto = Connected[7].X >= 0 && Connected[7].X < MapData.Width &&
                          Connected[7].Y >= 0 && Connected[7].Y < MapData.Height &&
                          MapData.Layers[Layer].Tiles[Connected[7].X + Connected[7].Y * MapData.Width] != null &&
                          MapData.Layers[Layer].Tiles[Connected[7].X + Connected[7].Y * MapData.Width].TileType == TileType.Autotile;
            bool SE = SEauto && MapData.Layers[Layer].Tiles[Connected[7].X + Connected[7].Y * MapData.Width].Index == AutotileIndex;
            if (CheckNeighbouring || TileData != null && TileData.TileType == TileType.Autotile && TileData.Index == AutotileIndex)
                  // Only try to update the current tile if it's assignment (not deletion)
                  // and if the current tile is also an autotile
            {
                int ID = -1;
                if (NW && NE && SE && SW && W && N && E && S) ID = 0;
                else if (!NW && NE && SE && SW && W && N && E && S) ID = 1;
                else if (NW && !NE && SE && SW && W && N && E && S) ID = 2;
                else if (!NW && !NE && SE && SW & W && N && E && S) ID = 3;
                else if (NW && NE && !SE && SW && W && N && E && S) ID = 4;
                else if (!NW && NE && !SE && SW && W && N && E && S) ID = 5;
                else if (NW && !NE && !SE && SW && W && N && E && S) ID = 6;
                else if (!NW && !NE && !SE && SW && W && N && E && S) ID = 7;
                else if (NW && NE && SE && !SW && W && N && E && S) ID = 8;
                else if (!NW && NE && SE && !SW && W && N && E && S) ID = 9;
                else if (NW && !NE && SE && !SW && W && N && E && S) ID = 10;
                else if (!NW && !NE && SE && !SW && W && N && E && S) ID = 11;
                else if (NW && NE && !SE && !SW && W && N && E && S) ID = 12;
                else if (!NW && NE && !SE && !SW && W && N && E && S) ID = 13;
                else if (NW && !NE && !SE && !SW && W && N && E && S) ID = 14;
                else if (!NW && !NE && !SE && !SW && W && N && E && S) ID = 15;
                else if (NE && SE && !W && N && E && S) ID = 16;
                else if (!NE && SE && !W && N && E && S) ID = 17;
                else if (NE && !SE && !W && N && E && S) ID = 18;
                else if (!NE && !SE && !W && N && E && S) ID = 19;
                else if (SE && SW && W && !N && E && S) ID = 20;
                else if (!SE && SW && W && !N && E && S) ID = 21;
                else if (SE && !SW && W && !N && E && S) ID = 22;
                else if (!SE && !SW && W && !N && E && S) ID = 23;
                else if (NW && SW && W && N && !E && S) ID = 24;
                else if (NW && !SW && W && N && !E && S) ID = 25;
                else if (!NW && SW && W && N && !E && S) ID = 26;
                else if (!NW && !SW && W && N && !E && S) ID = 27;
                else if (NW && NE && W && N && E && !S) ID = 28;
                else if (!NW && NE && W && N && E && !S) ID = 29;
                else if (NW && !NE && W && N && E && !S) ID = 30;
                else if (!NW && !NE && W && N && E && !S) ID = 31;
                else if (!W && N && !E && S) ID = 32;
                else if (W && !N && E && !S) ID = 33;
                else if (SE && !W && !N && E && S) ID = 34;
                else if (!SE && !W && !N && E && S) ID = 35;
                else if (SW && W && !N && !E && S) ID = 36;
                else if (!SW && W && !N && !E && S) ID = 37;
                else if (NW && W && N && !E && !S) ID = 38;
                else if (!NW && W && N && !E && !S) ID = 39;
                else if (NE && !W && N && E && !S) ID = 40;
                else if (!NE && !W && N && E && !S) ID = 41;
                else if (!W && !N && !E && S) ID = 42;
                else if (!W && !N && E && !S) ID = 43;
                else if (!W && N && !E && !S) ID = 44;
                else if (W && !N && !E && !S) ID = 45;
                else if (!W && !N && !E && !S) ID = 46;
                if (ID != -1 && !DeleteTile)
                {
                    for (int i = 0; i < AnimatedAutotiles.Count; i++)
                    {
                        List<int> data = AnimatedAutotiles[i];
                        if (data[0] == Layer && data[1] == X && data[2] == Y)
                        {
                            AnimatedAutotiles.RemoveAt(i);
                            break;
                        }
                    }
                    MapData.Layers[Layer].Tiles[X + Y * MapData.Width].ID = ID;
                    Autotile autotile = Data.Autotiles[MapData.AutotileIDs[AutotileIndex]];
                    if (autotile.AnimateSpeed > 0)
                    {
                        AnimatedAutotiles.Add(new List<int>() { Layer, X, Y, Data.Autotiles.IndexOf(autotile), ID });
                    }
                    int AnimX = 0;
                    if (autotile.Format == AutotileFormat.Single)
                    {
                        AnimX = ((int) Math.Floor((double) AnimateCount / autotile.AnimateSpeed) * 32) % autotile.AutotileBitmap.Width;
                        if (!Editor.GeneralSettings.ShowMapAnimations) AnimX = 0;
                        this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X, 32 * Y, 32, 32), autotile.AutotileBitmap,
                            new Rect(AnimX, 0, 32, 32));
                    }
                    else
                    {
                        AnimX = ((int) Math.Floor((double) AnimateCount / autotile.AnimateSpeed) * 96) % autotile.AutotileBitmap.Width;
                        if (!Editor.GeneralSettings.ShowMapAnimations) AnimX = 0;
                        List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][ID];
                        for (int i = 0; i < 4; i++)
                        {
                            this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X + 16 * (i % 2), 32 * Y + 16 * (int) Math.Floor(i / 2d), 16, 16), autotile.AutotileBitmap,
                                new Rect(16 * (Tiles[i] % 6) + AnimX, 16 * (int)Math.Floor(Tiles[i] / 6d), 16, 16));
                        }
                    }
                }
            }
            // Whether or not to update neighbouring tiles
            if (CheckNeighbouring)
            {
                // Only update neighbours if they contain autotiles
                // (they don't need to be the same autotile; if autotile B is drawn over A, then surrounding A must also be updated)
                if (NWauto) UpdateAutotiles(Layer, Connected[0].X, Connected[0].Y, MapData.Layers[Layer].Tiles[Connected[0].X + Connected[0].Y * MapData.Width].Index);
                if (Nauto) UpdateAutotiles(Layer, Connected[1].X, Connected[1].Y, MapData.Layers[Layer].Tiles[Connected[1].X + Connected[1].Y * MapData.Width].Index);
                if (NEauto) UpdateAutotiles(Layer, Connected[2].X, Connected[2].Y, MapData.Layers[Layer].Tiles[Connected[2].X + Connected[2].Y * MapData.Width].Index);
                if (Wauto) UpdateAutotiles(Layer, Connected[3].X, Connected[3].Y, MapData.Layers[Layer].Tiles[Connected[3].X + Connected[3].Y * MapData.Width].Index);
                if (Eauto) UpdateAutotiles(Layer, Connected[4].X, Connected[4].Y, MapData.Layers[Layer].Tiles[Connected[4].X + Connected[4].Y * MapData.Width].Index);
                if (SWauto) UpdateAutotiles(Layer, Connected[5].X, Connected[5].Y, MapData.Layers[Layer].Tiles[Connected[5].X + Connected[5].Y * MapData.Width].Index);
                if (Sauto) UpdateAutotiles(Layer, Connected[6].X, Connected[6].Y, MapData.Layers[Layer].Tiles[Connected[6].X + Connected[6].Y * MapData.Width].Index);
                if (SEauto) UpdateAutotiles(Layer, Connected[7].X, Connected[7].Y, MapData.Layers[Layer].Tiles[Connected[7].X + Connected[7].Y * MapData.Width].Index);
            }
        }
    }
}
