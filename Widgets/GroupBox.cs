using System;
using ODL;

namespace MKEditor.Widgets
{
    public class GroupBox : Container
    {
        public string Text { get; protected set; } = "";

        public GroupBox(object Parent)
            : base(Parent, "groupBox")
        {
            this.Text = this.Name;
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 8;
            this.Sprites["text"].Y = -3;
            this.Sprites["text"].Z = 2;
            this.Sprites["rect"] = new RectSprite(this.Viewport);
            this.Sprites["rect"].Y = 6;
            this.Sprites["line"] = new Sprite(this.Viewport);
            this.Sprites["line"].Bitmap = new SolidBitmap(1, 1);
            this.Sprites["line"].X = 6;
            this.Sprites["line"].Y = 6;
            this.Sprites["line"].Z = 1;
        }

        public Widget SetText(string text)
        {
            if (this.Text != text)
            {
                this.Text = text;
                Redraw();
            }
            return this;
        }

        protected override void Draw()
        {
            Font f = Font.Get("Fonts/Segoe UI", 12);
            Size s = f.TextSize(this.Text);
            if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
            this.Sprites["text"].Bitmap = new Bitmap(s);
            this.Sprites["text"].Bitmap.Font = f;
            this.Sprites["text"].Bitmap.Unlock();
            this.Sprites["text"].Bitmap.DrawText(this.Text, 0, 0, Color.BLACK);
            this.Sprites["text"].Bitmap.Lock();

            RectSprite r = Sprites["rect"] as RectSprite;
            r.SetSize(this.Size.Width, this.Size.Height - 6);
            r.SetOuterColor(220, 220, 220);
            this.Sprites["line"].Bitmap.Unlock();
            (this.Sprites["line"].Bitmap as SolidBitmap).SetSize(s.Width + 4, 1);
            (this.Sprites["line"].Bitmap as SolidBitmap).SetColor(240, 240, 240);
            this.Sprites["line"].Bitmap.Lock();

            base.Draw();
        }
    }
}
