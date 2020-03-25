using MKEditor.Game;
using System;
using ODL;
using System.Collections.Generic;

namespace MKEditor.Widgets
{
    public class AutotilePickerMap : PopupWindow
    {
        public Autotile Autotile { get; protected set; }
        public int SelectedTileID { get; protected set; } = 0;

        CursorWidget Cursor;

        public AutotilePickerMap(object Parent, string Name = "autotileTilePicker")
            : base(Parent, Name)
        {
            SetTitle("Individual Tile Combinations");
            SetSize(313, 285);
            Center();

            RectSprite bg1 = new RectSprite(this.Viewport);
            bg1.SetOuterColor(59, 91, 124);
            bg1.SetSize(278, 210);
            bg1.X = 19;
            bg1.Y = 34;
            Sprites["bg1"] = bg1;
            RectSprite bg2 = new RectSprite(this.Viewport);
            bg2.SetSize(276, 208);
            bg2.X = 20;
            bg2.Y = 35;
            bg2.SetOuterColor(17, 27, 38);
            bg2.SetInnerColor(24, 38, 53);
            Sprites["bg2"] = bg2;

            CreateButton("Cancel", delegate (object sender, EventArgs e)
            {
                SelectedTileID = -1;
                Close();
            });

            CreateButton("OK", delegate (object sender, EventArgs e)
            {
                Close();
            });

            Sprites["tiles"] = new Sprite(this.Viewport);
            Sprites["tiles"].X = 23;
            Sprites["tiles"].Y = 38;

            Cursor = new CursorWidget(this);
            Cursor.SetPosition(Sprites["tiles"].X - 7, Sprites["tiles"].Y - 7);
            Cursor.SetSize(32 + 14, 32 + 14);

            this.WidgetIM.OnMouseDown += MouseDown;
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
                int Y = (int) Math.Floor(i / 8d);
                List<int> Tiles = Autotile.AutotileCombinations[Autotile.Format][i];
                for (int j = 0; j < 4; j++)
                {
                    Sprites["tiles"].Bitmap.Build(
                        X * 34 + 16 * (j % 2),
                        Y * 34 + 16 * (int) Math.Floor(j / 2d),
                        Autotile.AutotileBitmap,
                        new Rect(
                            16 * (Tiles[j] % 6),
                            16 * (int) Math.Floor(Tiles[j] / 6d),
                            16,
                            16
                        )
                    );
                }
            }
            Sprites["tiles"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (WidgetIM.Hovering)
            {
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
    }
}
