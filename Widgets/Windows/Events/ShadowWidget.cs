using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ShadowWidget : Widget
{
    public int Thickness { get; protected set; } = 5;
    public bool Inverted { get; protected set; } = false;
    public byte Darkness { get; protected set; } = 64;

    public ShadowWidget(IContainer Parent) : base(Parent)
    {
        Sprites["left"] = new Sprite(this.Viewport);
        Sprites["top"] = new Sprite(this.Viewport);
        Sprites["right"] = new Sprite(this.Viewport);
        Sprites["bottom"] = new Sprite(this.Viewport);
        Sprites["topleft"] = new Sprite(this.Viewport);
        Sprites["bottomleft"] = new Sprite(this.Viewport);
        Sprites["topright"] = new Sprite(this.Viewport);
        Sprites["bottomright"] = new Sprite(this.Viewport);
        OnSizeChanged += _ => Redraw();
        Redraw();
        Reposition();
    }

    public void SetThickness(int Thickness)
    {
        if (this.Thickness != Thickness)
        {
            this.Thickness = Thickness;
            this.Redraw();
        }
    }

    public void SetInverted(bool Inverted)
    {
        if (this.Inverted != Inverted)
        {
            this.Inverted = Inverted;
            this.Redraw();
        }
    }

    public void SetDarkness(byte Darkness)
    {
        if (this.Darkness != Darkness)
        {
            this.Darkness = Darkness;
            this.Redraw();
        }
    }

    protected override void Draw()
    {
        base.Draw();
        int w = Thickness;
        int h = Thickness;
        int mw = Size.Width - w * 2;
        int mh = Size.Height - h * 2;
        Color c1 = Color.ALPHA;
        Color c2 = new Color(0, 0, 0, Darkness);
        if (Inverted) (c2, c1) = (c1, c2);

        Sprites["topleft"].Bitmap?.Dispose();
        Sprites["left"].Bitmap?.Dispose();
        Sprites["top"].Bitmap?.Dispose();
        
        Sprites["topleft"].Bitmap = new Bitmap(w, h);
        Sprites["topleft"].Bitmap.Unlock();
        Sprites["topleft"].Bitmap.FillGradientRect(new Rect(0, 0, w, h), c1, c1, c1, c2);
        Sprites["topleft"].Bitmap.Lock();

        if (mh > 0)
        {
            Sprites["left"].Bitmap = new Bitmap(w, mh);
            Sprites["left"].Bitmap.Unlock();
            Sprites["left"].Bitmap.FillGradientRect(new Rect(0, 0, w, mh), c1, c2, c1, c2);
            Sprites["left"].Bitmap.Lock();
        }

        if (mw > 0)
        {
            Sprites["top"].Bitmap = new Bitmap(mw, h);
            Sprites["top"].Bitmap.Unlock();
            Sprites["top"].Bitmap.FillGradientRect(new Rect(0, 0, mw, h), c1, c1, c2, c2);
            Sprites["top"].Bitmap.Lock();
        }

        Sprites["right"].Bitmap = Sprites["left"].Bitmap;
        Sprites["right"].MirrorX = true;

        Sprites["bottom"].Bitmap = Sprites["top"].Bitmap;
        Sprites["bottom"].MirrorY = true;

        Sprites["topright"].Bitmap = Sprites["topleft"].Bitmap;
        Sprites["topright"].MirrorX = true;

        Sprites["bottomleft"].Bitmap = Sprites["topleft"].Bitmap;
        Sprites["bottomleft"].MirrorY = true;

        Sprites["bottomright"].Bitmap = Sprites["topleft"].Bitmap;
        Sprites["bottomright"].MirrorX = true;
        Sprites["bottomright"].MirrorY = true;

        Reposition();
    }

    public void ShowTop(bool ShowTop)
    {
        Sprites["top"].Visible = ShowTop;
    }

    public void ShowLeft(bool ShowLeft)
    {
        Sprites["left"].Visible = ShowLeft;
    }

    public void ShowTopLeft(bool ShowTopLeft)
    {
        Sprites["topleft"].Visible = ShowTopLeft;
    }

    public void ShowBottomLeft(bool ShowBottomLeft)
    {
        Sprites["bottomleft"].Visible = ShowBottomLeft;
    }

    public void ShowTopRight(bool ShowTopRight)
    {
        Sprites["topright"].Visible = ShowTopRight;
    }

    public void ShowBottom(bool ShowBottom)
    {
        Sprites["bottom"].Visible = ShowBottom;
    }

    public void ShowRight(bool ShowRight)
    {
        Sprites["right"].Visible = ShowRight;
    }

    public void ShowBottomRight(bool ShowBottomRight)
    {
        Sprites["bottomright"].Visible = ShowBottomRight;
    }
    
    public void Reposition()
    {
        Sprites["top"].X = Thickness;
        Sprites["left"].Y = Thickness;
        Sprites["right"].X = Size.Width - Thickness;
        Sprites["right"].Y = Sprites["left"].Y;
        Sprites["bottom"].X = Sprites["top"].X;
        Sprites["bottom"].Y = Size.Height - Thickness;
        Sprites["topright"].X = Size.Width - Thickness;
        Sprites["bottomleft"].Y = Size.Height - Thickness;
        Sprites["bottomright"].X = Sprites["topright"].X;
        Sprites["bottomright"].Y = Sprites["bottomleft"].Y;
    }
}
