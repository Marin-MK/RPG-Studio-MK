using System;
using System.Linq;
using ODL;

namespace MKEditor.Widgets
{
    public class VScrollBar : Widget
    {
        public double SliderSize     { get; protected set; }
        public double Value          { get; protected set; }
        public bool   Hovering       { get { return SliderIM.Hovering; } }
        public bool   Dragging       { get { return SliderIM.ClickedLeftInArea == true; } }
        public Rect   MouseInputRect { get; set; }

        public Widget LinkedWidget;

        public int MinSliderHeight = 8;
        double OriginalSize = 0.1;

        public EventHandler<EventArgs> OnValueChanged;
        public EventHandler<DirectionEventArgs> OnControlScrolling;

        private Rect SliderRect;
        private int SliderRY = 0;
        private MouseInputManager SliderIM;

        public VScrollBar(object Parent, string Name = "vScrollBar")
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
                    if (LinkedWidget.MaxChildHeight > LinkedWidget.Viewport.Height)
                    {
                        LinkedWidget.ScrolledY = (int) Math.Round((LinkedWidget.MaxChildHeight - LinkedWidget.Viewport.Height) * this.Value);
                        LinkedWidget.UpdateBounds();
                    }
                }
                if (CallEvent && this.OnValueChanged != null) this.OnValueChanged.Invoke(this, new EventArgs());
                Redraw();
            }
        }

        public override Widget SetSize(Size size)
        {
            base.SetSize(size);
            SetSliderSize(OriginalSize);
            return this;
        }

        public void SetSliderSize(double size)
        {
            OriginalSize = size;
            double minsize = (double) MinSliderHeight / this.Size.Height;
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
            int height = this.Size.Height;
            int sliderheight = (int) Math.Round(height * this.SliderSize);
            Color sc = new Color(64, 104, 146);
            if (this.SliderIM.ClickedLeftInArea == true || SliderIM.HoverAnim())
            {
                sc = new Color(59, 227, 255);
            }
            if (this.Sprites["slider"].Bitmap != null) this.Sprites["slider"].Bitmap.Dispose();
            this.Sprites["slider"].Bitmap = new Bitmap(8, sliderheight);
            this.Sprites["slider"].Bitmap.Unlock();
            this.Sprites["slider"].Bitmap.FillRect(7, sliderheight - 1, sc);
            this.Sprites["slider"].Bitmap.DrawLine(7, 0, 7, sliderheight - 1, Color.BLACK);
            this.Sprites["slider"].Bitmap.DrawLine(0, sliderheight - 1, 7, sliderheight - 1, Color.BLACK);
            this.Sprites["slider"].Bitmap.Lock();
            this.Sprites["slider"].Y = (int) Math.Round((height - sliderheight) * this.Value);
            base.Draw();
        }

        public override void Update()
        {
            // Slider Input management
            int height = this.Size.Height;
            int sliderheight = (int) Math.Round(height * this.SliderSize);
            int sy = (int) Math.Round((height - sliderheight) * this.Value);
            this.SliderRect = new Rect(this.Viewport.X, this.Viewport.Y + sy, 8, sliderheight);

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
            if (!IsVisible()) return;
            if (e.LeftButton && !e.OldLeftButton && this.SliderIM.Hovering)
            {
                this.SliderRY = e.Y - this.Viewport.Y - (this.SliderRect.Y - this.Viewport.Y);
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
            if (!IsVisible()) return;
            int height = this.Size.Height;
            int sliderheight = (int) Math.Round(height * this.SliderSize);
            height -= sliderheight;
            int newy = (e.Y - this.Viewport.Y - this.SliderRY);
            newy = Math.Max(Math.Min(newy, height), 0);
            this.SetValue((double) newy / height);
            if (LinkedWidget.VAutoScroll)
            {
                LinkedWidget.ScrolledY = (int) Math.Round((LinkedWidget.MaxChildHeight - LinkedWidget.Viewport.Height) * this.Value);
                LinkedWidget.UpdateBounds();
            }
        }

        public void ScrollUp()
        {
            if (!IsVisible()) return;
            this.SetValue((LinkedWidget.ScrolledY - 11d) / (LinkedWidget.MaxChildHeight - LinkedWidget.Viewport.Height));
        }

        public void ScrollDown()
        {
            if (!IsVisible()) return;
            this.SetValue((LinkedWidget.ScrolledY + 11d) / (LinkedWidget.MaxChildHeight - LinkedWidget.Viewport.Height));
        }

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            if (!IsVisible()) return;
            // If a HScrollBar exists
            if (LinkedWidget.HScrollBar != null)
            {
                // Return if pressing shift (i.e. HScrollBar will scroll instead)
                if (Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT)) return;
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
                    int downcount = 0;
                    int upcount = 0;
                    if (e.WheelY < 0) downcount = Math.Abs(e.WheelY);
                    else upcount = e.WheelY;
                    for (int i = 0; i < downcount * 3; i++) this.ScrollDown();
                    for (int i = 0; i < upcount * 3; i++) this.ScrollUp();
                }
            }
        }
    }
}