﻿using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MapImageWidget : Widget
{
    public int MapID;
    public int RelativeX;
    public int RelativeY;

    public GridBackground GridBackground;
    public MapViewer MapViewer;
    public Map MapData;
    public Rect Rect;

    public int AnimateCount = 0;
    public List<List<int>> AnimatedAutotiles = new List<List<int>>();

    public double ZoomFactor = 1.0;

    public bool GridVisibility { get; protected set; } = false;
    public bool ShowMapAnimations { get; protected set; } = false;

    public MapImageWidget(IContainer Parent) : base(Parent)
    {
        SetBackgroundColor(73, 89, 109);
        this.GridBackground = new GridBackground(this);
        this.GridBackground.SetDocked(true);
        this.GridBackground.SetVisible(false);
        Sprites["dark"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(0, 0, 0, 0)));
        Sprites["dark"].Z = 999999;
        SetTimer("frame", (long)Math.Round(1000 / 60d)); // 60 FPS
    }

    public void SetZoomFactor(double factor)
    {
        if (MapData == null) return;
        for (int i = 0; i < MapData.Layers.Count; i++)
        {
            Sprites[i.ToString()].ZoomX = factor;
            Sprites[i.ToString()].ZoomY = factor;
        }
        this.ZoomFactor = factor;
        GridBackground.SetTileSize((int)Math.Round(32 * this.ZoomFactor));
        UpdateSize();
    }

    public void SetDarkOverlay(byte Opacity)
    {
        (Sprites["dark"].Bitmap as SolidBitmap).SetColor(0, 0, 0, Opacity);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        (Sprites["dark"].Bitmap as SolidBitmap).SetSize(this.Size);
    }

    public void SetLayerVisible(int layerindex, bool Visible)
    {
        MapData.Layers[layerindex].Visible = Visible;
        Sprites[layerindex.ToString()].Visible = Visible;
    }

    public override void Update()
    {
        base.Update();
        if (!Mouse.Accessible) return;
        if (!IsVisible()) return;
        if (TimerPassed("frame"))
        {
            ResetTimer("frame");
            if (!ShowMapAnimations) return;
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
                    DrawAutotile(data[0], data[1], data[2], data[3], data[4], (int)Math.Floor((double)AnimateCount / Data.Autotiles[data[3]].AnimateSpeed));
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
                    if (tilesetimage == null) continue;
                    int tilesetx = tile_id % 8;
                    int tilesety = (int)Math.Floor(tile_id / 8d);
                    if (tilesety * 32 + 32 <= tilesetimage.Height)
                        bmp.Build(new Rect(mapx, mapy, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32), BlendMode.None);
                }
                else if (MapData.Layers[Layer].Tiles[y * MapData.Width + x].TileType == TileType.Autotile)
                {
                    int autotile_index = MapData.Layers[Layer].Tiles[y * MapData.Width + x].Index;
                    int autotile_id = MapData.AutotileIDs[autotile_index];
                    Autotile autotile = Data.Autotiles[autotile_id];
                    if (autotile == null) continue;
                    if (autotile.AnimateSpeed > 0) AnimatedAutotiles.Add(new List<int>() { Layer, x, y, autotile_id, tile_id });
                    Bitmap autotileimage = autotile.AutotileBitmap;
                    if (autotileimage == null) continue;
                    if (autotile.Format == AutotileFormat.Single)
                    {
                        int AnimX = 0;
                        bmp.Build(new Rect(mapx, mapy, 32, 32), autotileimage, new Rect(AnimX, 0, 32, 32), BlendMode.None);
                    }
                    else
                    {
                        int AnimX = 0;
                        List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][tile_id];
                        for (int i = 0; i < 4; i++)
                        {
                            bmp.Build(new Rect(mapx + 16 * (i % 2), mapy + 16 * (int)Math.Floor(i / 2d), 16, 16), autotileimage,
                                new Rect(16 * (Tiles[i] % 6) + AnimX, 16 * (int)Math.Floor(Tiles[i] / 6d), 16, 16), BlendMode.None);
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

    public void SetMapAnimations(bool ShowMapAnimations)
    {
        this.ShowMapAnimations = ShowMapAnimations;
        if (!ShowMapAnimations)
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

    public void SetGridVisibility(bool GridVisibility)
    {
        this.GridVisibility = GridVisibility;
        GridBackground.SetVisible(GridVisibility);
    }

    public virtual void UpdateSize()
    {
        if (MapData == null)
        {
            this.SetSize(320, 320);
            return;
        }
        int Width = (int)Math.Round(MapData.Width * 32 * ZoomFactor);
        int Height = (int)Math.Round(MapData.Height * 32 * ZoomFactor);
        this.SetSize(Width, Height);
    }

    public virtual void SetMap(Map MapData, int RelativeX = 0, int RelativeY = 0)
    {
        this.MapData = MapData;
        this.MapID = MapData?.ID ?? -1;
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
        if (MapData == null) return;
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
        Sprite s = this.Sprites[LayerIndex.ToString()];
        if (Locked) s.Bitmap.Lock();
        else s.Bitmap.Unlock();
    }

    public List<Point> GetTilesFromMouse(int oldx, int oldy, int newx, int newy, int layer)
    {
        if (this.MapData == null) return new List<Point>();
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
        int x = (int)Math.Floor(newx / 32d);
        int y = (int)Math.Floor(newy / 32d);
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
        // Avoid drawing a line from top left to current tile
        if (oldx == -1 || oldy == -1)
        {
            oldx = newx;
            oldy = newy;
        }
        if (oldx == newx && oldy == newy) // Don't bother with line calculations: it's only one single tile.
        {
            Coords.Add(new Point((int)Math.Floor(newx / 32d), (int)Math.Floor(newy / 32d)));
        }
        else
        {
            // Interpolates the new mouse position over the old position and adds all tiles
            // in the line so as to not skip any tiles in between the old and new mouse position.
            int x1 = oldx;
            int y1 = oldy;
            int x2 = newx;
            int y2 = newy;
            if (x1 != x2)
            {
                for (int x = x1 > x2 ? x2 : x1; (x1 > x2) ? (x <= x1) : (x <= x2); x++)
                {
                    double fact = ((double)x - x1) / (x2 - x1);
                    int y = (int)Math.Round(y1 + ((y2 - y1) * fact));
                    int tilex = (int)Math.Floor(x / 32d);
                    int tiley = (int)Math.Floor(y / 32d);
                    if (!Coords.Exists(c => c.X == tilex && c.Y == tiley)) Coords.Add(new Point(tilex, tiley));
                }
            }
            if (y1 != y2)
            {
                for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                {
                    double fact = ((double)y - y1) / (y2 - y1);
                    int x = (int)Math.Round(x1 + ((x2 - x1) * fact));
                    int tilex = (int)Math.Floor(x / 32d);
                    int tiley = (int)Math.Floor(y / 32d);
                    if (!Coords.Exists(c => c.X == tilex && c.Y == tiley)) Coords.Add(new Point(tilex, tiley));
                }
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
        int x1 = MapViewer.OriginPoint.X * 32;
        int y1 = MapViewer.OriginPoint.Y * 32;
        int x2 = newx;
        int y2 = newy;
        if (Input.Press(Keycode.SHIFT))
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
        int ox2 = (int)Math.Round(newx / 32d);
        int oy2 = (int)Math.Round(newy / 32d);
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
            int ry = (int)Math.Round(b / a * Math.Sqrt(a * a - x * x));
            int tilex1 = (int)Math.Round((cx + x) / 32d);
            int tilex2 = (int)Math.Round((cx - x) / 32d);
            int tiley1 = (int)Math.Round((cy + ry) / 32d);
            int tiley2 = (int)Math.Round((cy - ry) / 32d);
            if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley1)) Coords.Add(new Point(tilex1, tiley1));
            if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley1)) Coords.Add(new Point(tilex2, tiley1));
            if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley2)) Coords.Add(new Point(tilex1, tiley2));
            if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley2)) Coords.Add(new Point(tilex2, tiley2));
        }
        List<Point> fillpoints = new List<Point>();
        int sx = (int)Math.Round((x1 > x2 ? x2 : x1) / 32d);
        int ex = (int)Math.Round((x1 > x2 ? x1 : x2) / 32d);
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
        int x1 = MapViewer.OriginPoint.X * 32;
        int y1 = MapViewer.OriginPoint.Y * 32;
        int x2 = newx;
        int y2 = newy;
        if (Input.Press(Keycode.SHIFT))
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
        int ox2 = (int)Math.Round(newx / 32d);
        int oy2 = (int)Math.Round(newy / 32d);
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
            int ry = (int)Math.Round(b / a * Math.Sqrt(a * a - x * x));
            int tilex1 = (int)Math.Round((cx + x) / 32d);
            int tilex2 = (int)Math.Round((cx - x) / 32d);
            int tiley1 = (int)Math.Round((cy + ry) / 32d);
            int tiley2 = (int)Math.Round((cy - ry) / 32d);
            if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley1)) Coords.Add(new Point(tilex1, tiley1));
            if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley1)) Coords.Add(new Point(tilex2, tiley1));
            if (!Coords.Exists(c => c.X == tilex1 && c.Y == tiley2)) Coords.Add(new Point(tilex1, tiley2));
            if (!Coords.Exists(c => c.X == tilex2 && c.Y == tiley2)) Coords.Add(new Point(tilex2, tiley2));
        }
        for (int y = 0; b > 0 ? y <= b : y >= b; y += (b > 0 ? 1 : -1))
        {
            int rx = (int)Math.Round(a / b * Math.Sqrt(b * b - y * y));
            int tilex1 = (int)Math.Round((cx + rx) / 32d);
            int tilex2 = (int)Math.Round((cx - rx) / 32d);
            int tiley1 = (int)Math.Round((cy + y) / 32d);
            int tiley2 = (int)Math.Round((cy - y) / 32d);
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
        int x1 = MapViewer.OriginPoint.X;
        int y1 = MapViewer.OriginPoint.Y;
        int x2 = (int)Math.Floor(newx / 32d);
        int y2 = (int)Math.Floor(newy / 32d);
        if (Input.Press(Keycode.SHIFT))
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
        int x1 = MapViewer.OriginPoint.X;
        int y1 = MapViewer.OriginPoint.Y;
        int x2 = (int)Math.Floor(newx / 32d);
        int y2 = (int)Math.Floor(newy / 32d);
        if (Input.Press(Keycode.SHIFT))
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

    public void DrawTiles(List<Point> Coords, int layer, Point OriginPoint)
    {
        SetLayerLocked(layer, false);
        for (int i = 0; i < Coords.Count; i++)
        {
            int MapTileX = Coords[i].X;
            int MapTileY = Coords[i].Y;
            if (MapTileX < 0 || MapTileX >= MapData.Width || MapTileY < 0 || MapTileY >= MapData.Height) continue;
            int OriginX = OriginPoint.X;
            int OriginY = OriginPoint.Y;
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

            TileData tiledata = Blank ? null : MapViewer.TileDataList[sely * (MapViewer.CursorWidth + 1) + selx];
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
            else if (OldTile != null && OldTile.ID != NewTile.ID) SameTile = false;

            if (!SameTile)
            {
                MapData.Layers[layer].Tiles[MapPosition] = NewTile;
                Editor.UnsavedChanges = true;
                if (TileGroupUndoAction.GetLatest() == null || TileGroupUndoAction.GetLatest().Ready)
                {
                    Editor.CanUndo = false;
                    TileGroupUndoAction.Create(MapID);
                }
                TileGroupUndoAction.AddToLatest(MapPosition, layer, NewTile, OldTile);
                DrawTile(MapTileX, MapTileY, layer, NewTile, OldTile, !Input.Press(Keycode.SHIFT));
            }
        }
        SetLayerLocked(layer, true);
    }

    public void DrawTile(int X, int Y, int Layer, TileData Tile, TileData OldTile, bool ForceUpdateNearbyAutotiles = false, bool MakeNeighboursUndoable = true)
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

        List<TileData> OldSurrounding = new List<TileData>(new TileData[8]);
        List<Point> Connected = GetConnectedTiles(X, Y);
        int id0 = Connected[0].X + Connected[0].Y * MapData.Width;
        int id1 = Connected[1].X + Connected[1].Y * MapData.Width;
        int id2 = Connected[2].X + Connected[2].Y * MapData.Width;
        int id3 = Connected[3].X + Connected[3].Y * MapData.Width;
        int id4 = Connected[4].X + Connected[4].Y * MapData.Width;
        int id5 = Connected[5].X + Connected[5].Y * MapData.Width;
        int id6 = Connected[6].X + Connected[6].Y * MapData.Width;
        int id7 = Connected[7].X + Connected[7].Y * MapData.Width;
        if (MakeNeighboursUndoable)
        {
            if (X > 0 && Y > 0) OldSurrounding[0] = (TileData)MapData.Layers[Layer].Tiles[id0]?.Clone();
            if (Y > 0) OldSurrounding[1] = (TileData)MapData.Layers[Layer].Tiles[id1]?.Clone();
            if (X < MapData.Width - 1 && Y > 0) OldSurrounding[2] = (TileData)MapData.Layers[Layer].Tiles[id2]?.Clone();
            if (X > 0) OldSurrounding[3] = (TileData)MapData.Layers[Layer].Tiles[id3]?.Clone();
            if (X < MapData.Width - 1) OldSurrounding[4] = (TileData)MapData.Layers[Layer].Tiles[id4]?.Clone();
            if (X > 0 && Y < MapData.Height - 1) OldSurrounding[5] = (TileData)MapData.Layers[Layer].Tiles[id5]?.Clone();
            if (Y < MapData.Height - 1) OldSurrounding[6] = (TileData)MapData.Layers[Layer].Tiles[id6]?.Clone();
            if (X < MapData.Width - 1 && Y < MapData.Height - 1) OldSurrounding[7] = (TileData)MapData.Layers[Layer].Tiles[id7]?.Clone();
        }

        if (OldTile != null && OldTile.TileType == TileType.Autotile && ForceUpdateNearbyAutotiles)
        {
            UpdateAutotiles(Layer, X, Y, OldTile.Index, ForceUpdateNearbyAutotiles, true);
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
                Bitmap bmp = Data.Tilesets[MapData.TilesetIDs[Tile.Index]].TilesetBitmap;
                if (bmp != null)
                    this.Sprites[Layer.ToString()].Bitmap.Build(
                        X * 32, Y * 32,
                        bmp,
                        new Rect(32 * (Tile.ID % 8), 32 * (int)Math.Floor(Tile.ID / 8d), 32, 32),
                        BlendMode.None
                    );
            }
            else if (Tile.TileType == TileType.Autotile)
            {
                if (Tile.ID != -1 && !ForceUpdateNearbyAutotiles) // Only draws
                {
                    AnimatedAutotiles.Add(new List<int>() { Layer, X, Y, MapData.AutotileIDs[Tile.Index], Tile.ID });
                    Autotile a = Data.Autotiles[MapData.AutotileIDs[Tile.Index]];
                    int frame = a != null && ShowMapAnimations ? (int)Math.Floor((double)AnimateCount / a.AnimateSpeed) : 0;
                    DrawAutotile(Layer, X, Y, MapData.AutotileIDs[Tile.Index], Tile.ID, frame);
                }
                else // Draws and updates
                {
                    UpdateAutotiles(Layer, X, Y, Tile.Index, ForceUpdateNearbyAutotiles, false);
                }
            }
        }

        if (MakeNeighboursUndoable)
        {
            if (X > 0 && Y > 0 && MapData.Layers[Layer].Tiles[id0]?.Equals(OldSurrounding[0]) != true) TileGroupUndoAction.AddToLatest(id0, Layer, MapData.Layers[Layer].Tiles[id0], OldSurrounding[0], true);
            if (Y > 0 && MapData.Layers[Layer].Tiles[id1]?.Equals(OldSurrounding[1]) != true) TileGroupUndoAction.AddToLatest(id1, Layer, MapData.Layers[Layer].Tiles[id1], OldSurrounding[1], true);
            if (X < MapData.Width - 1 && Y > 0 && MapData.Layers[Layer].Tiles[id2]?.Equals(OldSurrounding[2]) != true) TileGroupUndoAction.AddToLatest(id2, Layer, MapData.Layers[Layer].Tiles[id2], OldSurrounding[2], true);
            if (X > 0 && MapData.Layers[Layer].Tiles[id3]?.Equals(OldSurrounding[3]) != true) TileGroupUndoAction.AddToLatest(id3, Layer, MapData.Layers[Layer].Tiles[id3], OldSurrounding[3], true);
            if (X < MapData.Width - 1 && MapData.Layers[Layer].Tiles[id4]?.Equals(OldSurrounding[4]) != true) TileGroupUndoAction.AddToLatest(id4, Layer, MapData.Layers[Layer].Tiles[id4], OldSurrounding[4], true);
            if (X > 0 && Y < MapData.Height - 1 && MapData.Layers[Layer].Tiles[id5]?.Equals(OldSurrounding[5]) != true) TileGroupUndoAction.AddToLatest(id5, Layer, MapData.Layers[Layer].Tiles[id5], OldSurrounding[5], true);
            if (Y < MapData.Height - 1 && MapData.Layers[Layer].Tiles[id6]?.Equals(OldSurrounding[6]) != true) TileGroupUndoAction.AddToLatest(id6, Layer, MapData.Layers[Layer].Tiles[id6], OldSurrounding[6], true);
            if (X < MapData.Width - 1 && Y < MapData.Height - 1 && MapData.Layers[Layer].Tiles[id7]?.Equals(OldSurrounding[7]) != true) TileGroupUndoAction.AddToLatest(id7, Layer, MapData.Layers[Layer].Tiles[id7], OldSurrounding[7], true);
        }
    }

    public void DrawAutotile(int Layer, int X, int Y, int AutotileID, int TileID, int Frame)
    {
        Autotile autotile = Data.Autotiles[AutotileID];
        if (autotile.AutotileBitmap == null) return;
        int AnimX = 0;
        if (autotile.Format == AutotileFormat.Single)
        {
            AnimX = (Frame * 32) % autotile.AutotileBitmap.Width;
            this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X, 32 * Y, 32, 32), autotile.AutotileBitmap, new Rect(AnimX, 0, 32, 32), BlendMode.None);
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
                        32 * Y + 16 * (int)Math.Floor(i / 2d),
                        16,
                        16
                    ),
                    autotile.AutotileBitmap,
                    new Rect(
                        16 * (Tiles[i] % 6) + AnimX,
                        16 * (int)Math.Floor(Tiles[i] / 6d),
                        16,
                        16
                    ),
                    BlendMode.None
                );
            }
        }
    }

    public void UpdateAutotiles(int Layer, int X, int Y, int AutotileIndex, bool CheckNeighbouring = false, bool DeleteTile = false)
    {
        TileData TileData = MapData.Layers[Layer].Tiles[X + Y * MapData.Width];
        List<Point> Connected = GetConnectedTiles(X, Y);
        int id0 = Connected[0].X + Connected[0].Y * MapData.Width;
        int id1 = Connected[1].X + Connected[1].Y * MapData.Width;
        int id2 = Connected[2].X + Connected[2].Y * MapData.Width;
        int id3 = Connected[3].X + Connected[3].Y * MapData.Width;
        int id4 = Connected[4].X + Connected[4].Y * MapData.Width;
        int id5 = Connected[5].X + Connected[5].Y * MapData.Width;
        int id6 = Connected[6].X + Connected[6].Y * MapData.Width;
        int id7 = Connected[7].X + Connected[7].Y * MapData.Width;
        bool NWauto = Connected[0].X >= 0 && Connected[0].X < MapData.Width &&
                      Connected[0].Y >= 0 && Connected[0].Y < MapData.Height &&
                      MapData.Layers[Layer].Tiles[id0] != null &&
                      MapData.Layers[Layer].Tiles[id0].TileType == TileType.Autotile;
        bool NWex = NWauto && MapData.Layers[Layer].Tiles[id0].Index == AutotileIndex;
        bool NW = NWauto && (NWex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id0].Index]) == true);
        bool Nauto = Connected[1].X >= 0 && Connected[1].X < MapData.Width &&
                     Connected[1].Y >= 0 && Connected[1].Y < MapData.Height &&
                     MapData.Layers[Layer].Tiles[id1] != null &&
                     MapData.Layers[Layer].Tiles[id1].TileType == TileType.Autotile;
        bool Nex = Nauto && MapData.Layers[Layer].Tiles[id1].Index == AutotileIndex;
        bool N = Nauto && (Nex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id1].Index]) == true);
        bool NEauto = Connected[2].X >= 0 && Connected[2].X < MapData.Width &&
                      Connected[2].Y >= 0 && Connected[2].Y < MapData.Height &&
                      MapData.Layers[Layer].Tiles[id2] != null &&
                      MapData.Layers[Layer].Tiles[id2].TileType == TileType.Autotile;
        bool NEex = NEauto && MapData.Layers[Layer].Tiles[id2].Index == AutotileIndex;
        bool NE = NEauto && (NEex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id2].Index]) == true);
        bool Wauto = Connected[3].X >= 0 && Connected[3].X < MapData.Width &&
                     Connected[3].Y >= 0 && Connected[3].Y < MapData.Height &&
                     MapData.Layers[Layer].Tiles[id3] != null &&
                     MapData.Layers[Layer].Tiles[id3].TileType == TileType.Autotile;
        bool Wex = Wauto && MapData.Layers[Layer].Tiles[id3].Index == AutotileIndex;
        bool W = Wauto && (Wex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id3].Index]) == true);
        bool Eauto = Connected[4].X >= 0 && Connected[4].X < MapData.Width &&
                     Connected[4].Y >= 0 && Connected[4].Y < MapData.Height &&
                     MapData.Layers[Layer].Tiles[id4] != null &&
                     MapData.Layers[Layer].Tiles[id4].TileType == TileType.Autotile;
        bool Eex = Eauto && MapData.Layers[Layer].Tiles[id4].Index == AutotileIndex;
        bool E = Eauto && (Eex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id4].Index]) == true);
        bool SWauto = Connected[5].X >= 0 && Connected[5].X < MapData.Width &&
                      Connected[5].Y >= 0 && Connected[5].Y < MapData.Height &&
                      MapData.Layers[Layer].Tiles[id5] != null &&
                      MapData.Layers[Layer].Tiles[id5].TileType == TileType.Autotile;
        bool SWex = SWauto && MapData.Layers[Layer].Tiles[id5].Index == AutotileIndex;
        bool SW = SWauto && (SWex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id5].Index]) == true);
        bool Sauto = Connected[6].X >= 0 && Connected[6].X < MapData.Width &&
                     Connected[6].Y >= 0 && Connected[6].Y < MapData.Height &&
                     MapData.Layers[Layer].Tiles[id6] != null &&
                     MapData.Layers[Layer].Tiles[id6].TileType == TileType.Autotile;
        bool Sex = Sauto && MapData.Layers[Layer].Tiles[id6].Index == AutotileIndex;
        bool S = Sauto && (Sex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id6].Index]) == true);
        bool SEauto = Connected[7].X >= 0 && Connected[7].X < MapData.Width &&
                      Connected[7].Y >= 0 && Connected[7].Y < MapData.Height &&
                      MapData.Layers[Layer].Tiles[id7] != null &&
                      MapData.Layers[Layer].Tiles[id7].TileType == TileType.Autotile;
        bool SEex = SEauto && MapData.Layers[Layer].Tiles[id7].Index == AutotileIndex;
        bool SE = SEauto && (SEex || Data.Autotiles[MapData.AutotileIDs[AutotileIndex]]?.OverlappableBy.Contains(MapData.AutotileIDs[MapData.Layers[Layer].Tiles[id7].Index]) == true);

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
                int autotileid = MapData.AutotileIDs[AutotileIndex];
                Autotile autotile = Data.Autotiles[autotileid];
                if (autotile != null)
                {
                    if (autotile.AnimateSpeed > 0)
                    {
                        AnimatedAutotiles.Add(new List<int>() { Layer, X, Y, autotileid, ID });
                    }
                    int AnimX = 0;
                    if (autotile.Format == AutotileFormat.Single)
                    {
                        AnimX = ((int)Math.Floor((double)AnimateCount / autotile.AnimateSpeed) * 32) % autotile.AutotileBitmap.Width;
                        if (!ShowMapAnimations) AnimX = 0;
                        if (autotile.AutotileBitmap != null)
                        {
                            this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X, 32 * Y, 32, 32), autotile.AutotileBitmap,
                                new Rect(AnimX, 0, 32, 32), BlendMode.None);
                        }
                    }
                    else if (autotile.AutotileBitmap != null)
                    {
                        AnimX = ((int)Math.Floor((double)AnimateCount / autotile.AnimateSpeed) * 96) % autotile.AutotileBitmap.Width;
                        if (!ShowMapAnimations) AnimX = 0;
                        List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][ID];
                        for (int i = 0; i < 4; i++)
                        {
                            this.Sprites[Layer.ToString()].Bitmap.Build(new Rect(32 * X + 16 * (i % 2), 32 * Y + 16 * (int)Math.Floor(i / 2d), 16, 16), autotile.AutotileBitmap,
                                new Rect(16 * (Tiles[i] % 6) + AnimX, 16 * (int)Math.Floor(Tiles[i] / 6d), 16, 16), BlendMode.None);
                        }
                    }
                }
            }
        }
        // Whether or not to update neighbouring tiles
        if (CheckNeighbouring)
        {
            int max = MapData.Width * MapData.Height;
            // Only update neighbours if they contain autotiles
            // (they don't need to be the same autotile; if autotile B is drawn over A, then surrounding A must also be updated)
            if (id0 >= 0 && id0 < max && MapData.Layers[Layer].Tiles[id0] != null && (NWex || Nex && Wex)) UpdateAutotiles(Layer, Connected[0].X, Connected[0].Y, MapData.Layers[Layer].Tiles[id0].Index);
            if (id1 >= 0 && id1 < max && MapData.Layers[Layer].Tiles[id1] != null && Nauto) UpdateAutotiles(Layer, Connected[1].X, Connected[1].Y, MapData.Layers[Layer].Tiles[id1].Index);
            if (id2 >= 0 && id2 < max && MapData.Layers[Layer].Tiles[id2] != null && (NEex || Nex && Eex)) UpdateAutotiles(Layer, Connected[2].X, Connected[2].Y, MapData.Layers[Layer].Tiles[id2].Index);
            if (id3 >= 0 && id3 < max && MapData.Layers[Layer].Tiles[id3] != null && Wauto) UpdateAutotiles(Layer, Connected[3].X, Connected[3].Y, MapData.Layers[Layer].Tiles[id3].Index);
            if (id4 >= 0 && id4 < max && MapData.Layers[Layer].Tiles[id4] != null && Eauto) UpdateAutotiles(Layer, Connected[4].X, Connected[4].Y, MapData.Layers[Layer].Tiles[id4].Index);
            if (id5 >= 0 && id5 < max && MapData.Layers[Layer].Tiles[id5] != null && (SWex || Sex && Wex)) UpdateAutotiles(Layer, Connected[5].X, Connected[5].Y, MapData.Layers[Layer].Tiles[id5].Index);
            if (id6 >= 0 && id6 < max && MapData.Layers[Layer].Tiles[id6] != null && Sauto) UpdateAutotiles(Layer, Connected[6].X, Connected[6].Y, MapData.Layers[Layer].Tiles[id6].Index);
            if (id7 >= 0 && id7 < max && MapData.Layers[Layer].Tiles[id7] != null && (SEex || Sex && Eex)) UpdateAutotiles(Layer, Connected[7].X, Connected[7].Y, MapData.Layers[Layer].Tiles[id7].Index);
        }
    }

    public List<Point> GetConnectedTiles(int X, int Y)
    {
        return new List<Point>()
             {
                new Point(X - 1, Y - 1), new Point(X, Y - 1), new Point(X + 1, Y - 1),
                new Point(X - 1, Y    ),                      new Point(X + 1, Y),
                new Point(X - 1, Y + 1), new Point(X, Y + 1), new Point(X + 1, Y + 1)
            };
    }
}
