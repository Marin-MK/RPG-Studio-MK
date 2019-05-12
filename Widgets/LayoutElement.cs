using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Widgets
{
    public class LayoutElement : Widget
    {
        public Widget Widget;

        public LayoutElement(object Parent, Widget w)
            : base(Parent, "layoutElement")
        {
            this.Widget = w;
        }
    }
}
