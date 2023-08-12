using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			SpeciesList.RedrawNodeText(SpeciesList.SelectedItem);
		};

		Label nameLabel = new Label(parent);
		nameLabel.SetPosition(166, 59);
		nameLabel.SetSize(36, 18);
		nameLabel.SetText("Name");
		nameLabel.SetFont(Fonts.Paragraph);

		TextBox intNameBox = new TextBox(parent);
		intNameBox.SetPosition(217, 93);
		intNameBox.SetSize(150, 27);
		intNameBox.SetFont(Fonts.Paragraph);
		intNameBox.SetPopupStyle(false);
		intNameBox.SetText(spc.ID);
		intNameBox.OnTextChanged += _ => spc.ID = intNameBox.Text;

		Label intNameLabel = new Label(parent);
		intNameLabel.SetPosition(116, 97);
		intNameLabel.SetSize(86, 18);
		intNameLabel.SetText("Internal Name");
		intNameLabel.SetFont(Fonts.Paragraph);

		Label type1Label = new Label(parent);
		type1Label.SetPosition(164, 139);
		type1Label.SetSize(38, 18);
		type1Label.SetText("Type 1");
		type1Label.SetFont(Fonts.Paragraph);

		DropdownBox type1Box = new DropdownBox(parent);
		type1Box.SetPosition(217, 136);
		type1Box.SetSize(150, 24);
        type1Box.SetItems(Data.Sources.TypesListItemsAlphabetical);
		type1Box.SetSelectedIndex(Data.Sources.TypesListItemsAlphabetical.FindIndex(item => (Game.Type) item.Object == spc.Type1.Type));
		type1Box.OnSelectionChanged += _ => spc.Type1 = (TypeResolver) (Game.Type) type1Box.SelectedItem?.Object;

		bool hasType2 = spc.Type2 is not null;

		DropdownBox type2Box = new DropdownBox(parent);
		type2Box.SetPosition(217, 172);
		type2Box.SetSize(150, 24);
        type2Box.SetItems(Data.Sources.TypesListItemsAlphabetical);
		if (hasType2) type2Box.SetSelectedIndex(Data.Sources.TypesListItemsAlphabetical.FindIndex(item => (Game.Type) item.Object == spc.Type2.Type));
		type2Box.SetEnabled(hasType2);
		type2Box.OnSelectionChanged += _ => spc.Type2 = (TypeResolver) (Game.Type) type2Box.SelectedItem?.Object;

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
				spc.Type2 = (TypeResolver) (Game.Type) type2Box.SelectedItem?.Object;
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

		DropdownBox ability1Box = new DropdownBox(parent);
		ability1Box.SetPosition(629, 56);
		ability1Box.SetSize(150, 24);
        ability1Box.SetItems(Data.Sources.AbilitiesListItemsAlphabetical);
		ability1Box.SetSelectedIndex(Data.Sources.AbilitiesListItemsAlphabetical.FindIndex(item => (Ability) item.Object == spc.Abilities[0].Ability));
        ability1Box.OnSelectionChanged += _ =>
		{
			Species.Abilities[0] = (AbilityResolver) (Ability) ability1Box.SelectedItem?.Object;
		};

		bool hasAbil2 = spc.Abilities.Count > 1;

		DropdownBox ability2Box = new DropdownBox(parent);
		ability2Box.SetPosition(629, 96);
		ability2Box.SetSize(150, 24);
        ability2Box.SetItems(Data.Sources.AbilitiesListItemsAlphabetical);
		if (hasAbil2) ability2Box.SetSelectedIndex(Data.Sources.AbilitiesListItemsAlphabetical.FindIndex(item => (Ability) item.Object == spc.Abilities[1].Ability));
		ability2Box.SetEnabled(hasAbil2);
        ability2Box.OnSelectionChanged += _ =>
        {
            Species.Abilities[1] = (AbilityResolver) (Ability) ability2Box.SelectedItem?.Object;
        };

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
                Species.Abilities = new List<AbilityResolver>() 
                {
                    (AbilityResolver) (Ability) ability1Box.SelectedItem?.Object,
                    (AbilityResolver) (Ability) ability2Box.SelectedItem?.Object
                };
            }
            else
            {
                ability2Box.SetEnabled(false);
                Species.Abilities = new List<AbilityResolver>() { (AbilityResolver) (Ability) ability1Box.SelectedItem?.Object };
            }
        };

        bool hasHA = spc.HiddenAbilities.Count > 0;

		DropdownBox hiddenAbilityBox = new DropdownBox(parent);
		hiddenAbilityBox.SetPosition(629, 136);
		hiddenAbilityBox.SetSize(150, 24);
        hiddenAbilityBox.SetItems(Data.Sources.AbilitiesListItemsAlphabetical);
		if (hasHA) hiddenAbilityBox.SetSelectedIndex(Data.Sources.AbilitiesListItemsAlphabetical.FindIndex(item => (Ability) item.Object == spc.HiddenAbilities[0].Ability));
        hiddenAbilityBox.SetEnabled(hasHA);
        hiddenAbilityBox.OnSelectionChanged += _ =>
		{
            // Preserve data when more than 1 hidden ability is stored
            if (Species.HiddenAbilities.Count == 0) Species.HiddenAbilities.Add(null);
            Species.HiddenAbilities[0] = (AbilityResolver) (Ability) hiddenAbilityBox.SelectedItem?.Object;
		};

		CheckBox hiddenAbilityCheckBox = new CheckBox(parent);
		hiddenAbilityCheckBox.SetPosition(504, 140);
		hiddenAbilityCheckBox.SetText("Hidden Ability");
		hiddenAbilityCheckBox.SetChecked(hasHA);
		hiddenAbilityCheckBox.SetFont(Fonts.Paragraph);

        parent.UpdateSize();
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

		DropdownBox genderRatioBox = new DropdownBox(parent);
		genderRatioBox.SetPosition(274, 67);
		genderRatioBox.SetSize(160, 24);
		genderRatioBox.SetItems(Data.HardcodedData.GenderRatiosListItems);
		genderRatioBox.SetSelectedIndex(Data.HardcodedData.GenderRatiosListItems.FindIndex(item => item.Name == spc.GenderRatio));
		genderRatioBox.OnSelectionChanged += _ => spc.GenderRatio = genderRatioBox.SelectedItem.Name;

		Label eggGroup1Label = new Label(parent);
		eggGroup1Label.SetPosition(183, 110);
		eggGroup1Label.SetSize(70, 18);
		eggGroup1Label.SetText("Egg Group 1");
		eggGroup1Label.SetFont(Fonts.Paragraph);

		DropdownBox eggGroup1Box = new DropdownBox(parent);
		eggGroup1Box.SetPosition(274, 107);
		eggGroup1Box.SetSize(160, 24);
		eggGroup1Box.SetItems(Data.HardcodedData.EggGroupsListItems);
		eggGroup1Box.SetSelectedIndex(Data.HardcodedData.EggGroupsListItems.FindIndex(item => item.Name == spc.EggGroups[0]));
		eggGroup1Box.OnSelectionChanged += _ => spc.EggGroups[0] = eggGroup1Box.SelectedItem.Name;

		bool hasEggGroup2 = spc.EggGroups.Count > 1 && spc.EggGroups[1] is not null;

		DropdownBox eggGroup2Box = new DropdownBox(parent);
		eggGroup2Box.SetPosition(274, 147);
		eggGroup2Box.SetSize(160, 24);
		eggGroup2Box.SetEnabled(hasEggGroup2);
		eggGroup2Box.SetItems(Data.HardcodedData.EggGroupsListItems);
		if (hasEggGroup2) eggGroup2Box.SetSelectedIndex(Data.HardcodedData.EggGroupsListItems.FindIndex(item => item.Name == spc.EggGroups[1]));
		eggGroup2Box.OnSelectionChanged += _ => spc.EggGroups = new List<string>() { eggGroup1Box.SelectedItem.Name, eggGroup2Box.SelectedItem.Name };

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
				spc.EggGroups = new List<string>() { eggGroup1Box.SelectedItem.Name, eggGroup2Box.SelectedItem.Name };
			}
			else
			{
				eggGroup2Box.SetEnabled(false);
				spc.EggGroups = new List<string>() { eggGroup1Box.SelectedItem.Name };
			}
		};

		Label growthRateLabel = new Label(parent);
		growthRateLabel.SetPosition(564, 70);
		growthRateLabel.SetSize(76, 18);
		growthRateLabel.SetText("Growth Rate");
		growthRateLabel.SetFont(Fonts.Paragraph);

		DropdownBox growthRateBox = new DropdownBox(parent);
		growthRateBox.SetPosition(664, 67);
		growthRateBox.SetSize(160, 24);
		growthRateBox.SetItems(Data.HardcodedData.GrowthRatesListItems);
		growthRateBox.SetSelectedIndex(Data.HardcodedData.GrowthRatesListItems.FindIndex(item => item.Name == spc.GrowthRate));
		growthRateBox.OnSelectionChanged += _ => spc.GrowthRate = growthRateBox.SelectedItem.Name;

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

		parent.UpdateSize();
	}

	void CreateWildItemsContainer(DataContainer parent, Species spc)
	{
		bool hasCommonItem = spc.WildItemCommon.Count > 0 && spc.WildItemCommon[0] is not null;
		bool hasUncommonItem = spc.WildItemUncommon.Count > 0 && spc.WildItemUncommon[0] is not null;
		bool hasRareItem = spc.WildItemRare.Count > 0 && spc.WildItemRare[0] is not null;

		DropdownBox commonBox = new DropdownBox(parent);
		commonBox.SetPosition(444, 67);
		commonBox.SetSize(200, 24);
		commonBox.SetItems(Data.Sources.ItemsListItemsAlphabetical);
		if (hasCommonItem) commonBox.SetSelectedIndex(Data.Sources.ItemsListItemsAlphabetical.FindIndex(item => (Item) item.Object == spc.WildItemCommon[0].Item));
		commonBox.SetEnabled(hasCommonItem);
		commonBox.OnSelectionChanged += _ => spc.WildItemCommon = new List<ItemResolver>() { (ItemResolver) (Item) commonBox.SelectedItem?.Object };

		DropdownBox uncommonBox = new DropdownBox(parent);
		uncommonBox.SetPosition(444, 107);
		uncommonBox.SetSize(200, 24);
		uncommonBox.SetItems(Data.Sources.ItemsListItemsAlphabetical);
		if (hasUncommonItem) uncommonBox.SetSelectedIndex(Data.Sources.ItemsListItemsAlphabetical.FindIndex(item => (Item) item.Object == spc.WildItemUncommon[0].Item));
		uncommonBox.SetEnabled(hasUncommonItem);
		uncommonBox.OnSelectionChanged += _ => spc.WildItemUncommon = new List<ItemResolver>() { (ItemResolver) (Item) uncommonBox.SelectedItem?.Object };

		DropdownBox rareBox = new DropdownBox(parent);
		rareBox.SetPosition(444, 147);
		rareBox.SetSize(200, 24);
		rareBox.SetItems(Data.Sources.ItemsListItemsAlphabetical);
		if (hasRareItem) rareBox.SetSelectedIndex(Data.Sources.ItemsListItemsAlphabetical.FindIndex(item => (Item) item.Object == spc.WildItemRare[0].Item));
		rareBox.SetEnabled(hasRareItem);
		rareBox.OnSelectionChanged += _ => spc.WildItemRare = new List<ItemResolver>() { (ItemResolver) (Item) rareBox.SelectedItem?.Object };

		CheckBox commonCheckBox = new CheckBox(parent);
		commonCheckBox.SetPosition(337, 71);
		commonCheckBox.SetText("Common");
		commonCheckBox.SetFont(Fonts.Paragraph);
		commonCheckBox.SetChecked(hasCommonItem);
		commonCheckBox.OnCheckChanged += _ =>
		{
			if (commonCheckBox.Checked)
			{
				commonBox.SetEnabled(true);
				spc.WildItemCommon = new List<ItemResolver>() { (ItemResolver) (Item) commonBox.SelectedItem?.Object };
			}
			else
			{
				commonBox.SetEnabled(false);
				spc.WildItemCommon.Clear();
			}
		};

		CheckBox uncommonCheckBox = new CheckBox(parent);
		uncommonCheckBox.SetPosition(322, 111);
		uncommonCheckBox.SetText("Uncommon");
		uncommonCheckBox.SetFont(Fonts.Paragraph);
		uncommonCheckBox.SetChecked(hasUncommonItem);
		uncommonCheckBox.OnCheckChanged += _ =>
		{
			if (uncommonCheckBox.Checked)
			{
				uncommonBox.SetEnabled(true);
				spc.WildItemUncommon = new List<ItemResolver>() { (ItemResolver) (Item) uncommonBox.SelectedItem?.Object };
			}
			else
			{
				uncommonBox.SetEnabled(false);
				spc.WildItemUncommon.Clear();
			}
		};

		CheckBox rareCheckBox = new CheckBox(parent);
		rareCheckBox.SetPosition(366, 151);
		rareCheckBox.SetText("Rare");
		rareCheckBox.SetFont(Fonts.Paragraph);
		rareCheckBox.SetChecked(hasRareItem);
		rareCheckBox.OnCheckChanged += _ =>
		{
			if (rareCheckBox.Checked)
			{
				rareBox.SetEnabled(true);
				spc.WildItemRare = new List<ItemResolver>() { (ItemResolver) (Item) rareBox.SelectedItem?.Object };
			}
			else
			{
				rareBox.SetEnabled(false);
				spc.WildItemRare.Clear();
			}
		};

		parent.UpdateSize();
	}
}
