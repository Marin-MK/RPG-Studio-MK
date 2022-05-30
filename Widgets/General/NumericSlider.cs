using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class NumericSlider : Widget
{
    public int Value { get; protected set; } = 100;
    public int MinValue { get; protected set; } = 0;
    public int MaxValue { get; protected set; } = 100;
    public List<(int Value, double Factor, int X)> SnapValues { get; protected set; } = new List<(int, double, int)>();
    public int PixelSnapDifference { get; protected set; } = 4;
    public bool Enabled { get; protected set; } = true;

    public BaseEvent OnValueChanged;

    bool DraggingSlider = false;

    public NumericSlider(IContainer Parent) : base(Parent)
    {
        MinimumSize.Height = MaximumSize.Height = 17;
        Sprites["bars"] = new Sprite(this.Viewport);
        Sprites["bars"].X = 4;
        Sprites["slider"] = new Sprite(this.Viewport);
        Sprites["slider"].Bitmap = new Bitmap(8, 17);
        Sprites["slider"].Bitmap.Unlock();
        Sprites["slider"].Bitmap.FillRect(0, 0, 8, 17, new Color(55, 171, 206));
        Sprites["slider"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["slider"].Bitmap.SetPixel(7, 0, Color.ALPHA);
        Sprites["slider"].Bitmap.SetPixel(0, 16, Color.ALPHA);
        Sprites["slider"].Bitmap.SetPixel(7, 16, Color.ALPHA);
        Sprites["slider"].Bitmap.Lock();
        SetSize(107, 17);
    }

    public void SetValue(int Value)
    {
        if (this.Value != Value)
        {
            this.Value = Value;
            this.Redraw();
            this.OnValueChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void SetMinimumValue(int MinValue)
    {
        if (this.MinValue != MinValue)
        {
            this.MinValue = MinValue;
            RecalculateSnapFactors();
            this.Redraw();
        }
    }

    public void SetMaximumValue(int MaxValue)
    {
        if (this.MaxValue != MaxValue)
        {
            this.MaxValue = MaxValue;
            RecalculateSnapFactors();
            this.Redraw();
        }
    }

    public void SetSnapValues(params int[] Values)
    {
        this.SnapValues.Clear();
        foreach (int Value in Values)
        {
            double snapfactor = Math.Clamp((Value - MinValue) / (double)(MaxValue - MinValue), 0, 1);
            int x = (int)Math.Round(snapfactor * (Size.Width - 9));
            this.SnapValues.Add((Value, snapfactor, x));
        }
        this.Redraw();
    }

    public void SetPixelSnapDifference(int PixelSnapDifference)
    {
        this.PixelSnapDifference = PixelSnapDifference;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RecalculateSnapFactors();
    }

    void RecalculateSnapFactors()
    {
        for (int i = 0; i < SnapValues.Count; i++)
        {
            double snapfactor = Math.Clamp((SnapValues[i].Value - MinValue) / (double)(MaxValue - MinValue), 0, 1);
            int x = (int)Math.Round(snapfactor * (Size.Width - 9));
            SnapValues[i] = (SnapValues[i].Value, snapfactor, x);
        }
    }

    protected override void Draw()
    {
        base.Draw();
        int MaxX = Size.Width - 8;
        double factor = Math.Clamp((Value - MinValue) / (double)(MaxValue - MinValue), 0, 1);
        Sprites["slider"].X = (int) Math.Round(factor * MaxX);
        Sprites["slider"].Visible = Enabled;

        Color PreColor = new Color(55, 171, 206);
        Color PostColor = new Color(73, 89, 109);

        if (!Enabled) PreColor = PostColor;

        Sprites["bars"].Bitmap?.Dispose();
        Sprites["bars"].Bitmap = new Bitmap(Size.Width - 8, Size.Height);
        Sprites["bars"].Bitmap.Unlock();
        if (Enabled)
        {
            Sprites["bars"].Bitmap.FillRect(2, 7, Sprites["slider"].X - 8, 3, PreColor);
            if (Sprites["slider"].X < Size.Width - 14)
                Sprites["bars"].Bitmap.FillRect(Sprites["slider"].X + 6, 7, Size.Width - 14 - Sprites["slider"].X, 3, PostColor);
        }
        else
        {
            Sprites["bars"].Bitmap.FillRect(2, 7, Size.Width - 14, 3, PreColor);
        }
        foreach ((int Value, double Factor, int X) Snap in SnapValues)
        {
            Color c = Snap.Factor > factor ? PostColor : PreColor;
            Sprites["bars"].Bitmap.DrawLine(Snap.X, 1, Snap.X, Size.Height - 2, c);
            if (Snap.X > 0) Sprites["bars"].Bitmap.DrawLine(Snap.X - 1, 1, Snap.X - 1, Size.Height - 2, Color.ALPHA);
            if (Snap.X < Size.Width - 9) Sprites["bars"].Bitmap.DrawLine(Snap.X + 1, 1, Snap.X + 1, Size.Height - 2, Color.ALPHA);
        }
        Sprites["bars"].Bitmap.Lock();
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (!Enabled) return;
        DraggingSlider = true;
        MouseMoving(e);
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (DraggingSlider)
        {
            int rx = e.X - Viewport.X;
            if (rx < 4) return;
            double factor = 0;
            bool Snapping = false;
            if (!Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_LALT) && !Input.Press(odl.SDL2.SDL.SDL_Keycode.SDLK_RALT))
            {
                foreach ((int Value, double Factor, int X) Snap in SnapValues)
                {
                    if (Math.Abs((rx - 4) - Snap.X) <= this.PixelSnapDifference)
                    {
                        factor = Snap.Factor;
                        Snapping = true;
                    }
                }
            }
            if (!Snapping) factor = Math.Clamp((rx - 4) / (double)(Size.Width - 8), 0, 1);
            this.SetValue((int)Math.Round((MaxValue - MinValue) * factor + MinValue));
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (Mouse.LeftMouseReleased) DraggingSlider = false;
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            Redraw();
        }
    }
}
