using System;
using System.Collections.Generic;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class BrowserBox : Widget
    {
        public string Text { get { return TextArea.Text; } }
        public Color TextColor { get { return TextArea.TextColor; } }
        public bool ReadOnly { get { return TextArea.ReadOnly; } }
        public bool Enabled { get; protected set; } = true;

        public TextArea TextArea;

        public BaseEvent OnTextChanged { get { return TextArea.OnTextChanged; } set { TextArea.OnTextChanged = value; } }
        public BaseEvent OnDropDownClicked;

        public BrowserBox(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            TextArea = new TextArea(this);
            TextArea.SetPosition(3, 3);
            TextArea.SetCaretHeight(13);
            TextArea.SetZIndex(1);
            SetSize(100, 21);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            TextArea.SetSize(this.Size.Width - 21, this.Size.Height - 3);
        }

        public void SetEnabled(bool Enabled)
        {
            if (this.Enabled != Enabled)
            {
                this.Enabled = Enabled;
                this.Redraw();
                this.TextArea.SetEnabled(Enabled);
            }
        }

        public void SetInitialText(string Text)
        {
            if (this.Text != Text) this.TextArea.SetInitialText(Text);
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
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, Enabled ? new Color(54, 81, 108) : new Color(72, 72, 72));
            Sprites["bg"].Bitmap.FillRect(Size.Width - 18, 1, 17, Size.Height - 2, Enabled ? new Color(28, 50, 73) : new Color(72, 72, 72));
            int y = (int) Math.Floor(Size.Height / 2d) - 1;
            Color arrowcolor = Enabled ? Color.WHITE : new Color(28, 50, 73);
            Sprites["bg"].Bitmap.FillRect(Size.Width - 13, y, 7, 2, arrowcolor);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, y + 2, Size.Width - 8, y + 2, arrowcolor);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 11, y + 3, Size.Width - 9, y + 3, arrowcolor);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 10, y + 4, arrowcolor);
            Sprites["bg"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (!TextArea.WidgetIM.Hovering && TextArea.SelectedWidget)
            {
                Window.UI.SetSelectedWidget(null);
            }
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= Size.Width - 18 && rx < Size.Width - 1 &&
                ry >= 1 && ry < Size.Height - 1 && this.Enabled)
            {
                this.OnDropDownClicked?.Invoke(new BaseEventArgs());
            };
        }
    }
}
