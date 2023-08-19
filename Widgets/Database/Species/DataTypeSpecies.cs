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
            new MenuItem("New Form")
            {
                IsClickable = e => e.Value = HoveringSpecies is not null && HoveringSpecies.Form == 0
            },
            new MenuSeparator(),
            new MenuItem("Copy")
            {
                //OnClicked = CopySpecies
            },
            new MenuItem("Paste")
            {
                //OnClicked = PasteSpecies,
                //IsClickable = e => e.Value = Utilities.IsClipboardValidBinary(BinaryData.SPECIES)
            },
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                //OnClicked = DeleteSpecies
            }
        });

        SpeciesList.SetSelectedNode((TreeNode) SpeciesList.Root.Children[0]);
    }

	public void RedrawList()
	{
		List<TreeNode> SpeciesItems = new List<TreeNode>();
		foreach (KeyValuePair<string, Species> kvp in Data.Species)
		{
			if (kvp.Value.Form != 0)
            {
                TreeNode parent = SpeciesItems.Find(n => ((Species)n.Object).Name == kvp.Value.Name);
				TreeNode item = new TreeNode($"{kvp.Value.Form} - {kvp.Value.FormName ?? kvp.Value.Name}", kvp.Value);
                int idx = parent.Children.FindIndex(node => kvp.Value.Form < ((Species) ((TreeNode) node).Object).Form);
                if (idx == -1) idx = parent.Children.Count;
                parent.InsertChild(idx, item);
            }
            else
            {
				TreeNode item = new TreeNode(kvp.Value.Name, kvp.Value);
				SpeciesItems.Add(item);
            }
		}
		SpeciesList.SetItems(SpeciesItems);
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
	}

	public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
		if (ScrollContainer.Size.Width < MainContainer.Size.Width) ScrollContainer.SetPosition(MainContainer.Size.Width / 2 - ScrollContainer.Size.Width / 2, 0);
		else ScrollContainer.SetPosition(0, 0);
	}

    public void SetSpecies(Species Species, bool ForceUpdate = false)
    {
        //ListItem Item = TilesetList.Items.Find(i => i.Object == Tileset);
        //if (Item == null) throw new Exception("Could not find Tileset list item.");
        //TilesetList.SetSelectedIndex(TilesetList.Items.IndexOf(Item));
        //TilesetContainer.SetTileset(Tileset, ForceUpdate);
        //ScrollContainer.SetHeight(MainBox.Size.Height - ScrollContainer.Position.Y - 8);
        //if (TilesetContainer.Size.Height < ScrollContainer.Size.Height) ScrollContainer.SetHeight(TilesetContainer.Size.Height);
        //UpdateSelection(new BaseEventArgs());
    }

    //void CopyTileset(BaseEventArgs e)
    //{
    //    Tileset Tileset = (Tileset) TilesetList.HoveringItem.Object;
    //    Utilities.SetClipboard(Tileset, BinaryData.TILESET);
    //}

    //void PasteTileset(BaseEventArgs e)
    //{
    //    if (!Utilities.IsClipboardValidBinary(BinaryData.TILESET)) return;
    //    Tileset OldTileset = (Tileset) TilesetList.HoveringItem.Object;
    //    Tileset NewTileset = Utilities.GetClipboard<Tileset>();
    //    Data.Tilesets[OldTileset.ID] = NewTileset;
    //    NewTileset.ID = OldTileset.ID;
    //    OldTileset.TilesetBitmap?.Dispose();
    //    OldTileset.TilesetBitmap = null;
    //    OldTileset.TilesetListBitmap?.Dispose();
    //    OldTileset.TilesetListBitmap = null;
    //    for (int i = 0; i < 7; i++)
    //    {
    //        Data.Autotiles[NewTileset.ID * 7 + i] = NewTileset.Autotiles[i];
    //    }
    //    RedrawList();
    //    SetTileset(NewTileset, true);
    //    Undo.TilesetChangeUndoAction.Create(OldTileset, NewTileset);
    //}

    //void DeleteTileset(BaseEventArgs e)
    //{
    //    Tileset OldTileset = (Tileset) TilesetList.HoveringItem.Object;
    //    Tileset NewTileset = (Tileset) OldTileset.Clone();
    //    NewTileset.MakeEmpty();
    //    // Since making the tileset empty will dispose the bitmap,
    //    // we should also set the other references to null
    //    // or they will point to a disposed bitmap.
    //    OldTileset.TilesetBitmap = null;
    //    OldTileset.TilesetListBitmap = null;
    //    Data.Tilesets[NewTileset.ID] = NewTileset;
    //    RedrawList();
    //    SetTileset(NewTileset, true);
    //    Undo.TilesetChangeUndoAction.Create(OldTileset, NewTileset);
    //}
}
