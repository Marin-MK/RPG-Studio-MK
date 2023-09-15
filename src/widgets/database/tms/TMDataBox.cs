using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RPGStudioMK.Widgets;

public partial class DataTypeTMs
{
	void CreateMainContainer(DataContainer parent, Item tm)
	{
		Label typeLabel = new Label(parent);
		typeLabel.SetPosition(385, 74);
		typeLabel.SetSize(30, 18);
		typeLabel.SetText("Type");
		typeLabel.SetFont(Fonts.Paragraph);

		DropdownBox typeBox = new DropdownBox(parent);
		typeBox.SetPosition(431, 71);
		typeBox.SetSize(150, 24);
		typeBox.SetItems(new List<TreeNode>()
		{
			new TreeNode("TM"),
			new TreeNode("TR"),
			new TreeNode("HM")
		});
		Match m = Regex.Match(tm.ID, @"(TM|TR|HM)(\d+)");
		if (m.Success) typeBox.SetSelectedIndex(typeBox.Items.FindIndex(item => item.Text == m.Groups[1].Value));

		Label numberLabel = new Label(parent);
		numberLabel.SetPosition(366, 114);
		numberLabel.SetSize(49, 18);
		numberLabel.SetText("Number");
		numberLabel.SetFont(Fonts.Paragraph);

		NumericBox numberBox = new NumericBox(parent);
		numberBox.SetPosition(431, 108);
		numberBox.SetSize(100, 30);
		numberBox.SetMinValue(0);
		numberBox.SetValue(Convert.ToInt32(m.Groups[2].Value));
		numberBox.OnPlusClicked += _ =>
		{
			// Update name, num, id
			string type = typeBox.SelectedItem.Text;
			Match m = Regex.Match(tm.ID, @"(TM|TR|HM)(\d+)");
			int num = Convert.ToInt32(m.Groups[2].Value);
			int? result = GetFreeMachineNumber(type, num, 1);
			if (result is not null) num = (int) result;
			else return;
			Data.TMsHMs.Remove(tm.ID);
			tm.ID = $"{type}{Utilities.Digits(num, 2)}";
			Data.TMsHMs.Add(tm.ID, tm);
			tm.Name = tm.ID;
			tm.Plural = tm.Name + "s";
			TMList.SelectedItem.SetText($"{tm.Name} - {(tm.Move.Valid ? tm.Move.Move.Name : tm.Move.ID)}");
			TMList.RedrawNodeText(TMList.SelectedItem);
			numberBox.SetValue(num);
			Data.Sources.InvalidateTMs();
			if (!TimerExists("idle")) SetTimer("idle", 1000);
			else ResetTimer("idle");
		};
		numberBox.OnMinusClicked += _ =>
		{
			// Update name, num, id
			string type = typeBox.SelectedItem.Text;
			Match m = Regex.Match(tm.ID, @"(TM|TR|HM)(\d+)");
			int num = Convert.ToInt32(m.Groups[2].Value);
			if (num <= 0) return;
			int? result = GetFreeMachineNumber(type, num, -1);
			if (result is not null) num = (int) result;
			else return;
			Data.TMsHMs.Remove(tm.ID);
			tm.ID = $"{type}{Utilities.Digits(num, 2)}";
			Data.TMsHMs.Add(tm.ID, tm);
			tm.Name = tm.ID;
			tm.Plural = tm.Name + "s";
			TMList.SelectedItem.SetText($"{tm.Name} - {(tm.Move.Valid ? tm.Move.Move.Name : tm.Move.ID)}");
			TMList.RedrawNodeText(TMList.SelectedItem);
			numberBox.SetValue(num);
			Data.Sources.InvalidateTMs();
			if (!TimerExists("idle")) SetTimer("idle", 1000);
			else ResetTimer("idle");
		};
		numberBox.OnValueChanged += _ =>
		{
			// Update name, num, id
			string type = typeBox.SelectedItem.Text;
			Match m = Regex.Match(tm.ID, @"(TM|TR|HM)(\d+)");
			int currentNum = Convert.ToInt32(m.Groups[2].Value);
			int boxNum = numberBox.Value;
			if (IsMachineNumberFree(type, boxNum))
			{
				Data.TMsHMs.Remove(tm.ID);
				tm.ID = $"{type}{Utilities.Digits(boxNum, 2)}";
				Data.TMsHMs.Add(tm.ID, tm);
				tm.Name = tm.ID;
				tm.Plural = tm.Name + "s";
				TMList.SelectedItem.SetText($"{tm.Name} - {(tm.Move.Valid ? tm.Move.Move.Name : tm.Move.ID)}");
				TMList.RedrawNodeText(TMList.SelectedItem);
				Data.Sources.InvalidateTMs();
				if (!TimerExists("idle")) SetTimer("idle", 1000);
				else ResetTimer("idle");
			}
			else if (boxNum > currentNum) numberBox.OnPlusClicked?.Invoke(new BaseEventArgs());
			else if (boxNum < currentNum) numberBox.OnMinusClicked?.Invoke(new BaseEventArgs());
		};

		Label buyPriceLabel = new Label(parent);
		buyPriceLabel.SetPosition(360, 154);
		buyPriceLabel.SetSize(55, 18);
		buyPriceLabel.SetText("Buy Price");
		buyPriceLabel.SetFont(Fonts.Paragraph);

		NumericBox buyPriceBox = new NumericBox(parent);
		buyPriceBox.SetPosition(431, 148);
		buyPriceBox.SetSize(150, 30);
		buyPriceBox.SetMinValue(0);
		buyPriceBox.SetValue(tm.Price);
		buyPriceBox.OnValueChanged += _ => tm.Price = buyPriceBox.Value;

		Label sellPriceLabel = new Label(parent);
		sellPriceLabel.SetPosition(362, 194);
		sellPriceLabel.SetSize(53, 18);
		sellPriceLabel.SetText("Sell Price");
		sellPriceLabel.SetFont(Fonts.Paragraph);

		NumericBox sellPriceBox = new NumericBox(parent);
		sellPriceBox.SetPosition(431, 188);
		sellPriceBox.SetSize(150, 30);
		sellPriceBox.SetMinValue(0);
		sellPriceBox.SetValue(tm.SellPrice);
		sellPriceBox.OnValueChanged += _ => tm.SellPrice = sellPriceBox.Value;

		CheckBox consumableBox = new CheckBox(parent);
		consumableBox.SetPosition(340, 235);
		consumableBox.SetText("Consumable     ");
		consumableBox.SetFont(Fonts.Paragraph);
		consumableBox.SetMirrored(true);
		consumableBox.SetChecked(tm.Consumable);
		consumableBox.OnCheckChanged += _ => tm.Consumable = consumableBox.Checked;

		Label moveLabel = new Label(parent);
		moveLabel.SetPosition(382, 274);
		moveLabel.SetSize(33, 18);
		moveLabel.SetText("Move");
		moveLabel.SetFont(Fonts.Paragraph);

		MoveDropdownBox moveBox = new MoveDropdownBox(parent);
		moveBox.SetPosition(431, 271);
		moveBox.SetSize(150, 24);
		moveBox.SetMove(tm.Move);
		moveBox.OnMoveChanged += _ =>
		{
			tm.Move = moveBox.Move;
			TMList.SelectedItem.SetText($"{tm.Name} - {(tm.Move.Valid ? tm.Move.Move.Name : tm.Move.ID)}");
			TMList.RedrawNodeText(TMList.SelectedItem);
			Data.Sources.InvalidateTMs();
		};

		typeBox.OnSelectionChanged += _ =>
		{
			Data.TMsHMs.Remove(tm.ID);
			tm.ID = typeBox.SelectedItem.Text + "00";
			Data.TMsHMs.Add(tm.ID, tm);
			numberBox.OnPlusClicked?.Invoke(new BaseEventArgs());
		};
		OnUpdate += _ =>
		{
			if (TimerExists("idle") && TimerPassed("idle"))
			{
				RedrawList((Item) TMList.SelectedItem.Object);
				DestroyTimer("idle");
			}
		};

		parent.UpdateSize();
	}

	bool IsMachineNumberFree(string type, int num)
	{
		return !Data.TMsHMs.Values.Any(item =>
		{
			Match m = Regex.Match(item.ID, @"(TM|TR|HM)(\d+)");
			if (m.Success) return m.Groups[1].Value == type && Convert.ToInt32(m.Groups[2].Value) == num;
			return false;
		});
	}

	int? GetFreeMachineNumber(string type, int start, int mod)
	{
		int num = start + mod;
		while (!IsMachineNumberFree(type, num))
		{
			num += mod;
			if (num < 0) return null;
		}
		return num;
	}
}
