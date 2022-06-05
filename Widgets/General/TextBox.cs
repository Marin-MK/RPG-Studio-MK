namespace RPGStudioMK.Widgets;

public class TextBox : amethyst.TextBox
{
    public bool Enabled { get; protected set; } = true;

    public TextBox(IContainer Parent) : base(Parent)
    {
        TextArea.SetPosition(6, 4);
        TextArea.SetFont(Fonts.CabinMedium.Use(11));
        TextArea.SetCaretColor(Color.WHITE);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(Size.Width - 12, Size.Height - 8);
        Redraw();
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

    protected override void Draw()
    {
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(this.Size);
        Sprites["box"].Bitmap.Unlock();
        Color lightgrey = new Color(121, 121, 122);
        Color darkgrey = new Color(96, 100, 100);
        Color filler = new Color(10, 23, 37);
        Sprites["box"].Bitmap.DrawRect(Size, lightgrey);
        Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, lightgrey);
        Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, filler);
        Sprites["box"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
        Sprites["box"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["box"].Bitmap.SetPixel(0, 1, darkgrey);
        Sprites["box"].Bitmap.SetPixel(1, 0, darkgrey);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 1, 1, darkgrey);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 2, 0, darkgrey);
        Sprites["box"].Bitmap.SetPixel(0, Size.Height - 2, darkgrey);
        Sprites["box"].Bitmap.SetPixel(1, Size.Height - 1, darkgrey);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 2, darkgrey);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 1, darkgrey);
        Sprites["box"].Bitmap.SetPixel(2, 2, darkgrey);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 3, 2, darkgrey);
        Sprites["box"].Bitmap.SetPixel(2, Size.Height - 3, darkgrey);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 3, Size.Height - 3, darkgrey);
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }
}
