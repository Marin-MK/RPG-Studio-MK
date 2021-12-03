﻿namespace RPGStudioMK.Widgets;

public class PictureBox : Widget
{
    public Sprite Sprite { get { return Sprites["sprite"] as Sprite; } }
    public bool ResizeBox = true;

    public PictureBox(IContainer Parent) : base(Parent)
    {
        this.Sprites["sprite"] = new Sprite(this.Viewport);
    }

    public override void Update()
    {
        if (this.ResizeBox)
        {
            if (this.Sprite.Bitmap != null && !this.Sprite.Bitmap.Disposed)
            {
                if (this.Sprite.SrcRect.Width != this.Size.Width || this.Sprite.SrcRect.Height != this.Size.Height)
                {
                    this.SetSize(this.Sprite.SrcRect.Width, this.Sprite.SrcRect.Height);
                }
            }
            else
            {
                this.SetSize(1, 1);
            }
        }
        base.Update();
    }
}
