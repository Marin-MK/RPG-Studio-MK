using System;
using System.Collections.Generic;
using System.Text;
using amethyst;
using odl;

namespace RPGStudioMK.Widgets
{
    public class NumericSlider : Widget
    {
        public int Value { get; protected set; } = 100;
        public int MinValue { get; protected set; } = 0;
        public int MaxValue { get; protected set; } = 100;

        public BaseEvent OnValueChanged;

        bool DraggingSlider = false;

        public NumericSlider(IContainer Parent) : base(Parent)
        {
            MinimumSize.Height = MaximumSize.Height = 17;
            Sprites["bars"] = new Sprite(this.Viewport);
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
                this.Redraw();
            }
        }

        public void SetMaximumValue(int MaxValue)
        {
            if (this.MaxValue != MaxValue)
            {
                this.MaxValue = MaxValue;
                this.Redraw();
            }
        }

        protected override void Draw()
        {
            base.Draw();
            int MaxX = Size.Width - 8;
            double factor = Math.Clamp((Value - MinValue) / (double) (MaxValue - MinValue), 0, 1);
            Sprites["slider"].X = (int) Math.Round(factor * MaxX);

            Sprites["bars"].Bitmap?.Dispose();
            Sprites["bars"].Bitmap = new Bitmap(this.Size);
            Sprites["bars"].Bitmap.Unlock();
            Sprites["bars"].Bitmap.DrawLine(0, 1, 0, Size.Height - 2, new Color(55, 171, 206));
            Sprites["bars"].Bitmap.DrawLine(Size.Width - 1, 1, Size.Width - 1, Size.Height - 2, new Color(73, 89, 109));
            Sprites["bars"].Bitmap.FillRect(2, 7, Sprites["slider"].X - 4, 3, new Color(55, 171, 206));
            if (Sprites["slider"].X < Size.Width - 10)
                Sprites["bars"].Bitmap.FillRect(Sprites["slider"].X + 10, 7, Size.Width - Sprites["slider"].X - 12, 3, new Color(73, 89, 109));
            Sprites["bars"].Bitmap.Lock();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (e.LeftButton != e.OldLeftButton && e.LeftButton && WidgetIM.Hovering)
            {
                DraggingSlider = true;
                MouseMoving(e);
            }
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            if (DraggingSlider)
            {
                int rx = e.X - Viewport.X;
                double factor = Math.Clamp(rx / (double) Size.Width, 0, 1);
                this.SetValue((int) Math.Round((MaxValue - MinValue) * factor + MinValue));
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (e.LeftButton != e.OldLeftButton && !e.LeftButton) DraggingSlider = false;
        }
    }
}
