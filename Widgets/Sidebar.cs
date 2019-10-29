using System;
using ODL;

namespace MKEditor.Widgets
{
    class Sidebar : Widget
    {
        public TabView TabControl;

        public Sidebar(object Parent, string Name = "panel")
            : base(Parent, Name)
        {
            SetBackgroundColor(10, 23, 37);
            TabControl = new TabView(this);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TabControl.SetSize(this.Size);
        }
    }
}
