using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeSpecies : DataTypeBase
{
    public DataTypeSubTree SpeciesList;
    public SubmodeView Tabs;
    public Species? Species => (Species) SpeciesList.SelectedItem?.Object;
    public Species? HoveringSpecies => (Species) SpeciesList.HoveringItem?.Object;

    Grid Grid;
    VignetteFade Fade;
    Container MainContainer;
    Container ScrollContainer;
    VStackPanel StackPanel;

    HintWindow HintWindow;

    public DataTypeSpecies(IContainer Parent) : base(Parent)
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

        SpeciesList = new DataTypeSubTree("Species", Grid);
        SpeciesList.SetBackgroundColor(28, 50, 73);
        SpeciesList.SetGridRow(0, 1);
        SpeciesList.OnScrolling += _ => Editor.ProjectSettings.LastSpeciesScroll = SpeciesList.GetScroll();

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

        SpeciesList.OnSelectionChanged += _ => UpdateSelection();

        SpeciesList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New Species")
            {
                IsClickable = e => e.Value = HoveringSpecies is not null && HoveringSpecies.Form == 0,
                OnClicked = NewSpecies
            },
            new MenuItem("New Form")
            {
                IsClickable = e => e.Value = SpeciesList.HoveringItem is not null,
                OnClicked = NewForm
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = SpeciesList.HoveringItem is not null,
                OnClicked = CutSpecies
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = SpeciesList.HoveringItem is not null,
                OnClicked = CopySpecies
            },
            new MenuItem("Paste (smart)")
            {
                IsClickable = e => e.Value = SpeciesList.HoveringItem is not null && Utilities.IsClipboardValidBinary(BinaryData.SPECIES),
                OnClicked = PasteSpecies
            },
			new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = SpeciesList.HoveringItem is not null,
                OnClicked = DeleteSpecies
            }
        });
        Grid.UpdateLayout();

        RedrawList(Editor.ProjectSettings.LastSpeciesID);
    }

	public void RedrawList(Species? speciesToSelect = null)
	{
		List<TreeNode> SpeciesItems = new List<TreeNode>();
        TreeNode? nodeToSelect = null;
		foreach (ListItem listItem in Data.Sources.SpeciesAndForms)
		{
            Species spc = (Species) listItem.Object;
			if (spc.Form != 0)
            {
                TreeNode parent = SpeciesItems.Find(n => ((Species) n.Object).ID == spc.BaseSpecies.ID);
				TreeNode item = new TreeNode($"{spc.Form} - {spc.FormName ?? spc.Name}", spc);
                int idx = parent.Children.FindIndex(node => spc.Form < ((Species) ((TreeNode) node).Object).Form);
                if (idx == -1) idx = parent.Children.Count;
                parent.InsertChild(idx, item);
                if (speciesToSelect == spc) nodeToSelect = item;
                if (Editor.ProjectSettings.HiddenSpeciesForms.Contains(spc.BaseSpecies.ID)) parent.Collapse();
            }
            else
            {
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
		}
		SpeciesList.SetItems(SpeciesItems);
        if (nodeToSelect != null)
        {
            SpeciesList.SetScroll(Editor.ProjectSettings.LastSpeciesScroll);
            SpeciesList.SetSelectedNode(nodeToSelect);
        }
	}

    public void SelectSpecies(Species spc)
    {
        ITreeNode node = SpeciesList.Root.GetAllChildren(true).Find(n => (Species) ((TreeNode) n).Object == spc);
        SpeciesList.SetSelectedNode((TreeNode) node);
    }

    public void RedrawList(string speciesToSelect)
    {
        Species spc = (Species) Data.Sources.SpeciesAndForms.Find(spc => ((Species) spc.Object).ID == speciesToSelect)?.Object;
        if (spc == null) spc = (Species) Data.Sources.Species[0].Object;
        RedrawList(spc);
    }

    void UpdateSelection()
    {
		Editor.ProjectSettings.LastSpeciesSubmode = Tabs.SelectedIndex;
        Editor.ProjectSettings.LastSpeciesID = this.Species.ID;
		StackPanel?.Dispose();
        
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(1000);
        StackPanel.OnSizeChanged += _ => ScrollContainer.SetSize(StackPanel.Size.Width, StackPanel.Size.Height + 200);
        StackPanel.OnChildBoundsChanged += _ => StackPanel.UpdateLayout();

        if (Tabs.SelectedIndex == 0) // Main
        {
            DataContainer mainContainer = new DataContainer(StackPanel);
            mainContainer.SetText("Main");
            CreateMainContainer(mainContainer, this.Species);
            mainContainer.SetID("SPECIES_MAIN");

            if (this.Species.Form != 0)
            {
                DataContainer formContainer = new DataContainer(StackPanel);
                formContainer.SetText("Form");
                CreateFormContainer(formContainer, this.Species);
                formContainer.SetID("SPECIES_FORM");
            }

            DataContainer statsContainer = new DataContainer(StackPanel);
            statsContainer.SetText("Stats");
            CreateStatsContainer(statsContainer, this.Species);
            statsContainer.SetID("SPECIES_STATS");

            DataContainer evContainer = new DataContainer(StackPanel);
            evContainer.SetText("Effort Points");
            CreateEVContainer(evContainer, this.Species);
            evContainer.SetID("SPECIES_EVS");

            DataContainer miscContainer = new DataContainer(StackPanel);
            miscContainer.SetText("Misc");
            CreateMiscContainer(miscContainer, this.Species);
            miscContainer.SetID("SPECIES_MISC");

			DataContainer dexInfoContainer = new DataContainer(StackPanel);
			dexInfoContainer.SetText("Dex Info");
			CreateDexInfoContainer(dexInfoContainer, this.Species);
            dexInfoContainer.SetID("SPECIES_DEXINFO");

			DataContainer heldItemsContainer = new DataContainer(StackPanel);
            heldItemsContainer.SetText("Wild Held Items");
            CreateWildItemsContainer(heldItemsContainer, this.Species);
            heldItemsContainer.SetID("SPECIES_HELDITEMS");
        }
        else if (Tabs.SelectedIndex == 1) // Moves
        {
            DataContainer levelupContainer = new DataContainer(StackPanel);
            levelupContainer.SetText("Level-Up Moves");
            CreateLevelContainer(levelupContainer, this.Species);
            levelupContainer.SetID("SPECIES_LEVELUP");

            DataContainer evoMovesContainer = new DataContainer(StackPanel);
            evoMovesContainer.SetText("Evolution Moves");
            CreateEvoMovesContainer(evoMovesContainer, this.Species);
            evoMovesContainer.SetID("SPECIES_EVOMOVES");

            DataContainer tmContainer = new DataContainer(StackPanel);
            tmContainer.SetText("TMs & HMs");
            CreateTMContainer(tmContainer, this.Species);
            tmContainer.SetID("SPECIES_TMS");

            DataContainer eggMovesContainer = new DataContainer(StackPanel);
            eggMovesContainer.SetText("Egg Moves");
            CreateEggMovesContainer(eggMovesContainer, this.Species);
            eggMovesContainer.SetID("SPECIES_EGGMOVES");

            DataContainer tutorMovesContainer = new DataContainer(StackPanel);
            tutorMovesContainer.SetText("Tutor Moves");
            CreateTutorMovesContainer(tutorMovesContainer, this.Species);
            tutorMovesContainer.SetID("SPECIES_TUTOR");
        }
        else if (Tabs.SelectedIndex == 2) // Evolutions
        {
            DataContainer evoContainer = new DataContainer(StackPanel);
            evoContainer.SetText("Evolves Into");
            CreateEvoContainer(evoContainer, this.Species);
            evoContainer.SetID("SPECIES_EVOLUTION");

            DataContainer prevoContainer = new DataContainer(StackPanel);
            prevoContainer.SetText("Evolves From");
            CreatePrevoContainer(prevoContainer, this.Species);
            prevoContainer.SetID("SPECIES_PREVO");
        }
        else if (Tabs.SelectedIndex == 3) // Media
        {
            DataContainer spritesContainer = new DataContainer(StackPanel);
            spritesContainer.SetText("Sprites");
            CreateSpritesContainer(spritesContainer, this.Species);
            spritesContainer.SetID("SPECIES_SPRITES");

            DataContainer audioContainer = new DataContainer(StackPanel);
            audioContainer.SetText("Audio");
            CreateAudioContainer(audioContainer, this.Species);
            audioContainer.SetID("SPECIES_AUDIO");
        }

        if (ScrollContainer.Size.Width < MainContainer.Size.Width) ScrollContainer.SetPosition(MainContainer.Size.Width / 2 - ScrollContainer.Size.Width / 2, 0);
        else ScrollContainer.SetPosition(0, 0);
        StackPanel.Update();
	}

	public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (ScrollContainer is null) return;
		if (ScrollContainer.Size.Width < MainContainer.Size.Width) ScrollContainer.SetPosition(MainContainer.Size.Width / 2 - ScrollContainer.Size.Width / 2, 0);
		else ScrollContainer.SetPosition(0, 0);
	}

    void NewSpecies(BaseEventArgs e)
    {
        Species species = Species.Create();
        species.Name = "Missingno.";
        species.ID = EnsureUniqueID("MISSINGNO");
        species.BaseSpecies = (SpeciesResolver) species.ID;
        Data.Species.Add(species.ID, species);
        RedrawList();
        TreeNode newNode = (TreeNode) SpeciesList.Root.GetAllChildren(true).Find(n => (Species) ((TreeNode) n).Object == species);
        SpeciesList.SetSelectedNode(newNode);
    }

    void NewForm(BaseEventArgs e)
    {
        Species parentSpecies = SpeciesList.HoveringItem.Parent == SpeciesList.Root ? HoveringSpecies : (Species) SpeciesList.HoveringItem.Parent.Object;
        Species newForm = (Species) parentSpecies.Clone();
        newForm.BaseSpecies = (SpeciesResolver) parentSpecies.ID;
        newForm.Form = (int) GetFreeFormNumber(newForm.BaseSpecies, 0, 1);
        newForm.ID = newForm.BaseSpecies.ID + "_" + newForm.Form.ToString();
        Data.Species.Add(newForm.ID, newForm);
        RedrawList();
        TreeNode newNode = (TreeNode) SpeciesList.Root.GetAllChildren(true).Find(n => (Species) ((TreeNode) n).Object == newForm);
        SpeciesList.SetSelectedNode(newNode);
    }

    void CutSpecies(BaseEventArgs e)
    {
        if (SpeciesList.HoveringItem is null || SpeciesList.HoveringItem.Parent != SpeciesList.Root) return;
        CopySpecies(e);
        DeleteSpecies(e);
    }

    void CopySpecies(BaseEventArgs e)
	{
		if (SpeciesList.HoveringItem is null) return;
		List<Species> Species = new List<Species>();
        Species.Add((Species) SpeciesList.HoveringItem.Object);
        Species.AddRange(SpeciesList.HoveringItem.Children.Select(c => (Species) ((TreeNode) c).Object));
        Utilities.SetClipboard(Species, BinaryData.SPECIES);
    }

    string EnsureUniqueID(string id)
    {
        int i = 0;
        string query = id;
        while (Data.Species.ContainsKey(query))
        {
            i++;
            query = id + i.ToString();
        }
        return query;
    }

    void PasteSpecies(BaseEventArgs e)
    {
        if (SpeciesList.HoveringItem is null || !Utilities.IsClipboardValidBinary(BinaryData.SPECIES)) return;
        List<Species> data = Utilities.GetClipboard<List<Species>>();
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
	}

	void DeleteSpecies(BaseEventArgs e)
    {
        if (SpeciesList.HoveringItem is null) return;
        Species Species = (Species) SpeciesList.HoveringItem.Object;
		for (int i = SpeciesList.HoveringItem.Children.Count - 1; i >= 0; i--) 
        {
            TreeNode c = (TreeNode) SpeciesList.HoveringItem.Children[i];
            Species form = (Species) c.Object;
            Data.Species.Remove(form.ID);
            if (SpeciesList.SelectedItem == c) SpeciesList.SetSelectedNode(c.GetPreviousNode(false));
		}
        bool selectFirst = false;
        if (SpeciesList.SelectedItem == SpeciesList.HoveringItem)
        {
            TreeNode prevNode = SpeciesList.HoveringItem.GetPreviousNode(false);
            if (prevNode == SpeciesList.Root) selectFirst = true;
            else SpeciesList.SetSelectedNode(prevNode);
        }
        Data.Species.Remove(Species.ID);
        SpeciesList.HoveringItem.Delete(true);
        if (selectFirst) SpeciesList.SetSelectedNode((TreeNode) SpeciesList.Root.Children[0]);
        SpeciesList.RedrawAllNodes();
        Data.Sources.InvalidateSpecies();
    }
}
