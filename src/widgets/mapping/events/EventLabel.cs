using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventLabel : Widget
{
    public Event Event;
    public bool Selected { get; protected set; }

    public EventLabel(IContainer Parent) : base(Parent)
    {
        Sprites["sel"] = new Sprite(this.Viewport, new SolidBitmap(272, 29, new Color(19, 36, 55)));
        Sprites["sel"].Visible = false;
        Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(3, 28, new Color(0, 205, 255)));
        Sprites["hover"].Visible = false;
        Sprites["name"] = new Sprite(this.Viewport);
        SetSize(272, 29);
    }

    public void SetEvent(Event Event)
    {
        this.Event = Event;
        this.Redraw();
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["hover"].Visible = Mouse.Inside;
    }

    public void UpdateHoverLabel()
    {
        Sprites["hover"].Visible = Mouse.Inside;
    }

    public void SetSelected(bool Selected)
    {
        if (this.Selected != Selected)
        {
            this.Selected = Selected;
            Sprites["sel"].Visible = this.Selected;
            this.Redraw();
        }
    }

    protected override void Draw()
    {
        Sprites["name"].Bitmap?.Dispose();
        string Text = $"{Utilities.Digits(Event.ID, 3)}: {Event.Name}";
        Size s = Fonts.Paragraph.TextSize(Text);
        Sprites["name"].Bitmap = new Bitmap(s);
        Sprites["name"].Bitmap.Font = Fonts.Paragraph;
        Sprites["name"].Bitmap.Unlock();
        Sprites["name"].Bitmap.DrawText(Text, this.Selected ? new Color(58, 184, 243) : Color.WHITE);
        Sprites["name"].Bitmap.Lock();
        Sprites["name"].X = 9;
        Sprites["name"].Y = Size.Height / 2 - s.Height / 2;
        base.Draw();
    }
}
