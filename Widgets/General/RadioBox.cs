using System;

namespace RPGStudioMK.Widgets;

public class RadioBox : Widget
{
    public string Text { get; protected set; }
    public bool Checked { get; protected set; } = false;
    public Font Font { get; protected set; }
    public bool Enabled { get; protected set; } = true;

    bool Selecting = false;

    public BaseEvent OnCheckChanged;

    public RadioBox(IContainer Parent) : base(Parent)
    {
        this.Font = Fonts.CabinMedium.Use(9);
        Sprites["box"] = new Sprite(this.Viewport, new Bitmap(16, 16));
        RedrawBox(true);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = 20;
        Sprites["text"].Y = -1;
        SetText("");
        OnWidgetSelected += WidgetSelected;
        SetHeight(16);
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            if (!this.Enabled) this.SetChecked(false);
            this.Redraw();
            this.RedrawText();
        }
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            this.RedrawText();
        }
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            this.RedrawText();
        }
    }

    public void SetChecked(bool Checked)
    {
        if (this.Checked != Checked)
        {
            if (Checked)
            {
                foreach (Widget w in Parent.Widgets)
                {
                    if (w is RadioBox && w != this && ((RadioBox)w).Checked) ((RadioBox)w).SetChecked(false);
                }
            }
            this.Checked = Checked;
            Redraw();
            OnCheckChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void RedrawText()
    {
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        Size s = this.Font.TextSize(this.Text);
        if (20 + s.Width >= Size.Width) SetWidth(20 + s.Width);
        Sprites["text"].Bitmap = new Bitmap(Math.Max(1, s.Width), Math.Max(1, s.Height));
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(this.Text, this.Enabled ? Color.WHITE : new Color(72, 72, 72));
        Sprites["text"].Bitmap.Lock();
    }

    public void RedrawBox(bool Lock)
    {
        if (Lock) Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.Clear();
        Color lightoutline = Selecting || Mouse.Inside ? new Color(64, 104, 146) : new Color(86, 108, 134);
        Color filler = Selecting ? new Color(23, 36, 50) : lightoutline;
        Color outline = Selecting || Mouse.Inside ? new Color(23, 36, 50) : new Color(36, 34, 36);

        if (!this.Enabled)
        {
            lightoutline = new Color(72, 72, 72);
            filler = new Color(72, 72, 72);
        }

        // Outer outline
        Sprites["box"].Bitmap.DrawLine(1, 4, 4, 1, lightoutline);
        Sprites["box"].Bitmap.SetPixel(5, 1, lightoutline);
        Sprites["box"].Bitmap.DrawLine(6, 0, 9, 0, lightoutline);
        Sprites["box"].Bitmap.SetPixel(10, 1, lightoutline);
        Sprites["box"].Bitmap.DrawLine(11, 1, 14, 4, lightoutline);
        Sprites["box"].Bitmap.SetPixel(14, 5, lightoutline);
        Sprites["box"].Bitmap.DrawLine(15, 6, 15, 9, lightoutline);
        Sprites["box"].Bitmap.SetPixel(14, 10, lightoutline);
        Sprites["box"].Bitmap.DrawLine(14, 11, 11, 14, lightoutline);
        Sprites["box"].Bitmap.SetPixel(10, 14, lightoutline);
        Sprites["box"].Bitmap.DrawLine(9, 15, 6, 15, lightoutline);
        Sprites["box"].Bitmap.SetPixel(5, 14, lightoutline);
        Sprites["box"].Bitmap.DrawLine(4, 14, 1, 11, lightoutline);
        Sprites["box"].Bitmap.SetPixel(1, 10, lightoutline);
        Sprites["box"].Bitmap.DrawLine(0, 9, 0, 6, lightoutline);
        Sprites["box"].Bitmap.SetPixel(1, 5, lightoutline);
        // Filler
        Sprites["box"].Bitmap.FillRect(3, 3, 10, 10, filler);
        Sprites["box"].Bitmap.DrawLine(6, 2, 9, 2, filler);
        Sprites["box"].Bitmap.DrawLine(13, 6, 13, 9, filler);
        Sprites["box"].Bitmap.DrawLine(9, 13, 6, 13, filler);
        Sprites["box"].Bitmap.DrawLine(2, 9, 2, 6, filler);
        // Inner outline
        Sprites["box"].Bitmap.DrawLine(2, 4, 4, 2, outline);
        Sprites["box"].Bitmap.SetPixel(5, 2, outline);
        Sprites["box"].Bitmap.DrawLine(6, 1, 9, 1, outline);
        Sprites["box"].Bitmap.SetPixel(10, 2, outline);
        Sprites["box"].Bitmap.DrawLine(11, 2, 13, 4, outline);
        Sprites["box"].Bitmap.SetPixel(13, 5, outline);
        Sprites["box"].Bitmap.DrawLine(14, 6, 14, 9, outline);
        Sprites["box"].Bitmap.SetPixel(13, 10, outline);
        Sprites["box"].Bitmap.DrawLine(13, 11, 11, 13, outline);
        Sprites["box"].Bitmap.SetPixel(10, 13, outline);
        Sprites["box"].Bitmap.DrawLine(9, 14, 6, 14, outline);
        Sprites["box"].Bitmap.SetPixel(5, 13, outline);
        Sprites["box"].Bitmap.DrawLine(4, 13, 2, 11, outline);
        Sprites["box"].Bitmap.SetPixel(2, 10, outline);
        Sprites["box"].Bitmap.DrawLine(1, 9, 1, 6, outline);
        Sprites["box"].Bitmap.SetPixel(2, 5, outline);
        if (Lock) Sprites["box"].Bitmap.Lock();
    }

    protected override void Draw()
    {
        Sprites["box"].Bitmap.Unlock();
        RedrawBox(false);
        if (this.Checked && this.Enabled)
        {
            Color checkcolor = this.Enabled ? Color.WHITE : new Color(120, 120, 120);
            Sprites["box"].Bitmap.FillRect(6, 6, 4, 4, checkcolor);
            Sprites["box"].Bitmap.DrawLine(6, 5, 9, 5, checkcolor);
            Sprites["box"].Bitmap.DrawLine(5, 6, 5, 9, checkcolor);
            Sprites["box"].Bitmap.DrawLine(6, 10, 9, 10, checkcolor);
            Sprites["box"].Bitmap.DrawLine(10, 6, 10, 9, checkcolor);
        }
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (Mouse.Inside && this.Enabled)
        {
            Selecting = true;
            if (!this.Checked) SetChecked(true);
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (Selecting)
        {
            Selecting = false;
            Redraw();
        }
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Selecting = false;
        this.Redraw();
    }
}
