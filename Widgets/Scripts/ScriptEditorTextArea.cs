using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

// TODO:
// - Run tokenizer on separate thread on startup
// - Insert to replace character (in base class, MultilineTextArea)
// - Make Undo/Redo system of types insertion and addition and use the InsertText and RemoveText methods,
//   that way only what needs to be redrawn/retokenized will ever be redrawn/retokenized

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

    protected new int BottomLineIndex => base.BottomLineIndex + 1;

    public ScriptEditorTextArea(IContainer Parent) : base(Parent)
    {
        this.OverlaySelectedText = false;
        this.LineWrapping = false;
        SetSelectionBackgroundColor(new Color(128, 128, 255, 64));
        ConsiderInAutoScrollPositioningY = false;
        Sprites["nums"] = new Sprite(this.Viewport);
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
        int sidx = -1;
        // If there exists an entry (a list of tokens) for the current line, then we remove those tokens
        if (LineIndex < LineTokens.Count && LineTokens[LineIndex] != null)
        {
            //foreach (Token lineToken in LineTokens[LineIndex])
            //{
            //    if (sidx == -1)
            //    {
            //        sidx = Tokens.IndexOf(lineToken);
            //    }
            //    Tokens.Remove(lineToken);
            //}
            LineTokens[LineIndex].Clear();
            LineColors[LineIndex].Clear();
        }
        //if (sidx == -1)
        //{
        //    // This line initially didn't have any tokens, so we don't know where to insert
        //    // our tokens in the global token list.
        //    // So instead, start one line above us, and find the last token on that line
        //    // We go upwards until we find a token, and then we start inserting right after
        //    // that token.
        //    // If we don't find any, that means we start at 0.
        //    // This is inefficient, but the odds of a line already having a token
        //    // are very high, and the odds that the line above has at least one token is even higher, and so forth.
        //    Token LastToken = null;
        //    for (int i = LineIndex - 1; i >= 0; i--)
        //    {
        //        if (LineTokens[i].Count > 0)
        //        {
        //            LastToken = LineTokens[i].Last();
        //            break;
        //        }
        //    }
        //    sidx = LastToken == null ? 0 : Tokens.IndexOf(LastToken) + 1;
        //}
        Line line = Lines[LineIndex];
        List<Token> NewLineTokens = Tokenizer.Tokenize(line.Text);
        //Tokens.InsertRange(sidx, NewLineTokens);
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
            // class_definition, module_definition, constant, instance_variable, class_variable, global_variable
            "comment" => new Color(96, 160, 96),
            "number" or "hex" or "regex" or "string" => new Color(255, 128, 128),
            "class" or "def" or "if" or "true" or "false" or "else" or "end" or "begin" or
            "end" or "rescue" or "ensure" or "return" or "next" or "break" or "yield" or
            "alias" or "elsif" or "case" or "when" or "module" or "not" or "and" or "or"
            or "redo" or "retry" or "for" or "undef" or "unless" or "super" or "then" or
            "while" or "until" or "defined?" or "self" or "raise" or "do" => new Color(128, 128, 255),
            "assignment" or "symbol" or "empty_method" or "parenthesis_open" or "parenthesis_close" or
            "logical_operator" or "bitwise_operator" or "relational_operator" or "arithmetic_operator" or
            "range" or "object_access" or "line_end" or "ternary_operator" or "array_initialization" or
            "hash_initialization" or "array_access" or "block" or "argument_list" => new Color(96, 192, 192),
            "class_definition" or "module_definition" or "constant" or "instance_variable" or
            "class_variable" or "global_variable" => new Color(192, 192, 96),
            _ => TextColor
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
         * If these are strictly necessary, workarounds can be created to work with these. I don't think it's worth the effort though.
        Tokens = Tokenizer.Tokenize(this.Text);
        int LastLine = 0;
        for (int i = 0; i < Lines.Count; i++)
        {
            LineTokens.Add(new List<Token>());
            LineColors.Add(new List<(int, Color)>());
        }
        for (int i = 0; i < Tokens.Count; i++)
        {
            Token token = Tokens[i];
            Line Line = null;
            for (int j = LastLine; j < Lines.Count; j++)
            {
                if (token.Index < Lines[j].EndIndex)
                {
                    Line = Lines[j];
                    break;
                }
            }
            // No line was found for this token, meaning this token somehow likely has a bigger index than the full text length.
            // Unsure why this happens, but skip it just to be safe.
            if (Line == null) continue;
            AddTokenToLine(token, Line);
            LastLine = Line.LineIndex;
        }*/
        TokenizeUntokenizedLines(false);
        RedrawText();
    }

    (string Indentation, bool SmartSplit) GetIndentation()
    {
        int Count = 0;
        int LineIndex = 0;
        Token? NextToken = null;
        bool OpenedAccessThisLine = false;
        bool ClosedAccessThisLine = false;
        bool InRegex = false;
        foreach (List<Token> Line in LineTokens)
        {
            OpenedAccessThisLine = false;
            ClosedAccessThisLine = false;
            int TokenIndex = 0;
            if (NextToken != null) break;
            foreach (Token Token in Line)
            {
                if (LineIndex > Caret.Line.LineIndex || Token.Index >= Caret.Index)
                {
                    NextToken = Token;
                    break;
                }
                if (Token.Type == "if" || Token.Type == "rescue" || Token.Type == "unless")
                {
                    if (TokenIndex != 0)
                    {
                        // Line does not start with an if/rescue/unless.
                        // This may mean it is an inline statement, in which case we don't increase the indentation count,
                        // or it could mean it's simply one very long line, in which case we handle it as usual.
                        // So to detect if this is an inline statement, we need to determine if there is an expression or method call preceding
                        // this token. If we find a semi-colon, we know it's not an inline statement, because then it's just some code all on one line.
                        Token PriorToken = Line[TokenIndex - 1];
                        // In these cases: "do if", "; if", "{ if", we know that this can't be an inline statement, so the count must be increased.
                        // If none of these cases apply, we have an inline statement, and we should decrease the count to counteract the increment below.
                        if (!(PriorToken.Type == "do" || PriorToken.Type == "line_end" || PriorToken.Type == "block" && PriorToken.Value == "{"))
                        {
                            // rescue does not increase the count because begin does that, so only decrease the count if the token is not an inline rescue.
                            if (Token.Type != "rescue") Count--;
                        }
                    }
                }
                /*  "comment" => new Color(96, 160, 96),
                    "number" or "hex" or "regex" or "string" => new Color(255, 128, 128),
                    "class" or "def" or "if" or "true" or "false" or "else" or "end" or "begin" or
                    "end" or "rescue" or "ensure" or "return" or "next" or "break" or "yield" or
                    "alias" or "elsif" or "case" or "when" or "module" or "not" or "and" or "or"
                    or "redo" or "retry" or "for" or "undef" or "unless" or "super" or "then" or
                    "while" or "defined?" or "self" or "raise" or "do" => new Color(128, 128, 255),
                    "assignment" or "symbol" or "empty_method" or "parenthesis_open" or "parenthesis_close" or
                    "logical_operator" or "bitwise_operator" or "relational_operator" or "arithmetic_operator" or
                    "range" or "object_access" or "line_end" or "ternary_operator" or "array_initialization" or
                    "hash_initialization" or "array_access" or "block" or "argument_list" => new Color(96, 192, 192),
                    "class_definition" or "module_definition" or "constant" or "instance_variable" or
                    "class_variable" or "global_variable" => new Color(192, 192, 96),*/
                if (Token.Type == "class" || Token.Type == "module" || Token.Type == "def" || Token.Type == "if" || Token.Type == "do" || Token.Type == "unless" ||
                    Token.Type == "begin" || Token.Type == "for" || Token.Type == "while" || Token.Type == "until" || Token.Type == "case" ||
                    !OpenedAccessThisLine && (Token.Type == "block" && Token.Value == "{" || Token.Type == "array_access" && Token.Value == "["))
                {
                    Count++;
                }
                else if (Token.Type == "end" || !ClosedAccessThisLine && (Token.Type == "block" && Token.Value == "}" || Token.Type == "array_access" && Token.Value == "]"))
                {
                    Count--;
                }
                if (Token.Type == "block" && Token.Value == "{" || Token.Type == "array_access" && Token.Value == "[")
                {
                    OpenedAccessThisLine = true;
                    ClosedAccessThisLine = false;
                }
                if (Token.Type == "block" && Token.Value == "}" || Token.Type == "array_access" && Token.Value == "]")
                {
                    ClosedAccessThisLine = true;
                    OpenedAccessThisLine = false;
                }
                TokenIndex++;
            }
            // Since we haven't found an end on the same line as a do, we reset the doline variable and treat the next end as a regular end.
            LineIndex++;
        }
        bool SmartSplit = false;
        if (NextToken != null && Caret.Line.StartIndex <= NextToken.Index && NextToken.Index < Caret.Line.EndIndex) // We are on the same line as the next token
        {
            if (NextToken.Type == "else" || NextToken.Type == "when" || NextToken.Type == "rescue" || NextToken.Type == "retry" || NextToken.Type == "elsif") SmartSplit = true;
        }
        string str = "";
        if (SmartSplit) Count--;
        for (int i = 0; i < Count; i++) str += "  ";
        return (str, SmartSplit);
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
                int linestart = 0;
                foreach (char c in Caret.Line.Text)
                {
                    if (c == '\n' && linestart != Caret.Line.Text.Length - 1 || c == ' ') linestart++;
                    else break;
                }
                (string indentation, bool SmartSplit) = GetIndentation();
                int cdiff = Math.Min(indentation.Length, linestart - Caret.IndexInLine);
                NewText = "";
                if (SmartSplit) NewText += "  ";
                if (cdiff <= 0)
                {
                    NewText += e.Text + indentation;
                }
                else
                {
                    if (cdiff > 0) NewText += indentation.Substring(0, cdiff);
                    NewText += e.Text;
                    if (indentation.Length - cdiff > 0) NewText += indentation.Substring(0, indentation.Length - cdiff);
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

    protected class ScriptEditorState : TextAreaState
    {
        protected new ScriptEditorTextArea TextArea => (ScriptEditorTextArea) base.TextArea;

        public List<Line> Lines;
        public List<Token> Tokens;
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
                if (TextArea.Lines.Count != Lines.Count || TextArea.Lines[i].Text != Lines[i].Text) ChangedLines.Add(i);
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
