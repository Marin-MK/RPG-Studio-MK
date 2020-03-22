using System;
using ODL;

namespace MKEditor.Widgets
{
    public class SelectionBackground : Widget
    {
        public int Interval = 100;
        public int Offset = 0;

        public SelectionBackground(object Parent, string Name = "selectionBackground")
            : base(Parent, Name)
        {
            Sprites["left"] = new Sprite(this.Viewport);
            Sprites["top"] = new Sprite(this.Viewport);
            Sprites["top"].X = 1;
            Sprites["right"] = new Sprite(this.Viewport);
            Sprites["bottom"] = new Sprite(this.Viewport);
            Sprites["bottom"].X = 1;
            SetTimer("offset", Interval);
        }

        public override void Update()
        {
            if (TimerPassed("offset"))
            {
                ResetTimer("offset");
                Offset += 1;
                if (Offset >= 8) Offset = 0;
                if (Visible) Redraw();
            }
            base.Update();
        }

        public void SetSideVisible(string Side, bool Visible)
        {
           //Sprites[Side].Visible = Visible;
        }

        public void SetInterval(int Interval)
        {
            if (this.Interval != Interval)
            {
                DestroyTimer("offset");
                if (Interval > 0)
                {
                    SetTimer("offset", Interval);
                }
            }
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            Sprites["right"].X = this.Size.Width - 1;
            Sprites["bottom"].Y = this.Size.Height - 1;
            Redraw();
        }

        protected override void Draw()
        {
            if (Sprites["left"].Bitmap != null) Sprites["left"].Bitmap.Dispose();
            if (Sprites["top"].Bitmap != null) Sprites["top"].Bitmap.Dispose();

            Bitmap v = new Bitmap(1, this.Size.Height);
            v.Unlock();
            Bitmap h = new Bitmap(this.Size.Width - 2, 1);
            h.Unlock();

            Color c1 = Color.BLACK;
            Color c2 = new Color(255, 255, 0);// Color.WHITE;

            for (int y = 0; y < v.Height; y++)
            {
                if (Math.Floor((y + Offset) / 4d) % 2 == 0)
                    v.SetPixel(0, y, c1);
                else v.SetPixel(0, y, c2);
            }
            for (int x = 0; x < h.Width; x++)
            {
                if (Math.Floor((x + Offset) / 4d) % 2 == 0)
                    h.SetPixel(x, 0, c1);
                else h.SetPixel(x, 0, c2);
            }

            v.Lock();
            h.Lock();

            Sprites["left"].Bitmap = v;
            Sprites["top"].Bitmap = h;
            Sprites["right"].Bitmap = v;
            Sprites["right"].MirrorY = true;
            Sprites["bottom"].Bitmap = h;
            Sprites["bottom"].MirrorX = true;

            base.Draw();
        }
    }
}
