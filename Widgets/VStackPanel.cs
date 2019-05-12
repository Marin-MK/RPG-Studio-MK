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

        public new void UpdateLayout()
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
            this.UpdateLayout();
            return this;
        }
    }
}
