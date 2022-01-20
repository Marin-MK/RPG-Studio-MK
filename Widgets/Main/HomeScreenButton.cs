using System;
namespace RPGStudioMK.Widgets;

public class HomeScreenButton : Widget
{
    public string Text { get; protected set; } = "";

    public HomeScreenButton(IContainer Parent) : base(Parent)
    {
        SetSize(237, 213);
        Sprites["bg"] = new Sprite(this.Viewport, "assets/img/home_button.png");
        Sprites["icon"] = new Sprite(this.Viewport);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].Y = 154;
    }

    public void SetIcon(string path)
    {
        if (Sprites["icon"].Bitmap != null) Sprites["icon"].Bitmap.Dispose();
        Sprites["icon"].Bitmap = new Bitmap(path);
        Sprites["icon"].X = 119 - Sprites["icon"].Bitmap.Width / 2;
        Sprites["icon"].Y = 83 - Sprites["icon"].Bitmap.Height / 2;
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            if (string.IsNullOrEmpty(Text)) return;
            Sprites["text"].Bitmap = new Bitmap(212, 47);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = Fonts.UbuntuBold.Use(20);
            Sprites["text"].Bitmap.DrawText(Text, 118, 12, Color.WHITE, DrawOptions.CenterAlign);
            Sprites["text"].Bitmap.Lock();
        }
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap($"assets/img/home_button{(WidgetIM.Hovering ? "_hovering" : "")}.png");
    }
}