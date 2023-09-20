using System;
using System.Collections.Generic;
using System.Linq;


namespace RPGStudioMK.Widgets;

public class TreeView : Widget
{
    // This class does not make use of the default Draw/Redraw functionality,
    // as nodes are redrawn individually for optimization, but must therefore not
    // differ too much from the previous version.

    // The primary optimization of this implemention is that it should not redraw
    // the entire list when a node is collapsed or expanded.
    // - If a node is collapsed, it should discard the region of its children, and move the bitmap content that was below it,
    //   slightly up to where the region of the node's children started.
    // - If a node is expanded, it should move the bitmap content below the node down based on how many children will be drawn,
    //   and then draw only those children in the newly-freed region in the bitmap.
    // Note that the bitmap must be resized for this to work. This could be done by literally resizing the bitmap internally,
    // or by creating a new bitmap and copying everything over. This would need to happen after the new content is drawn or deleted,
    // otherwise we might copy something in an overlapping region, or copy somewhere off the bitmap.

    static Bitmap TreeIconsBitmap;

    /// <summary>
    /// The root node of the tree. This node is not displayed.
    /// </summary>
    public TreeNode Root { get; protected set; }
    /// <summary>
    /// The height allocated to each node.
    /// </summary>
    public int LineHeight { get; protected set; } = 24;
    /// <summary>
    /// The depth, or x offset each child node introduces.
    /// </summary>
    public int DepthIndent { get; protected set; } = 20;
    /// <summary>
    /// The depth at which connecting lines start.
    /// </summary>
    public int? LineStartDepth { get; protected set; } = 0;
    /// <summary>
    /// A global x offset for the entire displayed tree.
    /// </summary>
    public int XOffset { get; protected set; } = 6;
    /// <summary>
    /// Additional horizontal scroll area.
    /// </summary>
    public int ExtraXScrollArea { get; protected set; } = 0;
    /// <summary>
    /// Additional vertical scroll area.
    /// </summary>
    public int ExtraYScrollArea { get; protected set; } = 0;
    /// <summary>
    /// The node that is currently being hovered over. May be null if no node is being hovered over.
    /// </summary>
    public ITreeNode HoveringNode { get; protected set; }
    /// <summary>
    /// All nodes that are currently selected.
    /// </summary>
    public List<ITreeNode> SelectedNodes { get; protected set; } = new List<ITreeNode>();
    /// <summary>
    /// Whether one or multiple nodes are selected.
    /// </summary>
    public bool MultipleSelected => SelectedNodes.Count > 1;
    /// <summary>
    /// The currently selected node. May be null if no nodes are selected.
    /// </summary>
    public ITreeNode SelectedNode => SelectedNodes.Count > 0 ? SelectedNodes[0] : null;
    /// <summary>
    /// Whether nodes can be rearranged with drag-and-drop.
    /// </summary>
    public bool CanDragAndDrop { get; protected set; } = true;
    /// <summary>
    /// Whether multiple selections are allowed.
    /// </summary>
    public bool CanMultiSelect { get; protected set; } = false;
    /// <summary>
    /// Whether the tree has any nodes aside from the root node.
    /// </summary>
    public bool Empty => !Root.HasChildren;
    /// <summary>
    /// Whether a selection is required at all times.
    /// </summary>
    public bool RequireSelection { get; protected set; } = true;
    /// <summary>
    /// The font used to display each node.
    /// </summary>
    public Font Font { get; protected set; }
    /// <summary>
    /// The padding of the horizontal scrollbar when no vertical scrollbar is present.
    /// </summary>
    public Padding HScrollBarPaddingAlone { get; protected set; } = new Padding(1, 0, 1, -1);
    /// <summary>
    /// The padding of the horizontal scrollbar when there is also a vertical scrollbar present.
    /// </summary>
    public Padding HScrollBarPaddingShared { get; protected set; } = new Padding(1, 0, 13, -1);
    /// <summary>
    /// The padding of the vertical scrollbar when no horizontal scrollbar is present.
    /// </summary>
    public Padding VScrollBarPaddingAlone { get; protected set; } = new Padding(0, 1, -1, 1);
    /// <summary>
    /// The padding of the vertical scrollbar when there is also a horizontal scrollbar present.
    /// </summary>
    public Padding VScrollBarPaddingShared { get; protected set; } = new Padding(0, 1, -1, 13);
    /// <summary>
    /// Whether to auto-resize horizontally to fill the scroll container.
    /// </summary>
    public bool HResizeToFill { get; protected set; } = false;
    /// <summary>
    /// Whether to auto-resize vertically to fill the scroll container.
    /// </summary>
    public bool VResizeToFill { get; protected set; } = true;
    /// <summary>
    /// Whether to auto-resize in the scroll container, or to leave that to this widget's parent.
    /// </summary>
    public new bool AutoResize { get; protected set; } = true;

    /// <summary>
    /// Called whenever drag-and-drop is initiated.
    /// </summary>
    public GenericObjectEvent<ITreeNode> OnDragAndDropping;
    /// <summary>
    /// Called whenever a drag-and-drop event is completed.
    /// </summary>
    public GenericObjectEvent<(ITreeNode DroppedNode, TreeNode OldRoot, TreeNode NewRoot)> OnDragAndDropped;
    /// <summary>
    /// Called whenever the active selection changes.
    /// </summary>
    public BoolEvent OnSelectionChanged;
    /// <summary>
    /// Called whenever a node is expanded or collapsed.
    /// </summary>
    public GenericObjectEvent<TreeNode> OnNodeExpansionChanged;
    /// <summary>
    /// Called whenever the tree structure is rearranged at any point.
    /// </summary>
    public GenericObjectEvent<TreeNode> OnNodeGlobalIndexChanged;
    /// <summary>
    /// Called whenever the visibility of the vertical scrollbar changes.
    /// </summary>
    public BoolEvent OnVScrollBarVisibilityChanged;
    /// <summary>
    /// Called whenever either the horizontal or vertical scrollbar visility changes.
    /// </summary>
    public GenericObjectEvent<(bool, bool)> OnScrollBarVisiblityChanged;
    /// <summary>
    /// Called whenever the visiblity of the horizontal scrollbar changes.
    /// </summary>
    public BoolEvent OnHScrollBarVisibilityChanged;

    /// <summary>
    /// Keeps track of the y positioning of drawn nodes.
    /// </summary>
    private List<(ITreeNode Node, int Y)> LastDrawData = new List<(ITreeNode, int)>();
    /// <summary>
    /// Keeps track of selection sprites.
    /// </summary>
    private List<(ITreeNode Node, int SpriteIndex)> SelectionSprites = new List<(ITreeNode, int)>();

    /// <summary>
    /// The container in which the tree scrolls if <see cref="AutoResize"/> is true.
    /// </summary>
    private Container ScrollContainer;
    /// <summary>
    /// The container that automatically resizes with the tree.
    /// </summary>
    private Container SpriteContainer;
    /// <summary>
    /// The background sprite for the tree.
    /// </summary>
    private Sprite BGSprite => SpriteContainer.Sprites["bg"];
    /// <summary>
    /// The text sprite for the tree.
    /// </summary>
    private Sprite TXTSprite => SpriteContainer.Sprites["txt"];

    /// <summary>
    /// The active node during mouse events.
    /// </summary>
    private ITreeNode? ActiveNode;
    /// <summary>
    /// Whether a node is currently being dragged.
    /// </summary>
    private bool Dragging = false;
    /// <summary>
    /// The drag-and-drop state, or where it is currently being released relative to the hovered node.
    /// </summary>
    private DragStates? DragState;
    /// <summary>
    /// The depth offset for drawing the drag-and-dropped node.
    /// </summary>
    private int DragLineOffset = 0;
    /// <summary>
    /// The origin point where drag-and-drop began. Used to only start drag-and-drop after a certain radius outside this point.
    /// </summary>
    private Point? DragOriginPoint;
    /// <summary>
    /// Whether drag-and-drop is active. The mouse must be outside a certain radius of <see cref="DragOriginPoint"/> to be valid.
    /// </summary>
    private bool ValidatedDragMovement = false;
    /// <summary>
    /// The old node being hovered over.
    /// </summary>
    private ITreeNode OldHoveringNode;
    /// <summary>
    /// Records the node that was clicked on the first click, in order to check if a second click was on the same node for double-click events.
    /// </summary>
    private ITreeNode? DoubleClickNode;
    /// <summary>
    /// The root node before drag-and-drop takes place.
    /// </summary>
    private TreeNode? PreDragDropRootNode;
    /// <summary>
    /// The query that was typed for keyboard node selection.
    /// </summary>
	private string query = "";
    /// <summary>
    /// The node currently shown as selected based on the typed query.
    /// </summary>
	private TreeNode queryNode;

