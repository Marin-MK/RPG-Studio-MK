using System;
using System.Collections.Generic;
using System.Linq;
using ODL;
using static SDL2.SDL;

namespace MKEditor.Widgets
{
    public class TextBox : Widget
    {
        public string Text { get; protected set; }
        public int CaretIndex { get; protected set; }

        private Dictionary<char, int> CharWidths = new Dictionary<char, int>();
        private bool TextChanged = true;
        private int TextWidth = 0;

        private DateTime? CaretAnim;

        public TextBox(object Parent)
            : base(Parent, "textBox")
        {
            this.WidgetIM.OnHoverChanged += HoverChanged;
            this.OnWidgetSelect += WidgetSelect;
            this.SetSize(100, 20);
            this.Sprites["box"] = new Sprite(this.Viewport);
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].Bitmap = new Bitmap(1, 1);
            this.Sprites["text"].Bitmap.Font = new Font("Fonts/Segoe UI");
            this.Sprites["overlay"] = new Sprite(this.Viewport);
            this.Sprites["caret"] = new Sprite(this.Viewport);
            this.Sprites["caret"].Bitmap = new Bitmap(1, 13);
            this.Sprites["caret"].Bitmap.DrawLine(0, 0, 0, 12, Color.BLACK);
            this.Sprites["caret"].X = 3;
            this.Sprites["caret"].Y = 3;
            this.Sprites["caret"].Visible = false;
            this.Text = "";
            this.CaretIndex = 0;
        }

        public void UpdateWidths()
        {
            foreach (char c in this.Text)
            {
                if (!CharWidths.ContainsKey(c))
                {
                    int w = this.Sprites["text"].Bitmap.TextSize(c.ToString()).Width;
                    CharWidths[c] = w;
                }
            }
        }

        public void SetText(string Text)
        {
            while (Text.Contains("\n")) Text = Text.Replace("\n", "");
            if (this.Text != Text)
            {
                this.Text = Text;
                this.TextChanged = true;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (this.Sprites["box"].Bitmap != null) this.Sprites["box"].Bitmap.Dispose();
            this.Sprites["box"].Bitmap = new Bitmap(this.Size);
            Color c = new Color(122, 122, 122);
            if (this.Selected)
            {
                c.Set(0, 120, 215);
            }
            else if (this.WidgetIM.HoverAnim())
            {
                c.Set(23, 23, 23);
            }
            this.Sprites["box"].Bitmap.DrawRect(this.Size, c);
            this.Sprites["box"].Bitmap.FillRect(1, 1, this.Size.Width - 2, this.Size.Height - 2, Color.WHITE);

            if (this.Sprites["overlay"].Bitmap != null) this.Sprites["overlay"].Bitmap.Dispose();
            this.Sprites["overlay"].Bitmap = new Bitmap(this.Size);
            this.Sprites["overlay"].Bitmap.FillRect(1, 1, 2, this.Size.Height - 2, Color.WHITE);
            this.Sprites["overlay"].Bitmap.DrawLine(0, 0, 0, this.Size.Height, c);
            this.Sprites["overlay"].Bitmap.FillRect(this.Size.Width - 3, 1, 2, this.Size.Height - 2, Color.WHITE);
            this.Sprites["overlay"].Bitmap.DrawLine(this.Size.Width - 1, 0, this.Size.Width - 1, this.Size.Height, c);
            
            TextWidth = this.Sprites["text"].Bitmap.TextSize(this.Text).Width;

            if (this.TextChanged)
            {
                if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
                this.Sprites["text"].Bitmap = new Bitmap(Math.Max(TextWidth + 3, 1), this.Size.Height - 3);
                this.Sprites["text"].Bitmap.Font = new Font("Fonts/Segoe UI");
                this.Sprites["text"].Bitmap.DrawText(this.Text, 3, 1, Color.BLACK);
            }

            // 3 pixels unused at each end, + 2 for the caret (meaning width - 8)
            if (TextWidth > this.Size.Width - 8)
            {
                int diffx = TextWidth - (this.Size.Width - 8);
                Console.WriteLine(diffx);
                this.Sprites["text"].X = -diffx;
            }
            else
            {
                this.Sprites["text"].X = 0;
            }

            this.Sprites["caret"].X = this.Sprites["text"].Bitmap.Width + this.Sprites["text"].X + 1;

            this.TextChanged = false;
            base.Draw();
        }

        public override void Update()
        {
            if (this.CaretAnim != null && this.CaretAnim < DateTime.Now)
            {
                this.Sprites["caret"].Visible = !this.Sprites["caret"].Visible;
                this.CaretAnim = new DateTime(DateTime.Now.Ticks + (int) (0.6 * 10000000));
            }
            base.Update();
        }

        public override void WidgetSelected(object sender, EventArgs e)
        {
            Input.StartTextInput();
            this.Sprites["caret"].Visible = true;
            this.CaretAnim = new DateTime(DateTime.Now.Ticks + (int) (0.6 * 10000000));
        }

        public override void TextInput(object sender, TextInputEventArgs e)
        {
            string oldtext = this.Text;
            if (e.Backspace && this.Text.Length > 0)
            {
                this.Text = this.Text.Remove(this.CaretIndex - 1);
                this.CaretIndex--;
            }
            string NewStr = e.Text.Replace("\n", "");
            this.Text = this.Text.Insert(this.CaretIndex, NewStr);
            this.CaretIndex += NewStr.Length;
            if (oldtext != this.Text)
            {
                this.TextChanged = true;
                this.UpdateWidths();
                this.Redraw();
            }
        }

        public override void WidgetDeselected(object sender, EventArgs e)
        {
            Input.StopTextInput();
            this.Sprites["caret"].Visible = false;
            this.CaretAnim = null;
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (this.WidgetIM.Hovering)
            {
                Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
            }
            else
            {
                Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
            }
        }
    }
}
