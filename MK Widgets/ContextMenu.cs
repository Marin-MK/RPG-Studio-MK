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
            this.SetZIndex(999);
            this.SetWidth(260);
            this.Sprites["bg"] = new Sprite(this.Viewport);
            this.Sprites["selection"] = new Sprite(this.Viewport, new Bitmap(this.Size.Width, 31));
            this.Sprites["selection"].Visible = false;
            this.Sprites["items"] = new Sprite(this.Viewport);
            this.Sprites["items"].Z = 1;
            this.WidgetIM.OnHoverChanged += HoverChanged;
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
        }

        public void SetItems(List<IMenuItem> Items)
        {
            this.Items = new List<IMenuItem>(Items);
            this.SetSize(CalcSize());
        }

        protected override void Draw()
        {
            if (this.Sprites["bg"].Bitmap != null) this.Sprites["bg"].Bitmap.Dispose();
            Color bgc = new Color(47, 49, 54);
            this.Sprites["bg"].Bitmap = new Bitmap(this.Size);
            this.Sprites["bg"].Bitmap.Unlock();
            this.Sprites["bg"].Bitmap.FillQuadrant(9, 9, 10, Location.TopLeft, bgc);
            this.Sprites["bg"].Bitmap.FillQuadrant(this.Size.Width - 10, 9, 10, Location.TopRight, bgc);
            this.Sprites["bg"].Bitmap.FillQuadrant(9, this.Size.Height - 10, 10, Location.BottomLeft, bgc);
            this.Sprites["bg"].Bitmap.FillQuadrant(this.Size.Width - 10, this.Size.Height - 10, 10, Location.BottomRight, bgc);

            this.Sprites["bg"].Bitmap.FillRect(10, 0, this.Size.Width - 20, this.Size.Height, bgc);
            this.Sprites["bg"].Bitmap.FillRect(0, 10, 10, this.Size.Height - 20, bgc);
            this.Sprites["bg"].Bitmap.FillRect(this.Size.Width - 10, 10, 10, this.Size.Height - 20, bgc);

            this.Sprites["bg"].Bitmap.Lock();

            if (this.Sprites["items"].Bitmap != null) this.Sprites["items"].Bitmap.Dispose();
            this.Sprites["items"].Bitmap = new Bitmap(CalcSize());
            Font f = Font.Get("Fonts/Ubuntu-B", 13);
            Font fa = Font.Get("Fonts/FontAwesome Solid", 13);
            this.Sprites["items"].Bitmap.Unlock();

            int oldwidth = Size.Width;
            int oldheight = Size.Height;
            int y = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                IMenuItem item = this.Items[i];
                if (item is MenuItem)
                {
                    MenuItem menuitem = item as MenuItem;
                    string text = menuitem.Text;
                    Size s = f.TextSize(text);
                    Color c = Color.WHITE;
                    if (menuitem.IsClickable != null)
                    {
                        ConditionEventArgs e = new ConditionEventArgs();
                        menuitem.IsClickable.Invoke(this, e);
                        if (!e.ConditionValue) c = new Color(131, 137, 151);
                        menuitem.LastClickable = e.ConditionValue;
                    }
                    if (menuitem.Icon != Icon.NONE)
                    {
                        this.Sprites["items"].Bitmap.Font = fa;
                        this.Sprites["items"].Bitmap.DrawGlyph((char) menuitem.Icon, 9, y + 8, c);
                    }
                    this.Sprites["items"].Bitmap.Font = f;
                    this.Sprites["items"].Bitmap.DrawText(text, 30, y + 7, c);
                    y += 31;
                    if (30 + s.Width >= this.Size.Width) this.SetWidth(30 + s.Width + 4);
                }
                else if (item is MenuSeparator)
                {
                    this.Sprites["items"].Bitmap.DrawLine(1, y, Size.Width - 2, y, 38, 39, 42);
                    y += 1;
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
            int w = 260;
            int h = 0;
            Font f = Font.Get("Fonts/Ubuntu-B", 13);
            for (int i = 0; i < this.Items.Count; i++)
            {
                IMenuItem item = this.Items[i];
                if (item is MenuSeparator)
                    h += 1;
                else
                {
                    h += 31;
                    Size s = f.TextSize((item as MenuItem).Text);
                    if (30 + s.Width >= w) w = 30 + s.Width + 4;
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
            base.SizeChanged(sender, e);
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            this.Window.UI.OverContextMenu = this;
            base.HoverChanged(sender, e);
            this.MouseMoving(sender, e);
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            if (!WidgetIM.Hovering || rx < 0 || rx > this.Size.Width)
            {
                this.Sprites["selection"].Visible = false;
                this.SelectedItem = null;
                return;
            }
            int y = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                y += (Items[i] is MenuSeparator) ? 1 : 31;
                if (ry > y) continue;
                if (this.Items[i] is MenuSeparator)
                {
                    this.Sprites["selection"].Visible = false;
                    this.SelectedItem = null;
                }
                else
                {
                    this.Sprites["selection"].Bitmap.Unlock();
                    this.Sprites["selection"].Bitmap.Clear();
                    Color c = new Color(79, 82, 91);
                    if (i == 0)
                    {
                        this.Sprites["selection"].Bitmap.FillQuadrant(9, 9, 10, Location.TopLeft, c);
                        this.Sprites["selection"].Bitmap.FillQuadrant(this.Size.Width - 10, 9, 10, Location.TopRight, c);
                        this.Sprites["selection"].Bitmap.FillRect(10, 0, this.Size.Width - 20, 10, c);
                        this.Sprites["selection"].Bitmap.FillRect(0, 10, this.Size.Width, 21, c);
                    }
                    else if (i == this.Items.Count - 1)
                    {
                        this.Sprites["selection"].Bitmap.FillQuadrant(9, 21, 10, Location.BottomLeft, c);
                        this.Sprites["selection"].Bitmap.FillQuadrant(this.Size.Width - 10, 21, 10, Location.BottomRight, c);
                        this.Sprites["selection"].Bitmap.FillRect(10, 21, this.Size.Width - 20, 10, c);
                        this.Sprites["selection"].Bitmap.FillRect(0, 0, this.Size.Width, 21, c);
                    }
                    else
                    {
                        this.Sprites["selection"].Bitmap.FillRect(0, 0, this.Size.Width, 31, c);
                    }
                    this.Sprites["selection"].Bitmap.Lock();
                    this.Sprites["selection"].Visible = true;
                    this.Sprites["selection"].Y = y - 31;
                    this.SelectedItem = Items[i];
                }
                break;
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (WidgetIM.Hovering && this.SelectedItem != null && this.SelectedItem is MenuItem)
            {
                if ((this.SelectedItem as MenuItem).LastClickable)
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
}
