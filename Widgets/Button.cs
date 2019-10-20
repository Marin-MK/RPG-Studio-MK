using System;
using ODL;

namespace MKEditor.Widgets
{
    public class Button : Widget
    {
        public string Text { get; protected set; }

        public EventHandler<EventArgs> OnClicked;

        public Button(object Parent, string Name = "button")
            : base(Parent, Name)
        {
            SetSize(85, 33);
            Bitmap b = new Bitmap(10, 10);
            #region Corner piece
            b.Unlock();
            b.DrawLine(7, 0, 9, 0, 0, 22, 44, 6);
            b.DrawLine(4, 1, 9, 1, 0, 22, 44, 6);
            b.DrawLine(3, 2, 5, 2, 0, 22, 44, 6);
            b.SetPixel(3, 3, 0, 22, 44, 6);
            b.DrawLine(2, 3, 2, 5, 0, 22, 44, 6);
            b.DrawLine(1, 4, 1, 9, 0, 22, 44, 6);
            b.DrawLine(0, 7, 0, 9, 0, 22, 44, 6);
            b.SetPixel(2, 6, 9, 0, 22, 8);
            b.SetPixel(3, 4, 9, 0, 22, 8);
            b.SetPixel(4, 3, 9, 0, 22, 8);
            b.SetPixel(6, 2, 9, 0, 22, 8);
            b.DrawLine(7, 2, 9, 2, 0, 2, 4, 13);
            b.SetPixel(5, 3, 0, 2, 4, 13);
            b.SetPixel(4, 4, 0, 2, 4, 13);
            b.SetPixel(3, 5, 0, 2, 4, 13);
            b.DrawLine(2, 7, 2, 9, 0, 2, 4, 13);
            b.SetPixel(3, 6, 0, 9, 17, 19);
            b.SetPixel(6, 3, 0, 9, 17, 19);
            b.SetPixel(4, 5, 4, 2, 0, 21);
            b.SetPixel(5, 4, 4, 2, 0, 21);
            b.SetPixel(3, 7, 4, 2, 0, 21);
            b.SetPixel(7, 3, 4, 2, 0, 21);
            b.SetPixel(3, 8, 0, 2, 4, 26);
            b.SetPixel(8, 3, 0, 2, 4, 26);
            b.SetPixel(6, 4, 1, 0, 7, 33);
            b.SetPixel(4, 6, 1, 0, 7, 33);
            b.SetPixel(9, 3, 0, 6, 12, 32); //
            b.SetPixel(3, 9, 0, 6, 12, 32); //
            b.SetPixel(5, 5, 0, 2, 4, 38);
            b.SetPixel(7, 4, 0, 5, 4, 45);
            b.SetPixel(4, 7, 0, 5, 4, 45);
            b.SetPixel(8, 4, 0, 3, 0, 52);
            b.SetPixel(4, 8, 0, 3, 0, 52);
            b.SetPixel(9, 4, 0, 4, 4, 57);
            b.SetPixel(4, 9, 0, 4, 4, 57);
            b.SetPixel(6, 5, 0, 4, 4, 57);
            b.SetPixel(5, 6, 0, 4, 4, 57);
            b.SetPixel(5, 7, 0, 2, 4, 77);
            b.SetPixel(7, 5, 0, 2, 4, 77);
            b.SetPixel(6, 6, 0, 2, 1, 89);
            b.SetPixel(8, 5, 0, 2, 1, 89);
            b.SetPixel(5, 8, 0, 2, 1, 89);
            b.SetPixel(9, 5, 1, 0, 1, 99);
            b.SetPixel(5, 9, 1, 0, 1, 99);
            b.Lock();
            #endregion
            Sprites["topleft"] = new Sprite(this.Viewport, b);
            Sprites["bottomleft"] = new Sprite(this.Viewport, b);
            Sprites["bottomleft"].MirrorY = true;
            Sprites["bottomleft"].Y = Size.Height - 10;
            Sprites["topright"] = new Sprite(this.Viewport, b);
            Sprites["topright"].MirrorX = true;
            Sprites["topright"].X = Size.Width - 10;
            Sprites["bottomright"] = new Sprite(this.Viewport, b);
            Sprites["bottomright"].MirrorX = Sprites["bottomright"].MirrorY = true;
            Sprites["bottomright"].X = Sprites["topright"].X;
            Sprites["bottomright"].Y = Sprites["bottomleft"].Y;

            Sprites["filler"] = new Sprite(this.Viewport, new Bitmap(73, 21));
            Sprites["filler"].Bitmap.Unlock();
            Sprites["filler"].Bitmap.FillRect(0, 0, 73, 21, Color.WHITE);
            Sprites["filler"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["filler"].Bitmap.SetPixel(72, 0, Color.ALPHA);
            Sprites["filler"].Bitmap.SetPixel(0, 20, Color.ALPHA);
            Sprites["filler"].Bitmap.SetPixel(72, 20, Color.ALPHA);
            Sprites["filler"].Bitmap.Lock();
            Sprites["filler"].X = Sprites["filler"].Y = 6;
            Sprites["filler"].Color = new Color(64, 104, 146);

            Bitmap hor = new Bitmap(6, 1);
            hor.Unlock();
            hor.SetPixel(0, 0, 0, 22, 44, 6);
            hor.SetPixel(1, 0, 0, 22, 44, 6);
            hor.SetPixel(2, 0, 0, 2, 4, 13);
            hor.SetPixel(3, 0, 1, 0, 7, 33);
            hor.SetPixel(4, 0, 0, 2, 4, 64);
            hor.SetPixel(5, 0, 0, 1, 4, 108);
            hor.Lock();

            Bitmap vert = new Bitmap(1, 6);
            vert.Unlock();
            vert.SetPixel(0, 0, 0, 22, 44, 6);
            vert.SetPixel(0, 1, 0, 22, 44, 6);
            vert.SetPixel(0, 2, 0, 2, 4, 13);
            vert.SetPixel(0, 3, 1, 0, 7, 33);
            vert.SetPixel(0, 4, 0, 2, 4, 64);
            vert.SetPixel(0, 5, 0, 1, 4, 108);
            vert.Lock();

            Sprites["left"] = new Sprite(this.Viewport, hor);
            Sprites["left"].Y = 10;
            Sprites["left"].ZoomY = Size.Height - 20;
            Sprites["right"] = new Sprite(this.Viewport, hor);
            Sprites["right"].X = Sprites["topright"].X + 4;
            Sprites["right"].Y = Sprites["left"].Y;
            Sprites["right"].ZoomY = Sprites["left"].ZoomY;
            Sprites["right"].MirrorX = true;

            Sprites["top"] = new Sprite(this.Viewport, vert);
            Sprites["top"].X = 10;
            Sprites["top"].ZoomX = Size.Width - 20;
            Sprites["bottom"] = new Sprite(this.Viewport, vert);
            Sprites["bottom"].X = Sprites["top"].X;
            Sprites["bottom"].Y = Sprites["bottomleft"].Y + 4;
            Sprites["bottom"].ZoomX = Sprites["top"].ZoomX;
            Sprites["bottom"].MirrorY = true;

            Sprites["text"] = new Sprite(this.Viewport);

            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseDown += MouseDown;
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
                Font f = Font.Get("Fonts/ProductSans-B", 14);
                Size s = f.TextSize(Text);
                Sprites["text"].Bitmap = new Bitmap(s);
                Sprites["text"].Bitmap.Unlock();
                Sprites["text"].Bitmap.Font = f;
                Sprites["text"].Bitmap.DrawText(Text, Color.WHITE);
                Sprites["text"].Bitmap.Lock();
                Sprites["text"].X = Size.Width / 2 - s.Width / 2;
                Sprites["text"].Y = 7;
            }
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            Sprites["filler"].Color = WidgetIM.Hovering ? new Color(59, 227, 255) : new Color(64, 104, 146);
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (!WidgetIM.Hovering) return;
            if (OnClicked != null) OnClicked.Invoke(null, new EventArgs());
        }
    }
}
