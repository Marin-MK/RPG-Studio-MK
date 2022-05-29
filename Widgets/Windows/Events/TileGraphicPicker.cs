using System;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class TileGraphicPicker : Widget
{
    Size SelectionSize;
    CursorWidget Cursor;
    PictureBox TilesetBox;

    public int TileID = 0;

    public BaseEvent OnTileSelected;
    public BaseEvent OnTileDoubleClicked;

    public TileGraphicPicker(Size SelectionSize, Map Map, EventGraphic Graphic, IContainer Parent) : base(Parent)
    {
        this.SelectionSize = SelectionSize;

        GroupBox GroupBox = new GroupBox(this);
        GroupBox.SetDocked(true);

        Widget ScrollBar = new Widget(this);
        ScrollBar.SetWidth(3);
        ScrollBar.SetVDocked(true);
        ScrollBar.SetRightDocked(true);
        ScrollBar.SetMargins(0, 1, 12, 1);
        ScrollBar.Sprites["line"] = new Sprite(ScrollBar.Viewport);
        ScrollBar.OnSizeChanged += _ =>
        {
            ScrollBar.Sprites["line"].Bitmap?.Dispose();
            ScrollBar.Sprites["line"].Bitmap = new Bitmap(3, ScrollBar.Size.Height);
            ScrollBar.Sprites["line"].Bitmap.Unlock();
            ScrollBar.Sprites["line"].Bitmap.DrawLine(0, 0, 0, ScrollBar.Size.Height - 1, 17, 27, 38);
            ScrollBar.Sprites["line"].Bitmap.DrawLine(1, 0, 1, ScrollBar.Size.Height - 1, 59, 91, 124);
            ScrollBar.Sprites["line"].Bitmap.DrawLine(2, 0, 2, ScrollBar.Size.Height - 1, 17, 27, 38);
            ScrollBar.Sprites["line"].Bitmap.Lock();
        };

        Container ScrollContainer = new Container(GroupBox);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetMargins(2);

        TilesetBox = new PictureBox(ScrollContainer);
        Tileset Tileset = Data.Tilesets[Map.TilesetIDs[0]];
        if (Tileset.TilesetBitmap != null)
        {
            TilesetBox.Sprite.Bitmap = Tileset.TilesetBitmap;
            TilesetBox.Sprite.DestroyBitmap = false;
        }
        TilesetBox.OnDoubleLeftMouseDownInside += _ => OnTileDoubleClicked?.Invoke(new BaseEventArgs());

        Cursor = new CursorWidget(ScrollContainer);
        Cursor.ConsiderInAutoScrollCalculation = false;
        Cursor.SetSize(SelectionSize.Width * 32 + 14, SelectionSize.Height * 32 + 14);
        Cursor.SetVisible(Graphic.TileID >= 384);
        TileID = Graphic.TileID;
        if (Graphic.TileID >= 384)
        {
            int x = (Graphic.TileID - 384) % 8;
            int y = (Graphic.TileID - 384) / 8;
            Cursor.SetPosition(TilesetBox.Position.X - 7 + x * 32, TilesetBox.Position.Y - 7 + y * 32 - (SelectionSize.Height - 1) * 32);
        }

        VScrollBar vs = new VScrollBar(GroupBox);
        vs.SetVDocked(true);
        vs.SetMargins(0, 3, 1, 3);
        vs.SetRightDocked(true);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        ScrollContainer.OnSizeChanged += _ =>
        {
            int xadd = Cursor.Position.X - TilesetBox.Position.X;
            int yadd = Cursor.Position.Y - TilesetBox.Position.Y;
            TilesetBox.SetPosition(ScrollContainer.Size.Width / 2 - TilesetBox.Sprite.Bitmap.Width / 2, 0);
            Cursor.SetPosition(TilesetBox.Position.X + xadd, TilesetBox.Position.Y + yadd);
        };
        TilesetBox.OnMouseDown += MouseDownInsideTilesetBox;
    }

    public void HideCursor()
    {
        Cursor.SetVisible(false);
    }

    private void MouseDownInsideTilesetBox(MouseEventArgs e)
    {
        if (TilesetBox.Mouse.Inside && (TilesetBox.Mouse.LeftMouseTriggered && !TilesetBox.Mouse.RightMousePressed ||
            TilesetBox.Mouse.RightMouseTriggered && !TilesetBox.Mouse.LeftMousePressed))
        {
            int rx = e.X - TilesetBox.Viewport.X + TilesetBox.LeftCutOff;
            int ry = e.Y - TilesetBox.Viewport.Y + TilesetBox.TopCutOff;
            int tx = (int) Math.Floor(rx / 32d);
            int ty = (int) Math.Floor(ry / 32d);
            SelectTile(tx, ty);
        }
    }

    public void SelectTile(int X, int Y)
    {
        TileID = 384 + X + Y * 8;
        Cursor.SetPosition(TilesetBox.Position.X - 7 + X * 32, TilesetBox.Position.Y - 7 + Y * 32 - (SelectionSize.Height - 1) * 32);
        Cursor.SetVisible(true);
        OnTileSelected?.Invoke(new BaseEventArgs());
    }
}
