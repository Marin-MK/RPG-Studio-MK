using System;
using ODL;

namespace MKEditor.Widgets
{
    public class CursorWidget : Widget
    {
        public CursorWidget(object Parent, string Name = "cursorWidget")
            : base(Parent, Name)
        {
            RectSprite outer = new RectSprite(this.Viewport);
            outer.SetOuterColor(Color.BLACK);
            Sprites["outer"] = outer;

            RectSprite middle = new RectSprite(this.Viewport);
            middle.SetOuterColor(Color.WHITE);
            Sprites["middle"] = middle;

            RectSprite inner = new RectSprite(this.Viewport);
            inner.SetOuterColor(Color.BLACK);
            Sprites["inner"] = inner;

            this.SetSize(32, 32);

            middle.X = middle.Y = 1;
            inner.X = inner.Y = 2;

            this.SetZIndex(1);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (Sprites["outer"] as RectSprite).SetSize(this.Size);
            (Sprites["middle"] as RectSprite).SetSize(this.Size.Width - 2, this.Size.Height - 2);
            (Sprites["inner"] as RectSprite).SetSize(this.Size.Width - 4, this.Size.Height - 4);
        }
    }
}
