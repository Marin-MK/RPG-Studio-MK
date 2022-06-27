using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class GroupBoxWithScrollBars : Widget
{
    public Color OutlineColor { get; protected set; } = new Color(59, 91, 124);
    public Color InlineColor { get; protected set; } = new Color(17, 27, 38);
    public Color FillerColor { get; protected set; } = new Color(24, 38, 53);
    public Color ScrollBarFillerColor { get; protected set; } = new Color(24, 38, 53);

    public GroupBoxWithScrollBars(IContainer Parent) : base(Parent)
    {
        Sprites["gfxbox"] = new Sprite(Viewport);
    }

    public void SetOutlineColor(Color Color)
    {
        if (this.OutlineColor != Color)
        {
            this.OutlineColor = Color;
            Redraw();
        }
    }

    public void SetInlineColor(Color Color)
    {
        if (this.InlineColor != Color)
        {
            this.InlineColor = Color;
            Redraw();
        }
    }

    public void SetFillerColor(Color Color)
    {
        if (this.FillerColor != Color)
        {
            this.FillerColor = Color;
            Redraw();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Redraw();
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["gfxbox"].Bitmap?.Dispose();
        if (Size.Width < 16 || Size.Height < 16) return;
        Sprites["gfxbox"].Bitmap = new Bitmap(Size);
        Sprites["gfxbox"].Bitmap.Unlock();
        Sprites["gfxbox"].Bitmap.DrawRect(Size, OutlineColor);
        Sprites["gfxbox"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, InlineColor);
        Sprites["gfxbox"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, FillerColor);
        Sprites["gfxbox"].Bitmap.FillRect(Size.Width - 12, 2, 10, Size.Height - 15, ScrollBarFillerColor);
        Sprites["gfxbox"].Bitmap.FillRect(2, Size.Height - 12, Size.Width - 15, 10, ScrollBarFillerColor);
        Sprites["gfxbox"].Bitmap.FillRect(Size.Width - 14, Size.Height - 14, 13, 13, OutlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 15, 1, Size.Width - 15, Size.Height - 15, InlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 14, 1, Size.Width - 14, Size.Height - 15, OutlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 13, 1, Size.Width - 13, Size.Height - 15, InlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 12, Size.Height - 15, Size.Width - 3, Size.Height - 15, InlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(1, Size.Height - 15, Size.Width - 15, Size.Height - 15, InlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(1, Size.Height - 14, Size.Width - 15, Size.Height - 14, OutlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(1, Size.Height - 13, Size.Width - 15, Size.Height - 13, InlineColor);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 15, Size.Height - 12, Size.Width - 15, Size.Height - 3, InlineColor);
        Sprites["gfxbox"].Bitmap.Lock();
    }
}
