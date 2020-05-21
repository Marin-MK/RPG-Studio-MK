using ODL;
using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor.Widgets
{
    public class DynamicLabel : Label
    {
        public object TextFormat { get; protected set; }
        public Dictionary<string, object> Parameters { get; protected set; }
        public IUIParser Parser { get; protected set; }
        public List<Color> Colors { get; protected set; }
        public bool Parsing = true;

        public DynamicLabel(IContainer Parent) : base(Parent)
        {
            // Special label to process [c=0]{value}[c=1] stuff for dynamic commands/conditions
        }

        public void SetParameters(Dictionary<string, object> Parameters)
        {
            this.Parameters = Parameters;
        }

        public void SetTextFormat(object TextFormat)
        {
            this.TextFormat = TextFormat;
        }

        public void SetParser(IUIParser Parser)
        {
            this.Parser = Parser;
        }

        public void SetColors(List<Color> Colors)
        {
            this.Colors = Colors;
        }

        public override void SetText(string Text, DrawOptions DrawOptions = DrawOptions.LeftAlign)
        {
            if (this.Text != Text || this.TextFormat is string && (string) this.TextFormat != Text)
            {
                this.Text = Text;
                this.TextFormat = this.Text;
                this.DrawOptions = DrawOptions;
                this.RedrawText();
            }
        }

        public override void RedrawText()
        {
            if (Parameters == null || TextFormat == null || this.Colors == null) return;
            Sprites["text"].Bitmap?.Dispose();
            string text = Utilities.ProcessText(this.TextFormat, this.Parameters, this.Parser, this.Parsing);
            if (string.IsNullOrEmpty(text)) return;
            Size s = this.Font.TextSize(text);
            this.SetSize(s);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            int x = 0;
            Color color = this.Colors[0];
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '[' && text[i + 1] == 'c' && text[i + 2] == '=')
                {
                    int startnum = i + 3;
                    int endnum = -1;
                    for (int j = startnum; j < text.Length; j++)
                    {
                        if (!Utilities.IsNumeric(text[j]))
                        {
                            endnum = j;
                            break;
                        }
                    }
                    if (endnum != -1)
                    {
                        int idx = 0;
                        if (endnum != startnum) idx = Convert.ToInt32(text.Substring(startnum, endnum - startnum));
                        if (idx < this.Colors.Count) color = this.Colors[idx];
                        else throw new Exception($"Only {this.Colors.Count} defined colors; Index {idx} out of range.");
                    }
                    i = endnum;
                    continue;
                }
                Sprites["text"].Bitmap.DrawText(text[i].ToString(), x, 0, this.Enabled ? color : new Color(72, 72, 72), this.DrawOptions);
                x += Sprites["text"].Bitmap.TextSize(text[i]).Width;
            }
            Sprites["text"].Bitmap.Lock();
        }

        public override void SetValue(string Identifier, object Value)
        {
            if (string.IsNullOrEmpty(Identifier))
            {
                this.SetTextFormat((string) Value);
                this.RedrawText();
            }
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
            if (this.TextFormat == null)
            {
                this.SetHeight(Font.Size + 4);
                return;
            }
            string text = Utilities.ProcessText(this.TextFormat, this.Parameters, this.Parser, this.Parsing);
            if (string.IsNullOrEmpty(text))
            {
                this.SetHeight(Font.Size + 4);
                return;
            }
            List<string> Lines = Utilities.FormatString(this.Font, text, Size.Width);
            this.SetHeight((Font.Size + 4) * Lines.Count);
            Sprites["text"].Bitmap = new Bitmap(this.Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            int x = 0;
            Color color = this.Colors[0];
            int oldline = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') continue;
                if (this.Parsing && text[i] == '[' && text[i + 1] == 'c' && text[i + 2] == '=')
                {
                    int startnum = i + 3;
                    int endnum = -1;
                    for (int j = startnum; j < text.Length; j++)
                    {
                        if (!Utilities.IsNumeric(text[j]))
                        {
                            endnum = j;
                            break;
                        }
                    }
                    if (endnum != -1)
                    {
                        int idx = 0;
                        if (endnum != startnum) idx = Convert.ToInt32(text.Substring(startnum, endnum - startnum));
                        if (idx < this.Colors.Count) color = this.Colors[idx];
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
                Sprites["text"].Bitmap.DrawText(text[i].ToString(), x, line * (Font.Size + 4), this.Enabled ? color : new Color(72, 72, 72), this.DrawOptions);
                x += Sprites["text"].Bitmap.TextSize(text[i]).Width;
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
