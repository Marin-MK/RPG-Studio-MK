using System;
using System.Collections.Generic;
using System.Text;
using amethyst;
using odl;

namespace RPGStudioMK.Widgets
{
    public class DropdownBox : amethyst.DropdownBox
    {
        public DropdownBox(IContainer Parent) : base(Parent)
        {
            TextArea.SetPosition(6, 4);
            TextArea.SetFont(Font.Get("Fonts/ProductSans-M", 14));
            TextArea.SetCaretColor(Color.WHITE);
        }

        protected override void Draw()
        {
            Sprites["bg"].Bitmap?.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            if (this.ReadOnly)
            {
                Color outline = new Color(10, 23, 37);
                Color filler = this.Enabled ? new Color(50, 86, 120) : new Color(86, 108, 134);
                Sprites["bg"].Bitmap.DrawRect(Size, outline);
                Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
                Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
                Sprites["bg"].Bitmap.DrawLine(1, Size.Height - 2, Size.Width - 2, Size.Height - 2, outline);
                Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 3, filler);
                Sprites["bg"].Bitmap.SetPixel(1, 1, outline);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, outline);
                Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 3, outline);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 3, outline);
                int x = Size.Width - 22;
                int y = (int) Math.Floor(Size.Height / 2d) - 5;
                filler = new Color(179, 180, 181);
                Sprites["bg"].Bitmap.DrawLine(x + 1, y, x + 13, y, outline);
                Sprites["bg"].Bitmap.DrawLine(x, y + 1, x + 7, y + 8, outline);
                Sprites["bg"].Bitmap.DrawLine(x + 1, y + 3, x + 7, y + 9, outline);
                Sprites["bg"].Bitmap.DrawLine(x + 8, y + 7, x + 14, y + 1, outline);
                Sprites["bg"].Bitmap.DrawLine(x + 8, y + 8, x + 13, y + 3, outline);
                Sprites["bg"].Bitmap.DrawLine(x + 1, y + 1, x + 13, y + 1, filler);
                Sprites["bg"].Bitmap.DrawLine(x + 2, y + 2, x + 12, y + 2, filler);
                Sprites["bg"].Bitmap.DrawLine(x + 3, y + 3, x + 11, y + 3, filler);
                Sprites["bg"].Bitmap.DrawLine(x + 4, y + 4, x + 10, y + 4, filler);
                Sprites["bg"].Bitmap.DrawLine(x + 5, y + 5, x + 9, y + 5, filler);
                Sprites["bg"].Bitmap.DrawLine(x + 6, y + 6, x + 8, y + 6, filler);
                Sprites["bg"].Bitmap.SetPixel(x + 7, y + 7, filler);
            }
            else
            {
                Color lightgrey = new Color(121, 121, 122);
                Color darkgrey = new Color(96, 100, 100);
                Color filler = new Color(10, 23, 37);
                Sprites["bg"].Bitmap.DrawRect(Size, lightgrey);
                Sprites["bg"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, lightgrey);
                Sprites["bg"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, filler);
                Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
                Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
                Sprites["bg"].Bitmap.SetPixel(0, 1, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(1, 0, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 1, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 0, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 2, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 1, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 2, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 1, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(2, 2, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 3, 2, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(2, Size.Height - 3, darkgrey);
                Sprites["bg"].Bitmap.SetPixel(Size.Width - 3, Size.Height - 3, darkgrey);
                int x = Size.Width - 21;
                int y = (int) Math.Floor(Size.Height / 2d) - 2;
                Sprites["bg"].Bitmap.SetPixel(x + 0, y + 0, 100, 107, 114);
                Sprites["bg"].Bitmap.SetPixel(x + 0, y + 1, 92, 99, 107);
                Sprites["bg"].Bitmap.SetPixel(x + 1, y + 0, 169, 171, 173);
                Sprites["bg"].Bitmap.SetPixel(x + 1, y + 1, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 1, y + 2, 72, 80, 90);
                Sprites["bg"].Bitmap.SetPixel(x + 2, y + 0, 72, 80, 90);
                Sprites["bg"].Bitmap.SetPixel(x + 2, y + 1, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 2, y + 2, 171, 173, 174);
                Sprites["bg"].Bitmap.SetPixel(x + 2, y + 3, 37, 48, 60);
                Sprites["bg"].Bitmap.SetPixel(x + 3, y + 1, 115, 120, 126);
                Sprites["bg"].Bitmap.SetPixel(x + 3, y + 2, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 3, y + 3, 150, 153, 156);
                Sprites["bg"].Bitmap.SetPixel(x + 3, y + 4, 16, 29, 42);
                Sprites["bg"].Bitmap.SetPixel(x + 4, y + 1, 17, 29, 43);
                Sprites["bg"].Bitmap.SetPixel(x + 4, y + 2, 152, 155, 158);
                Sprites["bg"].Bitmap.SetPixel(x + 4, y + 3, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 4, y + 4, 113, 119, 125);
                Sprites["bg"].Bitmap.SetPixel(x + 5, y + 2, 40, 51, 62);
                Sprites["bg"].Bitmap.SetPixel(x + 5, y + 3, 173, 174, 175);
                Sprites["bg"].Bitmap.SetPixel(x + 5, y + 4, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 5, y + 5, 70, 79, 88);
                Sprites["bg"].Bitmap.SetPixel(x + 6, y + 3, 81, 89, 97);
                Sprites["bg"].Bitmap.SetPixel(x + 6, y + 4, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 6, y + 5, 170, 171, 173);
                Sprites["bg"].Bitmap.SetPixel(x + 6, y + 6, 31, 43, 55);
                Sprites["bg"].Bitmap.SetPixel(x + 7, y + 3, 129, 133, 138);
                Sprites["bg"].Bitmap.SetPixel(x + 7, y + 4, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 7, y + 5, 144, 147, 151);
                Sprites["bg"].Bitmap.SetPixel(x + 7, y + 6, 15, 27, 41);
                Sprites["bg"].Bitmap.SetPixel(x + 8, y + 2, 88, 96, 104);
                Sprites["bg"].Bitmap.SetPixel(x + 8, y + 3, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 8, y + 4, 166, 168, 170);
                Sprites["bg"].Bitmap.SetPixel(x + 8, y + 5, 30, 42, 54);
                Sprites["bg"].Bitmap.SetPixel(x + 9, y + 1, 58, 67, 78);
                Sprites["bg"].Bitmap.SetPixel(x + 9, y + 2, 177, 178, 179);
                Sprites["bg"].Bitmap.SetPixel(x + 9, y + 3, 177, 178, 179);
                Sprites["bg"].Bitmap.SetPixel(x + 9, y + 4, 56, 66, 76);
                Sprites["bg"].Bitmap.SetPixel(x + 10, y + 0, 31, 43, 55);
                Sprites["bg"].Bitmap.SetPixel(x + 10, y + 1, 167, 168, 170);
                Sprites["bg"].Bitmap.SetPixel(x + 10, y + 2, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 10, y + 3, 91, 98, 106);
                Sprites["bg"].Bitmap.SetPixel(x + 11, y + 0, 145, 149, 152);
                Sprites["bg"].Bitmap.SetPixel(x + 11, y + 1, 179, 180, 181);
                Sprites["bg"].Bitmap.SetPixel(x + 11, y + 2, 129, 133, 138);
                Sprites["bg"].Bitmap.SetPixel(x + 12, y + 0, 173, 174, 175);
                Sprites["bg"].Bitmap.SetPixel(x + 12, y + 1, 154, 157, 160);
                Sprites["bg"].Bitmap.SetPixel(x + 12, y + 2, 21, 33, 46);
                Sprites["bg"].Bitmap.SetPixel(x + 13, y + 0, 28, 40, 52);
                Sprites["bg"].Bitmap.SetPixel(x + 13, y + 1, 19, 31, 44);
            }
            Sprites["bg"].Bitmap.Lock();
            base.Draw();
        }
    }
}
