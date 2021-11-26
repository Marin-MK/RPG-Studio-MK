using System;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class CheckBox : Widget
    {
        public string Text { get; protected set; }
        public bool Checked { get; protected set; } = false;
        public Font Font { get; protected set; }
        public bool Enabled { get; protected set; } = true;
        public bool Mirrored { get; protected set; } = false;

        bool Selecting = false;

        public BaseEvent OnCheckChanged;

        public CheckBox(IContainer Parent) : base(Parent)
        {
            this.Font = Fonts.ProductSansMedium.Use(14);
            Sprites["box"] = new Sprite(this.Viewport, new Bitmap(16, 16));
            RedrawBox(true);
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = 20;
            Sprites["text"].Y = -1;
            SetText("");
            OnWidgetSelected += WidgetSelected;
            SetHeight(16);
        }

        public void SetEnabled(bool Enabled)
        {
            if (this.Enabled != Enabled)
            {
                this.Enabled = Enabled;
                this.Redraw();
                this.RedrawText();
            }
        }

        public void SetFont(Font Font)
        {
            if (this.Font != Font)
            {
                this.Font = Font;
                this.RedrawText();
            }
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                this.RedrawText();
            }
        }

        public void SetChecked(bool Checked)
        {
            if (this.Checked != Checked)
            {
                this.Checked = Checked;
                Redraw();
                OnCheckChanged?.Invoke(new BaseEventArgs());
            }
        }

        public void SetMirrored(bool Mirrored)
        {
            if (this.Mirrored != Mirrored)
            {
                this.Mirrored = Mirrored;
                Sprites["text"].X = this.Mirrored ? 0 : 20;
                Sprites["box"].X = this.Mirrored ? Size.Width - 16 : 0;
            }
        }

        public void RedrawText()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Size s = this.Font.TextSize(this.Text);
            if (20 + s.Width >= Size.Width) SetWidth(20 + s.Width);
            Sprites["text"].Bitmap = new Bitmap(Math.Max(1, s.Width), Math.Max(1, s.Height));
            Sprites["text"].Bitmap.Font = this.Font;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText(this.Text, this.Enabled ? Color.WHITE : new Color(72, 72, 72));
            Sprites["text"].Bitmap.Lock();
        }

        public void RedrawBox(bool Lock)
        {
            if (Lock) Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.Clear();
            Color lightoutline = Selecting || WidgetIM.Hovering ? new Color(64, 104, 146) : new Color(86, 108, 134);
            Color filler = Selecting ? new Color(23, 36, 50) : lightoutline;
            Color outline = Selecting || WidgetIM.Hovering ? new Color(23, 36, 50) : new Color(36, 34, 36);

            if (!this.Enabled)
            {
                lightoutline = new Color(72, 72, 72);
                filler = new Color(72, 72, 72);
            }

            Sprites["box"].Bitmap.SetPixel(1, 1, lightoutline);
            Sprites["box"].Bitmap.DrawLine(2, 0, 13, 0, lightoutline);
            Sprites["box"].Bitmap.DrawLine(2, 1, 13, 1, outline);
            Sprites["box"].Bitmap.SetPixel(14, 1, lightoutline);
            Sprites["box"].Bitmap.DrawLine(15, 2, 15, 13, lightoutline);
            Sprites["box"].Bitmap.DrawLine(14, 2, 14, 13, outline);
            Sprites["box"].Bitmap.SetPixel(14, 14, lightoutline);
            Sprites["box"].Bitmap.DrawLine(2, 15, 13, 15, lightoutline);
            Sprites["box"].Bitmap.DrawLine(2, 14, 13, 14, outline);
            Sprites["box"].Bitmap.SetPixel(1, 14, lightoutline);
            Sprites["box"].Bitmap.DrawLine(0, 2, 0, 13, lightoutline);
            Sprites["box"].Bitmap.DrawLine(1, 2, 1, 13, outline);
            Sprites["box"].Bitmap.FillRect(2, 2, 12, 12, filler);
            if (Lock) Sprites["box"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            Sprites["box"].Bitmap.Unlock();
            RedrawBox(false);
            if (this.Checked && this.Enabled)
            {
                int x = 4;
                int y = 4;
                Color w = this.Enabled ? Color.WHITE : new Color(120, 120, 120);
                Sprites["box"].Bitmap.DrawLine(x, y + 5, x + 1, y + 5, w);
                Sprites["box"].Bitmap.DrawLine(x + 1, y + 6, x + 4, y + 6, w);
                Sprites["box"].Bitmap.DrawLine(x + 2, y + 7, x + 4, y + 7, w);
                Sprites["box"].Bitmap.SetPixel(x + 3, y + 8, w);
                Sprites["box"].Bitmap.FillRect(x + 4, y + 4, 2, 2, w);
                Sprites["box"].Bitmap.FillRect(x + 5, y + 2, 2, 2, w);
                Sprites["box"].Bitmap.DrawLine(x + 6, y + 1, x + 7, y + 1, w);
                Sprites["box"].Bitmap.SetPixel(x + 7, y, w);
            }
            Sprites["box"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && this.Enabled)
            {
                Selecting = true;
                SetChecked(!this.Checked);
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (Selecting)
            {
                Selecting = false;
                Redraw();
            }
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            Selecting = false;
            this.Redraw();
        }

        public override object GetValue(string Identifier)
        {
            return this.Checked;
        }

        public override void SetValue(string Identifier, object Value)
        {
            SetChecked((string) Value == "true");
        }
    }
}
