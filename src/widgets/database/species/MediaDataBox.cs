using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.IO;

namespace RPGStudioMK.Widgets;

public partial class DataTypeSpecies
{
	string ResolveSpeciesSprite(string path, string speciesID, int form, int gender, bool shiny, bool shadow, string subFolder)
	{
		string trySubfolder = subFolder + "/";
		string trySpecies = speciesID;
		string tryForm = form > 0 ? "_" + form.ToString() : "";
		string tryGender = gender == 1 ? "_female" : "";
		string tryShadow = shadow ? "_shadow" : "";
		List<(int, string, string)> factors = new List<(int, string, string)>();
		if (shiny) factors.Add((4, subFolder + " shiny/", trySubfolder));
		if (shadow) factors.Add((3, tryShadow, ""));
		if (gender == 1) factors.Add((2, tryGender, ""));
		if (form > 0) factors.Add((1, tryForm, ""));
		factors.Add((0, trySpecies, "000"));
		// Go through each combination of parameters in turn to find an existing sprite
		for (int i = 0; i < Math.Pow(2, factors.Count); i++)
		{
			for (int j = 0; j < factors.Count; j++)
			{
				int index = j;
				string value = (i / (int) Math.Pow(2, index)) % 2 == 0 ? factors[j].Item2 : factors[j].Item3;
				switch (factors[j].Item1)
				{
					case 0:
						trySpecies = value;
						break;
					case 1:
						tryForm = value;
						break;
					case 2:
						tryGender = value;
						break;
					case 3:
						tryShadow = value;
						break;
					case 4:
						trySubfolder = value;
						break;
				}
			}
			string tryPath = path + trySubfolder + trySpecies + tryForm + tryGender + tryShadow;
			if (Bitmap.FindRealFilename(tryPath) is not null) return tryPath;
		}
		return null;
	}

	string ResolveFrontSprite(Species spc, bool isFemale, bool shiny, bool shadow)
	{
		return ResolveSpeciesSprite(Data.ProjectPath + "/Graphics/Pokemon/", spc.BaseSpecies.ID, spc.Form, isFemale ? 1 : 0, shiny, shadow, "Front");
	}

	string ResolveBackSprite(Species spc, bool isFemale, bool shiny, bool shadow)
	{
		return ResolveSpeciesSprite(Data.ProjectPath + "/Graphics/Pokemon/", spc.BaseSpecies.ID, spc.Form, isFemale ? 1 : 0, shiny, shadow, "Back");
	}

	string ResolveIconSprite(Species spc, bool isFemale, bool shiny, bool shadow)
	{
		return ResolveSpeciesSprite(Data.ProjectPath + "/Graphics/Pokemon/", spc.BaseSpecies.ID, spc.Form, isFemale ? 1 : 0, shiny, shadow, "Icons");
	}

	string ResolveFootprintSprite(Species spc)
	{
		if (spc.Form > 0)
		{
			string tryPath = Data.ProjectPath + $"/Graphics/Pokemon/Footprints/{spc.BaseSpecies.ID}_{spc.Form}";
			if (Bitmap.FindRealFilename(tryPath) is not null) return tryPath;
		}
		string path = Data.ProjectPath + $"/Graphics/Pokemon/Footprints/{spc.BaseSpecies.ID}";
		if (Bitmap.FindRealFilename(path) is not null) return path;
		return null;
	}

	string ResolveEggSprite(Species spc, string suffix = "")
	{
		if (spc.Form > 0)
		{
			string tryPath = Data.ProjectPath + $"/Graphics/Pokemon/Eggs/{spc.BaseSpecies.ID}_{spc.Form}{suffix}";
			if (Bitmap.FindRealFilename(tryPath) is not null) return tryPath;
		}
		string path = Data.ProjectPath + $"/Graphics/Pokemon/Eggs/{spc.BaseSpecies.ID}{suffix}";
		if (Bitmap.FindRealFilename(path) is not null) return path;
		path = Data.ProjectPath + $"/Graphics/Pokemon/Eggs/000{suffix}";
		if (Bitmap.FindRealFilename(path) is not null) return path;
		return null;
	}

