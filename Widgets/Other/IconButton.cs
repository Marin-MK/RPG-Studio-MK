using System;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class IconButton : Widget
    {
        public Icon Icon;
        public bool Toggleable = false;
        public bool Selectable = true;

        public bool Selected { get; protected set; } = false;

        public BaseEvent OnSelection;
        public BaseEvent OnDeselection;
        public BaseEvent OnClicked;

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
                SetIcon(Icon, Selected);
            }
        }

        public void SetIcon(Icon Icon, bool Selected = false)
        {
            Sprites["icon"].SrcRect.X = (int) Icon * 24;
            Sprites["icon"].SrcRect.Y = Selected ? 24 : 0;
            // Changing SrcRect doesn't update the viewport/renderer
            Sprites["icon"].Update();
            this.Icon = Icon;
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
            if (WidgetIM.Hovering && ry < 29 && !TimerExists("reset") && e.OldLeftButton != e.LeftButton && e.LeftButton)
            {
                if (Toggleable) SetSelected(!Selected);
                else if (Selectable) SetSelected(true);
                else // Normal pressable button
                {
                    SetIcon(Icon, true);
                    SetTimer("reset", 100);
                    this.OnClicked?.Invoke(e);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (TimerPassed("reset"))
            {
                SetIcon(Icon, false);
                DestroyTimer("reset");
            }
        }
    }

    public enum Icon
    {
        Cut                 = 0,
        Copy                = 1,
        Paste               = 2,
        Cancel              = 3,
        Undo                = 4,
        Redo                = 5,
        Grid                = 6,
        ZoomOut             = 7,
        ZoomIn              = 8,

        Up                  = 10,
        Down                = 11,
        Delete              = 12,
        Eyes                = 13,
        Eye                 = 14,
        Pencil              = 15,
        Bucket              = 16,
        CircleOutline       = 17,
        RectangleOutline    = 18,
        Selection           = 19,
        Eraser              = 20,
        Right               = 21,
        Save                = 22,
        Map                 = 23,
        Event               = 24,
        Script              = 25,
        Monster             = 26
    }
}
