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
        Sprites["bg"].X = Sprites["bg"].Y = 15;
        Sprites["gfx"] = new Sprite(this.Viewport);
        Sprites["overlay"] = new Sprite(this.Viewport);
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
        Sprites["gfx"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap?.Dispose();
        if (Graphic is null) return;
        if (!string.IsNullOrEmpty(Graphic.CharacterName))
        {
            string filename = Bitmap.FindRealFilename(Data.ProjectPath + "/Graphics/Characters/" + Graphic.CharacterName);
            if (!string.IsNullOrEmpty(filename))
            {
                Bitmap SourceBitmap = new Bitmap(filename);
                int sw = SourceBitmap.Width / Graphic.NumFrames;
                int sh = SourceBitmap.Height / Graphic.NumDirections;
                int sx = sw * Graphic.Pattern;
                int sy = sh * (Graphic.Direction / 2 - 1);
                Bitmap SmallBitmap = new Bitmap(sw, sh);
                SmallBitmap.Unlock();
                SmallBitmap.Build(0, 0, SourceBitmap, new Rect(sx, sy, sw, sh));
                SmallBitmap.Lock();
                SourceBitmap.Dispose();
                Sprites["gfx"].Bitmap = SmallBitmap;
                if (Graphic.CharacterHue != 0)
                {
                    Sprites["gfx"].Bitmap = SmallBitmap.ApplyHue(Graphic.CharacterHue);
                    SmallBitmap.Dispose();
                }
            }
        }
        else if (Graphic.TileID >= 384)
        {
            Tileset tileset = Data.Tilesets[Map.TilesetIDs[0]];
            if (tileset.TilesetBitmap != null)
            {
                Bitmap SourceBitmap = tileset.TilesetBitmap;
                int tx = (Graphic.TileID - 384) % 8;
                int ty = (Graphic.TileID - 384) / 8;
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
                if (Graphic.CharacterHue != 0)
                {
                    Sprites["gfx"].Bitmap = SmallBitmap.ApplyHue(Graphic.CharacterHue);
                    SmallBitmap.Dispose();
                }
            }
        }
        Sprites["gfx"].X = 15 + (Size.Width - 30) / 2 - Sprites["gfx"].SrcRect.Width / 2;
        Sprites["gfx"].Y = 15 + (Size.Height - 30) / 2 - Sprites["gfx"].SrcRect.Height + Event.Height * 16;
        Sprites["gfx"].Opacity = (byte) Graphic.Opacity;
        Sprites["bg"].Bitmap = new Bitmap(Size.Width - 30, Size.Height - 30);
        Sprites["bg"].Bitmap.Unlock();
        int xoffset = 0;
        int yoffset = 0;
        if (Graphic.TileID >= 384)
        {
            xoffset = (Sprites["gfx"].SrcRect.Width / 32) % 2 == 0 ? 16 : 0;
            yoffset = (Sprites["gfx"].SrcRect.Height / 32) % 2 == 0 ? 16 : 0;
        }
        else
        {
            xoffset = Event.Width % 2 == 0 ? 16 : 0;
            yoffset = Event.Height % 2 == 0 ? 16 : 0;
            Sprites["gfx"].Y = 15 + (Size.Height - 30) / 2 - Sprites["gfx"].SrcRect.Height + 16;
        }
        for (int y = 0; y < (Size.Height - 30); y++)
        {
            for (int x = 0; x < (Size.Width - 30); x++)
            {
                int gx = (x + xoffset) / 32;
                int gy = (y + yoffset) / 32;
                Color c = (gx + gy) % 2 == 0 ? new Color(86, 108, 134) : new Color(24, 38, 53);
                Sprites["bg"].Bitmap.SetPixel(x, y, c);
            }
        }
        Sprites["bg"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["overlay"].Bitmap?.Dispose();
        Sprites["overlay"].Bitmap = new Bitmap(this.Size);
        Sprites["overlay"].Bitmap.Unlock();
        Sprites["overlay"].Bitmap.FillGradientRectOutside(
            new Rect(0, 0, Size.Width, Size.Height),
            new Rect(15, 15, Size.Width - 30, Size.Width - 30),
            new Color(0, 0, 0, 96),
            Color.ALPHA,
            false
        );
        RedrawOverlay(false);
        Sprites["overlay"].Bitmap.Lock();
    }

    void RedrawOverlay(bool Lock = true)
    {
        if (Lock) Sprites["overlay"].Bitmap.Unlock();
        Sprites["overlay"].Bitmap.FillRect(15, 15, Size.Width - 30, Size.Height - 30, Color.ALPHA);
        Color OverlayColor = null;
        if (Mouse.LeftMousePressed && Mouse.LeftStartedInside)
        {
            OverlayColor = Mouse.Inside ? new Color(32, 170, 221) : new Color(255, 255, 255, 160);
        }
        else if (Mouse.Inside)
        {
            OverlayColor = new Color(255, 255, 255, 160);
        }
        else OverlayColor = new Color(86, 108, 134);
        Sprites["overlay"].Bitmap.FillGradientRectOutside(
            new Rect(15, 15, Size.Width - 30, Size.Height - 30),
            new Rect(30, 30, Size.Width - 60, Size.Height - 60),
            new Color(OverlayColor.Red, OverlayColor.Green, OverlayColor.Blue, 0),
            new Color(OverlayColor.Red, OverlayColor.Green, OverlayColor.Blue, OverlayColor.Alpha),
            false
        );
        if (Lock) Sprites["overlay"].Bitmap.Lock();
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        RedrawOverlay();
    }

    public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDown(e);
        if (Mouse.Inside) RedrawOverlay();
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (Mouse.LeftStartedInside) RedrawOverlay();
    }
}