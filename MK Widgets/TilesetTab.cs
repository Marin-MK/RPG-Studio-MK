using System;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetTab : Widget
    {
        public MKD.Tileset Tileset { get; protected set; }

        Container alltilesets;
        VStackPanel stack;

        CollapsibleContainer tbox;
        PictureBox tileset;

        public TilesetTab(object Parent, string Name = "tilesetTab")
            : base(Parent, Name)
        {
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

            stack = new VStackPanel(alltilesets);
            stack.SetWidth(this.Size.Width - 37);

            tbox = new CollapsibleContainer(stack);
            tbox.SetText("Outside");
            tbox.SetMargin(0, 0, 0, 8);

            tileset = new PictureBox(tbox);
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

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            alltilesets.SetSize(this.Size.Width - 37, this.Size.Height - 57);
            stack.SetWidth(this.Size.Width - 50);
            tbox.SetSize(alltilesets.Size.Width - 13, tileset.Size.Height + tileset.Position.Y);
        }
    }
}
