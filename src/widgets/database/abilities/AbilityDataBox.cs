using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RPGStudioMK.Widgets;

public partial class DataTypeAbilities
{
	void CreateMainContainer(DataContainer parent, Ability abil)
	{
		Label nameLabel = new Label(parent);
		nameLabel.SetPosition(379, 74);
		nameLabel.SetSize(36, 18);
		nameLabel.SetText("Name");
		nameLabel.SetFont(Fonts.Paragraph);

		TextBox nameBox = new TextBox(parent);
		nameBox.SetPosition(431, 70);
		nameBox.SetSize(150, 27);
		nameBox.SetFont(Fonts.Paragraph);
		nameBox.SetPopupStyle(false);
		nameBox.SetText(abil.Name);
		nameBox.OnTextChanged += _ =>
		{
			abil.Name = nameBox.Text;
			Data.Sources.InvalidateAbilities();
			if (!nameBox.TimerExists("idle")) nameBox.SetTimer("idle", 1000);
			else nameBox.ResetTimer("idle");
			AbilitiesList.SelectedItem.SetText(abil.Name);
			AbilitiesList.RedrawNode(AbilitiesList.SelectedItem);
		};
		nameBox.OnUpdate += _ =>
		{
			if (nameBox.TimerExists("idle") && nameBox.TimerPassed("idle"))
			{
				RedrawList((Ability) AbilitiesList.SelectedItem.Object);
				nameBox.DestroyTimer("idle");
			}
		};

		Label idLabel = new Label(parent);
		idLabel.SetPosition(401, 114);
		idLabel.SetSize(14, 18);
		idLabel.SetText("ID");
		idLabel.SetFont(Fonts.Paragraph);

		TextBox idBox = new TextBox(parent);
		idBox.SetPosition(431, 110);
		idBox.SetSize(150, 27);
		idBox.SetFont(Fonts.Paragraph);
		idBox.SetPopupStyle(false);
		idBox.SetText(abil.ID);
		idBox.OnTextChanged += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (match.Success)
			{
				if (Data.Abilities.ContainsKey(idBox.Text)) return;
				Data.Abilities.Remove(abil.ID);
				abil.ID = idBox.Text;
				Data.Abilities.Add(abil.ID, abil);
			}
		};
		idBox.TextArea.OnWidgetDeselected += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (!match.Success)
			{
				string newID = Utilities.Internalize(nameBox.Text);
				if (newID != abil.ID)
				{
					Data.Abilities.Remove(abil.ID);
					abil.ID = newID;
					Data.Abilities.Add(abil.ID, abil);
				}
			}
			idBox.SetText(abil.ID);
		};

		StringListWidget flagsBox = new StringListWidget(parent);
		flagsBox.SetText("Flags");
		flagsBox.SetPosition(401, 160);
		flagsBox.SetSize(160, 240);
		flagsBox.SetItems(abil.Flags);
		flagsBox.OnListChanged += _ => abil.Flags = flagsBox.AsStrings;

		Label descriptionLabel = new Label(parent);
		descriptionLabel.SetPosition(447, 412);
		descriptionLabel.SetSize(69, 18);
		descriptionLabel.SetText("Description");
		descriptionLabel.SetFont(Fonts.Paragraph);

		MultilineTextBox descriptionBox = new MultilineTextBox(parent);
		descriptionBox.SetPosition(218, 438);
		descriptionBox.SetSize(526, 112);
		descriptionBox.SetFont(Fonts.Paragraph);
		descriptionBox.SetText(abil.Description);
		descriptionBox.OnTextChanged += _ => abil.Description = descriptionBox.Text;

		parent.UpdateSize();
	}
}
