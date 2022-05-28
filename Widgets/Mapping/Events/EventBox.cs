using System;
using System.Collections.Generic;
using amethyst;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventBox : Widget
{
    public Event Event;

    MapImageWidget MapWidget;

    public EventBox(IContainer Parent) : base(Parent)
    {
        Sprites["gfx"] = new Sprite(this.Viewport);
        Sprites["box"] = new Sprite(this.Viewport);
    }

    public void RepositionSprites(MapImageWidget MapWidget)
    {
        this.MapWidget = MapWidget;
        int tx = MapWidget.Position.X + (int) Math.Round(this.Event.X * 32 * MapWidget.ZoomFactor);
        int ty = MapWidget.Position.Y + (int) Math.Round(this.Event.Y * 32 * MapWidget.ZoomFactor);
        ty -= (int) Math.Round(32 * (Event.Height - 1) * MapWidget.ZoomFactor);
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap((int) Math.Round(Event.Width * 32 * MapWidget.ZoomFactor), (int) Math.Round(Event.Height * 32 * MapWidget.ZoomFactor));
        Sprites["box"].Bitmap.Unlock();
        int pad = 2;
        Sprites["box"].Bitmap.DrawRect(pad, pad, Sprites["box"].Bitmap.Width - 2 * pad, Sprites["box"].Bitmap.Height - 2 * pad, new Color(255, 255, 255));
        Sprites["box"].Bitmap.FillRect(pad + 1, pad + 1, Sprites["box"].Bitmap.Width - 2 * pad - 2, Sprites["box"].Bitmap.Height - 2 * pad - 2, new Color(255, 255, 255, 96));
        Sprites["box"].Bitmap.Lock();
        Sprites["box"].X = tx;
        Sprites["box"].Y = ty;
        Sprites["gfx"].X = tx + (int) Math.Round(this.Event.Width * MapWidget.ZoomFactor / 2d);
        Sprites["gfx"].Y = ty + (int) Math.Round((32 - Sprites["gfx"].SrcRect.Height) * MapWidget.ZoomFactor);
        Sprites["gfx"].ZoomX = Sprites["gfx"].ZoomY = MapWidget.ZoomFactor;
    }

    public void SetEvent(Event Event)
    {
        this.Event = Event;
        Sprites["gfx"].Bitmap?.Dispose();
        for (int i = 0; i < Event.Pages.Count; i++)
        {
            EventGraphic gfx = Event.Pages[i].Graphic;
            if (!string.IsNullOrEmpty(gfx.CharacterName))
            {
                string filename = Bitmap.FindRealFilename(Data.ProjectPath + "/Graphics/Characters/" + gfx.CharacterName);
                if (!string.IsNullOrEmpty(filename))
                {
                    Sprites["gfx"].Bitmap = new Bitmap(filename);
                    Sprites["gfx"].SrcRect.Width = Sprites["gfx"].Bitmap.Width / gfx.NumFrames;
                    Sprites["gfx"].SrcRect.Height = Sprites["gfx"].Bitmap.Height / gfx.NumDirections;
                    Sprites["gfx"].SrcRect.Y = Sprites["gfx"].SrcRect.Height * (gfx.Direction / 2 - 1);
                    break;
                }
            }
        }
    }
}
