﻿using System;
using System.Collections.Generic;

using RPGStudioMK.Game;
using RPGStudioMK.Utility;

namespace RPGStudioMK.Widgets;

public class ScriptEditorBox : Widget
{
    public string Text => TextArea.Text;
    public Font Font => TextArea.Font;
    public Color TextColor => TextArea.TextColor;
    public int LineHeight => TextArea.LineHeight;
    public Color TextColorSelected => TextArea.TextColorSelected;
    public bool OverlaySelectedText => TextArea.OverlaySelectedText;
    public Color SelectionBackgroundColor => TextArea.SelectionBackgroundColor;
    public bool LineWrapping => TextArea.LineWrapping;
    public Color LineTextColor => TextArea.LineTextColor;
    public Color LineTextBackgroundColor => TextArea.LineTextBackgroundColor;
    public int LineTextWidth => TextArea.LineTextWidth;
    public int TextXOffset => TextArea.TextXOffset;
    public List<Script> OpenScripts => TabNavigator.OpenScripts;
    public Script? OpenScript => TabNavigator.OpenScript;
    public Script? PreviewScript => TabNavigator.PreviewScript;
    public List<Script> RecentScripts => TabNavigator.RecentScripts;

    //public BaseEvent OnTextChanged { get => TextArea.OnTextChanged; set => TextArea.OnTextChanged = value; }
    public BoolEvent OnCopy { get => TextArea.OnCopy; set => TextArea.OnCopy = value; }
    public BoolEvent OnPaste { get => TextArea.OnPaste; set => TextArea.OnPaste = value; }

    Dictionary<Script, ScriptEditorTextArea.ScriptEditorState> ScriptStates = new Dictionary<Script, ScriptEditorTextArea.ScriptEditorState>();

    Container ScrollContainer;
    ScriptEditorTextArea TextArea;
    ScriptTabNavigator TabNavigator;
    Grid MainGrid;
    FindReplaceBox FindReplaceBox;

