using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class ContextMenu : Widget
    {
        public List<IMenuItem> Items = new List<IMenuItem>();
        public IMenuItem SelectedItem;

        public ContextMenu(object Parent, string Name = "contextMenu")
            : base(Parent, Name)
        {
            this.Sprites["bg"] = new RectSprite(this.Viewport, this.Size, new Color(255, 191, 31), new Color(24, 25, 28));
            this.SetZIndex(999);
            this.SetWidth(200);
            this.Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(this.Size.Width - 2, 20, new Color(4, 4, 5)));
            this.Sprites["selection"].X = 1;
            this.Sprites["selection"].Visible = false;
            this.Sprites["items"] = new Sprite(this.Viewport);
            this.WidgetIM.OnHoverChanged += HoverChanged;
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
        }

        protected override void Draw()
        {
            if (this.Sprites["items"].Bitmap != null) this.Sprites["items"].Bitmap.Dispose();
            this.Sprites["items"].Bitmap = new Bitmap(CalcSize());
            Font f = Font.Get("Fonts/Quicksand Bold", 14);
            this.Sprites["items"].Bitmap.Unlock();
            this.Sprites["items"].Bitmap.Font = f;

            int oldwidth = Size.Width;
            int oldheight = Size.Height;
            int y = 5;
            for (int i = 0; i < this.Items.Count; i++)
            {
                IMenuItem item = this.Items[i];
                if (item is MenuItem)
                {
                    string text = (item as MenuItem).Text;
                    Size s = f.TextSize(text);
                    this.Sprites["items"].Bitmap.DrawText(text, 15, y, Color.WHITE);
                    y += 20;
                    if (15 + s.Width >= this.Size.Width) this.SetWidth(15 + s.Width + 4);
                }
                else if (item is MenuSeparator)
                {
                    this.Sprites["items"].Bitmap.DrawLine(1, y, Size.Width - 2, y, 38, 39, 42);
                    y += 6;
                }
            }

            if (y >= this.Size.Height) this.SetHeight(y);
            this.Drawn = false;
            this.Sprites["items"].Bitmap.Lock();
            base.Draw();
            if (oldwidth != Size.Width || oldheight != Size.Height) this.Redraw();
        }

        public Size CalcSize()
        {
            int w = 200;
            int h = 5;
            Font f = Font.Get("Fonts/Quicksand Bold", 16);
            for (int i = 0; i < this.Items.Count; i++)
            {
                IMenuItem item = this.Items[i];
                if (item is MenuSeparator)
                    h += 6;
                else
                {
                    h += 20;
                    Size s = f.TextSize((item as MenuItem).Text);
                    if (15 + s.Width >= w) w = 15 + s.Width + 4;
                }
            }
            return new Size(w, h);
        }

        public override void Dispose()
        {
            if (this.Window.UI.OverContextMenu == this) this.Window.UI.OverContextMenu = null;
            base.Dispose();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            (this.Sprites["bg"] as RectSprite).SetSize(this.Size);
            base.SizeChanged(sender, e);
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            this.Window.UI.OverContextMenu = this;
            base.HoverChanged(sender, e);
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering) return;
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            if (rx == 0 || rx == this.Size.Width - 1) return;
            int y = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                y += (Items[i] is MenuSeparator) ? 6 : 20;
                if (ry > y) continue;
                if (this.Items[i] is MenuSeparator)
                {
                    this.Sprites["selection"].Visible = false;
                    this.SelectedItem = null;
                }
                else
                {
                    this.Sprites["selection"].Visible = true;
                    this.Sprites["selection"].Y = y - 18;
                    this.SelectedItem = Items[i];
                }
                break;
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (WidgetIM.Hovering && this.SelectedItem != null)
            {
                if ((SelectedItem as MenuItem).OnLeftClick != null)
                {
                    (SelectedItem as MenuItem).OnLeftClick.Invoke(sender, e);
                }
                this.Dispose();
            }
        }
    }
}
