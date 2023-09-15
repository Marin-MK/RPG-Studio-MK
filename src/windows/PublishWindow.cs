using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class PublishWindow : PopupWindow
{
	public bool Apply = false;
	public List<string> Options = new List<string>();
	public string Filename;

	List<(string ID, CheckBox CheckBox)> CheckBoxes = new List<(string, CheckBox)>();

	Label aboutLabel;
	Container aboutBox;
	Label gameTitleLabel;
	Label gameTitleBox;
	//Label gameVersionLabel;
	//TextBox gameVersionBox;
	Label locationLabel;
	BrowserBox locationBox;
	Label filenameLabel;
	TextBox filenameBox;

	Container optionsBox;
	Label optionsLabel;

	public PublishWindow()
	{
		SetTitle("Publisher");
		MinimumSize = MaximumSize = new Size(463, 300 + ProjectPublisher.PublishOptions.Count * 30);
		SetSize(MinimumSize);
		Center();

		aboutLabel = new Label(this);
		aboutLabel.SetPosition(31, 54);
		aboutLabel.SetSize(41, 16);
		aboutLabel.SetText("About");
		aboutLabel.SetFont(Fonts.ParagraphBold);

		aboutBox = new Container(this);
		aboutBox.SetPosition(47, 79);
		aboutBox.SetSize(362, 163);

		gameTitleLabel = new Label(aboutBox);
		gameTitleLabel.SetPosition(4, 12);
		gameTitleLabel.SetSize(67, 18);
		gameTitleLabel.SetText("Game Title:");
		gameTitleLabel.SetFont(Fonts.Paragraph);

		//gameVersionLabel = new Label(aboutBox);
		//gameVersionLabel.SetPosition(4, 52);
		//gameVersionLabel.SetSize(87, 18);
		//gameVersionLabel.SetText("Game Version:");
		//gameVersionLabel.SetFont(Fonts.Paragraph);

		locationLabel = new Label(aboutBox);
		locationLabel.SetPosition(4, 52);
		locationLabel.SetSize(55, 18);
		locationLabel.SetText("Location:");
		locationLabel.SetFont(Fonts.Paragraph);

		filenameLabel = new Label(aboutBox);
		filenameLabel.SetPosition(4, 93);
		filenameLabel.SetSize(57, 18);
		filenameLabel.SetText("Filename:");
		filenameLabel.SetFont(Fonts.Paragraph);

		//gameVersionBox = new TextBox(aboutBox);
		//gameVersionBox.SetPosition(108, 48);
		//gameVersionBox.SetSize(240, 27);
		//gameVersionBox.SetFont(Fonts.Paragraph);
		//gameVersionBox.SetText(Editor.ProjectSettings.ProjectVersion);
		//gameVersionBox.OnTextChanged += _ =>
		//{
		//	if (filenameBox.Text == GenerateFilename())
		//	{
		//		Editor.ProjectSettings.ProjectVersion = gameVersionBox.Text;
		//		filenameBox.SetText(GenerateFilename());
		//	}
		//};

		gameTitleBox = new Label(aboutBox);
		gameTitleBox.SetPosition(108, 12);
		gameTitleBox.SetSize(149, 18);
		gameTitleBox.SetText(Editor.ProjectSettings.ProjectName);
		gameTitleBox.SetFont(Fonts.Paragraph);

		locationBox = new BrowserBox(aboutBox);
		locationBox.SetPosition(108, 49);
		locationBox.SetSize(240, 25);
		locationBox.SetFont(Fonts.Paragraph);
		if (!string.IsNullOrEmpty(Editor.ProjectSettings.LastExportLocation) && Directory.Exists(Editor.ProjectSettings.LastExportLocation))
		{
			locationBox.SetText(Editor.ProjectSettings.LastExportLocation);
		}
		locationBox.OnDropDownClicked += _ =>
		{
			OpenFileDialog ofd = new OpenFileDialog();
			string? folderChoice = ofd.ChooseFolder();
			if (!string.IsNullOrEmpty(folderChoice))
			{
				locationBox.SetText(folderChoice);
				Editor.ProjectSettings.LastExportLocation = folderChoice;
				Buttons[1].SetEnabled(true);
			}
			else Buttons[1].SetEnabled(false);
		};

		filenameBox = new TextBox(aboutBox);
		filenameBox.SetPosition(108, 90);
		filenameBox.SetSize(240, 25);
		filenameBox.SetFont(Fonts.Paragraph);
		filenameBox.SetText(GenerateFilename());
		filenameBox.OnTextChanged += _ =>
		{
			if (Path.GetInvalidFileNameChars().Any(c => filenameBox.Text.Contains(c)))
			{
				string newFilename = filenameBox.Text.ToCharArray().Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToString();
				filenameBox.SetText(newFilename);
			}
		};

		optionsLabel = new Label(this);
		optionsLabel.SetPosition(31, 211);
		optionsLabel.SetSize(53, 17);
		optionsLabel.SetText("Options");
		optionsLabel.SetFont(Fonts.ParagraphBold);

		optionsBox = new Container(this);
		optionsBox.SetPosition(47, 236);
		optionsBox.SetSize(362, ProjectPublisher.PublishOptions.Count * 30 - 4);

		int y = 4;
		ProjectPublisher.PublishOptions.ForEach(o =>
		{
			CheckBox cbox = new CheckBox(optionsBox);
			cbox.SetPosition(4, y);
			cbox.SetText(o.Text);
			cbox.SetChecked(o.Checked);
			cbox.SetFont(Fonts.Paragraph);
			y += 30;
			CheckBoxes.Add((o.ID, cbox));
		});

		CreateButton("Cancel", _ => Cancel());
		CreateButton("OK", _ => OK());

		Buttons[1].SetEnabled(!string.IsNullOrEmpty(locationBox.Text));

		RegisterShortcuts(new List<Shortcut>()
		{
			new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
		});
	}

	private string GenerateFilename()
	{
		return (Editor.ProjectSettings.ProjectName ?? "Untitled Project").Replace(' ', '-') + "-" + Editor.ProjectSettings.ProjectVersion;
	}

	private void OK()
	{
		Apply = true;
		Options = CheckBoxes.Where(o => o.CheckBox.Checked).Select(o => o.ID).ToList();
		Filename = filenameBox.Text;
		Close();
	}

	private void Cancel()
	{
		Close();
	}
}
