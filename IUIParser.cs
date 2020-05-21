using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor
{
    public interface IUIParser
    {
        Widgets.Widget GetWidgetFromName(string Name);
        string GetIdentifierFromName(string Name);
    }
}
