using System;
using ODL;

namespace MKEditor.Widgets
{
    public class ColoredBox : Widget
    {
        public Color InnerColor { get; protected set; } = Color.ALPHA;
        public Color OuterColor { get; protected set; } = Color.ALPHA;
        public int Thickness { get; protected set; } = 1;

        public ColoredBox(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new RectSprite(this.Viewport);
        }

        public void SetInnerColor(byte R, byte G, byte B, byte A = 255)
        {
            SetInnerColor(new Color(R, G, B, A));
        }
        public void SetInnerColor(Color inner)
        {
            this.InnerColor = inner;
            Redraw();
        }

        public void SetOuterColor(byte R, byte G, byte B, byte A = 255)
        {
            SetOuterColor(new Color(R, G, B, A));
        }
        public void SetOuterColor(Color outer)
        {
            this.OuterColor = outer;
            Redraw();
        }

        public void SetThickness(int thick)
        {
            this.Thickness = thick;
            Redraw();
        }

        protected override void Draw()
        {
            RectSprite box = Sprites["box"] as RectSprite;
            box.SetInnerColor(InnerColor);
            box.SetOuterColor(OuterColor);
            box.SetSize(this.Size, Thickness);
            base.Draw();
        }
    }
}
