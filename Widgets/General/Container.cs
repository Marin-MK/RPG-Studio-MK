using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Widgets
{
    public class Container : Widget
    {
        public Container(object Parent, string Name = "container", int Index = -1)
            : base(Parent, Name, Index)
        {

        }
    }
}
