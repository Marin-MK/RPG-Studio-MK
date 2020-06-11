using System;
using System.Linq;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class HScrollBar : BasicHScrollBar
    {
        public HScrollBar(IContainer Parent) : base(Parent)
        {
            this.SetSliderHeight(10);
            this.Arrow1Size = 0;
            this.Arrow2Size = 0;
        }

        protected override void Draw()
        {
            Color sc = SliderDragging || SliderHovering ? new Color(59, 227, 255) : new Color(64, 104, 146);
            Sprites["slider"].Bitmap?.Dispose();
            Sprites["slider"].Bitmap = new Bitmap(RealSliderWidth, SliderHeight);
            Sprites["slider"].Bitmap.Unlock();
            Sprites["slider"].Bitmap.FillRect(RealSliderWidth, SliderHeight - 2, sc);
            Sprites["slider"].Bitmap.DrawLine(0, SliderHeight - 3, RealSliderWidth - 1, SliderHeight - 3, Color.BLACK);
            Sprites["slider"].Bitmap.DrawLine(RealSliderWidth - 1, 0, RealSliderWidth - 1, SliderHeight - 3, Color.BLACK);
            Sprites["slider"].Bitmap.Lock();
            Sprites["slider"].X = Arrow1Size + RealSliderPosition;
            base.Draw();
        }
    }
}