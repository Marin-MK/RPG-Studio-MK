using System;
using ODL;

namespace MKEditor.Widgets
{
    public class ModeButton : Widget
    {
        public string Text { get; private set; } = "";
        public bool Selected { get; private set; }

        public BaseEvent OnSelection;
        public BaseEvent OnDeselection;

        public ModeButton(IContainer Parent, string Text, int Icon) : base(Parent)
        {
            Sprites["text"] = new Sprite(this.Viewport);
            Font f = Font.Get("Fonts/Ubuntu-B", 14);
            Size s = f.TextSize(Text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText(Text, Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = 27;
            Sprites["text"].Y = 6;

            Sprites["icon"] = new Sprite(this.Viewport);
            Sprites["icon"].Bitmap = Utilities.IconSheet;
            Sprites["icon"].SrcRect = new Rect(Icon * 24, 0, 24, 24);
            Sprites["icon"].Y = 3;

            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(27 + s.Width, 2, new Color(59, 227, 255)));
            Sprites["selector"].Visible = false;
            Sprites["selector"].Y = 29;

            OnWidgetSelected += WidgetSelected;
            SetSize(27 + s.Width, 31);
        }

        public void SetSelected(bool Selected, bool Starting = false)
        {
            if (this.Selected != Selected || Starting)
            {
                if (Selected)
                {
                    foreach (Widget w in Parent.Widgets)
                    {
                        if (!(w is ModeButton)) continue;
                        ModeButton b = w as ModeButton;
                        if (b.Selected) b.SetSelected(false);
                    }
                }
                this.Selected = Selected;
                if (!Starting)
                {
                    if (Selected) this.OnSelection?.Invoke(new BaseEventArgs());
                    if (!Selected) this.OnDeselection?.Invoke(new BaseEventArgs());
                }
                Redraw();
            }
        }

        protected override void Draw()
        {
            Sprites["text"].Color = Selected ? new Color(0, 165, 255) : Color.WHITE;
            Sprites["icon"].SrcRect.Y = Selected ? 24 : 0;
            base.Draw();
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            int ry = e.Y - Viewport.Y;
            Sprites["selector"].Visible = WidgetIM.Hovering && ry < 42;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (Sprites["selector"].Visible)
            {
                SetSelected(true);
            }
        }
    }
}
