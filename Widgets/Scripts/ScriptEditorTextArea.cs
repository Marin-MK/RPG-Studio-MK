using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ScriptEditorTextArea : MultilineTextArea
{
    protected List<Token> Tokens;
    protected List<List<Token>> LineTokens = new List<List<Token>>();
    protected List<List<(int, Color)>> LineColors = new List<List<(int, Color)>>();
    protected int OldScrolledY;
    protected int OldTopIndex;
    protected bool RequireScrollAdjustment = false;

    protected new int BottomLineIndex => base.BottomLineIndex + 1;

    public ScriptEditorTextArea(IContainer Parent) : base(Parent)
    {
        this.OverlaySelectedText = false;
        this.LineWrapping = false;
        SetSelectionBackgroundColor(new Color(128, 128, 255, 64));
        ConsiderInAutoScrollPositioningY = false;
    }

    public override void SetText(string Text, bool SetCaretToEnd = false, bool ClearUndoStates = true)
    {
        if (this.Text != Text)
        {
            base.SetText(Text, SetCaretToEnd, ClearUndoStates);
            RecalculateLines(true);
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
        foreach (Token lineToken in LineTokens[LineIndex])
        {
            if (sidx == -1)
            {
                sidx = Tokens.IndexOf(lineToken);
            }
            Tokens.Remove(lineToken);
        }
        if (sidx == -1)
        {
            // This line initially didn't have any tokens, so we don't know where to insert
            // our tokens in the global token list.
            // So instead, start one line above us, and find the last token on that line
            // We go upwards until we find a token, and then we start inserting right after
            // that token.
            // If we don't find any, that means we start at 0.
            // This is inefficient, but the odds of a line already having a token
            // are very high, and the odds that the line above has at least one token is even higher, and so forth.
            Token LastToken = null;
            for (int i = LineIndex - 1; i >= 0; i--)
            {
                if (LineTokens[i].Count > 0)
                {
                    LastToken = LineTokens[i].Last();
                    break;
                }
            }
            sidx = LastToken == null ? 0 : Tokens.IndexOf(LastToken) + 1;
        }
        LineTokens[LineIndex].Clear();
        LineColors[LineIndex].Clear();
        Line line = Lines[LineIndex];
        List<Token> NewLineTokens = Tokenizer.Tokenize(line.Text);
        Tokens.InsertRange(sidx, NewLineTokens);
        for (int i = 0; i < NewLineTokens.Count; i++)
        {
            Token t = NewLineTokens[i];
            t.Index += line.StartIndex;
            AddTokenToLine(t, line);
        }
    }

    protected void AddTokenToLine(Token Token, Line Line)
    {
        LineTokens[Line.LineIndex].Add(Token);
        int pos = Token.Index - Line.StartIndex;
        Color c = Token.Type switch
        {
            "comment" => new Color(64, 64, 64),
            "string" => new Color(128, 255, 128),
            "number" or "hex" or "regex" => new Color(255, 128, 128),
            "class" or "def" or "if" or "true" or "false" or "else" or "end" or "begin" or
            "end" or "rescue" or "ensure" or "return" or "next" or "break" or "yield" or
            "alias" or "elsif" or "case" or "when" or "module" or "not" or "and" or "or"
            or "redo" or "retry" or "for" or "undef" or "unless" or "super" or "then" or
            "while" or "defined?" or "self" or "raise" or "do" => new Color(128, 128, 255),
            "constant" or "class_definition" or "module_definition" => new Color(255, 255, 128),
            "instance_variable" => new Color(128, 255, 128),
            _ => new Color(255, 255, 255)
        };
        if (LineColors[Line.LineIndex].Count == 0 || !LineColors[Line.LineIndex].Last().Item2.Equals(c))
            LineColors[Line.LineIndex].Add((pos, c));
    }

    protected void RetokenizeAll()
    {
        Console.WriteLine("retokenizing all");
        LineTokens.Clear();
        LineColors.Clear();
        Tokens = Tokenizer.Tokenize(this.Text);
        int LastLine = 0;
        for (int i = 0; i < Tokens.Count; i++)
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
            AddTokenToLine(token, Line);
            LastLine = Line.LineIndex;
        }
        RedrawText();
    }

    protected List<Token> GetTokens(Line Line)
    {
        return LineTokens[Line.LineIndex];
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
        if (RequireScrollAdjustment) AdjustLinesForScroll(true);
        if (RequireCaretRepositioning)
        {
            if (Sprites["caret"].X - Parent.ScrolledX >= Parent.Size.Width)
            {
                Parent.ScrolledX += (Sprites["caret"].X - Parent.ScrolledX) - Parent.Size.Width + 1;
                ((Widget) Parent).UpdateAutoScroll();
            }
            else if (Sprites["caret"].X - Parent.ScrolledX < 0)
            {
                Parent.ScrolledX += Sprites["caret"].X - Parent.ScrolledX - 1;
                ((Widget) Parent).UpdateAutoScroll();
            }
            if (Sprites["caret"].Y < 0) ScrollUpPixels(-Sprites["caret"].Y);
            if (Sprites["caret"].Y >= Parent.Size.Height) ScrollDownPixels(Sprites["caret"].Y - Parent.Size.Height + LineHeight + LineMargins);
            RequireCaretRepositioning = false;
        }
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
            OldTopIndex = TopLineIndex;
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
        else SetSize(Lines.Max(l => l.LineWidth) + 3, h);
        Console.WriteLine("redrawing all");
        Lines.ForEach(line =>
        {
            if (line.LineIndex < TopLineIndex || line.LineIndex > BottomLineIndex) return;
            CreateLineSprite(line);
        });
        RequireRedrawText = false;
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
                    s.X = x;
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
                        s.X = x;
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
        sprite.Y = line.LineIndex * LineHeight + line.LineIndex * LineMargins - Parent.ScrolledY;
        sprite.Bitmap = new Bitmap(line.LineWidth, LineHeight + 2);
        sprite.Bitmap.Font = Font;
        sprite.Bitmap.Unlock();
        string text = line.Text.Replace('\n', ' ');
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
        if (!Sprites.ContainsKey($"line{LineIndex}")) throw new Exception("Cannot redraw non-existing line");
        DisposeLineSprite(LineIndex);
        CreateLineSprite(Lines[LineIndex]);
    }

    protected void AdjustLinesForScroll(bool Now = false)
    {
        if (!Now)
        {
            RequireScrollAdjustment = true;
            return;
        }
        int NewTopIndex = TopLineIndex;
        int LinesLost = NewTopIndex - OldTopIndex;
        int LineCount = BottomLineIndex - TopLineIndex;
        if (LinesLost < 0)
        {
            // Scrolled up, remove lines at the bottom
            LinesLost = -LinesLost;
            for (int i = 0; i < LinesLost; i++)
            {
                int idx = NewTopIndex + LineCount + i;
                DisposeLineSprite(idx);
            }
            // Create new lines at the top
            for (int i = NewTopIndex; i <= OldTopIndex; i++)
            {
                if (i >= 0 && !Sprites.ContainsKey($"line{i}")) CreateLineSprite(Lines[i]);
            }
            // Fully redraw selection boxes in case a selection has now come within bounds
            RedrawSelectionBoxes();
        }
        else if (LinesLost > 0)
        {
            // Scrolled down, remove lines at the top
            for (int i = 0; i < LinesLost; i++)
            {
                int idx = OldTopIndex + i;
                DisposeLineSprite(idx);
            }
            // Create new lines at the bottom
            for (int i = BottomLineIndex - LinesLost; i <= BottomLineIndex; i++)
            {
                if (i < Lines.Count && !Sprites.ContainsKey($"line{i}")) CreateLineSprite(Lines[i]);
            }
            // Fully redraw selection boxes in case a selection has now come within bounds
            RedrawSelectionBoxes();
        }
        // Reposition all on-screen lines
        for (int i = 0; i < Lines.Count; i++)
        {
            Line line = Lines[i];
            if (Sprites.ContainsKey($"line{line.LineIndex}"))
            {
                Sprite sprite = Sprites[$"line{line.LineIndex}"];
                sprite.Y = line.LineIndex * LineHeight + line.LineIndex * LineMargins - Parent.ScrolledY;
                Sprite boxsprite = Sprites.ContainsKey($"box{line.LineIndex}") ? Sprites[$"box{line.LineIndex}"] : null;
                if (boxsprite != null) boxsprite.Y = sprite.Y;
            }
            // Ensure we have no sprites that are no longer within reach
            // Already handled by the two LinesLost cases above,
            // but a line can fall through the cracks sometimes (think very fast scrolling)
            else DisposeLineSprite(i);
        }
        // Reposition caret
        Sprites["caret"].Y = Caret.Line.LineIndex * LineHeight + Caret.Line.LineIndex * LineMargins - Parent.ScrolledY;
        RequireScrollAdjustment = false;
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
        base.UpdateCaretPosition(ResetScroll);
        Sprites["caret"].Y -= Parent.ScrolledY;
    }

    protected override CaretIndex GetHoveredIndex(MouseEventArgs e)
    {
        int rx = e.X - Viewport.X + LeftCutOff;
        int ry = e.Y - Viewport.Y + Parent.ScrolledY;
        int LineIndex = ry / (LineHeight + LineMargins);
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
        this.Text = this.Text.Insert(Index, Text);
        if (Index <= Caret.Index) Caret.Index += Text.Length;
        if (Text == "\n") Caret.AtEndOfLine = false;
        AddUndoState();
        ResetIdle();
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
            // Moves all old lines below where we inserted down to their new correct position
            AdjustLinesForScroll();
            UpdateCaretPosition(true);
        }
    }

    protected override void RemoveText(int Index, int Count)
    {
        if (Index < 0) return;
        Count = Math.Min(Text.Length - Index, Count);
        if (Count < 1) return;
        SetPreviousViewState();
        Text = Text.Remove(Index, Count);
        bool DeletedNewline = Index < Caret.Line.StartIndex || Index + Count > Caret.Line.EndIndex;
        int LineIndex = Caret.Line.LineIndex;
        int IndexOfLine = Index - Lines[LineIndex].StartIndex;
        if (Index <= Caret.Index) Caret.Index = Math.Max(Index, Caret.Index - Count);
        Caret.AtEndOfLine = !Caret.Line.EndsInNewline && Caret.Index == Caret.Line.EndIndex + 1;
        AddUndoState();
        ResetIdle();
        if (!DeletedNewline)
        {
            // Deleted a character on this line (or deleted a selection without any newlines)
            UpdateLineText(LineIndex, Lines[LineIndex].Text.Remove(IndexOfLine, Count));
            RetokenizeLine(LineIndex);
            RedrawLine(LineIndex);
            UpdateCaretPosition(true);
        }
        else
        {
            // Deleted a newline (or deleted a selection with newlines)
            RecalculateLines();
        }
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

    protected override void DeleteSelection()
    {
        int start = SelectionLeft.Index;
        int end = SelectionRight.Index;
        int count = end - start;
        RemoveText(start, count);
        CancelSelection();
    }
}
