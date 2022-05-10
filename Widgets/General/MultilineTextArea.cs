using System;
using System.Collections.Generic;
using System.Linq;
using static odl.SDL2.SDL;

namespace RPGStudioMK.Widgets;

public class MultilineTextArea : Widget
{
    public string Text { get; protected set; } = "";
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public bool ReadOnly { get; protected set; } = false;
    public int CaretHeight { get; protected set; } = 13;

    public Index Caret = new Index(0);
    public Index SelectionStartIndex = new Index(-1);
    public Index SelectionEndIndex = new Index(-1);

    bool EnteringText = false;
    bool DrawnText = false;
    int VerticalIndex = 0;

    int MinIdx = -1;
    int MaxIdx = -1;
    bool SnapToWords = false;

    List<Sprite> SelectionFillers = new List<Sprite>();

    public BaseEvent OnTextChanged;

    public MultilineTextArea(IContainer Parent) : base(Parent)
    {
        this.Font = Fonts.ProductSansMedium.Use(12);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["caret"] = new Sprite(this.Viewport, new SolidBitmap(1, 16, Color.WHITE));
        Sprites["caret"].Y = 2;
        Sprites["caret"].Z = 1;
        OnWidgetSelected += WidgetSelected;
        OnDisposed += delegate (BaseEventArgs e)
        {
            this.Window.UI.SetSelectedWidget(null);
            Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        };
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text ?? "";
            RedrawText();
        }
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            RedrawText();
        }
    }

    public void SetColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            RedrawText();
        }
    }

    public void SetCaretIndex(int Index)
    {
        Caret.CharacterIndex = Index;
        SelectionStartIndex.CharacterIndex = -1;
        SelectionEndIndex.CharacterIndex = -1;
        RedrawText();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        this.RedrawText();
    }

    public override void WidgetSelected(BaseEventArgs e)
    {
        base.WidgetSelected(e);
        EnteringText = true;
        Input.StartTextInput();
        SetTimer("idle", 400);
    }

    public override void WidgetDeselected(BaseEventArgs e)
    {
        base.WidgetDeselected(e);
        EnteringText = false;
        Input.StopTextInput();
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        if (Mouse.Inside)
        {
            Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
        }
        else
        {
            Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        }
    }

    /// <summary>
    /// Cancels the selection and puts the caret on the left.
    /// </summary>
    public void CancelSelectionLeft()
    {
        if (SelectionEndIndex.CharacterIndex > SelectionStartIndex.CharacterIndex)
        {
            Caret.CharacterIndex -= SelectionEndIndex.CharacterIndex - SelectionStartIndex.CharacterIndex;
            RepositionSprites();
        }
        CancelSelectionHidden();
        RepositionSprites();
    }

    /// <summary>
    /// Cancels the selection and puts the caret on the right.
    /// </summary>
    public void CancelSelectionRight()
    {
        if (SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex)
        {
            Caret.CharacterIndex += SelectionStartIndex.CharacterIndex - SelectionEndIndex.CharacterIndex;
            RepositionSprites();
        }
        CancelSelectionHidden();
        RepositionSprites();
    }

    /// <summary>
    /// Cancels the selection without updating the caret.
    /// </summary>
    public void CancelSelectionHidden()
    {
        SelectionStartIndex.CharacterIndex = -1;
        SelectionEndIndex.CharacterIndex = -1;
    }

    /// <summary>
    /// Moves the caret to the left.
    /// </summary>
    /// <param name="Count">The number of characters to skip.</param>
    public void MoveCaretLeft(int Count = 1)
    {
        if (this.ReadOnly) return;
        if (Caret.CharacterIndex - Count < 0) return;
        string TextToCaret = this.Text.Substring(0, Caret.CharacterIndex);
        int charw = Font.TextSize(TextToCaret).Width - Font.TextSize(TextToCaret.Substring(0, TextToCaret.Length - Count)).Width;
        Caret.CharacterIndex -= Count;
        ResetIdle();
    }

    /// <summary>
    /// Moves the caret to the right.
    /// </summary>
    /// <param name="Count">The number of characters to skip.</param>
    public void MoveCaretRight(int Count = 1)
    {
        if (this.ReadOnly) return;
        if (Caret.CharacterIndex + Count > this.Text.Length) return;
        string TextToCaret = this.Text.Substring(0, Caret.CharacterIndex);
        string TextToCaretPlusOne = this.Text.Substring(0, Caret.CharacterIndex + Count);
        int charw = Font.TextSize(TextToCaretPlusOne).Width - Font.TextSize(TextToCaret).Width;
        Caret.CharacterIndex += Count;
        ResetIdle();
    }

    /// <summary>
    /// Inserts text to the left of the caret.
    /// </summary>
    /// <param name="InsertionIndex">The index at which to insert text.</param>
    /// <param name="Text">The text to insert.</param>
    public void InsertText(int InsertionIndex, string Text)
    {
        if (this.ReadOnly) return;
        if (Text.Length == 0) return;
        int charw = Font.TextSize(this.Text.Substring(0, InsertionIndex) + Text).Width - Font.TextSize(this.Text.Substring(0, InsertionIndex)).Width;
        this.Text = this.Text.Insert(InsertionIndex, Text);
        this.Caret.CharacterIndex += Text.Length;
        ResetIdle();
    }

    /// <summary>
    /// Deletes text to the left of the caret.
    /// </summary>
    /// <param name="StartIndex">Starting index of the range to delete.</param>
    /// <param name="Count">Number of characters to delete.</param>
    public void RemoveText(int StartIndex, int Count = 1)
    {
        if (this.ReadOnly) return;
        if (this.Text.Length == 0 || StartIndex < 0 || StartIndex >= this.Text.Length) return;
        string TextIncluding = this.Text.Substring(0, StartIndex + Count);
        int charw = Font.TextSize(TextIncluding).Width - Font.TextSize(this.Text.Substring(0, StartIndex)).Width;
        Caret.CharacterIndex -= Count;
        this.Text = this.Text.Remove(StartIndex, Count);
        ResetIdle();
    }

    /// <summary>
    /// Deletes the content inside the selection.
    /// </summary>
    public void DeleteSelection()
    {
        if (this.ReadOnly) return;
        int startidx = SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex ? SelectionEndIndex.CharacterIndex : SelectionStartIndex.CharacterIndex;
        int endidx = SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex ? SelectionStartIndex.CharacterIndex : SelectionEndIndex.CharacterIndex;
        CancelSelectionRight();
        RemoveText(startidx, endidx - startidx);
        ResetIdle();
    }

    /// <summary>
    /// Finds the next word that could be skipped to with control.
    /// </summary>
    /// <param name="Left">Whether to search to the left or right of the caret.</param>
    /// <returns>The next index to jump to when holding control.</returns>
    public virtual int FindNextCtrlIndex(bool Left) // or false for Right
    {
        string splitters = " `~!@#$%^&*()-=+[]{}\\|;:'\",.<>/?\n";
        bool found = false;
        if (Left)
        {
            bool insideword = Caret.CharacterIndex > 0 && !splitters.Contains(this.Text[Caret.CharacterIndex - 1]);
            for (int i = Caret.CharacterIndex - 1; i >= 0; i--)
            {
                char c = this.Text[i];
                if (!insideword && !splitters.Contains(c))
                {
                    insideword = true;
                }
                else if (insideword && splitters.Contains(c))
                {
                    return i + 1;
                }
            }
            return 0;
        }
        else
        {
            for (int i = Caret.CharacterIndex + 1; i < this.Text.Length; i++)
            {
                char c = this.Text[i];
                if (splitters.Contains(c) && i != Caret.CharacterIndex + 1)
                {
                    found = true;
                }
                else if (found && !splitters.Contains(c))
                {
                    return i;
                }
            }
            return this.Text.Length;
        }
    }

    /// <summary>
    /// Selects all text.
    /// </summary>
    public void SelectAll()
    {
        if (this.ReadOnly) return;
        MoveCaretRight(this.Text.Length - Caret.CharacterIndex);
        SelectionStartIndex.CharacterIndex = 0;
        SelectionEndIndex.CharacterIndex = this.Text.Length;
        RepositionSprites();
    }

    /// <summary>
    /// Copies the selected text to the clipboard and deletes the selection.
    /// </summary>
    public void CutSelection()
    {
        if (this.ReadOnly) return;
        if (SelectionStartIndex.CharacterIndex != -1)
        {
            int startidx = SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex ? SelectionEndIndex.CharacterIndex : SelectionStartIndex.CharacterIndex;
            int endidx = SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex ? SelectionStartIndex.CharacterIndex : SelectionEndIndex.CharacterIndex;
            string text = this.Text.Substring(startidx, endidx - startidx);
            Input.SetClipboard(text);
            DeleteSelection();
            RedrawText();
        }
    }

    /// <summary>
    /// Copies the selected text to the clipboard.
    /// </summary>
    public void CopySelection()
    {
        if (SelectionStartIndex.CharacterIndex != -1)
        {
            int startidx = SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex ? SelectionEndIndex.CharacterIndex : SelectionStartIndex.CharacterIndex;
            int endidx = SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex ? SelectionStartIndex.CharacterIndex : SelectionEndIndex.CharacterIndex;
            string text = this.Text.Substring(startidx, endidx - startidx);
            Input.SetClipboard(text);
        }
    }

    /// <summary>
    /// Pastes text from the clipboard to the text field.
    /// </summary>
    public void PasteText()
    {
        if (this.ReadOnly) return;
        if (TimerPassed("paste")) ResetTimer("paste");
        string text = Input.GetClipboard();
        text = text.Replace("\t", "").Replace("\r", "");
        if (SelectionStartIndex.CharacterIndex != -1 && SelectionStartIndex.CharacterIndex != SelectionEndIndex.CharacterIndex) DeleteSelection();
        InsertText(Caret.CharacterIndex, text);
        RedrawText();
    }

    /// <summary>
    /// Handles input for various keys.
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (!SelectedWidget)
        {
            if (TimerExists("double")) DestroyTimer("double");
            if (TimerExists("left")) DestroyTimer("left");
            if (TimerExists("left_initial")) DestroyTimer("left_initial");
            if (TimerExists("right")) DestroyTimer("right");
            if (TimerExists("right_initial")) DestroyTimer("right_initial");
            if (TimerExists("paste")) DestroyTimer("paste");
            if (EnteringText) WidgetDeselected(new BaseEventArgs());
            if (Sprites["caret"].Visible) Sprites["caret"].Visible = false;
            return;
        }

        bool homeeffect = false;
        bool endeffect = false;

        if (Input.Trigger(SDL_Keycode.SDLK_LEFT) || TimerPassed("left"))
        {
            Caret.EndOfLine = true;
            SelectionEndIndex.EndOfLine = true;
            if (TimerPassed("left")) ResetTimer("left");
            if (Caret.CharacterIndex > 0)
            {
                int Count = 1;
                if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                {
                    Count = Caret.CharacterIndex - FindNextCtrlIndex(true);
                }

                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex.CharacterIndex == -1)
                    {
                        SelectionStartIndex.CharacterIndex = Caret.CharacterIndex;
                    }
                    MoveCaretLeft(Count);
                    SelectionEndIndex.CharacterIndex = Caret.CharacterIndex;
                    if (Caret.LineIndex == Caret.Lines[Caret.Line].Length && Caret.Lines[Caret.Line].Last() != '\n')
                    {
                        Caret.EndOfLine = false;
                        SelectionEndIndex.EndOfLine = false;
                    }
                    RepositionSprites();
                    Caret.EndOfLine = true;
                    SelectionEndIndex.EndOfLine = true;
                }
                else
                {
                    if (SelectionStartIndex.CharacterIndex != -1)
                    {
                        CancelSelectionLeft();
                    }
                    else
                    {
                        MoveCaretLeft(Count);
                        if (Caret.LineIndex == Caret.Lines[Caret.Line].Length && Caret.Lines[Caret.Line].Last() != '\n')
                        {
                            Caret.EndOfLine = false;
                            SelectionEndIndex.EndOfLine = false;
                        }
                        RepositionSprites();
                    }
                }
                VerticalIndex = Caret.LineIndex;
            }
            else if (SelectionStartIndex.CharacterIndex != -1 && !(Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT)))
            {
                CancelSelectionLeft();
            }
        }
        if (Input.Trigger(SDL_Keycode.SDLK_RIGHT) || TimerPassed("right"))
        {
            Caret.EndOfLine = true;
            SelectionEndIndex.EndOfLine = true;
            if (TimerPassed("right")) ResetTimer("right");
            if (Caret.CharacterIndex < this.Text.Length)
            {
                int Count = 1;
                if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                {
                    Count = FindNextCtrlIndex(false) - Caret.CharacterIndex;
                }

                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex.CharacterIndex == -1)
                    {
                        SelectionStartIndex.CharacterIndex = Caret.CharacterIndex;
                    }
                    MoveCaretRight(Count);
                    SelectionEndIndex.CharacterIndex = Caret.CharacterIndex;
                    RepositionSprites();
                }
                else
                {
                    if (SelectionStartIndex.CharacterIndex != -1)
                    {
                        CancelSelectionRight();
                    }
                    else
                    {
                        MoveCaretRight(Count);
                        RepositionSprites();
                    }
                }
                VerticalIndex = Caret.LineIndex;
            }
            else if (SelectionStartIndex.CharacterIndex != -1 && !(Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT)))
            {
                CancelSelectionRight();
            }
        }
        if (Input.Trigger(SDL_Keycode.SDLK_UP) || TimerPassed("up"))
        {
            if (TimerPassed("up")) ResetTimer("up");
            if (Caret.Line > 0)
            {
                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex.CharacterIndex == -1) SelectionStartIndex.CharacterIndex = Caret.CharacterIndex;
                    if (SelectionEndIndex.CharacterIndex == -1) SelectionEndIndex.CharacterIndex = Caret.CharacterIndex;
                    int idx = VerticalIndex;
                    if (idx > SelectionEndIndex.Lines[SelectionEndIndex.Line - 1].Length)
                    {
                        idx = SelectionEndIndex.Lines[SelectionEndIndex.Line - 1].Length;
                        if (SelectionEndIndex.Lines[SelectionEndIndex.Line - 1].EndsWith('\n')) idx -= 1;
                    }
                    SelectionEndIndex.CharacterIndex = SelectionEndIndex.LineToCharIndex(SelectionEndIndex.Line - 1, idx);
                    Caret.CharacterIndex = SelectionEndIndex.CharacterIndex;
                }
                else
                {
                    if (SelectionStartIndex.CharacterIndex != -1)
                    {
                        CancelSelectionLeft();
                    }
                    else
                    {
                        int idx = VerticalIndex;
                        if (idx > Caret.Lines[Caret.Line - 1].Length)
                        {
                            idx = Caret.Lines[Caret.Line - 1].Length;
                            if (Caret.Lines[Caret.Line - 1].EndsWith('\n')) idx -= 1;
                        }
                        Caret.CharacterIndex = Caret.LineToCharIndex(Caret.Line - 1, idx);
                    }
                }
                RepositionSprites();
            }
            else
            {
                // If pressed Down on the last line, it's treated as Home instead.
                homeeffect = true;
            }
        }
        if (Input.Trigger(SDL_Keycode.SDLK_DOWN) || TimerPassed("down"))
        {
            bool AtLeftSide = Caret.LineIndex == 0;
            if (TimerPassed("down")) ResetTimer("down");
            if (Caret.Line < Caret.Lines.Count - 1)
            {
                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex.CharacterIndex == -1) SelectionStartIndex.CharacterIndex = Caret.CharacterIndex;
                    if (SelectionEndIndex.CharacterIndex == -1) SelectionEndIndex.CharacterIndex = Caret.CharacterIndex;
                    int idx = VerticalIndex;
                    if (idx > SelectionEndIndex.Lines[SelectionEndIndex.Line + 1].Length)
                    {
                        idx = SelectionEndIndex.Lines[SelectionEndIndex.Line + 1].Length;
                        if (SelectionEndIndex.Lines[SelectionEndIndex.Line + 1].EndsWith('\n')) idx -= 1;
                    }
                    SelectionEndIndex.CharacterIndex = SelectionEndIndex.LineToCharIndex(SelectionEndIndex.Line + 1, idx);
                    Caret.CharacterIndex = SelectionEndIndex.CharacterIndex;
                }
                else
                {
                    if (SelectionStartIndex.CharacterIndex != -1)
                    {
                        CancelSelectionRight();
                    }
                    else
                    {
                        int idx = VerticalIndex;
                        if (idx > Caret.Lines[Caret.Line + 1].Length)
                        {
                            idx = Caret.Lines[Caret.Line + 1].Length;
                            if (Caret.Lines[Caret.Line + 1].EndsWith('\n')) idx -= 1;
                        }
                        Caret.CharacterIndex = Caret.LineToCharIndex(Caret.Line + 1, idx);
                    }
                }
                Caret.EndOfLine = true;
                SelectionEndIndex.EndOfLine = true;
                if (AtLeftSide && Caret.LineIndex == Caret.Lines[Caret.Line].Length && Caret.Lines[Caret.Line].Last() != '\n' && Caret.Line < Caret.Lines.Count - 1)
                {
                    Caret.EndOfLine = false;
                    SelectionEndIndex.EndOfLine = false;
                }
                RepositionSprites();
            }
            else
            {
                // If pressed Down on the last line, it's treated as End instead.
                endeffect = true;
            }
        }
        if (Input.Trigger(SDL_Keycode.SDLK_HOME) || homeeffect)
        {
            if (Caret.LineIndex > 0)
            {
                bool CancelledSelection = false;
                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex.CharacterIndex == -1) SelectionStartIndex.CharacterIndex = Caret.CharacterIndex;
                    SelectionEndIndex.CharacterIndex = Caret.LineToCharIndex(Caret.Line, 0);
                }
                else
                {
                    if (SelectionStartIndex.CharacterIndex != -1)
                    {
                        CancelSelectionLeft();
                        CancelledSelection = true;
                    }
                }
                if (!CancelledSelection)
                {
                    if (Caret.LineIndex > 0) MoveCaretLeft(Caret.LineIndex);
                    if (Caret.LineIndex == Caret.Lines[Caret.Line].Length && Caret.Lines[Caret.Line].Last() != '\n')
                    {
                        Caret.EndOfLine = false;
                        SelectionEndIndex.EndOfLine = false;
                    }
                    VerticalIndex = Caret.LineIndex;
                }
                RepositionSprites();
            }
            else if (SelectionStartIndex.CharacterIndex != -1 && !Input.Press(SDL_Keycode.SDLK_LSHIFT) && !Input.Press(SDL_Keycode.SDLK_RSHIFT))
            {
                CancelSelectionLeft();
                RepositionSprites();
            }
        }
        if (Input.Trigger(SDL_Keycode.SDLK_END) || endeffect)
        {
            if (Caret.LineIndex < Caret.Lines[Caret.Line].Length)
            {
                bool CancelledSelection = false;
                Caret.EndOfLine = true;
                SelectionEndIndex.EndOfLine = true;
                if (Caret.LineIndex == Caret.Lines[Caret.Line].Length && Caret.Lines[Caret.Line].Last() != '\n' && Caret.Line < Caret.Lines.Count - 1)
                {
                    Caret.EndOfLine = false;
                    SelectionEndIndex.EndOfLine = false;
                }
                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex.CharacterIndex == -1) SelectionStartIndex.CharacterIndex = Caret.CharacterIndex;
                    if (Caret.Lines[Caret.Line].EndsWith('\n'))
                        SelectionEndIndex.CharacterIndex = Caret.LineToCharIndex(Caret.Line, Caret.Lines[Caret.Line].Length - 1);
                    else SelectionEndIndex.CharacterIndex = Caret.LineToCharIndex(Caret.Line, Caret.Lines[Caret.Line].Length);
                }
                else
                {
                    if (SelectionStartIndex.CharacterIndex != -1)
                    {
                        CancelSelectionRight();
                        CancelledSelection = true;
                    }
                }
                if (!CancelledSelection)
                {
                    int count = Caret.Lines[Caret.Line].Length - Caret.LineIndex;
                    if (Caret.Lines[Caret.Line].EndsWith('\n')) count -= 1;
                    if (count > 0) MoveCaretRight(count);
                    Caret.EndOfLine = true;
                    SelectionEndIndex.EndOfLine = true;
                    VerticalIndex = Caret.LineIndex;
                }
                RepositionSprites();
            }
            else if (SelectionStartIndex.CharacterIndex != -1 && !Input.Press(SDL_Keycode.SDLK_LSHIFT) && !Input.Press(SDL_Keycode.SDLK_RSHIFT))
            {
                CancelSelectionRight();
                RepositionSprites();
            }
        }
        if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
        {
            if (Input.Trigger(SDL_Keycode.SDLK_a))
            {
                SelectAll();
            }
            if (Input.Trigger(SDL_Keycode.SDLK_x))
            {
                CutSelection();
            }
            if (Input.Trigger(SDL_Keycode.SDLK_c))
            {
                CopySelection();
            }
            if (Input.Trigger(SDL_Keycode.SDLK_v) || TimerPassed("paste"))
            {
                PasteText();
            }
        }

        // Timers for repeated input
        if (Input.Press(SDL_Keycode.SDLK_LEFT))
        {
            if (!TimerExists("left_initial") && !TimerExists("left"))
            {
                SetTimer("left_initial", 300);
            }
            else if (TimerPassed("left_initial"))
            {
                DestroyTimer("left_initial");
                SetTimer("left", 50);
            }
        }
        else
        {
            if (TimerExists("left")) DestroyTimer("left");
            if (TimerExists("left_initial")) DestroyTimer("left_initial");
        }
        if (Input.Press(SDL_Keycode.SDLK_RIGHT))
        {
            if (!TimerExists("right_initial") && !TimerExists("right"))
            {
                SetTimer("right_initial", 300);
            }
            else if (TimerPassed("right_initial"))
            {
                DestroyTimer("right_initial");
                SetTimer("right", 50);
            }
        }
        else
        {
            if (TimerExists("right")) DestroyTimer("right");
            if (TimerExists("right_initial")) DestroyTimer("right_initial");
        }
        if (Input.Press(SDL_Keycode.SDLK_UP))
        {
            if (!TimerExists("up_initial") && !TimerExists("up"))
            {
                SetTimer("up_initial", 300);
            }
            else if (TimerPassed("up_initial"))
            {
                DestroyTimer("up_initial");
                SetTimer("up", 50);
            }
        }
        else
        {
            if (TimerExists("up")) DestroyTimer("up");
            if (TimerExists("up_initial")) DestroyTimer("up_initial");
        }
        if (Input.Press(SDL_Keycode.SDLK_DOWN))
        {
            if (!TimerExists("down_initial") && !TimerExists("down"))
            {
                SetTimer("down_initial", 300);
            }
            else if (TimerPassed("down_initial"))
            {
                DestroyTimer("down_initial");
                SetTimer("down", 50);
            }
        }
        else
        {
            if (TimerExists("down")) DestroyTimer("down");
            if (TimerExists("down_initial")) DestroyTimer("down_initial");
        }
        if ((Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL)) && Input.Press(SDL_Keycode.SDLK_v))
        {
            if (!TimerExists("paste_initial") && !TimerExists("paste"))
            {
                SetTimer("paste_initial", 300);
            }
            else if (TimerPassed("paste_initial"))
            {
                DestroyTimer("paste_initial");
                SetTimer("paste", 50);
            }
        }
        else
        {
            if (TimerExists("paste")) DestroyTimer("paste");
            if (TimerExists("paste_initial")) DestroyTimer("paste_initial");
        }

        if (TimerPassed("double")) DestroyTimer("double");

        if (TimerPassed("idle"))
        {
            Sprites["caret"].Visible = !Sprites["caret"].Visible;
            ResetTimer("idle");
        }

        if (this.ReadOnly) Sprites["caret"].Visible = false;
    }

    /// <summary>
    /// Resets the idle timer, which pauses the caret blinking.
    /// </summary>
    public void ResetIdle()
    {
        Sprites["caret"].Visible = true;
        if (TimerExists("idle")) ResetTimer("idle");
    }

    public void RedrawText()
    {
        this.DrawnText = true;
        Caret.Lines.Clear();
        Caret.CharacterWidths.Clear();
        int startidx = 0;
        int lastsplittableindex = -1;
        for (int i = 0; i < this.Text.Length; i++)
        {
            char c = this.Text[i];
            string txt = this.Text.Substring(startidx, i - startidx + 1);
            Size s = this.Font.TextSize(txt);
            if (c == '\n')
            {
                Caret.Lines.Add(this.Text.Substring(startidx, i - startidx + 1));
                startidx = i + 1;
                if (i == Text.Length - 1) Caret.Lines.Add("");
            }
            else if (s.Width >= this.Size.Width)
            {
                int endidx = lastsplittableindex == -1 ? i : lastsplittableindex + 1;
                Caret.Lines.Add(this.Text.Substring(startidx, endidx - startidx - 1));
                startidx = endidx - 1;
                lastsplittableindex = -1;
            }
            else if (c == ' ' || c == '-')
            {
                lastsplittableindex = i + 1;
            }
        }
        if (startidx != this.Text.Length)
        {
            Caret.Lines.Add(this.Text.Substring(startidx));
        }
        else if (Caret.Lines.Count == 0)
        {
            Caret.Lines.Add("");
        }
        for (int i = 0; i < Caret.Lines.Count; i++)
        {
            Caret.CharacterWidths.Add(new List<int>());
            for (int j = 0; j < Caret.Lines[i].Length; j++)
            {
                if (Caret.Lines[i][j] == '\n') Caret.CharacterWidths[i].Add((Caret.CharacterWidths[i].Count > 0 ? Caret.CharacterWidths[i].Last() : 0) + 4);
                else Caret.CharacterWidths[i].Add(Font.TextSize(Caret.Lines[i].Substring(0, j + 1)).Width);
            }
        }
        this.SelectionStartIndex.Lines = Caret.Lines;
        this.SelectionEndIndex.Lines = Caret.Lines;
        this.DrawnText = false;
        this.Redraw();
        this.RepositionSprites();
    }

    protected override void Draw()
    {
        if (!this.DrawnText)
        {
            Sprites["text"].Bitmap?.Dispose();
            int lineheight = Font.Size + 4;
            Sprites["text"].Bitmap = new Bitmap(Size.Width, Math.Max(1, lineheight * Caret.Lines.Count));
            this.SetHeight(Sprites["text"].Bitmap.Height + 2);
            if (Caret.Lines.Count > 0)
            {
                Sprites["text"].Bitmap.Unlock();
                Sprites["text"].Bitmap.Font = this.Font;
                for (int i = 0; i < Caret.Lines.Count; i++)
                {
                    Sprites["text"].Bitmap.DrawText(Caret.Lines[i].Replace("\n", ""), 0, lineheight * i, this.TextColor);
                }
                Sprites["text"].Bitmap.Lock();
            }
        }
        base.Draw();
    }

    /// <summary>
    /// Positions the caret and selection sprites.
    /// </summary>
    public void RepositionSprites()
    {
        if (Caret.Lines.Count == 0 || Caret.CharacterWidths.Count == 0) return;
        Sprites["caret"].X = Caret.LineIndex == 0 ? 0 : Caret.CharacterWidths[Caret.Line][Caret.LineIndex - 1];
        Sprites["caret"].Y = (Font.Size + 4) * Caret.Line;
        while (SelectionFillers.Count > 0)
        {
            SelectionFillers[0].Dispose();
            SelectionFillers.RemoveAt(0);
        }
        if (SelectionStartIndex.CharacterIndex != -1 && SelectionEndIndex.CharacterIndex != -1)
        {
            bool reverse = SelectionStartIndex.CharacterIndex > SelectionEndIndex.CharacterIndex;
            Index StartIdx = reverse ? SelectionEndIndex : SelectionStartIndex;
            Index EndIdx = reverse ? SelectionStartIndex : SelectionEndIndex;
            for (int line = StartIdx.Line; line <= EndIdx.Line; line++)
            {
                Sprites[$"sel{line}"] = new Sprite(this.Viewport);
                int x = 0;
                int width = 0;
                if (line == StartIdx.Line && EndIdx.Line != StartIdx.Line)
                {
                    x = StartIdx.LineIndex == 0 ? 0 : Caret.CharacterWidths[line][StartIdx.LineIndex - 1];
                    width = Caret.CharacterWidths[line].Last() - x;
                }
                else if (line != StartIdx.Line && line != EndIdx.Line)
                {
                    width = Caret.CharacterWidths[line].Last();
                }
                else if (line == EndIdx.Line && EndIdx.Line != StartIdx.Line)
                {
                    width = EndIdx.LineIndex == 0 ? 0 : Caret.CharacterWidths[line][EndIdx.LineIndex - 1];
                }
                else if (StartIdx.Line == EndIdx.Line)
                {
                    x = StartIdx.LineIndex == 0 ? 0 : Caret.CharacterWidths[line][StartIdx.LineIndex - 1];
                    width = (EndIdx.LineIndex == 0 ? 0 : Caret.CharacterWidths[line][EndIdx.LineIndex - 1]) - x;
                }
                if (line != StartIdx.Line && line != EndIdx.Line && width < 4) width = 4;
                SolidBitmap bmp = new SolidBitmap(width, Font.Size + 4, new Color(50, 50, 255, 100));
                Sprites[$"sel{line}"].Bitmap = bmp;
                Sprites[$"sel{line}"].X = x;
                Sprites[$"sel{line}"].Y = (Font.Size + 4) * line;
                SelectionFillers.Add((Sprite)Sprites[$"sel{line}"]);
            }
        }
    }

    public override void TextInput(TextEventArgs e)
    {
        base.TextInput(e);
        string text = this.Text;
        if (!string.IsNullOrEmpty(e.Text))
        {
            if (SelectionStartIndex.CharacterIndex != -1 && SelectionStartIndex.CharacterIndex != SelectionEndIndex.CharacterIndex) DeleteSelection();
            InsertText(Caret.CharacterIndex, e.Text);
        }
        else if (e.Backspace || e.Delete)
        {
            if (SelectionStartIndex.CharacterIndex != -1 && SelectionStartIndex.CharacterIndex != SelectionEndIndex.CharacterIndex)
            {
                DeleteSelection();
            }
            else
            {
                if (SelectionStartIndex.CharacterIndex == SelectionEndIndex.CharacterIndex) CancelSelectionHidden();
                if (e.Delete)
                {
                    if (Caret.CharacterIndex < this.Text.Length)
                    {
                        int Count = 1;
                        if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                            Count = FindNextCtrlIndex(false) - Caret.CharacterIndex;
                        MoveCaretRight(Count);
                        RemoveText(this.Caret.CharacterIndex - Count, Count);
                    }
                }
                else
                {
                    int Count = 1;
                    if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                        Count = Caret.CharacterIndex - FindNextCtrlIndex(true);
                    RemoveText(this.Caret.CharacterIndex - Count, Count);
                }
            }
        }
        if (this.Text != text)
        {
            this.OnTextChanged?.Invoke(new BaseEventArgs());
        }
        RedrawText();
    }

    public int GetHoveringIndex(MouseEventArgs e)
    {
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
        int Line = ry / (Font.Size + 4);
        if (Line < 0) Line = 0;
        if (Line >= Caret.Lines.Count)
        {
            Line = Caret.Lines.Count - 1;
            if (Caret.Lines[Line].Length == 0) return Caret.LineToCharIndex(Line, 0);
            rx = Caret.CharacterWidths[Line].Last();
        }
        if (Caret.Lines[Line].Length == 0) return Caret.LineToCharIndex(Line, 0);
        if (rx >= Caret.CharacterWidths[Line].Last())
        {
            int idx = Caret.LineToCharIndex(Line, Caret.Lines[Line].Length);
            if (Caret.Lines[Line].EndsWith('\n')) idx -= 1;
            if (Line == Caret.Lines.Count - 1)
            {
                Caret.EndOfLine = true;
                SelectionEndIndex.EndOfLine = true;
            }
            return idx;
        }
        else if (rx < 0)
        {
            Caret.EndOfLine = false;
            SelectionEndIndex.EndOfLine = false;
            return Caret.LineToCharIndex(Line, 0);
        }
        for (int i = 0; i < Caret.CharacterWidths[Line].Count; i++)
        {
            int thiswidth = i > 0 ? Caret.CharacterWidths[Line][i - 1] : 0;
            int nextwidth = Caret.CharacterWidths[Line][i];
            if (rx >= thiswidth && rx < nextwidth)
            {
                // In the middle of a character
                if (rx - thiswidth >= nextwidth - rx)
                {
                    // More to the right
                    if (i + 1 == Caret.CharacterWidths[Line].Count && Line != Caret.Lines.Count - 1)
                    {
                        Caret.EndOfLine = true;
                        SelectionEndIndex.EndOfLine = true;
                    }
                    return Caret.LineToCharIndex(Line, i + 1);
                }
                else
                {
                    // More to the left
                    if (i == 0)
                    {
                        Caret.EndOfLine = false;
                        SelectionEndIndex.EndOfLine = false;
                    }
                    return Caret.LineToCharIndex(Line, i);
                }
            }
        }
        return 0;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside || this.Text.Length == 0 || this.ReadOnly) return;
        if (SelectionStartIndex.CharacterIndex != -1 && SelectionStartIndex.CharacterIndex != SelectionEndIndex.CharacterIndex) CancelSelectionHidden();
        Caret.CharacterIndex = GetHoveringIndex(e);
        VerticalIndex = Caret.LineIndex;
        RepositionSprites();
        if (!TimerExists("double"))
        {
            SetTimer("double", 300);
        }
        else if (!TimerPassed("double"))
        {
            // Double clicked
            DoubleClick();
            DestroyTimer("double");
        }
    }

    public void DoubleClick()
    {
        int startindex = FindNextCtrlIndex(true);
        int endindex = FindNextCtrlIndex(false);
        if (endindex - startindex > 0)
        {
            SelectionStartIndex.CharacterIndex = startindex;
            SelectionEndIndex.CharacterIndex = endindex;
            MoveCaretLeft(Caret.CharacterIndex - startindex);
            MoveCaretRight(endindex - Caret.CharacterIndex);
            MinIdx = startindex;
            MaxIdx = endindex;
            SnapToWords = true;
            RepositionSprites();
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!e.LeftButton || !Mouse.LeftStartedInside || this.ReadOnly) return;
        int idx = GetHoveringIndex(e);


        if (SelectionStartIndex.CharacterIndex == -1) SelectionStartIndex.CharacterIndex = idx;
        if (SnapToWords)
        {
            if (idx > MaxIdx)
            {
                SelectionEndIndex.CharacterIndex = idx;
            }
            else if (idx < MinIdx)
            {
                SelectionEndIndex.CharacterIndex = idx;
                SelectionStartIndex.CharacterIndex = MaxIdx;
            }
            else if (idx > MinIdx && idx < MaxIdx)
            {
                SelectionStartIndex.CharacterIndex = MinIdx;
                SelectionEndIndex.CharacterIndex = MaxIdx;
            }
        }
        else SelectionEndIndex.CharacterIndex = idx;


        if (SelectionEndIndex.CharacterIndex != -1) Caret.CharacterIndex = SelectionEndIndex.CharacterIndex;
        RepositionSprites();
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (SnapToWords)
        {
            MinIdx = -1;
            MaxIdx = -1;
            SnapToWords = false;
        }
    }
}

