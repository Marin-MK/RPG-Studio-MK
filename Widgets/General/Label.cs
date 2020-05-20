using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class Label : Widget
    {
        public string Text { get; protected set; }
        public Font Font { get; protected set; }
        public Color TextColor { get; protected set; } = Color.WHITE;
        public DrawOptions DrawOptions { get; protected set; }
        public bool Enabled { get; protected set; } = true;

        public Label(IContainer Parent) : base(Parent)
        {
            Sprites["text"] = new Sprite(this.Viewport);
            this.Font = Font.Get("Fonts/ProductSans-M", 12);
        }

        public void SetEnabled(bool Enabled)
        {
            if (this.Enabled != Enabled)
            {
                this.Enabled = Enabled;
                RedrawText();
            }
        }

        public virtual void SetText(string Text, DrawOptions DrawOptions = ODL.DrawOptions.LeftAlign)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                this.DrawOptions = DrawOptions;
                RedrawText();
            }
        }

        public void SetDrawOptions(DrawOptions DrawOptions = ODL.DrawOptions.LeftAlign)
        {
            if (this.DrawOptions != DrawOptions)
            {
                this.DrawOptions = DrawOptions;
                RedrawText();
            }
        }

        public void SetFont(Font f)
        {
            if (this.Font != f)
            {
                this.Font = f;
                RedrawText();
            }
        }

        public void SetTextColor(Color c)
        {
            if (this.TextColor != c)
            {
                this.TextColor = c;
                RedrawText();
            }
        }

        public virtual void RedrawText()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            if (string.IsNullOrEmpty(this.Text)) return;
            Size s = this.Font.TextSize(this.Text, this.DrawOptions);
            this.SetSize(s);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            Sprites["text"].Bitmap.DrawText(this.Text, this.Enabled ? this.TextColor : new Color(72, 72, 72), this.DrawOptions);
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }

        public override object GetValue(string Identifier)
        {
            if (Identifier == "enabled") return this.Enabled;
            else if (string.IsNullOrEmpty(Identifier)) return this.Text;
            return base.GetValue(Identifier);
        }

        public override void SetValue(string Identifier, object Value)
        {
            if (Identifier == "enabled") this.SetEnabled((string) Value == "true");
            else if (string.IsNullOrEmpty(Identifier)) this.SetText((string) Value);
            else base.SetValue(Identifier, Value);
        }
    }

    public class MultilineLabel : Label
    {
        public MultilineLabel(IContainer Parent) : base(Parent)
        {
            
        }

        public override void RedrawText()
        {
            if (string.IsNullOrEmpty(Text)) return;
            List<string> Lines = Utilities.FormatString(this.Font, Text, Size.Width);
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            SetSize(Size.Width, (Font.Size + 4) * Lines.Count);
            Sprites["text"].Bitmap = new Bitmap(Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            for (int i = 0; i < Lines.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Lines[i], 0, (Font.Size + 2) * i, this.Enabled ? this.TextColor : new Color(72, 72, 72));
            }
            Sprites["text"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            this.RedrawText();
            this.Drawn = true;
        }
    }
}
