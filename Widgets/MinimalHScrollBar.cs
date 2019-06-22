using System;
using System.Linq;
using ODL;

namespace MKEditor.Widgets
{
    public class MinimalHScrollBar : Widget
    {
        public double SliderSize { get; protected set; }
        public double Value { get; protected set; }
        public bool Hovering { get { return SliderIM.Hovering; } }
        public bool Dragging { get { return SliderIM.ClickedLeftInArea == true; } }
        public Rect MouseInputRect { get; set; }

        public int MinSliderSize = 8;

        public EventHandler<EventArgs> OnValueChanged;

        private Rect SliderRect;
        private int SliderRX = 0;
        private MouseInputManager SliderIM;

        public MinimalHScrollBar(object Parent, string Name = "vScrollBar")
            : base(Parent, Name)
        {
            this.Size = new Size(17, 60);
            this.WidgetIM.OnMouseWheel += MouseWheel;
            this.Sprites["bar"] = new Sprite(this.Viewport);
            this.Sprites["bar"].Y = 5;
            this.Sprites["bar"].Bitmap = new SolidBitmap(1, 1, new Color(186, 186, 186));
            this.Sprites["slider"] = new Sprite(this.Viewport);
            this.Sprites["slider"].Y = 2;
            this.Sprites["slider"].Bitmap = new SolidBitmap(1, 1, new Color(186, 186, 186));
            this.SliderSize = 0.25;
            this.Value = 0;
            this.SliderIM = new MouseInputManager(this);
            this.SliderIM.OnMouseMoving += SliderMouseMoving;
            this.SliderIM.OnMouseDown += SliderMouseRight;
            this.SliderIM.OnMouseUp += SliderMouseLeft;
            this.SliderIM.OnHoverChanged += SliderHoverChanged;
        }

        public void SetValue(double value)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;
            if (this.Value != value)
            {
                this.Value = value;
                if (this.OnValueChanged != null) this.OnValueChanged.Invoke(this, new EventArgs());
                if (this is AutoHScrollBar)
                {
                    Widget w = this.Parent as Widget;
                    w.ScrolledX = (int) Math.Round((w.MaxChildWidth - w.Viewport.Width) * this.Value);
                    w.UpdateBounds();
                }
                Redraw();
            }
        }

        public void SetSliderSize(double size)
        {
            double minsize = (double) MinSliderSize / this.Size.Width;
            size = Math.Max(Math.Min(size, 1), 0);
            size = Math.Max(size, minsize);
            if (this.SliderSize != size)
            {
                this.SliderSize = size;
                this.Redraw();
            }
        }

        protected override void Draw()
        {
            int width = this.Size.Width - 4;
            int sliderwidth = (int) Math.Round(width * this.SliderSize);
            Color sc = new Color(205, 205, 205);
            if (this.SliderIM.ClickedLeftInArea == true)
            {
                sc.Set(96, 96, 96);
            }
            else if (this.SliderIM.HoverAnim())
            {
                sc.Set(192, 192, 192);
            }

            this.Sprites["bar"].Bitmap.Unlock();
            (this.Sprites["bar"].Bitmap as SolidBitmap).SetSize(this.Size.Width, 2);
            this.Sprites["bar"].Bitmap.Lock();

            this.Sprites["slider"].Bitmap.Unlock();
            (this.Sprites["slider"].Bitmap as SolidBitmap).SetSize(sliderwidth, 8);
            (this.Sprites["slider"].Bitmap as SolidBitmap).SetColor(sc);
            this.Sprites["slider"].Bitmap.Lock();
            this.Sprites["slider"].X = 2 + (int) Math.Round((width - sliderwidth) * this.Value);
            base.Draw();
        }

        public override void Update()
        {
            // Slider Input management
            int width = this.Size.Width - 4;
            int sliderwidth = (int) Math.Round(width * this.SliderSize);
            int sx = (int) Math.Round((width - sliderwidth) * this.Value);
            this.SliderRect = new Rect(this.Viewport.X + sx + 2, this.Viewport.Y, sliderwidth, 13);

            this.SliderIM.Update(this.SliderRect);

            base.Update();
        }

        private void SliderMouseMoving(object sender, MouseEventArgs e)
        {
            if (this.SliderIM.ClickedLeftInArea == true)
            {
                UpdateSlider(e);
            }
        }

        private void SliderMouseRight(object sender, MouseEventArgs e)
        {
            if (e.LeftButton && !e.OldLeftButton && this.SliderIM.Hovering)
            {
                this.SliderRX = e.X - this.Viewport.X - (this.SliderRect.X - this.Viewport.X);
                UpdateSlider(e);
            }
        }

        private void SliderMouseLeft(object sender, MouseEventArgs e)
        {
            if (!e.LeftButton && e.OldLeftButton && this.SliderIM.ClickedLeftInArea == true)
            {
                Redraw();
            }
        }

        private void SliderHoverChanged(object sender, MouseEventArgs e)
        {
            Redraw();
        }

        public void UpdateSlider(MouseEventArgs e)
        {
            int width = this.Size.Width - 4;
            int sliderwidth = (int) Math.Round(width * this.SliderSize);
            width -= sliderwidth;
            int newx = (e.X - this.Viewport.X - 2 - this.SliderRX);
            newx = Math.Max(Math.Min(newx, width), 0);
            this.SetValue((double) newx / width);
            if (this.Parent is Widget && (this.Parent as Widget).AutoScroll)
            {
                Widget w = this.Parent as Widget;
                w.ScrolledX = (int) Math.Round((w.MaxChildWidth - w.Viewport.Width) * this.Value);
                w.UpdateBounds();
            }
        }

        public void ScrollLeft()
        {
            Widget w = this.Parent as Widget;
            this.SetValue((w.ScrolledX - 11d) / (w.MaxChildWidth - w.Viewport.Width));
        }

        public void ScrollRight()
        {
            Widget w = this.Parent as Widget;
            this.SetValue((w.ScrolledX + 11d) / (w.MaxChildWidth - w.Viewport.Width));
        }

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            // If a VScrollBar exists
            if (Parent.ScrollBarY != null)
            {
                // Return if NOT pressing shift (i.e. VScrollBar will scroll instead)
                if (!Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT)) return;
            }
            bool inside = false;
            if (this.MouseInputRect != null) inside = this.MouseInputRect.Contains(e.X, e.Y);
            else inside = this.Viewport.Contains(e.X, e.Y);
            if (inside)
            {
                int leftcount = 0;
                int rightcount = 0;
                if (e.WheelY < 0) rightcount = Math.Abs(e.WheelY);
                else leftcount = e.WheelY;
                for (int i = 0; i < leftcount * 3; i++) this.ScrollLeft();
                for (int i = 0; i < rightcount * 3; i++) this.ScrollRight();
            }
        }
    }

    public class AutoHScrollBar : MinimalHScrollBar
    {
        public AutoHScrollBar(object Parent, string Name = "autoScrollBar")
            : base(Parent, Name)
        {

        }
    }
}