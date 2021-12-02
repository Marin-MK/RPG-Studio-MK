using System;
using System.Linq;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets;

public class VScrollBar : BasicVScrollBar
{
    public VScrollBar(IContainer Parent) : base(Parent)
    {
        this.SetSliderWidth(10);
        this.Arrow1Size = 0;
        this.Arrow2Size = 0;
    }

    protected override void Draw()
    {
        Color sc = SliderDragging || SliderHovering ? new Color(59, 227, 255) : new Color(64, 104, 146);
        Sprites["slider"].Bitmap?.Dispose();
        Sprites["slider"].Bitmap = new Bitmap(SliderWidth, RealSliderHeight);
        Sprites["slider"].Bitmap.Unlock();
        Sprites["slider"].Bitmap.FillRect(SliderWidth - 2, RealSliderHeight, sc);
        Sprites["slider"].Bitmap.DrawLine(SliderWidth - 3, 0, SliderWidth - 3, RealSliderHeight - 1, Color.BLACK);
        Sprites["slider"].Bitmap.DrawLine(0, RealSliderHeight - 1, SliderWidth - 3, RealSliderHeight - 1, Color.BLACK);
        Sprites["slider"].Bitmap.Lock();
        Sprites["slider"].Y = Arrow1Size + RealSliderPosition;
        base.Draw();
    }
}
