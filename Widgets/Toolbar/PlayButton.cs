using System;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class PlayButton : Widget
    {
        public PlayButton(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport, new Bitmap(70, 28));
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.FillRect(0, 0, 70, 28, new Color(87, 168, 127));
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(69, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, 27, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(69, 27, Color.ALPHA);
            Sprites["bg"].Bitmap.Lock();

            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].Y = 5;

            //Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(106, 2, new Color(59, 227, 255)));
            //Sprites["selector"].Visible = false;
            //Sprites["selector"].Y = 50;

            Sprites["icon"] = new Sprite(this.Viewport);
            Sprites["icon"].Bitmap = Utilities.IconSheet;
            Sprites["icon"].SrcRect = new Rect(21 * 24, 0, 24, 24);
            Sprites["icon"].X = 3;
            Sprites["icon"].Y = 2;

            OnWidgetSelected += WidgetSelected;

            SetSize(70, 28);
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Font f = Font.Get("Fonts/Ubuntu-B", 16);
            Size s = f.TextSize("Play");
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText("Play", Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = Size.Width / 2 - 5;
            
            base.Draw();
        }

        public void UpdateSelector(MouseEventArgs e)
        {
            int ry = e.Y - Viewport.Y;
            //Sprites["selector"].Visible = WidgetIM.Hovering && ry < 42;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering)
            {
                Editor.StartGame();
            }
        }
    }
}
