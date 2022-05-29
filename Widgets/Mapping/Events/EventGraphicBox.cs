using System;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventGraphicBox : Widget
{
    public Map Map;
    public Event Event;
    public EventGraphic Graphic;

    public EventGraphicBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["gfx"] = new Sprite(this.Viewport);
        OnSizeChanged += _ => RedrawGraphic();
    }

    public void SetGraphic(Map Map, Event Event, EventGraphic Graphic)
    {
        this.Map = Map;
        this.Event = Event;
        this.Graphic = Graphic;
        RedrawGraphic();
    }

    public void RedrawGraphic()
    {
        if (Sprites["gfx"].DestroyBitmap) Sprites["gfx"].Bitmap?.Dispose();
        else Sprites["gfx"].Bitmap = null;
        Sprites["bg"].Bitmap?.Dispose();
        if (Graphic is null) return;
        if (!string.IsNullOrEmpty(Graphic.CharacterName))
        {
            string filename = Bitmap.FindRealFilename(Data.ProjectPath + "/Graphics/Characters/" + Graphic.CharacterName);
            if (!string.IsNullOrEmpty(filename))
            {
                Sprites["gfx"].Bitmap = new Bitmap(filename);
                Sprites["gfx"].SrcRect.Width = Sprites["gfx"].Bitmap.Width / Graphic.NumFrames;
                Sprites["gfx"].SrcRect.Height = Sprites["gfx"].Bitmap.Height / Graphic.NumDirections;
                Sprites["gfx"].SrcRect.Y = Sprites["gfx"].SrcRect.Height * (Graphic.Direction / 2 - 1);
                Sprites["gfx"].DestroyBitmap = true;
            }
        }
        else if (Graphic.TileID >= 384)
        {
            Tileset tileset = Data.Tilesets[Map.TilesetIDs[0]];
            if (tileset.TilesetBitmap != null)
            {
                Sprites["gfx"].Bitmap = tileset.TilesetBitmap;
                Sprites["gfx"].DestroyBitmap = false;
                int tx = (Graphic.TileID - 384) % 8;
                int ty = (Graphic.TileID - 384) / 8;
                Sprites["gfx"].SrcRect.X = tx * 32;
                Sprites["gfx"].SrcRect.Y = ty * 32 - (Event.Height - 1) * 32;
                Sprites["gfx"].SrcRect.Width = Event.Width * 32;
                Sprites["gfx"].SrcRect.Height = Event.Height * 32;
                if (Sprites["gfx"].SrcRect.X + Sprites["gfx"].SrcRect.Width >= tileset.TilesetBitmap.Width)
                    Sprites["gfx"].SrcRect.Width = tileset.TilesetBitmap.Width - Sprites["gfx"].SrcRect.X;
                if (Sprites["gfx"].SrcRect.Y < 0)
                {
                    Sprites["gfx"].SrcRect.Height += Sprites["gfx"].SrcRect.Y;
                    Sprites["gfx"].SrcRect.Y = 0;
                }
            }
        }
        Sprites["gfx"].X = Size.Width / 2 - Sprites["gfx"].SrcRect.Width / 2;
        Sprites["gfx"].Y = Size.Height / 2 - Sprites["gfx"].SrcRect.Height + Event.Height * 16;
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        int xoffset = 0;
        int yoffset = 0;
        if (Graphic.TileID >= 384)
        {
            xoffset = (Sprites["gfx"].SrcRect.Width / 32) % 2 == 0 ? 6 : 22;
            yoffset = (Sprites["gfx"].SrcRect.Height / 32) % 2 == 0 ? 6 : 22;
        }
        else
        {
            xoffset = Event.Width % 2 == 0 ? 6 : 22;
            yoffset = 22;
            Sprites["gfx"].Y = Size.Height / 2 - Sprites["gfx"].SrcRect.Height + 16;
        }
        for (int y = 0; y < Size.Height; y++)
        {
            for (int x = 0; x < Size.Width; x++)
            {
                int gx = (x + xoffset) / 32;
                int gy = (y + yoffset) / 32;
                Color c = (gx + gy) % 2 == 0 ? new Color(246, 246, 246) : new Color(206, 206, 206);
                Sprites["bg"].Bitmap.SetPixel(x, y, c);
            }
        }
        Sprites["bg"].Bitmap.Lock();
    }
}
