using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class GroupBoxWithScrollBars : Widget
{
    public GroupBoxWithScrollBars(IContainer Parent) : base(Parent)
    {
        Sprites["gfxbox"] = new Sprite(Viewport);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        Color outline = new Color(59, 91, 124);
        Color inline = new Color(17, 27, 38);
        Color filler = new Color(24, 38, 53);
        base.SizeChanged(e);
        Sprites["gfxbox"].Bitmap?.Dispose();
        Sprites["gfxbox"].Bitmap = new Bitmap(Size);
        Sprites["gfxbox"].Bitmap.Unlock();
        Sprites["gfxbox"].Bitmap.DrawRect(Size, outline);
        Sprites["gfxbox"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, inline);
        Sprites["gfxbox"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, filler);
        Sprites["gfxbox"].Bitmap.FillRect(Size.Width - 14, Size.Height - 14, 13, 13, outline);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 15, 1, Size.Width - 15, Size.Height - 15, inline);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 14, 1, Size.Width - 14, Size.Height - 15, outline);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 13, 1, Size.Width - 13, Size.Height - 15, inline);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 12, Size.Height - 15, Size.Width - 3, Size.Height - 15, inline);
        Sprites["gfxbox"].Bitmap.DrawLine(1, Size.Height - 15, Size.Width - 15, Size.Height - 15, inline);
        Sprites["gfxbox"].Bitmap.DrawLine(1, Size.Height - 14, Size.Width - 15, Size.Height - 14, outline);
        Sprites["gfxbox"].Bitmap.DrawLine(1, Size.Height - 13, Size.Width - 15, Size.Height - 13, inline);
        Sprites["gfxbox"].Bitmap.DrawLine(Size.Width - 15, Size.Height - 12, Size.Width - 15, Size.Height - 3, inline);
        Sprites["gfxbox"].Bitmap.Lock();
    }
}
