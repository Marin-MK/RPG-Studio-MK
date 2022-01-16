using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class ProgressBar : Widget
{
    public float Progress { get; protected set; }
    public List<Color> Colors { get; protected set; } = new List<Color>() { new Color(87, 204, 253), new Color(50, 63, 228), new Color(129, 96, 224) };

    public ProgressBar(IContainer Parent) : base(Parent)
    {
        MinimumSize.Height = MaximumSize.Height = 34;
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["progress"] = new Sprite(this.Viewport);
        Sprites["progress"].X = 1;
        Sprites["progress"].Y = 1;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].Y = 9;
        SetHeight(34);
        SetTimer("frame", 1000 / 125); // 125 frames a second
    }

    public void SetProgress(float Progress)
    {
        Progress = Math.Clamp(Progress, 0, 1);
        if (this.Progress != Progress)
        {
            this.Progress = Progress;
            this.Redraw();
        }
    }

    public void SetColors(List<Color> Colors)
    {
        this.Colors = Colors;
        RedrawFiller();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(Size);
        Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.DrawLine(1, 0, Size.Width - 2, 0, new Color(86, 108, 134));
        Sprites["box"].Bitmap.DrawLine(1, Size.Height - 1, Size.Width - 2, Size.Height - 1, new Color(86, 108, 134));
        Sprites["box"].Bitmap.DrawLine(0, 1, 0, Size.Height - 2, new Color(86, 108, 134));
        Sprites["box"].Bitmap.DrawLine(Size.Width - 1, 1, Size.Width - 1, Size.Height - 2, new Color(86, 108, 134));
        Sprites["box"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(10, 23, 37));
        Sprites["box"].Bitmap.Lock();
        RedrawFiller();
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["text"].Bitmap?.Dispose();
        string text = (Math.Round(this.Progress * 1000d) / 10d).ToString() + "%";
        Font f = Fonts.UbuntuRegular.Use(14);
        Size s = f.TextSize(text);
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Font = f;
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(text, Color.WHITE);
        Sprites["text"].X = Size.Width / 2 - s.Width / 2;
        Sprites["text"].Bitmap.Lock();

        Sprites["progress"].SrcRect.Width = (int) Math.Round(this.Progress * (Size.Width - 2));
    }

    public override void Update()
    {
        base.Update();
        if (TimerPassed("frame"))
        {
            ResetTimer("frame");
            Sprites["progress"].SrcRect.X -= 1;
            if (Sprites["progress"].SrcRect.X < 0)
                Sprites["progress"].SrcRect.X = Sprites["progress"].Bitmap.Width - (Size.Width - 2);
            Sprites["progress"].Update();
        }
    }

    void RedrawFiller()
    {
        int w = (Size.Width - 2) * 2;
        int part = w / 2 / (Colors.Count - 1);
        w += part;
        Sprites["progress"].Bitmap?.Dispose();
        Sprites["progress"].Bitmap = new Bitmap(w, Size.Height - 2);
        Sprites["progress"].SrcRect.X = w - (Size.Width - 2);
        Sprites["progress"].SrcRect.Width = (int) Math.Round(this.Progress * (Size.Width - 2));
        Sprites["progress"].Bitmap.Unlock();
        for (int i = 0; i < Colors.Count * 2 - 1; i++)
        {
            Color c1 = Colors[i % Colors.Count];
            Color c2 = Colors[(i + 1) % Colors.Count];
            Sprites["progress"].Bitmap.FillGradientRect(part * i, 0, part, Size.Height - 2, c1, c2, c1, c2);
        }
        Sprites["progress"].Bitmap.Lock();
    }
}
