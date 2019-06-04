using System;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetTab : Widget
    {
        public MKD.Tileset Tileset { get; protected set; }

        Container alltilesets;
        VStackPanel stack;

        CollapsibleContainer tbox1;
        PictureBox tileset1;

        CollapsibleContainer tbox2;
        PictureBox tileset2;

        CollapsibleContainer tbox3;
        PictureBox tileset3;

        public TilesetTab(object Parent, string Name = "tilesetViewer")
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

            tbox1 = new CollapsibleContainer(stack);
            tbox1.SetText("Outside");
            tbox1.SetMargin(0, 0, 0, 8);

            tileset1 = new PictureBox(tbox1);
            tileset1.SetPosition(20, 33);

            tbox2 = new CollapsibleContainer(stack);
            tbox2.SetText("Second Outside");
            tbox2.SetMargin(0, 0, 0, 8);

            tileset2 = new PictureBox(tbox2);
            tileset2.SetPosition(20, 33);

            tbox3 = new CollapsibleContainer(stack);
            tbox3.SetText("Third Outside");
            tbox3.SetMargin(0, 0, 0, 8);

            tileset3 = new PictureBox(tbox3);
            tileset3.SetPosition(20, 33);
        }

        public void SetTileset(MKD.Tileset Tileset)
        {
            this.Tileset = Tileset;
            tileset1.Sprite.Bitmap = this.Tileset.ResultBitmap;
            tileset1.SetSize(tileset1.Sprite.Bitmap.Width, tileset1.Sprite.Bitmap.Height);
            tileset2.Sprite.Bitmap = this.Tileset.ResultBitmap;
            tileset2.SetSize(tileset2.Sprite.Bitmap.Width, tileset2.Sprite.Bitmap.Height);
            tileset3.Sprite.Bitmap = this.Tileset.ResultBitmap;
            tileset3.SetSize(tileset3.Sprite.Bitmap.Width, tileset3.Sprite.Bitmap.Height);
        }

        public override void Update()
        {
            base.Update();
        }

        protected override void Draw()
        {
            alltilesets.SetSize(this.Size.Width - 37, this.Size.Height - 57);
            stack.SetWidth(this.Size.Width - 50);
            tbox1.SetSize(alltilesets.Size.Width - 13, tileset1.Size.Height + tileset1.Position.Y);
            tbox2.SetSize(alltilesets.Size.Width - 13, tileset2.Size.Height + tileset2.Position.Y);
            tbox3.SetSize(alltilesets.Size.Width - 13, tileset3.Size.Height + tileset3.Position.Y);
            base.Draw();
        }
    }
}
