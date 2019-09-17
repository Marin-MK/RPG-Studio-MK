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
        public bool Selectable = true;

        public bool Selected { get; protected set; } = false;

        public EventHandler<EventArgs> OnSelection;
        public EventHandler<EventArgs> OnDeselection;

        public IconButton(object Parent, string Name = "iconButton")
            : base(Parent, Name)
        {
            this.SetSize(24, 27);
            Sprites["icon"] = new Sprite(this.Viewport);
            Sprites["icon"].Bitmap = Utilities.IconSheet;
            Sprites["icon"].SrcRect.Width = 24;
            Sprites["icon"].SrcRect.Height = 24;
            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(24, 2, 59, 227, 255));
            Sprites["selector"].Y = 24;
            Sprites["selector"].Visible = false;
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
                if (Selected && this.OnSelection != null) this.OnSelection.Invoke(this, new EventArgs());
                if (!Selected && this.OnDeselection != null) this.OnDeselection.Invoke(this, new EventArgs());
                SetIcon(IconX, IconY, Selected);
            }
        }

        public void SetIcon(int IconX, int IconY, bool Selected = false)
        {
            Sprites["icon"].SrcRect.X = IconX * 24;
            Sprites["icon"].SrcRect.Y = IconY * 24 + (Selected ? 24 : 0);
            this.IconX = IconX;
            this.IconY = IconY;
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            Sprites["selector"].Visible = WidgetIM.Hovering;
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
