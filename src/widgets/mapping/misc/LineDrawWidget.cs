using System;

namespace RPGStudioMK.Widgets;

public class LineDrawWidget : Widget
{
    public LineDrawWidget(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
    }

    public void DrawLine(Point p1, Point p2, Color InnerColor, Color OuterColor)
    {
        Size s = (p2 - p1).Abs().ToSize();
        if (s.Width < 1) s.Width = 1;
        if (s.Height < 1) s.Height = 1;
        int minx = Math.Min(p1.X, p2.X);
        int maxx = p1.X == p2.X ? minx : Math.Max(p1.X, p2.X) - minx - 1;
        int miny = Math.Min(p1.Y, p2.Y);
        int maxy = p1.Y == p2.Y ? miny : Math.Max(p1.Y, p2.Y) - miny - 1;
        // Both points will get a small rectangle drawn on them, so increase our width/height
        s.Width += 2;
        s.Height += 2;
        SetSize(s.Width + minx, s.Height + miny);
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(s);
        Sprites["bg"].Bitmap.Unlock();
        int p1x = Math.Min(p1.X - minx, maxx) + 1;
        int p1y = Math.Min(p1.Y - miny, maxy) + 1;
        int p2x = Math.Min(p2.X - minx, maxx) + 1;
        int p2y = Math.Min(p2.Y - miny, maxy) + 1;
        Sprites["bg"].Bitmap.FillRect(p1x - 1, p1y - 1, 3, 3, OuterColor);
        Sprites["bg"].Bitmap.FillRect(p2x - 1, p2y - 1, 3, 3, OuterColor);
        Sprites["bg"].Bitmap.DrawLine(p1x, p1y, p2x, p2y, InnerColor);
        if (p2x > p1x)
        {
            if (p2y > p1y)
            {
                Sprites["bg"].Bitmap.DrawLine(p1x + 1, p1y    , p2x    , p2y - 1, InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y + 1, p2x - 1, p2y    , InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x + 2, p1y    , p2x    , p2y - 2, OuterColor);
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y + 2, p2x - 2, p2y    , OuterColor);
            }
            else if (p2y < p1y)
            {
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y - 1, p2x - 1, p2y    , InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x + 1, p1y    , p2x    , p2y + 1, InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y - 2, p2x - 2, p2y    , OuterColor);
                Sprites["bg"].Bitmap.DrawLine(p1x + 2, p1y    , p2x    , p2y + 2, OuterColor);
            }
            else Sprites["bg"].Bitmap.DrawRect(s, OuterColor);
        }
        else if (p2x < p1x)
        {
            if (p2y > p1y)
            {
                Sprites["bg"].Bitmap.DrawLine(p1x - 1, p1y    , p2x    , p2y - 1, InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y + 1, p2x + 1, p2y    , InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x - 2, p1y    , p2x    , p2y - 2, OuterColor);
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y + 2, p2x + 2, p2y    , OuterColor);
            }
            else if (p2y < p1y)
            {
                Sprites["bg"].Bitmap.DrawLine(p1x - 1, p1y    , p2x    , p2y + 1, InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y - 1, p2x + 1, p2y    , InnerColor);
                Sprites["bg"].Bitmap.DrawLine(p1x - 2, p1y    , p2x    , p2y + 2, OuterColor);
                Sprites["bg"].Bitmap.DrawLine(p1x    , p1y - 2, p2x + 2, p2y    , OuterColor);
            }
            else Sprites["bg"].Bitmap.DrawRect(s, OuterColor);
        }
        else Sprites["bg"].Bitmap.DrawRect(s, OuterColor);
        Sprites["bg"].Bitmap.Lock();
        Sprites["bg"].X = minx;
        Sprites["bg"].Y = miny;
    }
}
