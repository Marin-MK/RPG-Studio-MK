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
        int ZIndex { get; }

        int ScrolledX { get; set; }
        int ScrolledY { get; set; }
        Point ScrolledPosition { get; }
        HScrollBar HScrollBar { get; }
        VScrollBar VScrollBar { get; }

        IContainer Parent { get; }
        int WindowLayer { get; set; }

        void Add(Widget w);
        Widget Get(string Name);
        Widget Remove(Widget w);
        string GetName(string Name);
    }
}
