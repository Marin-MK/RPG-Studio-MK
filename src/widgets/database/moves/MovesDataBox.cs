using RPGStudioMK.Game;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Widgets;

public partial class DataTypeMoves
{
	void CreateMainContainer(DataContainer parent, Move mov)
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
		nameBox.SetText(mov.Name);
		nameBox.OnTextChanged += _ =>
		{
			mov.Name = nameBox.Text;
			Data.Sources.InvalidateMoves();
			if (!nameBox.TimerExists("idle")) nameBox.SetTimer("idle", 1000);
			else nameBox.ResetTimer("idle");
			MovesList.SelectedItem.SetText(mov.Name);
			MovesList.RedrawNode(MovesList.SelectedItem);
		};
		nameBox.OnUpdate += _ =>
		{
			if (nameBox.TimerExists("idle") && nameBox.TimerPassed("idle"))
			{
				RedrawList((Move) MovesList.SelectedItem.Object);
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
		idBox.SetText(mov.ID);
		idBox.OnTextChanged += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (match.Success)
			{
				if (Data.Moves.ContainsKey(idBox.Text)) return;
				Data.Moves.Remove(mov.ID);
				mov.ID = idBox.Text;
				Data.Moves.Add(mov.ID, mov);
			}
		};
		idBox.TextArea.OnWidgetDeselected += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (!match.Success)
			{
				string newID = Utilities.Internalize(nameBox.Text);
				if (newID != mov.ID)
				{
					Data.Moves.Remove(mov.ID);
					mov.ID = newID;
					Data.Moves.Add(mov.ID, mov);
				}
			}
			idBox.SetText(mov.ID);
		};

		Label typeLabel = new Label(parent);
		typeLabel.SetPosition(385, 154);
		typeLabel.SetSize(30, 18);
		typeLabel.SetText("Type");
		typeLabel.SetFont(Fonts.Paragraph);

		TypeDropdownBox typeBox = new TypeDropdownBox(parent);
		typeBox.SetPosition(431, 151);
		typeBox.SetSize(150, 24);
		typeBox.SetType(mov.Type);
		typeBox.OnTypeChanged += _ => mov.Type = typeBox.Type;

		Label categoryLabel = new Label(parent);
		categoryLabel.SetPosition(359, 194);
		categoryLabel.SetSize(56, 18);
		categoryLabel.SetText("Category");
		categoryLabel.SetFont(Fonts.Paragraph);

		DropdownBox categoryBox = new DropdownBox(parent);
		categoryBox.SetPosition(431, 191);
		categoryBox.SetSize(150, 24);
		categoryBox.SetItems(Data.HardcodedData.MoveCategories.Select(c => new ListItem(c)).ToList());
		if (Data.HardcodedData.MoveCategories.Contains(mov.Category)) categoryBox.SetSelectedIndex(Data.HardcodedData.MoveCategories.IndexOf(mov.Category));
		else categoryBox.SetText(mov.Category);
		categoryBox.OnSelectionChanged += _ => mov.Category = categoryBox.SelectedItem.Name;

		Label powerLabel = new Label(parent);
		powerLabel.SetPosition(333, 234);
		powerLabel.SetSize(82, 18);
		powerLabel.SetText("Base Damage");
		powerLabel.SetFont(Fonts.Paragraph);

		NumericBox powerBox = new NumericBox(parent);
		powerBox.SetPosition(431, 228);
		powerBox.SetSize(100, 30);
		powerBox.SetMinValue(0);
		powerBox.SetValue(mov.BaseDamage);
		powerBox.OnValueChanged += _ => mov.BaseDamage = powerBox.Value;

		Label accuracyLabel = new Label(parent);
		accuracyLabel.SetPosition(361, 274);
		accuracyLabel.SetSize(54, 18);
		accuracyLabel.SetText("Accuracy");
		accuracyLabel.SetFont(Fonts.Paragraph);

		NumericBox accuracyBox = new NumericBox(parent);
		accuracyBox.SetPosition(431, 268);
		accuracyBox.SetSize(100, 30);
		accuracyBox.SetMinValue(0);
		accuracyBox.SetMaxValue(100);
		accuracyBox.SetValue(mov.Accuracy);
		accuracyBox.OnValueChanged += _ => mov.Accuracy = accuracyBox.Value;

		Label accuracyPercentageLabel = new Label(parent);
		accuracyPercentageLabel.SetPosition(537, 274);
		accuracyPercentageLabel.SetSize(12, 18);
		accuracyPercentageLabel.SetText("%");
		accuracyPercentageLabel.SetFont(Fonts.Paragraph);

