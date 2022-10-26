using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;


public class DropdownBox : amethyst.TextBox
{
    public bool ReadOnly { get { return TextArea.ReadOnly; } }
    public int SelectedIndex { get; protected set; } = 0;
    public List<ListItem> Items { get; protected set; } = new List<ListItem>();
    public bool Enabled { get; protected set; } = true;

    public BaseEvent OnDropDownClicked;
    public BaseEvent OnSelectionChanged;

    DropdownWidget DropdownWidget;

    public DropdownBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        TextArea.SetPosition(6, 2);
        TextArea.SetFont(Fonts.Paragraph);
        TextArea.SetCaretColor(Color.WHITE);
        TextArea.SetReadOnly(true);
        MinimumSize.Height = MaximumSize.Height = 24;
        SetHeight(24);
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.TextArea.SetEnabled(Enabled);
            this.Redraw();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(this.Size.Width - 28, this.Size.Height - 3);
    }

    public void SetReadOnly(bool ReadOnly)
    {
        TextArea.SetReadOnly(ReadOnly);
        Redraw();
    }

    public void SetSelectedIndex(int Index)
    {
        if (this.SelectedIndex != Index)
        {
            this.TextArea.SetText(Index >= Items.Count || Index == -1 ? "" : Items[Index].Name);
            this.SelectedIndex = Index;
            this.OnSelectionChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void SetItems(List<ListItem> Items)
    {
        this.Items = Items;
        this.TextArea.SetText(SelectedIndex >= Items.Count || SelectedIndex == -1 ? "" : Items[SelectedIndex].Name);
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Color Filler = this.Enabled ? new Color(10, 23, 37) : new Color(24, 38, 53);
        Sprites["bg"].Bitmap.FillRect(Size.Width, Size.Height - 2, Filler);
        Color ArrowColor = new Color(86, 108, 134);
        Color ArrowShadow = new Color(17, 27, 38);
        Color LineColor = Mouse.Inside && this.Enabled ? Color.WHITE : ArrowColor;
        Sprites["bg"].Bitmap.FillRect(0, Size.Height - 2, Size.Width, 2, LineColor);
        int x = Size.Width - 18;
        int y = Size.Height / 2 - 4;
        if (DropdownWidget != null) y -= 5;
        Sprites["bg"].Bitmap.FillRect(x, y, 11, 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 3, y + 4, x + 7, y + 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 4, y + 5, x + 6, y + 5, ArrowColor);
        Sprites["bg"].Bitmap.SetPixel(x + 5, y + 6, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x, y + 2, x + 5, y + 7, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x, y + 3, x + 5, y + 8, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 6, x + 10, y + 2, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 7, x + 10, y + 3, ArrowShadow);
        if (DropdownWidget != null) Sprites["bg"].Bitmap.FlipVertically(x, y, 11, 11);
        Sprites["bg"].Bitmap.Lock();
        base.Draw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!TextArea.Mouse.Inside && TextArea.SelectedWidget)
        {
            Window.UI.SetSelectedWidget(null);
        }
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (rx >= Size.Width - 28 && rx < Size.Width &&
            ry >= 0 && ry < Size.Height && this.Enabled)
        {
            this.OnDropDownClicked?.Invoke(new BaseEventArgs());
            if (this.Items.Count > 0)
            {
                DropdownWidget = new DropdownWidget(Window.UI, this.Size.Width, this.Items, this);
                DropdownWidget.SetPosition(this.Viewport.X, this.Viewport.Y + this.Viewport.Height - 2);
                DropdownWidget.SetSelectedIndex(SelectedIndex);
                DropdownWidget.OnDisposed += delegate (BaseEventArgs e)
                {
                    if (DropdownWidget.SelectedIndex != -1)
                    {
                        this.SetSelectedIndex(DropdownWidget.SelectedIndex);
                    }
                    DropdownWidget = null;
                    Redraw();
                };
                Redraw();
            }
        };
    }
}

public class DropdownWidget : Widget
{
    public int SelectedIndex { get; protected set; } = -1;

    ListDrawer List;
    DropdownBox DropdownBox;

    public DropdownWidget(IContainer Parent, int Width, List<ListItem> Items, DropdownBox DropdownBox) : base(Parent)
    {
        this.DropdownBox = DropdownBox;

        SetZIndex(Window.ActiveWidget is UIManager ? 9 : (Window.ActiveWidget as Widget).ZIndex + 9);
        SetSize(Width, Math.Min(9, Items.Count) * 20 + 3);

        WindowLayer = Window.ActiveWidget.WindowLayer + 1;
        Window.SetActiveWidget(this);

        Container ScrollContainer = new Container(this);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(1, 2, 12, 1);

        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, 3, 0, 2);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        List = new ListDrawer(ScrollContainer);
        List.ForceMouseStart = true; // Allows the mouse to be captured immediately,
                                     // rather than having to press within the listbox boundaries for it to be captured
        List.SetHDocked(true);
        List.SetItems(Items);

        Sprites["bg"] = new Sprite(this.Viewport);
    }

    public override void Dispose()
    {
        if (this.Window.ActiveWidget == this)
        {
            this.Window.Widgets.RemoveAt(Window.Widgets.Count - 1);
            this.Window.SetActiveWidget(Window.Widgets[Window.Widgets.Count - 1]);
        }
        base.Dispose();
    }

    public void SetSelectedIndex(int SelectedIndex)
    {
        List.SetSelectedIndex(SelectedIndex);
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if ((Mouse.LeftMouseTriggered || Mouse.RightMouseTriggered) && !Mouse.Inside) Dispose();
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (List.Mouse.Inside && List.HoveringIndex != -1)
        {
            if (Mouse.LeftStartedInside) this.SelectedIndex = List.SelectedIndex;
            if (DropdownBox.Mouse.LeftStartedInside) this.SelectedIndex = List.HoveringIndex;
            Dispose();
        }
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Color Outline = new Color(32, 170, 221);
        Color Filler = new Color(10, 23, 37);
        Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, Outline);
        Sprites["bg"].Bitmap.DrawLine(1, 1, Size.Width - 2, 1, Outline);
        Sprites["bg"].Bitmap.FillRect(1, 2, Size.Width - 2, Size.Height - 3, Filler);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 2, Size.Width - 12, Size.Height - 2, 40, 62, 84);
        Sprites["bg"].Bitmap.Lock();
        base.Draw();
    }
}