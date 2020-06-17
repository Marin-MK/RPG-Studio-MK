using System;
using System.Collections.Generic;
using System.Text;
using amethyst;

namespace RPGStudioMK
{
    public interface IUIParser
    {
        Widget GetWidgetFromName(string Name);
        string GetIdentifierFromName(string Name);
    }
}
