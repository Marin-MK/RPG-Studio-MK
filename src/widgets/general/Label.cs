﻿using System.Collections.Generic;


namespace RPGStudioMK.Widgets;

public class Label : Widget
{
    public string Text { get; protected set; }
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public DrawOptions DrawOptions { get; protected set; }
    public bool Enabled { get; protected set; } = true;
    public int WidthLimit { get; protected set; } = -1;
    public string LimitReplacementText { get; protected set; } = "...";

    public bool ReachedWidthLimit { get; protected set; } = false;

    protected bool DrawnText = false;

    public Label(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(this.Viewport);
        this.Font = Fonts.Paragraph;
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            RedrawText();
        }
    }

    public virtual void SetText(string Text, DrawOptions DrawOptions = odl.DrawOptions.LeftAlign)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            this.DrawOptions = DrawOptions;
            RedrawText();
        }
    }

    public virtual void SetWidthLimit(int WidthLimit)
    {
        if (this.WidthLimit != WidthLimit)
        {
            this.WidthLimit = WidthLimit;
            RedrawText();
        }
    }

    public virtual void SetLimitReplacementText(string LimitReplacementText)
    {
        if (this.LimitReplacementText != LimitReplacementText)
        {
            this.LimitReplacementText = LimitReplacementText;
            RedrawText();
        }
    }

    public void SetDrawOptions(DrawOptions DrawOptions = odl.DrawOptions.LeftAlign)
    {
        if (this.DrawOptions != DrawOptions)
        {
            this.DrawOptions = DrawOptions;
            RedrawText();
        }
    }

    public void SetFont(Font f)
    {
        if (this.Font != f)
        {
            this.Font = f;
            RedrawText();
        }
    }

    public void SetTextColor(Color c)
    {
        if (this.TextColor != c)
        {
            this.TextColor = c;
            RedrawText();
        }
    }

    public virtual void RedrawText(bool Now = false)
    {
        if (!Now)
        {
            DrawnText = false;
            return;
        }
        Sprites["text"].Bitmap?.Dispose();
        this.ReachedWidthLimit = false;
        if (string.IsNullOrEmpty(this.Text)) return;
        Size s = this.Font.TextSize(this.Text, this.DrawOptions);
        string text = this.Text;
        if (WidthLimit != -1 && s.Width >= WidthLimit)
        {
            // Cut the string off at WidthLimit - len(replacement)
            int maxw = WidthLimit;
            if (!string.IsNullOrEmpty(this.LimitReplacementText))
            {
                Size repsize = this.Font.TextSize(this.LimitReplacementText, this.DrawOptions);
                maxw -= repsize.Width;
            }
            for (int i = 1; i < this.Text.Length; i++)
            {
                Size cursize = this.Font.TextSize(this.Text.Substring(0, i));
                if (cursize.Width > maxw)
                {
                    text = this.Text.Substring(0, i);
                    break;
                }
            }
            text += LimitReplacementText;
            this.ReachedWidthLimit = true;
        }
        this.SetSize(s);
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.DrawText(text, this.Enabled ? this.TextColor : new Color(160, 160, 160), this.DrawOptions);
        Sprites["text"].Bitmap.Lock();
        DrawnText = true;
    }

    public override void Update()
    {
        base.Update();
        if (!DrawnText) RedrawText(true);
    }
}

public class MultilineLabel : Label
{
    private int? _LineHeight;
    public int LineHeight { get => _LineHeight ?? Font.Size + 5; set => _LineHeight = value; }
    public new bool WidthLimit = true;

    public MultilineLabel(IContainer Parent) : base(Parent)
    {

    }

    public void SetLineHeight(int LineHeight)
    {
        if (this.LineHeight != LineHeight)
        {
            this.LineHeight = LineHeight;
            this.Redraw();
        }
    }

    public override void RedrawText(bool Now = false)
    {
        if (!Now)
        {
            DrawnText = false;
            return;
        }
        Sprites["text"].Bitmap?.Dispose();
        if (string.IsNullOrEmpty(Text)) return;
        List<string> Lines = Utilities.FormatString(this.Font, Text, WidthLimit ? Size.Width : -1);
        SetSize(Size.Width, LineHeight * Lines.Count + 4);
        Sprites["text"].Bitmap = new Bitmap(Size);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        for (int i = 0; i < Lines.Count; i++)
        {
            Sprites["text"].Bitmap.DrawText(Lines[i], 0, LineHeight * i, this.Enabled ? this.TextColor : new Color(160, 160, 160));
        }
        Sprites["text"].Bitmap.Lock();
        DrawnText = true;
    }
}
