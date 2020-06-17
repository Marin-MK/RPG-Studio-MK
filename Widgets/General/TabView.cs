using System;
using System.Collections.Generic;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class TabView : Widget
    {
        public  List<TabContainer> Tabs          = new List<TabContainer>();
        public  List<string>       Names         = new List<string>();
        public  int                SelectedIndex { get; protected set; } = -1;
        private int                HoveringIndex = -1;
        public  Color              HeaderColor   { get; protected set; } = new Color(28, 50, 73);
        public  int                HeaderWidth   { get; protected set; } = 78;
        public  int                HeaderHeight  { get; protected set; } = 25;
        public  int                TextY         { get; protected set; } = 3;
        public  int                XOffset       { get; protected set; } = 0;

        public BaseEvent OnSelectionChanged;

        public TabView(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(HeaderWidth, HeaderHeight, HeaderColor));
            Sprites["header"] = new Sprite(this.Viewport, new SolidBitmap(1, 4, HeaderColor));
            Sprites["header"].Y = HeaderHeight;
            Sprites["text"] = new Sprite(this.Viewport);
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
                    Sprites["bg"].X = XOffset + Index * HeaderWidth;
                    Sprites["bg"].Visible = true;
                    this.OnSelectionChanged?.Invoke(new BaseEventArgs());
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

        public void SetHeaderColor(byte R, byte G, byte B, byte A = 255)
        {
            SetHeaderColor(new Color(R, G, B, A));
        }
        public void SetHeaderColor(Color c)
        {
            ((SolidBitmap) Sprites["bg"].Bitmap).SetColor(c);
            ((SolidBitmap) Sprites["header"].Bitmap).SetColor(c);
        }

        public void SetXOffset(int Offset)
        {
            if (this.XOffset != Offset)
            {
                this.XOffset = Offset;
                this.Redraw();
            }
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
            Sprites["bg"].X = XOffset + SelectedIndex * HeaderWidth;
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Sprites["text"].Bitmap = new Bitmap(Size);
            Font f = Font.Get("Fonts/Ubuntu-B", 15);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = f;
            for (int i = 0; i < this.Tabs.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Names[i], XOffset + i * HeaderWidth + HeaderWidth / 2, TextY, Color.WHITE, DrawOptions.CenterAlign);
            }
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }

        public void SetHeader(int Width, int Height, int TextY)
        {
            if (this.HeaderWidth != Width || this.HeaderHeight != Height || this.TextY != TextY)
            {
                this.HeaderWidth = Width;
                this.HeaderHeight = Height;
                this.TextY = TextY;
                Sprites["header"].Y = HeaderHeight;
                (Sprites["bg"].Bitmap as SolidBitmap).SetSize(HeaderWidth, HeaderHeight);
                Redraw();
            }
        }

        public TabContainer CreateTab(string Name)
        {
            TabContainer tc = new TabContainer(this);
            tc.SetPosition(0, HeaderHeight + 4);
            tc.SetVisible(false);
            tc.SetSize(this.Size.Width, this.Size.Height - HeaderHeight - 4);
            Font f = Font.Get("Fonts/Ubuntu-B", 15);
            int w = f.TextSize(Name).Width + 8;
            if (w > HeaderWidth) SetHeader(w, HeaderHeight, TextY);
            this.Tabs.Add(tc);
            this.Names.Add(Name);
            return tc;
        }

        public void DestroyTab(int Index)
        {
            this.Tabs[Index].Dispose();
            this.Tabs.RemoveAt(Index);
            this.Names.RemoveAt(Index);
        }

        public void SetName(int PageIndex, string Name)
        {
            Font f = Font.Get("Fonts/Ubuntu-B", 15);
            int w = f.TextSize(Name).Width + 8;
            if (w > HeaderWidth) SetHeader(w, HeaderHeight, TextY);
            this.Names[PageIndex] = Name;
            this.Redraw();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            (Sprites["header"].Bitmap as SolidBitmap).SetSize(Size.Width, 4);
            foreach (TabContainer tc in this.Tabs)
            {
                tc.SetSize(this.Size.Width, this.Size.Height - HeaderHeight - 4);
                tc.Widgets.ForEach(w => w.SetSize(tc.Size));
            }
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            if (!WidgetIM.Hovering)
            {
                HoveringIndex = -1;
                return;
            }
            int oldindex = HoveringIndex;
            int rx = e.X - this.Viewport.X - XOffset;
            int ry = e.Y - this.Viewport.Y;
            if (rx < 0 || ry >= HeaderHeight)
            {
                HoveringIndex = -1;
                return;
            }
            HoveringIndex = (int) Math.Floor(rx / (double) HeaderWidth);
            if (HoveringIndex >= Tabs.Count) HoveringIndex = -1;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
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
        public TabContainer(IContainer Parent) : base(Parent)
        {

        }
    }
}
