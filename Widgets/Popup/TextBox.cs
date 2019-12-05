using System;
using ODL;
using static SDL2.SDL;

namespace MKEditor.Widgets
{
    public class TextBox : Widget
    {
        public string Text { get { return TextArea.Text; } }
        public int CaretIndex { get { return TextArea.CaretIndex; } }
        public int SelectionStartIndex { get { return TextArea.SelectionStartIndex; } }
        public int SelectionEndIndex { get { return TextArea.SelectionEndIndex; } }

        TextArea TextArea;

        public EventHandler<EventArgs> OnTextChanged;

        public TextBox(object Parent, string Name = "textBox")
            : base(Parent, Name)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            TextArea = new TextArea(this);
            TextArea.SetPosition(3, 3);
        }

        public void SetInitialText(string Text)
        {
            TextArea.SetInitialText(Text);
            if (OnTextChanged != null) OnTextChanged.Invoke(null, new EventArgs());
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            TextArea.SetSize(Size.Width - 6, Size.Height - 6);
            base.SizeChanged(sender, e);
        }

        protected override void Draw()
        {
            if (Sprites["box"].Bitmap != null) Sprites["box"].Bitmap.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Color gray = new Color(86, 108, 134);
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
            Sprites["box"].Bitmap.Lock();
            base.Draw();
        }
    }
}
