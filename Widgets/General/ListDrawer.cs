using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class ListDrawer : Widget
{
    public BaseEvent OnSelectionChanged;
    public BaseEvent OnDoubleClicked;

    public Font Font { get; protected set; }
    public int LineHeight { get; protected set; } = 20;
    public List<ListItem> Items { get; protected set; } = new List<ListItem>();
    public bool Enabled { get; protected set; } = true;
    public int SelectedIndex { get; protected set; } = -1;
    public int HoveringIndex { get; protected set; } = -1;
    public bool ForceMouseStart = false;
    public ListItem SelectedItem { get { return SelectedIndex == -1 ? null : Items[SelectedIndex]; } }
    public ListItem HoveringItem { get { return HoveringIndex == -1 ? null : Items[HoveringIndex]; } }
    public Color SelectedItemColor { get; protected set; } = new Color(55, 187, 255);

    public bool CountRightMouseClicks = false;
    int DraggingIndex = -1;

    public ListDrawer(IContainer Parent) : base(Parent)
    {
        this.Font = Fonts.Paragraph;
        Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width, 20, new Color(28, 50, 73)));
        Sprites["selection"].Visible = false;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 20, new Color(59, 227, 255)));
        Sprites["hover"].Visible = false;
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.Redraw();
        }
    }

    public void SetItems(List<ListItem> Items)
    {
        this.Items = Items;
        Redraw();
        if (SelectedIndex >= Items.Count) SetSelectedIndex(Items.Count - 1);
    }

    public void SetFont(Font f)
    {
        if (this.Font != f)
        {
            this.Font = f;
            Redraw();
        }
    }

    public void SetLineHeight(int Height)
    {
        if (this.LineHeight != Height)
        {
            this.LineHeight = Height;
            if (SelectedIndex != -1) Sprites["selection"].Y = LineHeight * SelectedIndex;
            ((SolidBitmap)Sprites["hover"].Bitmap).SetSize(2, LineHeight);
            Redraw();
        }
    }

    public void SetSelectedItemColor(Color SelectedItemColor)
    {
        if (this.SelectedItemColor != SelectedItemColor)
        {
            this.SelectedItemColor = SelectedItemColor;
            Redraw();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        (Sprites["selection"].Bitmap as SolidBitmap).SetSize(Size.Width, LineHeight);
    }

    public void SetSelectedIndex(int Index, bool ForceRefresh = false)
    {
        if (this.SelectedIndex != Index || ForceRefresh)
        {
            this.SelectedIndex = Index;
            if (Index == -1)
            {
                Sprites["selection"].Visible = false;
            }
            else
            {
                Sprites["selection"].Visible = true;
                Sprites["selection"].Y = LineHeight * Index;
            }
            this.Redraw();
            this.OnSelectionChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void ForceRedraw()
    {
        this.Draw();
        MouseMoving(Graphics.LastMouseEvent);
    }

    protected override void Draw()
    {
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        SetSize(Size.Width, LineHeight * Items.Count);
        Sprites["text"].Bitmap = new Bitmap(Size);
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.Unlock();
        for (int i = 0; i < this.Items.Count; i++)
        {
            bool sel = i == SelectedIndex;
            if (DraggingIndex != -1) sel = i == DraggingIndex;
            Color c = this.Enabled ? (sel ? this.SelectedItemColor : Color.WHITE) : new Color(72, 72, 72);
            Sprites["text"].Bitmap.DrawText(this.Items[i].ToString(), 10, LineHeight * i + LineHeight / 2 - 10, c);
        }
        Sprites["text"].Bitmap.Lock();
        base.Draw();
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!Parent.Mouse.LeftStartedInside && !Parent.Mouse.RightStartedInside && !ForceMouseStart) return;
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
        int index = (int) Math.Floor(ry / (double)LineHeight);
        int oldhover = HoveringIndex;
        HoveringIndex = -1;
        Sprites["hover"].Visible = false;
        if (ry < 0 || index >= this.Items.Count) return;
        int olddrag = DraggingIndex;
        if (Mouse.Inside)
        {
            Sprites["hover"].Visible = true;
            Sprites["hover"].Y = index * LineHeight;
            HoveringIndex = index;
            if (Mouse.LeftMousePressed || CountRightMouseClicks && Mouse.RightMousePressed)
            {
                Sprites["selection"].Y = Sprites["hover"].Y;
                Sprites["selection"].Visible = true;
                DraggingIndex = HoveringIndex;
            }
        }
        if (HoveringIndex != oldhover || DraggingIndex != olddrag) Redraw();
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        if (!Mouse.Inside) Sprites["hover"].Visible = false;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        MouseMoving(e);
    }

    public override void DoubleLeftMouseDownInside(MouseEventArgs e)
    {
        base.DoubleLeftMouseDownInside(e);
        this.OnDoubleClicked?.Invoke(new BaseEventArgs());
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (DraggingIndex != -1 && ((Parent.Mouse.LeftStartedInside || ForceMouseStart) && Mouse.LeftMouseReleased || Parent.Mouse.RightStartedInside && Mouse.RightMouseReleased && CountRightMouseClicks))
        {
            int idx = DraggingIndex;
            DraggingIndex = -1;
            this.SetSelectedIndex(idx);
        }
    }
}
