using System;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetDisplay : Widget
    {
        Container MainContainer;
        public PictureBox TilesetBox;
        VScrollBar ScrollBar;

        public BaseEvent OnTilesetLoaded;
        public MouseEvent OnTileClicked;

        public TilesetDisplay(IContainer Parent) : base(Parent)
        {
            SetBackgroundColor(9, 21, 34);
            MainContainer = new Container(this);
            MainContainer.SetPosition(2, 2);
            MainContainer.VAutoScroll = true;

            ScrollBar = new VScrollBar(this);
            ScrollBar.SetPosition(267, 2);
            MainContainer.SetVScrollBar(ScrollBar);

            TilesetBox = new PictureBox(MainContainer);
            TilesetBox.SetSize(256, 200);

            TilesetBox.Sprites["controls"] = new Sprite(TilesetBox.Viewport);

            SetSize(277, 200);

            WidgetIM.OnMouseDown += MouseDown;
        }

        public void SetTileset(Game.Tileset Tileset)
        {
            if (TilesetBox.Sprites["controls"].Bitmap != null) TilesetBox.Sprites["controls"].Bitmap.Dispose();
            if (Tileset == null)
            {
                TilesetBox.Sprite.Bitmap = null;
                return;
            }
            TilesetBox.Sprite.Bitmap = Tileset.TilesetListBitmap;
            TilesetBox.Sprite.DestroyBitmap = false;
            TilesetBox.Sprites["controls"].Bitmap = new Bitmap(Tileset.TilesetListBitmap.Width, Tileset.TilesetListBitmap.Height);
            TilesetBox.SetSize(Tileset.TilesetListBitmap.Width, Tileset.TilesetListBitmap.Height);
            if (MainContainer.Size.Height - 4 > TilesetBox.Size.Height)
            {
                this.SetSize(267, TilesetBox.Size.Height + 4);
            }
            this.OnTilesetLoaded?.Invoke(new BaseEventArgs());
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            MainContainer.SetSize(this.Size.Width, Size.Height - 4);
            ScrollBar.SetHeight(Size.Height - 4);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (!this.WidgetIM.Hovering) return;
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx < 2 || rx > 264 || ry < 2 || ry > Size.Height - 3) return;
            rx -= 2;
            ry -= 2;
            ry += TilesetBox.Position.Y - TilesetBox.ScrolledPosition.Y;
            bool left = e.LeftButton != e.OldRightButton && e.LeftButton;
            bool right = e.RightButton != e.OldRightButton && e.RightButton;
            bool middle = e.MiddleButton != e.OldMiddleButton && e.MiddleButton;
            this.OnTileClicked?.Invoke(new MouseEventArgs(rx, ry, left, right, middle));
        }
    }
}
