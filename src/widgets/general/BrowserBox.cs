

namespace RPGStudioMK.Widgets;

public class BrowserBox : Widget
{
    public string Text => TextArea.Text;
    public Font Font => TextArea.Font;
    public Color TextColor => TextArea.TextColor;
    public bool ReadOnly => TextArea.ReadOnly;
    public bool Enabled { get; protected set; } = true;

    public TextArea TextArea;
    
    public TextEvent OnTextChanged { get { return TextArea.OnTextChanged; } set { TextArea.OnTextChanged = value; } }
    public BaseEvent OnDropDownClicked;

    bool HoveringArrow;
    bool PressingArrow;
    bool ClickedInside;

    public BrowserBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        TextArea = new TextArea(this);
        TextArea.SetPosition(3, 3);
        TextArea.SetZIndex(1);
        TextArea.SetReadOnly(true);
        MinimumSize.Height = MaximumSize.Height = 25;
        SetFont(Fonts.Paragraph);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(this.Size.Width - 29, this.Size.Height - 3);
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.Redraw();
            this.TextArea.SetEnabled(Enabled);
        }
    }

    public void SetText(string Text)
    {
        TextArea.SetText(Text);
    }

    public void SetFont(Font f)
    {
        TextArea.SetFont(f);
    }

    public void SetTextColor(Color c)
    {
        TextArea.SetTextColor(c);
    }

    public void SetReadOnly(bool ReadOnly)
    {
        TextArea.SetReadOnly(ReadOnly);
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, 86, 108, 134);
        Color FillerColor = this.Enabled ? new Color(10, 23, 37) : new Color(24, 38, 53);
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, FillerColor);
        Color OutlineColor = new Color(86, 108, 134);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 25, 1, Size.Width - 25, Size.Height - 2, OutlineColor);
        Color ArrowColor = this.Enabled ? (PressingArrow ? new Color(32, 170, 221) : HoveringArrow ? Color.WHITE : OutlineColor) : OutlineColor;
        if (PressingArrow && !HoveringArrow) ArrowColor = Color.WHITE;

        if (this.Enabled && (HoveringArrow || PressingArrow))
        {
            Sprites["bg"].Bitmap.DrawRect(Size.Width - 25, 0, 24, Size.Height, ArrowColor);
        }

        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);

        int x = Size.Width - 18;
        int y = 7;
        Sprites["bg"].Bitmap.FillRect(x + 2, y, 2, 11, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 4, y + 1, x + 4, y + 9, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 5, y + 2, x + 5, y + 8, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 3, x + 6, y + 7, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 7, y + 4, x + 7, y + 6, ArrowColor);
        Sprites["bg"].Bitmap.SetPixel(x + 8, y + 5, ArrowColor);

        Sprites["bg"].Bitmap.Lock();

        base.Draw();
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        bool OldHoveringArrow = HoveringArrow;
        if (rx >= Size.Width - 25 && rx < Size.Width && ry >= 0 && ry < Size.Height)
        {
            HoveringArrow = true;
        }
        else
        {
            HoveringArrow = false;
        }
        if (OldHoveringArrow != HoveringArrow) Redraw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (Mouse.LeftMouseTriggered)
        {
            if (!Mouse.Inside && this.SelectedWidget)
            {
                Window.UI.SetSelectedWidget(null);
                return;
            }
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= Size.Width - 25 && rx < Size.Width &&
                ry >= 0 && ry < Size.Height && this.Enabled)
            {
                ClickedInside = true;
                PressingArrow = true;
                Redraw();
            };
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (Mouse.LeftMouseReleased)
        {
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= Size.Width - 25 && rx < Size.Width && ry >= 0 && ry < Size.Height && this.Enabled && ClickedInside)
            {
                this.OnDropDownClicked?.Invoke(new BaseEventArgs());
            }
            PressingArrow = false;
            Redraw();
        }
    }
}
