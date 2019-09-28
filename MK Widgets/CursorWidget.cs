using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CursorWidget : Widget
    {
        Sprite topleft;
        Sprite topright;
        Sprite bottomleft;
        Sprite bottomright;

        Sprite top;
        Sprite left;
        Sprite right;
        Sprite bottom;

        public CursorWidget(object Parent, string Name = "cursorWidget")
            : base(Parent, Name)
        {
            Bitmap b = new Bitmap(23, 23);
            #region Cornerpiece with shadow
            b.Unlock();
            b.SetPixel(5, 0, 0, 0, 0, 1);
            b.SetPixel(6, 0, 0, 0, 0, 1);
            b.SetPixel(7, 0, 0, 0, 0, 1);
            b.SetPixel(8, 0, 0, 0, 0, 1);
            b.SetPixel(5, 1, 0, 0, 0, 1);
            b.SetPixel(4, 1, 0, 0, 0, 1);
            b.SetPixel(3, 2, 0, 0, 0, 1);
            b.SetPixel(2, 2, 0, 0, 0, 1);
            b.SetPixel(2, 3, 0, 0, 0, 1);
            b.SetPixel(1, 4, 0, 0, 0, 1);
            b.SetPixel(1, 5, 0, 0, 0, 1);
            b.SetPixel(0, 6, 0, 0, 0, 1);
            b.SetPixel(0, 7, 0, 0, 0, 1);
            b.SetPixel(0, 8, 0, 0, 0, 1);
            b.SetPixel(0, 9, 0, 0, 0, 1);
            b.SetPixel(0, 10, 0, 0, 0, 1);
            b.DrawLine(9, 0, 22, 0, 0, 0, 0, 2);
            b.DrawLine(0, 11, 0, 22, 0, 0, 0, 2);
            b.SetPixel(6, 1, 0, 0, 0, 2);
            b.SetPixel(4, 2, 0, 0, 0, 2);
            b.SetPixel(3, 3, 0, 0, 0, 2);
            b.SetPixel(2, 4, 0, 0, 0, 2);
            b.SetPixel(1, 6, 0, 0, 0, 2);
            b.SetPixel(7, 1, 0, 0, 0, 3);
            b.SetPixel(5, 2, 0, 0, 0, 3);
            b.SetPixel(2, 5, 0, 0, 0, 3);
            b.SetPixel(1, 7, 0, 0, 0, 3);
            b.SetPixel(1, 8, 0, 0, 0, 3);
            b.SetPixel(9, 1, 0, 0, 0, 4);
            b.SetPixel(8, 1, 0, 0, 0, 4);
            b.SetPixel(4, 3, 0, 0, 0, 4);
            b.SetPixel(3, 4, 0, 0, 0, 4);
            b.SetPixel(1, 9, 0, 0, 0, 4);
            b.SetPixel(1, 10, 0, 0, 0, 4);
            b.DrawLine(10, 1, 22, 1, 0, 0, 0, 5);
            b.SetPixel(6, 2, 0, 0, 0, 5);
            b.SetPixel(2, 6, 0, 0, 0, 5);
            b.DrawLine(1, 11, 1, 22, 0, 0, 0, 5);
            b.SetPixel(7, 2, 0, 0, 0, 7);
            b.SetPixel(5, 3, 0, 0, 0, 7);
            b.SetPixel(3, 5, 0, 0, 0, 7);
            b.SetPixel(2, 7, 0, 0, 0, 7);
            b.SetPixel(4, 4, 0, 0, 0, 8);
            b.SetPixel(2, 8, 0, 0, 0, 8);
            b.SetPixel(8, 2, 0, 0, 0, 9);
            b.SetPixel(9, 2, 0, 0, 0, 10);
            b.SetPixel(6, 3, 0, 0, 0, 10);
            b.SetPixel(3, 6, 0, 0, 0, 10);
            b.SetPixel(2, 9, 0, 0, 0, 10);
            b.SetPixel(2, 10, 0, 0, 0, 10);
            b.SetPixel(10, 2, 0, 0, 0, 11);
            b.SetPixel(11, 2, 0, 0, 0, 11);
            b.SetPixel(2, 11, 0, 0, 0, 11);
            b.SetPixel(2, 12, 0, 0, 0, 11);
            b.SetPixel(2, 13, 0, 0, 0, 11);
            b.DrawLine(12, 2, 22, 2, 0, 0, 0, 12);
            b.DrawLine(2, 14, 2, 22, 0, 0, 0, 12);
            b.SetPixel(5, 4, 0, 0, 0, 13);
            b.SetPixel(4, 5, 0, 0, 0, 13);
            b.SetPixel(7, 3, 0, 0, 0, 14);
            b.SetPixel(3, 7, 0, 0, 0, 14);
            b.SetPixel(8, 3, 0, 0, 0, 17);
            b.SetPixel(3, 8, 0, 0, 0, 17);
            b.SetPixel(6, 4, 0, 0, 0, 19);
            b.SetPixel(4, 6, 0, 0, 0, 19);
            b.SetPixel(9, 3, 0, 0, 0, 20);
            b.SetPixel(3, 9, 0, 0, 0, 20);
            b.SetPixel(5, 5, 0, 0, 0, 21);
            b.SetPixel(10, 3, 0, 0, 0, 22);
            b.SetPixel(3, 10, 0, 0, 0, 22);
            b.SetPixel(11, 3, 0, 0, 0, 23);
            b.SetPixel(3, 11, 0, 0, 0, 23);
            b.DrawLine(12, 3, 22, 3, 0, 0, 0, 24);
            b.DrawLine(3, 12, 3, 22, 0, 0, 0, 24);
            b.SetPixel(4, 7, 0, 0, 0, 25);
            b.SetPixel(7, 4, 0, 0, 0, 26);
            b.SetPixel(6, 5, 0, 0, 0, 31);
            b.SetPixel(5, 6, 0, 0, 0, 31);
            b.SetPixel(8, 4, 0, 0, 0, 32);
            b.SetPixel(4, 8, 0, 0, 0, 32);
            b.SetPixel(9, 4, 0, 0, 0, 37);
            b.SetPixel(4, 9, 0, 0, 0, 37);
            b.SetPixel(4, 10, 0, 0, 0, 40);
            b.SetPixel(10, 4, 0, 0, 0, 41);
            b.SetPixel(7, 5, 0, 0, 0, 42);
            b.SetPixel(5, 7, 0, 0, 0, 42);
            b.SetPixel(4, 11, 0, 0, 0, 42);
            b.SetPixel(11, 4, 0, 0, 0, 43);
            b.SetPixel(12, 4, 0, 0, 0, 44);
            b.SetPixel(4, 12, 0, 0, 0, 44);
            b.SetPixel(4, 13, 0, 0, 0, 44);
            b.SetPixel(4, 14, 0, 0, 0, 44);
            b.DrawLine(13, 4, 22, 4, 0, 0, 0, 45);
            b.DrawLine(4, 15, 4, 22, 0, 0, 0, 45);
            b.SetPixel(6, 6, 0, 0, 0, 46);
            b.SetPixel(8, 5, 0, 0, 0, 52);
            b.SetPixel(5, 8, 0, 0, 0, 52);
            b.SetPixel(9, 5, 0, 0, 0, 60);
            b.SetPixel(5, 9, 0, 0, 0, 60);
            b.SetPixel(6, 7, 0, 0, 0, 62);
            b.SetPixel(7, 6, 0, 0, 0, 63);
            b.SetPixel(10, 5, 0, 0, 0, 66);
            b.SetPixel(5, 10, 0, 0, 0, 66);
            b.SetPixel(11, 5, 0, 0, 0, 70);
            b.SetPixel(5, 11, 0, 0, 0, 70);
            b.SetPixel(12, 5, 0, 0, 0, 72);
            b.SetPixel(13, 5, 0, 0, 0, 72);
            b.SetPixel(5, 12, 0, 0, 0, 72);
            b.DrawLine(14, 5, 22, 5, 0, 0, 0, 73);
            b.DrawLine(5, 13, 5, 22, 0, 0, 0, 73);
            b.SetPixel(6, 8, 0, 0, 0, 77);
            b.SetPixel(8, 6, 0, 0, 0, 78);
            b.SetPixel(6, 9, 0, 0, 0, 89);
            b.SetPixel(9, 6, 0, 0, 0, 90);
            b.SetPixel(6, 10, 0, 0, 0, 98);
            b.SetPixel(10, 6, 0, 0, 0, 99);
            b.SetPixel(6, 11, 0, 0, 0, 103);
            b.SetPixel(11, 6, 0, 0, 0, 104);
            b.SetPixel(6, 12, 0, 0, 0, 106);
            b.SetPixel(12, 6, 0, 0, 0, 107);
            b.SetPixel(13, 6, 0, 0, 0, 108);
            b.SetPixel(6, 13, 0, 0, 0, 108);
            b.SetPixel(6, 14, 0, 0, 0, 108);
            b.DrawLine(14, 6, 22, 6, 0, 0, 0, 109);
            b.DrawLine(6, 15, 6, 22, 0, 0, 0, 109);
            b.DrawLine(7, 7, 22, 7, 0, 0, 0);
            b.DrawLine(7, 8, 7, 22, 0, 0, 0);
            b.DrawLine(8, 8, 22, 8, 255, 255, 255);
            b.DrawLine(8, 9, 8, 22, 255, 255, 255);
            b.DrawLine(9, 9, 22, 9, 0, 0, 0);
            b.DrawLine(9, 10, 9, 22, 0, 0, 0);
            b.Lock();
            #endregion

            topleft = new Sprite(this.Viewport, b);
            Sprites["topleft"] = topleft;
            topright = new Sprite(this.Viewport, b);
            topright.MirrorX = true;
            topright.X = 23;
            Sprites["topright"] = topright;
            bottomleft = new Sprite(this.Viewport, b);
            bottomleft.MirrorY = true;
            bottomleft.Y = 23;
            Sprites["bottomleft"] = bottomleft;
            bottomright = new Sprite(this.Viewport, b);
            bottomright.MirrorX = bottomright.MirrorY = true;
            bottomright.X = bottomright.Y = 23;
            Sprites["bottomright"] = bottomright;

            Bitmap vfiller = new Bitmap(1, 10);
            vfiller.Unlock();
            vfiller.SetPixel(0, 0, 0, 0, 0, 2);
            vfiller.SetPixel(0, 1, 0, 0, 0, 5);
            vfiller.SetPixel(0, 2, 0, 0, 0, 12);
            vfiller.SetPixel(0, 3, 0, 0, 0, 24);
            vfiller.SetPixel(0, 4, 0, 0, 0, 45);
            vfiller.SetPixel(0, 5, 0, 0, 0, 73);
            vfiller.SetPixel(0, 6, 0, 0, 0, 109);
            vfiller.SetPixel(0, 7, 0, 0, 0);
            vfiller.SetPixel(0, 8, 255, 255, 255);
            vfiller.SetPixel(0, 9, 0, 0, 0);
            vfiller.Lock();

            Bitmap hfiller = new Bitmap(10, 1);
            hfiller.Unlock();
            hfiller.SetPixel(0, 0, 0, 0, 0, 2);
            hfiller.SetPixel(1, 0, 0, 0, 0, 5);
            hfiller.SetPixel(2, 0, 0, 0, 0, 12);
            hfiller.SetPixel(3, 0, 0, 0, 0, 24);
            hfiller.SetPixel(4, 0, 0, 0, 0, 45);
            hfiller.SetPixel(5, 0, 0, 0, 0, 73);
            hfiller.SetPixel(6, 0, 0, 0, 0, 109);
            hfiller.SetPixel(7, 0, 0, 0, 0);
            hfiller.SetPixel(8, 0, 255, 255, 255);
            hfiller.SetPixel(9, 0, 0, 0, 0);
            hfiller.Lock();

            top = new Sprite(this.Viewport, vfiller);
            top.X = 23;
            top.ZoomX = 0;
            Sprites["top"] = top;
            left = new Sprite(this.Viewport, hfiller);
            left.Y = 23;
            left.ZoomY = 0;
            Sprites["left"] = left;
            right = new Sprite(this.Viewport, hfiller);
            right.Y = 23;
            right.MirrorX = true;
            right.ZoomY = 0;
            Sprites["right"] = right;
            bottom = new Sprite(this.Viewport, vfiller);
            bottom.X = 23;
            bottom.MirrorY = true;
            bottom.ZoomX = 0;
            Sprites["bottom"] = bottom;

            SetSize(46, 46);

            this.SetZIndex(1);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (true)//this.Size.Width < 46 || this.Size.Height < 46)
            {
                topleft.Visible = false;
                topright.Visible = false;
                bottomleft.Visible = false;
                bottomright.Visible = false;
                top.Visible = false;
                left.Visible = false;
                right.Visible = false;
                bottom.Visible = false;
                if (!Sprites.ContainsKey("rect")) Sprites["rect"] = new Sprite(this.Viewport);
                if (Sprites["rect"].Bitmap != null) Sprites["rect"].Bitmap.Dispose();
                Sprites["rect"].Bitmap = new Bitmap(Size);
                Sprites["rect"].Bitmap.Unlock();
                Sprites["rect"].Bitmap.DrawRect(7, 7, Size.Width - 14, Size.Height - 14, Color.BLACK);
                Sprites["rect"].Bitmap.DrawRect(8, 8, Size.Width - 16, Size.Height - 16, Color.WHITE);
                Sprites["rect"].Bitmap.DrawRect(9, 9, Size.Width - 18, Size.Height - 18, Color.BLACK);
                Sprites["rect"].Bitmap.Lock();
            }
            else
            {
                topleft.Visible = true;
                topright.Visible = true;
                bottomleft.Visible = true;
                bottomright.Visible = true;
                top.Visible = true;
                left.Visible = true;
                right.Visible = true;
                bottom.Visible = true;
                if (Sprites.ContainsKey("rect") && !Sprites["rect"].Bitmap.Disposed) Sprites["rect"].Bitmap.Dispose();
                topright.X = Size.Width - 23;
                bottomleft.Y = Size.Height - 23;
                bottomright.X = topright.X;
                bottomright.Y = bottomleft.Y;
                top.ZoomX = Size.Width - 46;
                left.ZoomY = Size.Height - 46;
                right.X = Size.Width - 10;
                right.ZoomY = Size.Height - 46;
                bottom.Y = Size.Height - 10;
                bottom.ZoomX = Size.Width - 46;
            }
        }
    }
}
