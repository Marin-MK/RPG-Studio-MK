using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeSpecies : Widget
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
        RedrawList();

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
        Tabs.OnSelectionChanged += _ => UpdateSelection();

        Tabs.CreateTab("Main");
        Tabs.CreateTab("Moves");
        Tabs.CreateTab("Evolutions");
        Tabs.CreateTab("Media");

        MainContainer = new Container(Grid);
        MainContainer.SetGrid(1, 1);

		HintWindow = new HintWindow(MainContainer);
		HintWindow.ConsiderInAutoScrollCalculation = HintWindow.ConsiderInAutoScrollPositioningX = HintWindow.ConsiderInAutoScrollPositioningY = false;
		HintWindow.SetBottomDocked(true);
		HintWindow.SetPadding(-3, 0, 0, -9);
		HintWindow.SetZIndex(10);
		HintWindow.SetVisible(false);

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

        ScrollContainer = new Container(MainContainer);

        SpeciesList.OnSelectionChanged += _ => UpdateSelection();
        Tabs.SelectTab(0);

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

        SpeciesList.SetSelectedNode((TreeNode) SpeciesList.Root.Children[0]);
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
            }
            else
            {
				TreeNode item = new TreeNode(spc.Name, spc);
                if (speciesToSelect == spc) nodeToSelect = item;
				SpeciesItems.Add(item);
            }
		}
		SpeciesList.SetItems(SpeciesItems);
        if (nodeToSelect != null)
        {
            SpeciesList.SetSelectedNode(nodeToSelect);
            SpeciesList.CenterOnSelectedNode();
        }
	}

    void UpdateSelection()
    {
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

            if (this.Species.Form != 0)
            {
                DataContainer formContainer = new DataContainer(StackPanel);
                formContainer.SetText("Form");
                CreateFormContainer(formContainer, this.Species);
            }

            DataContainer statsContainer = new DataContainer(StackPanel);
            statsContainer.SetText("Stats");
            CreateStatsContainer(statsContainer, this.Species);

            DataContainer evContainer = new DataContainer(StackPanel);
            evContainer.SetText("Effort Points");
            CreateEVContainer(evContainer, this.Species);

            DataContainer miscContainer = new DataContainer(StackPanel);
            miscContainer.SetText("Misc");
            CreateMiscContainer(miscContainer, this.Species);

			DataContainer dexInfoContainer = new DataContainer(StackPanel);
			dexInfoContainer.SetText("Dex Info");
			CreateDexInfoContainer(dexInfoContainer, this.Species);

			DataContainer heldItemsContainer = new DataContainer(StackPanel);
            heldItemsContainer.SetText("Wild Held Items");
            CreateWildItemsContainer(heldItemsContainer, this.Species);
        }
        else if (Tabs.SelectedIndex == 1) // Moves
        {
            DataContainer levelupContainer = new DataContainer(StackPanel);
            levelupContainer.SetText("Level-Up Moves");
            CreateLevelContainer(levelupContainer, this.Species);

            DataContainer evoMovesContainer = new DataContainer(StackPanel);
            evoMovesContainer.SetText("Evolution Moves");
            CreateEvoMovesContainer(evoMovesContainer, this.Species);

            DataContainer tmContainer = new DataContainer(StackPanel);
            tmContainer.SetText("TMs & HMs");
            CreateTMContainer(tmContainer, this.Species);

            DataContainer eggMovesContainer = new DataContainer(StackPanel);
            eggMovesContainer.SetText("Egg Moves");
            CreateEggMovesContainer(eggMovesContainer, this.Species);

            DataContainer tutorMovesContainer = new DataContainer(StackPanel);
            tutorMovesContainer.SetText("Tutor Moves");
            CreateTutorMovesContainer(tutorMovesContainer, this.Species);
        }
        else if (Tabs.SelectedIndex == 2) // Evolutions
        {
            DataContainer evoContainer = new DataContainer(StackPanel);
            evoContainer.SetText("Evolves Into");
            CreateEvoContainer(evoContainer, this.Species);

            DataContainer prevoContainer = new DataContainer(StackPanel);
            prevoContainer.SetText("Evolves From");
            CreatePrevoContainer(prevoContainer, this.Species);
        }
        else if (Tabs.SelectedIndex == 3) // Media
        {
            DataContainer spritesContainer = new DataContainer(StackPanel);
            spritesContainer.SetText("Sprites");
            CreateSpritesContainer(spritesContainer, this.Species);

            DataContainer audioContainer = new DataContainer(StackPanel);
            audioContainer.SetText("Audio");
            CreateAudioContainer(audioContainer, this.Species);
        }

        if (ScrollContainer.Size.Width < MainContainer.Size.Width) ScrollContainer.SetPosition(MainContainer.Size.Width / 2 - ScrollContainer.Size.Width / 2, 0);
        else ScrollContainer.SetPosition(0, 0);
        StackPanel.Update();
	}

	public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
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
        SpeciesList.SetSelectedNode(newNode, true);
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
        SpeciesList.SetSelectedNode(newNode, true);
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
		    }
        }
        RedrawList();
        TreeNode node = (TreeNode) SpeciesList.Root.GetAllChildren(true).Find(n => (Species) ((TreeNode) n).Object == data[0]);
        SpeciesList.SetSelectedNode(node, true);
		Data.Sources.InvalidateSpecies();
		//Undo.TilesetChangeUndoAction.Create(OldTileset, NewTileset);
	}

	void PasteForm(BaseEventArgs e)
	{
		if (SpeciesList.HoveringItem is null || SpeciesList.HoveringItem.Parent == SpeciesList.Root || !Utilities.IsClipboardValidBinary(BinaryData.SPECIES)) return;
		List<Species> data = Utilities.GetClipboard<List<Species>>();
		if (data.Count == 0) return;
        if (data.Any(f => f.Form == 0)) return;
        
		RedrawList();
		TreeNode node = (TreeNode) SpeciesList.Root.GetAllChildren(true).Find(n => (Species) ((TreeNode) n).Object == data[0]);
		SpeciesList.SetSelectedNode(node, true);
		Data.Sources.InvalidateSpecies();
		//Undo.TilesetChangeUndoAction.Create(OldTileset, NewTileset);
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
        //Undo.TilesetChangeUndoAction.Create(OldTileset, NewTileset);
    }
}
