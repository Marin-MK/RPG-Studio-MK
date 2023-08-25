using RPGStudioMK.Utility;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class FindReplaceBox : Widget
{
    CheckBox RegexBox;
    CheckBox CaseBox;
    Label FindLabel;
    Label ReplaceLabel;
    TextBox FindBox;
    TextBox ReplaceBox;
    Button FindButton;
    Button ReplaceOneButton;
    Button ReplaceAllButton;

    ScriptEditorBox ScriptEditorBox;
    bool ForceRecalculate = true;

    public FindReplaceBox(ScriptEditorBox ScriptEditorBox, IContainer Parent) : base(Parent)
    {
        this.ScriptEditorBox = ScriptEditorBox;
        SetBackgroundColor(40, 62, 84);

        RegexBox = new CheckBox(this);
        RegexBox.SetPosition(22, 17);
        RegexBox.SetText("Regex");
        RegexBox.OnCheckChanged += _ => ForceRecalculate = true;

        CaseBox = new CheckBox(this);
        CaseBox.SetPosition(22, 40);
        CaseBox.SetText("Case-Sensitive");
        CaseBox.OnCheckChanged += _ => ForceRecalculate = true;

        FindLabel = new Label(this);
        FindLabel.SetText("Find:");
        FindLabel.SetPosition(180, 14);

        ReplaceLabel = new Label(this);
        ReplaceLabel.SetText("Replace:");
        ReplaceLabel.SetPosition(159, 50);

        FindBox = new TextBox(this);
        FindBox.SetHDocked(true);
        FindBox.SetPadding(220, 7, 200, 0);
        FindBox.SetHeight(27);
        FindBox.SetFont(Fonts.Monospace);
        FindBox.SetDeselectOnEnterPressed(false);
        FindBox.OnEnterPressed += _ =>
        {
            if (!Disposed) InvokeSearch(Input.Press(Keycode.SHIFT));
        };
        FindBox.OnTextChanged += _ => ForceRecalculate = true;

        ReplaceBox = new TextBox(this);
        ReplaceBox.SetHDocked(true);
        ReplaceBox.SetPadding(220, 44, 200, 0);
        ReplaceBox.SetHeight(27);
        ReplaceBox.SetFont(Fonts.Monospace);
        ReplaceBox.SetDeselectOnEnterPressed(false);
        ReplaceBox.OnEnterPressed += _ =>
        {
            if (!Disposed) InvokeSearch(Input.Press(Keycode.SHIFT));
        };

        FindButton = new Button(this);
        FindButton.SetRightDocked(true);
        FindButton.SetPadding(0, 4, 110, 0);
        FindButton.SetText("Find");
        FindButton.OnClicked += _ => InvokeSearch(Input.Press(Keycode.SHIFT));

        ReplaceOneButton = new Button(this);
        ReplaceOneButton.SetRightDocked(true);
        ReplaceOneButton.SetPadding(0, 41, 110, 0);
        ReplaceOneButton.SetText("Replace");
        ReplaceOneButton.OnClicked += _ => InvokeReplaceOne(Input.Press(Keycode.SHIFT));

        ReplaceAllButton = new Button(this);
        ReplaceAllButton.SetRightDocked(true);
        ReplaceAllButton.SetPadding(0, 41, 10, 0);
        ReplaceAllButton.SetText("Replace All");
        ReplaceAllButton.SetWidth(90);
        ReplaceAllButton.OnClicked += _ => InvokeReplaceAll();

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ESCAPE), _ => DestroyWindow(), true),
            new Shortcut(this, new Key(Keycode.TAB), _ =>
            {
                ReplaceBox.TextArea.WidgetSelected(new BaseEventArgs());
                ReplaceBox.Redraw();
            }, true, e => e.Value = FindBox.TextArea.SelectedWidget),
            new Shortcut(this, new Key(Keycode.TAB, Keycode.SHIFT), _ =>
            {
                FindBox.TextArea.WidgetSelected(new BaseEventArgs());
                FindBox.Redraw();
            }, true, e => e.Value = ReplaceBox.TextArea.SelectedWidget),
            new Shortcut(this, new Key(Keycode.ENTER), _ => InvokeSearch(false)),
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.SHIFT), _ => InvokeSearch(true))
        });

        MinimumSize.Height = MaximumSize.Height = 77;
        SetHeight(77);
        ForceSelect();
    }

    public void ForceSelect()
    {
        FindBox.TextArea.WidgetSelected(new BaseEventArgs());
    }

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
        if (!FindBox.Mouse.Inside && !ReplaceBox.Mouse.Inside && Mouse.Inside) Input.SetCursor(CursorType.Arrow);
	}

    private void DestroyWindow()
    {
        Dispose();
        ScriptEditorBox.ClearOccurrences();
    }

    private void InvokeSearch(bool backwards)
    {
        if (string.IsNullOrEmpty(FindBox.Text)) return;
        if (ScriptEditorBox.ShouldRecalculateOccurrences() || ForceRecalculate)
        {
            ScriptFinderAndReplacer finder = ScriptEditorBox.CreateFinderAndReplacer();
            finder.SetUseRegex(RegexBox.Checked);
            finder.SetCaseSensitive(CaseBox.Checked);
            List<Occurrence> occurrences = finder.Find(FindBox.Text);
            ScriptEditorBox.SetOccurrences(occurrences);
            ForceSelect();
            ForceRecalculate = false;
        }
        else if (backwards) ScriptEditorBox.SelectPreviousOccurrence();
        else ScriptEditorBox.SelectNextOccurrence();
    }

    private void InvokeReplaceOne(bool backwards)
    {
		if (string.IsNullOrEmpty(FindBox.Text)) return;
		if (ScriptEditorBox.ShouldRecalculateOccurrences() || ForceRecalculate)
		{
			ScriptFinderAndReplacer finder = ScriptEditorBox.CreateFinderAndReplacer();
			finder.SetUseRegex(RegexBox.Checked);
			finder.SetCaseSensitive(CaseBox.Checked);
			List<Occurrence> occurrences = finder.Find(FindBox.Text);
			ScriptEditorBox.SetOccurrences(occurrences);
			ForceSelect();
			ForceRecalculate = false;
		}
		else if (backwards) ScriptEditorBox.ReplacePreviousOccurrence(ReplaceBox.Text);
		else ScriptEditorBox.ReplaceNextOccurrence(ReplaceBox.Text);
	}

    private void InvokeReplaceAll()
    {
		ScriptFinderAndReplacer finder = ScriptEditorBox.CreateFinderAndReplacer();
		finder.SetUseRegex(RegexBox.Checked);
		finder.SetCaseSensitive(CaseBox.Checked);
		List<Occurrence> occurrences = finder.Find(FindBox.Text);
		ScriptEditorBox.ReplaceAllOccurrences(occurrences, ReplaceBox.Text);
    }

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
        if (Window.Width < 750)
        {
            RegexBox.SetVisible(false);
            CaseBox.SetVisible(false);
            RegexBox.SetChecked(false);
            CaseBox.SetChecked(false);
			FindLabel.SetPosition(30, 14);
			ReplaceLabel.SetPosition(9, 50);
			FindBox.SetPadding(70, 7, 200, 0);
			ReplaceBox.SetPadding(70, 44, 200, 0);
		}
        else
        {
			RegexBox.SetVisible(true);
			CaseBox.SetVisible(true);
			FindLabel.SetPosition(180, 14);
			ReplaceLabel.SetPosition(159, 50);
			FindBox.SetPadding(220, 7, 200, 0);
			ReplaceBox.SetPadding(220, 44, 200, 0);
		}
	}
}
