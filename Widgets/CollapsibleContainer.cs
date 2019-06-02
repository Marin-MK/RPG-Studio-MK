using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CollapsibleContainer : Container
    {
        public string Text { get; protected set; }

        public CollapsibleContainer(object Parent, string Name = "collapsibleContainer")
            : base(Parent, Name)
        {
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 20;
            this.Sprites["line"] = new Sprite(this.Viewport);
            this.Sprites["line"].X = 20;
            this.Sprites["line"].Y = 19;
            this.Sprites["line"].Bitmap = new SolidBitmap(1, 1, new Color(127, 127, 127));
            this.Sprites["arrow"] = new Sprite(this.Viewport);
            this.Sprites["arrow"].Y = 3;
        }

        public Widget SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                this.Redraw();
            }
            return this;
        }

        protected override void Draw()
        {
            if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
            if (this.Text != null)
            {
                Font f = Font.Get("Fonts/Quicksand Bold", 16);
                Size s = f.TextSize(this.Text);
                this.Sprites["text"].Bitmap = new Bitmap(s);
                this.Sprites["text"].Bitmap.Unlock();
                this.Sprites["text"].Bitmap.Font = f;
                this.Sprites["text"].Bitmap.DrawText(this.Text, Color.WHITE);
                this.Sprites["text"].Bitmap.Lock();
            }

            this.Sprites["line"].Bitmap.Unlock();
            (this.Sprites["line"].Bitmap as SolidBitmap).SetSize(this.Size.Width - 20, 1);
            this.Sprites["line"].Bitmap.Lock();

            base.Draw();
        }
    }
}
