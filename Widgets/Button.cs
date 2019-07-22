using System;
using ODL;

namespace MKEditor.Widgets
{
    public class Button : Widget
    {
        public string Text { get; protected set; } = "";
        public bool RedrawText { get; protected set; } = true;

        public Button(object Parent)
            : base(Parent, "button")
        {
            this.WidgetIM.OnMouseDown += this.MouseDown;
            this.WidgetIM.OnMouseUp += this.MouseUp;
            this.WidgetIM.OnHoverChanged += this.HoverChanged;
            this.WidgetIM.OnLeftClick += this.LeftClick;
            this.OnLeftClick += this.LeftClick;
            this.OnWidgetSelect += this.WidgetSelect;
            this.SetSize(100, 20);
            this.Sprites["rect"] = new RectSprite(this.Viewport);
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Text = this.Name;
        }

        public Widget SetText(string text)
        {
            if (this.Text != text)
            {
                this.Text = text;
                this.RedrawText = true;
                Redraw();
            }
            return this;
        }

        protected override void Draw()
        {
            Color OuterColor = new Color(173, 173, 173);
            Color InnerColor = new Color(225, 225, 225);
            int Thickness = 1;
            if (this.WidgetIM.ClickAnim())
            {
                OuterColor.Set(0, 84, 153);
                InnerColor.Set(204, 228, 247);
            }
            else if (this.WidgetIM.HoverAnim())
            {
                OuterColor.Set(0, 120, 215);
                InnerColor.Set(229, 241, 251);
            }
            else if (this.SelectedWidget)
            {
                OuterColor.Set(0, 120, 215);
                Thickness = 2;
            }

            RectSprite r = Sprites["rect"] as RectSprite;
            r.SetSize(this.Size, Thickness);
            r.SetColor(OuterColor, InnerColor);

            Font f = Font.Get("Fonts/Segoe UI", 12);
            Size s = f.TextSize(this.Text);
            this.Sprites["text"].X = this.Size.Width / 2 - s.Width / 2;
            this.Sprites["text"].Y = this.Size.Height / 2 - 9;
            if (this.RedrawText)
            {
                if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
                this.Sprites["text"].Bitmap = new Bitmap(s);
                this.Sprites["text"].Bitmap.Unlock();
                this.Sprites["text"].Bitmap.Font = f;
                this.Sprites["text"].Bitmap.DrawText(this.Text, 0, 0, Color.BLACK);
                this.Sprites["text"].Bitmap.Lock();
            }
            base.Draw();
        }
    }
}