    /// <summary>
    /// Creates a new Tree View.
    /// </summary>
    /// <param name="Parent">The parent widget.</param>
	public TreeView(IContainer Parent) : base(Parent)
    {
        if (TreeIconsBitmap == null)
        {
            TreeIconsBitmap = new Bitmap("assets/img/tree_icons");
        }

        this.Font = Fonts.Paragraph;

        ScrollContainer = new Container(this);

        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        HScrollBar hs = new HScrollBar(this);
        hs.SetHDocked(true);
        hs.SetBottomDocked(true);
        ScrollContainer.SetHScrollBar(hs);
        ScrollContainer.HAutoScroll = true;

        vs.OnVisibilityChanged += _ =>
        {
            OnVScrollBarVisibilityChanged?.Invoke(new BoolEventArgs(vs.Visible));
            OnScrollBarVisiblityChanged?.Invoke(new GenericObjectEventArgs<(bool, bool)>((vs.Visible, hs.Visible)));
        };
        hs.OnVisibilityChanged += _ =>
        {
            OnHScrollBarVisibilityChanged?.Invoke(new BoolEventArgs(hs.Visible));
            OnScrollBarVisiblityChanged?.Invoke(new GenericObjectEventArgs<(bool, bool)>((vs.Visible, hs.Visible)));
        };

        SpriteContainer = new Container(ScrollContainer);
        SpriteContainer.Sprites["hover"] = new Sprite(SpriteContainer.Viewport, new SolidBitmap(1, 1, new Color(55, 187, 255)));
        SpriteContainer.Sprites["hover"].Visible = false;
        SpriteContainer.Sprites["hover"].Z = 1;
        SpriteContainer.Sprites["bg"] = new Sprite(SpriteContainer.Viewport);
        SpriteContainer.Sprites["bg"].Z = 2;
        SpriteContainer.Sprites["txt"] = new Sprite(SpriteContainer.Viewport);
        SpriteContainer.Sprites["txt"].Z = 2;
        SpriteContainer.Sprites["drag"] = new Sprite(SpriteContainer.Viewport);
        SpriteContainer.Sprites["drag"].Z = 3;

        OnWidgetSelected += WidgetSelected;
        this.Root = new TreeNode("ROOT");

        this.OnContextMenuOpening += e => e.Value = !ScrollContainer.HScrollBar.Mouse.Inside && !ScrollContainer.VScrollBar.Mouse.Inside;

        this.RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveDown()),
			new Shortcut(this, new Key(Keycode.DOWN, Keycode.SHIFT), _ => MoveDown(true), false, e => e.Value = CanMultiSelect),
			new Shortcut(this, new Key(Keycode.UP), _ => MoveUp()),
			new Shortcut(this, new Key(Keycode.UP, Keycode.SHIFT), _ => MoveUp(true), false, e => e.Value = CanMultiSelect),
			new Shortcut(this, new Key(Keycode.PAGEDOWN), _ => MovePageDown()),
			new Shortcut(this, new Key(Keycode.PAGEDOWN, Keycode.SHIFT), _ => MovePageDown(true), false, e => e.Value = CanMultiSelect),
			new Shortcut(this, new Key(Keycode.PAGEUP), _ => MovePageUp()),
			new Shortcut(this, new Key(Keycode.PAGEUP, Keycode.SHIFT), _ => MovePageUp(true), false, e => e.Value = CanMultiSelect),
			new Shortcut(this, new Key(Keycode.A, Keycode.CTRL), _ => SelectAll(), false, e => e.Value = CanMultiSelect)
        });
    }

    /// <summary>
    /// Whether the tree view can scroll horizontally if it needs to.
    /// </summary>
    /// <param name="hScrollable"></param>
    public void SetHScrollable(bool hScrollable)
    {
        ScrollContainer.HAutoScroll = hScrollable;
    }

    /// <summary>
    /// Exposes the mechanism for unlocking graphics, to make bulk drawing faster.
    /// </summary>
    public void UnlockGraphics()
    {
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
    }

    /// <summary>
    /// Exposes the mechanism for locking graphics, to make bulk drawing faster.
    /// </summary>
    public void LockGraphics()
    {
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
    }

    /// <summary>
    /// Sets whether to auto-resize in-widget (true) or to leave it to the parent (false).
    /// </summary>
    /// <param name="AutoResize"></param>
    public void SetAutoResize(bool AutoResize)
    {
        if (this.AutoResize != AutoResize)
        {
            this.AutoResize = AutoResize;
            ScrollContainer.VAutoScroll = AutoResize;
            ScrollContainer.HAutoScroll = AutoResize;
            ScrollContainer.HScrollBar.SetVisible(AutoResize);
            ScrollContainer.VScrollBar.SetVisible(AutoResize);
            if (!this.AutoResize)
            {
                ScrollContainer.SetSize(SpriteContainer.Size);
                this.SetSize(SpriteContainer.Size);
            }
            ScrollContainer.UpdateAutoScroll();
        }
    }

    /// <summary>
    /// Changes the root node of the tree.
    /// </summary>
    /// <param name="Root">The new root node.</param>
    /// <param name="SelectedNode">The node to select in the new tree.</param>
    public void SetRootNode(TreeNode Root, ITreeNode? SelectedNode = null)
    {
        if (this.Root != Root)
        {
            this.Root = Root;
            this.Root.GetAllChildren(true).ForEach(n =>
            {
                if (n is TreeNode)
                {
                    TreeNode Node = (TreeNode) n;
                    Node.OnGlobalIndexChanged = _ => OnNodeGlobalIndexChanged?.Invoke(new GenericObjectEventArgs<TreeNode>(Node));
                }
            });
            RedrawAllNodes();
            if (RequireSelection) SetSelectedNode(SelectedNode ?? (Root.HasChildren ? Root.Children[0] : null), false);
        }
    }

    /// <summary>
    /// Changes the line height allocated to each node.
    /// </summary>
    /// <param name="LineHeight">The new line height.</param>
    public void SetLineHeight(int LineHeight)
    {
        if (this.LineHeight != LineHeight)
        {
            this.LineHeight = LineHeight;
            RedrawAllNodes();
        }
    }

    /// <summary>
    /// Changes the depth indent attributed to each child node level.
    /// </summary>
    /// <param name="DepthIndent">The new depth indent.</param>
    public void SetDepthIndent(int DepthIndent)
    {
        if (this.DepthIndent != DepthIndent)
        {
            this.DepthIndent = DepthIndent;
            RedrawAllNodes();
        }
    }

    /// <summary>
    /// Changes whether drag-and-drop is allowed.
    /// </summary>
    /// <param name="CanDragAndDrop">Whether drag-and-drop is allowed.</param>
    public void SetCanDragAndDrop(bool CanDragAndDrop)
    {
        if (this.CanDragAndDrop != CanDragAndDrop)
        {
            this.CanDragAndDrop = CanDragAndDrop;
        }
    }

    /// <summary>
    /// Changes whether multiple selection are allowed.
    /// </summary>
    /// <param name="CanMultiSelect">Whether multiple selection are allowed.</param>
    public void SetCanMultiSelect(bool CanMultiSelect)
    {
        if (this.CanMultiSelect != CanMultiSelect)
        {
            this.CanMultiSelect = CanMultiSelect;
        }
    }

    /// <summary>
    /// Changes whether a selection is always required.
    /// </summary>
    /// <param name="RequireSelection">Whether a selection is always required.</param>
    public void SetRequireSelection(bool RequireSelection)
    {
        if (this.RequireSelection != RequireSelection)
        {
            this.RequireSelection = RequireSelection;
            if (this.RequireSelection && this.SelectedNodes.Count == 0 && Root.HasChildren)
            {
                this.SetSelectedNode(Root.Children[0], false);
            }
        }
    }

    /// <summary>
    /// Changes the font of the nodes.
    /// </summary>
    /// <param name="Font">The new font.</param>
    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            RedrawAllNodes();
        }
    }

    /// <summary>
    /// Changes the padding of the vertical scrollbar when no horizontal scrollbar is present.
    /// </summary>
    /// <param name="VScrollBarPaddingAlone">The new padding.</param>
    public void SetVScrollBarPaddingAlone(Padding VScrollBarPaddingAlone)
    {
        if (this.VScrollBarPaddingAlone != VScrollBarPaddingAlone)
        {
            this.VScrollBarPaddingAlone = VScrollBarPaddingAlone;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    /// <summary>
    /// Changes the padding of the vertical scrollbar when there is a horizontal scrollbar present.
    /// </summary>
    /// <param name="VScrollBarPaddingShared">The new padding.</param>
    public void SetVScrollBarPaddingShared(Padding VScrollBarPaddingShared)
    {
        if (this.VScrollBarPaddingShared != VScrollBarPaddingShared)
        {
            this.VScrollBarPaddingShared = VScrollBarPaddingShared;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    /// <summary>
    /// Changes the padding of the horizontal scrollbar when no vertical scrollbar is present.
    /// </summary>
    /// <param name="HScrollBarPaddingAlone">The new padding.</param>
    public void SetHScrollBarPaddingAlone(Padding HScrollBarPaddingAlone)
    {
        if (this.HScrollBarPaddingAlone != HScrollBarPaddingAlone)
        {
            this.HScrollBarPaddingAlone = HScrollBarPaddingAlone;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    /// <summary>
    /// Changes the padding of the horizontal scrollbar when there is a vertical scrollbar present.
    /// </summary>
    /// <param name="HScrollBarPaddingShared">Whether to auto-resize.</param>
    public void SetHScrollBarPaddingShared(Padding HScrollBarPaddingShared)
    {
        if (this.HScrollBarPaddingShared != HScrollBarPaddingShared)
        {
            this.HScrollBarPaddingShared = HScrollBarPaddingShared;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    /// <summary>
    /// Sets whether to automatically resize vertically to fill the container.
    /// </summary>
    /// <param name="VResizeToFill"></param>
    public void SetVResizeToFill(bool VResizeToFill)
    {
        if (this.VResizeToFill != VResizeToFill)
        {
            this.VResizeToFill = VResizeToFill;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    /// <summary>
    /// Changes whether to automatically resize horizontally to fill the container.
    /// </summary>
    /// <param name="HResizeToFill">Whether to auto-resize.</param>
    public void SetHResizeToFill(bool HResizeToFill)
    {
        if (this.HResizeToFill != HResizeToFill)
        {
            this.HResizeToFill = HResizeToFill;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    /// <summary>
    /// Changes the root node children.
    /// </summary>
    /// <param name="Nodes">The new child nodes.</param>
    /// <param name="SelectedNode">The node to select afterwards, or null.</param>
    public void SetNodes(List<TreeNode> Nodes, TreeNode? SelectedNode = null)
    {
        this.Root.ClearChildren();
        foreach (TreeNode node in Nodes)
        {
            this.Root.AddChild(node);
        }
        this.Root.GetAllChildren(true).ForEach(n =>
        {
            if (n is TreeNode)
            {
                TreeNode node = (TreeNode) n;
                node.OnGlobalIndexChanged = _ => OnNodeGlobalIndexChanged?.Invoke(new GenericObjectEventArgs<TreeNode>(node));
            }
        });
        RedrawAllNodes();
        if (RequireSelection) SetSelectedNode(SelectedNode ?? (Root.HasChildren ? Root.Children[0] : null), false);
    }

    /// <summary>
    /// Changes the x offset of the drawn tree.
    /// </summary>
    /// <param name="XOffset">The new x offset.</param>
    public void SetXOffset(int XOffset)
    {
        if (this.XOffset != XOffset)
        {
            this.XOffset = XOffset;
            this.RedrawAllNodes();
        }
    }

    /// <summary>
    /// Changes the additional horizontal scroll area.
    /// </summary>
    /// <param name="ExtraXScrollArea">The new scroll area.</param>
    public void SetExtraXScrollArea(int ExtraXScrollArea)
    {
        if (this.ExtraXScrollArea != ExtraXScrollArea)
        {
            this.ExtraXScrollArea = ExtraXScrollArea;
            if (BGSprite.Bitmap != null) this.UpdateSize();
        }
    }

    /// <summary>
    /// Changes the additional vertical scroll area.
    /// </summary>
    /// <param name="ExtraYScrollArea">The new scroll area.</param>
    public void SetExtraYScrollArea(int ExtraYScrollArea)
    {
        if (this.ExtraYScrollArea != ExtraYScrollArea)
        {
            this.ExtraYScrollArea = ExtraYScrollArea;
            if (BGSprite.Bitmap != null) this.UpdateSize();
        }
    }

    /// <summary>
    /// Changes the active and selected node values. Do not use to select a node in normal operation.
    /// </summary>
    /// <param name="node">The node to activate and select.</param>
    public void SetActiveAndSelectedNode(TreeNode node)
    {
        this.ActiveNode = node;
        this.SelectedNodes = new List<ITreeNode>() { node };
    }

    /// <summary>
    /// Changes whether a node is expanded or collapsed.
    /// </summary>
    /// <param name="Node">The node to expand or collapse.</param>
    /// <param name="Expanded">Whether the node should be expanded or collapsed.</param>
	public unsafe void SetExpanded(TreeNode Node, bool Expanded)
    {
        if (Node.Expanded != Expanded)
        {
            if (Expanded)
            {
                // The node is being expanded; shift everything below this node down, then redraw this node
                BGSprite.Bitmap.Unlock();
                TXTSprite.Bitmap.Unlock();
                Node.SetExpanded(Expanded);
                UpdateSize();
                (int NodeCount, int SepHeight) = Node.GetChildrenHeight(false);
                int Y = GetDrawnYCoord(Node);
                int Height = (NodeCount + 1) * LineHeight + SepHeight;
                int movy = Y + LineHeight;
                int movh = BGSprite.Bitmap.Height - (movy);
                int movamt = Height - LineHeight;
                BGSprite.Bitmap.FillRect(0, Y, BGSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                TXTSprite.Bitmap.FillRect(0, Y, TXTSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                int NewWidth = SpriteContainer.Size.Width - ExtraXScrollArea;
                BGSprite.Bitmap = BGSprite.Bitmap.Resize(NewWidth, BGSprite.Bitmap.Height + movamt);
                TXTSprite.Bitmap = TXTSprite.Bitmap.Resize(NewWidth, TXTSprite.Bitmap.Height + movamt);
                UpdateSize(false);
                TXTSprite.Bitmap.Font = this.Font;
                TXTSprite.Bitmap.BlendMode = BlendMode.Addition;
                BGSprite.Bitmap.Unlock();
                TXTSprite.Bitmap.Unlock();
                BGSprite.Bitmap.ShiftVertically(movy, movh, movamt, true);
                TXTSprite.Bitmap.ShiftVertically(movy, movh, movamt, true);
                int Index = LastDrawData.FindIndex(d => d.Node == Node);
                for (int i = Index + 1; i < LastDrawData.Count; i++)
                {
                    LastDrawData[i] = (LastDrawData[i].Node, LastDrawData[i].Y + movamt);
                }
                // Remove the data entry here, so it can be inserted in the call below.
                LastDrawData.RemoveAt(Index);
                Y = RedrawNode(Node, Y, true, () => Index);
                Node.GetAllChildren(false).ForEach(n =>
                {
                    Index++;
                    Y = RedrawNode(n, Y, true, () => Index);
                });
                BGSprite.Bitmap.Lock();
                TXTSprite.Bitmap.Lock();
            }
            else
            {
                // The node is being collapsed; shift everything below this node up, then redraw this node
                (int NodeCount, int SepHeight) = Node.GetChildrenHeight(false);
                BGSprite.Bitmap.Unlock();
                TXTSprite.Bitmap.Unlock();
                int Y = GetDrawnYCoord(Node);
                int Height = (NodeCount + 1) * LineHeight + SepHeight;
                // We only need to clear the node area itself; the area of its children will be overriden by the ShiftVertically method calls.
                BGSprite.Bitmap.FillRect(0, Y, BGSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                TXTSprite.Bitmap.FillRect(0, Y, TXTSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                Node.SetExpanded(Expanded);
                UpdateSize();
                RedrawNode(Node, Y, false);
                int movy = Y + Height;
                int movh = BGSprite.Bitmap.Height - (movy);
                int movamt = Height - LineHeight;
                BGSprite.Bitmap.ShiftVertically(movy, movh, -movamt, true);
                TXTSprite.Bitmap.ShiftVertically(movy, movh, -movamt, true);
				int NewWidth = SpriteContainer.Size.Width - ExtraXScrollArea;
				BGSprite.Bitmap = BGSprite.Bitmap.Resize(NewWidth, BGSprite.Bitmap.Height - movamt);
                TXTSprite.Bitmap = TXTSprite.Bitmap.Resize(NewWidth, TXTSprite.Bitmap.Height - movamt);
                UpdateSize(false);
                TXTSprite.Bitmap.Font = this.Font;
                TXTSprite.Bitmap.BlendMode = BlendMode.Addition;
                TXTSprite.Bitmap.Relock();
                int Index = LastDrawData.FindIndex(d => d.Node == Node);
                for (int i = Index + 1; i < LastDrawData.Count; i++)
                {
                    if (Node.Contains(LastDrawData[i].Node, true))
                    {
                        LastDrawData.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        LastDrawData[i] = (LastDrawData[i].Node, LastDrawData[i].Y - movamt);
                    }
                }
            }
            OnNodeExpansionChanged?.Invoke(new GenericObjectEventArgs<TreeNode>(Node));
        }
    }

    /// <summary>
    /// Called whenever the widget is selected.
    /// </summary>
	public override void WidgetSelected(BaseEventArgs e)
	{
		base.WidgetSelected(e);
        Input.StartTextInput();
	}

    /// <summary>
    /// Called whenever the widget is deselected.
    /// </summary>
	public override void WidgetDeselected(BaseEventArgs e)
	{
		base.WidgetDeselected(e);
        Input.StopTextInput();
	}

    /// <summary>
    /// Called whenever the widget received keyboard input.
    /// </summary>
	public override void TextInput(TextEventArgs e)
	{
		base.TextInput(e);
        if (string.IsNullOrEmpty(e.Text) || e.Backspace || e.Delete || e.Tab) return;
        if (TimerExists("idle") && !TimerPassed("idle"))
        {
            query += e.Text;
            ResetTimer("idle");
        }
        else
        {
            if (TimerExists("idle")) ResetTimer("idle");
            else SetTimer("idle", 500);
            query = e.Text;
        }
        SelectQuery(query);
	}

    /// <summary>
    /// Called whenever the Down key is pressed.
    /// </summary>
	public void MoveDown(bool shift = false)
    {
        if (shift)
        {
            SelectionAnchor ??= this.SelectedNode;
            TreeNode downNode = ((TreeNode) this.SelectedNode).GetNextNode(false);
            if (downNode is null) return;
			BGSprite.Bitmap.Unlock();
			TXTSprite.Bitmap.Unlock();
			ClearSelection(null, false);
			SetSelectedNode(downNode, true, true, true, false);
            GetNodeRange(SelectionAnchor, downNode).ForEach(n => SetSelectedNode(n, true, true, false, false));
			SetSelectedNode(SelectionAnchor, true, true, false, false);
			BGSprite.Bitmap.Lock();
			TXTSprite.Bitmap.Lock();
        }
        else
        {
            SelectionAnchor = null;
            TreeNode nextNode = (TreeNode) SelectedNode;
            nextNode = nextNode.GetNextNode(false);
            if (nextNode == null || nextNode.Root == nextNode) return;
            SetSelectedNode(nextNode, false);
        }
        EnsureSelectedNodeVisible();
		this.WidgetSelected(new BaseEventArgs());
	}

    /// <summary>
    /// Called whenever the Up key is pressed.
    /// </summary>
    public void MoveUp(bool shift = false)
    {
        if (shift)
        {
            SelectionAnchor ??= this.SelectedNode;
            TreeNode upNode = ((TreeNode) this.SelectedNode).GetPreviousNode(false);
            if (upNode is null || upNode.Root == upNode) return;
			BGSprite.Bitmap.Unlock();
			TXTSprite.Bitmap.Unlock();
			ClearSelection(null, false);
			SetSelectedNode(upNode, true, true, true, false);
            GetNodeRange(SelectionAnchor, upNode).ForEach(n => SetSelectedNode(n, true, true, false, false));
			SetSelectedNode(SelectionAnchor, true, true, false, false);
			BGSprite.Bitmap.Lock();
			TXTSprite.Bitmap.Lock();
        }
        else
        {
            SelectionAnchor = null;
            TreeNode prevNode = (TreeNode) SelectedNode;
            prevNode = prevNode.GetPreviousNode(false);
            if (prevNode == null || prevNode.Root == prevNode) return;
            SetSelectedNode(prevNode, false);
        }
        EnsureSelectedNodeVisible();
		this.WidgetSelected(new BaseEventArgs());
	}

    /// <summary>
    /// Ensures the selected node is within the field of view.
    /// </summary>
    public void EnsureSelectedNodeVisible()
    {
        int scrolledY = this.AutoResize ? ScrollContainer.ScrolledY : Parent.ScrolledY;
        int height = this.AutoResize ? ScrollContainer.Size.Height : Parent.Size.Height;
		(ITreeNode topNode, int topY) = LastDrawData.Find(d => d.Y + LineHeight / 2 > scrolledY);
		(ITreeNode bottomNode, int bottomY) = LastDrawData.FindLast(d => d.Y < scrolledY + height - LineHeight / 2);
        (_, int curY) = LastDrawData.Find(d => d.Node == SelectedNode);
        if (curY < topY)
        {
            scrolledY = curY;
		}
        else if (curY > bottomY)
		{
            scrolledY = curY - height + LineHeight;
            if (scrolledY < 0) scrolledY = 0;
        }
        if (this.AutoResize)
        {
            ScrollContainer.ScrolledY = scrolledY;
            ScrollContainer.UpdateAutoScroll();
        }
        else
        {
            Parent.ScrolledY = scrolledY;
            ((Widget) Parent).UpdateAutoScroll();
        }
    }

    /// <summary>
    /// Called whenever the Page Down key is pressed.
    /// </summary>
    public void MovePageDown(bool shift = false)
    {
        int scrolledY = this.AutoResize ? ScrollContainer.ScrolledY : Parent.ScrolledY;
        int height = this.AutoResize ? ScrollContainer.Size.Height : Parent.Size.Height;
		(ITreeNode bottomNode, int bottomY) = LastDrawData.FindLast(d => d.Y < scrolledY + height - LineHeight / 2);
        (_, int curY) = LastDrawData.Find(d => d.Node == SelectedNode);
        ITreeNode oldSelectedNode = this.SelectedNode;
        if (curY > bottomY)
        {
            scrolledY = curY;
        }
        else if (curY < bottomY)
		{
            int diff = scrolledY + height - bottomY;
            if (diff < LineHeight && diff >= LineHeight / 2)
            {
                scrolledY += LineHeight - diff;
            }
            SetSelectedNode(bottomNode, false);
        }
        else
        {
            scrolledY += height;
			(ITreeNode nextNode, int nextY) = LastDrawData.FindLast(d => d.Y < scrolledY + height - LineHeight / 2);
            int diff = scrolledY + height - nextY;
            if (diff < LineHeight && diff >= LineHeight / 2)
			{
				scrolledY += LineHeight - diff;
			}
            if (nextNode != null) SetSelectedNode(nextNode, false);
            else return;
		}
        if (shift)
        {
            SelectionAnchor ??= oldSelectedNode;
            BGSprite.Bitmap.Unlock();
            TXTSprite.Bitmap.Unlock();
            GetNodeRange(SelectionAnchor, this.SelectedNode).ForEach(n => SetSelectedNode(n, true, true, false, false));
            SetSelectedNode(SelectionAnchor, true, true, false, false);
            BGSprite.Bitmap.Lock();
            TXTSprite.Bitmap.Lock();
        }
        else SelectionAnchor = null;
		if (this.AutoResize)
        {
            ScrollContainer.ScrolledY = scrolledY;
            ScrollContainer.UpdateAutoScroll();
        }
        else
        {
            Parent.ScrolledY = scrolledY;
            ((Widget) Parent).UpdateAutoScroll();
        }
		this.WidgetSelected(new BaseEventArgs());
	}

    /// <summary>
    /// Called whenever the Page Up key is pressed.
    /// </summary>
    public void MovePageUp(bool shift = false)
    {
		int scrolledY = this.AutoResize ? ScrollContainer.ScrolledY : Parent.ScrolledY;
		int height = this.AutoResize ? ScrollContainer.Size.Height : Parent.Size.Height;
		(ITreeNode topNode, int topY) = LastDrawData.Find(d => d.Y + LineHeight / 2 > scrolledY);
		(_, int curY) = LastDrawData.Find(d => d.Node == SelectedNode);
		ITreeNode oldSelectedNode = this.SelectedNode;
		if (curY > topY)
		{
            if (topY < scrolledY)
            {
                scrolledY = topY;
            }
            SetSelectedNode(topNode, false);
		}
		else if (curY < topY)
		{
			scrolledY = curY;
		}
		else
		{
			scrolledY -= height;
            if (scrolledY < 0) scrolledY = 0;
			(ITreeNode prevNode, int prevY) = LastDrawData.Find(d => d.Y + LineHeight / 2 > scrolledY);
			if (prevY < scrolledY)
			{
                scrolledY = prevY;
			}
            if (prevNode != null) SetSelectedNode(prevNode, false);
            else return;
		}
        if (shift)
        {
			SelectionAnchor ??= oldSelectedNode;
			BGSprite.Bitmap.Unlock();
			TXTSprite.Bitmap.Unlock();
			GetNodeRange(SelectionAnchor, this.SelectedNode).ForEach(n => SetSelectedNode(n, true, true, false, false));
			SetSelectedNode(SelectionAnchor, true, true, false, false);
			BGSprite.Bitmap.Lock();
			TXTSprite.Bitmap.Lock();
		}
        else SelectionAnchor = null;
		if (this.AutoResize)
		{
			ScrollContainer.ScrolledY = scrolledY;
			ScrollContainer.UpdateAutoScroll();
		}
		else
		{
			Parent.ScrolledY = scrolledY;
			((Widget) Parent).UpdateAutoScroll();
		}
		this.WidgetSelected(new BaseEventArgs());
	}

    /// <summary>
    /// Selects all nodes.
    /// </summary>
    public void SelectAll()
    {
		BGSprite.Bitmap.Unlock();
		TXTSprite.Bitmap.Unlock();
        ClearSelection(null, false);
        List<ITreeNode> nodes = Root.GetAllChildren(true);
        SetSelectedNode(nodes[^1], true, true, true, false);
		nodes.Take(nodes.Count - 1).ToList().ForEach(n => SetSelectedNode(n, true, true, false, false));
		BGSprite.Bitmap.Lock();
		TXTSprite.Bitmap.Lock();
	}

    /// <summary>
    /// Center the tree list on the specified node.
    /// </summary>
    /// <param name="centerNode">The node to center the tree list on.</param>
    public void CenterOnNode(ITreeNode centerNode)
    {
        int scrolledY = this.AutoResize ? ScrollContainer.ScrolledY : Parent.ScrolledY;
        int height = this.AutoResize ? ScrollContainer.Size.Height : Parent.Size.Height;
        (_, int nodeY) = LastDrawData.Find(d => d.Node == centerNode);
        scrolledY = Math.Max(0, nodeY - height / 2);
        if (this.AutoResize)
        {
            ScrollContainer.ScrolledY = scrolledY;
            ScrollContainer.UpdateAutoScroll();
        }
        else
        {
            Parent.ScrolledY = scrolledY;
            ((Widget) Parent).UpdateAutoScroll();
        }
    }

    /// <summary>
    /// Calculates the maximum width of a node and its children.
    /// </summary>
    /// <param name="Start">The node to start calculating at.</param>
    /// <returns>The total width of a node and its children.</returns>
    private int CalculateMaxWidth(TreeNode Start)
    {
        int MaxWidth = 0;
        if (Start != Start.Root) // Do not include the root as it has no text
        {
            int w = (Start.Depth - 1) * DepthIndent + XOffset;
            w += 30; // offset of text to start of node wrt depth
            w += Font.TextSize(Start.Text).Width; // width of text
            MaxWidth = w;
        }
        if (!Start.Expanded) return MaxWidth;
        foreach (ITreeNode Child in Start.Children)
        {
            int cw = Child is TreeNodeSeparator ? 0 : CalculateMaxWidth((TreeNode) Child);
            if (cw > MaxWidth) MaxWidth = cw;
        }
        return MaxWidth;
    }

    /// <summary>
    /// Inserts a node as a child of a parent node and selectively redraws the tree.
    /// </summary>
    /// <param name="ParentNode">The parent node.</param>
    /// <param name="InsertionIndex">The index of the parent's child list, or null to append it.</param>
    /// <param name="NewNode">The node to insert.</param>
    public void InsertNode(TreeNode ParentNode, int? InsertionIndex, ITreeNode NewNode)
    {
        if (InsertionIndex == -1) InsertionIndex = null;
        bool DidNotHaveChildren = ParentNode.HasChildren;
        bool RedrawPrevSibling = ParentNode.HasChildren && (InsertionIndex == null || InsertionIndex == ParentNode.Children.Count);
        if (!ParentNode.Expanded && ParentNode.HasChildren) SetExpanded(ParentNode, true);
        ParentNode.InsertChild(InsertionIndex ?? ParentNode.Children.Count, NewNode);
        if (BGSprite.Bitmap is null || BGSprite.Bitmap.Disposed)
        {
			// We need to recreate the bitmaps first before we can draw
			// Redrawing all nodes now that we've added our new node, has the effect of 
			LastDrawData.Clear();
			(int RootNodeCount, int RootSepHeight) = Root.GetChildrenHeight(false);
			int MaxWidth = CalculateMaxWidth(Root) + ExtraXScrollArea;
			BGSprite.Bitmap = new Bitmap(MaxWidth, RootNodeCount * LineHeight + RootSepHeight, Graphics.MaxTextureSize.Width, 1024);
			TXTSprite.Bitmap = new Bitmap(MaxWidth, RootNodeCount * LineHeight + RootSepHeight, Graphics.MaxTextureSize.Width, 1024);
			TXTSprite.Bitmap.Font = this.Font;
		}
        UpdateSize();
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
        (int CountUntil, int SepHeightUntil) = ParentNode.GetChildrenHeightUntil(NewNode, false);
        int Y = GetDrawnYCoord(ParentNode) + CountUntil * LineHeight + SepHeightUntil;
        if (ParentNode != NewNode.Root) Y += LineHeight; // Add the height of the parent node itself, unless the parent node is the root (because it is not displayed)
        int NodeCount = 0;
        int SepHeight = 0;
        if (NewNode is TreeNode)
        {
            (NodeCount, SepHeight) = ((TreeNode) NewNode).GetChildrenHeight(false);
        }
        else
        {
            SepHeight = ((TreeNodeSeparator) NewNode).Height;
        }
        int Height = (NodeCount + 1) * LineHeight + SepHeight;
        int movy = Y;
        int movh = BGSprite.Bitmap.Height - movy;
        int movamt = Height;
        int NewWidth = SpriteContainer.Size.Width - ExtraXScrollArea;
        BGSprite.Bitmap = BGSprite.Bitmap.Resize(NewWidth, BGSprite.Bitmap.Height + movamt);
        TXTSprite.Bitmap = TXTSprite.Bitmap.Resize(NewWidth, TXTSprite.Bitmap.Height + movamt);
        UpdateSize(false);
        TXTSprite.Bitmap.Font = this.Font;
        TXTSprite.Bitmap.BlendMode = BlendMode.Addition;
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
        BGSprite.Bitmap.ShiftVertically(movy, movh, movamt, true);
        TXTSprite.Bitmap.ShiftVertically(movy, movh, movamt, true);
        int Index = LastDrawData.FindIndex(d => d.Y >= Y);
        if (Index < 0) Index = LastDrawData.Count;
        for (int i = Index; i < LastDrawData.Count; i++)
        {
            LastDrawData[i] = (LastDrawData[i].Node, LastDrawData[i].Y + movamt);
        }
        Y = RedrawNode(NewNode, Y, true, () => Index);
        if (NewNode is TreeNode) ((TreeNode) NewNode).GetAllChildren(false).ForEach(n =>
        {
            Index++;
            Y = RedrawNode(n, Y, true, () => Index);
        });
        if (!DidNotHaveChildren && ParentNode != Root)
        {
            BGSprite.Bitmap.FillRect(0, movy - LineHeight, BGSprite.Bitmap.Width, LineHeight, Color.ALPHA);
            TXTSprite.Bitmap.FillRect(0, movy - LineHeight, TXTSprite.Bitmap.Width, LineHeight, Color.ALPHA);
            ParentNode.Expand();
            RedrawNode(ParentNode, movy - LineHeight, false);
        }
        // If there is a new last node, then the previous last node does not have the line coming
        // from its parent indicating that there is another node.
        // So we draw that line manually here, outside of any node redrawing.
        if (RedrawPrevSibling && NewNode.Parent != this.Root)
        {
            int x = (NewNode.Depth - 1) * DepthIndent + XOffset;
            int sy = GetDrawnYCoord(ParentNode) + LineHeight;
            int ey = movy;
            BGSprite.Bitmap.DrawLine(x + 19 - DepthIndent, sy, x + 19 - DepthIndent, ey, new Color(64, 104, 146));
        }
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
        if (SelectedNode == null && RequireSelection) SetSelectedNode(NewNode, false);
    }

    /// <summary>
    /// Deletes a node and selectively redraws the tree.
    /// </summary>
    /// <param name="Node">The node to delete.</param>
    /// <param name="DeleteChildren">Whether to delete its child nodes or concatenate them in its parent.</param>
    /// <returns>The deleted nodes.</returns>
    public List<ITreeNode> DeleteNode(ITreeNode Node, bool DeleteChildren)
    {
        TreeNode Parent = Node.Parent;
        TreeNode? PreviousSibling = (Node as TreeNode)?.GetPreviousSibling();
        TreeNode? NextSibling = (Node as TreeNode)?.GetNextSibling();
        int OldDepth = Node.Depth;
        int ChildIndex = Parent.Children.IndexOf(Node);
        Node.Delete(DeleteChildren);
        UpdateSize();
        if (Root.Children.Count == 0)
        {
            BGSprite.Bitmap.Dispose();
            TXTSprite.Bitmap.Dispose();
            LastDrawData.Clear();
            SetSelectedNode(null, false);
            return null;
        }
        (int NodeCount, int SepHeight) = (0, 0);
        if (Node is TreeNode)
        {
            (NodeCount, SepHeight) = ((TreeNode) Node).GetChildrenHeight(false);
            NodeCount++; // Count the node itself
        }
        else
        {
            SepHeight = ((TreeNodeSeparator) Node).Height;
        }
        int Y = GetDrawnYCoord(Node);
        int HeightToClear = NodeCount * LineHeight + SepHeight;
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
        BGSprite.Bitmap.FillRect(0, Y, BGSprite.Bitmap.Width, HeightToClear, Color.ALPHA);
        TXTSprite.Bitmap.FillRect(0, Y, TXTSprite.Bitmap.Width, HeightToClear, Color.ALPHA);
        int movy = Y + HeightToClear;
        int movh = BGSprite.Bitmap.Height - movy;
        int shift = DeleteChildren ? HeightToClear : LineHeight;
        BGSprite.Bitmap.ShiftVertically(movy, movh, -shift, true);
        TXTSprite.Bitmap.ShiftVertically(movy, movh, -shift, true);
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
        int NewWidth = SpriteContainer.Size.Width - ExtraXScrollArea;
        BGSprite.Bitmap = BGSprite.Bitmap.Resize(NewWidth, BGSprite.Bitmap.Height - shift);
        TXTSprite.Bitmap = TXTSprite.Bitmap.Resize(NewWidth, TXTSprite.Bitmap.Height - shift);
        bool wasRelocked = false;
        UpdateSize(false);
        TXTSprite.Bitmap.Font = this.Font;
        TXTSprite.Bitmap.BlendMode = BlendMode.Addition;
        TXTSprite.Bitmap.Relock();
        int Index = LastDrawData.FindIndex(d => d.Y >= Y);
        for (int i = Index; i < LastDrawData.Count; i++)
        {
            if (LastDrawData[i].Y >= Y && LastDrawData[i].Y < Y + HeightToClear)
            {
                LastDrawData.RemoveAt(i);
                i--;
            }
            else LastDrawData[i] = (LastDrawData[i].Node, LastDrawData[i].Y - shift);
        }
        ITreeNode NodeToSelect = null;
        if (!DeleteChildren && Node is TreeNode && ((TreeNode) Node).HasChildren)
        {
            // We flattened the children to the node's parent, so now we have to redraw all of them to correct the depth
            BGSprite.Bitmap.Unlock();
            TXTSprite.Bitmap.Unlock();
            ((TreeNode) Node).GetAllChildren(false).ForEach(n =>
            {
                Y = RedrawNode(n, Y, true, () => Index);
                Index++;
            });
            wasRelocked = true;
            BGSprite.Bitmap.Lock();
            TXTSprite.Bitmap.Lock();
            NodeToSelect = ((TreeNode) Node).Children[0];
        }
        else if (NextSibling != null) NodeToSelect = NextSibling;
        else if (PreviousSibling != null) NodeToSelect = PreviousSibling;
        else NodeToSelect = Parent;
        if (ChildIndex == Parent.Children.Count) // We deleted the last node in the parent's list of nodes
        {
            BGSprite.Bitmap.Unlock();
            TXTSprite.Bitmap.Unlock();
            if (Parent.Children.Count == 0)
            {
                // If our parent no longer has any children, we redraw the parent to get rid of the collapse box
                BGSprite.Bitmap.FillRect(0, GetDrawnYCoord(Parent), BGSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                TXTSprite.Bitmap.FillRect(0, GetDrawnYCoord(Parent), TXTSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                RedrawNode(Parent, GetDrawnYCoord(Parent), false);
            }
            else if (OldDepth > 1) // Don't remove lines when we're at the root node, because the root node doesn't draw lines
            {
                // If we still have children and we deleted the last node, that means the line from our parent should stop
                // at an earlier point that it did before, which means we need to redraw our previous sibling (and all its children)
                // to get the proper line to show up.
                // Or if we're smart about it, similarly to what we did for inserting nodes, we can delete just the line, since we know its start and end point.
                // We can simply delete the part of the line that's no longer accurate and save ourselves the trouble of redrawing god knows how many nodes.
                ITreeNode prevNode = Parent.Children[ChildIndex - 1];
                int x = (OldDepth - 1) * DepthIndent + XOffset;
                int sy = GetDrawnYCoord(prevNode) + 12;
                int ey = movy - shift - 1;
                BGSprite.Bitmap.DrawLine(x + 19 - DepthIndent, sy, x + 19 - DepthIndent, ey, Color.ALPHA);
            }
            wasRelocked = true;
            BGSprite.Bitmap.Lock();
            TXTSprite.Bitmap.Lock();
        }
        if (NodeToSelect == null && !Empty) throw new Exception("Did not find a new node to select despite the tree not being empty");
        // If we still have our currently selected node, we don't need to change our selected node
        if (NodeToSelect == this.Root)
        {
            if (this.Root.HasChildren) NodeToSelect = this.Root.Children[0];
            else NodeToSelect = null;
        }
        if (!this.Root.Contains(SelectedNode)) SetSelectedNode(NodeToSelect, false);
        if (!wasRelocked) BGSprite.Bitmap.Relock();
        if (DeleteChildren && Node is TreeNode)
        {
            List<ITreeNode> List = ((TreeNode) Node).GetAllChildren(true);
            List.Insert(0, Node);
            return List;
        }
        return new List<ITreeNode>() { Node };
    }

    /// <summary>
    /// Redraws the entire tree.
    /// </summary>
    public void RedrawAllNodes()
    {
        LastDrawData.Clear();
        BGSprite.Bitmap?.Dispose();
        TXTSprite.Bitmap?.Dispose();
        if (Root.Children.Count == 0) return;
        (int RootNodeCount, int RootSepHeight) = Root.GetChildrenHeight(false);
        int MaxWidth = CalculateMaxWidth(Root) + ExtraXScrollArea;
        BGSprite.Bitmap = new Bitmap(MaxWidth, RootNodeCount * LineHeight + RootSepHeight, Graphics.MaxTextureSize.Width, 1024);
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap = new Bitmap(MaxWidth, RootNodeCount * LineHeight + RootSepHeight, Graphics.MaxTextureSize.Width, 1024);
        TXTSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Font = this.Font;
        UpdateSize(false); // No need to recalculate width as we just calculated it to find the bitmap width
        List<ITreeNode> nodes = Root.GetAllChildren(false);
        int y = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            y = RedrawNode(nodes[i], y);
        }
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
        UpdateSize(false);
    }

    /// <summary>
    /// Redraws a single node and its children. This may only be called if its height did not change (i.e. no children were added or removed).
    /// </summary>
    /// <param name="node">The node to redraw.</param>
    public void RedrawNode(ITreeNode node)
    {
        int y = GetDrawnYCoord(node);
        int h = 0;
        int dataIdx = LastDrawData.FindIndex(it => it.Node == node);
        LastDrawData.RemoveAt(dataIdx);
        if (node is TreeNode)
        {
            (int count, int sepHeight) = ((TreeNode) node).GetChildrenHeight(false);
            h = (count + 1) * LineHeight + sepHeight;
        }
        else h = LineHeight;
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
        BGSprite.Bitmap.FillRect(0, y, BGSprite.Bitmap.Width, h, Color.ALPHA);
        TXTSprite.Bitmap.FillRect(0, y, TXTSprite.Bitmap.Width, h, Color.ALPHA);
        y = RedrawNode(node, y, true, () => dataIdx);
        if (node is TreeNode)
        {
            List<ITreeNode> children = ((TreeNode) node).GetAllChildren(false);
            dataIdx++;
            LastDrawData.RemoveRange(dataIdx, children.Count);
            children.ForEach(n =>
            {
                y = RedrawNode(n, y, true, () => dataIdx);
                dataIdx++;
            });
        }
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
    }

    /// <summary>
    /// Redraws a single node.
    /// </summary>
    /// <param name="Node">The node to redraw.</param>
    /// <param name="y">The y position to draw the node at.</param>
    /// <param name="AddData">Whether to add the draw data to <see cref="LastDrawData"/>.</param>
    /// <param name="IndexProvider">A function that returns the index in <see cref="LastDrawData"/> to insert the data at if <paramref name="AddData"/> is true.</param>
    /// <returns></returns>
    private int RedrawNode(ITreeNode Node, int y, bool AddData = true, Func<int> IndexProvider = null)
    {
        int x = (Node.Depth - 1) * DepthIndent + XOffset;
        if (Node.Parent != Root)
        {
            ITreeNode Current = Node;
            while (Current.Parent != null && Current.Parent != Root)
            {
                TreeNode RCurr = Current as TreeNode;
                if (Current is not TreeNode || RCurr.GetNextSibling() != null)
                {
                    int px = (Current.Parent.Depth - 1) * DepthIndent + XOffset;
                    BGSprite.Bitmap.DrawLine(px + 19, y, px + 19, y + LineHeight - 1, new Color(64, 104, 146));
                }
                Current = Current.Parent;
            }
            BGSprite.Bitmap.DrawLine(x + 19 - DepthIndent, y, x + 19 - DepthIndent, y + 11, new Color(64, 104, 146));
            BGSprite.Bitmap.DrawLine(x, y + 11, x + 13, y + 11, new Color(64, 104, 146));
        }
        if (Node is TreeNodeSeparator)
        {
            TreeNodeSeparator sep = (TreeNodeSeparator) Node;
            if (AddData) LastDrawData.Add((sep, y));
            y += sep.Height;
        }
        else
        {
            TreeNode RNode = (TreeNode) Node;
            if (RNode.Children.Count > 0)
            {
                int sx = RNode.Expanded ? 11 : 0;
                BGSprite.Bitmap.Build(new Rect(x + 14, y + 6, 11, 11), TreeIconsBitmap, new Rect(sx, 0, 11, 11), BlendMode.None);
                if (RNode.Expanded) BGSprite.Bitmap.DrawLine(x + 19, y + 17, x + 19, y + LineHeight - 1, new Color(64, 104, 146));
            }
            bool sel = SelectedNodes.Contains(RNode);
			TXTSprite.Bitmap.DrawText(RNode.Text, x + 30, y + LineHeight / 2 - 10, sel ? new Color(55, 187, 255) : Color.WHITE);
            if (AddData)
            {
                if (IndexProvider != null)
                {
                    int idx = IndexProvider();
                    LastDrawData.Insert(idx, (Node, y));
                }
                else LastDrawData.Add((Node, y));
            }
            y += LineHeight;
        }
        return y;
    }

    /// <summary>
    /// Redraws the text of a node.
    /// </summary>
    /// <param name="Node">The node to redraw the text of.</param>
    public void RedrawNodeText(TreeNode Node, bool LockBitmaps = true)
    {
        if (LockBitmaps) TXTSprite.Bitmap.Unlock();
        int x = (Node.Depth - 1) * DepthIndent + XOffset;
        int y = GetDrawnYCoord(Node);
        TXTSprite.Bitmap.FillRect(0, y, TXTSprite.Bitmap.Width, LineHeight, Color.ALPHA);
        TXTSprite.Bitmap.DrawText(Node.Text, x + 30, y + LineHeight / 2 - 10, SelectedNodes.Contains(Node) ? new Color(55, 187, 255) : Color.WHITE);
        if (LockBitmaps) TXTSprite.Bitmap.Lock();
    }

    /// <summary>
    /// Determines the y position of a drawn node.
    /// </summary>
    /// <param name="Node">The node to match with.</param>
    /// <returns>The y position of the drawn node.</returns>
    public int GetDrawnYCoord(ITreeNode Node)
    {
        (_, int Y) = LastDrawData.Find(d => d.Node == Node);
        return Y;
    }

    /// <summary>
    /// Clears all selected nodes.
    /// </summary>
    public void ClearSelection(ITreeNode? exceptFor = null, bool LockBitmaps = true)
    {
        if (LockBitmaps && BGSprite.Bitmap is not null && !BGSprite.Bitmap.Disposed)
        {
            BGSprite.Bitmap.Unlock();
            TXTSprite.Bitmap.Unlock();
        }
        for (int i = 0; i < SelectionSprites.Count; i++)
        {
            if (SelectionSprites[i].Node == exceptFor) continue;
            SpriteContainer.Sprites[$"sel_{SelectionSprites[i].SpriteIndex}"].Dispose();
            SpriteContainer.Sprites.Remove($"sel_{SelectionSprites[i].SpriteIndex}");
            SelectionSprites.RemoveAt(i);
            i--;
        }
        for (int i = 0; i < SelectedNodes.Count; i++)
        {
            ITreeNode n = SelectedNodes[i];
            if (n == exceptFor) continue;
            SelectedNodes.RemoveAt(i);
            i--;
            if (n.Root == n) continue; // This node was deleted
            if (n is not TreeNode) continue;
            // Only redraw the node if the root has children
            if (Root.HasChildren) RedrawNodeText((TreeNode) n, false);
        }
        if (LockBitmaps && BGSprite.Bitmap is not null && !BGSprite.Bitmap.Disposed)
        {
            BGSprite.Bitmap.Lock();
            TXTSprite.Bitmap.Lock();
        }
    }

    /// <summary>
    /// Deselects a single node.
    /// </summary>
    /// <param name="node">The node to deselect.</param>
    public void DeselectNode(ITreeNode node)
    {
        for (int i = 0; i < SelectionSprites.Count; i++)
        {
            if (SelectionSprites[i].Node == node)
            {
                SpriteContainer.Sprites[$"sel_{SelectionSprites[i].SpriteIndex}"].Dispose();
                SpriteContainer.Sprites.Remove($"sel_{SelectionSprites[i].SpriteIndex}");
                SelectionSprites.RemoveAt(i);
                break;
            }
        }
        for (int i = 0; i < SelectedNodes.Count; i++)
        {
            if (SelectedNodes[i] == node)
            {
                SelectedNodes.RemoveAt(i);
                if (node is TreeNode) RedrawNodeText((TreeNode) node);
                break;
            }
        }
        OnSelectionChanged?.Invoke(new BoolEventArgs(false));
    }

    /// <summary>
    /// Expands all ancestors of a node to make the node visible.
    /// </summary>
    /// <param name="Node">The node to make visible.</param>
    private void ExpandUpTo(ITreeNode Node)
    {
        List<TreeNode> Ancestors = Node.GetAncestors();
        // Start at 1 to skip the root node, which is always expanded
        for (int i = 1; i < Ancestors.Count; i++)
        {
            if (!Ancestors[i].Expanded) SetExpanded(Ancestors[i], true);
        }
    }

    /// <summary>
    /// Selects an individual node.
    /// </summary>
    /// <param name="Node">The node to select.</param>
    private void SelectIndividualNode(ITreeNode Node, bool LockBitmaps = true)
    {
        ExpandUpTo(Node);
        int i = 0;
        while (SpriteContainer.Sprites.ContainsKey($"sel_{i}")) i++;
        SpriteContainer.Sprites[$"sel_{i}"] = new Sprite(SpriteContainer.Viewport);
        int height = Node is TreeNodeSeparator ? ((TreeNodeSeparator) Node).Height : LineHeight;
        SpriteContainer.Sprites[$"sel_{i}"].Bitmap = new SolidBitmap(SpriteContainer.Size.Width, height, new Color(28, 50, 73));
        int y = GetDrawnYCoord(Node);
        SelectedNodes.Add(Node);
        SpriteContainer.Sprites[$"sel_{i}"].Y = y;
        SpriteContainer.UpdateBounds();
        SelectionSprites.Add((Node, i));
        if (Node is TreeNode) RedrawNodeText((TreeNode) Node, LockBitmaps);
    }

    /// <summary>
    /// Updates a node's selection sprite.
    /// </summary>
    /// <param name="Node">The node to match with.</param>
    private void UpdateSelection(ITreeNode Node)
    {
        int i = SelectionSprites.Find(s => s.Node == Node).SpriteIndex;
        SpriteContainer.Sprites[$"sel_{i}"].Y = GetDrawnYCoord(Node);
    }

    /// <summary>
    /// Selects a node.
    /// </summary>
    /// <param name="Node">The node to select.</param>
    /// <param name="AllowMultiple">Whether multiple nodes may be selected.</param>
    /// <param name="DoubleClicked">Whether the node was double clicked.</param>
    /// <param name="InvokeEvent">Whether to invoke the <see cref="OnSelectionChanged"/> event.</param>
    public void SetSelectedNode(ITreeNode Node, bool AllowMultiple, bool DoubleClicked = true, bool InvokeEvent = true, bool LockBitmaps = true)
    {
        if (!AllowMultiple) ClearSelection(null, LockBitmaps);
        if (Node != null) SelectIndividualNode(Node, LockBitmaps);
        if (InvokeEvent) OnSelectionChanged?.Invoke(new BoolEventArgs(DoubleClicked));
    }

    /// <summary>
    /// Sets a node as being hovered over.
    /// </summary>
    /// <param name="Node">The node to set as hovered.</param>
    public void SetHoveringNode(ITreeNode Node)
    {
        DragState = null;
        this.HoveringNode = Node;
        if (Node != null && Node.Selectable)
        {
            int Y = GetDrawnYCoord(this.HoveringNode);
            int Height = this.HoveringNode is TreeNodeSeparator ? ((TreeNodeSeparator) this.HoveringNode).Height : LineHeight;
            SpriteContainer.Sprites["hover"].Y = Y;
            ((SolidBitmap) SpriteContainer.Sprites["hover"].Bitmap).SetSize(2, Height);
            SpriteContainer.Sprites["hover"].Visible = !Dragging || !ValidatedDragMovement;
        }
        else
        {
            SpriteContainer.Sprites["hover"].Visible = false;
        }
    }

    /// <summary>
    /// Called whenever the size of the widget changes.
    /// </summary>
    /// <param name="e"></param>
    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (this.Empty) ScrollContainer.SetSize(Size);
        else UpdateSize(false); // The max width of the tree itself does not depend on the size of this widget
    }

    /// <summary>
    /// Called to update the size of the tree containers.
    /// </summary>
    /// <param name="Recalculate">Whether to recalculate the maximum width for potential horizontal scrolling.</param>
    void UpdateSize(bool Recalculate = true)
    {
        int OldWidth = SpriteContainer.Size.Width;
        if (ScrollContainer.HScrollBar.Visible)
        {
            ScrollContainer.VScrollBar.SetPadding(VScrollBarPaddingShared);
            ScrollContainer.SetHeight(Size.Height - 12);
        }
        else
        {
            ScrollContainer.VScrollBar.SetPadding(VScrollBarPaddingAlone);
            ScrollContainer.SetHeight(Size.Height - (VResizeToFill ? 0 : 12));
        }
        if (ScrollContainer.VScrollBar.Visible)
        {
            ScrollContainer.HScrollBar.SetPadding(HScrollBarPaddingShared);
            ScrollContainer.SetWidth(Size.Width - 12);
        }
        else
        {
            ScrollContainer.HScrollBar.SetPadding(HScrollBarPaddingAlone);
            ScrollContainer.SetWidth(Size.Width - (HResizeToFill ? 0 : 12));
        }
        int w = Recalculate ? CalculateMaxWidth(Root) : BGSprite.Bitmap.Width;
        w += ExtraXScrollArea;
        if (w < ScrollContainer.Size.Width) w = ScrollContainer.Size.Width;
        SpriteContainer.SetSize(w, BGSprite.Bitmap.Height + ExtraYScrollArea);
        if (OldWidth != SpriteContainer.Size.Width)
        {
            // Resize all selection sprites
            SelectionSprites.ForEach(s =>
            {
                SolidBitmap bmp = (SolidBitmap) SpriteContainer.Sprites[$"sel_{s.SpriteIndex}"].Bitmap;
                bmp.SetSize(w, bmp.BitmapHeight);
            });
        }
        if (!AutoResize)
        {
            ScrollContainer.SetSize(SpriteContainer.Size);
            this.SetSize(SpriteContainer.Size);
        }
    }

    /// <summary>
    /// Called whenever the mouse is moving.
    /// </summary>
    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        ITreeNode OldHoveringNode = this.HoveringNode;
        DragStates? OldDragState = this.DragState;
        DragLineOffset = 0;
        SetHoveringNode(null);
        if (!SpriteContainer.Mouse.Inside || ScrollContainer.HScrollBar.Mouse.Inside || ScrollContainer.VScrollBar.Mouse.Inside)
        {
            RedrawDragState();
            return;
        }
        int rx = e.X - Viewport.X + SpriteContainer.LeftCutOff;
        int ry = e.Y - Viewport.Y + SpriteContainer.TopCutOff;
        float yfraction = 0f;
        for (int i = 0; i < LastDrawData.Count; i++)
        {
            ITreeNode Node = LastDrawData[i].Node;
            int ny = LastDrawData[i].Y;
            int realheight = Node is TreeNodeSeparator ? ((TreeNodeSeparator) Node).Height : LineHeight;
            if (ry >= ny && ry < ny + realheight)
            {
                SetHoveringNode(Node);
                yfraction = (float) (ry - ny) / (realheight - 1);
                break;
            }
        }
        if (Dragging && !ValidatedDragMovement && CanDragAndDrop)
        {
            Point mp = new Point(rx, ry);
            if (DragOriginPoint.Distance(mp) >= 10)
            {
                ValidatedDragMovement = true;
                PreDragDropRootNode = (TreeNode) this.Root.Clone();
            }
            this.OnDragAndDropping?.Invoke(new GenericObjectEventArgs<ITreeNode>(this.ActiveNode));
        }
        // Drag-and-drop
        if (Dragging && ValidatedDragMovement && HoveringNode != null)
        {
            bool CanDragOver = this.HoveringNode.CanDragOver;
            bool TopArea = !CanDragOver && yfraction < 0.5f || yfraction < 1f / 4;
            bool MiddleArea = CanDragOver && yfraction < 3f / 4;
            bool BottomArea = !TopArea && !MiddleArea;
            // Void this position if the hovering node is contained by the active node,
            // or if we're still on the active node
            if (ActiveNode is TreeNode && ((TreeNode) ActiveNode).Contains(HoveringNode, true) ||
                ActiveNode == HoveringNode)
            {
                RedrawDragState();
                return;
            }
            if (TopArea)
            {
                this.DragState = DragStates.SharedAbove;
            }
            else if (MiddleArea)
            {
                this.DragState = DragStates.Over;
            }
            else if (BottomArea)
            {
                this.DragState = DragStates.Below;
                ITreeNode? NextNode = null;
                if (this.HoveringNode is TreeNode) NextNode = ((TreeNode) this.HoveringNode).GetNextNode();
                else
                {
                    // Use the draw data to find the next node if we're hovering over a non-node.
                    int Index = LastDrawData.FindIndex(i => i.Node == this.HoveringNode);
                    if (Index < LastDrawData.Count - 1) NextNode = LastDrawData[Index + 1].Node;
                }
                if (NextNode == null || this.HoveringNode.Parent == NextNode.Parent) // If the next node is a sibling, share the line
                    this.DragState = DragStates.SharedBelow;
                if (this.HoveringNode is TreeNode && ((TreeNode) this.HoveringNode).HasChildren &&
                    ((TreeNode) this.HoveringNode).Children[0] == NextNode) // If the next node is the first child, share the line
                {
                    this.DragState = DragStates.SharedBelow;
                    DragLineOffset = DepthIndent;
                }
            }
            if (this.DragState != OldDragState || this.HoveringNode != OldHoveringNode)
            {
                RedrawDragState();
            }
        }
    }

    /// <summary>
    /// Redraws the drag-and-drop graphics.
    /// </summary>
    private void RedrawDragState()
    {
        SpriteContainer.Sprites["drag"].Bitmap?.Dispose();
        if (DragState != null)
        {
            int x = this.HoveringNode.Depth * DepthIndent + DragLineOffset;
            int y = GetDrawnYCoord(this.HoveringNode);
            int height = this.HoveringNode is TreeNodeSeparator ? ((TreeNodeSeparator) this.HoveringNode).Height : LineHeight;
            if (this.DragState == DragStates.Above || this.DragState == DragStates.Below ||
                this.DragState == DragStates.SharedAbove || this.DragState == DragStates.SharedBelow)
            {
                // Single line between two nodes
                SpriteContainer.Sprites["drag"].X = x;
                SpriteContainer.Sprites["drag"].Y = y + 2;
                switch (this.DragState)
                {
                    case DragStates.Below:
                        SpriteContainer.Sprites["drag"].Y = y + height - 2;
                        break;
                    case DragStates.SharedBelow:
                        SpriteContainer.Sprites["drag"].Y = y + height;
                        break;
                    case DragStates.SharedAbove:
                        SpriteContainer.Sprites["drag"].Y = y;
                        break;
                }
                int width = Math.Max(100, Size.Width - x - 10);
                SpriteContainer.Sprites["drag"].Bitmap = new SolidBitmap(width, 1, new Color(55, 187, 255));
            }
            else if (this.DragState == DragStates.Over)
            {
                // Over one node
                if (this.HoveringNode is TreeNode)
                    SpriteContainer.Sprites["drag"].X = x + TXTSprite.Bitmap.Font.TextSize(((TreeNode) this.HoveringNode).Text).Width + 30;
                else SpriteContainer.Sprites["drag"].X = 4;
                SpriteContainer.Sprites["drag"].Y = y + height / 2 - 3;
                SpriteContainer.Sprites["drag"].Bitmap = new Bitmap(7, 7);
                SpriteContainer.Sprites["drag"].Bitmap = new Bitmap(7, 7);
                SpriteContainer.Sprites["drag"].Bitmap.Unlock();
                Color c = new Color(55, 187, 255);
                SpriteContainer.Sprites["drag"].Bitmap.DrawLine(2, 0, 6, 0, c);
                SpriteContainer.Sprites["drag"].Bitmap.DrawLine(2, 0, 2, 6, c);
                SpriteContainer.Sprites["drag"].Bitmap.DrawLine(0, 4, 4, 4, c);
                SpriteContainer.Sprites["drag"].Bitmap.SetPixel(1, 5, c);
                SpriteContainer.Sprites["drag"].Bitmap.SetPixel(3, 5, c);
                SpriteContainer.Sprites["drag"].Bitmap.Lock();
                // Check if the hover-over icon is off-screen, and adjust our horizontal scroll if it is.
                int visx = SpriteContainer.LeftCutOff;
                int visw = ScrollContainer.Size.Width;
                if (SpriteContainer.Sprites["drag"].X < visx)
                {
                    ScrollContainer.ScrolledX = SpriteContainer.Sprites["drag"].X - 4;
                    ScrollContainer.UpdateAutoScroll();
                }
                else if (SpriteContainer.Sprites["drag"].X > visx + visw - 11)
                {
                    ScrollContainer.ScrolledX += SpriteContainer.Sprites["drag"].X - (visx + visw - 11);
                    ScrollContainer.UpdateAutoScroll();
                }
            }
        }
    }

    /// <summary>
    /// Called whenever the mouse is left-clicked inside the widget.
    /// </summary>
    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        this.WidgetSelected(new BaseEventArgs());
        this.ActiveNode = HoveringNode;
        if (HoveringNode != null && HoveringNode.Draggable && CanDragAndDrop)
        {
            this.Dragging = true;
            int rx = e.X - Viewport.X + SpriteContainer.LeftCutOff;
            int ry = e.Y - Viewport.Y + SpriteContainer.TopCutOff;
            this.DragOriginPoint = new Point(rx, ry);
        }
    }

    /// <summary>
    /// Called whenever a left-mouse click is released. This finalizes drag-and-drop actions, among other things.
    /// </summary>
    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (this.Dragging && this.ValidatedDragMovement)
        {
            if (this.ActiveNode != this.HoveringNode && // Drag-and-dropping over different node
                (this.ActiveNode is not TreeNode || !((TreeNode) this.ActiveNode).Contains(this.HoveringNode))) // If the hovered node is a child of the active node, we can't make active node a child of the hovering node.
            {
                if (this.DragState == DragStates.Over && this.HoveringNode is TreeNode) // If the hovered node is a node capable of having children
                {
                    DeleteNode(this.ActiveNode, true);
                    InsertNode((TreeNode) this.HoveringNode, null, this.ActiveNode);
                    this.OnDragAndDropped?.Invoke(new GenericObjectEventArgs<(ITreeNode, TreeNode, TreeNode)>((this.ActiveNode, PreDragDropRootNode, this.Root)));
                }
                else if (this.DragState == DragStates.Above || this.DragState == DragStates.SharedAbove)
                {
                    DeleteNode(this.ActiveNode, true);
                    int HoveredIndex = this.HoveringNode.Parent.Children.IndexOf(this.HoveringNode);
                    InsertNode(this.HoveringNode.Parent, HoveredIndex, this.ActiveNode);
                    this.OnDragAndDropped?.Invoke(new GenericObjectEventArgs<(ITreeNode, TreeNode, TreeNode)>((this.ActiveNode, PreDragDropRootNode, this.Root)));
                }
                else if (this.DragState == DragStates.Below || this.DragState == DragStates.SharedBelow)
                {
                    DeleteNode(this.ActiveNode, true);
                    // If we're below a node with visible children, then we want to insert the node as its first child rather than a sibling.
                    if (this.DragState == DragStates.SharedBelow && this.HoveringNode is TreeNode &&
                        ((TreeNode) this.HoveringNode).HasChildren && ((TreeNode) this.HoveringNode).Expanded)
                    {
                        InsertNode((TreeNode) this.HoveringNode, 0, this.ActiveNode);
                    }
                    else
                    {
                        int HoveredIndex = this.HoveringNode.Parent.Children.IndexOf(this.HoveringNode);
                        InsertNode(this.HoveringNode.Parent, HoveredIndex + 1, this.ActiveNode);
                    }
                    this.OnDragAndDropped?.Invoke(new GenericObjectEventArgs<(ITreeNode, TreeNode, TreeNode)>((this.ActiveNode, PreDragDropRootNode, this.Root)));
                }
                if (this.SelectedNode != null) UpdateSelection(this.SelectedNode);
            }
        }
        else if (this.ActiveNode != null && this.ActiveNode.Selectable && this.ActiveNode == this.HoveringNode)
        {
            int rx = e.X - Viewport.X + SpriteContainer.LeftCutOff;
            int ry = e.Y - Viewport.Y + SpriteContainer.TopCutOff;
            int NodeX = (this.ActiveNode.Depth - 1) * DepthIndent + XOffset;
            int NodeY = GetDrawnYCoord(ActiveNode);
            if (ActiveNode is TreeNode && rx >= NodeX + 14 && rx < NodeX + 25 && ry >= NodeY + 6 && ry < NodeY + 17)
            {
                SetExpanded((TreeNode) ActiveNode, !((TreeNode) ActiveNode).Expanded);
            }
            else
            {
                if (CanMultiSelect && Input.Press(Keycode.SHIFT))
                {
                    // Mark the anchor of the shift region selection
                    SelectionAnchor ??= this.SelectedNode;
                    List<ITreeNode> nodesInSelection = GetNodeRange(SelectionAnchor, this.ActiveNode);
                    BGSprite.Bitmap.Unlock();
                    TXTSprite.Bitmap.Unlock();
                    ClearSelection(null, false);
                    SetSelectedNode(SelectionAnchor, true, true, false, false);
                    nodesInSelection.ForEach(n => SetSelectedNode(n, true, true, false, false));
                    SetSelectedNode(this.ActiveNode, true, true, true, false);
                    BGSprite.Bitmap.Lock();
                    TXTSprite.Bitmap.Lock();
                }
                else
                {
                    if (CanMultiSelect && Input.Press(Keycode.CTRL))
                    {
                        SelectionAnchor = this.ActiveNode;
                        if (SelectedNodes.Contains(this.ActiveNode)) DeselectNode(this.ActiveNode);
                        else SetSelectedNode(this.ActiveNode, true, false);
                    }
                    else
                    {
                        SelectionAnchor = null;
                        if (TimerExists("double_click") && !TimerPassed("double_click"))
                        {
                            SetSelectedNode(this.ActiveNode, false, this.ActiveNode == DoubleClickNode); // Double click is only valid if the current node is the same node that we pressed last time
                            DoubleClickNode = null;
                            DestroyTimer("double_click");
                        }
                        else
                        {
                            if (TimerExists("double_click")) DestroyTimer("double_click");
                            SetTimer("double_click", 300);
                            SetSelectedNode(this.ActiveNode, false, false);
                            DoubleClickNode = this.ActiveNode;
                        }
                    }
                }
            }
        }
        this.Dragging = false;
        this.ActiveNode = null;
        this.DragState = null;
        this.DragLineOffset = 0;
        this.DragOriginPoint = null;
        if (this.ValidatedDragMovement) RedrawDragState();
        this.ValidatedDragMovement = false;
        if (Mouse.LeftStartedInside) this.WidgetSelected(new BaseEventArgs());
	}

    private ITreeNode? SelectionAnchor;

    private List<ITreeNode> GetNodeRange(ITreeNode startNode, ITreeNode endNode)
    {
        if (startNode == endNode) return new List<ITreeNode>();
        List<ITreeNode> allNodes = Root.GetAllChildren(true);
        List<ITreeNode> nodeRange = new List<ITreeNode>();
        ITreeNode? finishNode = null;
        foreach (ITreeNode node in allNodes)
        {
            if (finishNode is not null && node == finishNode) break;
            if (node == startNode) finishNode = endNode;
            else if (node == endNode) finishNode = startNode;
            else if (finishNode is not null) nodeRange.Add(node);
        }
        return nodeRange;
    }

    /// <summary>
    /// Called every tick to update various states.
    /// </summary>
    public override void Update()
    {
        base.Update();
        if (TimerExists("double_click") && TimerPassed("double_click"))
        {
            DestroyTimer("double_click");
            DoubleClickNode = null;
        }
        if (OldHoveringNode != HoveringNode)
        {
            if (TimerExists("long_hover")) DestroyTimer("long_hover");
        }
        if (Dragging && ValidatedDragMovement && (ActiveNode is not TreeNode || !((TreeNode) ActiveNode).Contains(HoveringNode)) && HoveringNode is TreeNode)
        {
            TreeNode HovNode = (TreeNode) HoveringNode;
            bool MayExpand = !HovNode.Expanded && HovNode.HasChildren && this.DragState == DragStates.Over;
            if (MayExpand && !TimerExists("long_hover"))
            {
                SetTimer("long_hover", 500);
            }
            else if (MayExpand && TimerPassed("long_hover"))
            {
                DestroyTimer("long_hover");
                SetExpanded(HovNode, true);
            }
            else if (TimerExists("long_hover") && (HovNode.Expanded || !HovNode.HasChildren || this.DragState != DragStates.Over))
            {
                DestroyTimer("long_hover");
            }
        }
        else
        {
            if (TimerExists("long_hover")) DestroyTimer("long_hover");
        }
        if (TimerExists("idle") && TimerPassed("idle"))
        {
            // Make query selection the real selection
            if (queryNode is not null && !string.IsNullOrEmpty(query))
            {
                query = "";
                queryNode = null;
                DestroyTimer("idle");
                OnSelectionChanged?.Invoke(new BoolEventArgs());
                WidgetSelected(new BaseEventArgs());
            }
        }
        if (Input.Trigger(Keycode.ESCAPE) && this.Dragging)
        {
            this.Dragging = false;
            this.ActiveNode = null;
            this.DragState = null;
            this.DragOriginPoint = null;
            if (ValidatedDragMovement) RedrawDragState();
            this.ValidatedDragMovement = false;
        }
        OldHoveringNode = HoveringNode;
    }

    /// <summary>
    /// Selects a node that matches the query.
    /// </summary>
    /// <param name="query">The query to match.</param>
    protected void SelectQuery(string query)
    {
        query = query.ToLower();
        List<TreeNode> matches = Root.GetAllChildren(true).FindAll(n =>
        {
            if (n is TreeNode) return ((TreeNode) n).Selectable && ((TreeNode) n).Text.ToLower().StartsWith(query);
            return false;
        }).Cast<TreeNode>().ToList();
        if (matches.Count == 0) return; // Play fail sound?
        queryNode = matches[0];
        SetSelectedNode(queryNode, false, false, false);
        CenterOnNode(queryNode);
    }
}

/// <summary>
/// The states in which drag-and-drop can be released.
/// </summary>
enum DragStates
{
    Above,
    Over,
    Below,
    SharedAbove,
    SharedBelow
}