using System;

namespace RPGStudioMK.Widgets;

public class GroupBox : Widget
{
    public Color OuterLineColor { get; protected set; } = new Color(59, 91, 124);
    public Color InnerLineColor { get; protected set; } = new Color(17, 27, 38);
    public Color InnerColor { get; protected set; } = new Color(24, 38, 53);

    public GroupBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
    }

    public void SetOuterLineColor(Color OuterLineColor)
    {
        if (this.OuterLineColor != OuterLineColor)
        {
            this.OuterLineColor = OuterLineColor;
            RedrawBox();
        }
    }

    public void SetInnerLineColor(Color InnerLineColor)
    {
        if (this.InnerLineColor != InnerLineColor)
        {
            this.InnerLineColor = InnerLineColor;
            RedrawBox();
        }
    }

    public void SetInnerColor(Color InnerColor)
    {
        if (this.InnerColor != InnerColor)
        {
            this.InnerColor = InnerColor;
            RedrawBox();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawBox();
    }

    private void RedrawBox()
    {
        if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(Size, OuterLineColor);
        Sprites["bg"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, InnerLineColor);
        Sprites["bg"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, InnerColor);
        Sprites["bg"].Bitmap.Lock();
    }
}