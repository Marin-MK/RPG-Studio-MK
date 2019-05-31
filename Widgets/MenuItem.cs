using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Widgets
{
    public class MenuItem : IMenuItem
    {
        public string Text;
        public List<IMenuItem> Items;
        public bool Checkable;
        public string Image;

        public MenuItem(string Text)
        {
            this.Text = Text;
            this.Checkable = false;
            this.Items = new List<IMenuItem>();
        }
    }
}
