using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CheckBox : Widget
    {
        public string Text { get; protected set; }
        public bool Checked { get; protected set; } = false;

        public CheckBox(object Parent, string Name = "checkBox")
            : base(Parent, Name)
        {
            Sprites["box"] = new Sprite(this.Viewport, new Bitmap(16, 16));
            Sprites["box"].Bitmap.Unlock();
            Color gray = new Color(108, 103, 110);
            Color dark = new Color(36, 34, 36);
            Sprites["box"].Bitmap.SetPixel(1, 1, gray);
            Sprites["box"].Bitmap.DrawLine(2, 0, 13, 0, gray);
            Sprites["box"].Bitmap.DrawLine(2, 1, 13, 1, dark);
            Sprites["box"].Bitmap.SetPixel(14, 1, gray);
            Sprites["box"].Bitmap.DrawLine(15, 2, 15, 13, gray);
            Sprites["box"].Bitmap.DrawLine(14, 2, 14, 13, dark);
            Sprites["box"].Bitmap.SetPixel(14, 14, gray);
            Sprites["box"].Bitmap.DrawLine(2, 15, 13, 15, gray);
            Sprites["box"].Bitmap.DrawLine(2, 14, 13, 14, dark);
            Sprites["box"].Bitmap.SetPixel(1, 14, gray);
            Sprites["box"].Bitmap.DrawLine(0, 2, 0, 13, gray);
            Sprites["box"].Bitmap.DrawLine(1, 2, 1, 13, dark);
            Sprites["box"].Bitmap.FillRect(2, 2, 12, 12, gray);
            Sprites["box"].Bitmap.Lock();
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = 20;
            Sprites["text"].Y = -1;
            SetText("");
            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseDown += MouseDown;
            WidgetIM.OnLeftClick += LeftClick;
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
                Font f = Font.Get("Fonts/Ubuntu-R", 14);
                Size s = f.TextSize(this.Text);
                if (20 + s.Width >= Size.Width) SetWidth(20 + s.Width);
                Sprites["text"].Bitmap = new Bitmap(Math.Max(1, s.Width), Math.Max(1, s.Height));
                Sprites["text"].Bitmap.Font = f;
                Sprites["text"].Bitmap.Unlock();
                Sprites["text"].Bitmap.DrawText(this.Text, Color.WHITE);
                Sprites["text"].Bitmap.Lock();
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

        protected override void Draw()
        {
            Color c = new Color(108, 103, 110);
            if (WidgetIM.ClickAnim()) c = new Color(36, 34, 36);
            else if (WidgetIM.HoverAnim()) c = new Color(79, 82, 91);
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

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (WidgetIM.Hovering) Redraw();
        }

        public override void LeftClick(object sender, MouseEventArgs e)
        {
            this.SetChecked(!this.Checked);
            base.LeftClick(sender, e);
        }
    }
}
