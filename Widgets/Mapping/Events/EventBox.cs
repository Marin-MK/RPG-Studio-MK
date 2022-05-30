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

    public void RepositionSprites(MapImageWidget MapWidget, int EventX, int EventY)
    {
        this.MapWidget = MapWidget;
        int tx = MapWidget.Position.X + (int) Math.Round(EventX * 32 * MapWidget.ZoomFactor);
        int ty = MapWidget.Position.Y + (int) Math.Round(EventY * 32 * MapWidget.ZoomFactor);
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
        if (Sprites["gfx"].DestroyBitmap) // Image
            Sprites["gfx"].X = tx + (int) Math.Round((this.Event.Width * 32 - Sprites["gfx"].SrcRect.Width) * MapWidget.ZoomFactor / 2d);
        else // Tile
            Sprites["gfx"].X = tx;
        Sprites["gfx"].Y = ty + (int) Math.Round((this.Event.Height * 32 - Sprites["gfx"].SrcRect.Height) * MapWidget.ZoomFactor);
        Sprites["gfx"].ZoomX = Sprites["gfx"].ZoomY = MapWidget.ZoomFactor;
    }

    public void SetEvent(Map Map, Event Event)
    {
        this.Event = Event;
        if (Sprites["gfx"].DestroyBitmap) Sprites["gfx"].Bitmap?.Dispose();
        else Sprites["gfx"].Bitmap = null;
        for (int i = 0; i < Event.Pages.Count; i++)
        {
            EventGraphic gfx = Event.Pages[i].Graphic;
            if (!string.IsNullOrEmpty(gfx.CharacterName))
            {
                string filename = Bitmap.FindRealFilename(Data.ProjectPath + "/Graphics/Characters/" + gfx.CharacterName);
                if (!string.IsNullOrEmpty(filename))
                {
                    Bitmap SourceBitmap = new Bitmap(filename);
                    int sw = SourceBitmap.Width / gfx.NumFrames;
                    int sh = SourceBitmap.Height / gfx.NumDirections;
                    int sx = sw * gfx.Pattern;
                    int sy = sh * (gfx.Direction / 2 - 1);
                    Bitmap SmallBitmap = new Bitmap(sw, sh);
                    SmallBitmap.Unlock();
                    SmallBitmap.Build(0, 0, SourceBitmap, new Rect(sx, sy, sw, sh));
                    SmallBitmap.Lock();
                    SourceBitmap.Dispose();
                    Sprites["gfx"].Bitmap = SmallBitmap;
                    if (gfx.CharacterHue != 0)
                    {
                        Sprites["gfx"].Bitmap = SmallBitmap.ApplyHue(gfx.CharacterHue);
                        SmallBitmap.Dispose();
                    }
                    Sprites["gfx"].Opacity = (byte) gfx.Opacity;
                    break;
                }
            }
            else if (gfx.TileID >= 384)
            {
                Tileset tileset = Data.Tilesets[Map.TilesetIDs[0]];
                if (tileset.TilesetBitmap != null)
                {
                    Bitmap SourceBitmap = tileset.TilesetBitmap;
                    int tx = (gfx.TileID - 384) % 8;
                    int ty = (gfx.TileID - 384) / 8;
                    int sx = tx * 32;
                    int sy = ty * 32 - (Event.Height - 1) * 32;
                    int sw = Event.Width * 32;
                    int sh = Event.Height * 32;
                    if (sx + sw >= SourceBitmap.Width)
                        sw = SourceBitmap.Width - sx;
                    if (sy < 0)
                    {
                        sh += sy;
                        sy = 0;
                    }
                    Bitmap SmallBitmap = new Bitmap(sw, sh);
                    SmallBitmap.Unlock();
                    SmallBitmap.Build(0, 0, SourceBitmap, new Rect(sx, sy, sw, sh));
                    SmallBitmap.Lock();
                    Sprites["gfx"].Bitmap = SmallBitmap;
                    if (gfx.CharacterHue != 0)
                    {
                        Sprites["gfx"].Bitmap = SmallBitmap.ApplyHue(gfx.CharacterHue);
                        SmallBitmap.Dispose();
                    }
                    Sprites["gfx"].Opacity = (byte) gfx.Opacity;
                    break;
                }
            }
        }
    }
}
