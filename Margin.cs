using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor
{
    public class Margin
    {
        public int Left { get; protected set; }
        public int Up { get; protected set; }
        public int Right { get; protected set; }
        public int Down { get; protected set; }

        public Margin()
            : this(0, 0, 0, 0) { }
        public Margin(int all)
            : this(all, all, all, all) { }
        public Margin(int horizontal, int vertical)
            : this(horizontal, vertical, horizontal, vertical) { }
        public Margin(int left, int up, int right, int down)
        {
            this.Left = left;
            this.Up = up;
            this.Right = right;
            this.Down = down;
        }
    }
}