    public ScriptEditorBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);

        MainGrid = new Grid(this);
        MainGrid.SetRows(
            new GridSize(30, Unit.Pixels),
            new GridSize(1),
            new GridSize(0, Unit.Pixels)
        );
        MainGrid.SetDocked(true);

        TabNavigator = new ScriptTabNavigator(MainGrid);
        TabNavigator.OnScriptClosing += e =>
        {
            UpdateScriptState(e.Object);
        };
        TabNavigator.OnScriptClosed += _ =>
        {
            SetScriptBox(this.OpenScript, false);
        };
        TabNavigator.OnOpenScriptChanged += e =>
        {
            SetScriptBox(this.OpenScript, false);
        };
        TabNavigator.OnMouseDown += e =>
        {
            if (TabNavigator.Mouse.Inside) TextArea.WidgetSelected(new BaseEventArgs());
        };

        bool wasPreviouslyInsideTextArea = false;

        ScrollContainer = new Container(MainGrid);
        ScrollContainer.SetGridRow(1);
        ScrollContainer.SetMargins(3, 3, 18, 18);
        ScrollContainer.OnMouseMoving += e =>
        {
            if (!TextArea.Interactable) return;
            bool InsideTextArea = ScrollContainer.Mouse.Inside && e.X - ScrollContainer.Viewport.X >= TextArea.TextXOffset;
            if (InsideTextArea && Input.SystemCursor == CursorType.Arrow) Input.SetCursor(CursorType.IBeam);
            else if (!InsideTextArea && wasPreviouslyInsideTextArea && Input.SystemCursor != CursorType.Arrow) Input.SetCursor(CursorType.Arrow);
            wasPreviouslyInsideTextArea = InsideTextArea;
        };
        ScrollContainer.OnMouseWheel += e =>
        {
            if (!TextArea.Interactable) return;
            if (!Input.Press(Keycode.CTRL)) return;
            int add = Math.Sign(e.WheelY);
            TextArea.SetFont(FontCache.GetOrCreate(Fonts.Monospace.Name, Math.Max(1, TextArea.Font.Size + add)));
        };
        ScrollContainer.OnSizeChanged += _ =>
        {
            TextArea.MinimumSize = ScrollContainer.Size;
            // Enforce update if size shrunk
            TextArea.SetSize(TextArea.Size);
        };

        TextArea = new ScriptEditorTextArea(ScrollContainer);
        TextArea.SetHDocked(true);
        TextArea.MinimumSize = ScrollContainer.Size;
        TextArea.SetSize(TextArea.Size);
        TextArea.SetFont(FontCache.GetOrCreate(Fonts.Monospace.Name, 16));
        TextArea.OnTextChanged += _ =>
        {
            if (TextArea.Interactable) this.OpenScript.Content = TextArea.Text;
            if (this.OpenScript == this.PreviewScript)
            {
                Script openScript = this.OpenScript;
                TabNavigator.SetPreviewScript(null, false);
                TabNavigator.SetOpenScript(openScript, true);
            }
        };

        VScrollBar vs = new VScrollBar(MainGrid);
        vs.SetGridRow(1);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, 3, 0, 3);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        HScrollBar hs = new HScrollBar(MainGrid);
        hs.SetGridRow(1);
        hs.SetHDocked(true);
        hs.SetBottomDocked(true);
        hs.SetPadding(3, 0, 13, 0);
        ScrollContainer.SetHScrollBar(hs);
        ScrollContainer.HAutoScroll = true;

        TextArea.Update();

        SetLineWrapping(false);
        UpdateScrollBar();

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.F, Keycode.CTRL), _ => ShowFindReplaceMenu(), true)
        });
    }

    public void UpdateWidth()
    {
		TextArea.MinimumSize = ScrollContainer.Size;
		TextArea.UpdateWidth();
    }

    private void ShowFindReplaceMenu()
    {
        if (FindReplaceBox is not null && !FindReplaceBox.Disposed)
        {
            FindReplaceBox.ForceSelect();
            return;
        }
        FindReplaceBox = new FindReplaceBox(this, MainGrid);
        FindReplaceBox.SetGridRow(2);
        FindReplaceBox.OnDisposed += _ =>
        {
            if (MainGrid.Disposed) return;
            MainGrid.Rows[2] = new GridSize(0, Unit.Pixels);
            MainGrid.UpdateContainers();
            MainGrid.UpdateLayout();
		    ScrollContainer.VScrollBar.SetPadding(new Padding(0, 3, 0, 13));
            ScrollContainer.HScrollBar.SetPadding(new Padding(3, 0, 13, 0));
			Redraw();
			Draw();
		};
        MainGrid.Rows[2] = new GridSize(77, Unit.Pixels);
        MainGrid.UpdateContainers();
        MainGrid.UpdateLayout();
		ScrollContainer.VScrollBar.SetPadding(new Padding(0, 3, 0, 90));
        ScrollContainer.HScrollBar.SetPadding(new Padding(3, 0, 13, 77));
		Redraw();
		Draw();
	}

    private void UpdateScriptState(Script script)
    {
        // Do not save the state if the script was not currently open.
        if (OpenScript != script) return;
        if (ScriptStates.ContainsKey(script)) ScriptStates[script] = new ScriptEditorTextArea.ScriptEditorState(TextArea);
        else ScriptStates.Add(script, new ScriptEditorTextArea.ScriptEditorState(TextArea));
    }

    public void UpdateSize()
    {
        TextArea.TokenizeUntokenizedLines();
    }

    private void UpdateScrollBar()
    {
        ScrollContainer.VScrollBar.SetScrollStep(TextArea.LineHeight + TextArea.LineMargins);
    }

    Script? LastShownScript;

    public void SetScriptBox(Script? script, bool preview)
    {
        if (script is null)
        {
			TextArea.SetInteractable(false);
			TextArea.SetReadOnly(true);
			TextArea.SetDrawLineNumbers(false);
			TextArea.SetText("");
            LastShownScript = null;
			return;
        }
        if (preview)
        {
            if (TabNavigator.IsOpen(script)) TabNavigator.SetOpenScript(script);
            else TabNavigator.SetPreviewScript(script, true);
        }
        else
        {
            if (TabNavigator.PreviewScript == script)
            {
                TabNavigator.SetPreviewScript(null, false);
				TabNavigator.SetOpenScript(script, true);
			}
            else TabNavigator.SetOpenScript(script);
        }
        if (LastShownScript != script)
        {
			TextArea.SetInteractable(true);
			TextArea.SetReadOnly(false);
			TextArea.SetDrawLineNumbers(true);
			if (LastShownScript is not null) UpdateScriptState(LastShownScript);
			TextArea.SetText(script.Content, false);
			if (ScriptStates.ContainsKey(script))
			{
			    var state = ScriptStates[script];
			    state.Apply(false);
			}
			TextArea.WidgetSelected(new BaseEventArgs());
		}
        LastShownScript = script;
	}

    public void SetPivot(int pivot)
    {
        TabNavigator.SetPivot(pivot);
    }

    public void SetFont(Font Font)
    {
        TextArea.SetFont(Font);
        UpdateScrollBar();
    }

    public void SetTextColor(Color Color)
    {
        TextArea.SetTextColor(Color);
    }

    public void SetLineHeight(int LineHeight)
    {
        TextArea.SetLineHeight(LineHeight);
        UpdateScrollBar();
    }

    public void SetTextColorSelected(Color Color)
    {
        TextArea.SetTextColorSelected(Color);
    }

    public void SetOverlaySelectedText(bool OverlaySelectedText)
    {
        TextArea.SetOverlaySelectedText(OverlaySelectedText);
    }

    public void SetSelectionBackgroundColor(Color Color)
    {
        TextArea.SetSelectionBackgroundColor(Color);
    }

    public void SetLineMargins(int LineMargins)
    {
        TextArea.SetLineMargins(LineMargins);
    }

    public void SetLineWrapping(bool LineWrapping)
    {
        if (LineWrapping)
        {
            TextArea.SetHDocked(true);
            ScrollContainer.SetPadding(3, 3, 14, 3);
            ScrollContainer.VScrollBar.SetPadding(0, 3, 0, 3);
            ScrollContainer.HScrollBar.SetVisible(false);
            ScrollContainer.HAutoScroll = false;
        }
        else
        {
            TextArea.SetHDocked(false);
            ScrollContainer.SetPadding(3, 3, 14, 14);
            ScrollContainer.VScrollBar.SetPadding(0, 3, 0, 13);
            ScrollContainer.HAutoScroll = true;
        }
        this.Redraw();
        TextArea.SetLineWrapping(LineWrapping);
    }

    public void SetLineTextColor(Color LineTextColor)
    {
        TextArea.SetLineTextColor(LineTextColor);
    }

    public void SetLineTextBackgroundColor(Color LineTextBackgroundColor)
    {
        TextArea.SetLineTextBackgroundColor(LineTextBackgroundColor);
    }

    public void SetLineTextWidth(int LineTextWidth)
    {
        TextArea.SetLineTextWidth(LineTextWidth);
    }

    public void SetTextXOffset(int TextXOffset)
    {
        TextArea.SetTextXOffset(TextXOffset);
    }

    public void Activate()
    {
        TextArea.OnWidgetSelected.Invoke(new BaseEventArgs());
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();

        Sprites["bg"].Bitmap.DrawRect(Size, new Color(86, 108, 134));
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(10, 23, 37));
        
        if (!LineWrapping) Sprites["bg"].Bitmap.FillRect(ScrollContainer.Size.Width + 9, ScrollContainer.Size.Height + 39, 11, 11, new Color(64, 104, 146));
        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(ScrollContainer.Size.Width - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, ScrollContainer.Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(ScrollContainer.Size.Width - 1, ScrollContainer.Size.Height - 1, Color.ALPHA);
        Color DarkOutline = new Color(40, 62, 84);
        Sprites["bg"].Bitmap.SetPixel(1, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(ScrollContainer.Size.Width - 2, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(1, ScrollContainer.Size.Height - 2, DarkOutline);
        if (LineWrapping) Sprites["bg"].Bitmap.SetPixel(ScrollContainer.Size.Width - 2, ScrollContainer.Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.DrawLine(ScrollContainer.Size.Width + 9, 1, ScrollContainer.Size.Width + 9, ScrollContainer.Size.Height + 49, DarkOutline);
        if (!LineWrapping) Sprites["bg"].Bitmap.DrawLine(1, ScrollContainer.Size.Height + 39, ScrollContainer.Size.Width + 19, ScrollContainer.Size.Height + 39, DarkOutline);
        if (!LineWrapping) Sprites["bg"].Bitmap.DrawLine(1, ScrollContainer.Size.Height + 50, ScrollContainer.Size.Width + 19, ScrollContainer.Size.Height + 50, new Color(86, 108, 134));

        Sprites["bg"].Bitmap.Lock();
    }

	public override void Update()
	{
		base.Update();
        // Set the Find & Replace window as selected if it's active and the scrollbar was the last selected widget
        if (Window.UI.SelectedWidget is VScrollBar || Window.UI.SelectedWidget is HScrollBar)
        {
            if (!Mouse.LeftMousePressed && !Mouse.RightMousePressed && !Mouse.MiddleMousePressed)
            {
                if (FindReplaceBox is not null && !FindReplaceBox.Disposed) FindReplaceBox.ForceSelect();
            }
        }
	}

	public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (ScrollContainer.Mouse.Inside) TextArea.OnWidgetSelected.Invoke(new BaseEventArgs());
    }

    public ScriptFinderAndReplacer CreateFinderAndReplacer()
    {
        MultilineTextArea.CaretIndex caret = TextArea.Caret;
        if (!string.IsNullOrEmpty(TextArea.SelectedText))
        {
		    caret = (MultilineTextArea.CaretIndex) TextArea.Caret.Clone();
			caret.Index += TextArea.SelectedText.Length;
        }
        ScriptFinderAndReplacer finder = new ScriptFinderAndReplacer();
        finder.SetScript(TextArea.Lines);
        return finder;
    }

    public void PreviewOccurrence(Script script, Occurrence occurrence)
    {
        // Open the containing script as a preview, jump to the relevant line, and select the occurrence
        SetScriptBox(script, !OpenScripts.Contains(script));
        TextArea.SelectOccurrence(occurrence, false, true);
    }

    public void SetOccurrences(List<Occurrence> occurrences)
    {
        int initialIdx = occurrences.FindIndex(oc => 
            oc.LineNumber > TextArea.Caret.Line.LineIndex || // Find the first occurrence on a line past the caret, or
            oc.LineNumber == TextArea.Caret.Line.LineIndex && oc.IndexInLine >= TextArea.Caret.IndexInLine // find the first occurrence on the same line as the caret, but past it
        );
        if (initialIdx == -1) initialIdx = 0;
        TextArea.SetOccurrences(occurrences, initialIdx);
    }

    public bool ShouldRecalculateOccurrences()
    {
        return !TextArea.HasOccurrences;
    }

    public void SelectNextOccurrence()
    {
        TextArea.SelectNextOccurrence();
    }

    public void SelectPreviousOccurrence()
    {
        TextArea.SelectPreviousOccurrence();
    }

	public void ReplaceNextOccurrence(string pattern)
	{
		TextArea.ReplaceNextOccurrence(pattern);
	}

	public void ReplacePreviousOccurrence(string pattern)
	{
		TextArea.ReplacePreviousOccurrence(pattern);
	}

    public int ReplaceSingleOccurrence(Occurrence occurrence, string pattern)
    {
        return TextArea.ReplaceSingleOccurrence(occurrence, pattern);
    }

    public void ReplaceAllOccurrences(List<Occurrence> occurrences, string pattern)
    {
        TextArea.ReplaceAllOccurrences(occurrences, pattern);
    }

	public void ClearOccurrences()
    {
        TextArea.ClearOccurrences();
    }
}
