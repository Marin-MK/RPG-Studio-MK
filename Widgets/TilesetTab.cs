using System;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetTab : Widget
    {
        public MKD.Tileset Tileset { get; protected set; }

        Container alltilesets;
        CollapsibleContainer tbox;
        PictureBox tileset;
        double decr = 0;

        public TilesetTab(object Parent, string Name = "tilesetViewer")
            : base(Parent, Name)
        {
            //this.Sprites["rect"] = new RectSprite(this.Viewport);
            //this.Sprites["rect"].X = 26;
            //this.Sprites["rect"].Y = 44;
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

            alltilesets = new Container(this);
            alltilesets.SetPosition(10, 47);
            alltilesets.AutoScroll = true;

            tbox = new CollapsibleContainer(alltilesets, "ccontainer");
            tbox.SetText("Outside");

            tileset = new PictureBox(tbox, "pbox");
            tileset.SetPosition(20, 33);
        }

        public void SetTileset(MKD.Tileset Tileset)
        {
            this.Tileset = Tileset;
            tileset.Sprite.Bitmap = this.Tileset.ResultBitmap;
            tileset.SetSize(tileset.Sprite.Bitmap.Width, tileset.Sprite.Bitmap.Height);
        }

        public override void Update()
        {
            base.Update();
        }

        protected override void Draw()
        {
            //RectSprite r = Sprites["rect"] as RectSprite;
            //r.SetSize(280, this.Size.Height - 70);
            //r.SetColor(186, 186, 186, 30, 32, 36);
            //tilesetcontainer.SetSize(273, this.Size.Height - 78);
            alltilesets.SetSize(this.Size.Width - 37, this.Size.Height - 57);
            tbox.SetSize(alltilesets.Size.Width - 13, tileset.Size.Height + tileset.Position.Y);
            base.Draw();
        }
    }
}
