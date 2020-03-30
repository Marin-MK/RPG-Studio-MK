using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class DropdownBox : Widget
    {
        public string Text { get { return TextArea.Text; } }
        public Color TextColor { get { return TextArea.TextColor; } }
        public bool ReadOnly { get { return TextArea.ReadOnly; } }

        public TextArea TextArea;

        public EventHandler<EventArgs> OnTextChanged { get { return TextArea.OnTextChanged; } set { TextArea.OnTextChanged = value; } }
        public EventHandler<EventArgs> OnDropDownClicked;

        public DropdownBox(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            TextArea = new TextArea(this);
            TextArea.SetPosition(3, 3);
            TextArea.SetCaretHeight(13);
            TextArea.SetZIndex(1);
            SetSize(100, 21);

            WidgetIM.OnMouseDown += MouseDown;
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TextArea.SetSize(this.Size.Width - 21, this.Size.Height - 3);
        }

        public void SetInitialText(string Text)
        {
            this.TextArea.SetInitialText(Text);
        }

        public void SetFont(Font f)
        {
            this.TextArea.SetFont(f);
        }

        public void SetTextColor(Color c)
        {
            this.TextArea.SetTextColor(c);
        }

        public void SetReadOnly(bool ReadOnly)
        {
            this.TextArea.SetReadOnly(ReadOnly);
        }

        protected override void Draw()
        {
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, 10, 23, 37);
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, 54, 81, 108);
            Sprites["bg"].Bitmap.FillRect(Size.Width - 18, 1, 17, Size.Height - 2, 28, 50, 73);
            int y = (int) Math.Floor(Size.Height / 2d) - 1;
            Sprites["bg"].Bitmap.FillRect(Size.Width - 13, y, 7, 2, Color.WHITE);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, y + 2, Size.Width - 8, y + 2, Color.WHITE);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 11, y + 3, Size.Width - 9, y + 3, Color.WHITE);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 10, y + 4, Color.WHITE);
            Sprites["bg"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (!TextArea.WidgetIM.Hovering && TextArea.SelectedWidget)
            {
                Window.UI.SetSelectedWidget(null);
            }
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= Size.Width - 18 && rx < Size.Width - 1 &&
                ry >= 1 && ry < Size.Height - 1)
            {
                if (OnDropDownClicked != null) OnDropDownClicked.Invoke(null, new EventArgs());
            };
        }
    }
}
