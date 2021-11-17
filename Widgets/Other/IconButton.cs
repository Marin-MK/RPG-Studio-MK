using System;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class IconButton : Widget
    {
        public int IconX;
        public int IconY;
        public bool Toggleable = false;
        public bool Selectable = true;

        public bool Selected { get; protected set; } = false;

        public BaseEvent OnSelection;
        public BaseEvent OnDeselection;

        public IconButton(IContainer Parent) : base(Parent)
        {
            SetSize(24, 28);
            Sprites["icon"] = new Sprite(Viewport);
            Sprites["icon"].Bitmap = Utilities.IconSheet;
            Sprites["icon"].DestroyBitmap = false;
            Sprites["icon"].SrcRect.Width = 24;
            Sprites["icon"].SrcRect.Height = 24;
            Sprites["selector"] = new Sprite(Viewport, new SolidBitmap(24, 2, 59, 227, 255));
            Sprites["selector"].Y = 26;
            Sprites["selector"].Visible = false;
            OnWidgetSelected += WidgetSelected;
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
                        if (b.Selected && !b.Toggleable) b.SetSelected(false);
                    }
                }
                this.Selected = Selected;
                if (Selected) this.OnSelection?.Invoke(new BaseEventArgs());
                if (!Selected) this.OnDeselection?.Invoke(new BaseEventArgs());
                SetIcon(IconX, IconY, Selected);
            }
        }

        public void SetIcon(int IconX, int IconY, bool Selected = false)
        {
            Sprites["icon"].SrcRect.X = IconX * 24;
            Sprites["icon"].SrcRect.Y = IconY * 24 + (Selected ? 24 : 0);
            // Changing SrcRect doesn't update the viewport/renderer
            Sprites["icon"].Update();
            this.IconX = IconX;
            this.IconY = IconY;
        }

        public void SetSelectorOffset(int pixels)
        {
            SetSize(24, 28 + pixels);
            Sprites["selector"].Y = 26 + pixels;
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            Sprites["selector"].Visible = WidgetIM.Hovering;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            int ry = e.Y - Viewport.Y;
            if (WidgetIM.Hovering && ry < 29 && !TimerExists("reset"))
            {
                if (Toggleable) SetSelected(!Selected);
                else if (Selectable) SetSelected(true);
                else // Normal pressable button
                {
                    SetIcon(IconX, IconY, true);
                    SetTimer("reset", 100);
                    this.OnLeftClick?.Invoke( e);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (TimerPassed("reset"))
            {
                SetIcon(IconX, IconY, false);
                DestroyTimer("reset");
            }
        }
    }
}
