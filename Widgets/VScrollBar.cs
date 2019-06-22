using System;
using ODL;

namespace MKEditor.Widgets
{
    public class VScrollBar : Widget
    {
        public double SliderSize { get; protected set; }
        public double Value { get; protected set; }

        public int MinSliderHeight = 8;

        private bool RedrawArrows;

        public MouseInputManager SliderIM;
        public MouseInputManager UpArrowIM;
        public MouseInputManager DownArrowIM;

        private Rect SliderRect;
        private int SliderRY = 0;

        private DateTime? UpArrowCount;
        private DateTime? DownArrowCount;

        public VScrollBar(object Parent)
            : base(Parent, "vScrollBar")
        {
            this.Size = new Size(17, 60);
            this.WidgetIM.OnMouseWheel += MouseWheel;
            this.Sprites["bar"] = new Sprite(this.Viewport);
            this.Sprites["slider"] = new Sprite(this.Viewport);
            this.SliderSize = 0.25;
            this.Value = 0;
            #region Initializes Slider Input Manager
            this.SliderIM = new MouseInputManager(this);
            this.SliderIM.OnMouseMoving += SliderMouseMoving;
            this.SliderIM.OnMouseDown += SliderMouseDown;
            this.SliderIM.OnMouseUp += SliderMouseUp;
            this.SliderIM.OnHoverChanged += SliderHoverChanged;
            #endregion
            #region Initializes Up Arrow Input Manager
            this.UpArrowIM = new MouseInputManager(this);
            this.UpArrowIM.OnMouseDown += UpArrowDown;
            this.UpArrowIM.OnMousePress += UpArrowPress;
            this.UpArrowIM.OnMouseUp += UpArrowUp;
            this.UpArrowIM.OnHoverChanged += UpArrowHover;
            #endregion
            #region Initializes Down Arrow Input Manager
            this.DownArrowIM = new MouseInputManager(this);
            this.DownArrowIM.OnMouseDown += DownArrowDown;
            this.DownArrowIM.OnMousePress += DownArrowPress;
            this.DownArrowIM.OnMouseUp += DownArrowUp;
            this.DownArrowIM.OnHoverChanged += DownArrowHover;
            #endregion
            this.RedrawArrows = true;
        }

        public void SetValue(double value)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;
            if (this.Value != value)
            {
                this.Value = value;
                Redraw();
            }
        }

        public void SetSliderSize(double size)
        {
            double minsize = (double) MinSliderHeight / (this.Size.Height - 34);
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
            if (this.RedrawSize || this.RedrawArrows)
            {
                if (this.Sprites["bar"].Bitmap != null) this.Sprites["bar"].Bitmap.Dispose();
                this.Sprites["bar"].Bitmap = new Bitmap(this.Size);
                this.Sprites["bar"].Bitmap.DrawLine(0, 0, 0, this.Size.Height - 1, Color.WHITE);
                this.Sprites["bar"].Bitmap.FillRect(1, 0, this.Size.Width - 1, this.Size.Height, 240, 240, 240);
                int rx = 5;
                int ry = 6;
                Color c = new Color(96, 96, 96);
                if (this.UpArrowIM.ClickAnim())
                {
                    c.Set(255, 255, 255);
                    this.Sprites["bar"].Bitmap.FillRect(1, 0, 15, 17, new Color(96, 96, 96));
                }
                else if (this.UpArrowIM.HoverAnim())
                {
                    c.Set(0, 0, 0);
                    this.Sprites["bar"].Bitmap.FillRect(1, 0, 15, 17, new Color(218, 218, 218));
                }
                this.Sprites["bar"].Bitmap.DrawLine(rx + 0, ry + 3, rx + 3, ry + 0, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 0, ry + 4, rx + 3, ry + 1, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 0, ry + 5, rx + 3, ry + 2, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 4, ry + 1, rx + 6, ry + 3, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 4, ry + 2, rx + 6, ry + 4, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 4, ry + 3, rx + 6, ry + 5, c);
                rx = 5;
                ry = this.Size.Height - 11;
                c = new Color(96, 96, 96);
                if (this.DownArrowIM.ClickAnim())
                {
                    c.Set(255, 255, 255);
                    this.Sprites["bar"].Bitmap.FillRect(1, this.Size.Height - 17, 15, 17, new Color(96, 96, 96));
                }
                else if (this.DownArrowIM.HoverAnim())
                {
                    c.Set(0, 0, 0);
                    this.Sprites["bar"].Bitmap.FillRect(1, this.Size.Height - 17, 15, 17, new Color(218, 218, 218));
                }
                this.Sprites["bar"].Bitmap.DrawLine(rx + 0, ry + 0, rx + 3, ry + 3, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 0, ry + 1, rx + 3, ry + 4, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 0, ry + 2, rx + 3, ry + 5, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 4, ry + 2, rx + 6, ry + 0, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 4, ry + 3, rx + 6, ry + 1, c);
                this.Sprites["bar"].Bitmap.DrawLine(rx + 4, ry + 4, rx + 6, ry + 2, c);
            }
            if (this.Sprites["slider"].Bitmap != null) this.Sprites["slider"].Bitmap.Dispose();
            this.Sprites["slider"].Bitmap = new Bitmap(this.Size);
            int height = this.Size.Height - 34;
            int sliderheight = (int) Math.Round(height * this.SliderSize);
            Color sc = new Color(205, 205, 205);
            if (this.SliderIM.ClickedLeftInArea == true)
            {
                sc.Set(96, 96, 96);
            }
            else if (this.SliderIM.HoverAnim() || this.UpArrowIM.HoverAnim() || this.DownArrowIM.HoverAnim())
            {
                sc.Set(192, 192, 192);
            }
            this.Sprites["slider"].Bitmap.FillRect(
                new Rect(1,
                    17 + (int) Math.Round((height - sliderheight) * this.Value),
                    15,
                    sliderheight),
                sc);
            this.RedrawArrows = false;
            base.Draw();
        }

