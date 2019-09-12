using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public interface IContainer
    {
        Viewport Viewport { get; }
        Point Position { get; }
        Size Size { get; }
        List<Widget> Widgets { get; }
        Point AdjustedPosition { get; }
        Size AdjustedSize { get; }

        int ScrolledX { get; set; }
        int ScrolledY { get; set; }
        Point ScrolledPosition { get; }
        HScrollBar HScrollBar { get; }
        VScrollBar VScrollBar { get; }

        IContainer Parent { get; }

        void Add(Widget w);
        Widget Get(string Name);
        Widget Remove(Widget w);
        string GetName(string Name);
    }
}
