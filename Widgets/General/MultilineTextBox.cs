using System;
using System.Collections.Generic;
using System.Text;
using ODL;

namespace MKEditor.Widgets
{
    public class MultilineTextBox : Widget
    {
        public MultilineTextArea TextArea;
        public string Text { get { return TextArea.Text; } }

        Container MainContainer;

        public MultilineTextBox(IContainer Parent) : base(Parent)
        {
            MainContainer = new Container(this);
            MainContainer.SetPosition(5, 5);
            MainContainer.VAutoScroll = true;
            VScrollBar vs = new VScrollBar(this);
            vs.ScrollStep = 5;
            MainContainer.SetVScrollBar(vs);
            TextArea = new MultilineTextArea(MainContainer);
            TextArea.OnSizeChanged += delegate (BaseEventArgs e)
            {
                if (TextArea.Size.Height < MainContainer.Size.Height) TextArea.SetHeight(MainContainer.Size.Height);
            };
            Sprites["box"] = new Sprite(this.Viewport);
        }

        public void SetText(string Text)
        {
            TextArea.SetText(Text);
        }

        public void SetFont(Font Font)
        {
            TextArea.SetFont(Font);
        }

        public void SetCaretIndex(int Index)
        {
            TextArea.SetCaretIndex(Index);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            Sprites["box"].Bitmap?.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Color light = new Color(86, 108, 134);
            Color dark = new Color(10, 23, 37);
            Sprites["box"].Bitmap.FillRect(Size, light);
            Sprites["box"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["box"].Bitmap.DrawLine(2, 1, Size.Width - 13, 1, dark);
            Sprites["box"].Bitmap.DrawLine(1, 2, 1, Size.Height - 3, dark);
            Sprites["box"].Bitmap.DrawLine(2, Size.Height - 2, Size.Width - 13, Size.Height - 2, dark);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 13, 2, Size.Width - 13, Size.Height - 3, dark);
            Sprites["box"].Bitmap.FillRect(Size.Width - 11, 1, 9, Size.Height - 2, dark);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 2, 2, Size.Width - 2, Size.Height - 3, dark);
            Sprites["box"].Bitmap.Lock();
            MainContainer.SetSize(Size.Width - 18, Size.Height - 7);
            MainContainer.VScrollBar.SetPosition(Size.Width - 10, 3);
            MainContainer.VScrollBar.SetSize(10, Size.Height - 6);
            TextArea.SetWidth(MainContainer.Size.Width);
            if (TextArea.Size.Height < MainContainer.Size.Height) TextArea.SetHeight(MainContainer.Size.Height);
        }

        public override object GetValue(string Identifier)
        {
            if (string.IsNullOrEmpty(Identifier)) return this.Text;
            return base.GetValue(Identifier);
        }

        public override void SetValue(string Identifier, object Value)
        {
            if (string.IsNullOrEmpty(Identifier)) this.SetText((string) Value);
            else if (Identifier == "index")
            {
                TextArea.Caret.CharacterIndex = ((string) Value).Length;
            }
            else base.SetValue(Identifier, Value);
        }
    }
}
