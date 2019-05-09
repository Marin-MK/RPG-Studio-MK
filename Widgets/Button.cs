using System;
using ODL;

namespace MKEditor.Widgets
{
    public class Button : TextWidget
    {
        public Button(object Parent, string Name = "button")
            : base(Parent, Name)
        {
            this.WidgetIM.OnMouseDown += this.MouseDown;
            this.WidgetIM.OnMouseUp += this.MouseUp;
            this.WidgetIM.OnHoverChanged += this.HoverChanged;
            this.SetSize(100, 20);
            this.Sprites["button"] = new Sprite(this.Viewport);
            this.Text = "Button";
        }

        protected override void Draw()
        {
            if (this.Sprites["button"].Bitmap != null) this.Sprites["button"].Bitmap.Dispose();
            Bitmap bmp = new Bitmap(this.Size);
            this.Sprites["button"].Bitmap = bmp;
            bmp.Font = new Font("Fonts/Segoe UI");
            Color OuterColor = new Color(173, 173, 173);
            Color InnerColor = new Color(225, 225, 225);
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
            bmp.DrawRect(0, 0, this.Size, OuterColor);
            bmp.FillRect(1, 1, this.Size.Width - 2, this.Size.Height - 2, InnerColor);
            bmp.DrawText(this.Text, this.Size.Width / 2, this.Size.Height / 2 - 9, Color.BLACK, DrawOptions.CenterAlign);
            base.Draw();
        }
    }
}
