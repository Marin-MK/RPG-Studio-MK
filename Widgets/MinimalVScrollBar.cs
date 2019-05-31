using System;
using System.Linq;
using ODL;

namespace MKEditor.Widgets
{
    public class MinimalVScrollBar : Widget
    {
        public double SliderSize { get; protected set; }
        public double Value { get; protected set; }
        public double ScrollStep = 0;

        public Rect MouseInputRect { get; set; }

        public int MinSliderHeight = 8;

        public MouseInputManager SliderIM;

        public EventHandler<EventArgs> OnValueChanged;

        private Rect SliderRect;
        private int SliderRY = 0;

        public MinimalVScrollBar(object Parent, string Name = "vScrollBar")
            : base(Parent, Name)
        {
            this.Size = new Size(17, 60);
            this.WidgetIM.OnMouseWheel += MouseWheel;
            this.Sprites["bar"] = new Sprite(this.Viewport);
            this.Sprites["slider"] = new Sprite(this.Viewport);
            this.Sprites["slider"].X = 5;
            this.Sprites["slider"].Bitmap = new SolidBitmap(1, 1, new Color(186, 186, 186));
            this.SliderSize = 0.25;
            this.Value = 0;
            this.SliderIM = new MouseInputManager(this);
            this.SliderIM.OnMouseMoving += SliderMouseMoving;
            this.SliderIM.OnMouseDown += SliderMouseDown;
            this.SliderIM.OnMouseUp += SliderMouseUp;
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
                Redraw();
            }
        }

        public void SetSliderSize(double size)
        {
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
            Color sc = new Color(205, 205, 205);
            if (this.SliderIM.ClickedInArea == true)
            {
                sc.Set(96, 96, 96);
            }
            else if (this.SliderIM.HoverAnim())
            {
                sc.Set(192, 192, 192);
            }

            this.Sprites["slider"].Bitmap.Unlock();
            (this.Sprites["slider"].Bitmap as SolidBitmap).SetSize(2, sliderheight);
            (this.Sprites["slider"].Bitmap as SolidBitmap).SetColor(sc);
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
            this.SliderRect = new Rect(this.Viewport.X, this.Viewport.Y + sy, 13, sliderheight);

            this.SliderIM.Update(this.SliderRect);

            base.Update();
        }
        
        private void SliderMouseMoving(object sender, MouseEventArgs e)
        {
            if (this.SliderIM.ClickedInArea == true)
            {
                UpdateSlider(e);
            }
        }

        private void SliderMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton && !e.OldLeftButton && this.SliderIM.Hovering)
            {
                this.SliderRY = e.Y - this.Viewport.Y - (this.SliderRect.Y - this.Viewport.Y);
                UpdateSlider(e);
            }
        }

        private void SliderMouseUp(object sender, MouseEventArgs e)
        {
            if (!e.LeftButton && e.OldLeftButton && this.SliderIM.ClickedInArea == true)
            {
                Redraw();
            }
        }

        private void SliderHoverChanged(object sender, MouseEventArgs e)
        {
            Redraw();
        }

        private void UpdateSlider(MouseEventArgs e)
        {
            int height = this.Size.Height;
            int sliderheight = (int) Math.Round(height * this.SliderSize);
            height -= sliderheight;
            int newy = (e.Y - this.Viewport.Y - this.SliderRY);
            newy = Math.Max(Math.Min(newy, height), 0);
            this.SetValue((double) newy / height);
            if (this.Parent is ListBox)
            {
                ListBox box = this.Parent as ListBox;
                box.SetTopIndex((int) Math.Round(this.Value * (box.Items.Count - box.VisibleItems)));
            }
            else if (this.Parent is Widget && (this.Parent as Widget).AutoScroll)
            {
                this.Parent.ScrollPercentageY = this.Value;
            }
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
                if (this.ScrollStep > 0)
                {
                    this.SetValue(this.Value - this.ScrollStep);
                }
                else
                {
                    this.SetValue(this.Value - this.SliderSize / 10f);

                }
            }
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
                if (this.ScrollStep > 0)
                {
                    this.SetValue(this.Value + this.ScrollStep);
                }
                else
                {
                    this.SetValue(this.Value + this.SliderSize / 10f);

                }
            }
        }

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            bool inside = false;
            if (this.MouseInputRect != null) inside = this.MouseInputRect.Contains(e.X, e.Y);
            else inside = this.Viewport.Contains(e.X, e.Y);
            if (!(this.Parent is ListBox) && inside)
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