using System;
using ODL;

namespace MKEditor.Widgets
{
    public class VStackPanel : Widget, ILayout
    {
        public bool NeedUpdate { get; set; } = true;

        public VStackPanel(IContainer Parent) : base(Parent)
        {
            this.Size = new Size(this.Parent.Size.Width, 1);
        }

        public override void Update()
        {
            if (this.NeedUpdate)
            {
                this.UpdateLayout();
                this.NeedUpdate = false;
            }
            base.Update();
        }

        public override void UpdateLayout()
        {
            int y = 0;
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                Widget w = this.Widgets[i];
                y += w.Margin.Up;
                int x = w.Margin.Left;
                int width = this.Size.Width - x - w.Margin.Right;
                w.SetWidth(width);
                w.SetPosition(x, y);
                y += w.Size.Height;
                y += w.Margin.Down;
            }
        }

        public override Widget SetSize(Size size)
        {
            base.SetSize(size);
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                Widget w = this.Widgets[i];
                w.SetWidth(size.Width);
            }
            return this;
        }

        public override void ChildBoundsChanged(BaseEventArgs e)
        {
            base.ChildBoundsChanged(e);
            UpdateHeight();
        }

        public void UpdateHeight()
        {
            int maxheight = 0;
            foreach (Widget w in this.Widgets)
            {
                int h = w.Position.Y + w.Size.Height;
                if (h > maxheight) maxheight = h;
            }
            this.SetHeight(maxheight);
        }

        public override void Add(Widget w)
        {
            base.Add(w);
            this.NeedUpdate = true;
        }
    }
}
