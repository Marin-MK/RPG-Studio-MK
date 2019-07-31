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

        bool StartedUp = false;
        bool StartedDown = false;
        long LeftDown = 0;

        public NumericBox(object Parent, string Name = "numericBox")
            : base(Parent, Name)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            WidgetIM.OnMouseMoving += MouseMoving;
            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseDown += MouseDown;
            WidgetIM.OnMouseUp += MouseUp;
            WidgetIM.OnMousePress += MousePress;
        }

        public void SetValue(int Value)
        {
            if (Value > MaxValue) Value = MaxValue;
            if (Value < MinValue) Value = MinValue;
            if (this.Value != Value)
            {
                this.Value = Value;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["box"].Bitmap != null) Sprites["box"].Bitmap.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Color gray = new Color(108, 103, 110);
            Color dark = new Color(36, 34, 36);
            Sprites["box"].Bitmap.SetPixel(1, 1, gray);
            Sprites["box"].Bitmap.DrawLine(2, 0, Size.Width - 3, 0, gray);
            Sprites["box"].Bitmap.DrawLine(2, 1, Size.Width - 3, 1, dark);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, 1, gray);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 1, 2, Size.Width - 1, Size.Height - 3, gray);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 2, 2, Size.Width - 2, Size.Height - 3, dark);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, gray);
            Sprites["box"].Bitmap.DrawLine(2, Size.Height - 1, Size.Width - 3, Size.Height - 1, gray);
            Sprites["box"].Bitmap.DrawLine(2, Size.Height - 2, Size.Width - 3, Size.Height - 2, dark);
            Sprites["box"].Bitmap.SetPixel(1, Size.Height - 2, gray);
            Sprites["box"].Bitmap.DrawLine(0, 2, 0, Size.Height - 3, gray);
            Sprites["box"].Bitmap.DrawLine(1, 2, 1, Size.Height - 3, dark);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 20, 2, Size.Width - 20, Size.Height - 3, dark);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 19, 13, Size.Width - 3, 13, dark);
            Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 22, Size.Height - 4, gray);

            Color up = gray;
            Color down = gray;
            if (SelectedUp) up = dark;
            else if (HoveringUp) up = new Color(79, 82, 91);
            if (SelectedDown) down = dark;
            else if (HoveringDown) down = new Color(79, 82, 91);

            Sprites["box"].Bitmap.FillRect(Size.Width - 19, 2, 17, 11, up);
            Sprites["box"].Bitmap.FillRect(Size.Width - 19, 14, 17, 11, down);

            int x = Size.Width - 14;
            int y = 6;
            #region Draw up arrow
            Color uparrow = new Color(55, 51, 55);
            Sprites["box"].Bitmap.SetPixel(x + 3, y, uparrow);
            Sprites["box"].Bitmap.DrawLine(x + 2, y + 1, x + 4, y + 1, uparrow);
            Sprites["box"].Bitmap.DrawLine(x + 1, y + 2, x + 5, y + 2, uparrow);
            Sprites["box"].Bitmap.DrawLine(x, y + 3, x + 6, y + 3, uparrow);
            #endregion

            y = 17;
            #region Draw down arrow
            Color downarrow = new Color(55, 51, 55);
            Sprites["box"].Bitmap.DrawLine(x, y, x + 6, y, downarrow);
            Sprites["box"].Bitmap.DrawLine(x + 1, y + 1, x + 5, y + 1, downarrow);
            Sprites["box"].Bitmap.DrawLine(x + 2, y + 2, x + 4, y + 2, downarrow);
            Sprites["box"].Bitmap.SetPixel(x + 3, y + 3, downarrow);
            #endregion

            Font f = Font.Get("Fonts/Ubuntu-R", 14);
            Sprites["box"].Bitmap.Font = f;
            string text = "";
            if (Value < 0) text += "-";
            text += Math.Abs(Value).ToString();
            Sprites["box"].Bitmap.DrawText(text, 5, 5, Color.WHITE);

            Sprites["box"].Bitmap.Lock();

            base.Draw();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            bool oldselup = SelectedUp;
            bool oldseldown = SelectedDown;
            bool oldup = HoveringUp;
            bool olddown = HoveringDown;
            SelectedUp = false;
            SelectedDown = false;
            HoveringUp = false;
            HoveringDown = false;
            int rx = e.X - Viewport.X + Position.X - ScrolledPosition.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            if (rx < 0 || ry < 0 || rx >= Size.Width || ry >= Size.Height) return;
            if (rx >= Size.Width - 20 && ry > 1 && ry < Size.Height - 2)
            {
                if (ry < 13 && !StartedDown)
                {
                    if (StartedUp) SelectedUp = true;
                    HoveringUp = true;
                }
                else if (ry > 13 && !StartedUp)
                {
                    if (StartedDown) SelectedDown = true;
                    HoveringDown = true;
                }
            }
            if (!HoveringUp && StartedUp) HoveringUp = true;
            if (!HoveringDown && StartedDown) HoveringDown = true;
            if (oldup != HoveringUp || olddown != HoveringDown ||
                oldselup != SelectedUp || oldseldown != SelectedDown) Redraw();
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (!WidgetIM.Hovering)
            {
                if (StartedDown || StartedUp)
                {
                    if (StartedDown)
                        HoveringDown = true;
                    else if (StartedUp)
                        HoveringUp = true;
                    SelectedDown = false;
                    SelectedUp = false;
                    Redraw();
                }
                else if (HoveringUp || HoveringDown)
                {
                    HoveringUp = false;
                    HoveringDown = false;
                    Redraw();
                }
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            SelectedUp = false;
            SelectedDown = false;
            if (HoveringUp)
            {
                SelectedUp = true;
                StartedUp = true;
                SetValue(Value + Increment);
            }
            if (HoveringDown)
            {
                SelectedDown = true;
                StartedDown = true;
                SetValue(Value - Increment);
            }
        }

        public override void MouseUp(object sender, MouseEventArgs e)
        {
            base.MouseUp(sender, e);
            if (e.LeftButton != e.OldLeftButton)
            {
                StartedUp = false;
                StartedDown = false;
                LeftDown = 0;
                MouseMoving(sender, e);
            }
        }

        public override void MousePress(object sender, MouseEventArgs e)
        {
            base.MousePress(sender, e);
            if (e.LeftButton)
            {
                if (LeftDown > 16 && LeftDown % 8 == 0)
                {
                    if (SelectedUp) SetValue(Value + Increment);
                    else if (SelectedDown) SetValue(Value - Increment);
                }
                LeftDown++;
            }
        }
    }
}
