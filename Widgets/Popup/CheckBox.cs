using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CheckBox : Widget
    {
        public string Text { get; protected set; }
        public bool Checked { get; protected set; } = false;
        public Font Font { get; protected set; } = Font.Get("Fonts/Ubuntu-R", 14);
        public bool BlueColorScheme { get; protected set; } = false;

        public CheckBox(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport, new Bitmap(16, 16));
            RedrawBox();
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = 20;
            Sprites["text"].Y = -1;
            SetText("");
            OnWidgetSelected += WidgetSelected;
            SetHeight(16);
        }

        public void SetBlueColorScheme(bool BlueColorScheme)
        {
            if (this.BlueColorScheme != BlueColorScheme)
            {
                this.BlueColorScheme = BlueColorScheme;
                this.RedrawBox();
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
            Sprites["text"].Bitmap.DrawText(this.Text, Color.WHITE);
            Sprites["text"].Bitmap.Lock();
        }

        public void RedrawBox()
        {
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.Clear();
            Color light = BlueColorScheme ? new Color(64, 104, 146) : new Color(108, 103, 110);
            Color outline = BlueColorScheme ? new Color(23, 36, 50) : new Color(36, 34, 36);
            Sprites["box"].Bitmap.SetPixel(1, 1, light);
            Sprites["box"].Bitmap.DrawLine(2, 0, 13, 0, light);
            Sprites["box"].Bitmap.DrawLine(2, 1, 13, 1, outline);
            Sprites["box"].Bitmap.SetPixel(14, 1, light);
            Sprites["box"].Bitmap.DrawLine(15, 2, 15, 13, light);
            Sprites["box"].Bitmap.DrawLine(14, 2, 14, 13, outline);
            Sprites["box"].Bitmap.SetPixel(14, 14, light);
            Sprites["box"].Bitmap.DrawLine(2, 15, 13, 15, light);
            Sprites["box"].Bitmap.DrawLine(2, 14, 13, 14, outline);
            Sprites["box"].Bitmap.SetPixel(1, 14, light);
            Sprites["box"].Bitmap.DrawLine(0, 2, 0, 13, light);
            Sprites["box"].Bitmap.DrawLine(1, 2, 1, 13, outline);
            Sprites["box"].Bitmap.FillRect(2, 2, 12, 12, light);
            Sprites["box"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            Color c = BlueColorScheme ? new Color(64, 104, 146) : new Color(108, 103, 110);
            if (WidgetIM.ClickAnim()) c = BlueColorScheme ? new Color(23, 36, 50) : new Color(36, 34, 36);
            else if (WidgetIM.HoverAnim()) c = BlueColorScheme ? new Color(40, 62, 84) : new Color(79, 82, 91);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.FillRect(2, 2, 12, 12, c);
            if (this.Checked)
            {
                int x = 4;
                int y = 4;
                Color w = Color.WHITE;
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
            if (WidgetIM.Hovering) Redraw();
        }

        public override void LeftClick(MouseEventArgs e)
        {
            base.LeftClick(e);
            this.SetChecked(!this.Checked);
        }
    }
}
