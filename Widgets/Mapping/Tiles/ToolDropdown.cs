using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets;

public class ToolDropdown : Widget
{
    public int HoveringIndex { get; protected set; } = -1;
    public Icon Icon1 { get; protected set; }
    public Icon Icon2 { get; protected set; }

    public ToolDropdown(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["selector"] = new Sprite(this.Viewport);
        Sprites["selector"].Bitmap = new SolidBitmap(32, 32, new Color(28, 50, 73));
        Sprites["selector"].X = 2;
        Sprites["selector"].Visible = false;
        Sprites["icon1"] = new Sprite(this.Viewport);
        Sprites["icon1"].Bitmap = Utilities.IconSheet;
        Sprites["icon1"].DestroyBitmap = false;
        Sprites["icon1"].X = 6;
        Sprites["icon1"].Y = 6;
        Sprites["icon2"] = new Sprite(this.Viewport);
        Sprites["icon2"].Bitmap = Utilities.IconSheet;
        Sprites["icon2"].DestroyBitmap = false;
        Sprites["icon2"].X = 6;
        Sprites["icon2"].Y = 40;
        SetSize(36, 68);
    }

    public void SetIcon1(Icon Icon)
    {
        this.Icon1 = Icon;
        Sprites["icon1"].SrcRect = new Rect((int)Icon * 24, 0, 24, 24);
        this.Redraw();
    }

    public void SetIcon2(Icon Icon)
    {
        this.Icon2 = Icon;
        Sprites["icon2"].SrcRect = new Rect((int)Icon * 24, 0, 24, 24);
        this.Redraw();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(this.Size);
        Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, new Color(10, 23, 37));
        Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(22, 176, 220));
        Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, new Color(10, 23, 37));
        Sprites["box"].Bitmap.Lock();
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["icon1"].SrcRect = new Rect((int)Icon1 * 24, this.HoveringIndex == 0 ? 24 : 0, 24, 24);
        Sprites["icon2"].SrcRect = new Rect((int)Icon2 * 24, this.HoveringIndex == 1 ? 24 : 0, 24, 24);
        if (this.HoveringIndex == -1) Sprites["selector"].Visible = false;
        else
        {
            Sprites["selector"].Visible = true;
            Sprites["selector"].Y = 2 + 32 * HoveringIndex;
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        int OldHoveringIndex = HoveringIndex;
        HoveringIndex = -1;
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (rx >= 2 && rx < 34)
        {
            if (ry >= 2 && ry < 34) HoveringIndex = 0;
            else if (ry >= 34 && ry < 66) HoveringIndex = 1;
        }
        if (OldHoveringIndex != HoveringIndex) this.Redraw();
    }
}
