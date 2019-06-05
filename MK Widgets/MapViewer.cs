using System;
using System.Collections.Generic;
using ODL;
namespace MKEditor.Widgets
{
    public class MapViewer : Widget
    {
        public MKD.Map Map;

        Bitmap MapBmp;
        Bitmap CursorBmp;
        Point CursorPos = new Point(0, 0);

        public MapViewer(object Parent, string Name = "mapViewer")
            : base(Parent, Name)
        {
            this.Sprites["map"] = new Sprite(this.Viewport);
            this.Sprites["cursor"] = new Sprite(this.Viewport);
            this.CreateCursorBitmap();
            this.Sprites["cursor"].Bitmap = this.CursorBmp;
            this.WidgetIM.OnMouseMoving += this.MouseMoving;
        }

        public void SetMap(MKD.Map Map)
        {
            this.Map = Map;
            this.CreateMapBitmap();
        }

        protected override void Draw()
        {
            if (this.MapBmp != null)
            {
                this.Sprites["map"].Bitmap = this.MapBmp;
            }
            base.Draw();
        }

        public void CreateMapBitmap()
        {
            if (MapBmp != null) MapBmp.Dispose();
            MapBmp = new Bitmap(this.Map.Width * 32, this.Map.Height * 32);
            MapBmp.Unlock();
            MKD.Tileset t = MKD.Tileset.GetTileset();
            Bitmap tbmp = new Bitmap(t.GraphicName);
            for (int y = 0; y < this.Map.Height; y++)
            {
                for (int x = 0; x < this.Map.Width; x++)
                {
                    int mapx = x * 32;
                    int mapy = y * 32;
                    int tile_id = this.Map.Tiles[y * this.Map.Width + x].TileID;
                    int tilesetx = tile_id % 8;
                    int tilesety = (int) Math.Floor(tile_id / 8d);
                    MapBmp.Build(new Rect(mapx, mapy, 32, 32), tbmp, new Rect(tilesetx, tilesety, 32, 32));
                }
            }
            MapBmp.Lock();
            this.Redraw();
        }

        public void CreateCursorBitmap()
        {
            if (this.CursorBmp != null) this.CursorBmp.Dispose();
            this.CursorBmp = new Bitmap(32, 32);
            this.CursorBmp.Unlock();
            for (int x = 0; x < 32; x++)
            {
                Color c1 = x % 2 == 0 ? Color.WHITE : Color.BLACK;
                Color c2 = x % 2 == 1 ? Color.WHITE : Color.BLACK;
                this.CursorBmp.SetPixel(x, 0, c1);
                this.CursorBmp.SetPixel(x, 31, c2);
            }
            for (int y = 0; y < 32; y++)
            {
                Color c1 = y % 2 == 0 ? Color.WHITE : Color.BLACK;
                Color c2 = y % 2 == 1 ? Color.WHITE : Color.BLACK;
                this.CursorBmp.SetPixel(0, y, c1);
                this.CursorBmp.SetPixel(31, y, c2);
            }
            this.CursorBmp.Lock();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            if (rx < 0 || ry < 0 || rx >= this.Viewport.Width || ry >= this.Viewport.Height) return; // Off the widget
            int tilex = (int) Math.Floor(rx / 32d);
            int tiley = (int) Math.Floor(ry / 32d);
            if (tilex >= this.Map.Width || tilex < 0 || tiley >= this.Map.Height || tiley < 0) return;
            int cx = tilex * 32;
            int cy = tiley * 32;
            if (this.Sprites["cursor"].X != cx || this.Sprites["cursor"].Y != cy) // Tile changed
            {
                this.Sprites["cursor"].X = cx;
                this.Sprites["cursor"].Y = cy;
            }
        }
    }
}
