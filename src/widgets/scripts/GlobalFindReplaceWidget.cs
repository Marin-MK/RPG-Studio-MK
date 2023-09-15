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
	SubmodeView TabControl;
	Label findLabel;
	TextBox findBox;
	CheckBox caseCheckBox;
	CheckBox regexCheckBox;
	Label searchOptionsLabel;
	Label searchInLabel;
	DropdownBox searchInBox;
	ListBox resultsBox;
	Label resultsLabel;
	Label occurrencesLabel;
	Button findAllButton;

	public GlobalFindReplaceWidget(IContainer parent) : base(parent)
    {
        SetBackgroundColor(40, 62, 84);
		SetSize(500, 500);

		Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(500, 1, new Color(86, 108, 134)));
		Sprites["bg"].Y = 29;

		TabControl = new SubmodeView(this);
		TabControl.SetHeight(29);
		TabControl.SetHDocked(true);
		TabControl.CreateTab("Find");
		TabControl.CreateTab("Replace");
		TabControl.SetHeaderHeight(31);

        findLabel = new Label(this);
		findLabel.SetPosition(53, 62);
		findLabel.SetSize(29, 18);
		findLabel.SetPadding(0, 0, 0, 95);
		findLabel.SetText("Find:");
		findLabel.SetFont(Fonts.Paragraph);

		findBox = new TextBox(this);
		findBox.SetPosition(0, 58);
		findBox.SetHeight(27);
		findBox.SetHDocked(true);
		findBox.SetPadding(105, 0, 19, 90);
		findBox.SetFont(Fonts.Monospace);
		findBox.OnEnterPressed += _ => FindAll();

		caseCheckBox = new CheckBox(this);
		caseCheckBox.SetPosition(51, 216);
		caseCheckBox.SetText("Case-Sensitive");
		caseCheckBox.SetFont(Fonts.Paragraph);

		regexCheckBox = new CheckBox(this);
		regexCheckBox.SetPosition(51, 189);
		regexCheckBox.SetText("Use regular expressions");
		regexCheckBox.SetFont(Fonts.Paragraph);

		searchOptionsLabel = new Label(this);
		searchOptionsLabel.SetPosition(31, 161);
		searchOptionsLabel.SetSize(93, 18);
		searchOptionsLabel.SetText("Search Options");
		searchOptionsLabel.SetFont(Fonts.Paragraph);

		searchInLabel = new Label(this);
		searchInLabel.SetPosition(31, 110);
		searchInLabel.SetSize(58, 18);
		searchInLabel.SetPadding(0, 0, 0, 95);
		searchInLabel.SetText("Search in:");
		searchInLabel.SetFont(Fonts.Paragraph);

		searchInBox = new DropdownBox(this);
		searchInBox.SetPosition(0, 108);
		searchInBox.SetHeight(24);
		searchInBox.SetHDocked(true);
		searchInBox.SetPadding(105, 0, 19, 0);
		searchInBox.SetItems(new List<TreeNode>()
		{
			new TreeNode("All scripts"),
			new TreeNode("Open scripts"),
			new TreeNode("Core scripts"),
			new TreeNode("Plugin scripts"),
			new TreeNode("Custom scripts")
		});
		searchInBox.SetSelectedIndex(0);

		resultsBox = new ListBox(this);
		resultsBox.SetDocked(true);
		resultsBox.SetPadding(15, 273, 15, 15);
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
				(Script script, Occurrence occ) = ((Script, Occurrence)) resultsBox.SelectedItem.Object;
				Editor.MainWindow.ScriptingWidget.PreviewOccurrence(script, occ);
				resultsBox.WidgetSelected(new BaseEventArgs());
			}
		};

		resultsLabel = new Label(this);
		resultsLabel.SetPosition(15, 245);
		resultsLabel.SetSize(49, 16);
		resultsLabel.SetText("Results");
		resultsLabel.SetFont(Fonts.ParagraphBold);

		occurrencesLabel = new Label(this);
		occurrencesLabel.SetRightDocked(true);
		occurrencesLabel.SetPadding(0, 245, 15, 0);
		occurrencesLabel.SetVisible(false);
		occurrencesLabel.SetText($"Occurrence ({resultsBox.SelectedIndex} / {resultsBox.Items.Count})");

		findAllButton = new Button(this);
		findAllButton.SetPosition(413, 206);
		findAllButton.SetSize(108, 33);
		findAllButton.SetPadding(0, 0, 15, 0);
		findAllButton.SetRightDocked(true);
		findAllButton.SetFont(Fonts.ParagraphBold);
		findAllButton.SetText("Find All");
		findAllButton.OnClicked = _ => FindAll();
    }

	void FindAll()
	{
		List<Script> scriptsToSearch = new List<Script>();
		List<List<string>> scriptsToLines = new List<List<string>>();
		switch(searchInBox.SelectedIndex)
		{
			case 0:
				// All scripts
				scriptsToSearch = Data.Scripts;
				break;
			case 1:
				// Open scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.OpenScripts;
				break;
			case 2:
				// Core scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.CoreScriptNode.GetAllChildren(true).Select(n => (Script) ((TreeNode) n).Object).ToList();
				break;
			case 3:
				// Plugin scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.PluginScriptNode.GetAllChildren(true).Select(n => (Script) ((TreeNode) n).Object).ToList();
				break;
			case 4:
				// Custom scripts
				scriptsToSearch = Editor.MainWindow.ScriptingWidget.CustomScriptNode.GetAllChildren(true).Select(n => (Script) ((TreeNode) n).Object).ToList();
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
		resultsBox.SetItems(occurrences.Select(o =>
		{
			Script script = scriptsToSearch[o.ID];
			string text = script.Name;
			text += ":" + o.LineNumber.ToString();
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
        else occurrencesLabel.SetVisible(false);
    }
}
