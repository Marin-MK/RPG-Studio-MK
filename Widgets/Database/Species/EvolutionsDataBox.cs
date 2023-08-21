using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public partial class DataTypeSpecies
{
	void CreateEvoContainer(DataContainer parent, Species spc)
	{
		VStackPanel panel = new VStackPanel(parent);
		panel.SetPosition(50, 50);
		panel.SetWidth(900);

		foreach (Evolution evo in spc.Evolutions)
		{
			if (evo.Prevolution) continue;
			EvolutionEntryWidget eew = new EvolutionEntryWidget(false, spc, panel);
			eew.SetEvolution(evo);
			eew.SetMargins(0, 3);
			eew.OnButtonClicked += _ =>
			{
				spc.Evolutions.Remove(evo);
				evo.Species.Species.Prevolutions.RemoveAll(ev => ev.Species.Species == spc && ev.Type == evo.Type && ev.Parameter == evo.Parameter);
				eew.Dispose();
				panel.UpdateLayout();
				parent.UpdateSize();
			};
		}

		Container addContainer = new Container(panel);
		addContainer.SetMargins(0, 10);

		Button addButton = new Button(addContainer);
		addButton.SetText("Add Evolution");
		addButton.SetWidth(160);
		addButton.OnClicked += _ =>
		{
			Evolution evo = new Evolution((SpeciesResolver) (Species) Data.Sources.SpeciesAndFormsListItemsAlphabetical[0].Object, Data.HardcodedData.EvolutionMethods[0], "");
			spc.Evolutions.Insert(0, evo);
			EvolutionEntryWidget eew = new EvolutionEntryWidget(false, spc, panel, 0);
			eew.SetEvolution(evo);
			eew.SetMargins(0, 3);
			eew.OnButtonClicked += _ =>
			{
				spc.Evolutions.Remove(evo);
				evo.Species.Species.Prevolutions.RemoveAll(ev => ev.Species.Species == spc && ev.Type == evo.Type && ev.Parameter == evo.Parameter);
				eew.Dispose();
				panel.UpdateLayout();
				parent.UpdateSize();
			};
			evo.Species.Species.Prevolutions.Add(new Evolution((SpeciesResolver) spc, evo.Type, evo.Parameter, true));
			panel.UpdateLayout();
			parent.UpdateSize();
		};

		panel.UpdateLayout();
		parent.UpdateSize();
	}

	void CreatePrevoContainer(DataContainer parent, Species spc)
	{
		VStackPanel panel = new VStackPanel(parent);
		panel.SetPosition(50, 50);
		panel.SetWidth(900);

		foreach (Evolution evo in spc.Prevolutions)
		{
			EvolutionEntryWidget eew = new EvolutionEntryWidget(true, spc, panel);
			eew.SetEvolution(evo, evo.Species);
			eew.SetMargins(0, 3);
			eew.SetEnabled(false);
		}

		panel.UpdateLayout();
		parent.UpdateSize();
	}
}
