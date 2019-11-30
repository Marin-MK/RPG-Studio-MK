using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CollapsibleContainer : Container
    {
        public string Text { get; protected set; }
        public bool Collapsed { get; protected set; }

        public EventHandler<EventArgs> OnCollapsedChanged;

        private MouseInputManager ArrowIM;

        public CollapsibleContainer(object Parent, string Name = "collapsibleContainer")
            : base(Parent, Name)
        {
            Sprites["header"] = new Sprite(this.Viewport);
            this.Collapsed = true;
            ArrowIM = new MouseInputManager(this);
            ArrowIM.OnLeftClick += delegate (object sender, MouseEventArgs e) { SetCollapsed(!this.Collapsed); };
            this.SetCollapsed(false);
            this.WidgetIM.OnMouseDown += MouseDown;
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
            if (this.Sprites["header"].Bitmap != null) this.Sprites["header"].Bitmap.Dispose();
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            this.Sprites["header"].Bitmap = new Bitmap(Size.Width, 22);
            this.Sprites["header"].Bitmap.Unlock();
            this.Sprites["header"].Bitmap.Font = f;
            Utilities.DrawCollapseBox(this.Sprites["header"].Bitmap as Bitmap, 3, 4, this.Collapsed);
            this.Sprites["header"].Bitmap.DrawText(this.Text, 22, 0, Color.WHITE);
            this.Sprites["header"].Bitmap.Lock();
            base.Draw();
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
                    this.SetSize(this.Size.Width, 22);
                }
                else
                {
                    int maxheight = 0;
                    foreach (Widget w in this.Widgets)
                    {
                        int h = w.Position.Y + w.Size.Height;
                        if (h > maxheight) maxheight = h;
                    }
                    this.SetSize(this.Size.Width, maxheight);
                }
                this.Redraw();
                if (this.OnCollapsedChanged != null) this.OnCollapsedChanged.Invoke(this, new EventArgs());
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx < 0 || rx >= 19 || ry < 1 || ry >= 21) return;
            SetCollapsed(!Collapsed);
        }
    }
}
