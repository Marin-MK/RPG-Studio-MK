using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class SubmodeView : Widget
{
    public List<TabContainer> Tabs = new List<TabContainer>();
    public List<string> Names = new List<string>();
    public int SelectedIndex { get; protected set; } = -1;
    private int HoveringIndex = -1;
    public int HeaderWidth { get; protected set; } = 78;
    public int HeaderHeight { get; protected set; } = 25;
    public int HeaderSelHeight { get; protected set; } = 4;
    public int TextY { get; protected set; } = 3;
    public Font Font { get; protected set; }
    public bool Centered { get; protected set; } = false;
    public Color HeaderBackgroundColor { get; protected set; } = Color.ALPHA;
    public Color HeaderSelBackgroundColor { get; protected set; } = Color.ALPHA;

    public BaseEvent OnSelectionChanged;

    public SubmodeView(IContainer Parent) : base(Parent)
    {
        this.Font = Fonts.TabFont;
        Sprites["header"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, HeaderBackgroundColor));
        Sprites["sel"] = new Sprite(this.Viewport, new SolidBitmap(HeaderWidth, 2, new Color(55, 187, 255)));
        Sprites["sel"].Y = HeaderHeight - HeaderSelHeight - 2;
        Sprites["selbg"] = new Sprite(this.Viewport, new SolidBitmap(HeaderWidth, HeaderHeight, HeaderSelBackgroundColor));
        Sprites["selbg"].Visible = false;
        Sprites["text"] = new Sprite(this.Viewport);
    }

    public void SelectTab(int Index)
    {
        if (Tabs.Count > 0)
        {
            if (SelectedIndex != -1 && SelectedIndex < Tabs.Count)
            {
                Tabs[SelectedIndex].SetVisible(false);
                Sprites["selbg"].Visible = false;
            }
            if (Index != -1 && Index < Tabs.Count)
            {
                Tabs[Index].SetVisible(true);
                this.SelectedIndex = Index;
                this.OnSelectionChanged?.Invoke(new BaseEventArgs());
                Sprites["selbg"].X = Index * HeaderWidth;
                Sprites["selbg"].Visible = true;
            }
            else Sprites["selbg"].Visible = false;
        }
        else Sprites["selbg"].Visible = false;
        Redraw();
    }

    public TabContainer GetTab(int Index)
    {
        return this.Tabs[Index];
    }

    public void SetHeaderWidth(int Width)
    {
        if (this.HeaderWidth != Width)
        {
            this.HeaderWidth = Width;
            Sprites["sel"].X = HoveringIndex * HeaderWidth;
            (Sprites["sel"].Bitmap as SolidBitmap).SetSize(HeaderWidth, 2);
            ((SolidBitmap) Sprites["selbg"].Bitmap).SetSize(HeaderWidth, HeaderHeight);
            Redraw();
        }
    }

    public void SetHeaderHeight(int Height)
    {
        if (this.HeaderHeight != Height)
        {
            this.HeaderHeight = Height;
            Sprites["sel"].Y = HeaderHeight - HeaderSelHeight - 2;
            ((SolidBitmap) Sprites["header"].Bitmap).SetSize(Size.Width, HeaderHeight);
            ((SolidBitmap) Sprites["selbg"].Bitmap).SetSize(HeaderWidth, HeaderHeight);
        }
    }

    public void SetTextY(int TextY)
    {
        if (this.TextY != TextY)
        {
            this.TextY = TextY;
            Redraw();
        }
    }

    public void SetHeaderSelHeight(int HeaderSelHeight)
    {
        if (this.HeaderSelHeight != HeaderSelHeight)
        {
            this.HeaderSelHeight = HeaderSelHeight;
            Sprites["sel"].Y = HeaderHeight - HeaderSelHeight - 2;
        }
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            Redraw();
        }
    }

    public void SetCentered(bool Centered)
    {
        if (this.Centered != Centered)
        {
            this.Centered = Centered;
            if (Sprites["text"].Bitmap != null && Centered)
            {
                Sprites["text"].X = Size.Width / 2 - Sprites["text"].Bitmap.Width / 2;
            }
            else if (!Centered)
            {
                Sprites["text"].X = 0;
            }
        }
    }

    public void SetHeaderBackgroundColor(byte R, byte G, byte B, byte A = 255)
    {
        SetHeaderBackgroundColor(new Color(R, G, B, A));
    }

    public void SetHeaderBackgroundColor(Color HeaderBackgroundColor)
    {
        if (this.HeaderBackgroundColor != HeaderBackgroundColor)
        {
            this.HeaderBackgroundColor = HeaderBackgroundColor;
            ((SolidBitmap) Sprites["header"].Bitmap).SetColor(HeaderBackgroundColor);
        }
    }

    public void SetHeaderSelBackgroundColor(byte R, byte G, byte B, byte A = 255)
    {
        SetHeaderSelBackgroundColor(new Color(R, G, B, A));
    }

    public void SetHeaderSelBackgroundColor(Color HeaderSelBackgroundColor)
    {
        if (this.HeaderSelBackgroundColor != HeaderSelBackgroundColor)
        {
            this.HeaderSelBackgroundColor = HeaderSelBackgroundColor;
            ((SolidBitmap) Sprites["selbg"].Bitmap).SetColor(HeaderSelBackgroundColor);
        }
    }

    protected override void Draw()
    {
        if (SelectedIndex == -1)
        {
            if (Tabs.Count > 0) SelectTab(0);
        }
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        for (int i = 0; i < this.Tabs.Count; i++)
        {
            int headerw = this.Font.TextSize(Names[i]).Width;
            if (headerw + 8 >= this.HeaderWidth) SetHeaderWidth(headerw + 8);
        }
        Sprites["text"].Bitmap = new Bitmap(HeaderWidth * Tabs.Count, Size.Height);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        for (int i = 0; i < this.Tabs.Count; i++)
        {
            Color c = Color.WHITE;
            if (i == SelectedIndex) c = new Color(55, 187, 255);
            Sprites["text"].Bitmap.DrawText(Names[i], i * HeaderWidth + HeaderWidth / 2, TextY, c, DrawOptions.CenterAlign);
        }
        Sprites["text"].Bitmap.Lock();
        if (Centered) Sprites["text"].X = Size.Width / 2 - Sprites["text"].Bitmap.Width / 2;
        base.Draw();
    }

    public TabContainer CreateTab(string Name)
    {
        TabContainer tc = new TabContainer(this);
        tc.SetPosition(0, HeaderHeight);
        tc.SetVisible(false);
        tc.SetSize(this.Size.Width, this.Size.Height - HeaderHeight);
        this.Tabs.Add(tc);
        this.Names.Add(Name);
        return tc;
    }

    public TabContainer InsertTab(int Index, string Name)
    {
        TabContainer tc = new TabContainer(this);
        tc.SetPosition(0, HeaderHeight);
        tc.SetVisible(false);
        tc.SetSize(this.Size.Width, this.Size.Height - HeaderHeight);
        this.Tabs.Insert(Index, tc);
        this.Names.Insert(Index, Name);
        return tc;
    }

    public void RemoveTab(int Index)
    {
        this.Tabs[Index].Dispose();
        this.Tabs.RemoveAt(Index);
        this.Names.RemoveAt(Index);
        if (this.SelectedIndex > 0 && this.SelectedIndex == this.Tabs.Count) this.SelectTab(Index - 1);
        else this.SelectTab(this.SelectedIndex);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        foreach (TabContainer tc in this.Tabs)
        {
            tc.SetSize(this.Size.Width, this.Size.Height - HeaderHeight);
            tc.Widgets.ForEach(w => w.SetSize(tc.Size));
        }
        (Sprites["header"].Bitmap as SolidBitmap).SetSize(Size.Width, HeaderHeight);
        Sprites["sel"].Y = HeaderHeight - HeaderSelHeight - 2;
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!Mouse.Inside)
        {
            HoveringIndex = -1;
            Sprites["sel"].Visible = false;
            return;
        }
        int oldindex = HoveringIndex;
        int rx = e.X - this.Viewport.X;
        int ry = e.Y - this.Viewport.Y;
        if (ry >= HeaderHeight)
        {
            HoveringIndex = -1;
            Sprites["sel"].Visible = false;
            return;
        }
        Sprites["sel"].Visible = true;
        HoveringIndex = (int)Math.Floor((rx - Sprites["text"].X) / (double)HeaderWidth);
        if (HoveringIndex >= Tabs.Count || HoveringIndex < 0)
        {
            HoveringIndex = -1;
            Sprites["sel"].Visible = false;
            return;
        }
        Sprites["sel"].X = Sprites["text"].X + HoveringIndex * HeaderWidth;
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
