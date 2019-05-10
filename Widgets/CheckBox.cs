using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CheckBox : TextWidget
    {
        public bool Checked { get; protected set; } = false;

        public CheckBox(object Parent, string Name = "checkBox")
            : base(Parent, Name)
        {
            this.WidgetIM.OnMouseDown += this.MouseDown;
            this.WidgetIM.OnMouseUp += this.MouseUp;
            this.WidgetIM.OnHoverChanged += this.HoverChanged;
            this.WidgetIM.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                this.SetChecked(!this.Checked);
            };
            this.OnWidgetSelect += WidgetSelect;
            this.MinimumSize = new Size(1, 15);
            this.MaximumSize = new Size(9999, 15);
            this.Sprites["box"] = new Sprite(this.Viewport);
            this.AutoResize = true;
        }

        public void SetChecked(bool Value)
        {
            if (this.Checked != Value)
            {
                this.Checked = Value;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (this.Sprites["box"].Bitmap != null) this.Sprites["box"].Bitmap.Dispose();

            Font font = new Font("Fonts/Segoe UI");
            if (this.AutoResize)
            {
                Size size = font.TextSize(this.Text);
                this.SetSize(size.Width + 24, size.Height);
            }

            Bitmap bmp = new Bitmap(this.Size);
            bmp.Font = font;
            this.Sprites["box"].Bitmap = bmp;

            Color Outer = new Color(51, 51, 51);
            Color Inner = new Color(255, 255, 255);
            if (this.WidgetIM.ClickAnim())
            {
                Outer.Set(0, 84, 153);
                Inner.Set(204, 228, 247);
            }
            else if (this.WidgetIM.HoverAnim())
            {
                Outer.Set(0, 120, 215);
            }

            bmp.DrawRect(0, 1, 13, 13, Outer);
            bmp.FillRect(1, 2, 11, 11, Inner);
            if (this.Checked)
            {
                bmp.Build(1, 2, GetCheckBitmap(this.WidgetIM.ClickAnim() ? 2 : this.WidgetIM.HoverAnim() ? 1 : 0));
            }

            bmp.DrawText(this.Text, 18, 0, Color.BLACK);
            base.Draw();
        }

        private Bitmap GetCheckBitmap(int state)
        {
            // Possible states:
            // 0 = normal
            // 1 = hover
            // 2 = click
            Bitmap bmp = new Bitmap(11, 11);
            if (state == 0)
            {
                bmp.DrawLine(1, 5, 3, 7, 72, 72, 72);
                bmp.DrawLine(4, 7, 9, 2, 72, 72, 72);
                bmp.DrawLine(1, 6, 3, 8, 107, 107, 107);
                bmp.DrawLine(4, 8, 9, 3, 107, 107, 107);
                bmp.DrawLine(0, 6, 2, 8, 246, 246, 246);
                bmp.DrawLine(5, 8, 10, 3, 246, 246, 246);
                bmp.DrawLine(2, 5, 3, 6, 226, 226, 226);
                bmp.DrawLine(4, 6, 8, 2, 226, 226, 226);
                bmp.SetPixel(0, 5, 184, 184, 184);
                bmp.SetPixel(10, 2, 184, 184, 184);
            }
            else
            {
                Color core1 = new Color(35, 130, 217);
                Color core2 = new Color(0, 120, 215);
                Color outer1 = new Color(134, 178, 230);
                Color outer2 = new Color(83, 150, 223);
                Color outer3 = new Color(188, 211, 240);
                Color outer4 = new Color(245, 248, 253);
                Color outer5 = new Color(223, 233, 248);
                Color outer6 = new Color(119, 169, 228);
                if (state == 2)
                {
                    core1 = new Color(25, 96, 159);
                    core2 = new Color(0, 84, 153);
                    outer1 = new Color(106, 148, 191);
                    outer2 = new Color(64, 118, 173);
                    outer3 = new Color(150, 182, 215);
                    outer4 = new Color(196, 221, 242);
                    outer5 = new Color(178, 206, 231);
                    outer6 = new Color(93, 138, 185);
                }
                bmp.DrawLine(1, 5, 2, 6, core1);
                bmp.SetPixel(3, 7, core2);
                bmp.DrawLine(4, 7, 9, 2, core2);
                bmp.DrawLine(4, 6, 8, 2, outer1);
                bmp.DrawLine(1, 6, 3, 8, outer2);
                bmp.DrawLine(4, 8, 9, 3, outer3);
                bmp.SetPixel(9, 1, outer3);
                bmp.DrawLine(0, 6, 2, 8, outer4);
                bmp.SetPixel(0, 4, outer5);
                bmp.DrawLine(1, 4, 3, 6, outer5);
                bmp.SetPixel(10, 2, outer5);
                bmp.SetPixel(0, 5, outer6);
            }
            return bmp;
        }
    }
}
