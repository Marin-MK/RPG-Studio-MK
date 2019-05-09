using System;
using ODL;

namespace MKEditor.Widgets
{
    public class ProgressBar : Widget
    {
        public double Value { get; protected set; } = 0;

        public ProgressBar(object Parent, string Name = "progressBar")
            : base(Parent, Name)
        {
            this.Sprites["bar"] = new Sprite(this.Viewport);
            this.Value = 0;
        }

        public void SetValue(double value)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;
            if (this.Value != value)
            {
                this.Value = value;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (this.Sprites["bar"].Bitmap != null) this.Sprites["bar"].Bitmap.Dispose();
            this.Sprites["bar"].Bitmap = new Bitmap(this.Size);
            this.Sprites["bar"].Bitmap.DrawRect(this.Size, 188, 188, 188);
            int width = (int) Math.Round(this.Value * (this.Size.Width - 2));
            width = Math.Min(width, this.Size.Width - 2);
            this.Sprites["bar"].Bitmap.FillRect(1, 1, width, this.Size.Height - 2, 6, 176, 37);
            this.Sprites["bar"].Bitmap.FillRect(width + 1, 1, this.Size.Width - 2 - width, this.Size.Height - 2, 230, 230, 230);
            base.Draw();
        }
    }
}
