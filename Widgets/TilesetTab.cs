using System;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetTab : Widget
    {
        public MKD.Tileset Tileset { get; protected set; }
        
        Container tilesetcontainer;
        PictureBox tileset;

        public TilesetTab(object Parent, string Name = "tilesetViewer")
            : base(Parent, Name)
        {
            this.Sprites["rect"] = new RectSprite(this.Viewport);
            this.Sprites["rect"].X = 26;
            this.Sprites["rect"].Y = 44;
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 6;
            this.Sprites["text"].Y = 14;
            Font f = Font.Get("Fonts/Quicksand Bold", 16);
            Size s = f.TextSize("Tilesets");
            this.Sprites["text"].Bitmap = new Bitmap(s);
            this.Sprites["text"].Bitmap.Unlock();
            this.Sprites["text"].Bitmap.Font = f;
            this.Sprites["text"].Bitmap.DrawText("Tilesets", 0, 0, Color.WHITE);
            this.Sprites["text"].Bitmap.Lock();

            tilesetcontainer = new Container(this);
            tilesetcontainer.AutoScroll = true;
            tilesetcontainer.SetPosition(30, 48);
            tilesetcontainer.SetSize(272, this.Size.Height - 78);

            tileset = new PictureBox(tilesetcontainer);
        }

        public void SetTileset(MKD.Tileset Tileset)
        {
            this.Tileset = Tileset;
            tileset.Sprite.Bitmap = this.Tileset.ResultBitmap;
        }

        protected override void Draw()
        {
            RectSprite r = Sprites["rect"] as RectSprite;
            r.SetSize(280, this.Size.Height - 70);
            r.SetColor(186, 186, 186, 30, 32, 36);
            tilesetcontainer.SetSize(272, this.Size.Height - 78);
            base.Draw();
        }
    }
}
