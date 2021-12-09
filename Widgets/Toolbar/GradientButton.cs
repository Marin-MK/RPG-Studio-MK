using System;

namespace RPGStudioMK.Widgets;

public class GradientButton : Widget
{
    public string Text { get; private set; } = "";

    public new BaseEvent OnLeftClick;

    public GradientButton(IContainer Parent, string Text) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["bg"].Bitmap = new Bitmap(1, 31);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawGradientLine(0, 0, 0, 30, new Color(47, 160, 193), new Color(90, 90, 201));
        Sprites["bg"].Bitmap.Lock();

        Sprites["text"] = new Sprite(this.Viewport);
        Font f = Fonts.UbuntuBold.Use(18);
        Size s = f.TextSize(Text);
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Font = f;
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(Text, Color.WHITE);
        Sprites["text"].Bitmap.Lock();
        Sprites["text"].X = 16;
        Sprites["text"].Y = 5;

        SetSize(32 + s.Width, 31);
    }

    public void SetGradient(Color c1, Color c2)
    {
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawGradientLine(0, 0, 0, 30, c1, c2);
        Sprites["bg"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["bg"].ZoomX = Size.Width;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton && e.LeftButton)
        {
            OnLeftClick?.Invoke(new BaseEventArgs());
        }
    }
}
