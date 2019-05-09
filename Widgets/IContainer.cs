using System;
using ODL;

namespace MKEditor.Widgets
{
    public interface IContainer
    {
        int RealX { get; }
        int RealY { get; }
        Size Size { get; }

        void Add(Widget w);
        void Get(string Name);
        void Remove(Widget w);
        string GetName(string Name);
    }
}