		Label ppLabel = new Label(parent);
		ppLabel.SetPosition(365, 314);
		ppLabel.SetSize(50, 18);
		ppLabel.SetText("Total PP");
		ppLabel.SetFont(Fonts.Paragraph);

		NumericBox ppBox = new NumericBox(parent);
		ppBox.SetPosition(431, 308);
		ppBox.SetSize(100, 30);
		ppBox.SetMinValue(0);
		ppBox.SetValue(mov.TotalPP);
		ppBox.OnValueChanged += _ => mov.TotalPP = ppBox.Value;

		Label priorityLabel = new Label(parent);
		priorityLabel.SetPosition(370, 354);
		priorityLabel.SetSize(45, 18);
		priorityLabel.SetText("Priority");
		priorityLabel.SetFont(Fonts.Paragraph);

		NumericBox priorityBox = new NumericBox(parent);
		priorityBox.SetPosition(431, 348);
		priorityBox.SetSize(100, 30);
		priorityBox.SetMinValue(0);
		priorityBox.SetValue(mov.Priority);
		priorityBox.OnValueChanged += _ => mov.Priority = priorityBox.Value;

		Label targetLabel = new Label(parent);
		targetLabel.SetPosition(376, 394);
		targetLabel.SetSize(39, 18);
		targetLabel.SetText("Target");
		targetLabel.SetFont(Fonts.Paragraph);

		DropdownBox targetBox = new DropdownBox(parent);
		targetBox.SetPosition(431, 391);
		targetBox.SetSize(150, 24);
		targetBox.SetItems(Data.HardcodedData.MoveTargets.Select(t => new ListItem(t)).ToList());
		if (Data.HardcodedData.MoveTargets.Contains(mov.Target)) targetBox.SetSelectedIndex(Data.HardcodedData.MoveTargets.IndexOf(mov.Target));
		else targetBox.SetText(mov.Target);
		targetBox.OnSelectionChanged += _ => mov.Target = targetBox.SelectedItem.Name;

