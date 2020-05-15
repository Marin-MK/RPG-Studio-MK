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

        public Grid(IContainer Parent) : base(Parent)
        {
            
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
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
                Widget w = this.Widgets[i];
                int width = 0;
                int height = 0;
                if (w.GridRowStart >= this.Rows.Count || w.GridRowEnd >= this.Rows.Count)
                {
                    throw new Exception("Widget GridRow value exceeds amount of defined rows");
                }
                if (w.GridColumnStart >= this.Columns.Count || w.GridColumnEnd >= this.Columns.Count)
                {
                    throw new Exception("Widget GridColumn value exceeds amount of defined columns");
                }
                for (int j = w.GridRowStart; j <= w.GridRowEnd; j++)
                {
                    height += this.Sizes[j * this.Columns.Count + w.GridColumnStart].Height;
                }
                for (int j = w.GridColumnStart; j <= w.GridColumnEnd; j++)
                {
                    width += this.Sizes[w.GridRowStart * this.Columns.Count + j].Width;
                }
                Point p = this.Positions[w.GridRowStart * this.Columns.Count + w.GridColumnStart];
                int x = p.X;
                int y = p.Y;
                x += w.Margin.Left;
                width -= w.Margin.Left + w.Margin.Right;
                y += w.Margin.Up;
                height -= w.Margin.Up + w.Margin.Down;
                w.SetPosition(x, y);
                w.SetSize(width, height);
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
            this.SetSize(Positions.Last().X + Sizes.Last().Width, Positions.Last().Y + Sizes.Last().Height);

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

        public override void ParentSizeChanged(BaseEventArgs e)
        {
            this.SetSize(Parent.Size);
            this.RedrawContainers = true;
        }

        public override void Add(Widget w)
        {
            base.Add(w);
            this.NeedUpdate = true;
        }
    }
}
