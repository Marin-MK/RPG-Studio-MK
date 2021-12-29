using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class TilesetDisplayContainer : Widget
{
    public Game.Tileset Tileset { get; protected set; }
    public TilesetDisplayMode Mode { get; protected set; }

    public TilesetDisplayContainer(IContainer Parent) : base(Parent)
    {
        Sprites["horizontal"] = new Sprite(this.Viewport, new SolidBitmap(263, 1, new Color(121, 121, 122)));
        Sprites["vertical"] = new Sprite(this.Viewport);
        Sprites["tileset"] = new Sprite(this.Viewport);
        Sprites["tileset"].Y = 33;
        Sprites["autotiles"] = new Sprite(this.Viewport);
        Sprites["impassable"] = new Sprite(this.Viewport);
    }

    public void SetTileset(Game.Tileset Tileset)
    {
        if (this.Tileset != Tileset)
        {
            this.Tileset = Tileset;
            if (this.Tileset != null && Tileset.TilesetListBitmap != null)
            {
                Sprites["tileset"].Bitmap = Tileset.TilesetListBitmap;
                Sprites["tileset"].DestroyBitmap = false;
                int tilecount = (int) Math.Ceiling(Tileset.TilesetBitmap.Height / 32d);
                for (int i = 1; i < tilecount; i++)
                {
                    Sprites["horizontal"].MultiplePositions.Add(new Point(0, 33 * i - 1));
                }
                Sprites["vertical"].Bitmap?.Dispose();
                Sprites["vertical"].Bitmap = new SolidBitmap(1, Tileset.TilesetListBitmap.Height, new Color(121, 121, 122));
                for (int i = 1; i < 8; i++)
                {
                    Sprites["vertical"].MultiplePositions.Add(new Point(i * 33 - 1, 0));
                }
                Sprites["autotiles"].Bitmap?.Dispose();
                Sprites["autotiles"].Bitmap = new Bitmap(263, 32);
                Sprites["autotiles"].Bitmap.Unlock();
                for (int i = 0; i < Tileset.Autotiles.Count; i++)
                {
                    if (Tileset.Autotiles[i] == null || Tileset.Autotiles[i].AutotileBitmap == null) continue;
                    Sprites["autotiles"].Bitmap.Build(
                        (i + 1) * 33, 0,
                        Tileset.Autotiles[i].AutotileBitmap,
                        new Rect(0, 0, 32, 32)
                    );
                }
                Sprites["autotiles"].Bitmap.Lock();
                SetHeight(Tileset.TilesetListBitmap.Height);
            }
            else SetHeight(32);
            SetMode(Mode);
        }
    }

    public void SetMode(TilesetDisplayMode Mode)
    {
        Sprites["impassable"].Bitmap?.Dispose();
        Sprites["impassable"].Bitmap = null;
        this.Mode = Mode;
        switch (Mode)
        {
            case TilesetDisplayMode.Passage:
                LoadPassabilities();
                break;
        }
    }

    void LoadPassabilities()
    {
        if (Tileset == null)
        {
            Sprites["impassable"].Bitmap?.Dispose();
            return;
        }
        if (Sprites["impassable"].Bitmap == null)
            Sprites["impassable"].Bitmap = new Bitmap("assets/img/database_tileset_impassable");
        int xoff = (32 - Sprites["impassable"].Bitmap.Width) / 2;
        int yoff = (32 - Sprites["impassable"].Bitmap.Width) / 2;
        Sprites["impassable"].MultiplePositions.Clear();
        for (int i = 384; i < Tileset.Passabilities.Count; i++)
        {
            Game.Passability tile = Tileset.Passabilities[i];
            if (Tileset.Passabilities[i] != Game.Passability.All)
            {
                int x = (i - 384) % 8;
                int y = (int) Math.Floor((i - 384) / 8d);
                Sprites["impassable"].MultiplePositions.Add(new Point(xoff + x * 33, yoff + (y + 1) * 33));
            }
        }
        for (int i = 0; i < 8; i++)
        {
            if (Tileset.Passabilities[i * 48] != Game.Passability.All)
            {
                Sprites["impassable"].MultiplePositions.Add(new Point(xoff + i * 33, yoff));
            }
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton && e.LeftButton)
        {
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            int tilex = (int) Math.Floor(rx / 33d);
            int tiley = (int) Math.Floor(ry / 33d);
            int idx = -1;
            if (tiley == 0)
            {
                idx = tilex * 48;
            }
            else
            { 
                idx = 384 + tilex + (tiley - 1) * 8;
            }
            Game.Passability OldPassability = Tileset.Passabilities[idx];
            Game.Passability passability = OldPassability == Game.Passability.All ? Game.Passability.None : Game.Passability.All;
            SetTilePassability(idx, tilex, tiley, passability);
            TilePassabilityChangeUndoAction.Create(Tileset, idx, tilex, tiley, OldPassability, Tileset.Passabilities[idx]);
        }
    }

    public void SetTilePassability(int TileIndex, int TileX, int TileY, Game.Passability Passability)
    {
        Tileset.Passabilities[TileIndex] = Passability;
        if (Sprites["impassable"].Bitmap != null)
        {
            int xoff = (32 - Sprites["impassable"].Bitmap.Width) / 2;
            int yoff = (32 - Sprites["impassable"].Bitmap.Width) / 2;
            Sprites["impassable"].MultiplePositions.RemoveAll(p =>
            {
                return p.X == TileX * 33 + xoff && p.Y == TileY * 33 + yoff;
            });
            if (Passability != Game.Passability.All)
            {
                Sprites["impassable"].MultiplePositions.Add(new Point(TileX * 33 + xoff, TileY * 33 + yoff));
            }
            // Ensure the renderer marks the change and forces a redraw
            Sprites["impassable"].Update();
        }
    }
}

public enum TilesetDisplayMode
{
    Passage,
    Directions,
    Priority,
    BushFlag,
    CounterFlag,
    TerrainTag
}