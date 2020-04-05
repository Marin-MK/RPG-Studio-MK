﻿using System;
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

        public new void UpdateLayout()
        {
            int y = 0;
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                LayoutContainer w = this.Widgets[i] as LayoutContainer;
                y += w.Widget.Margin.Up;
                int x = w.Widget.Margin.Left;
                int width = this.Size.Width - x - w.Widget.Margin.Right;
                w.SetWidth(width);
                w.SetPosition(x, y);
                y += w.Size.Height;
                y += w.Widget.Margin.Down;
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
            return this;
        }

        public void UpdateHeight()
        {
            this.UpdateLayout();
            int maxheight = 0;
            foreach (LayoutContainer lc in this.Widgets)
            {
                int h = lc.Position.Y + lc.Size.Height;
                if (h > maxheight) maxheight = h;
            }
            this.SetHeight(maxheight);
        }

        public override void Add(Widget w)
        {
            this.Insert(this.Widgets.Count, w);
        }

        public Widget Insert(int Index, Widget w)
        {
            if (w is LayoutContainer)
            {
                this.Widgets.Insert(Index, w);
            }
            else
            {
                LayoutContainer c = new LayoutContainer(this, Index);
                c.Widget = w;
                w.SetParent(c);
                c.OnChildBoundsChanged += delegate (ObjectEventArgs e)
                {
                    c.SetHeight(((Size) e.Object).Height);
                    w.SetHeight(((Size) e.Object).Height);
                    this.UpdateHeight();
                };
                w.Viewport = new Viewport(w.Window.Renderer, 0, 0, w.Size);
            }
            return w;
        }
    }
}
