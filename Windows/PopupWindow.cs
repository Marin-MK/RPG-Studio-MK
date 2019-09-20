using System;
using ODL;

namespace MKEditor.Widgets
{
    public class PopupWindow : Widget
    {
        public bool Blocked = false;
        public string DisplayName { get; protected set; }

        public PopupWindow(object Parent, string Name = "popupWindow")
            : base(Parent, Name)
        {
            Window.Blocked = true;
            Window.SetOverlayOpacity(128);
            // The black overlay over the main window has its own viewport with z = 1000.
            // So we have to go one higher to make it ignore this window in the overlay darkening.
            this.SetZIndex(1001);
            Sprites["window"] = new RectSprite(this.Viewport, new Size(this.Size.Width - 14, this.Size.Height - 14),
                new Color(59, 227, 255), new Color(40, 62, 84));
            Sprites["shadow_bottom"] = new Sprite(this.Viewport, new SolidBitmap(new Size(this.Size.Width - 14, 14), new Color(0, 0, 0, 55)));
            Sprites["shadow_bottom"].X = 14;
            Sprites["shadow_bottom"].Y = this.Size.Height - 14;
            Sprites["shadow_right"] = new Sprite(this.Viewport, new SolidBitmap(new Size(14, this.Size.Height - 28), new Color(0, 0, 0, 55)));
            Sprites["shadow_right"].X = this.Size.Width - 14;
            Sprites["shadow_right"].Y = 14;
            Sprites["name"] = new Sprite(this.Viewport);
            Sprites["name"].X = 5;
            Sprites["name"].Y = 3;
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (Sprites["window"] as RectSprite).SetSize(this.Size.Width - 14, this.Size.Height - 14);
            
            (Sprites["shadow_bottom"].Bitmap as SolidBitmap).SetSize(this.Size.Width - 14, 14);
            Sprites["shadow_bottom"].Y = this.Size.Height - 14;
            
            (Sprites["shadow_right"].Bitmap as SolidBitmap).SetSize(14, this.Size.Height - 28);
            Sprites["shadow_right"].X = this.Size.Width - 14;
        }

        public override void ParentSizeChanged(object sender, SizeEventArgs e)
        {
            base.ParentSizeChanged(sender, e);
            Center();
        }

        public void Center()
        {
            int width = Window.Width;
            int height = Window.Height;
            this.SetPosition(width / 2 - (this.Size.Width - 14) / 2, height / 2 - (this.Size.Height - 14) / 2);
        }

        public void SetName(string Name)
        {
            this.DisplayName = Name;
            Font f = Font.Get("Fonts/ProductSans-B", 14);
            Size s = f.TextSize(Name);
            if (Sprites["name"].Bitmap != null) Sprites["name"].Bitmap.Dispose();
            Sprites["name"].Bitmap = new Bitmap(s);
            Sprites["name"].Bitmap.Unlock();
            Sprites["name"].Bitmap.Font = f;
            Sprites["name"].Bitmap.DrawText(Name, Color.WHITE);
            Sprites["name"].Bitmap.Lock();
        }
    }
}