	string ResolveCry(Species spc, string suffix = "")
	{
		if (spc.Form > 0)
		{
			string tryPath = Data.ProjectPath + $"/Audio/SE/Cries/{spc.BaseSpecies.ID}_{spc.Form}{suffix}";
			if (Path.Exists(tryPath) ||
				Path.Exists(tryPath + ".mp3") ||
				Path.Exists(tryPath + ".ogg") ||
				Path.Exists(tryPath + ".mid") ||
				Path.Exists(tryPath + ".midi") ||
				Path.Exists(tryPath + ".wav")) return tryPath;
		}
		string path = Data.ProjectPath + $"/Audio/SE/Cries/{spc.BaseSpecies.ID}{suffix}";
		if (Path.Exists(path) ||
			Path.Exists(path + ".mp3") ||
			Path.Exists(path + ".ogg") ||
			Path.Exists(path + ".mid") ||
			Path.Exists(path + ".midi") ||
			Path.Exists(path + ".wav")) return path;
		return null;
	}

	void CreateSpritesContainer(DataContainer parent, Species spc)
	{
		CheckBox femaleCheckBox = new CheckBox(parent);
		femaleCheckBox.SetPosition(450, 46);
		femaleCheckBox.SetText("Female");
		femaleCheckBox.SetFont(Fonts.Paragraph);
		femaleCheckBox.SetChecked(Editor.GeneralSettings.ShowSpeciesSpritesAsFemale);

		CheckBox shadowCheckBox = new CheckBox(parent);
		shadowCheckBox.SetPosition(450, 77);
		shadowCheckBox.SetText("Shadow");
		shadowCheckBox.SetFont(Fonts.Paragraph);
		shadowCheckBox.SetChecked(Editor.GeneralSettings.ShowSpeciesSpritesAsShadow);

		// Front
		Label frontLabel = new Label(parent);
		frontLabel.SetPosition(249, 154);
		frontLabel.SetSize(33, 18);
		frontLabel.SetText("Front");
		frontLabel.SetFont(Fonts.Paragraph);

		ImagePreviewContainer frontBox = new ImagePreviewContainer(parent);
		frontBox.SetPosition(132, 180);
		frontBox.SetSize(266, 266);
		frontBox.SetFillMode(FillMode.Center);

		// Back
		Label backLabel = new Label(parent);
		backLabel.SetPosition(711, 154);
		backLabel.SetSize(29, 18);
		backLabel.SetText("Back");
		backLabel.SetFont(Fonts.Paragraph);

		ImagePreviewContainer backBox = new ImagePreviewContainer(parent);
		backBox.SetPosition(592, 180);
		backBox.SetSize(266, 266);
		backBox.SetFillMode(FillMode.Center);

		// Shiny Front
		Label shinyFrontLabel = new Label(parent);
		shinyFrontLabel.SetPosition(230, 474);
		shinyFrontLabel.SetSize(71, 18);
		shinyFrontLabel.SetText("Shiny Front");
		shinyFrontLabel.SetFont(Fonts.Paragraph);

		ImagePreviewContainer shinyFrontBox = new ImagePreviewContainer(parent);
		shinyFrontBox.SetPosition(132, 500);
		shinyFrontBox.SetSize(266, 266);
		shinyFrontBox.SetFillMode(FillMode.Center);

		// Shiny Back
		Label shinyBackLabel = new Label(parent);
		shinyBackLabel.SetPosition(694, 474);
		shinyBackLabel.SetSize(66, 18);
		shinyBackLabel.SetText("Shiny Back");
		shinyBackLabel.SetFont(Fonts.Paragraph);

		ImagePreviewContainer shinyBackBox = new ImagePreviewContainer(parent);
		shinyBackBox.SetPosition(592, 500);
		shinyBackBox.SetSize(266, 266);
		shinyBackBox.SetFillMode(FillMode.Center);

		// Icon
		Label iconLabel = new Label(parent);
		iconLabel.SetPosition(167, 794);
		iconLabel.SetSize(26, 18);
		iconLabel.SetText("Icon");
		iconLabel.SetFont(Fonts.Paragraph);

		CheckBox iconAnimateBox = new CheckBox(parent);
		iconAnimateBox.SetPosition(151, 928);
		iconAnimateBox.SetText("Animate");
		iconAnimateBox.SetFont(Fonts.Paragraph);
		iconAnimateBox.SetChecked(Editor.GeneralSettings.AnimateSpeciesIcons);
		iconAnimateBox.OnCheckChanged += _ => Editor.GeneralSettings.AnimateSpeciesIcons = iconAnimateBox.Checked;

		ImagePreviewContainer iconBox = new ImagePreviewContainer(parent);
		iconBox.SetPosition(132, 820);
		iconBox.SetSize(106, 106);
		iconBox.SetFillMode(FillMode.Center);
		iconBox.SetTimer("frame", 200);
		iconBox.OnUpdate += _ =>
		{
			if (iconBox.TimerPassed("frame") && iconBox.Bitmap is not null)
			{
				int srcx = iconBox.SrcRect.X == 0 ? iconBox.Bitmap.Width / 2 : 0;
				if (!iconAnimateBox.Checked) srcx = 0;
				iconBox.SetSrcRect(new Rect(srcx, 0, iconBox.Bitmap.Width / 2, iconBox.Bitmap.Height));
				iconBox.ResetTimer("frame");
			}
		};

		// Footprint
		Label footprintLabel = new Label(parent);
		footprintLabel.SetPosition(152, 956);
		footprintLabel.SetSize(57, 18);
		footprintLabel.SetText("Footprint");
		footprintLabel.SetFont(Fonts.Paragraph);

		ImagePreviewContainer footprintBox = new ImagePreviewContainer(parent);
		footprintBox.SetPosition(132, 980);
		footprintBox.SetSize(106, 106);
		footprintBox.SetZoomX(2);
		footprintBox.SetZoomY(2);
		footprintBox.SetFillMode(FillMode.Center);
		footprintBox.SetGridVisible(false);

		// Egg icon
		Label eggIconLabel = new Label(parent);
		eggIconLabel.SetPosition(315, 794);
		eggIconLabel.SetSize(51, 18);
		eggIconLabel.SetText("Egg Icon");
		eggIconLabel.SetFont(Fonts.Paragraph);

		CheckBox eggIconAnimateBox = new CheckBox(parent);
		eggIconAnimateBox.SetPosition(310, 928);
		eggIconAnimateBox.SetText("Animate");
		eggIconAnimateBox.SetFont(Fonts.Paragraph);
		eggIconAnimateBox.SetChecked(Editor.GeneralSettings.AnimateEggIcons);
		eggIconAnimateBox.OnCheckChanged += _ => Editor.GeneralSettings.AnimateEggIcons = eggIconAnimateBox.Checked;

		ImagePreviewContainer eggIconBox = new ImagePreviewContainer(parent);
		eggIconBox.SetPosition(292, 820);
		eggIconBox.SetSize(106, 106);
		eggIconBox.SetFillMode(FillMode.Center);
		eggIconBox.SetTimer("frame", 200);
		eggIconBox.OnUpdate += _ =>
		{
			if (eggIconBox.TimerPassed("frame") && eggIconBox.Bitmap is not null)
			{
				int srcx = !eggIconAnimateBox.Checked || eggIconBox.SrcRect.X == 0 ? eggIconBox.Bitmap.Width / 2 : 0;
				eggIconBox.SetSrcRect(new Rect(srcx, 0, eggIconBox.Bitmap.Width / 2, eggIconBox.Bitmap.Height));
				eggIconBox.ResetTimer("frame");
			}
		};

		// Egg
		Label eggLabel = new Label(parent);
		eggLabel.SetPosition(716, 794);
		eggLabel.SetSize(23, 18);
		eggLabel.SetText("Egg");
		eggLabel.SetFont(Fonts.Paragraph);

		ImagePreviewContainer eggBox = new ImagePreviewContainer(parent);
		eggBox.SetPosition(592, 820);
		eggBox.SetSize(266, 266);
		eggBox.SetFillMode(FillMode.Center);

		Container crackingContainer = new Container(parent);
		crackingContainer.SetPosition(eggBox.Position.X + 5, eggBox.Position.Y + 5);
		crackingContainer.SetSize(eggBox.Size.Width - 10, eggBox.Size.Height - 10);

		ImageBox crackingBox = new ImageBox(crackingContainer);
		crackingBox.SetFillMode(FillMode.Center);
		crackingBox.SetTimer("frame", 500);
		crackingBox.OnUpdate += _ =>
		{
			if (crackingBox.TimerPassed("frame"))
			{
				int crackUnitWidth = crackingBox.Bitmap.Width / 5;
				crackingBox.SetSrcRect(new Rect((crackingBox.SrcRect.X + crackUnitWidth) % (crackUnitWidth * 5), 0, crackUnitWidth, crackingBox.Bitmap.Height));
				crackingBox.ResetTimer("frame");
			}
		};
		crackingBox.SetVisible(Editor.GeneralSettings.AnimateEggCracks);

		CheckBox eggAnimateBox = new CheckBox(parent);
		eggAnimateBox.SetPosition(693, 1100);
		eggAnimateBox.SetText("Animate");
		eggAnimateBox.SetFont(Fonts.Paragraph);
		eggAnimateBox.SetChecked(Editor.GeneralSettings.AnimateEggCracks);
		eggAnimateBox.OnCheckChanged += _ =>
		{
			Editor.GeneralSettings.AnimateEggCracks = eggAnimateBox.Checked;
			crackingBox.SetVisible(eggAnimateBox.Checked);
		};

		void UpdateAllPaths()
		{
			frontBox.SetBitmap(ResolveFrontSprite(spc, femaleCheckBox.Checked, false, shadowCheckBox.Checked));
			backBox.SetBitmap(ResolveBackSprite(spc, femaleCheckBox.Checked, false, shadowCheckBox.Checked));
			shinyFrontBox.SetBitmap(ResolveFrontSprite(spc, femaleCheckBox.Checked, true, shadowCheckBox.Checked));
			shinyBackBox.SetBitmap(ResolveBackSprite(spc, femaleCheckBox.Checked, true, shadowCheckBox.Checked));
			iconBox.SetBitmap(ResolveIconSprite(spc, femaleCheckBox.Checked, false, shadowCheckBox.Checked));
			iconBox.SetSrcRect(new Rect(0, 0, iconBox.Bitmap.Width / 2, iconBox.Bitmap.Height));
			footprintBox.SetBitmap(ResolveFootprintSprite(spc));
			eggBox.SetBitmap(ResolveEggSprite(spc));
			crackingBox.SetBitmap(ResolveEggSprite(spc, "_cracks"));
			crackingBox.SetSrcRect(new Rect(0, 0, crackingBox.Bitmap.Width / 5, crackingBox.Bitmap.Height));
			eggIconBox.SetBitmap(ResolveEggSprite(spc, "_icon"));
			eggIconBox.SetSrcRect(new Rect(eggIconBox.Bitmap.Width / 2, 0, eggIconBox.Bitmap.Width / 2, eggIconBox.Bitmap.Height));
		}

		femaleCheckBox.OnCheckChanged += _ =>
		{
			Editor.GeneralSettings.ShowSpeciesSpritesAsFemale = femaleCheckBox.Checked;
			UpdateAllPaths();
		};
		shadowCheckBox.OnCheckChanged += _ => 
		{
			Editor.GeneralSettings.ShowSpeciesSpritesAsShadow = shadowCheckBox.Checked;
			UpdateAllPaths(); 
		};
		UpdateAllPaths();

		parent.UpdateSize();
	}

