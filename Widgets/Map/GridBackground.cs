using System;
using ODL;

namespace MKEditor.Widgets
{
    public class GridBackground : Widget
    {
        public int TileSize = 32;
        public int OffsetX = 0;
        public int OffsetY = 0;
        public string Border;

        SelectionBackground SelBG;

        public GridBackground(object Parent, string Name = "gridBackground")
            : base(Parent, Name)
        {
            Sprites["vert"] = new Sprite(this.Viewport);
            Sprites["hor"] = new Sprite(this.Viewport);
            SelBG = new SelectionBackground(this);
            SelBG.SetSideVisible("top", false);
            SelBG.SetSideVisible("left", false);
            SelBG.SetSideVisible("right", false);
            SelBG.SetSideVisible("bottom", false);
            RedrawGrid();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            SelBG.SetSize(this.Size);
            RedrawGrid();
        }

        public void SetOffset(int x, int y)
        {
            if (this.OffsetX != x || this.OffsetY != y)
            {
                this.OffsetX = x;
                this.OffsetY = y;
                RedrawGrid();
            }
        }

        public void SetTileSize(int TileSize)
        {
            if (this.TileSize != TileSize)
            {
                this.TileSize = TileSize;
                RedrawGrid();
            }
        }

        public void SetBorder(string Border)
        {
            if (this.Border != Border)
            {
                if (Border == ":north") SelBG.SetSideVisible("bottom", true);
                else if (Border == ":east") SelBG.SetSideVisible("left", true);
                else if (Border == ":south") SelBG.SetSideVisible("top", true);
                else if (Border == ":west") SelBG.SetSideVisible("right", true);
            }
        }

        public void RedrawGrid()
        {
            if (Sprites["hor"].Bitmap != null) Sprites["hor"].Bitmap.Dispose();
            Sprites["hor"].Bitmap = new Bitmap(this.Size.Width, 1);
            Sprites["hor"].Bitmap.Unlock();
            Sprites["hor"].MultiplePositions.Clear();
            if (Sprites["vert"].Bitmap != null) Sprites["vert"].Bitmap.Dispose();
            Sprites["vert"].Bitmap = new Bitmap(1, this.Size.Height);
            Sprites["vert"].Bitmap.Unlock();
            Sprites["vert"].MultiplePositions.Clear();

            Color c = new Color(0, 0, 0, 160);

            for (int x = 0; x < Size.Width; x++)
            {
                int ax = x - OffsetX;
                if (ax % 2 == 0)
                    Sprites["hor"].Bitmap.SetPixel(x, 0, c);
                if (ax > 0 && ax % TileSize == 0)
                    Sprites["vert"].MultiplePositions.Add(new Point(x, 0));
            }

            for (int y = 0; y < Size.Height; y++)
            {
                int ay = y - OffsetY;
                if (ay % 2 == 0)
                    Sprites["vert"].Bitmap.SetPixel(0, y, c);
                if (ay > 0 && ay % TileSize == 0)
                    Sprites["hor"].MultiplePositions.Add(new Point(0, y));
            }

            Sprites["hor"].Bitmap.Lock();
            Sprites["vert"].Bitmap.Lock();
        }
    }
}
