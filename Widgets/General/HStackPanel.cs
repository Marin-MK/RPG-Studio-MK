using System;
using ODL;

namespace MKEditor.Widgets
{
    public class HStackPanel : Widget, ILayout
    {
        public bool NeedUpdate { get; set; } = true;

        public HStackPanel(IContainer Parent) : base(Parent)
        {
            this.Size = new Size(this.Parent.Size);
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
            int x = 0;
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                Widget w = this.Widgets[i];
                x += w.Margin.Left;
                int y = w.Margin.Up;
                int height = this.Size.Height - y - w.Margin.Down;
                w.SetHeight(height);
                w.SetPosition(x, y);
                x += w.Size.Width;
                x += w.Margin.Right;
            }
        }

        public override Widget SetSize(Size size)
        {
            base.SetSize(size);
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                Widget w = this.Widgets[i];
                w.SetHeight(size.Height);
            }
            this.UpdateLayout();
            return this;
        }
    }
}
