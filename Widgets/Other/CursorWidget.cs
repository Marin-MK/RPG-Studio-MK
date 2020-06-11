using System;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class CursorWidget : Widget
    {
        public CursorWidget(IContainer Parent) : base(Parent)
        {
            Sprites["rect"] = new Sprite(this.Viewport);

            SetSize(32, 32);
            SetZIndex(1);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            if (Sprites["rect"].Bitmap != null) Sprites["rect"].Bitmap.Dispose();
            Sprites["rect"].Bitmap = new Bitmap(Size);
            Sprites["rect"].Bitmap.Unlock();
            Sprites["rect"].Bitmap.DrawRect(7, 7, Size.Width - 14, Size.Height - 14, Color.BLACK);
            Sprites["rect"].Bitmap.DrawRect(8, 8, Size.Width - 16, Size.Height - 16, Color.WHITE);
            Sprites["rect"].Bitmap.DrawRect(9, 9, Size.Width - 18, Size.Height - 18, Color.BLACK);
            Sprites["rect"].Bitmap.Lock();
        }
    }
}
