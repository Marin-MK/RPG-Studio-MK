using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeSpecies : GenericDataTypeBase<Species>
{
    protected SubmodeView Tabs;
	protected int? LastTabIndex;

    public DataTypeSpecies(IContainer Parent) : base(Parent)
	{
		this.Text = "Species";
		this.DataType = BinaryData.SPECIES;
		this.DataSource = Data.Species;
		this.GetNodeDataSource = () => Data.Sources.SpeciesAndForms;
		this.GetID = spc => spc.ID;
		this.SetID = (spc, id) => spc.ID = id;
		this.InvalidateData = Data.Sources.InvalidateSpecies;
		this.GetLastID = () => Editor.ProjectSettings.LastSpeciesID;
		this.SetLastID = id => Editor.ProjectSettings.LastSpeciesID = id;
		this.GetLastScroll = () => Editor.ProjectSettings.LastSpeciesScroll;
		this.SetLastScroll = (x) => Editor.ProjectSettings.LastSpeciesScroll = x;
		this.Containers = new List<(string Name, string ID, System.Action<DataContainer, Species> CreateContainer, System.Func<Species, bool>? Condition)>()
		{
            // Main
            ("Main", "SPECIES_MAIN", CreateMainContainer, spc => Tabs.SelectedIndex == 0),
			("Form", "SPECIES_FORM", CreateFormContainer, spc => Tabs.SelectedIndex == 0 && spc.Form != 0),
			("Stats", "SPECIES_STATS", CreateStatsContainer, spc => Tabs.SelectedIndex == 0),
			("Effort Points", "SPECIES_EVS", CreateEVContainer, spc => Tabs.SelectedIndex == 0),
			("Misc", "SPECIES_MISC", CreateMiscContainer, spc => Tabs.SelectedIndex == 0),
			("Dex Info", "SPECIES_DEXINFO", CreateDexInfoContainer, spc => Tabs.SelectedIndex == 0),
			("Wild Held Items", "SPECIES_HELDITEMS", CreateWildItemsContainer, spc => Tabs.SelectedIndex == 0),

            // Moves
			("Level-Up Moves", "SPECIES_LEVELUP", CreateLevelContainer, spc => Tabs.SelectedIndex == 1),
			("Evolution Moves", "SPECIES_EVOMOVES", CreateEvoMovesContainer, spc => Tabs.SelectedIndex == 1),
			("TMs & HMs", "SPECIES_TMS", CreateTMContainer, spc => Tabs.SelectedIndex == 1),
			("Egg Moves", "SPECIES_EGGMOVES", CreateEggMovesContainer, spc => Tabs.SelectedIndex == 1),
			("Tutor Moves", "SPECIES_TUTOR", CreateTutorMovesContainer, spc => Tabs.SelectedIndex == 1),

            // Evolutions
			("Evolves Into", "SPECIES_EVOLUTION", CreateEvoContainer, spc => Tabs.SelectedIndex == 2),
			("Evolves From", "SPECIES_PREVO", CreatePrevoContainer, spc => Tabs.SelectedIndex == 2),

            // Media
			("Sprites", "SPECIES_SPRITES", CreateSpritesContainer, spc => Tabs.SelectedIndex == 3),
			("Audio", "SPECIES_AUDIO", CreateAudioContainer, spc => Tabs.SelectedIndex == 3),
		};

		LateConstructor();
	}

	protected override void LateConstructor()
	{
		Grid = new Grid(this);
		Grid.SetColumns(
			new GridSize(201, Unit.Pixels),
			new GridSize(1),
			new GridSize(0, Unit.Pixels)
		);
		Grid.SetRows(
			new GridSize(29, Unit.Pixels),
			new GridSize(1)
		);

		Fade = new VignetteFade(Grid);
		Fade.SetGrid(1, 1);
		Fade.SetZIndex(2);

		DataList = new DataTypeSubTree("Species", Grid);
		DataList.SetBackgroundColor(28, 50, 73);
		DataList.SetGridRow(0, 1);
		DataList.SetXOffset(-6);
		DataList.OnScrolling += _ => SetLastScroll(DataList.GetScroll());
	}

	public override void Initialize()
	{
		Tabs = new SubmodeView(Grid);
		Tabs.SetBackgroundColor(23, 40, 56);
		Tabs.SetTextY(4);
		Tabs.SetHeaderHeight(29);
		Tabs.SetHeaderSelHeight(0);
		Tabs.SetHeaderWidth(140);
		Tabs.SetGrid(0, 1, 1, 2);
		Tabs.SetFont(Fonts.TabFont);
		Tabs.SetCentered(true);
		Tabs.SetHeaderBackgroundColor(10, 23, 37);
		Tabs.CreateTab("Main");
		Tabs.CreateTab("Moves");
		Tabs.CreateTab("Evolutions");
		Tabs.CreateTab("Media");
		Tabs.SelectTab(Editor.ProjectSettings.LastSpeciesSubmode);
		Tabs.OnSelectionChanged += _ => UpdateSelection();

		MainContainer = new Container(Grid);
		MainContainer.SetGrid(1, 1);

		VScrollBar vs = new VScrollBar(MainContainer);
		vs.SetRightDocked(true);
		vs.SetPadding(0, 3, 1, 3);
		vs.SetVDocked(true);
		vs.SetZIndex(1);
		vs.SetScrollStep(32);
		MainContainer.SetVScrollBar(vs);
		MainContainer.VAutoScroll = true;

		HScrollBar hs = new HScrollBar(MainContainer);
		hs.SetBottomDocked(true);
		hs.SetPadding(3, 0, 3, 1);
		hs.SetHDocked(true);
		hs.SetZIndex(1);
		hs.SetScrollStep(32);
		MainContainer.SetHScrollBar(hs);
		MainContainer.HAutoScroll = true;

		HintWindow = new HintWindow(MainContainer);
		HintWindow.ConsiderInAutoScrollCalculation = HintWindow.ConsiderInAutoScrollPositioningX = HintWindow.ConsiderInAutoScrollPositioningY = false;
		HintWindow.SetBottomDocked(true);
		HintWindow.SetPadding(-3, 0, 0, -9);
		HintWindow.SetZIndex(10);
		HintWindow.SetVisible(false);

		ScrollContainer = new Container(MainContainer);

		DataList.OnSelectionChanged += _ => UpdateSelection();

		DataList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New Species")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null && ((Species) DataList.HoveringItem.Object).Form == 0,
                OnClicked = NewSpecies
            },
            new MenuItem("New Form")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null,
                OnClicked = NewForm
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null,
                OnClicked = _ => CutData()
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null,
                OnClicked = _ => CopyData()
            },
            new MenuItem("Paste (smart)")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null && Clipboard.IsValid(BinaryData.SPECIES),
                OnClicked = _ => PasteData()
            },
			new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null,
                OnClicked = _ => DeleteData()
            }
        });
		Grid.UpdateLayout();

		RedrawList(GetLastID());
	}

	protected override void RedrawList(Species? speciesToSelect = null)
	{
		List<TreeNode> SpeciesItems = new List<TreeNode>();
        TreeNode? nodeToSelect = null;
		foreach (TreeNode listItem in Data.Sources.SpeciesAndForms)
		{
            Species spc = (Species) listItem.Object;
			// Skip subforms for now
			if (spc.Form != 0) continue;
			TreeNode item = new TreeNode(spc.Name, spc);
            if (speciesToSelect == spc) nodeToSelect = item;
            if (Editor.ProjectSettings.HiddenSpeciesForms.Contains(spc.ID)) item.Collapse();
            item.OnExpansionChanged += _ =>
            {
                if (item.Expanded) Editor.ProjectSettings.HiddenSpeciesForms.RemoveAll(id => id == spc.ID);
                else if (!Editor.ProjectSettings.HiddenSpeciesForms.Contains(spc.ID)) Editor.ProjectSettings.HiddenSpeciesForms.Add(spc.ID);
            };
			SpeciesItems.Add(item);
		}
		foreach (TreeNode listItem in Data.Sources.SpeciesAndForms)
		{
			Species spc = (Species) listItem.Object;
			// Skip base forms
			if (spc.Form == 0) continue;
			TreeNode parent = SpeciesItems.Find(n => ((Species) n.Object).ID == spc.BaseSpecies.ID);
			TreeNode item = new TreeNode($"{spc.Form} - {spc.FormName ?? spc.Name}", spc);
            int idx = parent.Children.FindIndex(node => spc.Form < ((Species) ((TreeNode) node).Object).Form);
            if (idx == -1) idx = parent.Children.Count;
            parent.InsertChild(idx, item);
            if (speciesToSelect == spc) nodeToSelect = item;
            if (Editor.ProjectSettings.HiddenSpeciesForms.Contains(spc.BaseSpecies.ID)) parent.Collapse();
		}
		DataList.SetItems(SpeciesItems);
        if (nodeToSelect != null)
        {
            DataList.SetScroll(GetLastScroll());
            DataList.SetSelectedNode(nodeToSelect);
        }
	}

	protected override Species CreateData()
	{
		throw new NotImplementedException();
	}

	protected override void UpdateSelection(bool forceUpdate = false)
    {
		Editor.ProjectSettings.LastSpeciesSubmode = Tabs.SelectedIndex;
		if (!forceUpdate && LastDisplayedData is not null && LastDisplayedData.Equals(this.SelectedItem) &&
			LastTabIndex is not null && LastTabIndex == Tabs.SelectedIndex) return;
        base.UpdateSelection(true);
		LastTabIndex = Tabs.SelectedIndex;
	}

    void NewSpecies(BaseEventArgs e)
    {
        Species species = Species.Create();
        species.Name = "Missingno.";
        species.ID = EnsureUniqueID("MISSINGNO");
        species.BaseSpecies = (SpeciesResolver) species.ID;
        DataSource.Add(species.ID, species);
        InvalidateData();
        RedrawList(species);
    }

    void NewForm(BaseEventArgs e)
    {
		Species parentSpecies = DataList.HoveringItem.Parent == DataList.Root ? HoveringItem : (Species) DataList.HoveringItem.Parent.Object;
        Species newForm = (Species) parentSpecies.Clone();
        newForm.BaseSpecies = (SpeciesResolver) parentSpecies.ID;
        newForm.Form = (int) GetFreeFormNumber(newForm.BaseSpecies, 0, 1);
        newForm.ID = newForm.BaseSpecies.ID + "_" + newForm.Form.ToString();
        DataSource.Add(newForm.ID, newForm);
        InvalidateData();
        RedrawList(newForm);
    }

    protected override void CopyData()
	{
		if (DataList.HoveringItem is null) return;
		List<Species> items = new List<Species>();
		// If we have more than 1 node selected, and the hovering node is part of the selection, delete the whole selection
		if (DataList.SelectedItems.Count > 1 && DataList.SelectedItems.Contains(DataList.HoveringItem))
		{
			items = DataList.SelectedItems.Select(n => (Species) n.Object).ToList();
			// Before we copy this selection, we first need to know if it can be pasted.
			// We enforce one simple rule to make that determination:
			// - All forms must have their base form included in the selection, OR
			// - The selection consists only of forms (only paste-able on base forms)
			// If these conditions are not met, we show a hint stating as such.
			bool allSubforms = items.All(d => d.Form != 0);
			if (!allSubforms)
			{
				// Now we must test whether all subforms have their base forms included as well.
				for (int i = 0; i < items.Count; i++)
				{
					// We are only interested in subforms
					if (items[i].Form == 0) continue;
					if (!items.Exists(s => s.Form == 0 && s.ID == items[i].BaseSpecies))
					{
						// This subform is missing its base form; this is a failure.
						HintWindow.SetText("Cannot copy a form without its base form in complex selections.");
						return;
					}
				}
			}
		}
		else
		{
			// Otherwise, if we have 1 node selected or the hovering node is not part of the selection,
			// then we only copy the hovering node and its children.
			items.Add((Species) DataList.HoveringItem.Object);
			items.AddRange(DataList.HoveringItem.Children.Select(c => (Species) ((TreeNode) c).Object));
		}
		Clipboard.SetObject(items, DataType);
    }

	protected override void PasteData()
	{
		if (DataList.HoveringItem is null || !Clipboard.IsValid(DataType)) return;
		List<Species> data = Clipboard.GetObject<List<Species>>();
		// Before we paste this selection, we first need to know if it can be pasted.
		// We have enforced some rules during copying, but we need to double-check those:
		// - All forms must have their base form included in the selection, OR
		// - The selection consists only of forms (only paste-able on base forms)
		// If these conditions are not met, we show a hint stating as such.
		bool allSubforms = data.All(d => d.Form != 0);
		if (!allSubforms)
		{
			// Now we must test whether all subforms have their base forms included as well.
			for (int i = 0; i < data.Count; i++)
			{
				// We are only interested in subforms
				if (data[i].Form == 0) continue;
				if (!data.Exists(s => s.Form == 0 && s.ID == data[i].BaseSpecies))
				{
					// This subform is missing its base form; this is a failure.
					HintWindow.SetText("Cannot paste a form without its base form in complex selections.");
					return;
				}
			}
			// Paste complex selection.
			// One important check to do first is if we're hovered over a subform; if so, we cannot continue,
			// as we cannot have base forms within other subforms.
			if (HoveringItem.Form != 0)
			{
				// Attempted to paste base-forms amongst sub-forms. Throw an error hint and return.
				HintWindow.SetText("Cannot paste base forms as subforms.");
				return;
			}
			// We know there are no loose subforms; all subforms that exist, also have their base form.
			// We begin by adding all base forms. Since we would lose the reference to the base form as stated on each subform,
			// we must also change all our subform's stated base form as soon as the actual base form's ID changes.
			foreach (Species spc in data)
			{
				// Skip subforms for now
				if (spc.Form != 0) continue;
				// Ensure we have a unique ID and that we point the Base Species to itself
				string oldID = spc.ID;
				spc.ID = EnsureUniqueID(spc.ID);
				spc.BaseSpecies = (SpeciesResolver) spc;
				Data.Species.Add(spc.ID, spc);
				foreach (Species form in data)
				{
					// Skip base forms
					if (form.Form == 0) continue;
					if (form.BaseSpecies.ID == oldID)
					{
						// This form belongs to the base form we just updated; now update this form.
						// We do not need to update the form number, because this base form is unique as enforced above.
						form.BaseSpecies = (SpeciesResolver) spc.ID;
						form.ID = spc.ID + "_" + form.Form.ToString();
						Data.Species.Add(form.ID, form);
					}
				}
			}
		}
		else
		{
			// Paste all-subforms
			// No further validation is needed, all species in the paste data will become
			// subforms of the currently selected species (or if we have a form selected, that form's base species)
			Species parentSpecies = HoveringItem.Form == 0 ? HoveringItem : (Species) DataList.HoveringItem.Parent.Object;
			for (int i = 0; i < data.Count; i++)
			{
				Species s = data[i];
				s.BaseSpecies = (SpeciesResolver) parentSpecies;
				s.Form = (int) GetFreeFormNumber(parentSpecies, 0, 1);
				s.ID = s.BaseSpecies.ID + "_" + s.Form.ToString();
				Data.Species.Add(s.ID, s);
			}
		}

		InvalidateData();
		RedrawList(data[0]);
		DataList.UnlockGraphics();
		DataList.Root.GetAllChildren(true).ForEach(n =>
		{
			if (data.Contains((Species) ((TreeNode) n).Object)) DataList.SetSelectedNode((TreeNode) n, true, true, true, false);
		});
		DataList.LockGraphics();
	}

	/*void PasteSpecies(BaseEventArgs e)
    {
        if (SpeciesList.HoveringItem is null || !Clipboard.IsValid(BinaryData.SPECIES)) return;
        List<Species> data = Clipboard.GetObject<List<Species>>();
        if (data.Count == 0) return;
        Species? speciesToSelect = null;
        if (data[0].Form == 0)
        {
            // Paste in main list
            if (SpeciesList.HoveringItem.Parent != SpeciesList.Root)
            {
                // Attempted to paste a base-form amongst sub-forms. Throw an error hint and return.
                HintWindow.SetText("Cannot paste base form as a sub-form.");
                return;
            }
            for (int i = 0; i < data.Count; i++) 
            {
                Species s = data[i];
			    if (s.Form == 0)
			    {
				    s.ID = EnsureUniqueID(s.ID);
				    s.BaseSpecies = (SpeciesResolver) s;
                    for (int j = i + 1; j < data.Count; j++)
                    {
                        if (data[j].Form != 0)
                        {
                            data[j].BaseSpecies = (SpeciesResolver) s.ID;
                            data[j].ID = data[j].BaseSpecies + "_" + data[j].Form.ToString();
                        }
                        else break;
                    }
			    }
                Data.Species.Add(s.ID, s);
                speciesToSelect = s;
		    }
        }
        else
        {
            // Paste as form
            Species parentSpecies = SpeciesList.HoveringItem.Parent == SpeciesList.Root ? HoveringSpecies : (Species) SpeciesList.HoveringItem.Parent.Object;
		    for (int i = 0; i < data.Count; i++)
		    {
			    Species s = data[i];
			    s.BaseSpecies = (SpeciesResolver) parentSpecies;
                s.Form = (int) GetFreeFormNumber(parentSpecies, 0, 1);
                s.ID = s.BaseSpecies.ID + "_" + s.Form.ToString();
			    Data.Species.Add(s.ID, s);
                speciesToSelect = s;
		    }
        }
		Data.Sources.InvalidateSpecies();
        RedrawList(speciesToSelect);
	}*/

	protected override void DeleteData()
	{
        if (DataList.HoveringItem is null) return;

		List<(TreeNode Node, Species Species)> items = new List<(TreeNode, Species)>();
		// If we have more than 1 node selected, and the hovering node is part of the selection, delete the whole selection
		if (DataList.SelectedItems.Count > 1 && DataList.SelectedItems.Contains(DataList.HoveringItem)) items = DataList.SelectedItems.Select(n => (n, (Species) n.Object)).ToList();
		else
		{
			// Otherwise, if we have 1 node selected or the hovering node is not part of the selection,
			// then we only delete the hovering node.
			items.Add((DataList.HoveringItem, (Species) DataList.HoveringItem.Object));
            items.AddRange(DataList.HoveringItem.Children.Select(n => ((TreeNode) n, (Species) ((TreeNode) n).Object)));
			DataList.ClearSelection(DataList.SelectedItem);
		}

        // Before we continue and delete all selected nodes, we have to enforce one basic rule:
        // - A node can only be deleted if all its subnodes are selected
        //   To do this algorithmically, we only need to check for all base forms whether all subforms are included.
        //   Subforms by themselves can always be deleted without any problem.
        // There is one exception; if only a single node is to be deleted, and that node is the base form, we will
        // automatically include all its subforms, as that is likely not to be a mistake. This is handled above.
        // If this condition is not met, we exit out of the procedure and show the user a hint.

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Species.Form != 0) continue;
            // Check whether all child nodes of this species are also included
            if (!items[i].Node.Children.All(n => items.Exists(d => d.Node == n)))
            {
                HintWindow.SetText("Cannot delete a species without\ndeleting all its alternate forms.");
                return;
            }
        }

        bool selectFirst = false;
        // If the selected item will also be deleted, we need to change our selection.
		if (items.Any(e => e.Item2.Equals((Species) DataList.SelectedItem.Object)))
		{
            TreeNode topMostNode = items.MinBy(n => n.Node.GlobalIndex).Node;
			TreeNode prevNode = topMostNode.GetPreviousNode(false);
			if (prevNode == DataList.Root) selectFirst = true;
			else DataList.SetSelectedNode(prevNode);
		}

        foreach (var dat in items)
        {
            Data.Species.Remove(dat.Species.ID);
            dat.Node.Delete(false);
        }

        if (selectFirst) DataList.SetSelectedNode((TreeNode) DataList.Root.Children[0]);
        DataList.RedrawAllNodes();
        Data.Sources.InvalidateSpecies();
    }
}
