using System;
using System.Collections.Generic;

namespace MKEditor.Widgets
{
    public interface IMenuItem { }

    public class MenuItem : IMenuItem
    {
        public string Text;
        public List<IMenuItem> Items;
        public string HelpText;
        public string Shortcut;
        public ODL.MouseEvent OnLeftClick;
        public ODL.BoolEvent IsClickable;
        public bool LastClickable = true;

        public MenuItem(string Text)
        {
            this.Text = Text;
            this.Items = new List<IMenuItem>();
        }
    }

    public class MenuSeparator : IMenuItem { }
}
