using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeMoves : Widget
{
    public DataTypeSubTree MovesList;
    public Move? Move => (Move) MovesList.SelectedItem?.Object;
    public Move? HoveringMove => (Move) MovesList.HoveringItem?.Object;

    Grid Grid;
    VignetteFade Fade;
    Container MainContainer;
    Container ScrollContainer;
    VStackPanel StackPanel;

    HintWindow HintWindow;

    public DataTypeMoves(IContainer Parent) : base(Parent)
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

		MovesList = new DataTypeSubTree("Moves", Grid);
        MovesList.SetBackgroundColor(28, 50, 73);
        MovesList.SetGridRow(0, 1);
        RedrawList();

        MainContainer = new Container(Grid);
		MainContainer.SetBackgroundColor(23, 40, 56);
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

        MovesList.OnSelectionChanged += _ => UpdateSelection();

        MovesList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New Move")
            {
                IsClickable = e => e.Value = MovesList.HoveringItem is not null,
                OnClicked = NewMove
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = MovesList.HoveringItem is not null,
                OnClicked = CutMove
            },
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = MovesList.HoveringItem is not null,
                OnClicked = CopyMove
            },
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = MovesList.HoveringItem is not null && Utilities.IsClipboardValidBinary(BinaryData.MOVE),
                OnClicked = PasteMove
            },
			new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = MovesList.HoveringItem is not null,
                OnClicked = DeleteMove
            }
        });

        MovesList.SetSelectedNode((TreeNode) MovesList.Root.Children[0]);
    }

	public void RedrawList(Move? moveToSelect = null)
	{
		List<TreeNode> MoveItems = new List<TreeNode>();
        TreeNode? nodeToSelect = null;
		foreach (ListItem listItem in Data.Sources.Moves)
		{
            Move mov = (Move) listItem.Object;
			TreeNode item = new TreeNode(mov.Name, mov);
            if (mov == moveToSelect) nodeToSelect = item;
			MoveItems.Add(item);
		}
		MovesList.SetItems(MoveItems);
        if (nodeToSelect != null)
        {
			MovesList.SetSelectedNode(nodeToSelect);
			MovesList.CenterOnSelectedNode();
		}
	}

    void UpdateSelection()
    {
        StackPanel?.Dispose();
        
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(1000);
        StackPanel.OnSizeChanged += _ => ScrollContainer.SetSize(StackPanel.Size.Width, StackPanel.Size.Height + 200);
        StackPanel.OnChildBoundsChanged += _ => StackPanel.UpdateLayout();

        DataContainer mainContainer = new DataContainer(StackPanel);
        mainContainer.SetText("Main");
        CreateMainContainer(mainContainer, this.Move);

        DataContainer descContainer = new DataContainer(StackPanel);
        descContainer.SetText("Description");
        CreateDescContainer(descContainer, this.Move);

        DataContainer effectContainer = new DataContainer(StackPanel);
        effectContainer.SetText("Effect");
        CreateEffectContainer(effectContainer, this.Move);

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

    void NewMove(BaseEventArgs e)
    {
        Move move = Game.Move.Create();
        move.Name = "Missingno.";
        move.ID = EnsureUniqueID("MISSINGNO");
        Data.Moves.Add(move.ID, move);
        RedrawList();
        TreeNode newNode = (TreeNode) MovesList.Root.GetAllChildren(true).Find(n => (Move) ((TreeNode) n).Object == move);
        MovesList.SetSelectedNode(newNode, true);
    }

    void CutMove(BaseEventArgs e)
    {
        if (MovesList.HoveringItem is null || MovesList.HoveringItem.Parent != MovesList.Root) return;
        CopyMove(e);
        DeleteMove(e);
    }

    void CopyMove(BaseEventArgs e)
	{
		if (MovesList.HoveringItem is null) return;
        Move move = HoveringMove;
        Utilities.SetClipboard(move, BinaryData.MOVE);
    }

    string EnsureUniqueID(string id)
    {
        int i = 0;
        string query = id;
        while (Data.Moves.ContainsKey(query))
        {
            i++;
            query = id + i.ToString();
        }
        return query;
    }

    void PasteMove(BaseEventArgs e)
    {
        if (MovesList.HoveringItem is null || !Utilities.IsClipboardValidBinary(BinaryData.MOVE)) return;
        Move data = Utilities.GetClipboard<Move>();
        data.ID = EnsureUniqueID(data.ID);
		Data.Moves.Add(data.ID, data);
        RedrawList();
        TreeNode node = (TreeNode) MovesList.Root.GetAllChildren(true).Find(n => (Move) ((TreeNode) n).Object == data);
        MovesList.SetSelectedNode(node, true);
		Data.Sources.InvalidateMoves();
	}

	void DeleteMove(BaseEventArgs e)
    {
        if (MovesList.HoveringItem is null) return;
        Move move = (Move) MovesList.HoveringItem.Object;
        bool selectFirst = false;
        if (MovesList.SelectedItem == MovesList.HoveringItem)
        {
            TreeNode prevNode = MovesList.HoveringItem.GetPreviousNode(false);
            if (prevNode == MovesList.Root) selectFirst = true;
            else MovesList.SetSelectedNode(prevNode);
        }
        Data.Moves.Remove(move.ID);
        MovesList.HoveringItem.Delete(true);
        if (selectFirst) MovesList.SetSelectedNode((TreeNode) MovesList.Root.Children[0]);
        MovesList.RedrawAllNodes();
        Data.Sources.InvalidateMoves();
    }
}
