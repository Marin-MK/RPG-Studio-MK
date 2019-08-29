﻿using System;
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
            //RectSprite outer = new RectSprite(this.Viewport);
            //outer.SetOuterColor(Color.BLACK);
            //Sprites["outer"] = outer;

            //RectSprite middle = new RectSprite(this.Viewport);
            //middle.SetOuterColor(Color.WHITE);
            //Sprites["middle"] = middle;

            //RectSprite inner = new RectSprite(this.Viewport);
            //inner.SetOuterColor(Color.BLACK);
            //Sprites["inner"] = inner;

            //this.SetSize(32, 32);

            //middle.X = middle.Y = 1;
            //inner.X = inner.Y = 2;

            //this.SetSize(32, 32);

            Bitmap b = new Bitmap(23, 23);
            #region Cursor w/ glow
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
            // 1px
            b.DrawLine(8, 9, 8, 22, 255, 255, 255);
            b.DrawLine(9, 9, 22, 9, 0, 0, 0);
            b.DrawLine(9, 10, 9, 22, 0, 0, 0);
            // 2px
            //b.DrawLine(8, 10, 8, 22, 255, 255, 255);
            //b.DrawLine(8, 9, 22, 9, 255, 255, 255);
            //b.DrawLine(9, 10, 9, 22, 255, 255, 255);
            //b.DrawLine(10, 10, 22, 10, 0, 0, 0); // 2px
            //b.DrawLine(10, 11, 10, 22, 0, 0, 0); // 2px
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

            Bitmap filler = new Bitmap(1, 10);
            filler.Unlock();
            filler.SetPixel(0, 0, 0, 0, 0, 2);
            filler.SetPixel(0, 1, 0, 0, 0, 5);
            filler.SetPixel(0, 2, 0, 0, 0, 12);
            filler.SetPixel(0, 3, 0, 0, 0, 24);
            filler.SetPixel(0, 4, 0, 0, 0, 45);
            filler.SetPixel(0, 5, 0, 0, 0, 73);
            filler.SetPixel(0, 6, 0, 0, 0, 109);
            filler.SetPixel(0, 7, 0, 0, 0);
            filler.SetPixel(0, 8, 255, 255, 255);
            filler.SetPixel(0, 9, 0, 0, 0);
            filler.Lock();

            top = new Sprite(this.Viewport, filler);
            top.X = 23;
            top.ZoomX = 0;
            Sprites["top"] = top;
            left = new Sprite(this.Viewport, filler);
            left.Y = 23;
            left.Angle = -90;
            left.ZoomX = 0;
            Sprites["left"] = left;
            right = new Sprite(this.Viewport, filler);
            right.Y = 23;
            right.Angle = 90;
            right.ZoomX = 0;
            Sprites["right"] = right;
            bottom = new Sprite(this.Viewport, filler);
            bottom.X = 23;
            bottom.Angle = 180;
            bottom.ZoomX = 0;
            Sprites["bottom"] = bottom;

            SetSize(46, 46);

            this.SetZIndex(1);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            //(Sprites["outer"] as RectSprite).SetSize(this.Size);
            //(Sprites["middle"] as RectSprite).SetSize(this.Size.Width - 2, this.Size.Height - 2);
            //(Sprites["inner"] as RectSprite).SetSize(this.Size.Width - 4, this.Size.Height - 4);
            topright.X = Size.Width - 23;
            bottomleft.Y = Size.Height - 23;
            bottomright.X = topright.X;
            bottomright.Y = bottomleft.Y;
            top.ZoomX = Size.Width - 46;
            left.ZoomX = Size.Height - 46;
            left.Y = Size.Height - 23;
            right.ZoomX = Size.Height - 46;
            right.X = Size.Width;
            bottom.X = Size.Width - 23;
            bottom.Y = Size.Height;
            bottom.ZoomX = Size.Width - 46;

        }

        public override void Update()
        {
            //Console.WriteLine(ScrolledY.ToString() + " : " + Parent.AdjustedPosition.Y.ToString());
            base.Update();
        }
    }
}