		parent.UpdateSize();
	}

	void CreateDescContainer(DataContainer parent, Move mov)
	{
		Label descriptionLabel = new Label(parent);
		descriptionLabel.SetPosition(447, 57);
		descriptionLabel.SetSize(69, 18);
		descriptionLabel.SetText("Description");
		descriptionLabel.SetFont(Fonts.Paragraph);

		MultilineTextBox descriptionBox = new MultilineTextBox(parent);
		descriptionBox.SetPosition(218, 83);
		descriptionBox.SetSize(526, 112);
		descriptionBox.SetFont(Fonts.Paragraph);
		descriptionBox.SetText(mov.Description);
		descriptionBox.OnTextChanged += _ => mov.Description = descriptionBox.Text;

		parent.UpdateSize();
	}

	void CreateEffectContainer(DataContainer parent, Move mov)
	{
		Label functionCodeLabel = new Label(parent);
		functionCodeLabel.SetPosition(328, 64);
		functionCodeLabel.SetSize(87, 18);
		functionCodeLabel.SetText("Function Code");
		functionCodeLabel.SetFont(Fonts.Paragraph);

		DropdownBox functionCodeBox = new DropdownBox(parent);
		functionCodeBox.SetPosition(431, 62);
		functionCodeBox.SetSize(300, 24);
		functionCodeBox.SetFont(Fonts.Paragraph);
		functionCodeBox.SetItems(Data.Sources.FunctionCodes);
		functionCodeBox.SetAllowInvalidSelection(true);
		int idx = Data.Sources.FunctionCodes.FindIndex(fc => (string) fc.Object == mov.FunctionCode);
		if (idx != -1) functionCodeBox.SetSelectedIndex(idx);
		else functionCodeBox.SetText(mov.FunctionCode);
		functionCodeBox.OnSelectionChanged += _ => mov.FunctionCode = functionCodeBox.SelectedItem.Name;
		functionCodeBox.OnTextChanged += _ => mov.FunctionCode = functionCodeBox.Text;

		Button updateFunctionCodes = new Button(parent);
		updateFunctionCodes.SetPosition(740, 58);
		updateFunctionCodes.SetSize(100, 31);
		updateFunctionCodes.SetText("Refresh");
		updateFunctionCodes.OnClicked += _ =>
		{
			Data.Sources.InvalidateFunctionCodes();
			functionCodeBox.SetItems(Data.Sources.FunctionCodes);
        };

		Label effectLabel = new Label(parent);
		effectLabel.SetPosition(331, 104);
		effectLabel.SetSize(115, 18);
		effectLabel.SetText("Effect Chance");
		effectLabel.SetFont(Fonts.Paragraph);

		NumericBox sideEffectBox = new NumericBox(parent);
		sideEffectBox.SetPosition(431, 98);
		sideEffectBox.SetSize(100, 30);
		sideEffectBox.SetMinValue(0);
		sideEffectBox.SetValue(mov.EffectChance);
		sideEffectBox.OnValueChanged += _ => mov.EffectChance = sideEffectBox.Value;

		Label sideEffectPercentageLabel = new Label(parent);
		sideEffectPercentageLabel.SetPosition(537, 104);
		sideEffectPercentageLabel.SetSize(12, 18);
		sideEffectPercentageLabel.SetText("%");
		sideEffectPercentageLabel.SetFont(Fonts.Paragraph);

		CheckBox makesContactBox = new CheckBox(parent);
		makesContactBox.SetPosition(329, 144);
		makesContactBox.SetText("Makes Contact    ");
		makesContactBox.SetFont(Fonts.Paragraph);
		makesContactBox.SetMirrored(true);
		makesContactBox.SetChecked(mov.Flags.Contains("Contact"));
		makesContactBox.OnCheckChanged += _ =>
		{
			if (makesContactBox.Checked) mov.Flags.Add("Contact");
			else mov.Flags.RemoveAll(e => e == "Contact");
		};

		CheckBox canProtectBox = new CheckBox(parent);
		canProtectBox.SetPosition(267, 174);
		canProtectBox.SetText("Can be Protected against    ");
		canProtectBox.SetFont(Fonts.Paragraph);
		canProtectBox.SetMirrored(true);
		canProtectBox.SetChecked(mov.Flags.Contains("CanProtect"));
		canProtectBox.OnCheckChanged += _ =>
		{
			if (canProtectBox.Checked) mov.Flags.Add("CanProtect");
			else mov.Flags.RemoveAll(e => e == "CanProtect");
		};

		CheckBox mirrorMoveBox = new CheckBox(parent);
		mirrorMoveBox.SetPosition(288, 204);
		mirrorMoveBox.SetText("Can be Mirror Move'd    ");
		mirrorMoveBox.SetFont(Fonts.Paragraph);
		mirrorMoveBox.SetMirrored(true);
		mirrorMoveBox.SetChecked(mov.Flags.Contains("CanMirrorMove"));
		mirrorMoveBox.OnCheckChanged += _ =>
		{
			if (mirrorMoveBox.Checked) mov.Flags.Add("CanMirrorMove");
			else mov.Flags.RemoveAll(e => e == "CanMirrorMove");
		};

		CheckBox thawsUserBox = new CheckBox(parent);
		thawsUserBox.SetPosition(324, 234);
		thawsUserBox.SetText("Thaws the User    ");
		thawsUserBox.SetFont(Fonts.Paragraph);
		thawsUserBox.SetMirrored(true);
		thawsUserBox.SetChecked(mov.Flags.Contains("ThawsUser"));
		thawsUserBox.OnCheckChanged += _ =>
		{
			if (thawsUserBox.Checked) mov.Flags.Add("ThawsUser");
			else mov.Flags.RemoveAll(e => e == "ThawsUser");
		};

		CheckBox highCritBox = new CheckBox(parent);
		highCritBox.SetPosition(272, 264);
		highCritBox.SetText("Has high Critical-hit rate    ");
		highCritBox.SetFont(Fonts.Paragraph);
		highCritBox.SetMirrored(true);
		highCritBox.SetChecked(mov.Flags.Contains("HighCriticalHitRate"));
		highCritBox.OnCheckChanged += _ =>
		{
			if (highCritBox.Checked) mov.Flags.Add("HighCriticalHitRate");
			else mov.Flags.RemoveAll(e => e == "HighCriticalHitRate");
		};

		CheckBox bitingBox = new CheckBox(parent);
		bitingBox.SetPosition(322, 294);
		bitingBox.SetText("Is a Biting move    ");
		bitingBox.SetFont(Fonts.Paragraph);
		bitingBox.SetMirrored(true);
		bitingBox.SetChecked(mov.Flags.Contains("Biting"));
		bitingBox.OnCheckChanged += _ =>
		{
			if (bitingBox.Checked) mov.Flags.Add("Biting");
			else mov.Flags.RemoveAll(e => e == "Biting");
		};

		CheckBox punchingBox = new CheckBox(parent);
		punchingBox.SetPosition(302, 324);
		punchingBox.SetText("Is a Punching move    ");
		punchingBox.SetFont(Fonts.Paragraph);
		punchingBox.SetMirrored(true);
		punchingBox.SetChecked(mov.Flags.Contains("Punching"));
		punchingBox.OnCheckChanged += _ =>
		{
			if (punchingBox.Checked) mov.Flags.Add("Punching");
			else mov.Flags.RemoveAll(e => e == "Punching");
		};

		CheckBox soundBox = new CheckBox(parent);
		soundBox.SetPosition(276, 354);
		soundBox.SetText("Is a Sound-based move    ");
		soundBox.SetFont(Fonts.Paragraph);
		soundBox.SetMirrored(true);
		soundBox.SetChecked(mov.Flags.Contains("Sound"));
		soundBox.OnCheckChanged += _ =>
		{
			if (soundBox.Checked) mov.Flags.Add("Sound");
			else mov.Flags.RemoveAll(e => e == "Sound");
		};

		CheckBox powderBox = new CheckBox(parent);
		powderBox.SetPosition(269, 384);
		powderBox.SetText("Is a Powder-based move    ");
		powderBox.SetFont(Fonts.Paragraph);
		powderBox.SetMirrored(true);
		powderBox.SetChecked(mov.Flags.Contains("Powder"));
		powderBox.OnCheckChanged += _ =>
		{
			if (powderBox.Checked) mov.Flags.Add("Powder");
			else mov.Flags.RemoveAll(e => e == "Powder");
		};

		CheckBox pulseBox = new CheckBox(parent);
		pulseBox.SetPosition(284, 414);
		pulseBox.SetText("Is a Pulse-based move    ");
		pulseBox.SetFont(Fonts.Paragraph);
		pulseBox.SetMirrored(true);
		pulseBox.SetChecked(mov.Flags.Contains("Pulse"));
		pulseBox.OnCheckChanged += _ =>
		{
			if (pulseBox.Checked) mov.Flags.Add("Pulse");
			else mov.Flags.RemoveAll(e => e == "Pulse");
		};

		CheckBox bombBox = new CheckBox(parent);
		bombBox.SetPosition(278, 444);
		bombBox.SetText("Is a Bomb-based move    ");
		bombBox.SetFont(Fonts.Paragraph);
		bombBox.SetMirrored(true);
		bombBox.SetChecked(mov.Flags.Contains("Bomb"));
		bombBox.OnCheckChanged += _ =>
		{
			if (bombBox.Checked) mov.Flags.Add("Bomb");
			else mov.Flags.RemoveAll(e => e == "Bomb");
		};

		CheckBox danceBox = new CheckBox(parent);
		danceBox.SetPosition(278, 474);
		danceBox.SetText("Is a Dance-based Move    ");
		danceBox.SetFont(Fonts.Paragraph);
		danceBox.SetMirrored(true);
		danceBox.SetChecked(mov.Flags.Contains("Dance"));
		danceBox.OnCheckChanged += _ =>
		{
			if (danceBox.Checked) mov.Flags.Add("Dance");
			else mov.Flags.RemoveAll(e => e == "Dance");
		};

		CheckBox minimizeBox = new CheckBox(parent);
		minimizeBox.SetPosition(304, 504);
		minimizeBox.SetText("Tramples Minimize    ");
		minimizeBox.SetFont(Fonts.Paragraph);
		minimizeBox.SetMirrored(true);
		minimizeBox.SetChecked(mov.Flags.Contains("TramplesMinimize"));
		minimizeBox.OnCheckChanged += _ =>
		{
			if (minimizeBox.Checked) mov.Flags.Add("TramplesMinimize");
			else mov.Flags.RemoveAll(e => e == "TramplesMinimize");
		};

		StringListWidget flagsBox = new StringListWidget(parent);
		flagsBox.SetText("Flags");
		flagsBox.SetPosition(401, 536);
		flagsBox.SetSize(160, 240);
		flagsBox.SetItems(mov.Flags.FindAll(f => !new string[]
		{
			"Contact",
			"CanProtect",
			"CanMirrorMove",
			"ThawsUser",
			"HighCriticalHitRate",
			"Biting",
			"Punching",
			"Sound",
			"Powder",
			"Pulse",
			"Bomb",
			"Dance",
			"TramplesMinimize"
		}.Contains(f)).ToList());

		parent.UpdateSize();
	}
}
