using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Widgets
{
    public class GridSize
    {
        public double Value { get; }
        public Unit Unit { get; }

        public GridSize(double Value, Unit Unit = Unit.Relative)
        {
            this.Value = Value;
            this.Unit = Unit;
        }
    }

    public enum Unit
    {
        Pixels,
        Relative
    }
}
