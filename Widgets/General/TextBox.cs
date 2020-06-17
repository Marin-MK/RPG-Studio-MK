using System;
using odl;
using static SDL2.SDL;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class TextBox : Widget
    {
        public string Text { get { return TextArea.Text; } }
        public int CaretIndex { get { return TextArea.CaretIndex; } }
        public int SelectionStartIndex { get { return TextArea.SelectionStartIndex; } }
        public int SelectionEndIndex { get { return TextArea.SelectionEndIndex; } }
        public bool Enabled { get; protected set; } = true;

        public int Skin { get; protected set; } = 0;

        public TextArea TextArea;

        public BaseEvent OnTextChanged { get { return TextArea.OnTextChanged; } set { TextArea.OnTextChanged = value; } }

        public TextBox(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            TextArea = new TextArea(this);
            TextArea.SetPosition(3, 3);
            TextArea.SetCaretHeight(13);
            TextArea.SetZIndex(1);
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
            if (this.Text != Text)
                TextArea.SetInitialText(Text);
        }

        public void SetCaretIndex(int Index)
        {
            TextArea.CaretIndex = Index;
            TextArea.RepositionSprites();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            TextArea.SetSize(Size.Width - 6, Size.Height - 6);
        }

        public void SetSkin(int Skin)
        {
            if (this.Skin != Skin)
            {
                this.Skin = Skin;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["box"].Bitmap != null) Sprites["box"].Bitmap.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            if (Skin == 0)
            {
                Color gray = this.Enabled ? new Color(86, 108, 134) : new Color(72, 72, 72);
                Color dark = new Color(36, 34, 36);
                Sprites["box"].Bitmap.SetPixel(1, 1, gray);
                Sprites["box"].Bitmap.DrawLine(2, 0, Size.Width - 3, 0, gray);
                Sprites["box"].Bitmap.DrawLine(2, 1, Size.Width - 3, 1, dark);
                Sprites["box"].Bitmap.SetPixel(Size.Width - 2, 1, gray);
                Sprites["box"].Bitmap.DrawLine(Size.Width - 1, 2, Size.Width - 1, Size.Height - 3, gray);
                Sprites["box"].Bitmap.DrawLine(Size.Width - 2, 2, Size.Width - 2, Size.Height - 3, dark);
                Sprites["box"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, gray);
                Sprites["box"].Bitmap.DrawLine(2, Size.Height - 1, Size.Width - 3, Size.Height - 1, gray);
                Sprites["box"].Bitmap.DrawLine(2, Size.Height - 2, Size.Width - 3, Size.Height - 2, dark);
                Sprites["box"].Bitmap.SetPixel(1, Size.Height - 2, gray);
                Sprites["box"].Bitmap.DrawLine(0, 2, 0, Size.Height - 3, gray);
                Sprites["box"].Bitmap.DrawLine(1, 2, 1, Size.Height - 3, dark);
                Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, gray);
            }
            else if (Skin == 1)
            {
                Sprites["box"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, 10, 23, 37);
                Sprites["box"].Bitmap.SetPixel(0, 0, Color.ALPHA);
                Sprites["box"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
                Sprites["box"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
                Sprites["box"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
                Sprites["box"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, 54, 81, 108);
            }
            Sprites["box"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (!WidgetIM.Hovering && TextArea.SelectedWidget)
            {
                Window.UI.SetSelectedWidget(null);
            }
        }

        public override object GetValue(string Identifier)
        {
            return this.Text;
        }

        public override void SetValue(string Identifier, object Value)
        {
            this.SetInitialText((string) Value);
        }
    }
}
