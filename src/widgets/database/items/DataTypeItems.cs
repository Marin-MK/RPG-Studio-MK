using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeItems : DataTypeBase
{
    public DataTypeSubTree ItemList;
    public Item? Item => (Item) ItemList.SelectedItem?.Object;
    public Item? HoveringItem => (Item) ItemList.HoveringItem?.Object;

    Grid Grid;
    VignetteFade Fade;
    Container MainContainer;
    Container ScrollContainer;
    VStackPanel StackPanel;

    HintWindow HintWindow;

    public DataTypeItems(IContainer Parent) : base(Parent)
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

        ItemList = new DataTypeSubTree("Items", Grid);
        ItemList.SetBackgroundColor(28, 50, 73);
        ItemList.OnScrolling += _ => Editor.ProjectSettings.LastItemScroll = ItemList.GetScroll();
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

        ItemList.OnSelectionChanged += _ => UpdateSelection();

        ItemList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New")
            {
                IsClickable = e => e.Value = ItemList.HoveringItem is not null,
                OnClicked = NewItem
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = ItemList.HoveringItem is not null,
                OnClicked = CutItem
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = ItemList.HoveringItem is not null,
                OnClicked = CopyItem
            },
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = ItemList.HoveringItem is not null && Clipboard.IsValid(BinaryData.ITEMS),
                OnClicked = PasteItem
            },
			new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = ItemList.HoveringItem is not null,
                OnClicked = DeleteItem
            }
        });
        Grid.UpdateLayout();

        RedrawList(Editor.ProjectSettings.LastItemID);
    }

	public void RedrawList(Item? itemToSelect = null)
	{
		List<TreeNode> ItemItems = new List<TreeNode>();
        TreeNode? nodeToSelect = null;
		foreach (TreeNode listItem in Data.Sources.Items)
		{
            if ((Item) listItem.Object == itemToSelect) nodeToSelect = listItem;
			ItemItems.Add(listItem);
		}
		ItemList.SetItems(ItemItems);
        if (nodeToSelect != null)
        {
            ItemList.SetScroll(Editor.ProjectSettings.LastItemScroll);
			ItemList.SetSelectedNode(nodeToSelect);
		}
	}

    public void RedrawList(string itemToSelect)
    {
        Item itm = (Item) Data.Sources.Items.Find(m => ((Item) m.Object).ID == itemToSelect)?.Object;
        if (itemToSelect == null) itm = (Item) Data.Sources.Items[0].Object;
        RedrawList(itm);
    }

    public void SelectItem(Item itm)
    {
        ITreeNode node = ItemList.Root.GetAllChildren(true).Find(n => (Item) ((TreeNode) n).Object == itm);
        ItemList.SetSelectedNode((TreeNode) node);
    }

    void UpdateSelection()
    {
        Editor.ProjectSettings.LastItemID = this.Item.ID;

        StackPanel?.Dispose();
        
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(1000);
        StackPanel.OnSizeChanged += _ => ScrollContainer.SetSize(StackPanel.Size.Width, StackPanel.Size.Height + 200);
        StackPanel.OnChildBoundsChanged += _ => StackPanel.UpdateLayout();

        DataContainer mainContainer = new DataContainer(StackPanel);
        mainContainer.SetText("Main");
        CreateMainContainer(mainContainer, this.Item);
        mainContainer.SetID("ITEM_MAIN");

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

    void NewItem(BaseEventArgs e)
    {
        Item item = Game.Item.Create();
        item.Name = "Missingno.";
        item.ID = EnsureUniqueID("MISSINGNO");
        Data.Items.Add(item.ID, item);
        Data.Sources.InvalidateItems();
        RedrawList(item);
    }

    void CutItem(BaseEventArgs e)
    {
        if (ItemList.HoveringItem is null || ItemList.HoveringItem.Parent != ItemList.Root) return;
        CopyItem(e);
        DeleteItem(e);
    }

    void CopyItem(BaseEventArgs e)
	{
		if (ItemList.HoveringItem is null) return;
		List<Item> items = new List<Item>();
		// If we have more than 1 node selected, and the hovering node is part of the selection, delete the whole selection
		if (ItemList.SelectedItems.Count > 1 && ItemList.SelectedItems.Contains(ItemList.HoveringItem)) items = ItemList.SelectedItems.Select(n => (Item) n.Object).ToList();
		else
		{
			// Otherwise, if we have 1 node selected or the hovering node is not part of the selection,
			// then we only delete the hovering node.
			items.Add((Item) ItemList.HoveringItem.Object);
		}
        Clipboard.SetObject(items, BinaryData.ITEMS);
    }

    string EnsureUniqueID(string id)
    {
        int i = 0;
        string query = id;
        while (Data.Items.ContainsKey(query))
        {
            i++;
            query = id + i.ToString();
        }
        return query;
    }

    void PasteItem(BaseEventArgs e)
    {
        if (ItemList.HoveringItem is null || !Clipboard.IsValid(BinaryData.ITEMS)) return;
        List<Item> data = Clipboard.GetObject<List<Item>>();
        foreach (Item itm in data)
        {
            itm.ID = EnsureUniqueID(itm.ID);
		    Data.Items.Add(itm.ID, itm);
        }
        Data.Sources.InvalidateItems();
        RedrawList(data[0]);
        ItemList.UnlockGraphics();
        ItemList.Root.GetAllChildren(true).ForEach(n =>
        {
            if (data.Contains((Item) ((TreeNode) n).Object)) ItemList.SetSelectedNode((TreeNode) n, true, true, true, false);
        });
        ItemList.LockGraphics();
	}

	void DeleteItem(BaseEventArgs e)
    {
        if (ItemList.HoveringItem is null) return;
        List<(TreeNode, Item)> items = new List<(TreeNode, Item)>();
        // If we have more than 1 node selected, and the hovering node is part of the selection, delete the whole selection
        if (ItemList.SelectedItems.Count > 1 && ItemList.SelectedItems.Contains(ItemList.HoveringItem)) items = ItemList.SelectedItems.Select(n => (n, (Item) n.Object)).ToList();
        else
        {
            // Otherwise, if we have 1 node selected or the hovering node is not part of the selection,
            // then we only delete the hovering node.
            items.Add((ItemList.HoveringItem, (Item) ItemList.HoveringItem.Object));
            ItemList.ClearSelection(ItemList.SelectedItem);
        }
        bool selectFirst = false;
        // If the selected item will also be deleted, we need to change our selection.
        if (items.Any(e => e.Item2 == (Item) ItemList.SelectedItem.Object))
        {
            TreeNode prevNode = ItemList.SelectedItems[0].GetPreviousNode(false);
            if (prevNode == ItemList.Root) selectFirst = true;
            else ItemList.SetSelectedNode(prevNode);
        }
        foreach ((TreeNode node, Item itm) in items)
        {
            Data.Items.Remove(itm.ID);
            node.Delete(true);
        }
        if (selectFirst) ItemList.SetSelectedNode((TreeNode) ItemList.Root.Children[0]);
        ItemList.RedrawAllNodes();
        Data.Sources.InvalidateItems();
    }
}
