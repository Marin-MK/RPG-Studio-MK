using System;
using ODL;

namespace MKEditor.Widgets
{
    public class NewLayerButton : Widget
    {
        public NewLayerButton(object Parent, string Name = "newLayerButton")
            : base(Parent, Name)
        {
            SetSize(85, 36);
            Sprites["shadow"] = new Sprite(this.Viewport);
            Bitmap b = new Bitmap(Size);
            #region Draw shadow
            b.Unlock();
            b.DrawLine(1, 1, 1, 3, new Color(0, 22, 45, 9));
            b.DrawLine(1, 1, 3, 1, new Color(0, 22, 45, 9));
            b.DrawLine(2, 0, Size.Width - 7, 0, new Color(0, 22, 45, 9));
            b.DrawLine(0, 2, 0, Size.Height - 7, new Color(0, 22, 45, 9));
            b.DrawLine(1, Size.Height - 2, 1, Size.Height - 8, new Color(0, 22, 45, 9));
            b.DrawLine(1, Size.Height - 2, 3, Size.Height - 6, new Color(0, 22, 45, 9));
            b.DrawLine(2, Size.Height - 5, Size.Width - 3, Size.Height - 5, new Color(0, 22, 45, 9));
            b.DrawLine(Size.Width - 2, Size.Height - 6, Size.Width - 4, Size.Height - 6, new Color(0, 22, 45, 9));
            b.DrawLine(Size.Width - 2, Size.Height - 6, Size.Width - 2, Size.Height - 8, new Color(0, 22, 45, 9));
            b.DrawLine(Size.Width - 4, 1, Size.Width - 2, 1, new Color(0, 22, 45, 9));
            b.DrawLine(Size.Width - 2, 1, Size.Width - 2, 3, new Color(0, 22, 45, 9));
            b.DrawLine(Size.Width - 1, 2, Size.Width - 1, Size.Height - 7, new Color(0, 22, 45, 9));

            b.SetPixel(2, 2, 0, 8, 17, 18);
            b.DrawLine(1, 4, 1, Size.Height - 9, 0, 8, 17, 18);
            b.DrawLine(4, 1, Size.Width - 5, 1, 0, 8, 17, 18);
            b.SetPixel(2, Size.Height - 7, 0, 8, 17, 18);
            b.DrawLine(4, Size.Height - 6, Size.Width - 5, Size.Height - 6, 0, 8, 17, 18);
            b.SetPixel(Size.Width - 3, 2, 0, 8, 17, 18);
            b.SetPixel(Size.Width - 3, Size.Height - 7, 0, 8, 17, 18);
            b.DrawLine(Size.Width - 2, 4, Size.Width - 1, Size.Height - 9, 0, 8, 17, 18);

            b.SetPixel(3, 2, 3, 0, 6, 31);
            b.SetPixel(2, 3, 3, 0, 6, 31);
            b.SetPixel(Size.Width - 4, 2, 3, 0, 6, 31);
            b.SetPixel(Size.Width - 3, 3, 3, 0, 6, 31);
            b.SetPixel(3, Size.Height - 7, 3, 0, 6, 31);
            b.SetPixel(2, Size.Height - 8, 3, 0, 6, 31);
            b.SetPixel(Size.Width - 4, Size.Height - 7, 3, 0, 6, 31);
            b.SetPixel(Size.Width - 3, Size.Height - 8, 3, 0, 6, 31);

            b.SetPixel(4, 2, 0, 5, 6, 46);
            b.SetPixel(2, 4, 0, 5, 6, 46);
            b.SetPixel(Size.Width - 5, 2, 0, 5, 6, 46);
            b.SetPixel(Size.Width - 3, 4, 0, 5, 6, 46);
            b.SetPixel(4, Size.Height - 7, 0, 5, 6, 46);
            b.SetPixel(2, Size.Height - 9, 0, 5, 6, 46);
            b.SetPixel(Size.Width - 5, Size.Height - 7, 0, 5, 6, 46);
            b.SetPixel(Size.Width - 3, Size.Height - 8, 0, 5, 6, 46);

            b.DrawLine(5, 2, Size.Width - 6, 2, 0, 3, 8, 55);
            b.DrawLine(2, 5, 2, Size.Height - 10, 0, 3, 8, 55);
            b.DrawLine(5, Size.Height - 7, Size.Width - 6, Size.Height - 7, 0, 3, 8, 55);
            b.DrawLine(Size.Width - 3, 5, Size.Width - 3, Size.Height - 10, 0, 3, 8, 55);
            b.Lock();
            #endregion
            Sprites["shadow"].Bitmap = b;

            Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(79, 26, new Color(64, 104, 146)));
            Sprites["bg"].X = 3;
            Sprites["bg"].Y = 3;

            Sprites["text"] = new Sprite(this.Viewport);
            Font f = Font.Get("Fonts/ProductSans-M", 12);
            Size s = f.TextSize("New Layer");
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText("New Layer", Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            Sprites["text"].Y = 8;

            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(85, 2, new Color(59, 227, 255)));
            Sprites["selector"].Y = 33;
            Sprites["selector"].Visible = false;

            WidgetIM.OnMouseMoving += MouseMoving;
            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseDown += MouseDown;
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering) return;
            int ry = e.Y - Viewport.Y;
            Sprites["selector"].Visible = ry < 32;
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (!WidgetIM.Hovering) Sprites["selector"].Visible = false;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (Sprites["selector"].Visible)
            {
                (Parent.Parent.Parent as LayersTab).NewLayer(sender, new EventArgs());
            }
        }
    }
}
