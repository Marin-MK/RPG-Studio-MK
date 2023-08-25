using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class GradientBox : Widget
{
    public Color TopLeftColor { get; protected set; }
    public Color TopRightColor { get; protected set; }
    public Color BottomLeftColor { get; protected set; }
    public Color BottomRightColor { get; protected set; }

    public GradientBox(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        OnSizeChanged += _ => Redraw();
    }

    public void SetTopLeftColor(Color Color)
    {
        if (this.TopLeftColor != Color)
        {
            this.TopLeftColor = Color;
            this.Redraw();
        }
    }

    public void SetTopRightColor(Color Color)
    {
        if (this.TopRightColor != Color)
        {
            this.TopRightColor = Color;
            this.Redraw();
        }
    }

    public void SetBottomLeftColor(Color Color)
    {
        if (this.BottomLeftColor != Color)
        {
            this.BottomLeftColor = Color;
            this.Redraw();
        }
    }

    public void SetBottomRightColor(Color Color)
    {
        if (this.BottomRightColor != Color)
        {
            this.BottomRightColor = Color;
            this.Redraw();
        }
    }

    public void SetBottomColor(Color Color)
    {
        if (this.BottomLeftColor != Color || this.BottomRightColor != Color)
        {
            this.BottomLeftColor = Color;
            this.BottomRightColor = Color;
            this.Redraw();
        }
    }

    public void SetTopColor(Color Color)
    {
        if (this.TopLeftColor != Color || this.TopRightColor != Color)
        {
            this.TopLeftColor = Color;
            this.TopRightColor = Color;
            this.Redraw();
        }
    }

    public void SetLeftColor(Color Color)
    {
        if (this.TopLeftColor != Color || this.BottomLeftColor != Color)
        {
            this.TopLeftColor = Color;
            this.BottomLeftColor = Color;
            this.Redraw();
        }
    }

    public void SetRightColor(Color Color)
    {
        if (this.TopRightColor != Color || this.BottomRightColor != Color)
        {
            this.TopRightColor = Color;
            this.BottomRightColor = Color;
            this.Redraw();
        }
    }

    public void SetColor(Color Color)
    {
        if (this.TopRightColor != Color || this.TopLeftColor != Color || this.BottomLeftColor != Color || this.BottomRightColor != Color)
        {
            this.TopLeftColor = Color;
            this.TopRightColor = Color;
            this.BottomRightColor = Color;
            this.BottomLeftColor = Color;
            this.Redraw();
        }
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["box"].Bitmap?.Dispose();
        if (TopLeftColor == null && TopRightColor == null) return;
        if (BottomLeftColor == null && BottomRightColor == null) return;
        Color c1 = TopLeftColor ?? TopRightColor;
        Color c2 = TopRightColor ?? TopLeftColor;
        Color c3 = BottomLeftColor ?? c2;
        Color c4 = BottomRightColor ?? c3;
        Sprites["box"].Bitmap = new Bitmap(Size);
        Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.FillGradientRect(0, 0, Size.Width, Size.Height, c1, c2, c3, c4);
        Sprites["box"].Bitmap.Lock();
    }
}