        public override void Update()
        {
            // Slider Input management
            int height = this.Size.Height - 34;
            int sliderheight = (int) Math.Round(height * this.SliderSize);
            int sy = (int) Math.Round((height - sliderheight) * this.Value);
            this.SliderRect = new Rect(this.Viewport.X, this.Viewport.Y + 17 + sy, 17, sliderheight);
            this.SliderIM.Update(this.SliderRect);

            // Up Arrow Input management
            this.UpArrowIM.Update(new Rect(this.Viewport.X, this.Viewport.Y, 17, 17));

            // Down Arrow Input management
            this.DownArrowIM.Update(new Rect(this.Viewport.X, this.Viewport.Y + this.Size.Height - 17, 17, 17));

            base.Update();
        }

        #region Handles Slider
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
                this.SliderRY = e.Y - this.Viewport.Y - 17 - (this.SliderRect.Y - 17 - this.Viewport.Y);
                UpdateSlider(e);
            }
        }

        private void SliderMouseUp(object sender, MouseEventArgs e)
        {
            if (!e.LeftButton && e.OldLeftButton && this.SliderIM.ClickedLeftInArea == true)
            {
                this.RedrawArrows = true;
                Redraw();
            }
        }

        private void SliderHoverChanged(object sender, MouseEventArgs e)
        {
            Redraw();
        }

        private void UpdateSlider(MouseEventArgs e)
        {
            int height = this.Size.Height - 34;
            int sliderheight = (int) Math.Round(height * this.SliderSize);
            height -= sliderheight;
            int newy = (e.Y - this.Viewport.Y - 17 - this.SliderRY);
            newy = Math.Max(Math.Min(newy, height), 0);
            this.SetValue((double) newy / height);
            if (this.Parent is ListBox)
            {
                ListBox box = this.Parent as ListBox;
                box.SetTopIndex((int) Math.Round(this.Value * (box.Items.Count - box.VisibleItems)));
            }
        }
        #endregion

        #region Handles Up Arrow
        private void UpArrowDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton && !e.OldLeftButton && this.UpArrowIM.Hovering)
            {
                // Wait 0.3 seconds until the next valid button press
                UpArrowCount = new DateTime(DateTime.Now.Ticks + (int) (0.3 * 10000000));
                this.RedrawArrows = true;
                ScrollUp();
                Redraw();
            }
        }

        private void UpArrowPress(object sender, MouseEventArgs e)
        {
            if (UpArrowCount != null && UpArrowCount < DateTime.Now && this.UpArrowIM.Hovering)
            {
                // Wait 0.04 seconds until the next valid button press
                UpArrowCount = new DateTime(DateTime.Now.Ticks + (int) (0.04 * 10000000));
                ScrollUp();
            }
        }

        private void UpArrowUp(object sender, MouseEventArgs e)
        {
            if (!e.LeftButton && e.OldLeftButton && this.UpArrowIM.ClickedLeftInArea == true)
            {
                UpArrowCount = null;
                this.RedrawArrows = true;
                Redraw();
            }
        }

        private void UpArrowHover(object sender, MouseEventArgs e)
        {
            this.RedrawArrows = true;
            Redraw();
        }

        public void ScrollUp()
        {
            if (this.Parent is ListBox)
            {
                ListBox box = this.Parent as ListBox;
                if (box.TopIndex - 1 >= 0)
                {
                    box.SetTopIndex(box.TopIndex - 1);
                }
            }
            else
            {
                this.SetValue(this.Value - this.SliderSize / 10f);
            }
        }
        #endregion

        #region Handles Down Arrow
        private void DownArrowDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton && !e.OldLeftButton && this.DownArrowIM.Hovering)
            {
                // Wait 0.3 seconds until the next valid button press
                DownArrowCount = new DateTime(DateTime.Now.Ticks + (int) (0.3 * 10000000));
                this.RedrawArrows = true;
                ScrollDown();
                Redraw();
            }
        }

        private void DownArrowPress(object sender, MouseEventArgs e)
        {
            if (DownArrowCount != null && DownArrowCount < DateTime.Now && this.DownArrowIM.Hovering)
            {
                // Wait 0.04 seconds until the next valid button press
                DownArrowCount = new DateTime(DateTime.Now.Ticks + (int) (0.04 * 10000000));
                ScrollDown();
            }
        }

        private void DownArrowUp(object sender, MouseEventArgs e)
        {
            if (!e.LeftButton && e.OldLeftButton && this.DownArrowIM.ClickedLeftInArea == true)
            {
                DownArrowCount = null;
                this.RedrawArrows = true;
                Redraw();
            }
        }

        private void DownArrowHover(object sender, MouseEventArgs e)
        {
            this.RedrawArrows = true;
            Redraw();
        }

        public void ScrollDown()
        {
            if (this.Parent is ListBox)
            {
                ListBox box = this.Parent as ListBox;
                if (box.TopIndex + 1 <= box.Items.Count - box.VisibleItems)
                {
                    box.SetTopIndex(box.TopIndex + 1);
                }
            }
            else
            {
                this.SetValue(this.Value + this.SliderSize / 10f);
            }
        }
        #endregion

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            if (!(this.Parent is ListBox) && this.Viewport.Contains(e.X, e.Y))
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
