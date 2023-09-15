using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeSpecies
{
	void CreateMainContainer(DataContainer parent, Species spc)
    {
		TextBox nameBox = new TextBox(parent);
		nameBox.SetPosition(217, 55);
		nameBox.SetSize(150, 27);
		nameBox.SetFont(Fonts.Paragraph);
		nameBox.SetPopupStyle(false);
		nameBox.SetText(spc.Name);
		nameBox.OnTextChanged += _ =>
		{
			spc.Name = nameBox.Text;
			SpeciesList.SelectedItem.SetText(spc.Name);
			SpeciesList.SelectedItem.Children.ForEach(c =>
			{
				TreeNode n = (TreeNode) c;
				Species s = (Species) n.Object;
				s.Name = spc.Name;
				n.SetText($"{s.Form} - {s.FormName ?? s.Name}");
			});
			Data.Sources.InvalidateSpecies();
			if (!nameBox.TimerExists("idle")) nameBox.SetTimer("idle", 1000);
			else nameBox.ResetTimer("idle");
			SpeciesList.SelectedItem.SetText(spc.Name);
			SpeciesList.RedrawNode(SpeciesList.SelectedItem);
		};
		nameBox.OnUpdate += _ =>
		{
			if (nameBox.TimerExists("idle") && nameBox.TimerPassed("idle"))
			{
				RedrawList((Species) SpeciesList.SelectedItem.Object);
				nameBox.DestroyTimer("idle");
			}
		};
		nameBox.SetEnabled(spc.Form == 0);
		nameBox.SetShowDisabledText(true);

		Label nameLabel = new Label(parent);
		nameLabel.SetPosition(166, 59);
		nameLabel.SetSize(36, 18);
		nameLabel.SetText("Name");
		nameLabel.SetFont(Fonts.Paragraph);
		nameLabel.SetEnabled(spc.Form == 0);

		TextBox idBox = new TextBox(parent);
		idBox.SetPosition(217, 93);
		idBox.SetSize(150, 27);
		idBox.SetFont(Fonts.Paragraph);
		idBox.SetPopupStyle(false);
		idBox.SetText(spc.ID);
		idBox.OnTextChanged += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (match.Success)
			{
				if (Data.Species.ContainsKey(idBox.Text)) return;
				Data.Species.Remove(spc.ID);
				spc.ID = idBox.Text;
				Data.Species.Add(spc.ID, spc);
				SpeciesList.SelectedItem.Children.ForEach(c =>
				{
					Species s = (Species) ((TreeNode) c).Object;
					Data.Species.Remove(s.ID);
					s.ID = spc.ID + "_" + s.Form.ToString();
					s.BaseSpecies = (SpeciesResolver) spc.ID;
					Data.Species.Add(s.ID, s);
				});
			}
		};
		idBox.TextArea.OnWidgetDeselected += _ =>
		{
			Match match = Regex.Match(idBox.Text, @"[A-Z][a-zA-Z_\d]*$");
			if (!match.Success)
			{
				string newID = Utilities.Internalize(nameBox.Text);
				if (newID != spc.ID)
				{
					Data.Species.Remove(spc.ID);
					spc.ID = newID;
					Data.Species.Add(spc.ID, spc);
				}
			}
			idBox.SetText(spc.ID);
		};
		idBox.SetEnabled(spc.Form == 0);
		idBox.SetShowDisabledText(true);

		Label idLabel = new Label(parent);
		idLabel.SetPosition(187, 97);
		idLabel.SetSize(86, 18);
		idLabel.SetText("ID");
		idLabel.SetFont(Fonts.Paragraph);
		idLabel.SetEnabled(spc.Form == 0);

		Label type1Label = new Label(parent);
		type1Label.SetPosition(164, 139);
		type1Label.SetSize(38, 18);
		type1Label.SetText("Type 1");
		type1Label.SetFont(Fonts.Paragraph);

		TypeDropdownBox type1Box = new TypeDropdownBox(parent);
		type1Box.SetPosition(217, 136);
		type1Box.SetSize(150, 24);
		type1Box.SetType(spc.Type1);
		type1Box.OnTypeChanged += _ => spc.Type1 = type1Box.Type;

		bool hasType2 = spc.Type2 is not null;

		TypeDropdownBox type2Box = new TypeDropdownBox(parent);
		type2Box.SetPosition(217, 172);
		type2Box.SetSize(150, 24);
		if (hasType2) type2Box.SetType(spc.Type2);
		type2Box.SetEnabled(hasType2);
		type2Box.OnTypeChanged += _ => spc.Type2 = type2Box.Type;

		CheckBox type2CheckBox = new CheckBox(parent);
		type2CheckBox.SetPosition(142, 178);
		type2CheckBox.SetText("Type 2");
		type2CheckBox.SetFont(Fonts.Paragraph);
		type2CheckBox.SetChecked(hasType2);
		type2CheckBox.OnCheckChanged += _ =>
		{
			if (type2CheckBox.Checked)
			{
				type2Box.SetEnabled(true);
				spc.Type2 = type2Box.Type;
			}
			else
			{
				type2Box.SetEnabled(false);
				spc.Type2 = null;
			}
		};

        Label ability1Label = new Label(parent);
		ability1Label.SetPosition(566, 59);
		ability1Label.SetSize(46, 18);
		ability1Label.SetText("Ability 1");
		ability1Label.SetFont(Fonts.Paragraph);

		AbilityDropdownBox ability1Box = new AbilityDropdownBox(parent);
		ability1Box.SetPosition(629, 56);
		ability1Box.SetSize(150, 24);
		ability1Box.SetAbility(spc.Abilities[0]);
		ability1Box.OnAbilityChanged += _ => spc.Abilities[0] = ability1Box.Ability;

		bool hasAbil2 = spc.Abilities.Count > 1;

		AbilityDropdownBox ability2Box = new AbilityDropdownBox(parent);
		ability2Box.SetPosition(629, 96);
		ability2Box.SetSize(150, 24);
		if (hasAbil2) ability2Box.SetAbility(spc.Abilities[1]);
		ability2Box.SetEnabled(hasAbil2);
		ability2Box.OnAbilityChanged += _ => spc.Abilities[1] = ability2Box.Ability;

		CheckBox ability2CheckBox = new CheckBox(parent);
		ability2CheckBox.SetPosition(544, 98);
		ability2CheckBox.SetText("Ability 2");
		ability2CheckBox.SetFont(Fonts.Paragraph);
        ability2CheckBox.SetChecked(hasAbil2);
        ability2CheckBox.OnCheckChanged += _ =>
        {
            if (ability2CheckBox.Checked)
            {
                ability2Box.SetEnabled(true);
                Species.Abilities = new List<AbilityResolver>() { ability1Box.Ability, ability2Box.Ability };
            }
            else
            {
                ability2Box.SetEnabled(false);
                Species.Abilities = new List<AbilityResolver>() { ability1Box.Ability };
            }
        };

        bool hasHA = spc.HiddenAbilities.Count > 0;

		AbilityDropdownBox hiddenAbilityBox = new AbilityDropdownBox(parent);
		hiddenAbilityBox.SetPosition(629, 136);
		hiddenAbilityBox.SetSize(150, 24);
		if (hasHA) hiddenAbilityBox.SetAbility(spc.HiddenAbilities[0]);
        hiddenAbilityBox.SetEnabled(hasHA);
		hiddenAbilityBox.OnAbilityChanged += _ => spc.HiddenAbilities = new List<AbilityResolver>() { hiddenAbilityBox.Ability };

		CheckBox hiddenAbilityCheckBox = new CheckBox(parent);
		hiddenAbilityCheckBox.SetPosition(504, 140);
		hiddenAbilityCheckBox.SetText("Hidden Ability");
		hiddenAbilityCheckBox.SetChecked(hasHA);
		hiddenAbilityCheckBox.SetFont(Fonts.Paragraph);
		hiddenAbilityCheckBox.OnCheckChanged += _ =>
		{
			if (hiddenAbilityCheckBox.Checked)
			{
				hiddenAbilityBox.SetEnabled(true);
				Species.HiddenAbilities = new List<AbilityResolver>() { hiddenAbilityBox.Ability };
			}
			else
			{
				hiddenAbilityBox.SetEnabled(false);
				Species.HiddenAbilities.Clear();
			}
		};

        parent.UpdateSize();
	}

	void CreateFormContainer(DataContainer parent, Species spc)
	{
		if (spc.Form == 0) throw new Exception("Cannot show form container for base forms!");

		Label formNameLabel = new Label(parent);
		formNameLabel.SetPosition(180, 93);
		formNameLabel.SetSize(71, 18);
		formNameLabel.SetText("Form Name");
		formNameLabel.SetFont(Fonts.Paragraph);

		TextBox formNameBox = new TextBox(parent);
		formNameBox.SetPosition(275, 89);
		formNameBox.SetSize(150, 27);
		formNameBox.SetFont(Fonts.Paragraph);
		formNameBox.SetText(spc.FormName);
		formNameBox.OnTextChanged += _ =>
		{
			spc.FormName = formNameBox.Text;
			Data.Sources.InvalidateSpecies();
			if (!formNameBox.TimerExists("idle")) formNameBox.SetTimer("idle", 1000);
			else formNameBox.ResetTimer("idle");
			SpeciesList.SelectedItem.SetText($"{spc.Form} - {spc.FormName ?? spc.Name}");
			SpeciesList.RedrawNodeText(SpeciesList.SelectedItem);
		};
		formNameBox.OnUpdate += _ =>
		{
			if (formNameBox.TimerExists("idle") && formNameBox.TimerPassed("idle"))
			{
				RedrawList((Species) SpeciesList.SelectedItem.Object);
				formNameBox.DestroyTimer("idle");
			}
		};

		Label formNumberLabel = new Label(parent);
		formNumberLabel.SetPosition(167, 133);
		formNumberLabel.SetSize(84, 18);
		formNumberLabel.SetText("Form Number");
		formNumberLabel.SetFont(Fonts.Paragraph);

		NumericBox dexFormBox = new NumericBox(parent);

		NumericBox formNumberBox = new NumericBox(parent);
		formNumberBox.SetPosition(275, 128);
		formNumberBox.SetSize(150, 30);
		formNumberBox.SetMinValue(1);
		formNumberBox.SetValue(spc.Form);
		formNumberBox.OnValueChanged += _ =>
		{
			if (IsFormNumberFree(spc, formNumberBox.Value))
			{
				spc.Form = formNumberBox.Value;
				dexFormBox.SetValue(spc.Form);
				TreeNode item = SpeciesList.SelectedItem;
				TreeNode parentNode = item.Parent;
				item.SetText($"{spc.Form} - {spc.FormName ?? spc.Name}");
				item.Delete(false);
				int idx = parentNode.Children.FindIndex(node => spc.Form < ((Species) ((TreeNode) node).Object).Form);
                if (idx == -1) idx = parentNode.Children.Count;
                parentNode.InsertChild(idx, item);
				SpeciesList.SetActiveAndSelectedNode(item);
				SpeciesList.RedrawNode(parentNode);
			}
		};
		formNumberBox.TextArea.OnWidgetDeselected += _ =>
		{
			if (!IsFormNumberFree(spc, formNumberBox.Value))
			{
				formNumberBox.SetValue(spc.Form);
				dexFormBox.SetValue(spc.Form);
			}
		};
		formNumberBox.OnPlusClicked += _ =>
		{
			int? newForm = GetFreeFormNumber(spc, spc.Form, 1);
			if (newForm is null) return;
			formNumberBox.SetValue((int) newForm);
		};
		formNumberBox.OnMinusClicked += _ =>
		{
			int? newForm = GetFreeFormNumber(spc, spc.Form, -1);
			if (newForm is null) return;
			formNumberBox.SetValue((int) newForm);
		};

		dexFormBox.SetPosition(275, 168);
		dexFormBox.SetSize(150, 30);
		dexFormBox.SetMinValue(1);
		dexFormBox.SetValue(spc.PokedexForm);
		dexFormBox.OnValueChanged += _ => spc.PokedexForm = dexFormBox.Value;

		Label dexFormLabel = new Label(parent);
		dexFormLabel.SetPosition(192, 174);
		dexFormLabel.SetSize(59, 18);
		dexFormLabel.SetText("Dex Form");
		dexFormLabel.SetFont(Fonts.Paragraph);

		bool hasMegaStone = spc.MegaStone is not null;

		ItemDropdownBox megaStoneBox = new ItemDropdownBox(parent);
		megaStoneBox.SetPosition(625, 70);
		megaStoneBox.SetSize(150, 24);
		megaStoneBox.SetItems(Data.Sources.Items.FindAll(item => ((Item) item.Object).Flags.Contains("MegaStone")).ToList());
		if (hasMegaStone) megaStoneBox.SetItem(spc.MegaStone);
		megaStoneBox.SetEnabled(hasMegaStone);
		megaStoneBox.OnItemChanged += _ => spc.MegaStone = megaStoneBox.Item;

		CheckBox megaStoneCheckBox = new CheckBox(parent);
		megaStoneCheckBox.SetPosition(510, 74);
		megaStoneCheckBox.SetText("Mega Stone");
		megaStoneCheckBox.SetFont(Fonts.Paragraph);
		megaStoneCheckBox.SetChecked(hasMegaStone);
		megaStoneCheckBox.OnCheckChanged += _ =>
		{
			if (megaStoneCheckBox.Checked)
			{
				megaStoneBox.SetEnabled(true);
				spc.MegaStone = megaStoneBox.Item;
			}
			else
			{
				megaStoneBox.SetEnabled(false);
				spc.MegaStone = null;
			}
		};

		bool hasMegaMove = spc.MegaMove is not null;

		MoveDropdownBox megaMoveBox = new MoveDropdownBox(parent);
		megaMoveBox.SetPosition(625, 110);
		megaMoveBox.SetSize(150, 24);
		if (hasMegaMove) megaMoveBox.SetMove(spc.MegaMove);
		megaMoveBox.SetEnabled(hasMegaMove);
		megaMoveBox.OnMoveChanged += _ => spc.MegaMove = megaMoveBox.Move;

		CheckBox megaMoveCheckBox = new CheckBox(parent);
		megaMoveCheckBox.SetPosition(510, 114);
		megaMoveCheckBox.SetText("Mega Move");
		megaMoveCheckBox.SetFont(Fonts.Paragraph);
		megaMoveCheckBox.SetChecked(hasMegaMove);
		megaMoveCheckBox.OnCheckChanged += _ =>
		{
			if (megaMoveCheckBox.Checked)
			{
				megaMoveBox.SetEnabled(true);
				spc.MegaMove = megaMoveBox.Move;
			}
			else
			{
				megaMoveBox.SetEnabled(false);
				spc.MegaMove = null;
			}
		};

		DropdownBox megaMessageBox = new DropdownBox(parent);
		megaMessageBox.SetPosition(625, 150);
		megaMessageBox.SetSize(150, 24);
		megaMessageBox.SetItems(new List<TreeNode>()
		{
			new TreeNode("Default"),
			new TreeNode("Fervent Wish")
		});
		megaMessageBox.SetSelectedIndex(spc.MegaMessage);
		megaMessageBox.OnSelectionChanged += _ => spc.MegaMessage = megaMessageBox.SelectedIndex;

		Label megaMessageLabel = new Label(parent);
		megaMessageLabel.SetPosition(515, 154);
		megaMessageLabel.SetSize(86, 18);
		megaMessageLabel.SetText("Mega Message");
		megaMessageLabel.SetFont(Fonts.Paragraph);

		NumericBox unmegaFormBox = new NumericBox(parent);
		unmegaFormBox.SetPosition(625, 187);
		unmegaFormBox.SetSize(150, 30);
		unmegaFormBox.SetMinValue(0);
		unmegaFormBox.SetValue(spc.UnmegaForm);
		unmegaFormBox.OnValueChanged += _ => spc.UnmegaForm = unmegaFormBox.Value;

		Label unmegaFormLabel = new Label(parent);
		unmegaFormLabel.SetPosition(515, 194);
		unmegaFormLabel.SetSize(86, 18);
		unmegaFormLabel.SetText("Unmega Form");
		unmegaFormLabel.SetFont(Fonts.Paragraph);

		parent.UpdateSize();
	}

	bool IsFormNumberFree(Species spc, int form)
	{
		List<Species> sameIDSpecies = Data.Species.Values.Where(s => s.ID == spc.BaseSpecies.ID || s.BaseSpecies.ID == spc.BaseSpecies.ID).ToList();
		return !sameIDSpecies.Any(s => s.Form == form);
	}

	int? GetFreeFormNumber(Species spc, int startForm, int mod)
	{
		int form = startForm + mod;
		while (!IsFormNumberFree(spc, form))
		{
			form += mod;
			if (form < 1) return null;
		}
		return form;
	}

	void CreateStatsContainer(DataContainer parent, Species spc)
	{
		Label totalLabel = new Label(parent);
		totalLabel.SetPosition(259, 306);
		totalLabel.SetSize(32, 18);
		totalLabel.SetText("Total");
		totalLabel.SetFont(Fonts.Paragraph);

		Label bstLabel = new Label(parent);
		bstLabel.SetPosition(350, 306);
		bstLabel.SetSize(25, 18);
		bstLabel.SetText("420");
		bstLabel.SetFont(Fonts.Paragraph);

		void updateBST()
		{
			bstLabel.SetText(spc.BaseStats.Total.ToString());
			bstLabel.RedrawText(true);
			bstLabel.SetPosition(360 - bstLabel.Size.Width / 2, 306);
		}

		updateBST();

		Label hpLabel = new Label(parent);
		hpLabel.SetPosition(273, 63);
		hpLabel.SetSize(18, 18);
		hpLabel.SetText("HP");
		hpLabel.SetFont(Fonts.Paragraph);

		NumericBox hpBox = new NumericBox(parent);
		NumericSlider hpSlider = new NumericSlider(parent);

		hpBox.SetPosition(314, 57);
		hpBox.SetSize(98, 30);
		hpBox.SetMinValue(0);
		hpBox.SetMaxValue(999);
		hpBox.SetValue(spc.BaseStats.HP);
		hpBox.OnValueChanged += _ =>
		{
			hpSlider.SetValue(hpBox.Value);
			spc.BaseStats.HP = hpBox.Value;
			updateBST();
		};

		hpSlider.SetPosition(428, 64);
		hpSlider.SetSize(300, 17);
		hpSlider.SetMinimumValue(0);
		hpSlider.SetMaximumValue(255);
		hpSlider.SetValue(spc.BaseStats.HP);
		hpSlider.OnValueChanged += _ =>
		{
			if (hpSlider.Value == 255 && hpBox.Value > 255) return;
			hpBox.SetValue(hpSlider.Value);
		};


		Label attackLabel = new Label(parent);
		attackLabel.SetPosition(252, 103);
		attackLabel.SetSize(39, 18);
		attackLabel.SetText("Attack");
		attackLabel.SetFont(Fonts.Paragraph);

		NumericBox attackBox = new NumericBox(parent);
		NumericSlider attackSlider = new NumericSlider(parent);

		attackBox.SetPosition(314, 97);
		attackBox.SetSize(98, 30);
		attackBox.SetMinValue(0);
		attackBox.SetMaxValue(999);
		attackBox.SetValue(spc.BaseStats.Attack);
		attackBox.OnValueChanged += _ =>
		{
			attackSlider.SetValue(attackBox.Value);
			spc.BaseStats.Attack = attackBox.Value;
			updateBST();
		};

		attackSlider.SetPosition(428, 104);
		attackSlider.SetSize(300, 17);
		attackSlider.SetMinimumValue(0);
		attackSlider.SetMaximumValue(255);
		attackSlider.SetValue(spc.BaseStats.Attack);
		attackSlider.OnValueChanged += _ =>
		{
			if (attackSlider.Value == 255 && attackBox.Value > 255) return;
			attackBox.SetValue(attackSlider.Value);
		};

		Label defenseLabel = new Label(parent);
		defenseLabel.SetPosition(241, 143);
		defenseLabel.SetSize(50, 18);
		defenseLabel.SetText("Defense");
		defenseLabel.SetFont(Fonts.Paragraph);

		NumericBox defenseBox = new NumericBox(parent);
		NumericSlider defenseSlider = new NumericSlider(parent);

		defenseBox.SetPosition(314, 137);
		defenseBox.SetSize(98, 30);
		defenseBox.SetMinValue(0);
		defenseBox.SetMaxValue(999);
		defenseBox.SetValue(spc.BaseStats.Defense);
		defenseBox.OnValueChanged += _ =>
		{
			defenseSlider.SetValue(defenseBox.Value);
			spc.BaseStats.Defense = defenseBox.Value;
			updateBST();
		};

		defenseSlider.SetPosition(428, 144);
		defenseSlider.SetSize(300, 17);
		defenseSlider.SetMinimumValue(0);
		defenseSlider.SetMaximumValue(255);
		defenseSlider.SetValue(spc.BaseStats.Defense);
		defenseSlider.OnValueChanged += _ =>
		{
			if (defenseSlider.Value == 255 && defenseBox.Value > 255) return;
			defenseBox.SetValue(defenseSlider.Value);
		};

		Label spatkLabel = new Label(parent);
		spatkLabel.SetPosition(207, 183);
		spatkLabel.SetSize(84, 18);
		spatkLabel.SetText("Special Attack");
		spatkLabel.SetFont(Fonts.Paragraph);

		NumericBox spatkBox = new NumericBox(parent);
		NumericSlider spatkSlider = new NumericSlider(parent);

		spatkBox.SetPosition(314, 177);
		spatkBox.SetSize(98, 30);
		spatkBox.SetMinValue(0);
		spatkBox.SetMaxValue(999);
		spatkBox.SetValue(spc.BaseStats.SpecialAttack);
		spatkBox.OnValueChanged += _ =>
		{
			spatkSlider.SetValue(spatkBox.Value);
			spc.BaseStats.SpecialAttack = spatkBox.Value;
			updateBST();
		};

		spatkSlider.SetPosition(428, 184);
		spatkSlider.SetSize(300, 17);
		spatkSlider.SetMinimumValue(0);
		spatkSlider.SetMaximumValue(255);
		spatkSlider.SetValue(spc.BaseStats.SpecialAttack);
		spatkSlider.OnValueChanged += _ =>
		{
			if (spatkSlider.Value == 255 && spatkBox.Value > 255) return;
			spatkBox.SetValue(spatkSlider.Value);
		};

		Label spdefLabel = new Label(parent);
		spdefLabel.SetPosition(196, 223);
		spdefLabel.SetSize(95, 18);
		spdefLabel.SetText("Special Defense");
		spdefLabel.SetFont(Fonts.Paragraph);

		NumericBox spdefBox = new NumericBox(parent);
		NumericSlider spdefSlider = new NumericSlider(parent);

		spdefBox.SetPosition(314, 217);
		spdefBox.SetSize(98, 30);
		spdefBox.SetMinValue(0);
		spdefBox.SetMaxValue(999);
		spdefBox.SetValue(spc.BaseStats.SpecialDefense);
		spdefBox.OnValueChanged += _ =>
		{
			spdefSlider.SetValue(spdefBox.Value);
			spc.BaseStats.SpecialDefense = spdefBox.Value;
			updateBST();
		};

		spdefSlider.SetPosition(428, 224);
		spdefSlider.SetSize(300, 17);
		spdefSlider.SetMinimumValue(0);
		spdefSlider.SetMaximumValue(255);
		spdefSlider.SetValue(spc.BaseStats.SpecialDefense);
		spdefSlider.OnValueChanged += _ =>
		{
			if (spdefSlider.Value == 255 && spdefBox.Value > 255) return;
			spdefBox.SetValue(spdefSlider.Value);
		};

		Label speedLabel = new Label(parent);
		speedLabel.SetPosition(253, 263);
		speedLabel.SetSize(38, 18);
		speedLabel.SetText("Speed");
		speedLabel.SetFont(Fonts.Paragraph);

		NumericBox speedBox = new NumericBox(parent);
		NumericSlider speedSlider = new NumericSlider(parent);

		speedBox.SetPosition(314, 257);
		speedBox.SetSize(98, 30);
		speedBox.SetMinValue(0);
		speedBox.SetMaxValue(999);
		speedBox.SetValue(spc.BaseStats.Speed);
		speedBox.OnValueChanged += _ =>
		{
			speedSlider.SetValue(speedBox.Value);
			spc.BaseStats.Speed = speedBox.Value;
			updateBST();
		};

		speedSlider.SetPosition(428, 264);
		speedSlider.SetSize(300, 17);
		speedSlider.SetMinimumValue(0);
		speedSlider.SetMaximumValue(255);
		speedSlider.SetValue(spc.BaseStats.Speed);
		speedSlider.OnValueChanged += _ =>
		{
			if (speedSlider.Value == 255 && speedBox.Value > 255) return;
			speedBox.SetValue(speedSlider.Value);
		};

		parent.UpdateSize();
	}

	void CreateEVContainer(DataContainer parent, Species spc)
	{
		NumericBox hpBox = new NumericBox(parent);
		//hpBox.SetPosition(52, 57);
		//hpBox.SetSize(74, 30);
		hpBox.SetPosition(452, 57);
		hpBox.SetSize(98, 30);
		hpBox.SetMinValue(0);
		hpBox.SetValue(spc.EVs.HP);
		hpBox.OnValueChanged += _ => spc.EVs.HP = hpBox.Value;

		Label hpLabel = new Label(parent);
		//hpLabel.SetPosition(28, 63);
		//hpLabel.SetSize(18, 18);
		hpLabel.SetPosition(415, 63);
		hpLabel.SetSize(18, 18);
		hpLabel.SetText("HP");
		hpLabel.SetFont(Fonts.Paragraph);

		NumericBox attackBox = new NumericBox(parent);
		//attackBox.SetPosition(217, 57);
		//attackBox.SetSize(75, 30);
		attackBox.SetPosition(452, 97);
		attackBox.SetSize(98, 30);
		attackBox.SetMinValue(0);
		attackBox.SetValue(spc.EVs.Attack);
		attackBox.OnValueChanged += _ => spc.EVs.Attack = attackBox.Value;

		Label attackLabel = new Label(parent);
		attackLabel.SetPosition(394, 103);
		attackLabel.SetSize(39, 18);
		//attackLabel.SetPosition(170, 63);
		//attackLabel.SetSize(39, 18);
		attackLabel.SetText("Attack");
		attackLabel.SetFont(Fonts.Paragraph);

		NumericBox defenseBox = new NumericBox(parent);
		//defenseBox.SetPosition(381, 57);
		//defenseBox.SetSize(75, 30);
		defenseBox.SetPosition(452, 137);
		defenseBox.SetSize(98, 30);
		defenseBox.SetMinValue(0);
		defenseBox.SetValue(spc.EVs.Defense);
		defenseBox.OnValueChanged += _ => spc.EVs.Defense = defenseBox.Value;

		Label defenseLabel = new Label(parent);
		//defenseLabel.SetPosition(326, 63);
		//defenseLabel.SetSize(50, 18);
		defenseLabel.SetPosition(383, 143);
		defenseLabel.SetSize(50, 18);
		defenseLabel.SetText("Defense");
		defenseLabel.SetFont(Fonts.Paragraph);

		NumericBox spdefBox = new NumericBox(parent);
		//spdefBox.SetPosition(722, 57);
		//spdefBox.SetSize(75, 30);
		spdefBox.SetPosition(452, 217);
		spdefBox.SetSize(98, 30);
		spdefBox.SetMinValue(0);
		spdefBox.SetValue(spc.EVs.SpecialDefense);
		spdefBox.OnValueChanged += _ => spc.EVs.SpecialDefense = spdefBox.Value;

		Label spdefLabel = new Label(parent);
		//spdefLabel.SetPosition(667, 63);
		//spdefLabel.SetSize(47, 18);
		spdefLabel.SetPosition(338, 223);
		spdefLabel.SetSize(95, 18);
		//spdefLabel.SetText("Sp. Def.");
		spdefLabel.SetText("Special Defense");
		spdefLabel.SetFont(Fonts.Paragraph);

		NumericBox spatkBox = new NumericBox(parent);
		//spatkBox.SetPosition(550, 57);
		//spatkBox.SetSize(75, 30);
		spatkBox.SetPosition(452, 177);
		spatkBox.SetSize(98, 30);
		spatkBox.SetMinValue(0);
		spatkBox.SetValue(spc.EVs.SpecialAttack);
		spatkBox.OnValueChanged += _ => spc.EVs.SpecialAttack = spatkBox.Value;

		Label spatkLabel = new Label(parent);
		//spatkLabel.SetPosition(499, 63);
		//spatkLabel.SetSize(45, 18);
		spatkLabel.SetPosition(349, 183);
		spatkLabel.SetSize(84, 18);
		//spatkBox.SetHelpText("Sp. Atk.");
		spatkLabel.SetText("Special Attack");
		spatkLabel.SetFont(Fonts.Paragraph);

		NumericBox speedBox = new NumericBox(parent);
		//speedBox.SetPosition(883, 57);
		//speedBox.SetSize(75, 30);
		speedBox.SetPosition(452, 257);
		speedBox.SetSize(98, 30);
		speedBox.SetMinValue(0);
		speedBox.SetValue(spc.EVs.Speed);
		speedBox.OnValueChanged += _ => spc.EVs.Speed = speedBox.Value;

		Label speedLabel = new Label(parent);
		//speedLabel.SetPosition(837, 63);
		//speedLabel.SetSize(38, 18);
		speedLabel.SetPosition(395, 263);
		speedLabel.SetSize(38, 18);
		speedLabel.SetText("Speed");
		speedLabel.SetFont(Fonts.Paragraph);

		parent.UpdateSize();
	}

	void CreateMiscContainer(DataContainer parent, Species spc)
	{
		Label genderRatioLabel = new Label(parent);
		genderRatioLabel.SetPosition(174, 70);
		genderRatioLabel.SetSize(79, 18);
		genderRatioLabel.SetText("Gender Ratio");
		genderRatioLabel.SetFont(Fonts.Paragraph);
		genderRatioLabel.SetEnabled(spc.Form == 0);

		DropdownBox genderRatioBox = new DropdownBox(parent);
		genderRatioBox.SetPosition(274, 67);
		genderRatioBox.SetSize(160, 24);
		genderRatioBox.SetItems(Data.HardcodedData.GenderRatiosListItems);
		if (Data.HardcodedData.GenderRatios.Contains(spc.GenderRatio)) genderRatioBox.SetSelectedIndex(Data.HardcodedData.GenderRatiosListItems.FindIndex(item => item.Text == spc.GenderRatio));
		else genderRatioBox.SetText(spc.GenderRatio);
		genderRatioBox.OnSelectionChanged += _ =>
		{
			spc.GenderRatio = genderRatioBox.SelectedItem.Text;
			SpeciesList.SelectedItem.Children.ForEach(c =>
			{
				Species s = (Species) ((TreeNode) c).Object;
				s.GenderRatio = spc.GenderRatio;
			});
		};
		genderRatioBox.SetEnabled(spc.Form == 0);
		genderRatioBox.SetShowDisabledText(true);

		Label eggGroup1Label = new Label(parent);
		eggGroup1Label.SetPosition(183, 110);
		eggGroup1Label.SetSize(70, 18);
		eggGroup1Label.SetText("Egg Group 1");
		eggGroup1Label.SetFont(Fonts.Paragraph);

		DropdownBox eggGroup1Box = new DropdownBox(parent);
		eggGroup1Box.SetPosition(274, 107);
		eggGroup1Box.SetSize(160, 24);
		eggGroup1Box.SetItems(Data.HardcodedData.EggGroupsListItems);
		if (Data.HardcodedData.EggGroups.Contains(spc.EggGroups[0])) eggGroup1Box.SetSelectedIndex(Data.HardcodedData.EggGroupsListItems.FindIndex(item => item.Text == spc.EggGroups[0]));
		else eggGroup1Box.SetText(spc.EggGroups[0]);
		eggGroup1Box.OnSelectionChanged += _ => spc.EggGroups[0] = eggGroup1Box.SelectedItem.Text;

		bool hasEggGroup2 = spc.EggGroups.Count > 1 && spc.EggGroups[1] is not null;

		DropdownBox eggGroup2Box = new DropdownBox(parent);
		eggGroup2Box.SetPosition(274, 147);
		eggGroup2Box.SetSize(160, 24);
		eggGroup2Box.SetEnabled(hasEggGroup2);
		eggGroup2Box.SetItems(Data.HardcodedData.EggGroupsListItems);
		if (hasEggGroup2 && Data.HardcodedData.EggGroups.Contains(spc.EggGroups[1])) eggGroup2Box.SetSelectedIndex(Data.HardcodedData.EggGroupsListItems.FindIndex(item => item.Text == spc.EggGroups[1]));
		else if (hasEggGroup2) eggGroup2Box.SetText(spc.EggGroups[1]);
		eggGroup2Box.OnSelectionChanged += _ => spc.EggGroups = new List<string>() { eggGroup1Box.SelectedItem.Text, eggGroup2Box.SelectedItem.Text };

		CheckBox eggGroup2CheckBox = new CheckBox(parent);
		eggGroup2CheckBox.SetPosition(162, 151);
		eggGroup2CheckBox.SetText("Egg Group 2");
		eggGroup2CheckBox.SetFont(Fonts.Paragraph);
		eggGroup2CheckBox.SetChecked(hasEggGroup2);
		eggGroup2CheckBox.OnCheckChanged += _ =>
		{
			if (eggGroup2CheckBox.Checked)
			{
				eggGroup2Box.SetEnabled(true);
				spc.EggGroups = new List<string>() { eggGroup1Box.SelectedItem.Text, eggGroup2Box.SelectedItem.Text };
			}
			else
			{
				eggGroup2Box.SetEnabled(false);
				spc.EggGroups = new List<string>() { eggGroup1Box.SelectedItem.Text };
			}
		};

		Label growthRateLabel = new Label(parent);
		growthRateLabel.SetPosition(564, 70);
		growthRateLabel.SetSize(76, 18);
		growthRateLabel.SetText("Growth Rate");
		growthRateLabel.SetFont(Fonts.Paragraph);
		growthRateLabel.SetEnabled(spc.Form == 0);

		DropdownBox growthRateBox = new DropdownBox(parent);
		growthRateBox.SetPosition(664, 67);
		growthRateBox.SetSize(160, 24);
		growthRateBox.SetItems(Data.HardcodedData.GrowthRatesListItems);
		if (Data.HardcodedData.GrowthRates.Contains(spc.GrowthRate)) growthRateBox.SetSelectedIndex(Data.HardcodedData.GrowthRatesListItems.FindIndex(item => item.Text == spc.GrowthRate));
		else growthRateBox.SetText(spc.GrowthRate);
		growthRateBox.OnSelectionChanged += _ =>
		{
			spc.GrowthRate = growthRateBox.SelectedItem.Text;
			SpeciesList.SelectedItem.Children.ForEach(c =>
			{
				Species s = (Species) ((TreeNode) c).Object;
				s.GrowthRate = spc.GrowthRate;
			});
		};
		growthRateBox.SetEnabled(spc.Form == 0);
		growthRateBox.SetShowDisabledText(true);

		Label catchRateLabel = new Label(parent);
		catchRateLabel.SetPosition(574, 110);
		catchRateLabel.SetSize(66, 18);
		catchRateLabel.SetText("Catch Rate");
		catchRateLabel.SetFont(Fonts.Paragraph);

		NumericBox catchRateBox = new NumericBox(parent);
		catchRateBox.SetPosition(664, 104);
		catchRateBox.SetSize(160, 30);
		catchRateBox.SetValue(spc.CatchRate);
		catchRateBox.OnValueChanged += _ => spc.CatchRate = catchRateBox.Value;

		Label expYieldLabel = new Label(parent);
		expYieldLabel.SetPosition(583, 150);
		expYieldLabel.SetSize(57, 18);
		expYieldLabel.SetText("EXP Yield");
		expYieldLabel.SetFont(Fonts.Paragraph);

		NumericBox expYieldBox = new NumericBox(parent);
		expYieldBox.SetPosition(664, 144);
		expYieldBox.SetSize(160, 30);
		expYieldBox.SetValue(spc.BaseEXP);
		expYieldBox.OnValueChanged += _ => spc.BaseEXP = expYieldBox.Value;

		Label eggStepsLabel = new Label(parent);
		eggStepsLabel.SetPosition(194, 190);
		eggStepsLabel.SetSize(59, 18);
		eggStepsLabel.SetText("Egg Steps");
		eggStepsLabel.SetFont(Fonts.Paragraph);

		NumericBox eggStepsBox = new NumericBox(parent);
		eggStepsBox.SetPosition(274, 184);
		eggStepsBox.SetSize(160, 30);
		eggStepsBox.SetValue(spc.HatchSteps);
		eggStepsBox.OnValueChanged += _ => spc.HatchSteps = eggStepsBox.Value;

		Label happinessLabel = new Label(parent);
		happinessLabel.SetPosition(545, 190);
		happinessLabel.SetSize(95, 18);
		happinessLabel.SetText("Base Happiness");
		happinessLabel.SetFont(Fonts.Paragraph);

		NumericBox happinessBox = new NumericBox(parent);
		happinessBox.SetPosition(664, 184);
		happinessBox.SetSize(160, 30);
		happinessBox.SetValue(spc.Happiness);
		happinessBox.OnValueChanged += _ => spc.Happiness = happinessBox.Value;

		bool hasIncense = spc.Incense is not null;

		ItemDropdownBox incenseBox = new ItemDropdownBox(parent);
		incenseBox.SetPosition(274, 227);
		incenseBox.SetSize(160, 24);
		if (hasIncense) incenseBox.SetItem(spc.Incense);
		incenseBox.SetEnabled(hasIncense && spc.Form == 0);
		incenseBox.SetShowDisabledText(spc.Form > 0 && spc.BaseSpecies.Species.Incense is not null);
		incenseBox.OnItemChanged += _ =>
		{
			spc.Incense = incenseBox.Item;
			SpeciesList.SelectedItem.Children.ForEach(c =>
			{
				Species s = (Species) ((TreeNode) c).Object;
				s.Incense = spc.Incense;
			});
		};

		CheckBox incenseCheckBox = new CheckBox(parent);
		incenseCheckBox.SetPosition(186, 230);
		incenseCheckBox.SetText("Incense");
		incenseCheckBox.SetFont(Fonts.Paragraph);
		incenseCheckBox.SetChecked(hasIncense);
		incenseCheckBox.SetEnabled(spc.Form == 0);
		incenseCheckBox.OnCheckChanged += _ =>
		{
			if (incenseCheckBox.Checked)
			{
				incenseBox.SetEnabled(true);
				spc.Incense = incenseBox.Item;
			}
			else
			{
				incenseBox.SetEnabled(false);
				spc.Incense = null;
			}
			SpeciesList.SelectedItem.Children.ForEach(c =>
			{
				Species s = (Species) ((TreeNode) c).Object;
				s.Incense = spc.Incense;
			});
		};

		SpeciesListWidget offspringBox = new SpeciesListWidget(true, parent);
		offspringBox.SetText("Offspring");
		offspringBox.SetSize(220, 216);
		offspringBox.SetPosition(176, 270);
		offspringBox.SetItems(spc.Offspring);
		offspringBox.OnListChanged += _ => spc.Offspring = offspringBox.AsResolvers;

		StringListWidget flagsBox = new StringListWidget(parent);
		flagsBox.SetText("Flags");
		flagsBox.SetSize(220, 216);
		flagsBox.SetPosition(576, 270);
		flagsBox.SetItems(spc.Flags);
		flagsBox.OnListChanged += _ => spc.Flags = flagsBox.AsStrings;

		parent.UpdateSize();
	}

	void CreateDexInfoContainer(DataContainer parent, Species spc)
	{
		Label kindLabel = new Label(parent);
		kindLabel.SetPosition(226, 70);
		kindLabel.SetSize(27, 18);
		kindLabel.SetText("Kind");
		kindLabel.SetFont(Fonts.Paragraph);

		TextBox kindBox = new TextBox(parent);
		kindBox.SetPosition(274, 67);
		kindBox.SetSize(155, 27);
		kindBox.SetFont(Fonts.Paragraph);
		kindBox.SetText(spc.Category);
		kindBox.OnTextChanged += _ => spc.Category = kindBox.Text;

		Label colorLabel = new Label(parent);
		colorLabel.SetPosition(547, 70);
		colorLabel.SetSize(33, 18);
		colorLabel.SetText("Color");
		colorLabel.SetFont(Fonts.Paragraph);

		DropdownBox colorBox = new DropdownBox(parent);
		colorBox.SetPosition(604, 67);
		colorBox.SetSize(150, 24);
		colorBox.SetItems(Data.HardcodedData.BodyColors.Select(clr => new TreeNode(clr)).ToList());
		if (Data.HardcodedData.BodyColors.Contains(spc.Color)) colorBox.SetSelectedIndex(Data.HardcodedData.BodyColors.IndexOf(spc.Color));
		else colorBox.SetText(spc.Color);
		colorBox.OnSelectionChanged += _ => spc.Color = Data.HardcodedData.BodyColors[colorBox.SelectedIndex];

		Label heightLabel = new Label(parent);
		heightLabel.SetPosition(213, 110);
		heightLabel.SetSize(40, 18);
		heightLabel.SetText("Height");
		heightLabel.SetFont(Fonts.Paragraph);

		FloatNumericBox heightBox = new FloatNumericBox(parent);
		heightBox.SetPosition(274, 105);
		heightBox.SetSize(120, 30);
		heightBox.SetValue(spc.Height);
		heightBox.OnValueChanged += _ => spc.Height = heightBox.Value;

		Label heightMetersLabel = new Label(parent);
		heightMetersLabel.SetPosition(400, 110);
		heightMetersLabel.SetSize(12, 18);
		heightMetersLabel.SetText("m");
		heightMetersLabel.SetFont(Fonts.Paragraph);

		Label weightLabel = new Label(parent);
		weightLabel.SetPosition(209, 150);
		weightLabel.SetSize(44, 18);
		weightLabel.SetText("Weight");
		weightLabel.SetFont(Fonts.Paragraph);

		FloatNumericBox weightBox = new FloatNumericBox(parent);
		weightBox.SetPosition(274, 145);
		weightBox.SetSize(120, 30);
		weightBox.SetValue(spc.Weight);
		weightBox.OnValueChanged += _ => spc.Weight = weightBox.Value;

		Label weightKilosBox = new Label(parent);
		weightKilosBox.SetPosition(400, 150);
		weightKilosBox.SetSize(14, 18);
		weightKilosBox.SetText("kg");
		weightKilosBox.SetFont(Fonts.Paragraph);

		Label habitatLabel = new Label(parent);
		habitatLabel.SetPosition(535, 110);
		habitatLabel.SetSize(45, 18);
		habitatLabel.SetText("Habitat");
		habitatLabel.SetFont(Fonts.Paragraph);

		DropdownBox habitatBox = new DropdownBox(parent);
		habitatBox.SetPosition(604, 107);
		habitatBox.SetSize(150, 24);
		habitatBox.SetItems(Data.HardcodedData.Habitats.Select(h => new TreeNode(h)).ToList());
		if (Data.HardcodedData.Habitats.Contains(spc.Habitat)) habitatBox.SetSelectedIndex(Data.HardcodedData.Habitats.IndexOf(spc.Habitat));
		else habitatBox.SetText(spc.Habitat);
		habitatBox.OnSelectionChanged += _ => spc.Habitat = Data.HardcodedData.Habitats[habitatBox.SelectedIndex];

		Label shapeLabel = new Label(parent);
		shapeLabel.SetPosition(542, 150);
		shapeLabel.SetSize(38, 18);
		shapeLabel.SetText("Shape");
		shapeLabel.SetFont(Fonts.Paragraph);

		DropdownBox shapeBox = new DropdownBox(parent);
		shapeBox.SetPosition(604, 147);
		shapeBox.SetSize(106, 24);
		shapeBox.SetItems(Data.HardcodedData.BodyShapes.Select(s => new TreeNode(s)).ToList());
		if (Data.HardcodedData.BodyShapes.Contains(spc.Shape)) shapeBox.SetSelectedIndex(Data.HardcodedData.BodyShapes.IndexOf(spc.Shape));
		else shapeBox.SetText(spc.Shape);

		ImageBox shapePreviewBox = new ImageBox(parent);
		shapePreviewBox.SetPosition(716, 143);
		shapePreviewBox.SetSize(30, 30);
		shapePreviewBox.SetBitmap("assets/img/body_shapes.png");
		shapePreviewBox.SetSrcRect(new Rect(0, 30 * shapeBox.SelectedIndex, 30, 30));
		shapeBox.OnSelectionChanged += _ =>
		{
			spc.Shape = Data.HardcodedData.BodyShapes[shapeBox.SelectedIndex];
			shapePreviewBox.SetSrcRect(new Rect(0, 30 * shapeBox.SelectedIndex, 30, 30));
		};

		Label generationLabel = new Label(parent);
		generationLabel.SetPosition(186, 190);
		generationLabel.SetSize(67, 18);
		generationLabel.SetText("Generation");
		generationLabel.SetFont(Fonts.Paragraph);

		NumericBox generationBox = new NumericBox(parent);
		generationBox.SetPosition(274, 185);
		generationBox.SetSize(120, 30);
		generationBox.SetMinValue(1);
		generationBox.SetValue(spc.Generation);
		generationBox.OnValueChanged += _ => spc.Generation = generationBox.Value;

		Label dexEntryLabel = new Label(parent);
		dexEntryLabel.SetPosition(451, 230);
		dexEntryLabel.SetSize(61, 18);
		dexEntryLabel.SetText("Dex Entry");
		dexEntryLabel.SetFont(Fonts.Paragraph);

		MultilineTextBox dexEntryBox = new MultilineTextBox(parent);
		dexEntryBox.SetPosition(150, 260);
		dexEntryBox.SetSize(690, 107);
		dexEntryBox.SetFont(Fonts.Paragraph);
		dexEntryBox.SetText(spc.PokedexEntry);
		dexEntryBox.OnTextChanged += _ => spc.PokedexEntry = dexEntryBox.Text;

		parent.UpdateSize();
	}

	void CreateWildItemsContainer(DataContainer parent, Species spc)
	{
		ItemListWidget commonList = new ItemListWidget(parent);
		commonList.SetPosition(126, 54);
		commonList.SetSize(160, 216);
		commonList.SetText("Common");
		commonList.SetItems(spc.WildItemCommon);
		commonList.OnListChanged += _ => spc.WildItemCommon = commonList.AsResolvers;

		ItemListWidget uncommonList = new ItemListWidget(parent);
		uncommonList.SetPosition(406, 54);
		uncommonList.SetSize(160, 216);
		uncommonList.SetText("Uncommon");
		uncommonList.SetItems(spc.WildItemUncommon);
		uncommonList.OnListChanged += _ => spc.WildItemUncommon = uncommonList.AsResolvers;

		ItemListWidget rareList = new ItemListWidget(parent);
		rareList.SetPosition(686, 54);
		rareList.SetSize(160, 216);
		rareList.SetText("Rare");
		rareList.SetItems(spc.WildItemRare);
		rareList.OnListChanged += _ => spc.WildItemRare = rareList.AsResolvers;

		parent.UpdateSize();
	}
}
