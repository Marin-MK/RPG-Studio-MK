using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class NumericDataSlider : Widget
{
    public bool Slider { get; protected set; } = false;
    public double Value { get; protected set; } = 0;
    public double? MaxValue { get; protected set; } = null;
    public double? MinValue { get; protected set; } = 0;
    public bool AllowDecimals { get; protected set; } = false;

    bool DraggingSlider = false;

    TextArea TextArea;

    public NumericDataSlider(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["slider"] = new Sprite(this.Viewport);
        Sprites["slider"].X = 55;
        Sprites["slider"].Y = 7;
        MinimumSize = new Size(48, 25);
        MaximumSize = new Size(-1, 25);
        TextArea = new TextArea(this);
        TextArea.SetPosition(9, 4);
        TextArea.SetFont(Fonts.ProductSansMedium.Use(11));
        TextArea.SetTextX(10);
        TextArea.SetText(Value.ToString());
        TextArea.SetNumericOnly(true);
        TextArea.SetZIndex(1);
        TextArea.OnTextChanged += delegate (TextEventArgs e)
        {
            if (TextArea.Text.Length == 0) TextArea.SetTextX(12);
            else if (TextArea.Text.Length == 1) TextArea.SetTextX(9);
            else if (TextArea.Text.Length == 2) TextArea.SetTextX(6);
            else if (TextArea.Text.Length == 3) TextArea.SetTextX(3);
            else TextArea.SetTextX(0);
            if (TextArea.Text.Length == 0 || TextArea.Text == "-")
            {
                SetValue(0, false);
            }
            else SetValue(Convert.ToInt32(TextArea.Text));
        };
        TextArea.RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(TextArea, new Key(Keycode.DOWN), PressingDown),
                new Shortcut(TextArea, new Key(Keycode.UP), PressingUp)
            });
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (Slider) TextArea.SetSize(30, 17);
        else TextArea.SetSize(Size.Width - 18, 17);
    }

    public void SetSlider(bool Slider)
    {
        if (this.Slider != Slider)
        {
            this.Slider = Slider;
            RedrawSlider();
        }
    }

    public void SetMinValue(double? MinValue)
    {
        if (this.MinValue != MinValue)
        {
            this.MinValue = MinValue;
            if (MinValue != null && Value < MinValue) SetValue((double)MinValue);
            RedrawSlider();
        }
    }

    public void SetMaxValue(double? MaxValue)
    {
        if (this.MaxValue != MaxValue)
        {
            this.MaxValue = MaxValue;
            if (MaxValue != null && Value > MaxValue) SetValue((double)MaxValue);
            RedrawSlider();
        }
    }

    public void SetValue(double Value, bool ForceRedrawText = true)
    {
        if (this.Value != Value)
        {
            if (MinValue != null && Value < MinValue) Value = (double)MinValue;
            if (MaxValue != null && Value > MaxValue) Value = (double)MaxValue;
            this.Value = Value;
            if (ForceRedrawText) TextArea.SetText(Value.ToString());
            RedrawSlider();
        }
    }

    public void SetAllowDecimals(bool AllowDecimals)
    {
        if (this.AllowDecimals != AllowDecimals)
        {
            this.AllowDecimals = AllowDecimals;
        }
    }

    protected override void Draw()
    {
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(48, 25);
        Sprites["box"].Bitmap.Unlock();
        Color grey = new Color(126, 126, 126);
        Color dark = new Color(74, 83, 91);
        Color filler = new Color(10, 23, 37);
        Sprites["box"].Bitmap.FillRect(5, 2, 38, 21, filler);
        Sprites["box"].Bitmap.DrawLine(0, 10, 0, 14, grey);
        Sprites["box"].Bitmap.DrawLine(1, 8, 1, 16, grey);
        Sprites["box"].Bitmap.DrawLine(2, 6, 2, 18, grey);
        Sprites["box"].Bitmap.DrawLine(3, 4, 3, 20, grey);
        Sprites["box"].Bitmap.DrawLine(4, 2, 4, 22, grey);
        Sprites["box"].Bitmap.DrawLine(5, 1, 5, 7, grey);
        Sprites["box"].Bitmap.DrawLine(5, 17, 5, 23, grey);
        Sprites["box"].Bitmap.DrawLine(6, 0, 6, 3, grey);
        Sprites["box"].Bitmap.DrawLine(6, 21, 6, 24, grey);
        Sprites["box"].Bitmap.SetPixel(5, 0, dark);
        Sprites["box"].Bitmap.SetPixel(5, 24, dark);
        Sprites["box"].Bitmap.SetPixel(6, 4, dark);
        Sprites["box"].Bitmap.SetPixel(5, 8, dark);
        Sprites["box"].Bitmap.SetPixel(5, 16, dark);
        Sprites["box"].Bitmap.SetPixel(6, 20, dark);
        Sprites["box"].Bitmap.FillRect(9, 0, 30, 2, dark);
        Sprites["box"].Bitmap.FillRect(9, 23, 30, 2, dark);
        Sprites["box"].Bitmap.DrawLine(47, 10, 47, 14, grey);
        Sprites["box"].Bitmap.DrawLine(46, 8, 46, 16, grey);
        Sprites["box"].Bitmap.DrawLine(45, 6, 45, 18, grey);
        Sprites["box"].Bitmap.DrawLine(44, 4, 44, 20, grey);
        Sprites["box"].Bitmap.DrawLine(43, 2, 43, 22, grey);
        Sprites["box"].Bitmap.DrawLine(42, 1, 42, 7, grey);
        Sprites["box"].Bitmap.DrawLine(42, 17, 42, 23, grey);
        Sprites["box"].Bitmap.DrawLine(41, 0, 41, 3, grey);
        Sprites["box"].Bitmap.DrawLine(41, 21, 41, 24, grey);
        Sprites["box"].Bitmap.SetPixel(42, 0, dark);
        Sprites["box"].Bitmap.SetPixel(42, 24, dark);
        Sprites["box"].Bitmap.SetPixel(41, 4, dark);
        Sprites["box"].Bitmap.SetPixel(42, 8, dark);
        Sprites["box"].Bitmap.SetPixel(42, 16, dark);
        Sprites["box"].Bitmap.SetPixel(41, 20, dark);
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }

    public void RedrawSlider()
    {
        Sprites["slider"].Bitmap?.Dispose();
        if (Slider && Size.Width > 60)
        {
            if (MinValue == null || MaxValue == null) throw new Exception("MinValue and MaxValue must both be non-null if Slider is enabled.");
            Sprites["slider"].Bitmap = new Bitmap(Size.Width - 55, 10);
            Sprites["slider"].Bitmap.Unlock();
            int width = Size.Width - 57;
            Sprites["slider"].Bitmap.FillRect(0, 2, width, 6, new Color(124, 124, 124));
            double fraction = (Value - (double)MinValue) / ((double)MaxValue - (double)MinValue);
            int bluewidth = (int)Math.Floor(width * fraction);
            Sprites["slider"].Bitmap.FillRect(0, 2, bluewidth, 6, new Color(32, 170, 221));
            Color dark = new Color(10, 23, 37);
            Sprites["slider"].Bitmap.FillRect(2, 8, Size.Width - 57, 2, dark);
            Sprites["slider"].Bitmap.SetPixel(1, 8, dark);
            Sprites["slider"].Bitmap.FillRect(Size.Width - 57, 4, 2, 4, dark);
            Sprites["slider"].Bitmap.SetPixel(Size.Width - 57, 3, dark);
            Sprites["slider"].Bitmap.DrawLine(bluewidth, 2, bluewidth, 7, dark);
            Sprites["slider"].Bitmap.DrawLine(bluewidth + 1, 0, bluewidth + 1, 7, new Color(28, 50, 73));
            Sprites["slider"].Bitmap.Lock();
        }
    }

    public void PressingDown(BaseEventArgs e)
    {
        if (Input.Press(Keycode.SHIFT))
        {
            SetValue(Value - 10);
        }
        else
        {
            SetValue(Value - 1);
        }
    }

    public void PressingUp(BaseEventArgs e)
    {
        if (Input.Press(Keycode.SHIFT))
        {
            SetValue(Value + 10);
        }
        else
        {
            SetValue(Value + 1);
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!TextArea.Mouse.Inside && TextArea.SelectedWidget)
        {
            Window.UI.SetSelectedWidget(null);
            if (string.IsNullOrEmpty(TextArea.Text)) TextArea.SetText("0");
        }
    }

    public override void MousePress(MouseEventArgs e)
    {
        base.MousePress(e);
        if (e.LeftButton && !e.RightButton && !e.MiddleButton)
        {
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (Slider && Size.Width > 60 && rx >= 55 && rx < Size.Width - 1)
            {
                if (DraggingSlider || ry >= 6 && ry < 17)
                {
                    if (MinValue == null || MaxValue == null) throw new Exception("MinValue and MaxValue must both be non-null if Slider is enabled.");
                    double fraction = (double)(rx - 55) / (Size.Width - 57);
                    double Value = (double)MinValue + fraction * (double)(MaxValue - MinValue);
                    if (AllowDecimals) SetValue(Value);
                    else SetValue(Math.Round(Value));
                    DraggingSlider = true;
                }
            }
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (!e.LeftButton && e.OldLeftButton)
        {
            DraggingSlider = false;
        }
    }
}
