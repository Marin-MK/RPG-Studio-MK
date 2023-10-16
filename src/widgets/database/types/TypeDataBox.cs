using RPGStudioMK.Game;
using System.Text.RegularExpressions;
using System.Xml;

namespace RPGStudioMK.Widgets;

public partial class DataTypeTypes
{
	void CreateMainContainer(DataContainer parent, Type tp)
	{
		Label idLabel = new Label(parent);
		idLabel.SetPosition(401, 104);
		idLabel.SetSize(14, 18);
		idLabel.SetText("ID");
		idLabel.SetFont(Fonts.Paragraph);

		TextBox nameBox = new TextBox(parent);
		nameBox.SetPosition(431, 60);
		nameBox.SetSize(150, 27);
		nameBox.SetFont(Fonts.Paragraph);
		nameBox.SetPopupStyle(false);
		nameBox.SetText(tp.Name);
		nameBox.OnTextChanged += _ =>
		{
			tp.Name = nameBox.Text;
			Data.Sources.InvalidateTypes();
			if (!nameBox.TimerExists("idle")) nameBox.SetTimer("idle", 1000);
			else nameBox.ResetTimer("idle");
			DataList.SelectedItem.SetText(tp.Name);
			DataList.RedrawNode(DataList.SelectedItem);
		};
		nameBox.OnUpdate += _ =>
		{
			if (nameBox.TimerExists("idle") && nameBox.TimerPassed("idle"))
			{
				RedrawList((Type) DataList.SelectedItem.Object);
				nameBox.DestroyTimer("idle");
			}
		};

		Label nameLabel = new Label(parent);
		nameLabel.SetPosition(379, 64);
		nameLabel.SetSize(36, 18);
		nameLabel.SetText("Name");
		nameLabel.SetFont(Fonts.Paragraph);

		TextBox idBox = new TextBox(parent);
		idBox.SetPosition(431, 100);
		idBox.SetSize(150, 27);
		idBox.SetFont(Fonts.Paragraph);
		idBox.SetPopupStyle(false);
		idBox.SetText(tp.ID);
		idBox.OnTextChanged += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (match.Success)
			{
				if (Data.Types.ContainsKey(idBox.Text)) return;
				Data.Types.Remove(tp.ID);
				tp.ID = idBox.Text;
				Data.Types.Add(tp.ID, tp);
			}
		};
		idBox.TextArea.OnWidgetDeselected += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (!match.Success)
			{
				string newID = Utilities.Internalize(nameBox.Text);
				if (newID != tp.ID)
				{
					Data.Types.Remove(tp.ID);
					tp.ID = newID;
					Data.Types.Add(tp.ID, tp);
				}
			}
			idBox.SetText(tp.ID);
		};

		CheckBox specialTypeCheckBox = new CheckBox(parent);
		specialTypeCheckBox.SetPosition(340, 145);
		specialTypeCheckBox.SetText("Special Type    ");
		specialTypeCheckBox.SetFont(Fonts.Paragraph);
		specialTypeCheckBox.SetMirrored(true);
		specialTypeCheckBox.SetChecked(tp.SpecialType);
		specialTypeCheckBox.OnCheckChanged += _ => tp.SpecialType = specialTypeCheckBox.Checked;

		CheckBox pseudoTypeCheckBox = new CheckBox(parent);
		pseudoTypeCheckBox.SetPosition(341, 185);
		pseudoTypeCheckBox.SetText("Pseudo Type   ");
		pseudoTypeCheckBox.SetFont(Fonts.Paragraph);
		pseudoTypeCheckBox.SetMirrored(true);
		pseudoTypeCheckBox.SetChecked(tp.PseudoType);
		pseudoTypeCheckBox.OnCheckChanged += _ => tp.PseudoType = pseudoTypeCheckBox.Checked;

		Label iconPosLabel = new Label(parent);
		iconPosLabel.SetPosition(337, 224);
		iconPosLabel.SetSize(78, 18);
		iconPosLabel.SetText("Icon Position");
		iconPosLabel.SetFont(Fonts.Paragraph);

		NumericBox iconPosBox = new NumericBox(parent);
		iconPosBox.SetPosition(431, 218);
		iconPosBox.SetSize(150, 30);
		iconPosBox.SetValue(tp.IconPosition is null ? -1 : (int) tp.IconPosition);
		iconPosBox.OnValueChanged += _ => tp.IconPosition = iconPosBox.Value == -1 ? null : iconPosBox.Value;

		parent.UpdateSize();
	}

	void CreateRelationsContainer(DataContainer parent, Type tp)
	{
		TypeListWidget weaknessList = new TypeListWidget(parent);
		weaknessList.SetPosition(126, 54);
		weaknessList.SetSize(160, 216);
		weaknessList.SetText("Weaknesses");
		weaknessList.SetItems(tp.Weaknesses);
		weaknessList.OnListChanged += _ => tp.Weaknesses = weaknessList.AsResolvers;

		TypeListWidget resistanceList = new TypeListWidget(parent);
		resistanceList.SetPosition(406, 54);
		resistanceList.SetSize(160, 216);
		resistanceList.SetText("Resistances");
		resistanceList.SetItems(tp.Resistances);
		resistanceList.OnListChanged += _ => tp.Resistances = resistanceList.AsResolvers;

		TypeListWidget immunityList = new TypeListWidget(parent);
		immunityList.SetPosition(686, 54);
		immunityList.SetSize(160, 216);
		immunityList.SetText("Immunities");
		immunityList.SetItems(tp.Immunities);
		immunityList.OnListChanged += _ => tp.Immunities = immunityList.AsResolvers;

		parent.UpdateSize();
	}

	void CreateFlagsContainer(DataContainer parent, Type tp)
	{
		StringListWidget flagsBox = new StringListWidget(parent);
		flagsBox.SetText("Flags");
		flagsBox.SetPosition(401, 74);
		flagsBox.SetSize(160, 240);
		flagsBox.SetItems(tp.Flags);
		flagsBox.OnListChanged += _ => tp.Flags = flagsBox.AsStrings;

		parent.UpdateSize();
	}
}
