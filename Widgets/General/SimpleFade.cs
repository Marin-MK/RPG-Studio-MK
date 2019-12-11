using System;
using ODL;

namespace MKEditor.Widgets
{
    public class SimpleFade : Widget
    {
        public SimpleFade(object Parent, string Name = "simpleFade")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport);

            SetZIndex(1);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(this.Size);
            Sprites["bg"].Bitmap.Unlock();

            Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, 0, 3, 3, 109);
            Sprites["bg"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, 0, 3, 3, 109);

            Sprites["bg"].Bitmap.DrawRect(2, 2, Size.Width - 4, Size.Height - 4, 1, 0, 3, 77);
            Sprites["bg"].Bitmap.DrawRect(3, 3, Size.Width - 6, Size.Height - 6, 1, 0, 3, 77);

            Sprites["bg"].Bitmap.DrawRect(4, 4, Size.Width - 8, Size.Height - 8, 0, 3, 3, 55);
            Sprites["bg"].Bitmap.DrawRect(5, 5, Size.Width - 10, Size.Height - 10, 0, 3, 3, 55);

            Sprites["bg"].Bitmap.DrawRect(6, 6, Size.Width - 12, Size.Height - 12, 1, 4, 0, 38);
            Sprites["bg"].Bitmap.DrawRect(7, 7, Size.Width - 14, Size.Height - 14, 1, 4, 0, 38);

            Sprites["bg"].Bitmap.DrawRect(8, 8, Size.Width - 16, Size.Height - 16, 1, 4, 0, 28);
            Sprites["bg"].Bitmap.DrawRect(9, 9, Size.Width - 18, Size.Height - 18, 1, 4, 0, 28);

            Sprites["bg"].Bitmap.DrawRect(10, 10, Size.Width - 20, Size.Height - 20, 4, 1, 0, 21);
            Sprites["bg"].Bitmap.DrawRect(11, 11, Size.Width - 22, Size.Height - 22, 4, 1, 0, 21);

            Sprites["bg"].Bitmap.DrawRect(12, 12, Size.Width - 24, Size.Height - 24, 0, 8, 17, 18);
            Sprites["bg"].Bitmap.DrawRect(13, 13, Size.Width - 26, Size.Height - 26, 0, 8, 17, 18);

            Sprites["bg"].Bitmap.Lock();
        }
    }
}
