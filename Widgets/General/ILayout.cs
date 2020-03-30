using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Widgets
{
    public interface ILayout
    {
        bool NeedUpdate { get; set; }

        void UpdateLayout();
    }

    public class LayoutContainer : Container
    {
        public Widget Widget;
        public new int GridRowStart { get { return Widget.GridRowStart; } }
        public new int GridRowEnd { get { return Widget.GridRowEnd; } }
        public new int GridColumnStart { get { return Widget.GridColumnStart; } }
        public new int GridColumnEnd { get { return Widget.GridColumnEnd; } }

        public LayoutContainer(IContainer Parent, int Index = -1) : base(Parent, Index)
        {

        }
    }
}
