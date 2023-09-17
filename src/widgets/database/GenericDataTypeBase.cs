using RPGStudioMK.Game;
using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public abstract class GenericDataTypeBase<T> : SimpleDataTypeBase
{
	protected string Text;
	protected BinaryData DataType;
	protected Dictionary<string, T> DataSource;
	protected Func<List<TreeNode>> GetNodeDataSource;
	protected Func<T, string> GetID;
	protected Action<T, string> SetID;
	protected Action InvalidateData;
	protected Func<string> GetLastID;
	protected Action<string> SetLastID;
	protected Func<int> GetLastScroll;
	protected Action<int> SetLastScroll;
	protected List<(string Name, string ID, Action<DataContainer, T> CreateContainer, Func<T, bool>? Condition)> Containers;

	protected Grid Grid;
	protected VignetteFade Fade;
	protected DataTypeSubTree DataList;
	protected Container MainContainer;
	protected Container ScrollContainer;
	protected HintWindow HintWindow;
	protected VStackPanel StackPanel;
	protected T? LastDisplayedData;

	public T? SelectedItem => (T) DataList.SelectedItem?.Object;
	public T? HoveringItem => (T) DataList.HoveringItem?.Object;

	public GenericDataTypeBase(IContainer parent) : base(parent)
    {

    }

	protected virtual void LateConstructor()
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

		DataList = new DataTypeSubTree(Text, Grid);
		DataList.SetBackgroundColor(28, 50, 73);
		DataList.OnScrolling += _ => SetLastScroll(DataList.GetScroll());
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

        DataList.OnSelectionChanged += _ => UpdateSelection();

        DataList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null,
                OnClicked = _ => NewData()
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
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = DataList.HoveringItem is not null && Clipboard.IsValid(DataType),
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

	protected abstract T CreateData();

	protected virtual void UpdateSelection(bool forceUpdate = false)
	{
		if (!forceUpdate && LastDisplayedData is not null && LastDisplayedData.Equals(this.SelectedItem)) return;
		SetLastID(GetID(this.SelectedItem));

		StackPanel?.Dispose();

		StackPanel = new VStackPanel(ScrollContainer);
		StackPanel.SetWidth(1000);
		StackPanel.OnSizeChanged += _ => ScrollContainer.SetSize(StackPanel.Size.Width, StackPanel.Size.Height + 200);
		StackPanel.OnChildBoundsChanged += _ => StackPanel.UpdateLayout();

		Containers.ForEach(c =>
		{
			if (c.Condition?.Invoke(this.SelectedItem) ?? true)
			{
				DataContainer ctr = new DataContainer(StackPanel);
				ctr.SetText(c.Name);
				c.CreateContainer(ctr, this.SelectedItem);
				ctr.SetID(c.ID);
			}
		});

		if (ScrollContainer.Size.Width < MainContainer.Size.Width) ScrollContainer.SetPosition(MainContainer.Size.Width / 2 - ScrollContainer.Size.Width / 2, 0);
		else ScrollContainer.SetPosition(0, 0);
		StackPanel.Update();
		LastDisplayedData = this.SelectedItem;
	}

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		if (ScrollContainer is null) return;
		if (ScrollContainer.Size.Width < MainContainer.Size.Width) ScrollContainer.SetPosition(MainContainer.Size.Width / 2 - ScrollContainer.Size.Width / 2, 0);
		else ScrollContainer.SetPosition(0, 0);
	}

	protected virtual void RedrawList(T? DataToSelect)
    {
		List<TreeNode> ItemItems = new List<TreeNode>();
		TreeNode? nodeToSelect = null;
		foreach (TreeNode listItem in GetNodeDataSource())
		{
			if (((T) listItem.Object).Equals(DataToSelect)) nodeToSelect = listItem;
			ItemItems.Add(listItem);
		}
		DataList.SetItems(ItemItems);
		if (nodeToSelect != null)
		{
			DataList.SetScroll(GetLastScroll());
			DataList.SetSelectedNode(nodeToSelect);
		}
	}

	protected virtual void RedrawList(string dataToSelect)
	{
		List<TreeNode> nodeSource = GetNodeDataSource();
		T? data = (T) nodeSource.Find(m => GetID((T) m.Object) == dataToSelect)?.Object;
		if (dataToSelect == null) data = (T) nodeSource[0].Object;
		RedrawList(data);
	}

	protected virtual string EnsureUniqueID(string id)
	{
		int i = 0;
		string query = id;
		while (DataSource.ContainsKey(query))
		{
			i++;
			query = id + i.ToString();
		}
		return query;
	}

	public override void SelectData<D>(D dat)
	{
		ITreeNode node = DataList.Root.GetAllChildren(true).Find(n => GetID((T) ((TreeNode) n).Object) == dat.ID);
		DataList.SetSelectedNode((TreeNode) node);
	}

	protected virtual void NewData()
	{
		T data = CreateData();
		SetID(data, EnsureUniqueID("MISSINGNO"));
		DataSource.Add(GetID(data), data);
		InvalidateData();
		RedrawList(data);
	}

	protected virtual void CutData()
	{
		if (DataList.HoveringItem is null || DataList.HoveringItem.Parent != DataList.Root) return;
		CopyData();
		DeleteData();
	}

	protected virtual void CopyData()
	{
		if (DataList.HoveringItem is null) return;
		List<T> items = new List<T>();
		// If we have more than 1 node selected, and the hovering node is part of the selection, copy the whole selection
		if (DataList.SelectedItems.Count > 1 && DataList.SelectedItems.Contains(DataList.HoveringItem)) items = DataList.SelectedItems.Select(n => (T) n.Object).ToList();
		else
		{
			// Otherwise, if we have 1 node selected or the hovering node is not part of the selection,
			// then we only copy the hovering node.
			items.Add((T) DataList.HoveringItem.Object);
		}
		Clipboard.SetObject(items, DataType);
	}

    protected virtual void PasteData()
    {
		if (DataList.HoveringItem is null || !Clipboard.IsValid(DataType)) return;
		List<T> data = Clipboard.GetObject<List<T>>();
		foreach (T itm in data)
		{
			SetID(itm, EnsureUniqueID(GetID(itm)));
			DataSource.Add(GetID(itm), itm);
		}
        InvalidateData();
		RedrawList(data[0]);
		DataList.UnlockGraphics();
		DataList.Root.GetAllChildren(true).ForEach(n =>
		{
			if (data.Contains((T) ((TreeNode) n).Object)) DataList.SetSelectedNode((TreeNode)n, true, true, true, false);
		});
		DataList.LockGraphics();
	}

	protected virtual void DeleteData()
    {
		if (DataList.HoveringItem is null) return;
		List<(TreeNode, T)> items = new List<(TreeNode, T)>();
		// If we have more than 1 node selected, and the hovering node is part of the selection, delete the whole selection
		if (DataList.SelectedItems.Count > 1 && DataList.SelectedItems.Contains(DataList.HoveringItem)) items = DataList.SelectedItems.Select(n => (n, (T) n.Object)).ToList();
		else
		{
			// Otherwise, if we have 1 node selected or the hovering node is not part of the selection,
			// then we only delete the hovering node.
			items.Add((DataList.HoveringItem, (T) DataList.HoveringItem.Object));
			DataList.ClearSelection(DataList.SelectedItem);
		}
		bool selectFirst = false;
		// If the selected item will also be deleted, we need to change our selection.
		if (items.Any(e => e.Item2.Equals((T) DataList.SelectedItem.Object)))
		{
			TreeNode prevNode = DataList.SelectedItems[0].GetPreviousNode(false);
			if (prevNode == DataList.Root) selectFirst = true;
			else DataList.SetSelectedNode(prevNode);
		}
		foreach ((TreeNode node, T itm) in items)
		{
			DataSource.Remove(GetID(itm));
			node.Delete(true);
		}
		if (selectFirst) DataList.SetSelectedNode((TreeNode) DataList.Root.Children[0]);
		DataList.RedrawAllNodes();
        InvalidateData();
	}
}