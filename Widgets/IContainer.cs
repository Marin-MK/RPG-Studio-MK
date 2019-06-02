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

        double ScrollPercentageX { get; set; }
        double ScrollPercentageY { get; set; }
        Point ScrolledPosition { get; }

        IContainer Add(Widget w);
        IContainer Get(string Name);
        IContainer Remove(Widget w);
        string GetName(string Name);
    }
}
