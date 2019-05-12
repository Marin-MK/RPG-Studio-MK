using System;
using ODL;

namespace MKEditor.Widgets
{
    public class RadioButton : Widget
    {
        public bool Checked { get; protected set; } = false;
        public string Text { get; protected set; } = "";

        public RadioButton(object Parent)
            : base(Parent, "radioButton")
        {
            this.Size = new Size(85, 17);
            this.MinimumSize = new Size(14, 17);
            this.WidgetIM.OnHoverChanged += HoverChanged;
            this.WidgetIM.OnMouseDown += MouseDown;
            this.WidgetIM.OnMouseUp += MouseUp;
            this.OnLeftClick += LeftClick;
            this.OnWidgetSelect += WidgetSelect;
            this.Sprites["btn"] = new Sprite(this.Viewport);
            this.AutoResize = true;
        }

        public void SetChecked(bool Checked)
        {
            if (this.Checked != Checked)
            {
                this.Checked = Checked;
                this.Redraw();
            }
        }

        protected override void Draw()
        {
            if (this.Sprites["btn"].Bitmap != null) this.Sprites["btn"].Bitmap.Dispose();

            Font font = new Font("Fonts/Segoe UI");
            if (this.AutoResize)
            {
                Size size = font.TextSize(this.Text);
                this.SetSize(size.Width + 24, size.Height);
            }

            this.Sprites["btn"].Bitmap = new Bitmap(this.Size);
            this.Sprites["btn"].Bitmap.Font = font;

            Color c = new Color(51, 51, 51);

            if (this.WidgetIM.ClickAnim())
            {
                c.Set(0, 84, 153);
                Color filler = new Color(204, 228, 247);
                this.Sprites["btn"].Bitmap.FillRect(3, 4, 9, 9, filler);
                this.Sprites["btn"].Bitmap.DrawCircle(7, 8, 6, filler);
            }
            else if (this.WidgetIM.HoverAnim())
            {
                c.Set(0, 120, 215);
            }

            this.Sprites["btn"].Bitmap.DrawCircle(7, 8, 7, c);

            if (this.Checked)
            {
                this.Sprites["btn"].Bitmap.DrawCircle(7, 8, 4, c);
                this.Sprites["btn"].Bitmap.DrawCircle(7, 8, 3, c);
                this.Sprites["btn"].Bitmap.DrawCircle(7, 8, 2, c);
                this.Sprites["btn"].Bitmap.DrawCircle(7, 8, 1, c);
            }

            this.Sprites["btn"].Bitmap.DrawText(this.Text, 18, 0, Color.BLACK);
            base.Draw();
        }

        public override void LeftClick(object sender, MouseEventArgs e)
        {
            RadioButton w = this.Parent.Widgets.Find(wt => wt is RadioButton && wt != this && (wt as RadioButton).Checked) as RadioButton;
            if (w != null || w == null && !this.Checked)
            {
                if (w != null) w.SetChecked(false);
                this.SetChecked(true);
            }
        }
    }
}
