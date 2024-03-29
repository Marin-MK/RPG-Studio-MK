﻿

namespace RPGStudioMK.Widgets;

public class GradientButton : Widget
{
    public bool Enabled { get; protected set; } = true;
    public string Text { get; protected set; } = "";

    public GradientButton(IContainer Parent, string Text) : base(Parent)
    {
        this.Text = Text;

        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["text"] = new Sprite(this.Viewport);
        Font f = Fonts.Header;
        Size s = f.TextSize(Text);
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Font = f;
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(Text, Color.WHITE);
        Sprites["text"].Bitmap.Lock();
        Sprites["text"].X = 40 - s.Width / 2;
        Sprites["text"].Y = 5;

        MinimumSize = MaximumSize = new Size(80, 30);
        SetSize(80, 30);
    }

    public void SetButtonColor(Colors Color)
    {
        int srcy = Sprites["bg"].SrcRect.Y;
        Sprites["bg"].Bitmap?.Dispose();
        string filename = "assets/img/gradient_button_";
        if (Color == Colors.Red) filename += "red";
        else if (Color == Colors.Green) filename += "green";
        else filename += "blue";
        Sprites["bg"].Bitmap = new Bitmap(filename);
        Sprites["bg"].SrcRect.Height = 30;
        Sprites["bg"].SrcRect.Y = srcy;
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["bg"].SrcRect.Y = Enabled ? (Mouse.Inside ? 30 : 0) : 60;
        Sprites["bg"].Update();
    }

    public enum Colors
    {
        Red,
        Green,
        Blue
    }
}
