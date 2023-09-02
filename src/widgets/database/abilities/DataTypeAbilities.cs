using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeAbilities : DataTypeBase
{
    public DataTypeSubTree AbilitiesList;
    public Ability? Ability => (Ability) AbilitiesList.SelectedItem?.Object;
    public Ability? HoveringAbility => (Ability) AbilitiesList.HoveringItem?.Object;

    Grid Grid;
    VignetteFade Fade;
    Container MainContainer;
    Container ScrollContainer;
    VStackPanel StackPanel;

    HintWindow HintWindow;

    public DataTypeAbilities(IContainer Parent) : base(Parent)
    {
        Grid = new Grid(this);
        Grid.SetColumns(
            new GridSize(201, Unit.Pixels),
            new GridSize(1),
            new GridSize(0, Unit.Pixels)
        );

        Fade = new VignetteFade(Grid);
        Fade.SetGridColumn(1);
        Fade.SetZIndex(2);

        AbilitiesList = new DataTypeSubTree("Abilities", Grid);
        AbilitiesList.SetBackgroundColor(28, 50, 73);
        AbilitiesList.OnScrolling += _ => Editor.ProjectSettings.LastAbilityScroll = AbilitiesList.GetScroll();
    }

	public override void Initialize()
	{
	    MainContainer = new Container(Grid);
		MainContainer.SetBackgroundColor(23, 40, 56);
        MainContainer.SetGridColumn(1);

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

        AbilitiesList.OnSelectionChanged += _ => UpdateSelection();

        AbilitiesList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New")
            {
                IsClickable = e => e.Value = AbilitiesList.HoveringItem is not null,
                OnClicked = NewAbility
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = AbilitiesList.HoveringItem is not null,
                OnClicked = CutAbility
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = AbilitiesList.HoveringItem is not null,
                OnClicked = CopyAbility
            },
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = AbilitiesList.HoveringItem is not null && Clipboard.IsValid(BinaryData.ABILITY),
                OnClicked = PasteAbility
            },
			new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = AbilitiesList.HoveringItem is not null,
                OnClicked = DeleteAbility
            }
        });
        Grid.UpdateLayout();

        RedrawList(Editor.ProjectSettings.LastAbilityID);
    }

	public void RedrawList(Ability? abilityToSelect = null)
	{
		List<TreeNode> AbilityItems = new List<TreeNode>();
        TreeNode? nodeToSelect = null;
		foreach (ListItem listItem in Data.Sources.Abilities)
		{
            Ability abil = (Ability) listItem.Object;
			TreeNode item = new TreeNode(abil.Name, abil);
            if (abil == abilityToSelect) nodeToSelect = item;
			AbilityItems.Add(item);
		}
		AbilitiesList.SetItems(AbilityItems);
        if (nodeToSelect != null)
        {
            AbilitiesList.SetScroll(Editor.ProjectSettings.LastAbilityScroll);
			AbilitiesList.SetSelectedNode(nodeToSelect);
		}
	}

    public void RedrawList(string abilityToSelect)
    {
        Ability abil = (Ability) Data.Sources.Abilities.Find(m => ((Ability) m.Object).ID == abilityToSelect)?.Object;
        if (abilityToSelect == null) abil = (Ability) Data.Sources.Abilities[0].Object;
        RedrawList(abil);
    }

    public void SelectAbility(Ability abil)
    {
        ITreeNode node = AbilitiesList.Root.GetAllChildren(true).Find(n => (Ability) ((TreeNode) n).Object == abil);
        AbilitiesList.SetSelectedNode((TreeNode) node);
    }

    void UpdateSelection()
    {
        Editor.ProjectSettings.LastAbilityID = this.Ability.ID;

        StackPanel?.Dispose();
        
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(1000);
        StackPanel.OnSizeChanged += _ => ScrollContainer.SetSize(StackPanel.Size.Width, StackPanel.Size.Height + 200);
        StackPanel.OnChildBoundsChanged += _ => StackPanel.UpdateLayout();

        DataContainer mainContainer = new DataContainer(StackPanel);
        mainContainer.SetText("Main");
        CreateMainContainer(mainContainer, this.Ability);
        mainContainer.SetID("ABILITY_MAIN");

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

    void NewAbility(BaseEventArgs e)
    {
        Ability ability = Game.Ability.Create();
        ability.Name = "Missingno.";
        ability.ID = EnsureUniqueID("MISSINGNO");
        Data.Abilities.Add(ability.ID, ability);
        Data.Sources.InvalidateAbilities();
        RedrawList(ability);
    }

    void CutAbility(BaseEventArgs e)
    {
        if (AbilitiesList.HoveringItem is null || AbilitiesList.HoveringItem.Parent != AbilitiesList.Root) return;
        CopyAbility(e);
        DeleteAbility(e);
    }

    void CopyAbility(BaseEventArgs e)
	{
		if (AbilitiesList.HoveringItem is null) return;
        Ability ability = HoveringAbility;
        Clipboard.SetObject(ability, BinaryData.ABILITY);
    }

    string EnsureUniqueID(string id)
    {
        int i = 0;
        string query = id;
        while (Data.Abilities.ContainsKey(query))
        {
            i++;
            query = id + i.ToString();
        }
        return query;
    }

    void PasteAbility(BaseEventArgs e)
    {
        if (AbilitiesList.HoveringItem is null || !Clipboard.IsValid(BinaryData.ABILITY)) return;
        Ability data = Clipboard.GetObject<Ability>();
        data.ID = EnsureUniqueID(data.ID);
		Data.Abilities.Add(data.ID, data);
        Data.Sources.InvalidateAbilities();
        RedrawList(data);
	}

	void DeleteAbility(BaseEventArgs e)
    {
        if (AbilitiesList.HoveringItem is null) return;
        Ability ability = (Ability) AbilitiesList.HoveringItem.Object;
        bool selectFirst = false;
        if (AbilitiesList.SelectedItem == AbilitiesList.HoveringItem)
        {
            TreeNode prevNode = AbilitiesList.HoveringItem.GetPreviousNode(false);
            if (prevNode == AbilitiesList.Root) selectFirst = true;
            else AbilitiesList.SetSelectedNode(prevNode);
        }
        Data.Abilities.Remove(ability.ID);
        AbilitiesList.HoveringItem.Delete(true);
        if (selectFirst) AbilitiesList.SetSelectedNode((TreeNode) AbilitiesList.Root.Children[0]);
        AbilitiesList.RedrawAllNodes();
        Data.Sources.InvalidateAbilities();
    }
}
