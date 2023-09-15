using System;
using System.Collections.Generic;
using System.Linq;


namespace RPGStudioMK.Widgets;

public class Button : Widget
{
    public string Text { get; protected set; }
    public Font Font { get; protected set; }
    public bool Enabled { get; protected set; } = true;
    public Color TextColor { get; protected set; } = Color.WHITE;
    public bool LeftAlign { get; protected set; } = false;
    public int TextX { get; protected set; } = 0;
    public bool Repeatable { get; protected set; } = false;

    public BaseEvent OnClicked;

    int MaxWidth;

    bool isInside = false;
    bool startedInside = false;
    bool DrawnText = false;

    public Button(IContainer Parent) : base(Parent)
    {
        this.Font = Fonts.ParagraphBold;

        Bitmap corner = Utilities.ButtonCornerFade;
        Bitmap hor = Utilities.ButtonHorizontalFade;
        Bitmap vert = Utilities.ButtonVerticalFade;

        Sprites["topleft"] = new Sprite(this.Viewport, corner);
        Sprites["topleft"].DestroyBitmap = false;

        Sprites["bottomleft"] = new Sprite(this.Viewport, corner);
        Sprites["bottomleft"].MirrorY = true;
        Sprites["bottomleft"].DestroyBitmap = false;

        Sprites["topright"] = new Sprite(this.Viewport, corner);
        Sprites["topright"].MirrorX = true;
        Sprites["topright"].DestroyBitmap = false;

        Sprites["bottomright"] = new Sprite(this.Viewport, corner);
        Sprites["bottomright"].MirrorX = Sprites["bottomright"].MirrorY = true;
        Sprites["bottomright"].DestroyBitmap = false;

        Sprites["left"] = new Sprite(this.Viewport, hor);
        Sprites["left"].Y = corner.Width;
        Sprites["left"].DestroyBitmap = false;
        Sprites["right"] = new Sprite(this.Viewport, hor);
        Sprites["right"].Y = Sprites["left"].Y;
        Sprites["right"].MirrorX = true;
        Sprites["right"].DestroyBitmap = false;

        Sprites["top"] = new Sprite(this.Viewport, vert);
        Sprites["top"].X = corner.Width;
        Sprites["top"].DestroyBitmap = false;
        Sprites["bottom"] = new Sprite(this.Viewport, vert);
        Sprites["bottom"].X = Sprites["top"].X;
        Sprites["bottom"].MirrorY = true;
        Sprites["bottom"].DestroyBitmap = false;

        Sprites["filler"] = new Sprite(this.Viewport);
        Sprites["filler"].X = Sprites["filler"].Y = corner.Width;

        Sprites["text"] = new Sprite(this.Viewport);

        OnWidgetSelected += WidgetSelected;

        SetSize(85, 33);
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            RedrawText();
        }
    }

    public void SetFont(Font Font)
    {
        if (!this.Font.Equals(Font))
        {
            this.Font = Font;
            RedrawText();
        }
    }

    public void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            RedrawText();
        }
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            RedrawFiller();
            RedrawText();
        }
    }

    public void SetLeftAlign(bool LeftAlign)
    {
        if (this.LeftAlign != LeftAlign)
        {
            this.LeftAlign = LeftAlign;
            if (!LeftAlign)
            {
                Sprites["text"].X = Size.Width / 2 - MaxWidth / 2;
            }
            else
            {
                Sprites["text"].X = 10 + TextX;
            }
        }
    }

    public void SetTextX(int TextX)
    {
        if (this.TextX != TextX)
        {
            this.TextX = TextX;
            if (LeftAlign) Sprites["text"].X = 10 + TextX;
        }
    }

    public void SetRepeatable(bool Repeatable)
    {
        if (this.Repeatable != Repeatable)
        {
            this.Repeatable = Repeatable;
        }
    }

    public void RedrawText(bool Now = false)
    {
        if (!Now)
        {
            DrawnText = false;
            return;
        }
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        if (string.IsNullOrEmpty(this.Text)) return;
        List<string> Lines = this.Text.Split('\n').ToList();
        MaxWidth = 0;
        Lines.ForEach(l => MaxWidth = Math.Max(MaxWidth, this.Font.TextSize(l).Width));
        Sprites["text"].Bitmap = new Bitmap(MaxWidth, Size.Height);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        Color c = this.Enabled ? this.TextColor : new Color(147, 158, 169);
        for (int i = 0; i < Lines.Count; i++)
        {
            Sprites["text"].Bitmap.DrawText(Lines[i], MaxWidth / 2, i * 18, c, DrawOptions.CenterAlign);
        }
        Sprites["text"].Bitmap.Lock();
        if (!LeftAlign)
        {
            Sprites["text"].X = Size.Width / 2 - MaxWidth / 2;
        }
        else
        {
            Sprites["text"].X = 10 + TextX;
        }
        Sprites["text"].Y = Size.Height / 2 - 9 * Lines.Count - Font.Size / 2 + 4;
        DrawnText = true;
    }

    public void RedrawFiller()
    {
        if (Sprites["filler"].Bitmap != null) Sprites["filler"].Bitmap.Dispose();
        int w = Size.Width - Sprites["filler"].X * 2;
        int h = Size.Height - Sprites["filler"].Y * 2;
        Sprites["filler"].Bitmap = new Bitmap(w, h);
        Sprites["filler"].Bitmap.Unlock();
        if (this.Enabled)
        {
            if (Mouse.LeftMousePressed && startedInside && isInside)
                Sprites["filler"].Bitmap.FillRect(0, 0, w, h, new Color(32, 170, 221));
            else if (isInside || Mouse.LeftMousePressed && startedInside)
                Sprites["filler"].Bitmap.FillGradientRect(0, 0, w, h, new Color(51, 86, 121), new Color(51, 86, 121), new Color(32, 170, 221), new Color(32, 170, 221));
            else Sprites["filler"].Bitmap.FillRect(0, 0, w, h, new Color(51, 86, 121));
        }
        else Sprites["filler"].Bitmap.FillRect(0, 0, w, h, new Color(51, 86, 121));
        Sprites["filler"].Bitmap.Lock();
    }

    public override void Update()
    {
        base.Update();
        if (!DrawnText) RedrawText(true);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawFiller();

        int o = Sprites["filler"].X;

        Sprites["bottomleft"].Y = Size.Height - o;
        Sprites["topright"].X = Size.Width - o;
        Sprites["bottomright"].X = Sprites["topright"].X;
        Sprites["bottomright"].Y = Sprites["bottomleft"].Y;

        Sprites["right"].X = Sprites["topright"].X;
        Sprites["left"].ZoomY = Size.Height - 2 * o;
        Sprites["right"].ZoomY = Sprites["left"].ZoomY;

        Sprites["top"].ZoomX = Size.Width - 2 * o;
        Sprites["bottom"].Y = Sprites["bottomleft"].Y;
        Sprites["bottom"].ZoomX = Sprites["top"].ZoomX;

        if (!string.IsNullOrEmpty(this.Text))
        {
            if (!LeftAlign)
            {
                Sprites["text"].X = Size.Width / 2 - MaxWidth / 2;
            }
            else
            {
                Sprites["text"].X = 10 + TextX;
            }
            Sprites["text"].Y = Size.Height / 2 - 9 * this.Text.Split('\n').Length - Font.Size / 2 + 4;
        }
    }

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
        int rx = e.X - Viewport.X + LeftCutOff;
        int ry = e.Y - Viewport.Y + TopCutOff;
        bool newInside;
        if (rx < 5 || rx >= Size.Width - 5 || ry < 5 || ry >= Size.Height - 5)
        {
            newInside = false;
            if (!e.CursorHandled && newInside != isInside) Input.SetCursor(CursorType.Arrow);
        }
        else
        {
            newInside = true;
        }
        if (newInside != isInside)
        {
            isInside = newInside;
            RedrawFiller();
        }
        else isInside = newInside;
	}

	public override void LeftMousePress(MouseEventArgs e)
    {
        base.LeftMousePress(e);
        if (isInside && Repeatable && this.Enabled)
        {
            startedInside = true;
            if (TimerPassed("press"))
            {
                OnClicked?.Invoke(new BaseEventArgs());
                ResetTimer("press");
            }
            if (TimerPassed("initial"))
            {
                OnClicked?.Invoke(new BaseEventArgs());
                SetTimer("press", 50);
                DestroyTimer("initial");
            }
        }
    }

    public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDown(e);
        if (!isInside) return;
        startedInside = true;
        if (!this.Enabled) return;
        if (Repeatable)
        {
            OnClicked?.Invoke(new BaseEventArgs());
            SetTimer("initial", 300);
        }
        RedrawFiller();
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (TimerExists("press")) DestroyTimer("press");
        if (TimerExists("initial")) DestroyTimer("initial");
        if (startedInside && !Repeatable && this.Enabled)
        {
            if (isInside)
            {
                this.OnClicked?.Invoke(new BaseEventArgs());
                if (!Disposed && !this.Enabled) RedrawText();
            }
        }
        startedInside = false;
        if (!Disposed && Graphics.CanUpdate()) RedrawFiller();
    }
}
