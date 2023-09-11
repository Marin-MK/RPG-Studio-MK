using System;
using System.Collections.Generic;
using System.IO;

namespace RPGStudioMK.Widgets;

public class PublishFinishedWindow : PopupWindow
{
	public bool Apply = false;

	Label publishedLabel;
	Label shaLabel;
	TextBox shaBox;
	Button copyButton;
	Label filenameLabel;
	TextBox filenameBox;
	Button openButton;

	public PublishFinishedWindow(string filename, string sha)
	{
		SetTitle("Publisher");
		MinimumSize = MaximumSize = new Size(653, 243);
		SetSize(MinimumSize);
		Center();

		publishedLabel = new Label(this);
		publishedLabel.SetPosition(213, 49);
		publishedLabel.SetSize(230, 18);
		publishedLabel.SetText("The project was successfully published!");
		publishedLabel.SetFont(Fonts.Paragraph);

		filenameLabel = new Label(this);
		filenameLabel.SetPosition(71, 95);
		filenameLabel.SetSize(57, 18);
		filenameLabel.SetText("Filename:");
		filenameLabel.SetFont(Fonts.Paragraph);

		filenameBox = new TextBox(this);
		filenameBox.SetPosition(139, 91);
		filenameBox.SetSize(379, 27);
		filenameBox.SetFont(Fonts.Paragraph);
		filenameBox.SetReadOnly(true);
		filenameBox.SetText(filename.Replace('\\', '/'));

		openButton = new Button(this);
		openButton.SetPosition(524, 88);
		openButton.SetSize(60, 33);
		openButton.SetFont(Fonts.ParagraphBold);
		openButton.SetText("Open");
		openButton.OnClicked += _ => Utilities.OpenFolder(Path.GetDirectoryName(filename));

		shaLabel = new Label(this);
		shaLabel.SetPosition(75, 135);
		shaLabel.SetSize(53, 18);
		shaLabel.SetText("SHA256:");
		shaLabel.SetFont(Fonts.Paragraph);

		shaBox = new TextBox(this);
		shaBox.SetPosition(139, 131);
		shaBox.SetSize(379, 27);
		shaBox.SetFont(Fonts.Paragraph);
		shaBox.SetReadOnly(true);
		shaBox.SetText(sha);

		copyButton = new Button(this);
		copyButton.SetPosition(524, 128);
		copyButton.SetSize(60, 33);
		copyButton.SetFont(Fonts.ParagraphBold);
		copyButton.SetText("Copy");
		copyButton.OnClicked += _ => Clipboard.SetContent(shaBox.Text);

		CreateButton("OK", _ => OK());

		RegisterShortcuts(new List<Shortcut>()
		{
			new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
		});
	}

	private void OK()
	{
		Apply = true;
		Close();
	}
}
