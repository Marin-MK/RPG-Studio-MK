using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeMoves : DataTypeBase
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

        Fade = new VignetteFade(Grid);
        Fade.SetGridColumn(1);
        Fade.SetZIndex(2);

        MovesList = new DataTypeSubTree("Moves", Grid);
        MovesList.SetBackgroundColor(28, 50, 73);
        MovesList.OnScrolling += _ => Editor.ProjectSettings.LastMoveScroll = MovesList.GetScroll();
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

        MovesList.OnSelectionChanged += _ => UpdateSelection();

        MovesList.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New")
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
                IsClickable = e => e.Value = MovesList.HoveringItem is not null && Clipboard.IsValid(BinaryData.MOVE),
                OnClicked = PasteMove
            },
			new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = MovesList.HoveringItem is not null,
                OnClicked = DeleteMove
            }
        });
        Grid.UpdateLayout();

        RedrawList(Editor.ProjectSettings.LastMoveID);
    }

	public void RedrawList(Move? moveToSelect = null)
	{
		List<TreeNode> MoveItems = new List<TreeNode>();
        TreeNode? nodeToSelect = null;
		foreach (TreeNode listItem in Data.Sources.Moves)
		{
            if ((Move) listItem.Object == moveToSelect) nodeToSelect = listItem;
			MoveItems.Add(listItem);
		}
		MovesList.SetItems(MoveItems);
        if (nodeToSelect != null)
        {
            MovesList.SetScroll(Editor.ProjectSettings.LastMoveScroll);
			MovesList.SetSelectedNode(nodeToSelect);
		}
	}

    public void RedrawList(string moveToSelect)
    {
        Move mov = (Move) Data.Sources.Moves.Find(m => ((Move) m.Object).ID == moveToSelect)?.Object;
        if (moveToSelect == null) mov = (Move) Data.Sources.Moves[0].Object;
        RedrawList(mov);
    }

    public void SelectMove(Move mov)
    {
        ITreeNode node = MovesList.Root.GetAllChildren(true).Find(n => (Move) ((TreeNode) n).Object == mov);
        MovesList.SetSelectedNode((TreeNode) node);
    }

    void UpdateSelection()
    {
        Editor.ProjectSettings.LastMoveID = this.Move.ID;

        StackPanel?.Dispose();
        
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(1000);
        StackPanel.OnSizeChanged += _ => ScrollContainer.SetSize(StackPanel.Size.Width, StackPanel.Size.Height + 200);
        StackPanel.OnChildBoundsChanged += _ => StackPanel.UpdateLayout();

        DataContainer mainContainer = new DataContainer(StackPanel);
        mainContainer.SetText("Main");
        CreateMainContainer(mainContainer, this.Move);
        mainContainer.SetID("MOVES_MAIN");

        DataContainer descContainer = new DataContainer(StackPanel);
        descContainer.SetText("Description");
        CreateDescContainer(descContainer, this.Move);
        descContainer.SetID("MOVES_DESC");

        DataContainer effectContainer = new DataContainer(StackPanel);
        effectContainer.SetText("Effect");
        CreateEffectContainer(effectContainer, this.Move);
        effectContainer.SetID("MOVES_EFFECT");

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

    void NewMove(BaseEventArgs e)
    {
        Move move = Game.Move.Create();
        move.Name = "Missingno.";
        move.ID = EnsureUniqueID("MISSINGNO");
        Data.Moves.Add(move.ID, move);
        Data.Sources.InvalidateMoves();
        RedrawList(move);
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
        Clipboard.SetObject(move, BinaryData.MOVE);
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
        if (MovesList.HoveringItem is null || !Clipboard.IsValid(BinaryData.MOVE)) return;
        Move data = Clipboard.GetObject<Move>();
        data.ID = EnsureUniqueID(data.ID);
		Data.Moves.Add(data.ID, data);
        Data.Sources.InvalidateMoves();
        RedrawList(data);
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
