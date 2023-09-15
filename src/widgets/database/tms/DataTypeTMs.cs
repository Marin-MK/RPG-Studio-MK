using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeTMs : DataTypeBase
{
    public DataTypeSubTree TMList;
    public Item? TM => (Item) TMList.SelectedItem?.Object;
    public Item? HoveringTM => (Item) TMList.HoveringItem?.Object;

    Grid Grid;
    VignetteFade Fade;
    Container MainContainer;
    Container ScrollContainer;
    VStackPanel StackPanel;

    HintWindow HintWindow;

    public DataTypeTMs(IContainer Parent) : base(Parent)
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

        TMList = new DataTypeSubTree("TMs & HMs", Grid);
        TMList.SetBackgroundColor(28, 50, 73);
        TMList.OnScrolling += _ => Editor.ProjectSettings.LastAbilityScroll = TMList.GetScroll();
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

        TMList.OnSelectionChanged += _ => UpdateSelection();

        TMList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New")
            {
                IsClickable = e => e.Value = TMList.HoveringItem is not null,
                OnClicked = NewTM
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = TMList.HoveringItem is not null,
                OnClicked = CutTM
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = TMList.HoveringItem is not null,
                OnClicked = CopyTM
            },
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = TMList.HoveringItem is not null && Clipboard.IsValid(BinaryData.TM),
                OnClicked = PasteTM
            },
			new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = TMList.HoveringItem is not null,
                OnClicked = DeleteTM
            }
        });
        Grid.UpdateLayout();

        RedrawList(Editor.ProjectSettings.LastTMID);
    }

	public void RedrawList(Item? tmToSelect = null)
	{
		List<TreeNode> TMItems = new List<TreeNode>();
        TreeNode? nodeToSelect = null;
		foreach (TreeNode listItem in Data.Sources.TMsHMs)
		{
            if ((Item) listItem.Object == tmToSelect) nodeToSelect = listItem;
			TMItems.Add(listItem);
		}
		TMList.SetItems(TMItems);
        if (nodeToSelect != null)
        {
            TMList.SetScroll(Editor.ProjectSettings.LastTMScroll);
			TMList.SetSelectedNode(nodeToSelect);
		}
	}

    public void RedrawList(string tmToSelect)
    {
        Item tm = (Item) Data.Sources.TMsHMs.Find(m => ((Item) m.Object).ID == tmToSelect)?.Object;
        if (tmToSelect == null) tm = (Item) Data.Sources.TMsHMs[0].Object;
        RedrawList(tm);
    }

    public void SelectAbility(Item tm)
    {
        ITreeNode node = TMList.Root.GetAllChildren(true).Find(n => (Item) ((TreeNode) n).Object == tm);
        TMList.SetSelectedNode((TreeNode) node);
    }

    void UpdateSelection()
    {
        Editor.ProjectSettings.LastTMID = this.TM.ID;

        StackPanel?.Dispose();
        
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(1000);
        StackPanel.OnSizeChanged += _ => ScrollContainer.SetSize(StackPanel.Size.Width, StackPanel.Size.Height + 200);
        StackPanel.OnChildBoundsChanged += _ => StackPanel.UpdateLayout();

        DataContainer mainContainer = new DataContainer(StackPanel);
        mainContainer.SetText("Main");
        CreateMainContainer(mainContainer, this.TM);
        mainContainer.SetID("TM_MAIN");

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

    void NewTM(BaseEventArgs e)
    {
        Item item = Game.Item.CreateTM();
        foreach (TreeNode listItem in Data.Sources.Moves)
        {
            Move move = (Move) listItem.Object;
            if (Data.TMsHMs.Any(tm => tm.Value.Move.ID == move.ID)) continue;
            item.Move = (MoveResolver) move;
            break;
        }
        item.ID = "TM" + Utilities.Digits((int) GetFreeMachineNumber("TM", 0, 1), 2);
        item.Name = item.ID;
        item.Plural = item.Name + "s";
        Data.TMsHMs.Add(item.ID, item);
        Data.Sources.InvalidateTMs();
        RedrawList(item);
    }

    void CutTM(BaseEventArgs e)
    {
        if (TMList.HoveringItem is null || TMList.HoveringItem.Parent != TMList.Root) return;
        CopyTM(e);
        DeleteTM(e);
    }

    void CopyTM(BaseEventArgs e)
	{
		if (TMList.HoveringItem is null) return;
        Item tm = HoveringTM;
        Clipboard.SetObject(tm, BinaryData.TM);
    }

    string EnsureUniqueID(string id)
    {
        int i = 1;
        string query = id;
        while (Data.TMsHMs.ContainsKey(query))
        {
            i++;
            query = id + i.ToString();
        }
        return query;
    }

    void PasteTM(BaseEventArgs e)
    {
        if (TMList.HoveringItem is null || !Clipboard.IsValid(BinaryData.TM)) return;
        Item data = Clipboard.GetObject<Item>();
        Match m = Regex.Match(data.ID, @"(TM|TR|HM)(\d+)");
        if (m.Success) data.ID = m.Groups[1].Value + Utilities.Digits((int) GetFreeMachineNumber(m.Groups[1].Value, Convert.ToInt32(m.Groups[2].Value) - 1, 1), 2);
        else data.ID = "TM" + Utilities.Digits((int) GetFreeMachineNumber("TM", 0, 1), 2);
        data.Name = data.ID;
        data.Plural = data.Name + "s";
		Data.TMsHMs.Add(data.ID, data);
        Data.Sources.InvalidateTMs();
        RedrawList(data);
	}

	void DeleteTM(BaseEventArgs e)
    {
        if (TMList.HoveringItem is null) return;
        Item tm = (Item) TMList.HoveringItem.Object;
        bool selectFirst = false;
        if (TMList.SelectedItem == TMList.HoveringItem)
        {
            TreeNode prevNode = TMList.HoveringItem.GetPreviousNode(false);
            if (prevNode == TMList.Root) selectFirst = true;
            else TMList.SetSelectedNode(prevNode);
        }
        Data.TMsHMs.Remove(tm.ID);
        TMList.HoveringItem.Delete(true);
        if (selectFirst) TMList.SetSelectedNode((TreeNode) TMList.Root.Children[0]);
        TMList.RedrawAllNodes();
        Data.Sources.InvalidateTMs();
    }
}
