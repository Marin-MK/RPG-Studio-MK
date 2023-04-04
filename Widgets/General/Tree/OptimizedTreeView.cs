using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class OptimizedTreeView : Widget
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

    // TODO:
    // - Deselect a node if it was selected and it is selected again with control down
    // - Select first visible parent node if a node is collapsed and the one single selected node is within that tree
    // - Deselect all selected nodes within a node tree if that node is collapsed

    static Bitmap TreeIconsBitmap;

    public OptimizedNode Root { get; protected set; }
    public int LineHeight { get; protected set; } = 24;
    public int DepthIndent { get; protected set; } = 20;
    public int XOffset { get; protected set; } = 6;
    public int ExtraXScrollArea { get; protected set; } = 0;
    public int ExtraYScrollArea { get; protected set; } = 0;
    public IOptimizedNode HoveringNode { get; protected set; }
    public List<IOptimizedNode> SelectedNodes { get; protected set; } = new List<IOptimizedNode>();
    public bool MultipleSelected => SelectedNodes.Count > 1;
    public IOptimizedNode SelectedNode => SelectedNodes.Count > 0 ? SelectedNodes[0] : null;
    public bool DragAndDrop { get; protected set; } = true;
    public bool Empty => !Root.HasChildren;
    public bool RequireSelection { get; protected set; } = true;
    public Font Font { get; protected set; }
    public Padding HScrollBarPaddingAlone { get; protected set; } = new Padding(1, 0, 1, -1);
    public Padding HScrollBarPaddingShared { get; protected set; } = new Padding(1, 0, 13, -1);
    public Padding VScrollBarPaddingAlone { get; protected set; } = new Padding(0, 1, -1, 1);
    public Padding VScrollBarPaddingShared { get; protected set; } = new Padding(0, 1, -1, 13);
    public bool HResizeToFill { get; protected set; } = false;
    public bool VResizeToFill { get; protected set; } = true;

    public GenericObjectEvent<IOptimizedNode> OnDragAndDropping;
    public GenericObjectEvent<(IOptimizedNode DroppedNode, OptimizedNode OldRoot, OptimizedNode NewRoot)> OnDragAndDropped;
    public BoolEvent OnSelectionChanged;
    public GenericObjectEvent<OptimizedNode> OnNodeExpansionChanged;
    public GenericObjectEvent<OptimizedNode> OnNodeGlobalIndexChanged;

    private List<(IOptimizedNode Node, int Y)> LastDrawData = new List<(IOptimizedNode, int)>();
    private List<(IOptimizedNode Node, int SpriteIndex)> SelectionSprites = new List<(IOptimizedNode, int)>();

    private Container ScrollContainer;
    private Container SpriteContainer;
    private Sprite BGSprite => SpriteContainer.Sprites["bg"];
    private Sprite TXTSprite => SpriteContainer.Sprites["txt"];

    private IOptimizedNode? ActiveNode;
    private bool Dragging = false;
    private DragStates? DragState;
    private int DragLineOffset = 0;
    private Point? DragOriginPoint;
    private bool ValidatedDragMovement = false;
    private IOptimizedNode OldHoveringNode;
    private IOptimizedNode? DoubleClickNode;
    private OptimizedNode? PreDragDropRootNode;

    public OptimizedTreeView(IContainer Parent) : base(Parent)
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
        this.Root = new OptimizedNode("ROOT");

        this.OnContextMenuOpening += e => e.Value = !ScrollContainer.HScrollBar.Mouse.Inside && !ScrollContainer.VScrollBar.Mouse.Inside;
    }

    public void SetRootNode(OptimizedNode Root, IOptimizedNode? SelectedNode = null)
    {
        if (this.Root != Root)
        {
            this.Root = Root;
            this.Root.GetAllChildren(true).ForEach(n =>
            {
                if (n is OptimizedNode)
                {
                    OptimizedNode Node = (OptimizedNode) n;
                    Node.OnGlobalIndexChanged = _ => OnNodeGlobalIndexChanged?.Invoke(new GenericObjectEventArgs<OptimizedNode>(Node));
                }
            });
            RedrawAllNodes();
            if (RequireSelection) SetSelectedNode(SelectedNode ?? (Root.HasChildren ? Root.Children[0] : null), false);
        }
    }

    public void SetLineHeight(int LineHeight)
    {
        if (this.LineHeight != LineHeight)
        {
            this.LineHeight = LineHeight;
            RedrawAllNodes();
        }
    }

    public void SetDepthIndent(int DepthIndent)
    {
        if (this.DepthIndent != DepthIndent)
        {
            this.DepthIndent = DepthIndent;
            RedrawAllNodes();
        }
    }

    public void SetDragAndDrop(bool DragAndDrop)
    {
        if (this.DragAndDrop != DragAndDrop)
        {
            this.DragAndDrop = DragAndDrop;
        }
    }

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

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            RedrawAllNodes();
        }
    }

    public void SetVScrollBarPaddingAlone(Padding VScrollBarPaddingAlone)
    {
        if (this.VScrollBarPaddingAlone != VScrollBarPaddingAlone)
        {
            this.VScrollBarPaddingAlone = VScrollBarPaddingAlone;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    public void SetVScrollBarPaddingShared(Padding VScrollBarPaddingShared)
    {
        if (this.VScrollBarPaddingShared != VScrollBarPaddingShared)
        {
            this.VScrollBarPaddingShared = VScrollBarPaddingShared;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    public void SetHScrollBarPaddingAlone(Padding HScrollBarPaddingAlone)
    {
        if (this.HScrollBarPaddingAlone != HScrollBarPaddingAlone)
        {
            this.HScrollBarPaddingAlone = HScrollBarPaddingAlone;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    public void SetHScrollBarPaddingShared(Padding HScrollBarPaddingShared)
    {
        if (this.HScrollBarPaddingShared != HScrollBarPaddingShared)
        {
            this.HScrollBarPaddingShared = HScrollBarPaddingShared;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    public void SetVResizeToFill(bool VResizeToFill)
    {
        if (this.VResizeToFill != VResizeToFill)
        {
            this.VResizeToFill = VResizeToFill;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    public void SetHResizeToFill(bool HResizeToFill)
    {
        if (this.HResizeToFill != HResizeToFill)
        {
            this.HResizeToFill = HResizeToFill;
            if (BGSprite.Bitmap != null) UpdateSize();
        }
    }

    public void SetNodes(List<OptimizedNode> Nodes, OptimizedNode? SelectedNode = null)
    {
        this.Root.ClearChildren();
        foreach (OptimizedNode node in Nodes)
        {
            this.Root.AddChild(node);
        }
        this.Root.GetAllChildren(true).ForEach(n =>
        {
            if (n is OptimizedNode)
            {
                OptimizedNode node = (OptimizedNode) n;
                node.OnGlobalIndexChanged = _ => OnNodeGlobalIndexChanged?.Invoke(new GenericObjectEventArgs<OptimizedNode>(node));
            }
        });
        RedrawAllNodes();
        if (RequireSelection) SetSelectedNode(SelectedNode ?? (Root.HasChildren ? Root.Children[0] : null), false);
    }

    public void SetXOffset(int XOffset)
    {
        if (this.XOffset != XOffset)
        {
            this.XOffset = XOffset;
            this.RedrawAllNodes();
        }
    }

    public void SetExtraXScrollArea(int ExtraXScrollArea)
    {
        if (this.ExtraXScrollArea != ExtraXScrollArea)
        {
            this.ExtraXScrollArea = ExtraXScrollArea;
            if (BGSprite.Bitmap != null) this.UpdateSize();
        }
    }

    public void SetExtraYScrollArea(int ExtraYScrollArea)
    {
        if (this.ExtraYScrollArea != ExtraYScrollArea)
        {
            this.ExtraYScrollArea = ExtraYScrollArea;
            if (BGSprite.Bitmap != null) this.UpdateSize();
        }
    }

    public unsafe void SetExpanded(OptimizedNode Node, bool Expanded)
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
                BGSprite.Bitmap = BGSprite.Bitmap.Resize(BGSprite.Bitmap.Width, BGSprite.Bitmap.Height - movamt);
                TXTSprite.Bitmap = TXTSprite.Bitmap.Resize(TXTSprite.Bitmap.Width, TXTSprite.Bitmap.Height - movamt);
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
            OnNodeExpansionChanged?.Invoke(new GenericObjectEventArgs<OptimizedNode>(Node));
        }
    }

    private int CalculateMaxWidth(OptimizedNode Start)
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
        foreach (IOptimizedNode Child in Start.Children)
        {
            int cw = Child is OptimizedNodeSeparator ? 0 : CalculateMaxWidth((OptimizedNode) Child);
            if (cw > MaxWidth) MaxWidth = cw;
        }
        return MaxWidth;
    }

    public void InsertNode(OptimizedNode ParentNode, int? InsertionIndex, IOptimizedNode NewNode)
    {
        bool DidNotHaveChildren = ParentNode.HasChildren;
        bool RedrawPrevSibling = ParentNode.HasChildren && (InsertionIndex == null || InsertionIndex == ParentNode.Children.Count);
        if (!ParentNode.Expanded && ParentNode.HasChildren) SetExpanded(ParentNode, true);
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
        ParentNode.InsertChild(InsertionIndex ?? ParentNode.Children.Count, NewNode);
        UpdateSize();
        (int CountUntil, int SepHeightUntil) = ParentNode.GetChildrenHeightUntil(NewNode, false);
        int Y = GetDrawnYCoord(ParentNode) + CountUntil * LineHeight + SepHeightUntil;
        if (ParentNode != NewNode.Root) Y += LineHeight; // Add the height of the parent node itself, unless the parent node is the root (because it is not displayed)
        int NodeCount = 0;
        int SepHeight = 0;
        if (NewNode is OptimizedNode)
        {
            (NodeCount, SepHeight) = ((OptimizedNode) NewNode).GetChildrenHeight(false);
        }
        else
        {
            SepHeight = ((OptimizedNodeSeparator) NewNode).Height;
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
        if (NewNode is OptimizedNode) ((OptimizedNode) NewNode).GetAllChildren(false).ForEach(n =>
        {
            Index++;
            Y = RedrawNode(n, Y, true, () => Index);
        });
        if (!DidNotHaveChildren)
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
            BGSprite.Bitmap.DrawLine(x + 19 - DepthIndent, sy, x + 19 - DepthIndent, ey, new Color(46, 104, 146));
        }
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
    }

    public List<IOptimizedNode> DeleteNode(IOptimizedNode Node, bool DeleteChildren)
    {
        OptimizedNode Parent = Node.Parent;
        OptimizedNode? NextSibling = (Node as OptimizedNode)?.GetNextSibling();
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
        if (Node is OptimizedNode)
        {
            (NodeCount, SepHeight) = ((OptimizedNode) Node).GetChildrenHeight(false);
            NodeCount++; // Count the node itself
        }
        else
        {
            SepHeight = ((OptimizedNodeSeparator) Node).Height;
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
        IOptimizedNode NodeToSelect = null;
        if (!DeleteChildren && Node is OptimizedNode && ((OptimizedNode) Node).HasChildren)
        {
            // We flattened the children to the node's parent, so now we have to redraw all of them to correct the depth
            BGSprite.Bitmap.Unlock();
            TXTSprite.Bitmap.Unlock();
            ((OptimizedNode) Node).GetAllChildren(false).ForEach(n =>
            {
                Y = RedrawNode(n, Y, true, () => Index);
                Index++;
            });
            BGSprite.Bitmap.Lock();
            TXTSprite.Bitmap.Lock();
            NodeToSelect = ((OptimizedNode) Node).Children[0];
        }
        else if (NextSibling != null) NodeToSelect = NextSibling;
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
                IOptimizedNode PreviousSibling = Parent.Children[ChildIndex - 1];
                int x = (OldDepth - 1) * DepthIndent + XOffset;
                int sy = GetDrawnYCoord(PreviousSibling) + 12;
                int ey = movy - shift - 1;
                BGSprite.Bitmap.DrawLine(x + 19 - DepthIndent, sy, x + 19 - DepthIndent, ey, Color.ALPHA);
            }
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
        if (DeleteChildren && Node is OptimizedNode)
        {
            List<IOptimizedNode> List = ((OptimizedNode) Node).GetAllChildren(true);
            List.Insert(0, Node);
            return List;
        }
        return new List<IOptimizedNode>() { Node };
    }

    public void RedrawAllNodes()
    {
        LastDrawData.Clear();
        BGSprite.Bitmap?.Dispose();
        TXTSprite.Bitmap?.Dispose();
        if (Root.Children.Count == 0) return;
        (int RootNodeCount, int RootSepHeight) = Root.GetChildrenHeight(false);
        int MaxWidth = CalculateMaxWidth(Root) + ExtraXScrollArea;
        BGSprite.Bitmap = new Bitmap(MaxWidth, RootNodeCount * LineHeight + RootSepHeight);
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap = new Bitmap(MaxWidth, RootNodeCount * LineHeight + RootSepHeight);
        TXTSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Font = this.Font;
        UpdateSize(false); // No need to recalculate width as we just calculated it to find the bitmap width
        List<IOptimizedNode> nodes = Root.GetAllChildren(false);
        int y = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            y = RedrawNode(nodes[i], y);
        }
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
        UpdateSize(false);
    }

    private int RedrawNode(IOptimizedNode Node, int y, bool AddData = true, Func<int> IndexProvider = null)
    {
        int x = (Node.Depth - 1) * DepthIndent + XOffset;
        if (Node.Parent != Root)
        {
            IOptimizedNode Current = Node;
            while (Current.Parent != null && Current.Parent != Root)
            {
                int Index = Current.Parent.Children.IndexOf(Current);
                OptimizedNode RCurr = Current as OptimizedNode;
                if (Current is not OptimizedNode || RCurr.GetNextSibling() != null)
                {
                    int px = (Current.Parent.Depth - 1) * DepthIndent + XOffset;
                    BGSprite.Bitmap.DrawLine(px + 19, y, px + 19, y + LineHeight - 1, new Color(64, 104, 146));
                }
                Current = Current.Parent;
            }
            BGSprite.Bitmap.DrawLine(x + 19 - DepthIndent, y, x + 19 - DepthIndent, y + 11, new Color(64, 104, 146));
            BGSprite.Bitmap.DrawLine(x, y + 11, x + 13, y + 11, new Color(64, 104, 146));
        }
        if (Node is OptimizedNodeSeparator)
        {
            OptimizedNodeSeparator sep = (OptimizedNodeSeparator) Node;
            if (AddData) LastDrawData.Add((sep, y));
            y += sep.Height;
        }
        else
        {
            OptimizedNode RNode = (OptimizedNode) Node;
            if (RNode.Children.Count > 0)
            {
                int sx = RNode.Expanded ? 11 : 0;
                BGSprite.Bitmap.Build(new Rect(x + 14, y + 6, 11, 11), TreeIconsBitmap, new Rect(sx, 0, 11, 11), BlendMode.None);
                if (RNode.Expanded) BGSprite.Bitmap.DrawLine(x + 19, y + 17, x + 19, y + LineHeight - 1, new Color(64, 104, 146));
            }
            bool sel = SelectedNodes.Contains(RNode);
            TXTSprite.Bitmap.DrawText(RNode.Text, x + 30, y + 2, sel ? new Color(55, 187, 255) : Color.WHITE);
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

    public void RedrawNodeText(OptimizedNode Node)
    {
        TXTSprite.Bitmap.Unlock();
        int x = (Node.Depth - 1) * DepthIndent + XOffset;
        int y = GetDrawnYCoord(Node);
        TXTSprite.Bitmap.FillRect(0, y, TXTSprite.Bitmap.Width, LineHeight, Color.ALPHA);
        TXTSprite.Bitmap.DrawText(Node.Text, x + 30, y + 2, SelectedNodes.Contains(Node) ? new Color(55, 187, 255) : Color.WHITE);
        TXTSprite.Bitmap.Lock();
    }

    private int GetDrawnYCoord(IOptimizedNode Node)
    {
        (_, int Y) = LastDrawData.Find(d => d.Node == Node);
        return Y;
    }

    private void ClearSelection()
    {
        SelectionSprites.ForEach(s =>
        {
            SpriteContainer.Sprites[$"sel_{s.SpriteIndex}"].Dispose();
            SpriteContainer.Sprites.Remove($"sel_{s.SpriteIndex}");
        });
        SelectionSprites.Clear();
        while (SelectedNodes.Count > 0)
        {
            IOptimizedNode n = SelectedNodes[0];
            SelectedNodes.RemoveAt(0);
            if (n.Root == n) continue; // This node was deleted
            if (n is not OptimizedNode) continue;
            RedrawNodeText((OptimizedNode) n);
        }
    }

    private void ExpandUpTo(IOptimizedNode Node)
    {
        List<OptimizedNode> Ancestors = Node.GetAncestors();
        // Start at 1 to skip the root node, which is always expanded
        for (int i = 1; i < Ancestors.Count; i++)
        {
            if (!Ancestors[i].Expanded) SetExpanded(Ancestors[i], true);
        }
    }

    private void SelectIndividualNode(IOptimizedNode Node)
    {
        ExpandUpTo(Node);
        int i = 0;
        while (SpriteContainer.Sprites.ContainsKey($"sel_{i}")) i++;
        SpriteContainer.Sprites[$"sel_{i}"] = new Sprite(SpriteContainer.Viewport);
        int height = Node is OptimizedNodeSeparator ? ((OptimizedNodeSeparator) Node).Height : LineHeight;
        SpriteContainer.Sprites[$"sel_{i}"].Bitmap = new SolidBitmap(SpriteContainer.Size.Width, height, new Color(28, 50, 73));
        int y = GetDrawnYCoord(Node);
        SelectedNodes.Add(Node);
        SpriteContainer.Sprites[$"sel_{i}"].Y = y;
        SpriteContainer.UpdateBounds();
        SelectionSprites.Add((Node, i));
        if (Node is OptimizedNode) RedrawNodeText((OptimizedNode) Node);
    }

    private void UpdateSelection(IOptimizedNode Node)
    {
        int i = SelectionSprites.Find(s => s.Node == Node).SpriteIndex;
        SpriteContainer.Sprites[$"sel_{i}"].Y = GetDrawnYCoord(Node);
    }

    public void SetSelectedNode(IOptimizedNode Node, bool AllowMultiple, bool DoubleClicked = true)
    {
        if (!AllowMultiple) ClearSelection();
        if (Node != null) SelectIndividualNode(Node);
        OnSelectionChanged?.Invoke(new BoolEventArgs(DoubleClicked));
    }

    public void SetHoveringNode(IOptimizedNode Node)
    {
        DragState = null;
        this.HoveringNode = Node;
        if (Node != null && Node.Selectable)
        {
            int Y = GetDrawnYCoord(this.HoveringNode);
            int Height = this.HoveringNode is OptimizedNodeSeparator ? ((OptimizedNodeSeparator) this.HoveringNode).Height : LineHeight;
            SpriteContainer.Sprites["hover"].Y = Y;
            ((SolidBitmap) SpriteContainer.Sprites["hover"].Bitmap).SetSize(2, Height);
            SpriteContainer.Sprites["hover"].Visible = !Dragging || !ValidatedDragMovement;
        }
        else
        {
            SpriteContainer.Sprites["hover"].Visible = false;
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (this.Empty) ScrollContainer.SetSize(Size);
        else UpdateSize(false); // The max width of the tree itself does not depend on the size of this widget
    }

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
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        IOptimizedNode OldHoveringNode = this.HoveringNode;
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
            IOptimizedNode Node = LastDrawData[i].Node;
            int ny = LastDrawData[i].Y;
            int realheight = Node is OptimizedNodeSeparator ? ((OptimizedNodeSeparator) Node).Height : LineHeight;
            if (ry >= ny && ry < ny + realheight)
            {
                SetHoveringNode(Node);
                yfraction = (float) (ry - ny) / (realheight - 1);
                break;
            }
        }
        if (Dragging && !ValidatedDragMovement)
        {
            Point mp = new Point(rx, ry);
            if (DragOriginPoint.Distance(mp) >= 10)
            {
                ValidatedDragMovement = true;
                PreDragDropRootNode = (OptimizedNode) this.Root.Clone();
            }
            this.OnDragAndDropping?.Invoke(new GenericObjectEventArgs<IOptimizedNode>(this.ActiveNode));
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
            if (ActiveNode is OptimizedNode && ((OptimizedNode) ActiveNode).Contains(HoveringNode, true) ||
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
                IOptimizedNode? NextNode = null;
                if (this.HoveringNode is OptimizedNode) NextNode = ((OptimizedNode) this.HoveringNode).GetNextNode();
                else
                {
                    // Use the draw data to find the next node if we're hovering over a non-node.
                    int Index = LastDrawData.FindIndex(i => i.Node == this.HoveringNode);
                    if (Index < LastDrawData.Count - 1) NextNode = LastDrawData[Index + 1].Node;
                }
                if (NextNode == null || this.HoveringNode.Parent == NextNode.Parent) // If the next node is a sibling, share the line
                    this.DragState = DragStates.SharedBelow;
                if (this.HoveringNode is OptimizedNode && ((OptimizedNode) this.HoveringNode).HasChildren &&
                    ((OptimizedNode) this.HoveringNode).Children[0] == NextNode) // If the next node is the first child, share the line
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

    private void RedrawDragState()
    {
        SpriteContainer.Sprites["drag"].Bitmap?.Dispose();
        if (DragState != null)
        {
            int x = this.HoveringNode.Depth * DepthIndent + DragLineOffset;
            int y = GetDrawnYCoord(this.HoveringNode);
            int height = this.HoveringNode is OptimizedNodeSeparator ? ((OptimizedNodeSeparator) this.HoveringNode).Height : LineHeight;
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
                if (this.HoveringNode is OptimizedNode)
                    SpriteContainer.Sprites["drag"].X = x + TXTSprite.Bitmap.Font.TextSize(((OptimizedNode) this.HoveringNode).Text).Width + 30;
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

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        this.ActiveNode = HoveringNode;
        if (HoveringNode != null && HoveringNode.Draggable)
        {
            this.Dragging = true;
            int rx = e.X - Viewport.X + SpriteContainer.LeftCutOff;
            int ry = e.Y - Viewport.Y + SpriteContainer.TopCutOff;
            this.DragOriginPoint = new Point(rx, ry);
        }
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (this.Dragging && this.ValidatedDragMovement)
        {
            if (this.ActiveNode != this.HoveringNode && // Drag-and-dropping over different node
                (this.ActiveNode is not OptimizedNode || !((OptimizedNode) this.ActiveNode).Contains(this.HoveringNode))) // If the hovered node is a child of the active node, we can't make active node a child of the hovering node.
            {
                if (this.DragState == DragStates.Over && this.HoveringNode is OptimizedNode) // If the hovered node is a node capable of having children
                {
                    DeleteNode(this.ActiveNode, true);
                    InsertNode((OptimizedNode) this.HoveringNode, null, this.ActiveNode);
                    this.OnDragAndDropped?.Invoke(new GenericObjectEventArgs<(IOptimizedNode, OptimizedNode, OptimizedNode)>((this.ActiveNode, PreDragDropRootNode, this.Root)));
                }
                else if (this.DragState == DragStates.Above || this.DragState == DragStates.SharedAbove)
                {
                    DeleteNode(this.ActiveNode, true);
                    int HoveredIndex = this.HoveringNode.Parent.Children.IndexOf(this.HoveringNode);
                    InsertNode(this.HoveringNode.Parent, HoveredIndex, this.ActiveNode);
                    this.OnDragAndDropped?.Invoke(new GenericObjectEventArgs<(IOptimizedNode, OptimizedNode, OptimizedNode)>((this.ActiveNode, PreDragDropRootNode, this.Root)));
                }
                else if (this.DragState == DragStates.Below || this.DragState == DragStates.SharedBelow)
                {
                    DeleteNode(this.ActiveNode, true);
                    // If we're below a node with visible children, then we want to insert the node as its first child rather than a sibling.
                    if (this.DragState == DragStates.SharedBelow && this.HoveringNode is OptimizedNode &&
                        ((OptimizedNode) this.HoveringNode).HasChildren && ((OptimizedNode) this.HoveringNode).Expanded)
                    {
                        InsertNode((OptimizedNode) this.HoveringNode, 0, this.ActiveNode);
                    }
                    else
                    {
                        int HoveredIndex = this.HoveringNode.Parent.Children.IndexOf(this.HoveringNode);
                        InsertNode(this.HoveringNode.Parent, HoveredIndex + 1, this.ActiveNode);
                    }
                    this.OnDragAndDropped?.Invoke(new GenericObjectEventArgs<(IOptimizedNode, OptimizedNode, OptimizedNode)>((this.ActiveNode, PreDragDropRootNode, this.Root)));
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
            if (ActiveNode is OptimizedNode && rx >= NodeX + 14 && rx < NodeX + 25 && ry >= NodeY + 6 && ry < NodeY + 17)
            {
                SetExpanded((OptimizedNode) ActiveNode, !((OptimizedNode) ActiveNode).Expanded);
            }
            else
            {
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
        this.Dragging = false;
        this.ActiveNode = null;
        this.DragState = null;
        this.DragLineOffset = 0;
        this.DragOriginPoint = null;
        if (this.ValidatedDragMovement) RedrawDragState();
        this.ValidatedDragMovement = false;
    }

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
        if (Dragging && ValidatedDragMovement && (ActiveNode is not OptimizedNode || !((OptimizedNode) ActiveNode).Contains(HoveringNode)) && HoveringNode is OptimizedNode)
        {
            OptimizedNode HovNode = (OptimizedNode) HoveringNode;
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
}

enum DragStates
{
    Above,
    Over,
    Below,
    SharedAbove,
    SharedBelow
}