public class Index
{
    public List<string> Lines = new List<string>();
    public bool EndOfLine = true;
    public List<List<int>> CharacterWidths = new List<List<int>>();

    public int CharacterIndex;
    public int Line
    {
        get
        {
            int idx = this.CharacterIndex;
            for (int i = 0; i < Lines.Count; i++)
            {
                if (idx == Lines[i].Length)
                {
                    if (Lines[i].EndsWith('\n')) return i + 1;
                    else return EndOfLine ? i : i + 1;
                }
                if (idx < Lines[i].Length) return i;
                idx -= Lines[i].Length;
            }
            return Lines.Count - 1;
        }
    }
    public int LineIndex
    {
        get
        {
            int idx = this.CharacterIndex;
            for (int i = 0; i < Lines.Count; i++)
            {
                if (idx == Lines[i].Length)
                {
                    if (Lines[i].EndsWith('\n')) return 0;
                    else return EndOfLine ? idx : 0;
                }
                if (idx < Lines[i].Length) return idx;
                idx -= Lines[i].Length;
            }
            return Lines.Last().Length;
        }
    }

    public Index(int CharacterIndex)
    {
        this.CharacterIndex = CharacterIndex;
    }

    public int LineToCharIndex(int Line, int LineIndex)
    {
        int idx = 0;
        for (int i = 0; i < Line; i++)
        {
            idx += Lines[i].Length;
        }
        idx += LineIndex;
        return idx;
    }
}
