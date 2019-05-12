using System;
using ODL;

namespace MKEditor.Widgets
{
    public class GroupBox : Container
    {
        public string Text { get; protected set; } = "";

        public GroupBox(object Parent)
            : base(Parent, "groupBox")
        {
            this.Text = this.Name;
            this.Sprites["panel"] = new Sprite(this.Viewport);
        }

        public void SetText(string text)
        {
            if (this.Text != text)
            {
                this.Text = text;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (this.Sprites["panel"].Bitmap != null) this.Sprites["panel"].Bitmap.Dispose();
            this.Sprites["panel"].Bitmap = new Bitmap(this.Size);
            this.Sprites["panel"].Bitmap.Font = new Font("Fonts/Segoe UI", 12);
            this.Sprites["panel"].Bitmap.DrawLines(220, 220, 220,
                new Point(6, 6),
                new Point(0, 6),
                new Point(0, this.Size.Height - 1),
                new Point(this.Size.Width - 1, this.Size.Height - 1),
                new Point(this.Size.Width - 1, 6));
            this.Sprites["panel"].Bitmap.DrawText(this.Text, 8, -3, Color.BLACK);
            int width = this.Sprites["panel"].Bitmap.TextSize(this.Text).Width;
            this.Sprites["panel"].Bitmap.DrawLine(
                new Point(width + 10, 6),
                new Point(this.Size.Width - 1, 6),
                220, 220, 220);
            base.Draw();
        }
    }
}
