using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class Label : Widget
    {
        public string Text { get; protected set; }
        public Font Font { get; protected set; }
        public Color TextColor { get; protected set; } = Color.WHITE;
        public DrawOptions DrawOptions { get; protected set; }

        public Label(object Parent, string Name = "label")
            : base(Parent, Name)
        {
            Sprites["text"] = new Sprite(this.Viewport);
            this.Font = Font.Get("Fonts/ProductSans-M", 12);
        }

        public void SetText(string Text, DrawOptions DrawOptions = ODL.DrawOptions.LeftAlign)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                this.DrawOptions = DrawOptions;
                Redraw();
            }
        }

        public void SetDrawOptions(DrawOptions DrawOptions = ODL.DrawOptions.LeftAlign)
        {
            if (this.DrawOptions != DrawOptions)
            {
                this.DrawOptions = DrawOptions;
                Redraw();
            }
        }

        public void SetFont(Font f)
        {
            if (this.Font != f)
            {
                this.Font = f;
                Redraw();
            }
        }

        public void SetTextColor(Color c)
        {
            if (this.TextColor != c)
            {
                this.TextColor = c;
                Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            if (string.IsNullOrEmpty(this.Text)) return;
            Size s = this.Font.TextSize(this.Text, this.DrawOptions);
            this.SetSize(s);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            Sprites["text"].Bitmap.DrawText(this.Text, this.TextColor, this.DrawOptions);
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }
    }

    public class MultilineLabel : Label
    {
        public MultilineLabel(object Parent, string Name = "multilineLabel")
            : base(Parent, Name)
        {
            
        }

        public void RedrawText()
        {
            List<string> Words = new List<string>();
            foreach (string word in this.Text.Split(' '))
            {
                if (word.Contains("\n"))
                {
                    List<string> splits = new List<string>(word.Split('\n'));
                    for (int i = 0; i < splits.Count; i++)
                    {
                        Words.Add(splits[i]);
                        if (i != splits.Count - 1) Words.Add("\n");
                    }
                }
                else
                {
                    Words.Add(word);
                }
            }
            List<string> Lines = new List<string>() { "" };
            int width = Size.Width;
            for (int i = 0; i < Words.Count; i++)
            {
                if (Words[i] == "\n")
                {
                    if (Lines[Lines.Count - 1].Length > 0) Lines[Lines.Count - 1] = Lines[Lines.Count - 1].Remove(Lines[Lines.Count - 1].Length - 1);
                    Lines.Add("");
                    continue;
                }
                string text = Lines[Lines.Count - 1] + Words[i];
                Size s = Font.TextSize(text);
                if (s.Width >= width)
                {
                    if (Lines[Lines.Count - 1].Length > 0) Lines[Lines.Count - 1] = Lines[Lines.Count - 1].Remove(Lines[Lines.Count - 1].Length - 1);
                    Lines.Add(Words[i] + " ");
                }
                else
                {
                    Lines[Lines.Count - 1] += Words[i] + " ";
                }
            }
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            SetSize(Size.Width, (Font.Size + 4) * Lines.Count);
            Sprites["text"].Bitmap = new Bitmap(Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            for (int i = 0; i < Lines.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Lines[i], 0, (Font.Size + 2) * i, Color.WHITE);
            }
            Sprites["text"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            this.RedrawText();
            this.Drawn = true;
        }
    }
}
