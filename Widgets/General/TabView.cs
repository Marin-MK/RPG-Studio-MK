﻿using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TabView : Widget
    {
        public  List<TabContainer> Tabs          = new List<TabContainer>();
        private List<string>       Names         = new List<string>();
        public  int                SelectedIndex { get; protected set; } = -1;
        private int                HoveringIndex = -1;

        public EventHandler<EventArgs> OnSelectionChanged;

        public TabView(object Parent, string Name = "tabView")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(78, 25, new Color(28, 50, 73)));
            Sprites["header"] = new Sprite(this.Viewport, new SolidBitmap(1, 4, new Color(28, 50, 73)));
            Sprites["header"].Y = 25;
            Sprites["text"] = new Sprite(this.Viewport);
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
        }

        public void SelectTab(int Index)
        {
            if (Tabs.Count > 0)
            {
                if (SelectedIndex != -1 && SelectedIndex < Tabs.Count)
                {
                    Tabs[SelectedIndex].SetVisible(false);
                }
                if (Index != -1 && Index < Tabs.Count)
                {
                    Tabs[Index].SetVisible(true);
                    this.SelectedIndex = Index;
                    Sprites["bg"].X = Index * 78;
                    Sprites["bg"].Visible = true;
                    if (this.OnSelectionChanged != null) this.OnSelectionChanged.Invoke(this, new EventArgs());
                }
            }
            else
            {
                Sprites["bg"].Visible = false;
            }
        }

        public TabContainer GetTab(int Index)
        {
            return this.Tabs[Index];
        }

        protected override void Draw()
        {
            if (SelectedIndex == -1)
            {
                if (Tabs.Count > 0) SelectTab(0);
                else
                {
                    Sprites["bg"].Visible = false;
                    return;
                }
            }
            Sprites["bg"].Visible = true;
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Sprites["text"].Bitmap = new Bitmap(Size);
            Font f = Font.Get("Fonts/Ubuntu-B", 15);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = f;
            for (int i = 0; i < this.Tabs.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Names[i], i * 78 + 39, 3, Color.WHITE, DrawOptions.CenterAlign);
            }
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }

        public TabContainer CreateTab(string Name)
        {
            TabContainer tc = new TabContainer(this);
            tc.SetPosition(0, 29);
            tc.SetVisible(false);
            tc.SetSize(this.Size.Width, this.Size.Height - 29);
            this.Tabs.Add(tc);
            this.Names.Add(Name);
            return tc;
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (Sprites["header"].Bitmap as SolidBitmap).SetSize(Size.Width, 4);
            foreach (TabContainer tc in this.Tabs)
            {
                tc.SetSize(this.Size.Width, this.Size.Height - 29);
                tc.Widgets.ForEach(w => w.SetSize(tc.Size));
            }
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering)
            {
                HoveringIndex = -1;
                return;
            }
            int oldindex = HoveringIndex;
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            if (ry >= 25)
            {
                HoveringIndex = -1;
                return;
            }
            HoveringIndex = (int) Math.Floor(rx / 78d);
            if (HoveringIndex >= Tabs.Count) HoveringIndex = -1;
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