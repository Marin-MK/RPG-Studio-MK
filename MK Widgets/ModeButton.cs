using System;
using ODL;

namespace MKEditor.Widgets
{
    public class ModeButton : Widget
    {
        public string Text { get; private set; } = "";
        public bool Selected { get; private set; }

        public EventHandler<EventArgs> OnSelection;
        public EventHandler<EventArgs> OnDeselection;

        public ModeButton(object Parent, string Name = "modeButton")
            : base(Parent, Name)
        {
            Bitmap c = new Bitmap(12, 12);
            #region Create corner piece
            c.Unlock();
            c.SetPixel(0, 1, 0, 0, 0, 1);
            c.SetPixel(1, 0, 0, 0, 0, 1);
            c.SetPixel(2, 0, 0, 0, 0, 2);
            c.SetPixel(0, 2, 0, 0, 0, 2);
            c.SetPixel(3, 0, 0, 0, 0, 3);
            c.SetPixel(0, 3, 0, 0, 0, 3);
            c.SetPixel(1, 1, 0, 0, 0, 3);
            c.SetPixel(4, 0, 0, 0, 0, 4);
            c.SetPixel(0, 4, 0, 0, 0, 5);
            c.SetPixel(5, 0, 0, 0, 0, 6);
            c.SetPixel(0, 5, 0, 0, 0, 6);
            c.SetPixel(0, 6, 0, 0, 0, 7);
            c.SetPixel(2, 1, 0, 0, 0, 7);
            c.SetPixel(1, 2, 0, 0, 0, 7);
            c.SetPixel(6, 0, 0, 0, 0, 7);
            c.SetPixel(7, 0, 0, 0, 0, 8);
            c.SetPixel(0, 7, 0, 0, 0, 8);
            c.SetPixel(8, 0, 0, 0, 0, 10);
            c.SetPixel(0, 8, 0, 0, 0, 10);
            c.SetPixel(9, 0, 0, 0, 0, 11);
            c.SetPixel(3, 1, 0, 0, 0, 11);
            c.SetPixel(1, 3, 0, 0, 0, 11);
            c.SetPixel(0, 9, 0, 0, 0, 11);
            c.SetPixel(10, 0, 0, 0, 0, 12);
            c.SetPixel(0, 10, 0, 0, 0, 12);
            c.SetPixel(11, 0, 0, 0, 0, 13);
            c.SetPixel(2, 2, 0, 0, 0, 13);
            c.SetPixel(0, 11, 0, 0, 0, 13);
            c.SetPixel(4, 1, 0, 0, 0, 15);
            c.SetPixel(1, 4, 0, 0, 0, 15);
            c.SetPixel(1, 5, 0, 0, 0, 18);
            c.SetPixel(5, 1, 0, 0, 0, 19);
            c.SetPixel(3, 2, 0, 0, 0, 20);
            c.SetPixel(2, 3, 0, 0, 0, 20);
            c.SetPixel(1, 6, 0, 0, 0, 22);
            c.SetPixel(6, 1, 0, 0, 0, 22);
            c.SetPixel(7, 1, 0, 0, 0, 26);
            c.SetPixel(4, 2, 0, 0, 0, 26);
            c.SetPixel(2, 4, 0, 0, 0, 26);
            c.SetPixel(1, 7, 0, 0, 0, 26);
            c.SetPixel(3, 3, 0, 0, 0, 29);
            c.SetPixel(8, 1, 0, 0, 0, 30);
            c.SetPixel(1, 8, 0, 0, 0, 30);
            c.SetPixel(5, 2, 0, 0, 0, 33);
            c.SetPixel(2, 5, 0, 0, 0, 33);
            c.SetPixel(9, 1, 0, 0, 0, 34);
            c.SetPixel(1, 9, 0, 0, 0, 34);
            c.SetPixel(10, 1, 0, 0, 0, 37);
            c.SetPixel(1, 10, 0, 0, 0, 37);
            c.SetPixel(11, 1, 0, 0, 0, 38);
            c.SetPixel(1, 11, 0, 0, 0, 38);
            c.SetPixel(4, 3, 0, 0, 0, 38);
            c.SetPixel(3, 4, 0, 0, 0, 38);
            c.SetPixel(6, 2, 0, 0, 0, 39);
            c.SetPixel(2, 6, 0, 0, 0, 39);
            c.SetPixel(7, 2, 0, 0, 0, 45);
            c.SetPixel(2, 7, 0, 0, 0, 45);
            c.SetPixel(3, 5, 0, 0, 0, 46);
            c.SetPixel(5, 3, 0, 0, 0, 47);
            c.SetPixel(4, 4, 0, 0, 0, 49);
            c.SetPixel(8, 2, 0, 0, 0, 52);
            c.SetPixel(2, 8, 0, 0, 0, 52);
            c.SetPixel(3, 6, 0, 0, 0, 55);
            c.SetPixel(6, 3, 0, 0, 0, 56);
            c.SetPixel(9, 2, 0, 0, 0, 58);
            c.SetPixel(2, 9, 0, 0, 0, 58);
            c.SetPixel(5, 4, 0, 0, 0, 61);
            c.SetPixel(4, 5, 0, 0, 0, 61);
            c.SetPixel(10, 2, 0, 0, 0, 62);
            c.SetPixel(2, 10, 0, 0, 0, 63);
            c.SetPixel(11, 2, 0, 0, 0, 64);
            c.SetPixel(2, 11, 0, 0, 0, 64);
            c.SetPixel(7, 3, 0, 0, 0, 64);
            c.SetPixel(3, 7, 0, 0, 0, 64);
            c.SetPixel(6, 4, 0, 0, 0, 72);
            c.SetPixel(4, 6, 0, 0, 0, 72);
            c.SetPixel(8, 3, 0, 0, 0, 73);
            c.SetPixel(3, 8, 0, 0, 0, 73);
            c.SetPixel(5, 5, 0, 0, 0, 75);
            c.SetPixel(9, 3, 0, 0, 0, 82);
            c.SetPixel(3, 9, 0, 0, 0, 82);
            c.SetPixel(7, 4, 0, 0, 0, 83);
            c.SetPixel(4, 7, 0, 0, 0, 84);
            c.SetPixel(10, 3, 0, 0, 0, 88);
            c.SetPixel(3, 10, 0, 0, 0, 88);
            c.SetPixel(11, 3, 0, 0, 0, 89);
            c.SetPixel(3, 11, 0, 0, 0, 89);
            //c.SetPixel(6, 5, 0, 0, 0, 89);
            //c.SetPixel(5, 6, 0, 0, 0, 89);
            c.SetPixel(8, 4, 0, 0, 0, 95);
            c.SetPixel(4, 8, 0, 0, 0, 95);
            //c.SetPixel(7, 5, 0, 0, 0, 103);
            //c.SetPixel(5, 7, 0, 0, 0, 103);
            //c.SetPixel(6, 6, 0, 0, 0, 105);
            c.SetPixel(9, 4, 0, 0, 0, 106);
            c.SetPixel(4, 9, 0, 0, 0, 107);
            c.SetPixel(10, 4, 0, 0, 0, 113);
            c.SetPixel(4, 10, 0, 0, 0, 114);
            c.SetPixel(11, 4, 0, 0, 0, 115);
            c.SetPixel(4, 11, 0, 0, 0, 115);
            //c.SetPixel(8, 5, 0, 0, 0, 117);
            //c.SetPixel(5, 8, 0, 0, 0, 117);
            //c.SetPixel(7, 6, 0, 0, 0, 122);
            //c.SetPixel(6, 7, 0, 0, 0, 122);
            //c.SetPixel(9, 5, 0, 0, 0, 131);
            //c.SetPixel(5, 9, 0, 0, 0, 131);
            //c.SetPixel(8, 6, 0, 0, 0, 138);
            //c.SetPixel(6, 8, 0, 0, 0, 139);
            //c.SetPixel(10, 5, 0, 0, 0, 139);
            //c.SetPixel(5, 10, 0, 0, 0, 139);
            //c.SetPixel(11, 5, 0, 0, 0, 140);
            //c.SetPixel(5, 11, 0, 0, 0, 140);
            //c.SetPixel(7, 7, 0, 0, 0, 141);
            //c.SetPixel(9, 6, 0, 0, 0, 155);
            //c.SetPixel(6, 9, 0, 0, 0, 155);
            //c.SetPixel(8, 7, 0, 0, 0, 160);
            //c.SetPixel(7, 8, 0, 0, 0, 160);
            //c.SetPixel(10, 6, 0, 0, 0, 164);
            //c.SetPixel(6, 10, 0, 0, 0, 165);
            //c.SetPixel(11, 6, 0, 0, 0, 166);
            //c.SetPixel(6, 11, 0, 0, 0, 166);
            //c.SetPixel(9, 7, 0, 0, 0, 179);
            //c.SetPixel(7, 9, 0, 0, 0, 179);
            //c.SetPixel(8, 8, 0, 0, 0, 182);
            //c.SetPixel(10, 7, 0, 0, 0, 190);
            //c.SetPixel(7, 10, 0, 0, 0, 190);
            //c.SetPixel(11, 7, 0, 0, 0, 191);
            //c.SetPixel(7, 11, 0, 0, 0, 191);
            //c.SetPixel(9, 8, 0, 0, 0, 203);
            //c.SetPixel(8, 9, 0, 0, 0, 204);
            //c.SetPixel(10, 8, 0, 0, 0, 215);
            //c.SetPixel(8, 10, 0, 0, 0, 216);
            //c.SetPixel(11, 8, 0, 0, 0, 217);
            //c.SetPixel(8, 11, 0, 0, 0, 217);
            //c.SetPixel(9, 9, 0, 0, 0, 227);
            //c.SetPixel(10, 9, 0, 0, 0, 241);
            //c.SetPixel(9, 10, 0, 0, 0, 241);
            //c.SetPixel(11, 9, 0, 0, 0, 242);
            //c.SetPixel(9, 11, 0, 0, 0, 242);
            //c.SetPixel(10, 10, 0, 0, 0, 254);
            //c.SetPixel(11, 10, 0, 0, 0, 255);
            //c.SetPixel(10, 11, 0, 0, 0, 255);
            //c.SetPixel(11, 11, 0, 0, 0, 255);
            c.Lock();
            #endregion
            Sprites["topleft"] = new Sprite(this.Viewport, c);
            Sprites["topright"] = new Sprite(this.Viewport, c);
            Sprites["topright"].MirrorX = true;
            Sprites["bottomleft"] = new Sprite(this.Viewport, c);
            Sprites["bottomleft"].MirrorY = true;
            Sprites["bottomright"] = new Sprite(this.Viewport, c);
            Sprites["bottomright"].MirrorX = Sprites["bottomright"].MirrorY = true;

            Bitmap h = new Bitmap(5, 1);
            h.Unlock();
            h.SetPixel(0, 0, 0, 0, 0, 13);
            h.SetPixel(1, 0, 0, 0, 0, 38);
            h.SetPixel(2, 0, 0, 0, 0, 64);
            h.SetPixel(3, 0, 0, 0, 0, 89);
            h.SetPixel(4, 0, 0, 0, 0, 115);
            h.Lock();

            Sprites["left"] = new Sprite(this.Viewport, h);
            Sprites["right"] = new Sprite(this.Viewport, h);
            Sprites["right"].MirrorX = true;

            Bitmap v = new Bitmap(1, 5);
            v.Unlock();
            v.SetPixel(0, 0, 0, 0, 0, 13);
            v.SetPixel(0, 1, 0, 0, 0, 38);
            v.SetPixel(0, 2, 0, 0, 0, 64);
            v.SetPixel(0, 3, 0, 0, 0, 89);
            v.SetPixel(0, 4, 0, 0, 0, 115);
            v.Lock();

            Sprites["top"] = new Sprite(this.Viewport, v);
            Sprites["bottom"] = new Sprite(this.Viewport, v);
            Sprites["bottom"].MirrorY = true;

            Sprites["bg"] = new Sprite(this.Viewport, new Bitmap(96, 32));
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.FillRect(0, 0, 96, 32, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(95, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, 31, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(95, 31, Color.ALPHA);
            Sprites["bg"].Bitmap.Lock();
            Sprites["bg"].X = 5;
            Sprites["bg"].Y = 5;

            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].Y = 10;

            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(106, 2, new Color(59, 227, 255)));
            Sprites["selector"].Visible = false;
            Sprites["selector"].Y = 50;

            WidgetIM.OnHoverChanged += UpdateSelector;
            WidgetIM.OnMouseMoving += UpdateSelector;
            WidgetIM.OnMouseDown += MouseDown;

            SetSize(106, 52);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            Sprites["topright"].X = Size.Width - Sprites["topright"].Bitmap.Width;
            Sprites["bottomleft"].Y = Size.Height - Sprites["bottomleft"].Bitmap.Height - 10;
            Sprites["bottomright"].X = Size.Width - Sprites["bottomright"].Bitmap.Width;
            Sprites["bottomright"].Y = Size.Height - Sprites["bottomright"].Bitmap.Height - 10;

            Sprites["right"].X = Size.Width - Sprites["right"].Bitmap.Width;
            Sprites["bottom"].Y = Size.Height - Sprites["bottom"].Bitmap.Height - 10;

            Sprites["left"].Y = Sprites["topleft"].Bitmap.Height;
            Sprites["left"].ZoomY = Size.Height - 2 * Sprites["topleft"].Bitmap.Height - 10;
            Sprites["right"].Y = Sprites["left"].Y;
            Sprites["right"].ZoomY = Sprites["left"].ZoomY;
            Sprites["top"].X = Sprites["topleft"].Bitmap.Width;
            Sprites["top"].ZoomX = Size.Width - 2 * Sprites["topleft"].Bitmap.Width;
            Sprites["bottom"].X = Sprites["top"].X;
            Sprites["bottom"].ZoomX = Sprites["top"].ZoomX;
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                Redraw();
            }
        }

        public void SetSelected(bool Selected)
        {
            if (this.Selected != Selected)
            {
                if (Selected)
                {
                    foreach (Widget w in Parent.Widgets)
                    {
                        if (!(w is ModeButton)) continue;
                        ModeButton b = w as ModeButton;
                        if (b.Selected) b.SetSelected(false);
                    }
                }
                this.Selected = Selected;
                if (Selected && this.OnSelection != null) this.OnSelection.Invoke(this, new EventArgs());
                if (!Selected && this.OnDeselection != null) this.OnDeselection.Invoke(this, new EventArgs());
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Font f = Font.Get("Fonts/ProductSans-B", 16);
            Size s = f.TextSize(Text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText(Text, Selected ? new Color(0, 165, 255) : Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            base.Draw();
        }

        public void UpdateSelector(object sender, MouseEventArgs e)
        {
            int ry = e.Y - Viewport.Y;
            Sprites["selector"].Visible = WidgetIM.Hovering && ry < 42;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (Sprites["selector"].Visible)
            {
                SetSelected(true);
            }
        }
    }
}
