using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TabView : Widget
    {
        public  List<TabContainer> Tabs          = new List<TabContainer>();
        private List<string>       Names         = new List<string>();
        private List<int>          Widths        = new List<int>();
        public  int                SelectedIndex { get; protected set; } = -1;
        private int                HoveringIndex = -1;

        public EventHandler<EventArgs> OnSelectionChanged;

        public TabView(object Parent, string Name = "tabView")
            : base(Parent, Name)
        {
            Sprites["header"] = new Sprite(this.Viewport);
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
        }

        public void SelectTab(int Index)
        {
            if (SelectedIndex != -1 && SelectedIndex < Tabs.Count)
            {
                Tabs[SelectedIndex].SetVisible(false);
            }
            if (Index != -1 && Index < Tabs.Count)
            {
                Tabs[Index].SetVisible(true);
                this.SelectedIndex = Index;
                if (this.OnSelectionChanged != null) this.OnSelectionChanged.Invoke(this, new EventArgs());
            }
        }

        public TabContainer GetTab(int Index)
        {
            return this.Tabs[Index];
        }

        protected override void Draw()
        {
            if (Sprites["header"].Bitmap != null) Sprites["header"].Bitmap.Dispose();
            if (SelectedIndex == -1)
            {
                if (Tabs.Count > 0) SelectTab(0);
                else return;
            }
            Sprites["header"].Bitmap = new Bitmap(this.Size.Width, 26);
            Sprites["header"].Bitmap.Unlock();
            Sprites["header"].Bitmap.FillRect(new Rect(0, 23, this.Size.Width, 3), new Color(255, 168, 54));
            Font f = Font.Get("Fonts/Ubuntu-R", 16);
            Sprites["header"].Bitmap.Font = f;
            int x = 5;
            Widths.Clear();
            for (int i = 0; i < this.Tabs.Count; i++)
            {
                string name = this.Names[i];
                bool selected = i == this.SelectedIndex;
                bool hovering = i == this.HoveringIndex;
                int width = f.TextSize(name).Width + 8;
                Widths.Add(width);

                if (selected || hovering)
                {
                    Color c = selected ? new Color(255, 168, 54) : new Color(79, 82, 91);
                    Sprites["header"].Bitmap.DrawLine(new Point(x + 2, 0), new Point(x + width - 4, 0), c);
                    Sprites["header"].Bitmap.DrawLine(new Point(x + 1, 1), new Point(x + width - 2, 1), c);
                    Sprites["header"].Bitmap.FillRect(new Rect(x, 2, width, 21), c);
                }

                Color textcolor = selected ? Color.BLACK : Color.WHITE;
                Sprites["header"].Bitmap.DrawText(name, x + 4, 4, textcolor);
                x += width + 4;
            }
            Sprites["header"].Bitmap.Lock();
            base.Draw();
        }

        public void CreateTab(string Name)
        {
            TabContainer tc = new TabContainer(this);
            tc.SetPosition(0, 26);
            tc.SetSize(this.Size.Width, this.Size.Height - 26);
            this.Tabs.Add(tc);
            this.Names.Add(Name);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            foreach (TabContainer tc in this.Tabs)
            {
                tc.SetSize(this.Size.Width, this.Size.Height - 26);
            }
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            int oldindex = HoveringIndex;
            bool valid = true;
            if (!WidgetIM.Hovering) valid = false;
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            if (rx < 5 || ry >= 23) valid = false;
            int x = 5;
            if (valid)
            {
                for (int i = 0; i < Widths.Count; i++)
                {
                    x += Widths[i];
                    if (rx < x)
                    {
                        // This accounts for the 4px between tabs
                        if (x - rx > Widths[i]) break;
                        HoveringIndex = i;
                        Redraw();
                        return;
                    }
                    x += 4;
                }
            }
            HoveringIndex = -1;
            if (oldindex != HoveringIndex) Redraw();
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (HoveringIndex != -1)
            {
                SelectTab(HoveringIndex);
                Redraw();
            }
        }

        public void Refresh()
        {
            foreach (TabContainer tc in this.Tabs)
            {
                tc.SetVisible(!tc.Visible);
                tc.SetVisible(tc.Visible);
            }
        }
    }

    public class TabContainer : Container
    {
        public TabContainer(object Parent, string Name = "tabContainer")
            : base(Parent, Name)
        {

        }
    }
}
