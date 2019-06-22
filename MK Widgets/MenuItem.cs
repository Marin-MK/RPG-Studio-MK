using System;
using System.Collections.Generic;

namespace MKEditor.Widgets
{
    public interface IMenuItem { }

    public class MenuItem : IMenuItem
    {
        public string Text;
        public List<IMenuItem> Items;
        public bool Checkable;
        public string Image;
        public string Shortcut;
        public EventHandler<ODL.MouseEventArgs> OnLeftClick;

        public MenuItem(string Text)
        {
            this.Text = Text;
            this.Checkable = false;
            this.Items = new List<IMenuItem>();
        }
    }

    public class MenuSeparator : IMenuItem { }
}
