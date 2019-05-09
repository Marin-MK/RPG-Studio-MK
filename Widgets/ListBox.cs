using System;
using System.Collections;
using ODL;

namespace MKEditor.Widgets
{
    public class ListBox : Widget
    {
        public ArrayList Items { get; protected set; }
        public int? Index { get; protected set; }
        public int VisibleItems { get { return (this.Size.Height - 4) / 13; } }

        private VScrollBar ScrollBar;

        public int TopIndex { get; protected set; }

        public ListBox(object Parent, string Name = "listBox")
            : base(Parent, Name)
        {
            this.WidgetIM.OnMouseDown += this.MouseDown;
            this.WidgetIM.OnMouseWheel += this.MouseWheel;
            this.WidgetIM.OnMouseMoving += this.MouseMoving;
            this.Sprites["box"] = new Sprite(this.Viewport);
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Items = new ArrayList();
            this.Index = 0;
            this.TopIndex = 0;
            this.SetSize(120, 69);
        }

        public void AddItem(object o)
        {
            this.Items.Add(o);
            Redraw();
        }

        public void SetItems(ArrayList items)
        {
            this.Items = items;
            this.Index = 0;
            this.TopIndex = 0;
            Redraw();
        }

        public override void SetSize(Size size)
        {
            int height = (int) Math.Floor((size.Height - 4) / 13f) * 13 + 4;
            size.Clamp(10, 9999, 15, height);
            base.SetSize(size);
        }

        public void SetIndex(int index)
        {
            if (Items.Count > 0)
            {
                index = Math.Min(index, this.Items.Count - 1);
                if (this.Index != index)
                {
                    this.Index = index;
                    Redraw();
                }
            }
            else
            {
                this.Index = null;
            }
        }

        public void SetTopIndex(int topindex)
        {
            if (this.TopIndex != topindex)
            {
                this.TopIndex = topindex;
                if (this.ScrollBar != null)
                {
                    this.ScrollBar.SetValue((double) this.TopIndex / (this.Items.Count - this.VisibleItems));
                }
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (this.Items.Count > this.VisibleItems)
            {
                if (this.ScrollBar == null)
                {
                    this.SizeChanged = true;
                    this.ScrollBar = new VScrollBar(this);
                    this.ScrollBar.SetSliderSize((double) this.VisibleItems / this.Items.Count);
                }
            }
            if (this.SizeChanged)
            {
                if (this.Sprites["box"].Bitmap != null) this.Sprites["box"].Bitmap.Dispose();
                this.Sprites["box"].Bitmap = new Bitmap(this.Size);
                this.Sprites["box"].Bitmap.DrawRect(this.Size, 130, 135, 144);
                this.Sprites["box"].Bitmap.FillRect(1, 1, this.Size.Width - 2, this.Size.Height - 2, 255, 255, 255);
                if (this.ScrollBar != null)
                {
                    this.ScrollBar.SetPosition(this.Size.Width - 19, 2);
                    this.ScrollBar.SetSize(17, this.Size.Height - 4);
                    this.ScrollBar.SetSliderSize((double) this.VisibleItems / this.Items.Count);
                }
            }
            if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
            this.Sprites["text"].Bitmap = new Bitmap(this.Size);
            this.Sprites["text"].Bitmap.Font = new Font("Fonts/Segoe UI", 12);
            int extra = this.Items.Count > this.VisibleItems ? 17 : 0;
            for (int i = 0; i < VisibleItems; i++)
            {
                if (i >= this.Items.Count) continue;
                object o = this.Items[TopIndex + i];
                Color c = new Color(0, 0, 0);
                if (this.Index == i + TopIndex)
                {
                    this.Sprites["text"].Bitmap.FillRect(2, 2 + 13 * i, this.Size.Width - extra - 4, 13, new Color(0, 120, 215));
                    c = new Color(255, 255, 255);
                }
                this.Sprites["text"].Bitmap.DrawText(o.ToString(), 4, 13 * i, c);
            }
            if (this.ScrollBar != null) this.ScrollBar.Redraw();
            base.Draw();
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton && !e.OldLeftButton)
            {
                UpdateMouse(e);
            }
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            UpdateMouse(e);
        }

        private void UpdateMouse(MouseEventArgs e)
        {
            if (this.WidgetIM.ClickedInArea == true)
            {
                int rx = e.X - this.RealRect.X;
                int ry = e.Y - this.RealRect.Y;
                int extra = this.Items.Count > this.VisibleItems ? 17 : 0;
                if (rx > 1 && rx < this.Size.Width - extra - 2)
                {
                    if (ry > 1 && ry < this.Size.Height - 2)
                    {
                        int idx = this.TopIndex + (int) Math.Floor((ry - 2) / 13d);
                        if (idx < this.Items.Count) this.SetIndex(idx);
                    }
                }
            }
        }

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            if (this.ScrollBar != null && this.RealRect.Contains(e.X, e.Y))
            {
                int downcount = 0;
                int upcount = 0;
                if (e.WheelY < 0) downcount = Math.Abs(e.WheelY);
                else upcount = e.WheelY;
                for (int i = 0; i < downcount * 3; i++) this.ScrollBar.ScrollDown();
                for (int i = 0; i < upcount * 3; i++) this.ScrollBar.ScrollUp();
            }
        }
    }
}
