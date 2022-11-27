using System;
using System.Collections.Generic;
using System.Linq;

// TODO:
// - Run tokenizer on separate thread on startup
// - Make Undo/Redo system of types insertion and addition and use the InsertText and RemoveText methods,
//   that way only what needs to be redrawn/retokenized will ever be redrawn/retokenized
// - Trigger Undo/Redo state recording every X seconds rather than every single character insertion/removal

namespace RPGStudioMK.Widgets;

public class ScriptEditorTextArea : MultilineTextArea
{
    public Color LineTextColor { get; protected set; } = new Color(192, 192, 192);
    public Color LineTextBackgroundColor { get; protected set; } = new Color(10, 23, 37);
    public int LineTextWidth { get; protected set; } = 40;
    public int TextXOffset { get; protected set; } = 42;

    protected List<List<Token>> LineTokens = new List<List<Token>>();
    protected List<List<(int, Color)>> LineColors = new List<List<(int, Color)>>();
    protected int OldScrolledY;
    protected int OldLineCount;
    protected bool RequireScrollAdjustment = false;
    protected int SelectedLineNumberOrigin = -1;
    protected int LastLineNumber;

    protected new int BottomLineIndex => base.BottomLineIndex + 1;

    public ScriptEditorTextArea(IContainer Parent) : base(Parent)
    {
        this.OverlaySelectedText = false;
        this.LineWrapping = false;
        SetSelectionBackgroundColor(new Color(128, 128, 255, 64));
        ConsiderInAutoScrollPositioningY = false;
        Sprites["nums"] = new Sprite(this.Viewport);
        Sprites["guide"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(64, 64, 128)));
    }

    public void SetLineTextColor(Color LineTextColor)
    {
        if (this.LineTextColor != LineTextColor)
        {
            this.LineTextColor = LineTextColor;
            RedrawLineNumbers();
        }
    }
    public void SetLineTextBackgroundColor(Color LineTextBackgroundColor)
    {
        if (this.LineTextBackgroundColor != LineTextBackgroundColor)
        {                
            this.LineTextBackgroundColor = LineTextBackgroundColor;
            RedrawLineNumbers();
        }
    }

    public void SetLineTextWidth(int LineTextWidth)
    {
        if (this.LineTextWidth != LineTextWidth)
        {        
            this.LineTextWidth = LineTextWidth;
            RedrawLineNumbers();
        }
    }

    public void SetTextXOffset(int TextXOffset)
    {
        if (this.TextXOffset != TextXOffset)
        {
            this.TextXOffset = TextXOffset;
        }
    }

    public override void SetFont(Font Font)
    {
        if (this.Font == null && Font != null || this.Font != null && Font == null || !this.Font.Equals(Font))
        {
            Sprites["guide"].X = TextXOffset + Font.TextSize('a').Width * 80;
        }
        base.SetFont(Font);
    }

    public override void SetText(string Text, bool SetCaretToEnd = false, bool ClearUndoStates = true)
    {
        if (this.Text != Text)
        {
            if (HasSelection) CancelSelection();
            Text = Text.Replace("\r", "");
            Caret.AtEndOfLine = false;
            Caret.Index = 0;
            this.Text = Text;
            if (HasSelection) CancelSelection();
            if (SetCaretToEnd) Caret.Index = Text.Length;
            this.Parent.ScrolledX = 0;
            this.Parent.ScrolledY = 0;
            ((Widget) this.Parent).UpdateAutoScroll();
            RecalculateLines(true);
            if (ClearUndoStates)
            {
                this.UndoableStates.Clear();
                AddUndoState();
            }
        }
    }

    protected override void RecalculateLines(bool Now = false)
    {
        base.RecalculateLines(Now);
        if (Now) RetokenizeAll();
    }

    public override void SetLineWrapping(bool LineWrapping)
    {
        if (LineWrapping) throw new MethodNotSupportedException(this);
    }

    public override void SetOverlaySelectedText(bool OverlaySelectedText)
    {
        if (!OverlaySelectedText) throw new MethodNotSupportedException(this);
    }

    public override void SetTextColorSelected(Color TextColorSelected)
    {
        throw new MethodNotSupportedException(this);
    }

    protected void RetokenizeLine(int LineIndex)
    {
        // If there exists an entry (a list of tokens) for the current line, then we remove those tokens
        if (LineIndex < LineTokens.Count && LineTokens[LineIndex] != null)
        {
            LineTokens[LineIndex].Clear();
            LineColors[LineIndex].Clear();
        }
        Line line = Lines[LineIndex];
        List<Token> NewLineTokens = Tokenizer.Tokenize(line.Text);
        for (int i = 0; i < NewLineTokens.Count; i++)
        {
            Token t = NewLineTokens[i];
            t.Index += line.StartIndex;
            AddTokenToLine(t, line);
        }
        // Ensures a list exist at this line so we don't retokenize this line in the future.
        AddTokenToLine(null, line);
    }

    protected void ShiftTokens(int StartIndex, int Offset)
    {
        foreach (List<Token> Line in LineTokens)
        {
            // If Line is null, that means we have yet to tokenize this line.
            // As the indices are written on tokenization, which happens with the lastest information,
            // we don't need to remember any shifting of any kind here.
            if (Line == null) continue;
            foreach (Token Token in Line)
            {
                if (Token.Index >= StartIndex) Token.Index += Offset;
            }
        }
    }

    public void TokenizeUntokenizedLines(bool RedrawLines = true)
    {
        List<int> RedrawableLines = new List<int>();
        for (int i = TopLineIndex; i <= BottomLineIndex; i++)
        {
            if (i < Lines.Count)
            {
                if (i >= LineTokens.Count || LineTokens[i] == null)
                {
                    RetokenizeLine(i);
                    RedrawableLines.Add(i);
                }
            }
        }
        if (RedrawLines)
        {
            foreach (int line in RedrawableLines)
            {
                RedrawLine(line);
            }
        }
    }

    protected void AddTokenToLine(Token Token, Line Line)
    {
        if (Line.LineIndex >= LineTokens.Count)
        {
            int diff = Line.LineIndex - LineTokens.Count + 1;
            for (int i = 0; i < diff; i++) LineTokens.Add(null);
        }
        if (Line.LineIndex >= LineColors.Count)
        {
            int diff = Line.LineIndex - LineColors.Count + 1;
            for (int i = 0; i < diff; i++) LineColors.Add(null);
        }
        if (LineTokens[Line.LineIndex] == null) LineTokens[Line.LineIndex] = new List<Token>();
        if (LineColors[Line.LineIndex] == null) LineColors[Line.LineIndex] = new List<(int, Color)>();
        // Used to ensure a list exist at this line index.
        if (Token == null) return;
        LineTokens[Line.LineIndex].Add(Token);
        int pos = Token.Index - Line.StartIndex;
        Color c = Token.Type switch
        {
            "comment" or "begin_multiline_comment" or "end_multiline_comment" => new Color(96, 160, 96),
            "number" or "hex" or "regex" or "string" => new Color(255, 128, 128),
            "assignment" or "symbol" or "empty_method" or "parenthesis_open" or "parenthesis_close" or
            "logical_operator" or "bitwise_operator" or "relational_operator" or "arithmetic_operator" or
            "range" or "object_access" or "line_end" or "ternary_operator" or "array_initialization" or
            "hash_initialization" or "array_access" or "block" or "argument_list" or "constant_access" => new Color(96, 192, 192),
            "class_definition" or "module_definition" or "constant" or "instance_variable" or
            "class_variable" or "global_variable" => new Color(192, 192, 96),
            _ => Token.IsKeyword ? new Color(128, 128, 255) : TextColor
        };
        if (LineColors[Line.LineIndex].Count == 0 || !LineColors[Line.LineIndex].Last().Item2.Equals(c))
        {
            LineColors[Line.LineIndex].Add((pos, c));
        }
        if (Token.Type == "symbol")
        {
            // Since the symbol token matches the colon as well as the name, we reset its color ourselves after 1 character, the colon.
            LineColors[Line.LineIndex].Add((pos + 1, TextColor));
        }
    }

    protected void RetokenizeAll()
    {
        LineTokens.Clear();
        LineColors.Clear();
        for (int i = TopLineIndex; i <= BottomLineIndex; i++)
        {
            if (i < Lines.Count) RetokenizeLine(i);
        }
        /* We could fully tokenize the entire script as below, but this is horribly inefficient.
         * Instead, we can also do it line-by-line, and on an as-needed basis, i.e. tokenize
         * a line whenever it is about to be displayed. This is the same strategy as additions and deletions
         * in the text; we don't retokenize the entire script, only the affected lines.
         * That means we cannot have multi-line tokens, but those are only relevant for multi-line strings and comments
         * and the like, which are rather uncommon in Ruby.
         * If these are strictly necessary, workarounds can be created to work with these. I don't think it's worth the effort though.*/
        TokenizeUntokenizedLines(false);
        RedrawText();
    }

    (int Indentation, bool IsBlank) GetLineIndentation(int LineIndex)
    {
        if (LineIndex < 0) throw new Exception("Line Index is less than 0.");
        if (LineIndex >= Lines.Count) throw new Exception("Line Index is more than the number of lines.");
        int Count = Lines[LineIndex].Text.Length - 1;
        bool IsBlank = true;
        for (int i = 0; i < Lines[LineIndex].Text.Length; i++)
        {
            char c = Lines[LineIndex].Text[i];
            if (c != ' ' && c != '\n')
            {
                Count = i;
                IsBlank = false;
                break;
            }
        }
        return (Count, IsBlank);
    }

    (string Indentation, int SplitIndex) GetIndentation()
    {
        int Count = 0;
        if (Caret.Line.LineIndex > 0)
        {
            int LineIndex = Caret.Line.LineIndex;
            while (LineIndex >= 0)
            {
                (int Indt, bool IsBlank) = GetLineIndentation(LineIndex);
                if (!IsBlank)
                {
                    Count = Indt;
                    break;
                }
                LineIndex--;
            }
        }
        int OriginalCount = Count;
        List<Token> Line = LineTokens[Caret.Line.LineIndex].FindAll(t => t.Index + t.Length <= Caret.Index);
        Token? FirstToken = Line.Count > 0 ? Line[0] : null;
        Token? FirstKeyword = Line.Find(t => t.IsKeyword);
        Token? LastToken = Line.Count > 0 ? Line[^1] : null;
        Token? NextToken = Caret.Line.LineIndex < LineTokens.Count - 1 && LineTokens[Caret.Line.LineIndex + 1].Count > 0 ? LineTokens[Caret.Line.LineIndex + 1][0] : null;
        if (Line.Any(t => t.Type == "class" || t.Type == "module" || t.Type == "def" || t.Type == "begin" || t.Type == "for" ||
                     t.Type == "while" || t.Type == "until" || t.Type == "when" || t.Type == "else" || t.Type == "elsif" ||
                     t.Type == "retry" || t.Type == "ensure"))
        {
            Count += 2;
        }
        else if (FirstToken != null && (FirstToken.Type == "if" || FirstToken.Type == "unless" || FirstToken.Type == "rescue"))
        {
            Count += 2;
        }
        else if (Line.Any(t => t.Type == "do") && !Line.Any(t => t.Type == "end"))
        {
            Count += 2;
        }
        // Now determine the split index of the indentation, i.e. if no characters exist on the left of our caret on this line,
        // then the indentation will be split based on how many spaces are in between the line start and the caret to smoothly make it drop down a line while maintaining the same caret positioning on the x axis.
        int SplitIndex = 0;
        foreach (char c in Caret.Line.Text)
        {
            if (c == ' ') SplitIndex++;
            else break;
        }
        if (SplitIndex > Count) Count = SplitIndex;
        SplitIndex -= Caret.IndexInLine;
        string str = "";
        for (int i = 0; i < Count; i++) str += " ";
        return (str, SplitIndex);
    }

    public override void TextInput(TextEventArgs e)
    {
        string text = this.Text;
        if (!string.IsNullOrEmpty(e.Text))
        {
            if (e.Text == "\n" && Input.Press(Keycode.CTRL)) return;
            string NewText = e.Text;
            if (e.Text == "\n")
            {
                (string Indentation, int SplitIndex) = GetIndentation();
                NewText = "";
                if (SplitIndex <= 0)
                {
                    NewText += e.Text + Indentation;
                }
                else
                {
                    if (SplitIndex > 0) NewText += Indentation.Substring(0, SplitIndex);
                    NewText += e.Text;
                    if (Indentation.Length - SplitIndex > 0) NewText += Indentation.Substring(0, Indentation.Length - SplitIndex);
                }
            }
            if (e.Text != "\n" && InsertMode)
            {
                int DistanceToNewline = -1;
                for (int i = Caret.Index; i < Text.Length; i++)
                {
                    if (Text[i] == '\n')
                    {
                        DistanceToNewline = i - Caret.Index;
                        break;
                    }
                }
                if (DistanceToNewline > 0)
                {
                    RemoveText(Caret.Index, Math.Min(DistanceToNewline, NewText.Length));
                }
                else if (DistanceToNewline == -1)
                {
                    RemoveText(Caret.Index, Math.Min(Text.Length - Caret.Index, NewText.Length));
                }
            }
            InsertText(Caret.Index, NewText);
        }
        else if (e.Backspace || e.Delete)
        {
            if (HasSelection) DeleteSelection();
            else
            {
                if (e.Delete)
                {
                    if (Caret.Index < this.Text.Length)
                    {
                        int Count = 1;
                        if (Input.Press(Keycode.CTRL)) Count = TextArea.FindNextCtrlIndex(this.Text, this.Caret.Index, false) - Caret.Index;
                        RemoveText(this.Caret.Index, Count);
                    }
                }
                else
                {
                    int Count = 1;
                    if (Input.Press(Keycode.CTRL)) Count = Caret.Index - TextArea.FindNextCtrlIndex(this.Text, this.Caret.Index, true);
                    RemoveText(this.Caret.Index - Count, Count);
                }
            }
        }
        else if (e.Tab)
        {
            TabInput();
        }
        if (this.Text != text) OnTextChanged?.Invoke(new BaseEventArgs());
    }

    protected List<Token> GetTokens(Line Line)
    {
        return LineTokens[Line.LineIndex];
    }

    protected void RedrawLineNumbers()
    {
        Sprites["nums"].Bitmap?.Dispose();
        if (LineTextWidth < 1) return;
        Sprites["nums"].Bitmap = new Bitmap(LineTextWidth, Parent.Size.Height);
        Sprites["nums"].Bitmap.Unlock();
        Sprites["nums"].Bitmap.FillRect(0, 0, LineTextWidth - 1, Parent.Size.Height, LineTextBackgroundColor);
        Sprites["nums"].Bitmap.DrawLine(LineTextWidth - 1, 0, LineTextWidth - 1, Parent.Size.Height - 1, new Color(86, 108, 134));
        Sprites["nums"].Bitmap.Font = this.Font;
        int offset = Parent.ScrolledY % (LineHeight + LineMargins);
        for (int i = TopLineIndex; i <= BottomLineIndex; i++)
        {
            if (i >= Lines.Count) break;
            int y = -offset + (i - TopLineIndex) * (LineHeight + LineMargins);
            Sprites["nums"].Bitmap.DrawText((i + 1).ToString(), LineTextWidth - 3, y, LineTextColor, DrawOptions.RightAlign);
        }
        Sprites["nums"].Bitmap.Lock();
    }

    protected override void OwnUpdate()
    {
        if (!SelectedWidget)
        {
            if (EnteringText) WidgetDeselected(new BaseEventArgs());
            if (Sprites["caret"].Visible) Sprites["caret"].Visible = false;
        }
        if (OldScrolledY != Parent.ScrolledY) AdjustLinesForScroll();
        if (RequireRecalculation) RecalculateLines(true);
        if (RequireRedrawText) RedrawText(true);
        if (RequireCaretRepositioning)
        {
            if (Sprites["caret"].X - Parent.ScrolledX >= Parent.Size.Width)
            {
                Parent.ScrolledX += (Sprites["caret"].X - Parent.ScrolledX) - Parent.Size.Width + 1;
                ((Widget) Parent).UpdateAutoScroll();
            }
            else if (Sprites["caret"].X - Parent.ScrolledX < TextXOffset)
            {
                Parent.ScrolledX += TextXOffset + Sprites["caret"].X - Parent.ScrolledX - 1;
                ((Widget) Parent).UpdateAutoScroll();
            }
            if (Sprites["caret"].Y < 0) ScrollUpPixels(-Sprites["caret"].Y);
            if (Sprites["caret"].Y >= Parent.Size.Height) ScrollDownPixels(Sprites["caret"].Y - Parent.Size.Height + LineHeight + LineMargins);
            RequireCaretRepositioning = false;
        }
        if (RequireScrollAdjustment) AdjustLinesForScroll(true);
        if (TimerPassed("idle"))
        {
            Sprites["caret"].Visible = !Sprites["caret"].Visible;
            ResetTimer("idle");
        }
        if (TimerPassed("state"))
        {
            ResetTimer("state");
            // Text changes since last timer passage of "state",
            // so we add the current state
            if (this.Text != UndoableStates.Last().Text) AddUndoState();
        }
        if (!RequireScrollAdjustment)
        {
            OldScrolledY = Parent.ScrolledY;
            OldLineCount = this.Lines.Count;
        }
    }

    protected override void RedrawText(bool Now = false)
    {
        if (!Now)
        {
            RequireRedrawText = true;
            return;
        }
        LineSprites.ForEach(s => s.Dispose());
        LineSprites.Clear();
        SelBoxSprites.ForEach(s => s.Dispose());
        SelBoxSprites.Clear();
        foreach (KeyValuePair<string, Sprite> kvp in Sprites)
        {
            if (kvp.Key.StartsWith("line") || kvp.Key.StartsWith("box")) Sprites.Remove(kvp.Key);
        }
        if (this.Font == null) return;
        int mc = (Lines.Count - 1) * LineMargins;
        int h = Lines.Count * LineHeight + mc + 3;
        if (h >= Parent.Size.Height) h += (h - Parent.Size.Height) % LineHeight;
        if (h % LineHeight != 0) h += (LineHeight + LineMargins) - (h % (LineHeight + LineMargins));
        if (LineWrapping) SetHeight(h);
        else SetSize(TextXOffset + Lines.Max(l => l.LineWidth) + 3, h);
        Lines.ForEach(line =>
        {
            if (line.LineIndex < TopLineIndex || line.LineIndex > BottomLineIndex) return;
            CreateLineSprite(line);
        });
        RequireRedrawText = false;
        RedrawLineNumbers();
        UpdateCaretPosition(false);
        UpdateBounds();
    }

    protected override void RedrawSelectionBoxes()
    {
        foreach (Sprite boxsprite in SelBoxSprites)
        {
            foreach (string key in Sprites.Keys)
            {
                if (Sprites[key] == boxsprite)
                {
                    Sprites.Remove(key);
                    break;
                }
            }
            boxsprite.Dispose();
        }
        SelBoxSprites.Clear();
        if (!HasSelection) return;
        int startidx = SelectionLeft.Line.LineIndex;
        int endidx = SelectionRight.Line.LineIndex;
        for (int i = startidx; i <= endidx; i++)
        {
            if (i < TopLineIndex || i > BottomLineIndex) continue;
            Line line = Lines[i];
            int boxy = line.LineIndex * LineHeight + line.LineIndex * LineMargins - Parent.ScrolledY;
            int boxheight = LineHeight + 2;
            if (HasSelection)
            {
                // Since the font rendering has a lot of internal garbage related to positioning
                // like kerning, just summing the size of the glyphs will make the selected text
                // appear to shift. That's why instead of only drawing that selection, we draw the
                // full text in order to get identical kerning, and then we just draw the selected
                // part of that text. This way the text position will always be the same as the original text.
                // This is only necessary if there is non-selected text before the selected text, as that determines
                // the location of the selected text.
                if (SelectionLeft.Line.LineIndex == line.LineIndex && SelectionRight.Line.LineIndex == line.LineIndex)
                {
                    // Selection starts and ends on this line
                    int x = SelectionLeft.Line.WidthUpTo(SelectionLeft.IndexInLine);
                    int w = SelectionRight.Line.WidthUpTo(SelectionRight.IndexInLine) - x;
                    // Another failsafe for some specific kerning
                    if (x + w > line.LineWidth) w = line.LineWidth - x;
                    Sprite s = new Sprite(this.Viewport);
                    s.X = TextXOffset + x;
                    s.Y = boxy;
                    s.Z = 1;
                    s.Bitmap = new SolidBitmap(w, boxheight, SelectionBackgroundColor);
                    SelBoxSprites.Add(s);
                    Sprites[$"box{line.LineIndex}"] = s;
                }
                else if (SelectionLeft.Line.LineIndex < line.LineIndex && SelectionRight.Line.LineIndex == line.LineIndex)
                {
                    // Selection ends on this line
                    int w = SelectionRight.Line.WidthUpTo(SelectionRight.IndexInLine);
                    if (w > line.LineWidth) w = line.LineWidth;
                    Sprite s = new Sprite(this.Viewport);
                    s.X = TextXOffset;
                    s.Y = boxy;
                    s.Z = 1;
                    s.Bitmap = new SolidBitmap(w, boxheight, SelectionBackgroundColor);
                    SelBoxSprites.Add(s);
                    Sprites[$"box{line.LineIndex}"] = s;
                }
                else if (SelectionLeft.Line.LineIndex == line.LineIndex && SelectionRight.Line.LineIndex > line.LineIndex)
                {
                    // Selection starts on this line
                    int x = SelectionLeft.Line.WidthUpTo(SelectionLeft.IndexInLine);
                    int w = Font.TextSize(line.Text.Replace('\n', ' ')).Width - SelectionLeft.Line.WidthUpTo(SelectionLeft.IndexInLine);
                    if (x + w > line.LineWidth) w = line.LineWidth - x;
                    if (w > 0)
                    {
                        Sprite s = new Sprite(this.Viewport);
                        s.X = TextXOffset + x;
                        s.Y = boxy;
                        s.Z = 1;
                        s.Bitmap = new SolidBitmap(w, boxheight, SelectionBackgroundColor);
                        SelBoxSprites.Add(s);
                        Sprites[$"box{line.LineIndex}"] = s;
                    }
                }
                else if (SelectionLeft.Line.LineIndex < line.LineIndex && SelectionRight.Line.LineIndex > line.LineIndex)
                {
                    // Full line is selected
                    int w = Font.TextSize(line.Text.Replace('\n', ' ')).Width;
                    if (w > line.LineWidth) w = line.LineWidth;
                    Sprite s = new Sprite(this.Viewport);
                    s.X = TextXOffset;
                    s.Y = boxy;
                    s.Z = 1;
                    s.Bitmap = new SolidBitmap(w, LineHeight + 2, SelectionBackgroundColor);
                    SelBoxSprites.Add(s);
                    Sprites[$"box{line.LineIndex}"] = s;
                }
            }
        }
        UpdateBounds();
    }

    protected Sprite CreateLineSprite(Line line)
    {
        Sprite sprite = new Sprite(this.Viewport);
        sprite.X = TextXOffset;
        sprite.Y = line.LineIndex * LineHeight + line.LineIndex * LineMargins - Parent.ScrolledY;
        sprite.Bitmap = new Bitmap(line.LineWidth, LineHeight + 2, Graphics.MaxTextureSize);
        if (TextXOffset + line.LineWidth > Size.Width) SetWidth(TextXOffset + line.LineWidth);
        sprite.Bitmap.Font = Font;
        string text = line.Text.Replace('\n', ' ');
        if (Font != null && !string.IsNullOrEmpty(text))
        {
            sprite.Bitmap.Unlock();
            List<(int pos, Color color)> colors = line.LineIndex < LineColors.Count ? LineColors[line.LineIndex] : null;
            if (colors != null && colors.Count > 0)
            {
                int x = 0;
                void draw_text(string str, Color color)
                {
                    int w = sprite.Bitmap.TextSize(str).Width;
                    sprite.Bitmap.DrawText(str, x, 0, color);
                    x += w;
                }
                if (colors[0].pos > 0) draw_text(text.Substring(0, colors[0].pos), this.TextColor);
                for (int i = 0; i < colors.Count; i++)
                {
                    int startidx = colors[i].pos;
                    int len = (i == colors.Count - 1 ? text.Length : colors[i + 1].pos) - startidx;
                    draw_text(text.Substring(startidx, len), colors[i].color);
                }
            }
            else sprite.Bitmap.DrawText(text, this.TextColor, this.DrawOptions);
            sprite.Bitmap.Lock();
        }
        if (Sprites.ContainsKey($"line{line.LineIndex}")) throw new Exception("Index already in use");
        Sprites[$"line{line.LineIndex}"] = sprite;
        LineSprites.Add(sprite);
        return sprite;
    }

    protected void DisposeLineSprite(int LineIndex)
    {
        if (!Sprites.ContainsKey($"line{LineIndex}")) return;
        Sprites[$"line{LineIndex}"].Dispose();
        LineSprites.Remove(Sprites[$"line{LineIndex}"]);
        if (Sprites.ContainsKey($"box{LineIndex}"))
        {
            Sprites[$"box{LineIndex}"].Dispose();
            SelBoxSprites.Remove(Sprites[$"box{LineIndex}"]);
            Sprites.Remove($"box{LineIndex}");
        }
        Sprites.Remove($"line{LineIndex}");
    }

    protected void RedrawLine(int LineIndex)
    {
        DisposeLineSprite(LineIndex);
        if (LineIndex < Lines.Count) CreateLineSprite(Lines[LineIndex]);
    }

    protected void AdjustLinesForScroll(bool Now = false)
    {
        if (!Now)
        {
            RequireScrollAdjustment = true;
            return;
        }
        // Reposition all on-screen lines
        int count = Math.Max(OldLineCount, Lines.Count);
        for (int i = 0; i < count; i++)
        {
            // For all existing lines within view
            if (i >= TopLineIndex && i <= BottomLineIndex && i < Lines.Count)
            {
                Line line = Lines[i];
                // Create the line sprite if it doesn't exist
                if (!Sprites.ContainsKey($"line{line.LineIndex}"))
                {
                    CreateLineSprite(line);
                }
                else
                {
                    // Or update its position if it does exist
                    Sprite sprite = Sprites[$"line{line.LineIndex}"];
                    sprite.Y = line.LineIndex * LineHeight + line.LineIndex * LineMargins - Parent.ScrolledY;
                    Sprite boxsprite = Sprites.ContainsKey($"box{line.LineIndex}") ? Sprites[$"box{line.LineIndex}"] : null;
                    if (boxsprite != null) boxsprite.Y = sprite.Y;
                }
            }
            // Ensure we have no sprites that are no longer within reach
            // Already handled by the two LinesLost cases above,
            // but a line can fall through the cracks sometimes (think very fast scrolling)
            else if (Sprites.ContainsKey($"line{i}")) DisposeLineSprite(i);
        }
        TokenizeUntokenizedLines();
        // Reposition caret
        Sprites["caret"].Y = Caret.Line.LineIndex * LineHeight + Caret.Line.LineIndex * LineMargins - Parent.ScrolledY;
        RedrawSelectionBoxes();
        RequireScrollAdjustment = false;
        OldScrolledY = Parent.ScrolledY;
        RedrawLineNumbers();
    }

    protected override void ScrollDownPixels(int px)
    {
        base.ScrollDownPixels(px);
        AdjustLinesForScroll();
    }

    protected override void ScrollUpPixels(int px)
    {
        base.ScrollUpPixels(px);
        AdjustLinesForScroll();
    }

    protected override void UpdateCaretPosition(bool ResetScroll)
    {
        Console.WriteLine(Caret.Index);
        Sprites["caret"].X = TextXOffset + Caret.Line.WidthUpTo(Caret.IndexInLine);
        Sprites["caret"].Y = Caret.Line.LineIndex * LineHeight + Caret.Line.LineIndex * LineMargins - Parent.ScrolledY;
        if (ResetScroll) RequireCaretRepositioning = true;
    }

    protected override bool MouseInsideTextArea(MouseEventArgs e)
    {
        return base.MouseInsideTextArea(e) && e.X - Viewport.X + LeftCutOff >= TextXOffset;
    }

    protected override CaretIndex GetHoveredIndex(MouseEventArgs e)
    {
        int rx = e.X - Viewport.X + LeftCutOff - TextXOffset;
        int ry = e.Y - Viewport.Y + Parent.ScrolledY;
        int LineIndex = (int) Math.Round((double) (ry - Font.Size / 2) / (LineHeight + LineMargins));
        if (LineIndex < 0) LineIndex = 0;
        if (LineIndex >= Lines.Count) LineIndex = Lines.Count - 1;
        Line Line = Lines[LineIndex];
        if (Line.Length == 0) return new CaretIndex(this) { Index = Line.StartIndex };
        if (rx >= Line.LineWidth)
        {
            return new CaretIndex(this) { Index = Line.EndIndex + (Line.EndsInNewline ? 0 : 1), AtEndOfLine = !Line.EndsInNewline };
        }
        else if (rx < 0)
        {
            return new CaretIndex(this) { Index = Line.StartIndex, AtEndOfLine = false };
        }
        int idx = Line.StartIndex + Line.GetIndexAroundWidth(rx);
        return new CaretIndex(this) { Index = idx, AtEndOfLine = !Line.EndsInNewline && idx == Line.EndIndex + 1 };
    }

    protected override void InsertText(int Index, string Text)
    {
        Text = Text.Replace("\r", "");
        if (Text.Length == 0) return;
        SetPreviousViewState();
        if (HasSelection)
        {
            int count = SelectionRight.Index - SelectionLeft.Index;
            if (Index > SelectionLeft.Index) Index -= count;
            DeleteSelection();
        }
        bool InsertedNewlines = Text.Contains('\n');
        int IndexOfLine = Caret.IndexInLine;
        int LineIndex = Caret.Line.LineIndex;
        int StartIndex = Caret.Index;
        this.Text = this.Text.Insert(Index, Text);
        if (Index <= Caret.Index) Caret.Index += Text.Length;
        if (Text == "\n") Caret.AtEndOfLine = false;
        int EndIndex = Caret.Index;
        ResetIdle();
        ShiftTokens(StartIndex, EndIndex - StartIndex);
        if (!InsertedNewlines)
        {
            // Inserted a character on this line (or pasted text without any newlines)
            UpdateLineText(LineIndex, Lines[LineIndex].Text.Insert(IndexOfLine, Text));
            RetokenizeLine(LineIndex);
            RedrawLine(LineIndex);
            UpdateCaretPosition(true);
        }
        else
        {
            // Inserted a newline (or pasted longer text with newlines)
            // If we paste text that is "word1\nword2", then "word1" is added to the current line,
            // and then we insert our new text.
            int NewLineIndex = Text.IndexOf('\n') + 1;
            // We also include the newline character in this current line's text, so + 1.
            string NewCurrentLineText = Lines[LineIndex].Text.Substring(0, IndexOfLine) + Text.Substring(0, NewLineIndex);
            // The text we insert below this line is the cut-off text from this line + whatever we insert
            string TextToInsertBelow = Text.Substring(NewLineIndex) + Lines[LineIndex].Text.Substring(IndexOfLine, Lines[LineIndex].Text.Length - IndexOfLine);
            // Update the current line
            if (Lines[LineIndex].Text != NewCurrentLineText)
            {
                // Only update if the line changed, the line did not change
                // if we insert something that starts with a newline for instance
                UpdateLineText(LineIndex, NewCurrentLineText);
                RetokenizeLine(LineIndex);
                RedrawLine(LineIndex);
            }
            // Insert a line for each newline character we find
            int startidx = Lines[LineIndex].StartIndex + Lines[LineIndex].Length;
            int stridx = 0;
            int line = LineIndex + 1;
            for (int i = 0; i < TextToInsertBelow.Length; i++)
            {
                if (TextToInsertBelow[i] == '\n' || i == TextToInsertBelow.Length - 1)
                {
                    string linetext = TextToInsertBelow.Substring(stridx, i - stridx + 1);
                    InsertLine(line, startidx, linetext);
                    CreateLineSprite(Lines[line]);
                    startidx += linetext.Length;
                    stridx = i + 1;
                    line++;
                }
            }
            if (TextToInsertBelow.Length == 0)
            {
                InsertLine(line, startidx, "");
                CreateLineSprite(Lines[line]);
            }
            // Moves all old lines below where we inserted down to their new correct position
            AdjustLinesForScroll(true);
            UpdateCaretPosition(true);
            UpdateHeight();
            RedrawLineNumbers();
        }
        AddUndoState();
        VerifyLines();
    }

    protected override void RemoveText(int Index, int Count)
    {
        if (Index < 0) return;
        Count = Math.Min(Text.Length - Index, Count);
        if (Count < 1) return;
        SetPreviousViewState();
        int StartIndex = Caret.Index;
        Text = Text.Remove(Index, Count);
        bool DeletedNewline = Index < Caret.Line.StartIndex || Index + Count > Caret.Line.EndIndex;
        int IndexInLine = Index - Lines[Caret.Line.LineIndex].StartIndex;
        int TopIndex = Index;
        int TopLineIndex = (Lines.Find(l => TopIndex >= l.StartIndex && TopIndex <= l.EndIndex) ?? Lines.Last()).LineIndex;
        int TopIndexInLine = TopIndex - Lines[TopLineIndex].StartIndex;
        int BottomIndex = Index + Count;
        int BottomLineIndex = (Lines.Find(l => BottomIndex >= l.StartIndex && BottomIndex <= l.EndIndex) ?? Lines.Last()).LineIndex;
        int BottomIndexInLine = BottomIndex - Lines[BottomLineIndex].StartIndex;
        bool CaretOnRight = Caret.Index >= Index;
        if (Index <= Caret.Index) Caret.Index = Math.Max(Index, Caret.Index - Count);
        Caret.AtEndOfLine = !Caret.Line.EndsInNewline && Caret.Index == Caret.Line.EndIndex + 1;
        int EndIndex = Caret.Index;
        ShiftTokens(StartIndex, -Count);
        ResetIdle();
        if (!DeletedNewline)
        {
            // Deleted a character on this line (or deleted a selection without any newlines)
            UpdateLineText(TopLineIndex, Lines[TopLineIndex].Text.Remove(IndexInLine, Count));
            RetokenizeLine(TopLineIndex);
            RedrawLine(TopLineIndex);
            UpdateCaretPosition(true);
        }
        else
        {
            // Deleted a newline (or deleted a selection with newlines)
            // We merge the line at the bottom of where we delete into the one at the top,
            // and then delete the lines between (top+1) and (bottom).
            string TopLineNewText = Lines[TopLineIndex].Text.Substring(0, TopIndexInLine) + Lines[BottomLineIndex].Text.Substring(BottomIndexInLine);
            UpdateLineText(TopLineIndex, TopLineNewText);
            RetokenizeLine(TopLineIndex);
            RedrawLine(TopLineIndex);
            // Delete from last to first, otherwise line indexes would shift and we delete the wrong lines
            for (int i = BottomLineIndex; i > TopLineIndex; i--)
            {
                // Delete line
                DeleteLine(i);
            }
            // If the bottom line we'd be displaying is more than the number of lines we have, then we must scroll up by the difference
            if (CaretOnRight)
            {
                // Deleting a large selection with the caret being on the right means
                // that we have to scroll up.
                // So if the caret is on the right/bottom, we scroll up by the number of deleted lines times the line height
                ScrollUp(BottomLineIndex - TopLineIndex);
            }
            // Create any new non-existing line sprites between this.TopLineIndex and this.BottomLineIndex
            AdjustLinesForScroll(true);
            UpdateCaretPosition(true);
            UpdateHeight();
            RedrawLineNumbers();
        }
        AddUndoState();
        VerifyLines();
    }

    protected override void UpdateHeight()
    {
        int mc = (Lines.Count - 1) * LineMargins;
        int h = Lines.Count * LineHeight + mc + 3;
        if (h >= Parent.Size.Height) h += (h - Parent.Size.Height) % LineHeight;
        if (h % LineHeight != 0) h += (LineHeight + LineMargins) - (h % (LineHeight + LineMargins));
        if (LineWrapping) SetHeight(h);
        else SetSize(TextXOffset + Lines.Max(l => l.LineWidth) + 3, h);
    }

    protected void UpdateLineText(int LineIndex, string NewText)
    {
        // A character was added or removed on this line, so we update
        // this line's text and all subsequent line Start indexes to maintain consistency
        Line thisline = Lines[LineIndex];
        int diff = NewText.Length - thisline.Text.Length;
        // diff > 0 -> addition
        // diff < 0 -> deletion
        thisline.SetText(NewText);
        for (int i = LineIndex + 1; i < Lines.Count; i++)
        {
            Lines[i].StartIndex += diff;
        }
    }

    protected void InsertLine(int LineIndex, int StartIndex, string LineText)
    {
        Line line = new Line(this.Font);
        line.StartIndex = StartIndex;
        line.LineIndex = LineIndex;
        line.SetText(LineText);
        for (int i = Lines.Count - 1; i >= LineIndex; i--)
        {
            // Increment the startindex of all lines below where we will insert
            // with the length of our line
            Lines[i].StartIndex += LineText.Length;
            // Line index increases by 1
            Lines[i].LineIndex++;
            // Then increment the keys of all existing line/box sprites by 1
            // to make place for our new line.
            if (Sprites.ContainsKey($"line{i}"))
            {
                Sprite s = Sprites[$"line{i}"];
                Sprites.Remove($"line{i}");
                Sprites.Add($"line{i + 1}", s);
            }
            if (Sprites.ContainsKey($"box{i}"))
            {
                Sprite s = Sprites[$"box{i}"];
                Sprites.Remove($"box{i}");
                Sprites.Add($"box{i + 1}", s);
            }
        }
        // Some line sprites will now fall below the screen too
        // These will be removed by AdjustLinesForScroll()
        Lines.Insert(LineIndex, line);
        LineTokens.Insert(LineIndex, new List<Token>());
        LineColors.Insert(LineIndex, new List<(int, Color)>());
        RetokenizeLine(LineIndex);
    }

    protected void DeleteLine(int LineIndex)
    {
        Line line = Lines[LineIndex];
        DisposeLineSprite(LineIndex);
        for (int i = LineIndex + 1; i < Lines.Count; i++)
        {
            // Decrement the start index of all lines below where we're deleting by the length of the deleted line
            Lines[i].StartIndex -= line.Text.Length;
            // Decrement the line index
            Lines[i].LineIndex--;
            if (Sprites.ContainsKey($"line{i}"))
            {
                Sprite s = Sprites[$"line{i}"];
                Sprites.Remove($"line{i}");
                Sprites.Add($"line{i - 1}", s);
            }
            if (Sprites.ContainsKey($"box{i}"))
            {
                Sprite s = Sprites[$"box{i}"];
                Sprites.Remove($"box{i}");
                Sprites.Add($"box{i - 1}", s);
            }
        }
        Lines.RemoveAt(LineIndex);
        LineTokens.RemoveAt(LineIndex);
        LineColors.RemoveAt(LineIndex);
    }

    protected void VerifyLines()
    {
        for (int i = 0; i < Lines.Count; i++)
        {
            if (Lines[i].Text != this.Text.Substring(Lines[i].StartIndex, Lines[i].Length))
            {
                throw new Exception($"Line {i} text is not the same as in the real text string!");
            }
        }
    }

    protected override void DeleteSelection()
    {
        int start = SelectionLeft.Index;
        int end = SelectionRight.Index;
        int count = end - start;
        CancelSelection();
        RemoveText(start, count);
    }

    protected override void AddUndoState()
    {
        this.UndoableStates.Add(new ScriptEditorState(this));
        this.RedoableStates.Clear();
    }

    protected override void Undo()
    {
        if (UndoableStates.Count < 2) return;
        ScriptEditorState PreviousState = (ScriptEditorState) UndoableStates[UndoableStates.Count - 2];
        PreviousState.Apply();
        RedoableStates.Add(UndoableStates[UndoableStates.Count - 1]);
        UndoableStates.RemoveAt(UndoableStates.Count - 1);
    }

    protected override void Redo()
    {
        if (RedoableStates.Count < 1) return;
        ScriptEditorState PreviousState = (ScriptEditorState) RedoableStates[RedoableStates.Count - 1];
        PreviousState.Apply();
        UndoableStates.Add(PreviousState);
        RedoableStates.RemoveAt(RedoableStates.Count - 1);
    }

    public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDown(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y + Parent.ScrolledY;
        if (rx >= 0 && rx < LineTextWidth && Mouse.Inside)
        {
            // Clicked a line number; selected whole line
            int linenum = ry / (LineHeight + LineMargins);
            if (linenum >= Lines.Count || linenum < 0) return;
            Line line = Lines[linenum];
            SetSelection(line.StartIndex, line.Length + (line.EndsInNewline ? 0 : 1), true);
            SelectedLineNumberOrigin = linenum;
            LastLineNumber = linenum;
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!Mouse.LeftMousePressed || SelectedLineNumberOrigin == -1) return;
        int ry = e.Y - Viewport.Y + Parent.ScrolledY;
        // Clicked a line number; selected whole line
        int linenum = ry / (LineHeight + LineMargins);
        if (linenum >= Lines.Count || linenum < 0) return;
        if (linenum == LastLineNumber && LastLineNumber != -1) return;
        Line line = Lines[linenum];
        Line Origin = Lines[SelectedLineNumberOrigin];
        if (line.LineIndex < Origin.LineIndex)
        {
            SetSelection(line.StartIndex, Origin.StartIndex - line.StartIndex + Origin.Length + (Origin.EndsInNewline ? 0 : 1));
        }
        else if (line.LineIndex > Origin.LineIndex)
        {
            SetSelection(Origin.StartIndex, line.StartIndex - Origin.StartIndex + line.Length + (line.EndsInNewline ? 0 : 1));
        }
        else SetSelection(line.StartIndex, line.Length + (line.EndsInNewline ? 0 : 1));
        LastLineNumber = linenum;
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        SelectedLineNumberOrigin = -1;
        LastLineNumber = -1;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ((SolidBitmap) Sprites["guide"].Bitmap).SetSize(1, Size.Height);
    }

    protected class ScriptEditorState : TextAreaState
    {
        protected new ScriptEditorTextArea TextArea => (ScriptEditorTextArea) base.TextArea;

        public List<Line> Lines;
        public List<List<Token>> LineTokens;
        public List<List<(int, Color)>> LineColors;

        public ScriptEditorState(ScriptEditorTextArea TextArea) : base (TextArea)
        {
            this.Lines = CloneLineList(TextArea.Lines);
            this.LineTokens = CloneLineTokenList(TextArea.LineTokens);
            this.LineColors = CloneLineColorList(TextArea.LineColors);
        }

        protected static List<Line> CloneLineList(List<Line> List)
        {
            return List == null ? null : List.Select(line => (Line) line.Clone()).ToList();
        }

        protected static List<Token> CloneTokenList(List<Token> List)
        {
            return List == null ? null : List.Select(token => (Token) token?.Clone()).ToList();
        }

        protected static List<List<Token>> CloneLineTokenList(List<List<Token>> List)
        {
            return List == null ? null : List.Select(line => line?.Select(token => (Token) token.Clone()).ToList()).ToList();
        }

        protected static List<List<(int, Color)>> CloneLineColorList(List<List<(int, Color)>> List)
        {
            return List == null ? null : List.Select(line => line?.Select(lc => (lc.Item1, (Color) lc.Item2.Clone())).ToList()).ToList();
        }

        public override void Apply()
        {
            List<int> ChangedLines = new List<int>();
            this.TextArea.Caret = (CaretIndex) this.Caret.Clone();
            this.TextArea.Parent.ScrolledX = this.ParentScrolledX;
            this.TextArea.Parent.ScrolledY = this.ParentScrolledY;
            ((Widget) this.TextArea.Parent).UpdateAutoScroll();
            // We've set ScrolledX/ScrolledY, so TextArea.TopLineIndex has been updated to the old value
            // If any lines changed between TopLineIndex and BottomLineIndex, we redraw those
            // Note that this will redraw every line if newlines were involved!
            for (int i = TextArea.TopLineIndex; i <= TextArea.BottomLineIndex; i++)
            {
                if (i < Lines.Count && (TextArea.Lines.Count != Lines.Count || TextArea.Lines[i].Text != Lines[i].Text)) ChangedLines.Add(i);
            }
            this.TextArea.Text = this.Text;
            this.TextArea.Lines = CloneLineList(this.Lines);
            this.TextArea.LineTokens = CloneLineTokenList(this.LineTokens);
            this.TextArea.LineColors = CloneLineColorList(this.LineColors);
            this.TextArea.AdjustLinesForScroll();
            this.TextArea.UpdateCaretPosition(true);
            ChangedLines.ForEach(line => TextArea.RedrawLine(line));
            TextArea.VerifyLines();
        }
    }
}
