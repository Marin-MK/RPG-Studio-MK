using System;
using ODL;
using static SDL2.SDL;

namespace MKEditor.Widgets
{
    public class TextBox : Widget
    {
        public string Text { get { return TextEntryField.Text; } }
        public int CaretIndex { get { return TextEntryField.CaretIndex; } }
        public int SelectionIndexStart { get { return TextEntryField.SelectionIndexStart; } }
        public int SelectionIndexStop { get { return TextEntryField.SelectionIndexStop; } }

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

    public class TextEntryField : Widget
    {
        TextBox TextBox;

        public string Text = "";
        public int CaretIndex = 0;
        public int SelectionIndexStart = -1;
        public int SelectionIndexStop = -1;

        bool TextEntry = false;

        int CaretSelectionDistance = 0;

        public TextEntryField(object Parent, string Name = "textEntryField")
            : base(Parent, Name)
        {
            TextBox = Parent as TextBox;
            Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(1, 21, new Color(128, 128, 128)));
            Sprites["selection"].Visible = false;
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["caret"] = new Sprite(this.Viewport, new SolidBitmap(1, 21, new Color(36, 34, 36)));
            Sprites["caret"].Y = 1;
            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseDown += MouseDown;
            OnWidgetSelected += WidgetSelected;
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
                    bool DontMoveCaret = !(Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT)) && SelectionIndexStart != -1;
                    if (!DontMoveCaret)
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
                    int oldstart = SelectionIndexStart;
                    int oldstop = SelectionIndexStop;
                    if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                    {
                        if (SelectionIndexStart == -1) // No selection yet
                        {
                            SelectionIndexStart = CaretIndex + 1;
                            SelectionIndexStop = CaretIndex;
                        }
                        else
                        {
                            SelectionIndexStop -= 1;
                        }
                    }
                    else
                    {
                        SelectionIndexStart = SelectionIndexStop = -1;
                    }
                    UpdateSelection();
                    if (DontMoveCaret)
                    {
                        CaretIndex = oldidx;
                        SetCaretIndex(Math.Min(oldstart, oldstop));
                    }
                }
                else if (CaretIndex > oldidx) // Moved right
                {
                    bool DontMoveCaret = !(Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT)) && SelectionIndexStart != -1;
                    if (!DontMoveCaret)
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
                    int oldstart = SelectionIndexStart;
                    int oldstop = SelectionIndexStop;
                    if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                    {
                        if (SelectionIndexStart == -1) // No selection yet
                        {
                            SelectionIndexStart = CaretIndex - 1;
                            SelectionIndexStop = CaretIndex;
                        }
                        else
                        {
                            SelectionIndexStop += 1;
                        }
                    }
                    else
                    {
                        SelectionIndexStart = SelectionIndexStop = -1;
                    }
                    UpdateSelection();
                    if (DontMoveCaret)
                    {
                        CaretIndex = oldidx;
                        SetCaretIndex(Math.Max(oldstart, oldstop));
                    }
                }
            }
            else if (!Input.Press(SDL_Keycode.SDLK_LSHIFT) && !Input.Press(SDL_Keycode.SDLK_RSHIFT))
            {
                SelectionIndexStart = SelectionIndexStop = -1;
                UpdateSelection();
            }
        }

        public override void WidgetSelected(object sender, MouseEventArgs e)
        {
            base.WidgetSelected(sender, e);
            TextEntry = true;
            Input.StartTextInput();
            Sprites["caret"].Visible = true;
        }

        public override void WidgetDeselected(object sender, EventArgs e)
        {
            base.WidgetDeselected(sender, e);
            TextEntry = false;
            Input.StopTextInput();
        }

        public override void TextInput(object sender, TextInputEventArgs e)
        {
            if (!TextEntry) return;
            string oldtext = this.Text;
            int oldwidth = Sprites["text"].Bitmap.TextSize(Text.Substring(0, CaretIndex)).Width;
            if (e.Backspace)
            {
                if (SelectionIndexStart != -1 && SelectionIndexStop != -1)
                {
                    int length = Math.Abs(SelectionIndexStop - SelectionIndexStart);
                    this.Text = this.Text.Remove(SelectionIndexStop > SelectionIndexStart ? SelectionIndexStart : SelectionIndexStop, length);
                    if (SelectionIndexStop > SelectionIndexStart) this.CaretIndex -= length;
                    SelectionIndexStart = -1;
                    SelectionIndexStop = -1;
                }
                else if (CaretIndex > 0)
                {
                    this.Text = this.Text.Remove(this.CaretIndex - 1, 1);
                    this.CaretIndex--;
                }
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
            UpdateSelection();

            if (oldtext != this.Text)
            {
                Redraw();
            }
        }

        private void UpdateSelection()
        {
            if (SelectionIndexStart != -1)
            {
                Sprites["selection"].Visible = true;
                Sprites["caret"].Visible = false;
                int widthtocaret = Sprites["text"].Bitmap.TextSize(Text.Substring(0, CaretIndex)).Width;
                int widthtoselectstart = Sprites["text"].Bitmap.TextSize(Text.Substring(0, SelectionIndexStart)).Width;
                int start = SelectionIndexStart > SelectionIndexStop ? SelectionIndexStop : SelectionIndexStart;
                int end = SelectionIndexStart > SelectionIndexStop ? SelectionIndexStart : SelectionIndexStop;
                int selwidth = Sprites["text"].Bitmap.TextSize(Text.Substring(start, end - start)).Width;
                int diff = widthtocaret - widthtoselectstart;
                if (diff > 0) // caret is right of selection start
                {
                    Console.WriteLine("caret is right");
                    Sprites["selection"].X = Sprites["caret"].X - diff;
                }
                else // caret is left of selection start
                {
                    Sprites["selection"].X = Sprites["caret"].X;// - diff - selwidth;
                }
                (Sprites["selection"].Bitmap as SolidBitmap).SetSize(selwidth, 21);
            }
            else
            {
                Sprites["selection"].Visible = false;
                Sprites["caret"].Visible = true;
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
            if (Window.UI.SelectedWidget == this)
            {
                if (!TimerExists("switch")) SetTimer("switch", 500);
                if (TimerPassed("switch"))
                {
                    ResetTimer("switch");
                    Sprites["caret"].Visible = !Sprites["caret"].Visible;
                }
            }
            else
            {
                Sprites["caret"].Visible = false;
            }
            base.Update();
        }

        protected override void Draw()
        {
            Font f = Font.Get("Fonts/ProductSans-R", 14);
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Sprites["text"].Bitmap = new Bitmap(f.TextSize(this.Text).Width + 4, Size.Height);
            Sprites["text"].Bitmap.Unlock();
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

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (e.LeftButton != e.OldLeftButton && e.LeftButton && !WidgetIM.Hovering && Window.UI.SelectedWidget == this)
            {
                Window.UI.SetSelectedWidget(null);
            }
        }
    }
}
