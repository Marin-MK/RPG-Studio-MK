using System;
using System.Linq;
using ODL;

namespace MKEditor.Widgets
{
    public class HScrollBar : Widget
    {
        public double SliderSize { get; protected set; }
        public double Value { get; protected set; }
        public bool Hovering { get { return SliderIM.Hovering; } }
        public bool Dragging { get { return SliderIM.ClickedLeftInArea == true; } }
        public Rect MouseInputRect { get; set; }

        public Widget LinkedWidget;

        public int MinSliderSize = 8;

        public EventHandler<EventArgs> OnValueChanged;
        public EventHandler<DirectionEventArgs> OnControlScrolling;

        private Rect SliderRect;
        private int SliderRX = 0;
        private MouseInputManager SliderIM;

        public HScrollBar(object Parent, string Name = "hScrollBar")
            : base(Parent, Name)
        {
            this.Size = new Size(17, 60);
            this.ConsiderInAutoScrollPositioning = this.ConsiderInAutoScrollCalculation = false;
            this.WidgetIM.OnMouseWheel += MouseWheel;
            this.Sprites["slider"] = new Sprite(this.Viewport);
            this.SliderSize = 0.25;
            this.Value = 0;
            this.SliderIM = new MouseInputManager(this);
            this.SliderIM.OnMouseMoving += SliderMouseMoving;
            this.SliderIM.OnMouseDown += SliderMouseDown;
            this.SliderIM.OnMouseUp += SliderMouseUp;
            this.SliderIM.OnHoverChanged += SliderHoverChanged;
        }

        public void SetValue(double value, bool CallEvent = true)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;
            if (this.Value != value)
            {
                this.Value = value;
                if (LinkedWidget != null)
                {
                    if (LinkedWidget.MaxChildWidth > LinkedWidget.Viewport.Width)
                    {
                        LinkedWidget.ScrolledX = (int) Math.Round((LinkedWidget.MaxChildWidth - LinkedWidget.Viewport.Width) * this.Value);
                        LinkedWidget.UpdateBounds();
                    }
                }
                if (CallEvent && this.OnValueChanged != null) this.OnValueChanged.Invoke(this, new EventArgs());
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
            int width = this.Size.Width;
            int sliderwidth = (int) Math.Round(width * this.SliderSize);
            Color sc = new Color(64, 104, 146);
            if (this.SliderIM.ClickedLeftInArea == true || SliderIM.HoverAnim())
            {
                sc = new Color(59, 227, 255);
            }
            if (this.Sprites["slider"].Bitmap != null) this.Sprites["slider"].Bitmap.Dispose();
            this.Sprites["slider"].Bitmap = new Bitmap(sliderwidth, 8);
            this.Sprites["slider"].Bitmap.Unlock();
            this.Sprites["slider"].Bitmap.FillRect(sliderwidth - 1, 7, sc);
            this.Sprites["slider"].Bitmap.DrawLine(0, 7, sliderwidth - 1, 7, Color.BLACK);
            this.Sprites["slider"].Bitmap.DrawLine(sliderwidth - 1, 0, sliderwidth - 1, 6, Color.BLACK);
            this.Sprites["slider"].Bitmap.Lock();
            this.Sprites["slider"].X = (int) Math.Round((width - sliderwidth) * this.Value);
            base.Draw();
        }

        public override void Update()
        {
            // Slider Input management
            int width = this.Size.Width;
            int sliderwidth = (int) Math.Round(width * this.SliderSize);
            int sx = (int) Math.Round((width - sliderwidth) * this.Value);
            this.SliderRect = new Rect(this.Viewport.X + sx, this.Viewport.Y, sliderwidth, 8);

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

        private void SliderMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton && !e.OldLeftButton && this.SliderIM.Hovering)
            {
                this.SliderRX = e.X - this.Viewport.X - (this.SliderRect.X - this.Viewport.X);
                UpdateSlider(e);
            }
        }

        private void SliderMouseUp(object sender, MouseEventArgs e)
        {
            Redraw();
        }

        private void SliderHoverChanged(object sender, MouseEventArgs e)
        {
            Redraw();
        }

        public void UpdateSlider(MouseEventArgs e)
        {
            int width = this.Size.Width;
            int sliderwidth = (int) Math.Round(width * this.SliderSize);
            width -= sliderwidth;
            int newx = (e.X - this.Viewport.X - this.SliderRX);
            newx = Math.Max(Math.Min(newx, width), 0);
            this.SetValue((double) newx / width);
            if (LinkedWidget.HAutoScroll)
            {
                LinkedWidget.ScrolledX = (int) Math.Round((LinkedWidget.MaxChildWidth - LinkedWidget.Viewport.Width) * this.Value);
                LinkedWidget.UpdateBounds();
            }
        }

        public void ScrollLeft()
        {
            this.SetValue((LinkedWidget.ScrolledX - 11d) / (LinkedWidget.MaxChildWidth - LinkedWidget.Viewport.Width));
        }

        public void ScrollRight()
        {
            this.SetValue((LinkedWidget.ScrolledX + 11d) / (LinkedWidget.MaxChildWidth - LinkedWidget.Viewport.Width));
        }

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            // If a VScrollBar exists
            if (LinkedWidget.VScrollBar != null)
            {
                // Return if NOT pressing shift (i.e. VScrollBar will scroll instead)
                if (!Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) && !Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT)) return;
            }
            bool inside = false;
            if (this.MouseInputRect != null) inside = this.MouseInputRect.Contains(e.X, e.Y);
            else inside = this.Viewport.Contains(e.X, e.Y);
            if (inside)
            {
                if (Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RCTRL))
                {
                    if (OnControlScrolling != null) OnControlScrolling.Invoke(sender, new DirectionEventArgs(e.WheelY > 0, e.WheelY < 0));
                }
                else
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
    }
}