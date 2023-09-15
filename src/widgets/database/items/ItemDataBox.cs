using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RPGStudioMK.Widgets;

public partial class DataTypeItems
{
	void CreateMainContainer(DataContainer parent, Item item)
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
		nameBox.SetText(item.Name);
		nameBox.OnTextChanged += _ =>
		{
			item.Name = nameBox.Text;
			Data.Sources.InvalidateItems();
			if (!nameBox.TimerExists("idle")) nameBox.SetTimer("idle", 1000);
			else nameBox.ResetTimer("idle");
			DataList.SelectedItem.SetText(item.Name);
			DataList.RedrawNode(DataList.SelectedItem);
		};
		nameBox.OnUpdate += _ =>
		{
			if (nameBox.TimerExists("idle") && nameBox.TimerPassed("idle"))
			{
				RedrawList((Item) DataList.SelectedItem.Object);
				nameBox.DestroyTimer("idle");
			}
		};

		Label pluralLabel = new Label(parent);
		pluralLabel.SetPosition(381, 114);
		pluralLabel.SetSize(34, 18);
		pluralLabel.SetText("Plural");
		pluralLabel.SetFont(Fonts.Paragraph);

		TextBox pluralBox = new TextBox(parent);
		pluralBox.SetPosition(431, 110);
		pluralBox.SetSize(150, 27);
		pluralBox.SetFont(Fonts.Paragraph);
		pluralBox.SetPopupStyle(false);
		pluralBox.SetText(item.Plural);
		pluralBox.OnTextChanged += _ => item.Plural = pluralBox.Text;

		Label idLabel = new Label(parent);
		idLabel.SetPosition(401, 154);
		idLabel.SetSize(14, 18);
		idLabel.SetText("ID");
		idLabel.SetFont(Fonts.Paragraph);

		TextBox idBox = new TextBox(parent);
		idBox.SetPosition(431, 150);
		idBox.SetSize(150, 27);
		idBox.SetFont(Fonts.Paragraph);
		idBox.SetPopupStyle(false);
		idBox.SetText(item.ID);
		idBox.OnTextChanged += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (match.Success)
			{
				if (Data.Items.ContainsKey(idBox.Text)) return;
				Data.Items.Remove(item.ID);
				item.ID = idBox.Text;
				Data.Items.Add(item.ID, item);
			}
		};
		idBox.TextArea.OnWidgetDeselected += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (!match.Success)
			{
				string newID = Utilities.Internalize(nameBox.Text);
				if (newID != item.ID)
				{
					Data.Items.Remove(item.ID);
					item.ID = newID;
					Data.Items.Add(item.ID, item);
				}
			}
			idBox.SetText(item.ID);
		};

		Label pocketLabel = new Label(parent);
		pocketLabel.SetPosition(375, 194);
		pocketLabel.SetSize(40, 18);
		pocketLabel.SetText("Pocket");
		pocketLabel.SetFont(Fonts.Paragraph);

		DropdownBox pocketBox = new DropdownBox(parent);
		pocketBox.SetPosition(431, 191);
		pocketBox.SetSize(150, 24);
		pocketBox.SetItems(Data.HardcodedData.ItemPockets.Select(p => new TreeNode(p)).ToList());
		pocketBox.SetSelectedIndex(item.Pocket - 1);
		pocketBox.OnSelectionChanged += _ => item.Pocket = pocketBox.SelectedIndex + 1;

		Label buyPriceLabel = new Label(parent);
		buyPriceLabel.SetPosition(360, 234);
		buyPriceLabel.SetSize(55, 18);
		buyPriceLabel.SetText("Buy Price");
		buyPriceLabel.SetFont(Fonts.Paragraph);

		NumericBox buyPriceBox = new NumericBox(parent);
		buyPriceBox.SetPosition(431, 228);
		buyPriceBox.SetSize(150, 30);
		buyPriceBox.SetMinValue(0);
		buyPriceBox.SetValue(item.Price);
		buyPriceBox.OnValueChanged += _ => item.Price = buyPriceBox.Value;

		Label sellPriceLabel = new Label(parent);
		sellPriceLabel.SetPosition(362, 274);
		sellPriceLabel.SetSize(53, 18);
		sellPriceLabel.SetText("Sell Price");
		sellPriceLabel.SetFont(Fonts.Paragraph);

		NumericBox sellPriceBox = new NumericBox(parent);
		sellPriceBox.SetPosition(431, 268);
		sellPriceBox.SetSize(150, 30);
		sellPriceBox.SetMinValue(0);
		sellPriceBox.SetValue(item.SellPrice);
		sellPriceBox.OnValueChanged += _ => item.SellPrice = sellPriceBox.Value;

		CheckBox consumableBox = new CheckBox(parent);
		consumableBox.SetPosition(340, 315);
		consumableBox.SetText("Consumable     ");
		consumableBox.SetFont(Fonts.Paragraph);
		consumableBox.SetMirrored(true);
		consumableBox.SetChecked(item.Consumable);
		consumableBox.OnCheckChanged += _ => item.Consumable = consumableBox.Checked;

		Label fieldUseLabel = new Label(parent);
		fieldUseLabel.SetPosition(361, 354);
		fieldUseLabel.SetSize(54, 18);
		fieldUseLabel.SetText("Field Use");
		fieldUseLabel.SetFont(Fonts.Paragraph);

		DropdownBox fieldUseBox = new DropdownBox(parent);
		fieldUseBox.SetPosition(431, 351);
		fieldUseBox.SetSize(150, 24);
		fieldUseBox.SetItems(Data.HardcodedData.ItemFieldUses.Select(u => new TreeNode(u)).ToList());
		fieldUseBox.SetSelectedIndex(item.FieldUse);
		fieldUseBox.OnSelectionChanged += _ => item.FieldUse = fieldUseBox.SelectedIndex;

		Label battleUseLabel = new Label(parent);
		battleUseLabel.SetPosition(353, 394);
		battleUseLabel.SetSize(62, 18);
		battleUseLabel.SetText("Battle Use");
		battleUseLabel.SetFont(Fonts.Paragraph);

		DropdownBox battleUseBox = new DropdownBox(parent);
		battleUseBox.SetPosition(431, 391);
		battleUseBox.SetSize(150, 24);
		battleUseBox.SetItems(Data.HardcodedData.ItemBattleUses.Select(u => new TreeNode(u)).ToList());
		battleUseBox.SetSelectedIndex(item.BattleUse);
		battleUseBox.OnSelectionChanged += _ => item.BattleUse = battleUseBox.SelectedIndex;

		StringListWidget flagsBox = new StringListWidget(parent);
		flagsBox.SetText("Flags");
		flagsBox.SetPosition(401, 440);
		flagsBox.SetSize(160, 240);
		flagsBox.SetItems(item.Flags);
		flagsBox.OnListChanged += _ => item.Flags = flagsBox.AsStrings;

		Label descriptionLabel = new Label(parent);
		descriptionLabel.SetPosition(447, 690);
		descriptionLabel.SetSize(69, 18);
		descriptionLabel.SetText("Description");
		descriptionLabel.SetFont(Fonts.Paragraph);

		MultilineTextBox descriptionBox = new MultilineTextBox(parent);
		descriptionBox.SetPosition(218, 714);
		descriptionBox.SetSize(526, 112);
		descriptionBox.SetFont(Fonts.Paragraph);
		descriptionBox.SetText(item.Description);
		descriptionBox.OnTextChanged += _ => item.Description = descriptionBox.Text;

		parent.UpdateSize();
	}
}
