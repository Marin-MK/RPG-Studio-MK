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

        CursorWidget Cursor;

        public MapViewer(object Parent, string Name = "mapViewer")
            : base(Parent, Name)
        {
            Cursor = new CursorWidget(this);
            Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(this.Size, Color.BLACK));
            this.WidgetIM.OnMouseMoving += this.MouseMoving;
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
            base.MouseMoving(sender, e);
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
            if (Cursor.Position.X != cx || Cursor.Position.Y != cy) // Tile changed
            {
                Cursor.SetPosition(cx, cy);
            }
        }
    }
}
