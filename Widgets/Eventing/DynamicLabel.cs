using odl;
using System;
using System.Collections.Generic;
using System.Text;
using amethyst;

namespace MKEditor.Widgets
{
    public class DynamicLabel : Label
    {
        public List<Color> Colors { get; protected set; }
        public bool Parsing = true;

        public DynamicLabel(IContainer Parent) : base(Parent)
        {
            // Special label to process color embedded in the text.
        }

        public void SetColors(List<Color> Colors)
        {
            this.Colors = Colors;
        }

        public override void RedrawText()
        {
            Sprites["text"].Bitmap?.Dispose();
            if (string.IsNullOrEmpty(this.Text)) return;
            Size s = this.Font.TextSize(Text);
            this.SetSize(s.Width + 4, s.Height);
            Sprites["text"].Bitmap = new Bitmap(this.Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            int x = 0;
            Color color = this.Colors == null ? this.TextColor : this.Colors[0];
            for (int i = 0; i < Text.Length; i++)
            {
                if (this.Parsing && Text[i] == '[' && Text[i + 1] == 'c' && Text[i + 2] == '=')
                {
                    int startnum = i + 3;
                    int endnum = -1;
                    for (int j = startnum; j < Text.Length; j++)
                    {
                        if (!Utilities.IsNumeric(Text[j]))
                        {
                            endnum = j;
                            break;
                        }
                    }
                    if (endnum != -1)
                    {
                        int idx = 0;
                        if (endnum != startnum) idx = Convert.ToInt32(Text.Substring(startnum, endnum - startnum));
                        if (this.Colors != null && idx < this.Colors.Count) color = this.Colors[idx];
                        else color = Color.WHITE;// throw new Exception($"Only {this.Colors.Count} defined colors; Index {idx} out of range.");
                    }
                    i = endnum;
                    continue;
                }
                Sprites["text"].Bitmap.DrawText(Text[i].ToString(), x, 0, this.Enabled ? color : new Color(72, 72, 72), this.DrawOptions);
                x += Sprites["text"].Bitmap.TextSize(Text[i]).Width;
            }
            Sprites["text"].Bitmap.Lock();
        }
    }

    public class MultilineDynamicLabel : DynamicLabel
    {
        public MultilineDynamicLabel(IContainer Parent) : base(Parent)
        {

        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            this.RedrawText();
        }

        public override void RedrawText()
        {
            Sprites["text"].Bitmap?.Dispose();
            if (string.IsNullOrEmpty(this.Text))
            {
                this.SetHeight(Font.Size + 4);
                return;
            }
            List<string> Lines = Utilities.FormatString(this.Font, Text, Size.Width);
            this.SetHeight((Font.Size + 4) * Lines.Count);
            Sprites["text"].Bitmap = new Bitmap(this.Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            int x = 0;
            Color color = this.Colors == null ? this.TextColor : this.Colors[0];
            int oldline = -1;
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '\n') continue;
                if (this.Parsing && Text[i] == '[' && Text[i + 1] == 'c' && Text[i + 2] == '=')
                {
                    int startnum = i + 3;
                    int endnum = -1;
                    for (int j = startnum; j < Text.Length; j++)
                    {
                        if (!Utilities.IsNumeric(Text[j]))
                        {
                            endnum = j;
                            break;
                        }
                    }
                    if (endnum != -1)
                    {
                        int idx = 0;
                        if (endnum != startnum) idx = Convert.ToInt32(Text.Substring(startnum, endnum - startnum));
                        if (idx < this.Colors.Count) color = this.Colors == null ? Color.WHITE : this.Colors[idx];
                        else throw new Exception($"Only {this.Colors.Count} defined colors; Index {idx} out of range.");
                    }
                    i = endnum;
                    continue;
                }
                int line = Lines.Count - 1;
                int remidx = i;
                for (int j = 0; j < Lines.Count; j++)
                {
                    if (remidx < Lines[j].Length)
                    {
                        line = j;
                        break;
                    }
                    remidx -= Lines[j].Length;
                }
                if (line != oldline) x = 0;
                Sprites["text"].Bitmap.DrawText(Text[i].ToString(), x, line * (Font.Size + 4), this.Enabled ? color : new Color(72, 72, 72), this.DrawOptions);
                x += Sprites["text"].Bitmap.TextSize(Text[i]).Width;
                oldline = line;
            }
            Sprites["text"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            this.RedrawText();
            base.Draw();
        }
    }
}
