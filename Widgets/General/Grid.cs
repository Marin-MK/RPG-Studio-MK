using System;
using System.Collections.Generic;
using System.Linq;
using ODL;

namespace MKEditor.Widgets
{
    public class Grid : Widget, ILayout
    {
        public bool NeedUpdate { get; set; } = true;
        public List<GridSize> Rows = new List<GridSize>();
        public List<GridSize> Columns = new List<GridSize>();
        public Size[] Sizes;
        public Point[] Positions;

        private bool RedrawContainers = true;

        public Grid(object Parent)
            : base(Parent, "grid")
        {
            this.Size = new Size(this.Parent.Size);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            UpdateContainers();
            UpdateLayout();
        }

        public override void Update()
        {
            if (this.RedrawContainers)
            {
                UpdateContainers();
                this.RedrawContainers = false;
            }
            if (this.NeedUpdate)
            {
                this.UpdateLayout();
                this.NeedUpdate = false;
            }
            base.Update();
        }

        public new void UpdateLayout()
        {
            if (this.Sizes == null || this.Sizes.Length == 0) UpdateContainers();
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                LayoutContainer lc = this.Widgets[i] as LayoutContainer;
                int width = 0;
                int height = 0;
                if (lc.Widget.GridRowStart >= this.Rows.Count || lc.Widget.GridRowEnd >= this.Rows.Count)
                {
                    throw new Exception("Widget GridRow value exceeds amount of defined rows");
                }
                if (lc.Widget.GridColumnStart >= this.Columns.Count || lc.Widget.GridColumnEnd >= this.Columns.Count)
                {
                    throw new Exception("Widget GridColumn value exceeds amount of defined columns");
                }
                for (int j = lc.Widget.GridRowStart; j <= lc.Widget.GridRowEnd; j++)
                {
                    height += this.Sizes[j * this.Columns.Count + lc.Widget.GridColumnStart].Height;
                }
                for (int j = lc.Widget.GridColumnStart; j <= lc.Widget.GridColumnEnd; j++)
                {
                    width += this.Sizes[lc.Widget.GridRowStart * this.Columns.Count + j].Width;
                }
                Point p = this.Positions[lc.Widget.GridRowStart * this.Columns.Count + lc.Widget.GridColumnStart];
                int x = p.X;
                int y = p.Y;
                x += lc.Widget.Margin.Left;
                width -= lc.Widget.Margin.Left + lc.Widget.Margin.Right;
                y += lc.Widget.Margin.Up;
                height -= lc.Widget.Margin.Up + lc.Widget.Margin.Down;
                lc.SetPosition(x, y);
                lc.SetSize(width, height);

                lc.Widget.SetSize(width, height);

                //if (w is Grid) (w as Grid).UpdateContainers();
                //else if (w is ILayout) (w as ILayout).UpdateLayout();
            }
        }

        public override void Add(Widget w)
        {
            if (w is LayoutContainer)
            {
                this.Widgets.Add(w);
            }
            else
            {
                LayoutContainer c = new LayoutContainer(this);
                c.Widget = w;
                w.SetParent(c);
                w.Viewport = c.Viewport;
            }
        }

        public void UpdateContainers()
        {
            if (this.Sizes != null) this.Sizes = null;
            if (this.Positions != null) this.Positions = null;
            if (this.Rows.Count == 0) this.Rows.Add(new GridSize(1, Unit.Relative));
            if (this.Columns.Count == 0) this.Columns.Add(new GridSize(1, Unit.Relative));

            this.Sizes = new Size[this.Rows.Count * this.Columns.Count];
            this.Positions = new Point[this.Rows.Count * this.Columns.Count];

            int WidthToSpread = this.Size.Width;
            double TotalWidthPoints = 0;
            int HeightToSpread = this.Size.Height;
            double TotalHeightPoints = 0;
            for (int i = 0; i < this.Rows.Count; i++)
            {
                GridSize s = this.Rows[i];
                if (s.Unit == Unit.Pixels)
                {
                    HeightToSpread -= (int) s.Value;
                }
                else if (s.Unit == Unit.Relative)
                {
                    TotalHeightPoints += s.Value;
                }
            }
            for (int i = 0; i < this.Columns.Count; i++)
            {
                GridSize s = this.Columns[i];
                if (s.Unit == Unit.Pixels)
                {
                    WidthToSpread -= (int) s.Value;
                }
                else if (s.Unit == Unit.Relative)
                {
                    TotalWidthPoints += s.Value;
                }
            }

            double WidthPerPoint = WidthToSpread / TotalWidthPoints;
            double HeightPerPoint = HeightToSpread / TotalHeightPoints;

            int x = 0;
            int y = 0;
            for (int i = 0; i < this.Rows.Count; i++)
            {
                GridSize row = this.Rows[i];
                int height = 0;
                if (row.Unit == Unit.Pixels) height = (int) row.Value;
                else if (row.Unit == Unit.Relative) height = (int) Math.Round(HeightPerPoint * row.Value);

                for (int j = 0; j < this.Columns.Count; j++)
                {
                    GridSize column = this.Columns[j];
                    int width = 0;
                    if (column.Unit == Unit.Pixels) width = (int) column.Value;
                    else if (column.Unit == Unit.Relative) width = (int) Math.Round(WidthPerPoint * column.Value);
                    Sizes[i * this.Columns.Count + j] = new Size(width, height);
                    Positions[i * this.Columns.Count + j] = new Point(x, y);
                    x += width;
                }

                x = 0;
                y += height;
            }

            this.NeedUpdate = true;
        }

        public void SetRows(params GridSize[] Rows)
        {
            this.Rows = Rows.ToList();
            this.RedrawContainers = true;
        }

        public void SetColumns(params GridSize[] Columns)
        {
            this.Columns = Columns.ToList();
            this.RedrawContainers = true;
        }

        public override void ParentSizeChanged(object sender, SizeEventArgs e)
        {
            this.Size = new Size(e.Width, e.Height);
            this.RedrawContainers = true;
        }
    }
}
