using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewer : Widget
    {
        public MKD.Map Map;
        public TilesetTab TilesetTab;
        public LayersTab LayersTab;

        public int RelativeMouseX = 0;
        public int RelativeMouseY = 0;
        public int MapTileX = 0;
        public int MapTileY = 0;

        MKD.Tileset MTileset;

        CursorWidget Cursor;

        public MapViewer(object Parent, string Name = "mapViewer")
            : base(Parent, Name)
        {
            Cursor = new CursorWidget(this);
            Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(this.Size, Color.BLACK));
            this.WidgetIM.OnMouseMoving += this.MouseMoving;
            this.WidgetIM.OnMouseDown += this.MouseDown;
            this.OnWidgetSelect += WidgetSelect;
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            Sprites["bg"].Bitmap.Unlock();
            (Sprites["bg"].Bitmap as SolidBitmap).SetSize(this.Size.Width - 14, this.Size.Height - 14);
            Sprites["bg"].Bitmap.Lock();
        }

        public void SetMap(MKD.Map Map)
        {
            this.Map = Map;
            MTileset = MKD.Tileset.GetTileset();
            this.CreateLayerBitmaps();
        }

        public void CreateLayerBitmaps()
        {
            foreach (string s in this.Sprites.Keys)
            {
                if (s != "bg") this.Sprites[s].Dispose();
            }
            int layercount = this.Map.Layers.Count;
            for (int i = 0; i < layercount; i++)
            {
                this.Sprites[i.ToString()] = new Sprite(this.Viewport, this.Map.Width * 32, this.Map.Height * 32);
                this.Sprites[i.ToString()].Bitmap.Unlock();
            }
            MKD.Tileset t = MKD.Tileset.GetTileset();
            Bitmap tbmp = new Bitmap(t.GraphicName);
            for (int layer = 0; layer < this.Map.Layers.Count; layer++)
            {
                Bitmap layerbmp = this.Sprites[layer.ToString()].Bitmap as Bitmap;
                for (int y = 0; y < this.Map.Height; y++)
                {
                    for (int x = 0; x < this.Map.Width; x++)
                    {
                        if (this.Map.Layers[layer] == null || this.Map.Layers[layer].Tiles == null ||
                            y * this.Map.Width + x >= this.Map.Layers[layer].Tiles.Count ||
                            this.Map.Layers[layer].Tiles[y * this.Map.Width + x] == null) continue;
                        int tileset_index = this.Map.Layers[layer].Tiles[y * this.Map.Width + x].TilesetIndex;
                        int tileset_id = this.Map.Tilesets[tileset_index];
                        int mapx = x * 32;
                        int mapy = y * 32;
                        int tile_id = this.Map.Layers[layer].Tiles[y * this.Map.Width + x].TileID;
                        int tilesetx = tile_id % 8;
                        int tilesety = (int) Math.Floor(tile_id / 8d);
                        layerbmp.Build(new Rect(mapx, mapy, 32, 32), tbmp, new Rect(tilesetx * 32, tilesety * 32, 32, 32));
                    }
                }
            }
            for (int i = 0; i < layercount; i++)
            {
                this.Sprites[i.ToString()].Bitmap.Lock();
            }
            this.SetSize(this.Map.Width * 32, this.Map.Height * 32);
            this.LayersTab.CreateLayers();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            // Cursor placement
            int oldmousex = RelativeMouseX;
            int oldmousey = RelativeMouseY;
            if (Parent.ScrollBarX != null && (Parent.ScrollBarX.Dragging || Parent.ScrollBarX.Hovering)) return;
            if (Parent.ScrollBarY != null && (Parent.ScrollBarY.Dragging || Parent.ScrollBarY.Hovering)) return;
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            if (rx < 0 || ry < 0 || rx >= this.Viewport.Width || ry >= this.Viewport.Height) return; // Off the widget
            int movedx = this.Position.X - this.ScrolledPosition.X;
            int movedy = this.Position.Y - this.ScrolledPosition.Y;
            rx += movedx;
            ry += movedy;
            int tilex = (int) Math.Floor(rx / 32d);
            int tiley = (int) Math.Floor(ry / 32d);
            if (tilex >= this.Map.Width || tilex < 0 || tiley >= this.Map.Height || tiley < 0) return;
            int cx = tilex * 32 + this.ScrolledX;
            int cy = tiley * 32 + this.ScrolledY;
            RelativeMouseX = cx;
            RelativeMouseY = cy;
            if (Cursor.Position.X != cx || Cursor.Position.Y != cy) // Tile changed
            {
                Cursor.SetPosition(cx, cy);
                MapTileX = tilex;
                MapTileY = tiley;
            }
            UpdateTilePlacement(oldmousex, oldmousey, RelativeMouseX, RelativeMouseY);
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            UpdateTilePlacement(RelativeMouseX, RelativeMouseY, RelativeMouseX, RelativeMouseY);
        }

        public void UpdateTilePlacement(int oldx = -1, int oldy = -1, int newx = -1, int newy = -1)
        {
            if (Parent.ScrollBarX != null && (Parent.ScrollBarX.Dragging || Parent.ScrollBarX.Hovering)) return;
            if (Parent.ScrollBarY != null && (Parent.ScrollBarY.Dragging || Parent.ScrollBarY.Hovering)) return;
            bool line = !(oldx == newx && oldy == newy);
            // Input handling
            if (WidgetIM.ClickedLeftInArea == true)
            {
                int Layer = this.LayersTab.SelectedLayer;
                int TileID = this.TilesetTab.TileY * 8 + this.TilesetTab.TileX;

                this.Sprites[Layer.ToString()].Bitmap.Unlock();

                List<Point> TempCoords = new List<Point>();
                if (line)
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
                else
                {
                    TempCoords.Add(new Point((int) Math.Floor(newx / 32d), (int) Math.Floor(newy / 32d)));
                }

                for (int i = 0; i < TempCoords.Count; i++)
                {
                    int MapTileX = TempCoords[i].X;
                    int MapTileY = TempCoords[i].Y;
                    int MapTileIndex = MapTileY * this.Map.Width + MapTileX;
                    this.Map.Layers[Layer].Tiles[MapTileIndex] = new MKD.TileData
                    {
                        TilesetIndex = this.TilesetTab.TilesetIndex,
                        TileID = TileID
                    };

                    this.Sprites[Layer.ToString()].Bitmap.Build(
                        MapTileX * 32, MapTileY * 32,
                        MTileset.ResultBitmap,
                        new Rect(this.TilesetTab.TileX * 33, this.TilesetTab.TileY * 33, 32, 32)
                    );
                }

                this.Sprites[Layer.ToString()].Bitmap.Lock();
            }
        }
    }
}
