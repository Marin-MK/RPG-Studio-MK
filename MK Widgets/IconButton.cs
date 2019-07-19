using System;
using ODL;

namespace MKEditor.Widgets
{
    public class IconButton : Widget
    {
        public Icon Icon;
        public int IconX;
        public int IconY;
        public bool Toggleable = false;

        private bool Selected = false;

        public IconButton(object Parent, string Name = "iconButton")
            : base(Parent, Name)
        {
            this.SetSize(26, 26);
            Sprites["bg"] = new Sprite(this.Viewport);
            Sprites["bg"].Bitmap = new Bitmap(26, 26);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.FillRect(0, 0, 26, 26, Color.WHITE);
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(25, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, 25, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(25, 25, Color.ALPHA);
            Sprites["bg"].Color = Color.ALPHA;
            Sprites["bg"].Bitmap.Lock();
            Sprites["icon"] = new Sprite(this.Viewport);
            Sprites["icon"].Bitmap = new Bitmap(26, 26);
            this.WidgetIM.OnHoverChanged += HoverChanged;
            this.WidgetIM.OnMouseDown += MouseDown;
        }

        public void SetSelected(bool Selected)
        {
            if (this.Selected != Selected)
            {
                if (Selected && !Toggleable)
                {
                    foreach (Widget w in Parent.Widgets)
                    {
                        if (!(w is IconButton)) continue;
                        IconButton b = w as IconButton;
                        if (b.Selected) b.SetSelected(false);
                    }
                }
                this.Selected = Selected;
                Redraw();
            }
        }

        public void SetIcon(int IconX, int IconY)
        {
            Sprites["icon"].Bitmap.Unlock();
            Sprites["icon"].Bitmap.Build(new Rect(1, 1, 24, 24), Widget.IconSheet, new Rect(IconX * 24, IconY * 24, 24, 24));
            Sprites["icon"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            Sprites["bg"].Color = Color.ALPHA;
            Sprites["icon"].Color = Color.WHITE;
            if (Selected)
            {
                Sprites["bg"].Color = new Color(255, 168, 54);
                Sprites["icon"].Color = Color.BLACK;
            }
            else if (WidgetIM.Hovering)
            {
                Sprites["bg"].Color = new Color(79, 82, 91);
            }
            base.Draw();
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (WidgetIM.Hovering)
            {
                if (Toggleable) SetSelected(!Selected);
                else SetSelected(true);
            }
        }
    }
}
