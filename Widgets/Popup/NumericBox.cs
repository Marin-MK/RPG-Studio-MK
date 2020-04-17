using System;
using ODL;

namespace MKEditor.Widgets
{
    public class NumericBox : Widget
    {
        public bool HoveringUp = false;
        public bool SelectedUp = false;
        public bool HoveringDown = false;
        public bool SelectedDown = false;

        public int Value { get; protected set; } = 0;
        public int MaxValue = 999999;
        public int MinValue = -999999;
        public int Increment = 1;
        public Color TextColor { get; protected set; } = Color.WHITE;

        public BaseEvent OnValueChanged;

        public NumericBox(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);

            SetSize(66, 27);
        }

        public void SetValue(int Value)
        {
            if (Value > MaxValue) Value = MaxValue;
            if (Value < MinValue) Value = MinValue;
            if (this.Value != Value)
            {
                this.Value = Value;
                this.OnValueChanged?.Invoke(new BaseEventArgs());
                Redraw();
            }
        }

        public void SetTextColor(Color TextColor)
        {
            if (this.TextColor != TextColor)
            {
                this.TextColor = TextColor;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["box"].Bitmap != null) Sprites["box"].Bitmap.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Color light = new Color(86, 108, 134);
            Color dark = new Color(36, 34, 36);
            Sprites["box"].Bitmap.SetPixel(1, 1, light);
            Sprites["box"].Bitmap.DrawLine(2, 0, Size.Width - 19, 0, light);
            Sprites["box"].Bitmap.DrawLine(2, 1, Size.Width - 19, 1, dark);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 18, 1, light);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 17, 2, Size.Width - 17, Size.Height - 3, light);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 18, 2, Size.Width - 18, Size.Height - 3, dark);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 18, Size.Height - 2, light);
            Sprites["box"].Bitmap.DrawLine(2, Size.Height - 1, Size.Width - 19, Size.Height - 1, light);
            Sprites["box"].Bitmap.DrawLine(2, Size.Height - 2, Size.Width - 19, Size.Height - 2, dark);
            Sprites["box"].Bitmap.SetPixel(1, Size.Height - 2, light);
            Sprites["box"].Bitmap.DrawLine(0, 2, 0, Size.Height - 3, light);
            Sprites["box"].Bitmap.DrawLine(1, 2, 1, Size.Height - 3, dark);
            Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 20, Size.Height - 4, light);
            for (int i = 0; i < 2; i++)
            {
                int x = Size.Width - 15;
                int y = i == 0 ? 0 : 14;
                Color outer = new Color(16, 25, 36);
                Color mid = new Color(55, 73, 93);
                bool sel = HoveringUp && i == 0 || HoveringDown && i == 1;
                Color fill = sel ? new Color(59, 227, 255) : new Color(86, 108, 134);
                Sprites["box"].Bitmap.FillRect(x + 1, y + 1, 13, 11, fill);
                Sprites["box"].Bitmap.SetPixel(x + 1, y + 1, outer);
                Sprites["box"].Bitmap.DrawLine(x + 2, y, x + 12, y, outer);
                Sprites["box"].Bitmap.SetPixel(x + 13, y + 1, outer);
                Sprites["box"].Bitmap.DrawLine(x + 14, y + 2, x + 14, y + 10, outer);
                Sprites["box"].Bitmap.SetPixel(x + 13, y + 11, outer);
                Sprites["box"].Bitmap.DrawLine(x, y + 2, x, y + 10, outer);
                Sprites["box"].Bitmap.SetPixel(x + 1, y + 11, outer);
                Sprites["box"].Bitmap.DrawLine(x + 2, y + 12, x + 12, y + 12, outer);
                Sprites["box"].Bitmap.SetPixel(x + 1, y + 2, mid);
                Sprites["box"].Bitmap.SetPixel(x + 2, y + 1, mid);
                Sprites["box"].Bitmap.SetPixel(x + 12, y + 1, mid);
                Sprites["box"].Bitmap.SetPixel(x + 13, y + 2, mid);
                Sprites["box"].Bitmap.SetPixel(x + 1, y + 10, mid);
                Sprites["box"].Bitmap.SetPixel(x + 2, y + 11, mid);
                Sprites["box"].Bitmap.SetPixel(x + 12, y + 11, mid);
                Sprites["box"].Bitmap.SetPixel(x + 13, y + 10, mid);
                Sprites["box"].Bitmap.DrawLine(x + 5, y + 6, x + 9, y + 6, sel ? Color.BLACK : Color.WHITE);
                if (i == 0) Sprites["box"].Bitmap.DrawLine(x + 7, y + 4, x + 7, y + 8, sel ? Color.BLACK : Color.WHITE);
            }
            Sprites["box"].Bitmap.Lock();
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            string text = "";
            if (Value < 0) text += "-";
            text += Math.Abs(Value).ToString();
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            Size s = f.TextSize(text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.DrawText(text, this.TextColor);
            Sprites["text"].X = Size.Width - 21 - s.Width;
            Sprites["text"].Y = 5;
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            bool oldup = HoveringUp;
            bool olddown = HoveringDown;
            if (!WidgetIM.Hovering)
            {
                HoveringUp = false;
                HoveringDown = false;
                if (oldup != HoveringUp || olddown != HoveringDown) Redraw();
                return;
            }
            int rx = e.X - Viewport.X + Position.X - ScrolledPosition.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            if (rx < Size.Width - 16)
            {
                HoveringUp = false;
                HoveringDown = false;
                if (oldup != HoveringUp || olddown != HoveringDown) Redraw();
                return;
            }

            HoveringUp = ry < 14;
            HoveringDown = ry >= 14;

            if (oldup != HoveringUp || olddown != HoveringDown) Redraw();
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            if (!WidgetIM.Hovering)
            {
                if (HoveringUp || HoveringDown) Redraw();
                HoveringUp = false;
                HoveringDown = false;
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (HoveringUp) SetValue(Value + Increment);
            if (HoveringDown) SetValue(Value - Increment);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (TimerExists("cooldown")) DestroyTimer("cooldown");
            if (TimerExists("press")) DestroyTimer("press");
        }

        public override void MousePress(MouseEventArgs e)
        {
            base.MousePress(e);
            if (e.LeftButton && (HoveringUp || HoveringDown))
            {
                if (!TimerExists("press") && !TimerExists("cooldown"))
                {
                    SetTimer("cooldown", 400);
                }
                else if (TimerPassed("cooldown"))
                {
                    SetTimer("press", 50);
                    DestroyTimer("cooldown");
                }
                else if (TimerPassed("press"))
                {
                    if (HoveringUp) SetValue(Value + Increment);
                    else if (HoveringDown) SetValue(Value - Increment);
                    ResetTimer("press");
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (Value < MinValue) SetValue(MinValue);
            else if (Value > MaxValue) SetValue(MaxValue);
        }
    }
}
