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
				eew.Dispose();
				panel.UpdateLayout();
				parent.UpdateSize();
			};
		};

		panel.UpdateLayout();
		parent.UpdateSize();
	}

	void CreatePrevoContainer(DataContainer parent, Species spc)
	{
		List<(SpeciesResolver Base, Evolution Evo)> prevos = new List<(SpeciesResolver, Evolution)>();
		foreach (Species candidate in Data.Species.Values)
		{
			foreach (Evolution evo in candidate.Evolutions)
			{
				if (evo.Species.Species != spc) continue;
				if (prevos.Any(prevo => prevo.Base.Species == candidate && prevo.Evo.Type == evo.Type && prevo.Evo.Parameter.Equals(evo.Parameter))) continue;
				prevos.Add(((SpeciesResolver) candidate, evo));
			}
		}

		VStackPanel panel = new VStackPanel(parent);
		panel.SetPosition(50, 50);
		panel.SetWidth(900);

		foreach ((SpeciesResolver Base, Evolution evo) in prevos)
		{
			EvolutionEntryWidget eew = new EvolutionEntryWidget(true, spc, panel);
			eew.SetEvolution(evo, Base);
			eew.SetMargins(0, 3);
			eew.SetEnabled(false);
		}

		panel.UpdateLayout();
		parent.UpdateSize();
	}
}
