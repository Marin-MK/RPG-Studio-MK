using RPGStudioMK.Game;
using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class AutotilePickerMap : PopupWindow
{
    public Autotile Autotile { get; protected set; }
    public int SelectedTileID { get; protected set; } = 0;

    CursorWidget Cursor;

    public AutotilePickerMap()
    {
        SetTitle("Individual Tile Combinations");
        MinimumSize = MaximumSize = new Size(313, 285);
        SetSize(MaximumSize);
        Center();

        ColoredBox box1 = new ColoredBox(this);
        box1.SetOuterColor(59, 91, 124);
        box1.SetSize(278, 210);
        box1.SetPosition(19, 34);

        ColoredBox box2 = new ColoredBox(this);
        box2.SetOuterColor(17, 27, 38);
        box2.SetInnerColor(24, 38, 53);
        box2.SetSize(276, 208);
        box2.SetPosition(20, 35);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        Sprites["tiles"] = new Sprite(this.Viewport);
        Sprites["tiles"].X = 23;
        Sprites["tiles"].Y = 38;

        Cursor = new CursorWidget(this);
        Cursor.SetPosition(Sprites["tiles"].X - 7, Sprites["tiles"].Y - 7);
        Cursor.SetSize(32 + 14, 32 + 14);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    private void Cancel()
    {
        SelectedTileID = -1;
        Close();
    }

    private void OK()
    {
        Close();
    }

    public void SetAutotile(Autotile Autotile)
    {
        if (this.Autotile != Autotile)
        {
            this.Autotile = Autotile;
            Redraw();
        }
    }

    protected override void Draw()
    {
        if (Sprites["tiles"].Bitmap != null) Sprites["tiles"].Bitmap.Dispose();
        Sprites["tiles"].Bitmap = new Bitmap(270, 202);
        Sprites["tiles"].Bitmap.Unlock();
        for (int i = 0; i < 48; i++)
        {
            int X = i % 8;
            int Y = (int)Math.Floor(i / 8d);
            List<int> Tiles = Autotile.AutotileCombinations[Autotile.Format][i];
            for (int j = 0; j < 4; j++)
            {
                Sprites["tiles"].Bitmap.Build(
                    X * 34 + 16 * (j % 2),
                    Y * 34 + 16 * (int)Math.Floor(j / 2d),
                    Autotile.AutotileBitmap,
                    new Rect(
                        16 * (Tiles[j] % 6),
                        16 * (int)Math.Floor(Tiles[j] / 6d),
                        16,
                        16
                    )
                );
            }
        }
        Sprites["tiles"].Bitmap.Lock();
        base.Draw();
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (rx < Sprites["tiles"].X || ry < Sprites["tiles"].Y || rx >= Sprites["tiles"].X + Sprites["tiles"].Bitmap.Width ||
            ry >= Sprites["tiles"].Y + Sprites["tiles"].Bitmap.Height) return;
        rx -= Sprites["tiles"].X;
        ry -= Sprites["tiles"].Y;
        int TileX = (int) Math.Floor(rx / 34d);
        int TileY = (int) Math.Floor(ry / 34d);
        Cursor.SetPosition(Sprites["tiles"].X + TileX * 34 - 7, Sprites["tiles"].Y + TileY * 34 - 7);
        SelectedTileID = TileX + TileY * 8;
    }
}
