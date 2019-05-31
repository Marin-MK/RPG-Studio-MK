using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public interface IContainer
    {
        int RealX { get; }
        int RealY { get; }
        Size Size { get; }
        List<Widget> Widgets { get; }

        double ScrollPercentageX { get; set; }
        double ScrollPercentageY { get; set; }

        IContainer Add(Widget w);
        IContainer Get(string Name);
        IContainer Remove(Widget w);
        string GetName(string Name);
    }
}
