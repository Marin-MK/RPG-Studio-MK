using RPGStudioMK.Game;
using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class GlobalFindReplaceWidget : Widget
{
	public int SelectedTab => TabControl.SelectedIndex;

	SubmodeView TabControl;
	Label findLabel;
	TextBox findBox;
	Label replaceLabel;
	TextBox replaceBox;
	CheckBox caseCheckBox;
	CheckBox regexCheckBox;
	Label searchOptionsLabel;
	Label searchInLabel;
	DropdownBox searchInBox;
	ListBox resultsBox;
	Label resultsLabel;
	Label occurrencesLabel;
	Button findAllButton;
	Button replaceNextButton;
	Button replaceAllButton;

	bool useFindAllCache = false;

	public GlobalFindReplaceWidget(IContainer parent) : base(parent)
	{
		SetBackgroundColor(40, 62, 84);
		SetSize(500, 500);

		OnWidgetSelected += WidgetSelected;

		Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(500, 1, new Color(86, 108, 134)));
		Sprites["bg"].Y = 29;

		TabControl = new SubmodeView(this);
		TabControl.SetHeight(29);
		TabControl.SetHDocked(true);
		TabControl.CreateTab("Find");
		TabControl.CreateTab("Replace");
		TabControl.SetHeaderHeight(31);
		TabControl.OnSelectionChanged += _ => UpdateWidgetPositions();

		findLabel = new Label(this);
		findLabel.SetPosition(53, 62);
		findLabel.SetSize(29, 18);
		findLabel.SetText("Find:");
		findLabel.SetFont(Fonts.Paragraph);

		findBox = new TextBox(this);
		findBox.SetHeight(27);
		findBox.SetHDocked(true);
		findBox.SetPadding(105, 58, 19, 90);
		findBox.SetFont(Fonts.Monospace);
		findBox.OnEnterPressed += _ => FindAll();
		findBox.OnTextChanged += _ => useFindAllCache = false;

		replaceLabel = new Label(this);
		replaceLabel.SetPosition(31, 100);
		replaceLabel.SetSize(29, 18);
		replaceLabel.SetText("Replace:");
		replaceLabel.SetFont(Fonts.Paragraph);

		replaceBox = new TextBox(this);
		replaceBox.SetHeight(27);
		replaceBox.SetHDocked(true);
		replaceBox.SetPadding(105, 96, 19, 90);
		replaceBox.SetFont(Fonts.Monospace);
		replaceBox.OnEnterPressed += _ => FindAll();

		caseCheckBox = new CheckBox(this);
		caseCheckBox.SetText("Case-Sensitive");
		caseCheckBox.SetFont(Fonts.Paragraph);
		caseCheckBox.OnCheckChanged += _ => useFindAllCache = false;

		regexCheckBox = new CheckBox(this);
		regexCheckBox.SetText("Use regular expressions");
		regexCheckBox.SetFont(Fonts.Paragraph);
		regexCheckBox.OnCheckChanged += _ => useFindAllCache = false;

		searchOptionsLabel = new Label(this);
		searchOptionsLabel.SetSize(93, 18);
		searchOptionsLabel.SetText("Search Options");
		searchOptionsLabel.SetFont(Fonts.Paragraph);

		searchInLabel = new Label(this);
		searchInLabel.SetSize(58, 18);
		searchInLabel.SetText("Search in:");
		searchInLabel.SetFont(Fonts.Paragraph);

		searchInBox = new DropdownBox(this);
		searchInBox.SetHeight(24);
		searchInBox.SetHDocked(true);
		searchInBox.SetItems(new List<TreeNode>()
		{
			new TreeNode("All scripts"),
			new TreeNode("Open scripts"),
			new TreeNode("Current script"),
			new TreeNode("Core scripts"),
			new TreeNode("Plugin scripts"),
			new TreeNode("Custom scripts")
		});
		searchInBox.SetSelectedIndex(0);
		searchInBox.OnSelectionChanged += _ => useFindAllCache = false;

		resultsBox = new ListBox(this);
		resultsBox.SetDocked(true);
		resultsBox.SetFont(Fonts.Monospace);
		resultsBox.SetLineHeight(30);
		resultsBox.OnSelectionChanged += _ =>
		{
			if (resultsBox.Items.Count > 0)
			{
				occurrencesLabel.SetText($"Occurrence ({resultsBox.SelectedIndex} / {resultsBox.Items.Count})");
				occurrencesLabel.RedrawText(true);
				occurrencesLabel.UpdatePositionAndSizeIfDocked();
			}
			if (resultsBox.SelectedIndex != -1)
			{
				(Script script, Occurrence occ) = ((Script, Occurrence))resultsBox.SelectedItem.Object;
				Editor.MainWindow.ScriptingWidget.PreviewOccurrence(script, occ);
				resultsBox.WidgetSelected(new BaseEventArgs());
			}
		};

		resultsLabel = new Label(this);
		resultsLabel.SetSize(49, 16);
		resultsLabel.SetText("Results");
		resultsLabel.SetFont(Fonts.ParagraphBold);

		occurrencesLabel = new Label(this);
		occurrencesLabel.SetRightDocked(true);
		occurrencesLabel.SetVisible(false);
		occurrencesLabel.SetText($"Occurrence ({resultsBox.SelectedIndex} / {resultsBox.Items.Count})");

		findAllButton = new Button(this);
		findAllButton.SetSize(108, 33);
		findAllButton.SetRightDocked(true);
		findAllButton.SetFont(Fonts.ParagraphBold);
		findAllButton.SetText("Find All");
		findAllButton.OnClicked = _ => FindAll();

		replaceNextButton = new Button(this);
		replaceNextButton.SetSize(108, 33);
		replaceNextButton.SetRightDocked(true);
		replaceNextButton.SetFont(Fonts.ParagraphBold);
		replaceNextButton.SetText("Replace One");
		replaceNextButton.OnClicked = _ => ReplaceNext();

		replaceAllButton = new Button(this);
		replaceAllButton.SetSize(108, 33);
		replaceAllButton.SetRightDocked(true);
		replaceAllButton.SetFont(Fonts.ParagraphBold);
		replaceAllButton.SetText("Replace All");
		replaceAllButton.OnClicked = _ => ReplaceAll();

		UpdateWidgetPositions();

		RegisterShortcuts(new List<Shortcut>()
		{
			new Shortcut(this, new Key(Keycode.ESCAPE), _ => Dispose(), true),
			new Shortcut(this, new Key(Keycode.TAB), _ =>
			{
				replaceBox.TextArea.WidgetSelected(new BaseEventArgs());
				replaceBox.Redraw();
			}, true, e => e.Value = findBox.TextArea.SelectedWidget && TabControl.SelectedIndex == 1),
			new Shortcut(this, new Key(Keycode.TAB, Keycode.SHIFT), _ =>
			{
				findBox.TextArea.WidgetSelected(new BaseEventArgs());
				findBox.Redraw();
			}, true, e => e.Value = replaceBox.TextArea.SelectedWidget),
			new Shortcut(this, new Key(Keycode.ENTER), _ => FindAll()),
			new Shortcut(this, new Key(Keycode.ENTER, Keycode.SHIFT), _ => FindAll())
		});
	}

	public void FocusFindBox()
	{
		findBox.TextArea.WidgetSelected(new BaseEventArgs());
		//findBox.Redraw();
	}

	public void SelectTab(int index)
	{
		TabControl.SelectTab(index);
	}

	void UpdateWidgetPositions()
	{
		if (TabControl.SelectedIndex == 0) // Find
		{
			caseCheckBox.SetPosition(51, 216);
			regexCheckBox.SetPosition(51, 189);
			searchOptionsLabel.SetPosition(31, 161);
			searchInLabel.SetPosition(31, 110);
			searchInLabel.SetPadding(0, 0, 0, 95);
			searchInBox.SetPadding(105, 108, 19, 0);
			resultsBox.SetPadding(15, 273, 15, 15);
			resultsLabel.SetPosition(15, 245);
			occurrencesLabel.SetPadding(0, 245, 15, 0);
			findAllButton.SetPadding(0, 206, 15, 0);
			replaceNextButton.SetVisible(false);
			replaceAllButton.SetVisible(false);
			replaceLabel.SetVisible(false);
			replaceBox.SetVisible(false);
		}
		else // Replace
		{
			caseCheckBox.SetPosition(51, 254);
			regexCheckBox.SetPosition(51, 227);
			searchOptionsLabel.SetPosition(31, 199);
			searchInLabel.SetPosition(31, 148);
			searchInBox.SetPadding(105, 146, 19, 0);
			resultsBox.SetPadding(15, 311, 15, 15);
			resultsLabel.SetPosition(15, 283);
			occurrencesLabel.SetPadding(0, 283, 15, 0);
			findAllButton.SetPadding(0, 244, 120, 0);
			replaceNextButton.SetPadding(0, 214, 15, 0);
			replaceAllButton.SetPadding(0, 244, 15, 0);
			replaceNextButton.SetVisible(true);
			replaceAllButton.SetVisible(true);
			replaceLabel.SetVisible(true);
			replaceBox.SetVisible(true);
		}
	}

	List<(Script, Occurrence)> FindAll(bool visual = true)
	{
		List<Script> scriptsToSearch = new List<Script>();
		List<List<string>> scriptsToLines = new List<List<string>>();
		switch (searchInBox.SelectedIndex)
		{
			case 0:
				// All scripts
				scriptsToSearch = Data.Scripts;
				break;
			case 1:
				// Open scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.OpenScripts;
				break;
			case 2: // Current script
				if (Editor.MainWindow.ScriptingWidget.OpenScript is not null) scriptsToSearch.Add(Editor.MainWindow.ScriptingWidget.OpenScript);
				break;
			case 3:
				// Core scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.CoreScriptNode.GetAllChildren(true).Select(n => (Script)((TreeNode)n).Object).ToList();
				break;
			case 4:
				// Plugin scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.PluginScriptNode.GetAllChildren(true).Select(n => (Script)((TreeNode)n).Object).Where(s => s is not null).ToList();
				break;
			case 5:
				// Custom scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.CustomScriptNode.GetAllChildren(true).Select(n => (Script)((TreeNode)n).Object).Where(s => s is not null).ToList();
				break;
		}
		ScriptFinderAndReplacer finder = new ScriptFinderAndReplacer();
		finder.SetUseRegex(regexCheckBox.Checked);
		finder.SetCaseSensitive(caseCheckBox.Checked);
		List<Occurrence> occurrences = new List<Occurrence>();
		for (int i = 0; i < scriptsToSearch.Count; i++)
		{
			Script script = scriptsToSearch[i];
			List<string> lines = script.Content.Split('\n').ToList();
			scriptsToLines.Add(lines);
			finder.SetScript(lines);
			List<Occurrence> scriptOccurrences = finder.Find(findBox.Text);
			foreach (Occurrence occ in scriptOccurrences)
			{
				occ.ID = i;
				occurrences.Add(occ);
				if (occurrences.Count >= 50000) break;
			}
			if (occurrences.Count >= 50000) break;
		}
		if (visual)
		{
			resultsBox.SetItems(occurrences.Select(o =>
			{
				Script script = scriptsToSearch[o.ID];
				string text = script.Name;
				text += ":" + (o.LineNumber + 1).ToString();
				text += " | ";
				text += scriptsToLines[o.ID][o.LineNumber].Trim();
				return new TreeNode(text, (script, o));
			}).ToList());
			if (occurrences.Count > 0)
			{
				resultsBox.SetSelectedIndex(0);
				occurrencesLabel.SetVisible(true);
				occurrencesLabel.SetText($"Occurrence ({resultsBox.SelectedIndex} / {resultsBox.Items.Count})");
				occurrencesLabel.RedrawText(true);
				occurrencesLabel.UpdatePositionAndSizeIfDocked();
			}
			else
			{
				occurrencesLabel.SetVisible(false);
				new MessageBox("Find", "No occurrences found.", ButtonType.OK, IconType.Info);
			}
		}
		useFindAllCache = true;
		return occurrences.Select(occ => (scriptsToSearch[occ.ID], occ)).ToList();
	}

	void ReplaceNext()
	{
		if (!useFindAllCache) FindAll();
		if (resultsBox.Items.Count == 0)
		{
			new MessageBox("Replacer", "No occurrences found.", ButtonType.OK, IconType.Info);
			return;
		}
		(Script script, Occurrence occ) = ((Script, Occurrence)) resultsBox.SelectedItem.Object;
		int diff = 0;
		if (Editor.MainWindow.ScriptingWidget.OpenScript == script)
		{
			diff = Editor.MainWindow.ScriptingWidget.ReplaceSingleOccurrence(occ, replaceBox.Text);
		}
		else
		{
			diff = ReplaceClosedOccurrence(script, occ, replaceBox.Text);
		}
		resultsBox.Items.ForEach(n =>
		{
			(Script s, Occurrence o) = ((Script, Occurrence)) n.Object;
			if (s == script)
			{
				if (o.GlobalIndex > occ.GlobalIndex) o.GlobalIndex += diff;
				if (occ.LineNumber == o.LineNumber && o.IndexInLine > occ.IndexInLine) o.IndexInLine += diff;
			}
		});
		resultsBox.RemoveItem(resultsBox.SelectedItem);
		resultsBox.WidgetSelected(new BaseEventArgs());
	}

	void ReplaceAll()
	{
		List<(Script script, Occurrence occ)> occurrences = useFindAllCache ? resultsBox.Items.Select(n => ((Script, Occurrence)) n.Object).ToList() : FindAll();
		if (occurrences.Count == 0)
		{
			new MessageBox("Replace", "No occurrences were replaced.", ButtonType.OK, IconType.Info);
			return;
		}
		for (int i = 0; i < occurrences.Count; i++)
		{
			var item = occurrences[i];
			int diff = 0;
			if (Editor.MainWindow.ScriptingWidget.OpenScript == item.script)
			{
				diff = Editor.MainWindow.ScriptingWidget.ReplaceSingleOccurrence(item.occ, replaceBox.Text);
			}
			else
			{
				diff = ReplaceClosedOccurrence(item.script, item.occ, replaceBox.Text);
			}
			for (int j = i; j < occurrences.Count; j++)
			{
				if (occurrences[j].script != item.script) break;
				if (occurrences[j].occ.GlobalIndex > item.occ.GlobalIndex) occurrences[j].occ.GlobalIndex += diff;
				if (i == j && occurrences[j].occ.IndexInLine > item.occ.IndexInLine) occurrences[j].occ.IndexInLine += diff;
			}
		}
		resultsBox.WidgetSelected(new BaseEventArgs());
		int scriptCount = occurrences.DistinctBy(item => item.script).Count();
		resultsBox.SetItems(new List<TreeNode>());
		new MessageBox("Replace", $"Replaced {occurrences.Count} occurrence{(occurrences.Count > 1 ? "s" : "")} in {scriptCount} script{(scriptCount > 1 ? "s" : "")}.", ButtonType.OK, IconType.Info);
	}

	int ReplaceClosedOccurrence(Script script, Occurrence occurrence, string pattern)
	{
		for (int i = 0; i < occurrence.Captures.Length; i++)
		{
			pattern = pattern.Replace($"${i + 1}", occurrence.Captures[i]);
		}
		script.Content = script.Content
			.Remove(occurrence.GlobalIndex, occurrence.Length)
			.Insert(occurrence.GlobalIndex, pattern);
		return pattern.Length - occurrence.Length;
	}
}
