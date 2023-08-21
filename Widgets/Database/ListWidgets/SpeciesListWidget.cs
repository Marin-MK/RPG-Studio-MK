﻿using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class SpeciesListWidget : DropdownListWidget
{
	public List<SpeciesResolver> AsResolvers => Items.Select(item => (SpeciesResolver) (Species) item.Object).ToList();
	public List<Species> AsSpecies => Items.Select(item => (Species) item.Object).ToList();

    public SpeciesListWidget(bool includeForms, IContainer parent) : base(parent)
    {
		SetAvailableItems(
			includeForms ? Data.Sources.SpeciesAndFormsListItemsAlphabetical : Data.Sources.SpeciesListItemsAlphabetical
		);
	}

	public void SetItems(List<SpeciesResolver> items)
	{
		SetItems(items.Select(item => new ListItem(item.Species.Name, item.Species)).ToList());
	}

	public void SetItems(List<Species> items)
	{
		SetItems(items.Select(item => new ListItem(item.Form == 0 ? item.Name : $"{item.Name} ({item.FormName ?? item.Form.ToString()})", item)).ToList());
	}
}