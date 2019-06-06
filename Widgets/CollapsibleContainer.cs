using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CollapsibleContainer : Container
    {
        public string Text { get; protected set; }
        public bool Collapsed { get; protected set; }

        private MouseInputManager ArrowIM;

        public CollapsibleContainer(object Parent, string Name = "collapsibleContainer")
            : base(Parent, Name)
        {
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 20;
            this.Sprites["line"] = new Sprite(this.Viewport);
            this.Sprites["line"].X = 20;
            this.Sprites["line"].Y = 19;
            this.Sprites["line"].Bitmap = new SolidBitmap(1, 1, new Color(127, 127, 127));
            this.Sprites["arrow"] = new Sprite(this.Viewport);
            this.Collapsed = true;
            ArrowIM = new MouseInputManager(this);
            ArrowIM.OnLeftClick += delegate (object sender, MouseEventArgs e) { SetCollapsed(!this.Collapsed); };
            this.SetCollapsed(false);
        }

        public Widget SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                this.Redraw();
            }
            return this;
        }

        protected override void Draw()
        {
            if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
            if (this.Text != null)
            {
                Font f = Font.Get("Fonts/Quicksand Bold", 16);
                Size s = f.TextSize(this.Text);
                this.Sprites["text"].Bitmap = new Bitmap(s);
                this.Sprites["text"].Bitmap.Unlock();
                this.Sprites["text"].Bitmap.Font = f;
                this.Sprites["text"].Bitmap.DrawText(this.Text, Color.WHITE);
                this.Sprites["text"].Bitmap.Lock();
            }

            this.Sprites["line"].Bitmap.Unlock();
            (this.Sprites["line"].Bitmap as SolidBitmap).SetSize(this.Size.Width - 20, 1);
            this.Sprites["line"].Bitmap.Lock();

            base.Draw();
        }

        public override void Update()
        {
            int DiffX = this.Position.X - this.ScrolledPosition.X;
            int DiffY = this.Position.Y - this.ScrolledPosition.Y;
            int x = this.Viewport.X - DiffX;
            int y = this.Viewport.Y - DiffY;
            int w = 13 - DiffX;
            int h = 13 - DiffY;
            ArrowIM.Update(new Rect(x, y, w, h));
            base.Update();
        }

        public void SetCollapsed(bool Collapsed)
        {
            if (this.Collapsed != Collapsed)
            {
                this.Collapsed = Collapsed;
                this.Widgets.ForEach(w =>
                {
                    w.SetVisible(!this.Collapsed);
                });
                if (this.Collapsed)
                {
                    this.MaximumSize = new Size(9999, 20);
                    this.SetSize(this.Size.Width, 20);
                }
                else
                {
                    int maxheight = 0;
                    foreach (Widget w in this.Widgets)
                    {
                        int h = w.Position.Y + w.Size.Height;
                        if (h > maxheight) maxheight = h;
                    }
                    this.MaximumSize = new Size(9999, 9999);
                    this.SetSize(this.Size.Width, maxheight);
                }
                this.UpdateCollapsed();
            }
        }

        private void UpdateCollapsed()
        {
            Bitmap bmp;
            if (this.Collapsed)
            {
                this.Sprites["line"].Visible = false;
                if (this.Sprites["arrow"].Y == 0 && this.Sprites["arrow"].Bitmap != null) return;
                bmp = new Bitmap(7, 13);
                bmp.Unlock();
                Color c = new Color(127, 127, 127);
                bmp.DrawLine(0, 0, 0, 12, c);
                bmp.DrawLine(1, 1, 1, 11, c);
                bmp.DrawLine(2, 2, 2, 10, c);
                bmp.DrawLine(3, 3, 3, 9, c);
                bmp.DrawLine(4, 4, 4, 8, c);
                bmp.DrawLine(5, 5, 5, 7, c);
                bmp.SetPixel(6, 6, c);
                this.Sprites["arrow"].X = 4;
                this.Sprites["arrow"].Y = 0;
            }
            else
            {
                this.Sprites["line"].Visible = true;
                if (this.Sprites["arrow"].Y == 4) return;
                bmp = new Bitmap(13, 7);
                bmp.Unlock();
                Color c = new Color(127, 127, 127);
                bmp.DrawLine(0, 0, 12, 0, c);
                bmp.DrawLine(1, 1, 11, 1, c);
                bmp.DrawLine(2, 2, 10, 2, c);
                bmp.DrawLine(3, 3, 9, 3, c);
                bmp.DrawLine(4, 4, 8, 4, c);
                bmp.DrawLine(5, 5, 7, 5, c);
                bmp.SetPixel(6, 6, c);
                this.Sprites["arrow"].X = 0;
                this.Sprites["arrow"].Y = 4;
            }
            bmp.Lock();
            this.Sprites["arrow"].Bitmap = bmp;
        }
    }
}
