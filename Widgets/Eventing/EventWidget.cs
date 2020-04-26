using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class EventWidget : Widget
    {
        public Event EventData;
        public Point BoxPosition;
        public Size BoxSize;
        public double ZoomFactor;

        public EventWidget(IContainer Parent) : base(Parent)
        {
            Sprites["gfx"] = new Sprite(this.Viewport);
            Sprites["box"] = new Sprite(this.Viewport);
        }

        public void SetEvent(Event Event)
        {
            this.EventData = Event;
            Sprites["gfx"].Bitmap?.Dispose();
            if (Event.Pages.Count > 0)
            {
                EventGraphic gfx = Event.Pages[0].Graphic;
                if (gfx.Type == "file")
                {
                    if (System.IO.File.Exists(Data.ProjectPath + "/" + gfx.Param.ToString() + ".png"))
                    {
                        Sprites["gfx"].Bitmap = new Bitmap(Data.ProjectPath + "/" + gfx.Param.ToString());
                        Sprites["gfx"].SrcRect.Y = Sprites["gfx"].Bitmap.Height / 4 * ((gfx.Direction / 2) - 1);
                        Sprites["gfx"].SrcRect.Width = Sprites["gfx"].Bitmap.Width / 4;
                        Sprites["gfx"].SrcRect.Height = Sprites["gfx"].Bitmap.Height / 4;
                    }
                }
            }
        }

        public void SetZoomFactor(double ZoomFactor)
        {
            Sprites["gfx"].ZoomX = ZoomFactor;
            Sprites["gfx"].ZoomY = ZoomFactor;
            Sprites["box"].ZoomX = ZoomFactor;
            Sprites["box"].ZoomY = ZoomFactor;
            this.ZoomFactor = ZoomFactor;
        }

        public void SetBoxPosition(int X, int Y)
        {
            BoxPosition = new Point(X, Y);
        }

        public void SetBoxSize(int Width, int Height)
        {
            BoxSize = new Size(Width, Height);
        }

        public void Reposition()
        {
            Sprites["box"].Bitmap?.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.BoxSize);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.DrawRect(0, 0, this.BoxSize.Width, this.BoxSize.Height, Color.WHITE);
            Sprites["box"].Bitmap.FillRect(1, 1, this.BoxSize.Width - 2, this.BoxSize.Height - 2, new Color(255, 255, 255, 80));
            Sprites["box"].Bitmap.Lock();
            Sprites["box"].X = BoxPosition.X;
            Sprites["box"].Y = BoxPosition.Y;
            if (Sprites["gfx"].Bitmap != null)
            {
                Sprites["gfx"].X = BoxPosition.X + (int)Math.Round(BoxSize.Width / 2 * ZoomFactor - Sprites["gfx"].SrcRect.Width / 2 * ZoomFactor);
                Sprites["gfx"].Y = BoxPosition.Y + (int)Math.Round(BoxSize.Height * ZoomFactor - Sprites["gfx"].SrcRect.Height * ZoomFactor);
            }
        }
    }
}
