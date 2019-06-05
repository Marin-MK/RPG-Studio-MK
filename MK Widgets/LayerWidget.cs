using System;
using ODL;

namespace MKEditor.Widgets
{
    public class LayerWidget : Widget
    {
        private int ColorIndex = 0;

        public LayerWidget(object Parent, string Name = "layerWidget")
            : base(Parent, Name)
        {
            this.SetSize(280, 40);
            ColorIndex = this.Parent.Parent.Widgets.FindAll(w => (w as LayoutContainer).Widget is LayerWidget).Count % 2;
            this.SetBackgroundColor(ColorIndex == 0 ? new Color(36, 38, 41) : new Color(48, 50, 53));
        }
    }
}
