using System;
using ODL;

namespace MKEditor.Widgets
{
    public class GridBackground : Widget
    {
        public int TileSize = 32;
        public int OffsetX = 0;
        public int OffsetY = 0;

        public GridBackground(object Parent, string Name = "gridBackground")
            : base(Parent, Name)
        {
            Sprites["grid"] = new Sprite(this.Viewport);
            Redraw();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            Redraw();
        }

        public void SetOffset(int x, int y)
        {
            if (this.OffsetX != x || this.OffsetY != y)
            {
                this.OffsetX = x;
                this.OffsetY = y;
                Redraw();
            }
        }

        public void SetTileSize(int TileSize)
        {
            if (this.TileSize != TileSize)
            {
                this.TileSize = TileSize;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["grid"].Bitmap != null) Sprites["grid"].Bitmap.Dispose();
            Sprites["grid"].Bitmap = new Bitmap(this.Size);
            Sprites["grid"].Bitmap.Unlock();
            Color c = new Color(0, 0, 0, 160);
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    x -= OffsetX;
                    y -= OffsetX;
                    if (x > 0 && x % TileSize == 0 && y % 2 == 0 ||
                        y > 0 && y % TileSize == 0 && x % 2 == 0)
                    {
                        Sprites["grid"].Bitmap.SetPixel(x, y, c);
                    }
                }
            }
            Sprites["grid"].Bitmap.Lock();
            base.Draw();
        }
    }
}
