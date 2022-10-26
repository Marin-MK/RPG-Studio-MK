using System;

namespace RPGStudioMK.Widgets;

public class ScriptEditorTextBox : Widget
{
    public string Text => TextArea.Text;
    public Font Font => TextArea.Font;
    public Color TextColor => TextArea.TextColor;
    public int LineHeight => TextArea.LineHeight;
    public Color TextColorSelected => TextArea.TextColorSelected;
    public bool OverlaySelectedText => TextArea.OverlaySelectedText;
    public Color SelectionBackgroundColor => TextArea.SelectionBackgroundColor;
    public bool LineWrapping => TextArea.LineWrapping;
    public Color LineTextColor => TextArea.LineTextColor;
    public Color LineTextBackgroundColor => TextArea.LineTextBackgroundColor;
    public int LineTextWidth => TextArea.LineTextWidth;
    public int TextXOffset => TextArea.TextXOffset;

    public BaseEvent OnTextChanged { get => TextArea.OnTextChanged; set => TextArea.OnTextChanged = value; }
    public BoolEvent OnCopy { get => TextArea.OnCopy; set => TextArea.OnCopy = value; }
    public BoolEvent OnPaste { get => TextArea.OnPaste; set => TextArea.OnPaste = value; }

    Container ScrollContainer;
    ScriptEditorTextArea TextArea;

    public ScriptEditorTextBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);

        ScrollContainer = new Container(this);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(3, 3, 14, 3);
        ScrollContainer.OnMouseMoving += e =>
        {
            bool InsideTextArea = ScrollContainer.Mouse.Inside && e.X - ScrollContainer.Viewport.X >= TextArea.TextXOffset;
            Input.SetCursor(InsideTextArea ? CursorType.IBeam : CursorType.Arrow);
        };
        ScrollContainer.OnMouseWheel += e =>
        {
            if (!Input.Press(Keycode.CTRL)) return;
            int add = Math.Sign(e.WheelY);
            TextArea.SetFont(Font.Get(Fonts.Monospace.Name, Math.Max(1, TextArea.Font.Size + add)));
        };

        TextArea = new ScriptEditorTextArea(ScrollContainer);
        TextArea.SetHDocked(true);
        TextArea.MinimumSize.Height = Size.Height;

        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, 3, 0, 3);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;
        double oldval = vs.Value;
        vs.OnValueChanged += _ => TextArea.WidgetSelected(new BaseEventArgs());

        HScrollBar hs = new HScrollBar(this);
        hs.SetHDocked(true);
        hs.SetBottomDocked(true);
        hs.SetPadding(3, 0, 13, 0);
        ScrollContainer.SetHScrollBar(hs);
        ScrollContainer.HAutoScroll = true;
        hs.OnValueChanged += _ => TextArea.WidgetSelected(new BaseEventArgs());

        TextArea.Update();

        SetLineWrapping(false);
    }

    public void UpdateSize()
    {
        TextArea.TokenizeUntokenizedLines();
    }

    private void UpdateScrollBar()
    {
        //ScrollContainer.VScrollBar.MinScrollStep = (TextArea.LineHeight + TextArea.LineMargins) / 1f;
        //ScrollContainer.VScrollBar.ScrollStep = (float) ScrollContainer.VScrollBar.MinScrollStep / 3f;
    }

    public void SetText(string Text, bool SetCaretToEnd = false)
    {
        TextArea.SetText(Text, SetCaretToEnd);
    }

    public void SetFont(Font Font)
    {
        TextArea.SetFont(Font);
        UpdateScrollBar();
    }

    public void SetTextColor(Color Color)
    {
        TextArea.SetTextColor(Color);
    }

    public void SetLineHeight(int LineHeight)
    {
        TextArea.SetLineHeight(LineHeight);
        UpdateScrollBar();
    }

    public void SetTextColorSelected(Color Color)
    {
        TextArea.SetTextColorSelected(Color);
    }

    public void SetOverlaySelectedText(bool OverlaySelectedText)
    {
        TextArea.SetOverlaySelectedText(OverlaySelectedText);
    }

    public void SetSelectionBackgroundColor(Color Color)
    {
        TextArea.SetSelectionBackgroundColor(Color);
    }

    public void SetLineMargins(int LineMargins)
    {
        TextArea.SetLineMargins(LineMargins);
    }

    public void SetLineWrapping(bool LineWrapping)
    {
        if (LineWrapping)
        {
            TextArea.SetHDocked(true);
            ScrollContainer.SetPadding(3, 3, 14, 3);
            ScrollContainer.VScrollBar.SetPadding(0, 3, 0, 3);
            ScrollContainer.HScrollBar.SetVisible(false);
            ScrollContainer.HAutoScroll = false;
        }
        else
        {
            TextArea.SetHDocked(false);
            ScrollContainer.SetPadding(3, 3, 14, 14);
            ScrollContainer.VScrollBar.SetPadding(0, 3, 0, 13);
            ScrollContainer.HAutoScroll = true;
        }
        this.Redraw();
        TextArea.SetLineWrapping(LineWrapping);
    }

    public void SetLineTextColor(Color LineTextColor)
    {
        TextArea.SetLineTextColor(LineTextColor);
    }

    public void SetLineTextBackgroundColor(Color LineTextBackgroundColor)
    {
        TextArea.SetLineTextBackgroundColor(LineTextBackgroundColor);
    }

    public void SetLineTextWidth(int LineTextWidth)
    {
        TextArea.SetLineTextWidth(LineTextWidth);
    }

    public void SetTextXOffset(int TextXOffset)
    {
        TextArea.SetTextXOffset(TextXOffset);
    }

    public void Activate()
    {
        TextArea.OnWidgetSelected.Invoke(new BaseEventArgs());
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(Size, new Color(86, 108, 134));
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(10, 23, 37));
        if (!LineWrapping) Sprites["bg"].Bitmap.FillRect(Size.Width - 12, Size.Height - 12, 11, 11, new Color(64, 104, 146));
        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
        Color DarkOutline = new Color(40, 62, 84);
        Sprites["bg"].Bitmap.SetPixel(1, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, DarkOutline);
        if (LineWrapping) Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, DarkOutline);
        if (!LineWrapping) Sprites["bg"].Bitmap.DrawLine(1, Size.Height - 12, Size.Width - 2, Size.Height - 12, DarkOutline);
        Sprites["bg"].Bitmap.Lock();
    }

    public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (ScrollContainer.Mouse.Inside) TextArea.OnWidgetSelected.Invoke(new BaseEventArgs());
        else Window.UI.SetSelectedWidget(null);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.MinimumSize.Height = Size.Height;
    }
}
