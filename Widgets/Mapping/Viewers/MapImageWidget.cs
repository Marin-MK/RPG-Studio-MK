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
                        // Each individual tile
                        if (m.Layers[layer] == null || m.Layers[layer].Tiles == null ||
                            y * m.Width + x >= m.Layers[layer].Tiles.Count ||
                            m.Layers[layer].Tiles[y * m.Width + x] == null) continue;
                        if (m.Layers[layer].Tiles[y * m.Width + x].TileType == TileType.Autotile) throw new Exception("Cannot draw autotiles yet.");
                        int tileset_index = m.Layers[layer].Tiles[y * m.Width + x].Index;
                        int tileset_id = m.TilesetIDs[tileset_index];
                        Bitmap tilesetimage = Data.Tilesets[tileset_id].TilesetBitmap;
                        int mapx = (x - SX) * 32;
                        int mapy = (y - SY) * 32;
                        int tile_id = m.Layers[layer].Tiles[y * m.Width + x].ID;
                        int tilesetx = tile_id % 8;
                        int tilesety = (int) Math.Floor(tile_id / 8d);
                        bmps[layer].Build(new Rect(mapx, mapy, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32));
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
            // This resets the two points tiles are drawn in between if the mouse has gone off the map (otherwise it'd draw a line between
            // the last point on the map and the current point on the map)
            bool blanktile = Editor.MainWindow.MapWidget.MapViewerTiles.TilesetPanel.EraserButton.Selected;
            bool line = !(oldx == newx && oldy == newy);
            List<Point> TempCoords = new List<Point>();
            if (Editor.MainWindow.MapWidget.MapViewerTiles.TilesetPanel.FillButton.Selected)
            {
                int sx, sy, ex, ey;
                if (MapViewer.SelectionX != -1 && MapViewer.SelectionY != -1 && MapViewer.SelectionWidth != 0 && MapViewer.SelectionHeight != 0 && MapViewer.SelectionBackground.Visible)
                {
                    int mx = (int)Math.Floor(newx / 32d);
                    int my = (int)Math.Floor(newy / 32d);
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
                    double fact = ((double)x - x1) / (x2 - x1);
                    int y = (int)Math.Round(y1 + ((y2 - y1) * fact));
                    if (y >= 0)
                    {
                        int tilex = (int)Math.Floor(x / 32d);
                        int tiley = (int)Math.Floor(y / 32d);
                        if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                    }
                }
                int sy = y1 > y2 ? y2 : y1;
                for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                {
                    double fact = ((double)y - y1) / (y2 - y1);
                    int x = (int)Math.Round(x1 + ((x2 - x1) * fact));
                    if (x >= 0)
                    {
                        int tilex = (int)Math.Floor(x / 32d);
                        int tiley = (int)Math.Floor(y / 32d);
                        if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                    }
                }
            }
            else // Just one singular tile
            {
                TempCoords.Add(new Point((int)Math.Floor(newx / 32d), (int)Math.Floor(newy / 32d)));
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
                    int actualy = MapTileY + (int)Math.Floor((double)j / (MapViewer.CursorWidth + 1));
                    int MapPosition = actualy * MapData.Width + actualx;
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
                    int sely = (int)Math.Floor((double)j / (MapViewer.CursorWidth + 1));
                    if (OriginDiffY < 0) sely -= OriginDiffY;
                    if (OriginDiffY > 0) sely -= OriginDiffY;
                    if (sely < 0) sely += MapViewer.CursorHeight + 1;
                    sely %= MapViewer.CursorHeight + 1;

                    TileData tiledata = MapViewer.TileDataList[sely * (MapViewer.CursorWidth + 1) + selx];
                    TileType tiletype = TileType.Tileset;
                    int tileid = -1;
                    int tilesetindex = -1;
                    int tilesetx = -1;
                    int tilesety = -1;
                    if (tiledata != null)
                    {
                        tiletype = tiledata.TileType;
                        tileid = tiledata.ID;
                        tilesetindex = tiledata.Index;
                        tilesetx = tileid % 8;
                        tilesety = (int) Math.Floor(tileid / 8d);
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
                            Index = tilesetindex,
                            ID = tileid
                        };
                    }

                    if (olddata != MapData.Layers[layer].Tiles[MapPosition])
                    {
                        if (tiletype == TileType.Tileset)
                        {
                            Editor.UnsavedChanges = true;
                            this.Sprites[layer.ToString()].Bitmap.FillRect(actualx * 32, actualy * 32, 32, 32, Color.ALPHA);
                            if (!Blank)
                            {
                                this.Sprites[layer.ToString()].Bitmap.Build(
                                    actualx * 32, actualy * 32,
                                    Data.Tilesets[MapData.TilesetIDs[tilesetindex]].TilesetBitmap,
                                    new Rect(tilesetx * 32, tilesety * 32, 32, 32)
                                );
                            }
                        }
                        else
                        {
                            throw new Exception("Cannot draw autotiles yet.");
                        }
                    }
                }
            }

            this.Sprites[layer.ToString()].Bitmap.Lock();
        }
    }
}
