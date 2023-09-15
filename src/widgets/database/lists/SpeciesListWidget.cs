using RPGStudioMK.Game;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class SpeciesListWidget : DropdownListWidget<SpeciesDropdownBox>
{
	public List<SpeciesResolver> AsResolvers => Items.Select(item => (SpeciesResolver) (Species) item.Object).ToList();
	public List<Species> AsSpecies => Items.Select(item => (Species) item.Object).ToList();

    public SpeciesListWidget(bool includeForms, IContainer parent) : base(parent, new object[] { includeForms })
    {
		SetAvailableItems(
			includeForms ? Data.Sources.SpeciesAndForms : Data.Sources.Species
		);
	}

	public void SetItems(List<SpeciesResolver> items)
	{
		SetItems(items.Select(item => new TreeNode(item.Valid ? item.Species.Name : item.ID, item.Valid ? item.Species : null)).ToList());
	}

	public void SetItems(List<Species> items)
	{
		SetItems(items.Select(item => new TreeNode(item.Form == 0 ? item.Name : $"{item.Name} ({item.FormName ?? item.Form.ToString()})", item)).ToList());
	}
}
