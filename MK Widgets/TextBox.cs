using System;
using ODL;
using static SDL2.SDL;

namespace MKEditor.Widgets
{
    public class TextBox : Widget
    {
        public string Text { get { return TextEntryField.Text; } }
        public int CaretIndex { get { return TextEntryField.CaretIndex; } }

        TextEntryField TextEntryField;

        public TextBox(object Parent, string Name = "textBox")
            : base(Parent, Name)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            TextEntryField = new TextEntryField(this);
            TextEntryField.SetPosition(4, 2);
        }

        public void SetText(string Text)
        {
            TextEntryField.SetText(Text);
        }

        public void SetCaretIndex(int Index)
        {
            TextEntryField.SetCaretIndex(Index);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            TextEntryField.SetSize(Size.Width - 8, Size.Height - 4);
            base.SizeChanged(sender, e);
        }

        protected override void Draw()
        {
            if (Sprites["box"].Bitmap != null) Sprites["box"].Bitmap.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Color gray = new Color(108, 103, 110);
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

    public class TextEntryField : Widget
    {
        TextBox TextBox;

        public string Text = "";
        public int CaretIndex = 0;

        public TextEntryField(object Parent, string Name = "textEntryField")
            : base(Parent, Name)
        {
            TextBox = Parent as TextBox;
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["caret"] = new Sprite(this.Viewport, new SolidBitmap(1, 21, new Color(32, 32, 32)));
            Sprites["caret"].Y = 1;
            WidgetIM.OnHoverChanged += HoverChanged;
            OnWidgetSelect += WidgetSelect;
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                Redraw();
            }
        }

        public void SetCaretIndex(int Index)
        {
            if (Index < 0) Index = 0;
            if (Index > Text.Length) Index = Text.Length;
            if (this.CaretIndex != Index)
            {
                int oldidx = CaretIndex;
                this.CaretIndex = Index;
                if (CaretIndex < oldidx) // Moved left
                {
                    int Diff = oldidx - CaretIndex;
                    string movedtext = Text.Substring(CaretIndex, Diff);
                    int movedwidth = Sprites["text"].Bitmap.TextSize(movedtext).Width;
                    Sprites["caret"].X -= movedwidth;
                    if (Sprites["caret"].X < 0)
                    {
                        Sprites["text"].X += -Sprites["caret"].X;
                        Sprites["caret"].X = 0;
                    }
                }
                else if (CaretIndex > oldidx) // Moved right
                {
                    int Diff = CaretIndex - oldidx;
                    string movedtext = Text.Substring(oldidx, Diff);
                    int movedwidth = Sprites["text"].Bitmap.TextSize(movedtext).Width;
                    Sprites["caret"].X += movedwidth;
                    if (Sprites["caret"].X >= Size.Width - 1)
                    {
                        Sprites["text"].X -= Sprites["caret"].X - Size.Width + 1;
                        Sprites["caret"].X = Size.Width - 1;
                    }
                }
            }
        }

        public override void WidgetSelect(object sender, MouseEventArgs e)
        {
            base.WidgetSelect(sender, e);
            Input.StartTextInput();
        }

        public override void TextInput(object sender, TextInputEventArgs e)
        {
            string oldtext = this.Text;
            int oldwidth = Sprites["text"].Bitmap.TextSize(Text.Substring(0, CaretIndex)).Width;
            if (e.Backspace && CaretIndex > 0)
            {
                this.Text = this.Text.Remove(this.CaretIndex - 1, 1);
                this.CaretIndex--;
            }
            string NewStr = e.Text.Replace("\n", "");
            this.Text = this.Text.Insert(this.CaretIndex, NewStr);
            this.CaretIndex += NewStr.Length;
            int newwidth = Sprites["text"].Bitmap.TextSize(Text.Substring(0, CaretIndex)).Width;
            // Positions the caret properly
            if (newwidth > oldwidth)
            {
                Sprites["caret"].X += newwidth - oldwidth;
                if (Sprites["caret"].X >= Size.Width - 1)
                {
                    Sprites["text"].X -= Sprites["caret"].X - Size.Width + 1;
                    Sprites["caret"].X = Size.Width - 1;
                }
            }
            else if (newwidth < oldwidth)
            {
                Sprites["caret"].X -= oldwidth - newwidth;
                if (Sprites["caret"].X < 0)
                {
                    Sprites["text"].X += -Sprites["caret"].X;
                    Sprites["caret"].X = 0;
                }
            }
            if (-Sprites["text"].X + Size.Width - 1 > Math.Max(Size.Width - 1, Sprites["text"].Bitmap.TextSize(Text).Width))
            {
                int Diff = -Sprites["text"].X + Size.Width - 1 - Sprites["text"].Bitmap.TextSize(Text).Width;
                if (Sprites["text"].X + Diff > 0)
                {
                    Diff = -Sprites["text"].X;
                }
                Sprites["text"].X += Diff;
                Sprites["caret"].X += Diff;
            }

            if (oldtext != this.Text)
            {
                Redraw();
            }
        }

        public override void Update()
        {
            if (Input.Trigger(SDL_Keycode.SDLK_LEFT))
            {
                SetCaretIndex(CaretIndex - 1);
            }
            if (Input.Trigger(SDL_Keycode.SDLK_RIGHT))
            {
                SetCaretIndex(CaretIndex + 1);
            }
            base.Update();
        }

        protected override void Draw()
        {
            Font f = Font.Get("Fonts/Ubuntu-B", 16);
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Sprites["text"].Bitmap = new Bitmap(f.TextSize(this.Text).Width + 4, Size.Height);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Clear();
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.DrawText(this.Text, 0, 2, Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (this.WidgetIM.Hovering)
                Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
            else
                Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        }
    }
}
