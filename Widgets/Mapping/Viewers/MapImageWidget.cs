using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class MapImageWidget : Widget
    {
        public GridBackground GridBackground;
        public MapViewerBase MapViewer;
        public Map MapData;

        public int AnimateCount = 0;
        public List<List<int>> AnimatedAutotiles = new List<List<int>>();

        public string Side;
        public int Offset;

        public double ZoomFactor = 1.0;

        public MapImageWidget(object Parent, string Name = "mapImageWidget")
            : base(Parent, Name)
        {
            SetBackgroundColor(73, 89, 109);
            this.MapViewer = this.Parent.Parent as MapViewerBase;
            this.GridBackground = new GridBackground(this);
            Sprites["dark"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(0, 0, 0, 0)));
            Sprites["dark"].Z = 99999999;
            SetTimer("frame", (long) Math.Round(1000 / 60d)); // 60 FPS
        }

        public void SetZoomFactor(double factor)
        {
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                Sprites[i.ToString()].ZoomX = Sprites[i.ToString()].ZoomY = factor;
            }
            this.ZoomFactor = factor;
            GridBackground.SetTileSize((int) Math.Round(32 * this.ZoomFactor));
            UpdateSize();
        }

        public void SetDarkOverlay(byte Opacity)
        {
            (Sprites["dark"].Bitmap as SolidBitmap).SetColor(0, 0, 0, Opacity);
        }

        public void UpdateSize()
        {
            int Width = (int) Math.Round(MapData.Width * 32 * ZoomFactor);
            int Height = (int) Math.Round(MapData.Height * 32 * ZoomFactor);
            if (Side == ":north" || Side == ":south") Height = (int) Math.Round(Math.Min(6, MapData.Height) * 32 * ZoomFactor);
            else if (Side == ":east" || Side == ":west") Width = (int) Math.Round(Math.Min(6, MapData.Width) * 32 * ZoomFactor);
            this.SetSize(Width, Height);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            GridBackground.SetSize(this.Size);
            (Sprites["dark"].Bitmap as SolidBitmap).SetSize(this.Size);
        }

        public void LoadLayers(Map MapData, string Side = "", int Offset = 0)
        {
            this.MapData = MapData;
            this.Side = Side;
            this.Offset = Offset;
            UpdateSize();
            RedrawLayers();
        }

        public void SetLayerVisible(int layerindex, bool Visible)
        {
            MapData.Layers[layerindex].Visible = Visible;
            Sprites[layerindex.ToString()].Visible = Visible;
        }

        public void CreateNewLayer(int Index, Game.Layer LayerData)
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
            Sprites[key] = new Sprite(this.Viewport, MapData.Width * 32, MapData.Height * 32);
            Sprites[key].Z = Index;
            Sprites[key].ZoomX = Sprites[key].ZoomY = this.ZoomFactor;
        }

        public void DeleteLayer(int Index)
        {
            this.Sprites[Index.ToString()].Dispose();
            this.Sprites.Remove(Index.ToString());
            for (int i = Index + 1; i < MapData.Layers.Count; i++)
            {
                Sprite s = this.Sprites[i.ToString()] as Sprite;
                s.Z -= 2;
                this.Sprites.Remove(i.ToString());
                this.Sprites.Add((i - 1).ToString(), s);
            }
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

        public List<Bitmap> GetBitmaps(int MapID, int SX, int SY, int Width, int Height)
        {
            AnimatedAutotiles.Clear();
            List<Bitmap> bmps = new List<Bitmap>();
            Map m = Data.Maps[MapID];
            if (SX < 0) SX = 0;
            if (SY < 0) SY = 0;
            if (SX + Width > m.Width) Width = m.Width - SX;
            if (SY + Height > m.Height) Height = m.Height - SY;
            for (int layer = 0; layer < m.Layers.Count; layer++)
            {
                bmps.Add(new Bitmap(Width * 32, Height * 32));
                bmps[layer].Unlock();
                // Iterate through all vertical tiles
                for (int y = SY; y < SY + Height; y++)
                {
                    // Iterate through all horizontal tiles
                    for (int x = SX; x < SX + Width; x++)
                    {
                        // Draw each individual tile
                        int mapx = (x - SX) * 32;
                        int mapy = (y - SY) * 32;
                        if (m.Layers[layer] == null || m.Layers[layer].Tiles == null ||
                            y * m.Width + x >= m.Layers[layer].Tiles.Count ||
                            m.Layers[layer].Tiles[y * m.Width + x] == null) continue;
                        int tile_id = m.Layers[layer].Tiles[y * m.Width + x].ID;
                        if (m.Layers[layer].Tiles[y * m.Width + x].TileType == TileType.Tileset)
                        {
                            int tileset_index = m.Layers[layer].Tiles[y * m.Width + x].Index;
                            int tileset_id = m.TilesetIDs[tileset_index];
                            Bitmap tilesetimage = Data.Tilesets[tileset_id].TilesetBitmap;
                            int tilesetx = tile_id % 8;
                            int tilesety = (int) Math.Floor(tile_id / 8d);
                            bmps[layer].Build(new Rect(mapx, mapy, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32));
                        }
                        else if (m.Layers[layer].Tiles[y * m.Width + x].TileType == TileType.Autotile)
                        {
                            int autotile_index = m.Layers[layer].Tiles[y * m.Width + x].Index;
                            int autotile_id = m.AutotileIDs[autotile_index];
                            Autotile autotile = Data.Autotiles[autotile_id];
                            if (autotile.AnimateSpeed > 0) AnimatedAutotiles.Add(new List<int>() { layer, x - SX, y - SY, autotile_id, tile_id });
                            Bitmap autotileimage = autotile.AutotileBitmap;
                            if (autotile.Format == AutotileFormat.Single)
                            {
                                int AnimX = 0;
                                bmps[layer].Build(new Rect(mapx, mapy, 32, 32), autotileimage, new Rect(AnimX, 0, 32, 32));
                            }
                            else
                            {
                                int AnimX = 0;
                                List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][tile_id];
                                for (int i = 0; i < 4; i++)
                                {
                                    bmps[layer].Build(new Rect(mapx + 16 * (i % 2), mapy + 16 * (int) Math.Floor(i / 2d), 16, 16), autotileimage,
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
                bmps[layer].Lock();
            }
            return bmps;
        }

        public void RedrawLayers()
        {
            foreach (string s in this.Sprites.Keys)
            {
                if (s != "_bg" && s != "dark") this.Sprites[s].Dispose();
            }
            // Create layers
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()] = new Sprite(this.Viewport);
                this.Sprites[i.ToString()].Z = i * 2;
                this.Sprites[i.ToString()].Visible = MapData.Layers[i].Visible;
            }
            int SX = 0;
            int SY = 0;
            int Width = MapData.Width;
            int Height = MapData.Height;
            if (this.Side == ":north")
            {
                SY = MapData.Height - 7;
                Height = 6;
            }
            else if (this.Side == ":east")
            {
                Width = 6;
            }
            else if (this.Side == ":south")
            {
                Height = 6;
            }
            else if (this.Side == ":west")
            {
                SX = MapData.Width - 7;
                Width = 6;
            }
            List<Bitmap> bmps = GetBitmaps(MapData.ID, SX, SY, Width, Height);
            for (int i = 0; i < bmps.Count; i++) Sprites[i.ToString()].Bitmap = bmps[i];
            // Zoom layers
            SetZoomFactor(ZoomFactor);
        }

        public void DrawTiles(int oldx, int oldy, int newx, int newy, int layer)
        {
            MapViewerTiles MapViewer = this.MapViewer as MapViewerTiles;
            // Avoid drawing a line from top left to current tile
            if (oldx == -1 || oldy == -1)
            {
                oldx = newx;
                oldy = newy;
            }
            Point Origin = MapViewer.OriginPoint;
            bool blanktile = Editor.MainWindow.MapWidget.MapViewerTiles.TilesPanel.EraserButton.Selected;
            bool point = oldx == newx && oldy == newy;
            bool line = !point;
            List<Point> TempCoords = new List<Point>();
            if (Editor.MainWindow.MapWidget.MapViewerTiles.TilesPanel.FillButton.Selected)
            {
                int sx, sy, ex, ey;
                if (MapViewer.SelectionX != -1 && MapViewer.SelectionY != -1 && MapViewer.SelectionWidth != 0 && MapViewer.SelectionHeight != 0 && MapViewer.SelectionBackground.Visible)
                {
                    int mx = (int) Math.Floor(newx / 32d);
                    int my = (int) Math.Floor(newy / 32d);
                    sx = MapViewer.SelectionX;
                    ex = MapViewer.SelectionX + MapViewer.SelectionWidth;
                    sy = MapViewer.SelectionY;
                    ey = MapViewer.SelectionY + MapViewer.SelectionHeight;
                    if (!(mx >= sx && mx < ex && my >= sy && my < ey)) return; // Outside selection
                }
                else
                {
                    sx = 0;
                    sy = 0;
                    ex = MapData.Width;
                    ey = MapData.Height;
                }
                for (int y = sy; y < ey; y++)
                {
                    for (int x = sx; x < ex; x++)
                    {
                        TempCoords.Add(new Point(x, y));
                    }
                }
            }
            else if (line) // Draw tiles between several tiles - use simple line drawing algorithm to determine the tiles to draw on
            {
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
                    if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                }
                int sy = y1 > y2 ? y2 : y1;
                for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                {
                    double fact = ((double) y - y1) / (y2 - y1);
                    int x = (int) Math.Round(x1 + ((x2 - x1) * fact));
                    int tilex = (int) Math.Floor(x / 32d);
                    int tiley = (int) Math.Floor(y / 32d);
                    if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                }
            }
            else if (point) // Just one singular tile
            {
                TempCoords.Add(new Point((int) Math.Floor(newx / 32d), (int) Math.Floor(newy / 32d)));
            }

            this.Sprites[layer.ToString()].Bitmap.Unlock();
            for (int i = 0; i < TempCoords.Count; i++)
            {
                // Both of these depend on the origin, but we need them both at the top left as we begin drawing there
                // and to be able to compare them to use in the modulus, they both have to be adjusted
                int MapTileX = TempCoords[i].X;
                int MapTileY = TempCoords[i].Y;
                int OriginX = MapViewer.OriginPoint.X;
                int OriginY = MapViewer.OriginPoint.Y;
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

                    int actualx = MapTileX + (j % (MapViewer.CursorWidth + 1));
                    int actualy = MapTileY + (int) Math.Floor((double) j / (MapViewer.CursorWidth + 1));
                    int MapPosition = actualx + actualy * MapData.Width;
                    if (actualx < 0 || actualx >= MapData.Width || actualy < 0 || actualy >= MapData.Height) continue;
                    if (MapViewer.SelectionX != -1 && MapViewer.SelectionY != -1 && MapViewer.SelectionWidth != 0 && MapViewer.SelectionHeight != 0 && MapViewer.SelectionBackground.Visible)
                    {
                        // NOT within the selection
                        if (!(actualx >= MapViewer.SelectionX && actualx < MapViewer.SelectionX + MapViewer.SelectionWidth &&
                              actualy >= MapViewer.SelectionY && actualy < MapViewer.SelectionY + MapViewer.SelectionHeight))
                            continue;
                    }

                    int selx = j % (MapViewer.CursorWidth + 1);
                    if (OriginDiffX < 0) selx -= OriginDiffX;
                    if (OriginDiffX > 0) selx -= OriginDiffX;
                    if (selx < 0) selx += MapViewer.CursorWidth + 1;
                    selx %= MapViewer.CursorWidth + 1;
                    int sely = (int) Math.Floor((double)j / (MapViewer.CursorWidth + 1));
                    if (OriginDiffY < 0) sely -= OriginDiffY;
                    if (OriginDiffY > 0) sely -= OriginDiffY;
                    if (sely < 0) sely += MapViewer.CursorHeight + 1;
                    sely %= MapViewer.CursorHeight + 1;

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

                    TileData olddata = MapData.Layers[layer].Tiles[MapPosition];
                    if (Blank)
                    {
                        MapData.Layers[layer].Tiles[MapPosition] = null;
                    }
                    else
                    {
                        MapData.Layers[layer].Tiles[MapPosition] = new TileData
                        {
                            TileType = tiletype,
                            Index = index,
                            ID = tileid
                        };
                    }

                    if (olddata is null && MapData.Layers[layer].Tiles[MapPosition] != null ||
                        !(olddata is null) && MapData.Layers[layer].Tiles[MapPosition] == null ||
                        !(olddata is null) && 
                        (olddata.TileType != MapData.Layers[layer].Tiles[MapPosition].TileType ||
                        olddata.Index != MapData.Layers[layer].Tiles[MapPosition].Index ||
                        olddata.ID != MapData.Layers[layer].Tiles[MapPosition].ID))
                    {
                        for (int k = 0; k < AnimatedAutotiles.Count; k++)
                        {
                            List<int> data = AnimatedAutotiles[k];
                            if (data[0] == layer && data[1] == actualx && data[2] == actualy)
                            {
                                AnimatedAutotiles.RemoveAt(k);
                                break;
                            }
                        }

                        if (Blank)
                        {
                            if (!(olddata is null) && olddata.TileType == TileType.Autotile)
                            {
                                UpdateAutotiles(layer, actualx, actualy, olddata.Index, true, true);
                            }
                            else
                            {
                                this.Sprites[layer.ToString()].Bitmap.FillRect(actualx * 32, actualy * 32, 32, 32, Color.ALPHA);
                            }
                        }
                        else
                        {
                            if (tiletype == TileType.Tileset)
                            {
                                Editor.UnsavedChanges = true;
                                this.Sprites[layer.ToString()].Bitmap.FillRect(actualx * 32, actualy * 32, 32, 32, Color.ALPHA);
                                if (!Blank)
                                {
                                    this.Sprites[layer.ToString()].Bitmap.Build(
                                        actualx * 32, actualy * 32,
                                        Data.Tilesets[MapData.TilesetIDs[index]].TilesetBitmap,
                                        new Rect(32 * (tileid % 8), 32 * (int) Math.Floor(tileid / 8d), 32, 32)
                                    );
                                }
                            }
                            else if (tiletype == TileType.Autotile)
                            {
                                if (tileid != -1) // Only draws
                                {
                                    AnimatedAutotiles.Add(new List<int>() { layer, actualx, actualy, MapData.AutotileIDs[index], tileid });
                                    DrawAutotile(layer, actualx, actualy, MapData.AutotileIDs[index], tileid,
                                        (int) Math.Floor((double) AnimateCount / Data.Autotiles[MapData.AutotileIDs[index]].AnimateSpeed));
                                }
                                else // Draws and updates
                                {
                                    UpdateAutotiles(layer, actualx, actualy, index, true, false);
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine();
            this.Sprites[layer.ToString()].Bitmap.Lock();
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
            if (CheckNeighbouring || !(MapData.Layers[Layer].Tiles[X + Y * MapData.Width] == null ||
                  MapData.Layers[Layer].Tiles[X + Y * MapData.Width].TileType != TileType.Autotile ||
                  MapData.Layers[Layer].Tiles[X + Y * MapData.Width].Index != AutotileIndex))
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
                        this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X, 32 * Y, 32, 32), autotile.AutotileBitmap,
                            new Rect(AnimX, 0, 32, 32));
                    }
                    else
                    {
                        AnimX = ((int) Math.Floor((double) AnimateCount / autotile.AnimateSpeed) * 96) % autotile.AutotileBitmap.Width;
                        List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][ID];
                        for (int i = 0; i < 4; i++)
                        {
                            this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X + 16 * (i % 2), 32 * Y + 16 * (int)Math.Floor(i / 2d), 16, 16), autotile.AutotileBitmap,
                                new Rect(16 * (Tiles[i] % 6) + AnimX, 16 * (int)Math.Floor(Tiles[i] / 6d), 16, 16));
                        }
                    }
                }
                else
                {
                    MapData.Layers[Layer].Tiles[X + Y * MapData.Width] = null;
                    this.Sprites[Layer.ToString()].Bitmap.FillRect(X * 32, Y * 32, 32, 32, Color.ALPHA);
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
