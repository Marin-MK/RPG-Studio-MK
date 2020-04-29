using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class DeleteMapPopup : MessageBox
    {
        public CheckBox DeleteChildMaps;

        public DeleteMapPopup(string Title, string Message, ButtonType type = ButtonType.OK, IconType IconType = IconType.None, List<string> _buttons = null)
            : base(Title, Message, type, IconType, _buttons)
        {
            SetSize(Size.Width, Size.Height + 20);
            label.SetPosition(label.Position.X, label.Position.Y - 10);
            DeleteChildMaps = new CheckBox(this);
            DeleteChildMaps.SetPosition(label.Position.X - 2, label.Position.Y + label.Size.Height + 16);
            DeleteChildMaps.SetText("Delete all child maps");
            DeleteChildMaps.SetFont(label.Font);
        }
    }
}
