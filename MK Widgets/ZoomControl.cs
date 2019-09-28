using System;
using ODL;

namespace MKEditor.Widgets
{
    public class ZoomControl : Widget
    {
        IconButton ZoomOut;
        IconButton ZoomIn;

        public int Level { get; protected set; }

        public ZoomControl(object Parent, string Name = "zoomControl")
            : base(Parent, Name)
        {
            Viewport.Name = "ZoomControl";
            ZoomOut = new IconButton(this);
            ZoomOut.Selectable = false;
            ZoomOut.SetIcon(7, 0);
            ZoomOut.SetSelectorOffset(-2);
            ZoomOut.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                SetLevel(Level - 1);
            };
            ZoomIn = new IconButton(this);
            ZoomIn.Selectable = false;
            ZoomIn.SetIcon(8, 0);
            ZoomIn.SetPosition(65, 0);
            ZoomIn.SetSelectorOffset(-2);
            ZoomIn.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                SetLevel(Level + 1);
            };

            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].Name = "ZoomText";

            SetLevel(0);
            SetSize(88, 26);
        }

        public void SetLevel(int Level)
        {
            if (Level < -3) Level = -3;
            if (Level > 3) Level = 3;
            if (this.Level != Level)
            {
                this.Level = Level;
                double f = 1;
                if (Level == -3) f = 0.125;
                if (Level == -2) f = 0.25;
                if (Level == -1) f = 0.5;
                if (Level == 1) f = 2;
                if (Level == 2) f = 3;
                if (Level == 3) f = 4;
                (Parent as StatusBar).MapViewer.SetZoomFactor(f);
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            string text = "";
            if (Level == -3) text = "12.5%";
            if (Level == -2) text = "25%";
            if (Level == -1) text = "50%";
            if (Level == 0) text = "100%";
            if (Level == 1) text = "200%";
            if (Level == 2) text = "300%";
            if (Level == 3) text = "400%";
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
