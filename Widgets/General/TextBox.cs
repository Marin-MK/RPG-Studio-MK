namespace RPGStudioMK.Widgets;

public class TextBox : amethyst.TextBox
{
    public bool Enabled { get; protected set; } = true;
    public bool PopupStyle { get; protected set; } = true;

    public TextBox(IContainer Parent) : base(Parent)
    {
        TextArea.SetPosition(6, 4);
        TextArea.SetFont(Fonts.CabinMedium.Use(11));
        TextArea.SetCaretColor(Color.WHITE);
        TextArea.OnWidgetSelected += _ => Redraw();
        TextArea.OnWidgetDeselected += _ => Redraw();
        TextArea.OnHoverChanged += _ => Redraw();
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
        if (this.PopupStyle)
        {
            Color Edge = TextArea.SelectedWidget ? new Color(32, 170, 221) : TextArea.Mouse.Inside ? Color.WHITE : new Color(86, 108, 134);
            Color Outline = new Color(36, 34, 36);
            Color Filler = this.Enabled ? new Color(86, 108, 134) : new Color(40, 62, 84);
            Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, Outline);
            Sprites["box"].Bitmap.SetPixel(1, 1, Edge);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, 1, Edge);
            Sprites["box"].Bitmap.SetPixel(1, Size.Height - 2, Edge);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, Edge);
            Sprites["box"].Bitmap.DrawLine(2, 0, Size.Width - 3, 0, Edge);
            Sprites["box"].Bitmap.DrawLine(0, 2, 0, Size.Height - 3, Edge);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 1, 2, Size.Width - 1, Size.Height - 3, Edge);
            Sprites["box"].Bitmap.DrawLine(2, Size.Height - 1, Size.Width - 3, Size.Height - 1, Edge);
            Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, Filler);
        }
        else
        {
            Color Edge = new Color(121, 121, 122);
            Color Detail = new Color(96, 100, 100);
            Color Filler = new Color(10, 23, 37);
            Sprites["box"].Bitmap.DrawRect(Size, Edge);
            Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, Edge);
            Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, Filler);
            Sprites["box"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(0, 1, Detail);
            Sprites["box"].Bitmap.SetPixel(1, 0, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, 1, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, 0, Detail);
            Sprites["box"].Bitmap.SetPixel(0, Size.Height - 2, Detail);
            Sprites["box"].Bitmap.SetPixel(1, Size.Height - 1, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 2, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 1, Detail);
            Sprites["box"].Bitmap.SetPixel(2, 2, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 3, 2, Detail);
            Sprites["box"].Bitmap.SetPixel(2, Size.Height - 3, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 3, Size.Height - 3, Detail);
        }
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }
}
