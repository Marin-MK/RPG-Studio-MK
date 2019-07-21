using System;
using ODL;

namespace MKEditor.Widgets
{
    public class Label : Widget
    {
        public string Text { get; protected set; }
        public Font Font { get; protected set; }
        public Color TextColor { get; protected set; } = Color.WHITE;
        public DrawOptions DrawOptions { get; protected set; }

        public Label(object Parent, string Name = "label")
            : base(Parent, Name)
        {
            Sprites["text"] = new Sprite(this.Viewport);
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                Redraw();
            }
        }

        public void SetFont(Font f)
        {
            if (this.Font != f)
            {
                this.Font = f;
                Redraw();
            }
        }

        public void SetTextColor(Color c)
        {
            if (this.TextColor != c)
            {
                this.TextColor = c;
                Redraw();
            }
        }

        public void SetDrawOptions(DrawOptions o)
        {
             if (this.DrawOptions != o)
            {
                this.DrawOptions = o;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Size s = this.Font.TextSize(this.Text);
            this.SetSize(s);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            Sprites["text"].Bitmap.DrawText(this.Text, this.TextColor);
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }
    }
}
