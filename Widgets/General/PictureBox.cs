using System;
using ODL;

namespace MKEditor.Widgets
{
    public class PictureBox : Widget
    {
        public Sprite Sprite { get { return Sprites["sprite"] as Sprite; } }

        public PictureBox(object Parent, string Name = "pictureBox")
            : base(Parent, Name)
        {
            this.Sprites["sprite"] = new Sprite(this.Viewport);
        }

        public override void Update()
        {
            if (this.Sprite.Bitmap != null)
            {
                if (this.Sprite.Bitmap.Width != this.Size.Width || this.Sprite.Bitmap.Height != this.Size.Height)
                {
                    this.SetSize(this.Sprite.Bitmap.Width, this.Sprite.Bitmap.Height);
                }
            }
            base.Update();
        }
    }
}