	void CreateAudioContainer(DataContainer parent, Species spc) 
	{
		Label cryLabel = new Label(parent);
		cryLabel.SetPosition(93, 63);
		cryLabel.SetSize(24, 18);
		cryLabel.SetText("Cry:");
		cryLabel.SetFont(Fonts.Paragraph);

		Button playCry = new Button(parent);
		playCry.SetPosition(134, 56);
		playCry.SetSize(69, 33);
		playCry.SetFont(Fonts.ParagraphBold);
		playCry.SetText("Play");
		playCry.OnClicked += _ =>
		{
			Audio.Play(ResolveCry(spc));
		};

		Label faintCryLabel = new Label(parent);
		faintCryLabel.SetPosition(63, 103);
		faintCryLabel.SetSize(54, 18);
		faintCryLabel.SetText("Faint cry:");
		faintCryLabel.SetFont(Fonts.Paragraph);

		Button playFaintCry = new Button(parent);
		playFaintCry.SetPosition(134, 96);
		playFaintCry.SetSize(69, 33);
		playFaintCry.SetFont(Fonts.ParagraphBold);
		playFaintCry.SetText("Play");
		playFaintCry.OnClicked += _ =>
		{
			Audio.Play(ResolveCry(spc, "_faint") ?? ResolveCry(spc));
		};

		parent.UpdateSize();
	}
}
