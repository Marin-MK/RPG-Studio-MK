namespace RPGStudioMK.Widgets;

public class NewMultilineTextBox : Widget
{
    public string Text { get { return TextArea.Text; } }

    Container ScrollContainer;
    NewMultilineTextArea TextArea;

    public NewMultilineTextBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        ScrollContainer = new Container(this);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(3, 3, 14, 3);
        ScrollContainer.OnHoverChanged += _ => Input.SetCursor(ScrollContainer.Mouse.Inside ? odl.SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM : odl.SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        TextArea = new NewMultilineTextArea(ScrollContainer);
        TextArea.SetHDocked(true);
        TextArea.SetFont(Fonts.CabinMedium.Use(9));
        TextArea.OnWidgetSelected.Invoke(new BaseEventArgs());
        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, 3, 0, 3);
        vs.ScrollStep = TextArea.LineHeight / 3f;
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;
        SetText("Welcome to the Pokémon Center! We heal your injured Pokémon for free, so they'll be feeling better in a flash and you can go on your way to greatness. Furthermore, please do not hesitate to stop by in the future, because the health of your Pokémon is of paramount importance to us. Have a nice day!");
        //SetText("Now comes the time to test.\nNow comes the time to test.\nNow comes the time to test.\nNow comes the time to test.\nThese strings have newline characters, so it's different.\nWill it work, or will it break?\nWe're about to find out I guess.\n\nAlso paragraphs are fun too!\nAnd I'm gonna end in a blank newline too.\n\n");
        //SetText("a\nb\nc\nd\ne\nf\ng\nh\ni\nj\nk\nl\nm\nn\no\np\nq\nr\ns\nt\nu\nv\nw\nx\ny\nz");
        //SetText("");
    }

    public void SetText(string Text)
    {
        TextArea.SetText(Text);
    }

    public void SetFont(Font Font)
    {
        TextArea.SetFont(Font);
    }

    public void SetTextColor(Color Color)
    {
        TextArea.SetTextColor(Color);
    }
    
    public void SetLineHeight(int LineHeight)
    {
        TextArea.SetLineHeight(LineHeight);
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

    protected override void Draw()
    {
        base.Draw();
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(Size, new Color(86, 108, 134));
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(10, 23, 37));
        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
        Color DarkOutline = new Color(40, 62, 84);
        Sprites["bg"].Bitmap.SetPixel(1, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.Lock();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (Mouse.LeftMouseTriggered)
        {
            if (ScrollContainer.Mouse.Inside) TextArea.OnWidgetSelected.Invoke(new BaseEventArgs());
            else Window.UI.SetSelectedWidget(null);
        }
    }
}
