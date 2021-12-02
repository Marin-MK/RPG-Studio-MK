using System;
using System.Collections.Generic;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class ProgressBar : Widget
    {
        public float Progress { get; protected set; }

        public ProgressBar(IContainer Parent) : base (Parent)
        {
            MinimumSize.Height = MaximumSize.Height = 34;
            Sprites["box"] = new Sprite(this.Viewport);
            Sprites["progress"] = new Sprite(this.Viewport);
            Sprites["progress"].X = 1;
            Sprites["progress"].Y = 1;
            Sprites["progress"].Bitmap = new Bitmap(1, 32);
            Sprites["progress"].Bitmap.Unlock();
            #region Build gradient
            Sprites["progress"].Bitmap.SetPixel(0, 0, new Color(54, 162, 60));
            Sprites["progress"].Bitmap.SetPixel(0, 1, new Color(54, 164, 62));
            Sprites["progress"].Bitmap.SetPixel(0, 2, new Color(55, 165, 62));
            Sprites["progress"].Bitmap.SetPixel(0, 3, new Color(56, 167, 63));
            Sprites["progress"].Bitmap.SetPixel(0, 4, new Color(57, 169, 64));
            Sprites["progress"].Bitmap.SetPixel(0, 5, new Color(58, 171, 65));
            Sprites["progress"].Bitmap.SetPixel(0, 6, new Color(59, 174, 67));
            Sprites["progress"].Bitmap.SetPixel(0, 7, new Color(60, 175, 67));
            Sprites["progress"].Bitmap.SetPixel(0, 8, new Color(61, 178, 68));
            Sprites["progress"].Bitmap.SetPixel(0, 9, new Color(61, 180, 69));
            Sprites["progress"].Bitmap.SetPixel(0, 10, new Color(62, 181, 70));
            Sprites["progress"].Bitmap.SetPixel(0, 11, new Color(64, 184, 71));
            Sprites["progress"].Bitmap.SetPixel(0, 12, new Color(64, 185, 72));
            Sprites["progress"].Bitmap.SetPixel(0, 13, new Color(65, 188, 72));
            Sprites["progress"].Bitmap.SetPixel(0, 14, new Color(65, 189, 74));
            Sprites["progress"].Bitmap.SetPixel(0, 15, new Color(74, 191, 74));
            Sprites["progress"].Bitmap.SetPixel(0, 16, new Color(67, 194, 76));
            Sprites["progress"].Bitmap.SetPixel(0, 17, new Color(69, 196, 77));
            Sprites["progress"].Bitmap.SetPixel(0, 18, new Color(70, 198, 78));
            Sprites["progress"].Bitmap.SetPixel(0, 19, new Color(70, 200, 78));
            Sprites["progress"].Bitmap.SetPixel(0, 20, new Color(71, 202, 80));
            Sprites["progress"].Bitmap.SetPixel(0, 21, new Color(72, 204, 80));
            Sprites["progress"].Bitmap.SetPixel(0, 22, new Color(73, 205, 81));
            Sprites["progress"].Bitmap.SetPixel(0, 23, new Color(74, 208, 82));
            Sprites["progress"].Bitmap.SetPixel(0, 24, new Color(74, 210, 83));
            Sprites["progress"].Bitmap.SetPixel(0, 25, new Color(76, 212, 85));
            Sprites["progress"].Bitmap.SetPixel(0, 26, new Color(77, 213, 85));
            Sprites["progress"].Bitmap.SetPixel(0, 27, new Color(78, 216, 87));
            Sprites["progress"].Bitmap.SetPixel(0, 28, new Color(79, 218, 87));
            Sprites["progress"].Bitmap.SetPixel(0, 29, new Color(79, 220, 88));
            Sprites["progress"].Bitmap.SetPixel(0, 30, new Color(80, 221, 89));
            Sprites["progress"].Bitmap.SetPixel(0, 31, new Color(80, 223, 90));
            #endregion
            Sprites["progress"].Bitmap.Lock();
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].Y = 9;
            SetHeight(34);
        }

        public void SetProgress(float Progress)
        {
            Progress = Math.Clamp(Progress, 0, 1);
            if (this.Progress != Progress)
            {
                this.Progress = Progress;
                this.Redraw();
            }
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            Sprites["box"].Bitmap?.Dispose();
            Sprites["box"].Bitmap = new Bitmap(Size);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.DrawLine(1, 0, Size.Width - 2, 0, new Color(86, 108, 134));
            Sprites["box"].Bitmap.DrawLine(1, Size.Height - 1, Size.Width - 2, Size.Height - 1, new Color(86, 108, 134));
            Sprites["box"].Bitmap.DrawLine(0, 1, 0, Size.Height - 2, new Color(86, 108, 134));
            Sprites["box"].Bitmap.DrawLine(Size.Width - 1, 1, Size.Width - 1, Size.Height - 2, new Color(86, 108, 134));
            Sprites["box"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(10, 23, 37));
            Sprites["box"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            base.Draw();
            Sprites["text"].Bitmap?.Dispose();
            string text = (Math.Round(this.Progress * 1000d) / 10d).ToString() + "%";
            Font f = Fonts.UbuntuRegular.Use(14);
            Size s = f.TextSize(text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText(text, Color.WHITE);
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            Sprites["text"].Bitmap.Lock();

            Sprites["progress"].ZoomX = this.Progress * (Size.Width - 2);
        }
    }
}
