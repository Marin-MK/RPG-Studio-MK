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
        public bool Enabled { get; protected set; } = true;

        public BaseEvent OnValueChanged;

        public NumericBox(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);

            SetSize(66, 27);
        }

        public void SetEnabled(bool Enabled)
        {
            if (this.Enabled != Enabled)
            {
                this.Enabled = Enabled;
                this.Redraw();
            }
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
            Color light = this.Enabled ? new Color(86, 108, 134) : new Color(72, 72, 72);
            Color dark = new Color(10, 23, 37);

            Sprites["box"].Bitmap.FillRect(Size, light);
            Sprites["box"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - 15, Size.Height - 2, dark);
            Sprites["box"].Bitmap.SetPixel(1, 1, light);
            Sprites["box"].Bitmap.SetPixel(1, Size.Height - 2, light);
            Color UpColor = this.Enabled && SelectedUp ? new Color(55, 187, 255) : this.Enabled && HoveringUp ? new Color(28, 50, 73) : dark;
            Sprites["box"].Bitmap.FillRect(Size.Width - 13, 1, 12, 12, UpColor);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, 1, light);
            Sprites["box"].Bitmap.FillRect(Size.Width - 11, 6, 8, 2, light);
            Sprites["box"].Bitmap.FillRect(Size.Width - 8, 3, 2, 8, light);
            Color DownColor = this.Enabled && SelectedDown ? new Color(55, 187, 255) : this.Enabled && HoveringDown ? new Color(28, 50, 73) : dark;
            Sprites["box"].Bitmap.FillRect(Size.Width - 13, 14, 12, 12, DownColor);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, light);
            Sprites["box"].Bitmap.FillRect(Size.Width - 11, 19, 8, 2, light);
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
            if (this.Enabled) Sprites["text"].Bitmap.DrawText(text, this.Enabled ? this.TextColor : new Color(72, 72, 72));
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
                SelectedDown = false;
                SelectedUp = false;
                HoveringUp = false;
                HoveringDown = false;
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (e.LeftButton == e.OldLeftButton) return;
            if (HoveringUp && this.Enabled)
            {
                SetValue(Value + Increment);
                SelectedDown = false;
                SelectedUp = true;
                Redraw();
            }
            if (HoveringDown && this.Enabled)
            {
                SetValue(Value - Increment);
                SelectedUp = false;
                SelectedDown = true;
                Redraw();
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (e.LeftButton == e.OldLeftButton) return;
            if (TimerExists("cooldown")) DestroyTimer("cooldown");
            if (TimerExists("press")) DestroyTimer("press");
            if (SelectedUp || SelectedDown)
            {
                SelectedUp = false;
                SelectedDown = false;
                Redraw();
            }
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
                    if (HoveringUp && this.Enabled)
                    {
                        SetValue(Value + Increment);
                        SelectedDown = false;
                        SelectedUp = true;
                        Redraw();
                    }
                    else if (HoveringDown && this.Enabled)
                    {
                        SetValue(Value - Increment);
                        SelectedUp = false;
                        SelectedDown = true;
                        Redraw();
                    }
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

        public override object GetValue(string Identifier)
        {
            return this.Value;
        }

        public override void SetValue(string Identifier, object Value)
        {
            if (!string.IsNullOrEmpty(Value.ToString())) this.SetValue(Convert.ToInt32(Value));
        }
    }
}
