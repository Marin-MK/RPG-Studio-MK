using System;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class ZoomControl : Widget
    {
        IconButton ZoomOut;
        IconButton ZoomIn;

        public double Factor { get; protected set; } = 1;

        public ZoomControl(IContainer Parent) : base(Parent)
        {
            ZoomOut = new IconButton(this);
            ZoomOut.Selectable = false;
            ZoomOut.SetIcon(7, 0);
            ZoomOut.SetSelectorOffset(-2);
            ZoomOut.OnMouseDown += delegate (MouseEventArgs e)
            {
                if (ZoomOut.WidgetIM.Hovering && e.LeftButton != e.OldLeftButton) DecreaseZoom();
            };
            ZoomIn = new IconButton(this);
            ZoomIn.Selectable = false;
            ZoomIn.SetIcon(8, 0);
            ZoomIn.SetPosition(65, 0);
            ZoomIn.SetSelectorOffset(-2);
            ZoomIn.OnMouseDown += delegate (MouseEventArgs e)
            {
                if (ZoomIn.WidgetIM.Hovering && e.LeftButton != e.OldLeftButton) IncreaseZoom();
            };

            Sprites["text"] = new Sprite(this.Viewport);

            SetSize(88, 26);
        }

        public void SetZoomFactor(double Factor, bool FromMapViewer = false)
        {
            if (this.Factor != Factor)
            {
                this.Factor = Factor;
                if (!FromMapViewer)
                {
                    if (Editor.MainWindow.MapWidget != null) Editor.MainWindow.MapWidget.SetZoomFactor(Factor, true);
                    if (Editor.MainWindow.EventingWidget != null) Editor.MainWindow.EventingWidget.SetZoomFactor(Factor, true);
                }
                Redraw();
            }
        }

        public void IncreaseZoom()
        {
            if (this.Factor == 0.125) SetZoomFactor(0.25);
            else if (this.Factor == 0.25) SetZoomFactor(0.5);
            else if (this.Factor == 0.5) SetZoomFactor(1.0);
            else if (this.Factor == 1.0) SetZoomFactor(2.0);
            else if (this.Factor == 2.0) SetZoomFactor(3.0);
            else if (this.Factor == 3.0) SetZoomFactor(4.0);
        }

        public void DecreaseZoom()
        {
            if (this.Factor == 0.25) SetZoomFactor(0.125);
            else if (this.Factor == 0.5) SetZoomFactor(0.25);
            else if (this.Factor == 1.0) SetZoomFactor(0.5);
            else if (this.Factor == 2.0) SetZoomFactor(1.0);
            else if (this.Factor == 3.0) SetZoomFactor(2.0);
            else if (this.Factor == 4.0) SetZoomFactor(3.0);
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Font f = Font.Get("Fonts/ProductSans-M", 14);

            string text = "";
            if (100 * this.Factor / Math.Floor(100 * this.Factor) == 0)
            {
                text = (100 * this.Factor).ToString() + "%";
            }
            else
            {
                text = (Math.Round(1000 * this.Factor) / 10d).ToString() + "%";
            }
            Size s = f.TextSize(text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText(text, Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = Size.Width / 2 - s.Width / 2 + 1;
            Sprites["text"].Y = 4;
            base.Draw();
        }
    }
}
