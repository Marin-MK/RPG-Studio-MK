using System;

namespace RPGStudioMK.Widgets;

public class DataContainer : Widget
{
    public BaseEvent OnCollapsedChanged;

    public bool Collapsed { get; protected set; } = false;
    public string Text { get; protected set; }

    public DataContainer(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(1, 36, new Color(28, 50, 73)));
        Sprites["dropdown"] = new Sprite(this.Viewport);
        Sprites["dropdown"].X = 15;
        Sprites["dropdown"].Y = 15;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = 34;
        Sprites["text"].Y = 12;
        // Force recollapse
        this.Collapsed = true;
        SetCollapsed(false);
        SetHeight(36);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ((SolidBitmap)Sprites["bg"].Bitmap).SetSize(Size.Width, 36);
    }

    public void SetCollapsed(bool Collapsed)
    {
        if (this.Collapsed != Collapsed)
        {
            this.Collapsed = Collapsed;
            Sprites["dropdown"].Bitmap?.Dispose();
            Sprites["dropdown"].Bitmap = new Bitmap(9, 9);
            Sprites["dropdown"].Bitmap.Unlock();
            Color grey = new Color(127, 137, 147);
            if (Collapsed)
            {
                Sprites["dropdown"].Bitmap.FillRect(0, 1, 4, 7, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(0, 0, grey);
                Sprites["dropdown"].Bitmap.SetPixel(0, 8, grey);
                Sprites["dropdown"].Bitmap.SetPixel(2, 0, grey);
                Sprites["dropdown"].Bitmap.SetPixel(2, 8, grey);
                Sprites["dropdown"].Bitmap.SetPixel(1, 0, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(1, 8, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(4, 1, grey);
                Sprites["dropdown"].Bitmap.SetPixel(4, 7, grey);
                Sprites["dropdown"].Bitmap.FillRect(4, 2, 2, 5, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(6, 2, grey);
                Sprites["dropdown"].Bitmap.SetPixel(6, 6, grey);
                Sprites["dropdown"].Bitmap.FillRect(6, 3, 2, 3, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(8, 4, Color.WHITE);
            }
            else
            {
                Sprites["dropdown"].Bitmap.FillRect(1, 0, 7, 4, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(0, 0, grey);
                Sprites["dropdown"].Bitmap.SetPixel(8, 0, grey);
                Sprites["dropdown"].Bitmap.SetPixel(0, 2, grey);
                Sprites["dropdown"].Bitmap.SetPixel(8, 2, grey);
                Sprites["dropdown"].Bitmap.SetPixel(0, 1, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(8, 1, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(1, 4, grey);
                Sprites["dropdown"].Bitmap.SetPixel(7, 4, grey);
                Sprites["dropdown"].Bitmap.FillRect(2, 4, 5, 2, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(2, 6, grey);
                Sprites["dropdown"].Bitmap.SetPixel(6, 6, grey);
                Sprites["dropdown"].Bitmap.FillRect(3, 6, 3, 2, Color.WHITE);
                Sprites["dropdown"].Bitmap.SetPixel(4, 8, Color.WHITE);
            }
            Sprites["dropdown"].Bitmap.Lock();
            this.Widgets.ForEach(w =>
            {
                w.SetVisible(!this.Collapsed);
            });
            if (this.Collapsed)
            {
                this.SetSize(this.Size.Width, 36);
            }
            else
            {
                int maxheight = 0;
                foreach (Widget w in this.Widgets)
                {
                    int h = w.Position.Y + w.Size.Height;
                    if (h > maxheight) maxheight = h;
                }
                this.SetSize(this.Size.Width, Math.Max(36, maxheight + 12));
            }
            this.OnCollapsedChanged?.Invoke(new BaseEventArgs());
            ((VStackPanel)Parent).UpdateLayout();
        }
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            Sprites["text"].Bitmap?.Dispose();
            Font f = Fonts.Header;
            Size s = f.TextSize(this.Text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.DrawText(this.Text, Color.WHITE);
            Sprites["text"].Bitmap.Lock();
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        int ry = e.Y - Viewport.Y;
        if (!Mouse.Inside || ry >= 36) return;
        SetCollapsed(!Collapsed);
    }
}
