using System;
using ODL;

namespace MKEditor.Widgets
{
    public class VStackPanel : Widget, ILayout
    {
        public bool NeedUpdate { get; set; } = true;

        public VStackPanel(object Parent)
            : base(Parent, "vStackPanel")
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

        public new void UpdateLayout()
        {
            int y = 0;
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                LayoutContainer w = this.Widgets[i] as LayoutContainer;
                y += w.Margin.Up;
                int x = w.Margin.Left;
                int width = this.Size.Width - x - w.Margin.Right;
                w.SetWidth(width);
                w.SetPosition(x, y);
                y += w.Size.Height;
                y += w.Margin.Down;
                w.Widget.SetPosition(0, 0);
                w.Widget.SetWidth(width);
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
            this.UpdateLayout();
            return this;
        }

        public void UpdateHeight()
        {
            int maxheight = 0;
            foreach (LayoutContainer lc in this.Widgets)
            {
                if (lc.Position.Y + lc.Size.Height > maxheight) maxheight = lc.Position.Y + lc.Size.Height;
            }
            this.SetHeight(maxheight);
        }

        public override IContainer Add(Widget w)
        {
            if (w is LayoutContainer)
            {
                this.Widgets.Add(w);
            }
            else
            {
                LayoutContainer c = new LayoutContainer(this);
                c.Widget = w;
                w.SetParent(c);
                c.OnChildSizeChanged += delegate (object sender, SizeEventArgs e)
                {
                    c.SetHeight(e.Height);
                    w.SetHeight(e.Height);
                    this.UpdateHeight();
                };
                w.Viewport = new Viewport(w.Window.Renderer, 0, 0, w.Size);
            }
            return w;
        }
    }
}
