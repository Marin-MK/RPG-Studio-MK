using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class HelpText : Widget
    {
        public Font Font { get; protected set; } = Font.Get("Fonts/ProductSans-M", 14);
        public int MaxWidth { get; protected set; } = 300;

        public HelpText(object Parent, string Name = "helpText")
            : base(Parent, Name)
        {
            Sprites["rect"] = new RectSprite(this.Viewport);
            (Sprites["rect"] as RectSprite).SetOuterColor(55, 187, 255);
            (Sprites["rect"] as RectSprite).SetInnerColor(48, 96, 148);
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = Sprites["text"].Y = 7;
        }

        public void SetText(string Text)
        {
            List<string> Lines = new List<string>();
            string lastline = "";
            string lastword = "";
            string splitters = " `~!@#$%^&*()-=+[]{}\\|;:'\",.<>/?\n";
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '\n')
                {
                    Lines.Add(lastline + lastword);
                    lastline = "";
                    lastword = "";
                    continue;
                }
                lastword += Text[i];
                if (splitters.Contains(Text[i]))
                {
                    lastline += lastword;
                    lastword = "";
                }
                if (Font.TextSize(lastline + lastword).Width > MaxWidth)
                {
                    Lines.Add(lastline);
                    lastline = lastword;
                    lastword = "";
                }
            }
            if (!string.IsNullOrEmpty(lastword)) lastline += lastword;
            if (!string.IsNullOrEmpty(lastline)) Lines.Add(lastline);
            int w = 0;
            int h = Lines.Count * 24;
            for (int i = 0; i < Lines.Count; i++)
            {
                int linewidth = Font.TextSize(Lines[i]).Width;
                if (linewidth > w) w = linewidth;
            }
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Sprites["text"].Bitmap = new Bitmap(w, h);
            SetSize(w + 14, h + 14);
            Sprites["text"].Bitmap.Font = this.Font;
            Sprites["text"].Bitmap.Unlock();
            for (int i = 0; i < Lines.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Lines[i], 0, i * 24, Color.WHITE);
            }
            Sprites["text"].Bitmap.Lock();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (Sprites["rect"] as RectSprite).SetSize(Size);
        }
    }
}
