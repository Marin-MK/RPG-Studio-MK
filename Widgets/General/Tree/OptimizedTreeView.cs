using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

    static Bitmap TreeIconsBitmap;

    public OptimizedNode Root { get; protected set; }
    public int LineHeight { get; protected set; } = 24;
    public int DepthIndent { get; protected set; } = 20;
    public int XOffset { get; protected set; } = 6;
    public IOptimizedNode HoveringNode { get; protected set; }
    public List<IOptimizedNode> SelectedNodes { get; protected set; } = new List<IOptimizedNode>();
    public bool MultipleSelected => SelectedNodes.Count > 1;
    public IOptimizedNode SelectedNode => SelectedNodes.Count > 0 ? SelectedNodes[0] : null;

    List<(IOptimizedNode Node, int Y)> LastDrawData = new List<(IOptimizedNode, int)>();
    List<(IOptimizedNode Node, int SpriteIndex)> SelectionSprites = new List<(IOptimizedNode, int)>();

    Container ScrollContainer;
    Sprite BGSprite => ScrollContainer.Sprites["bg"];
    Sprite TXTSprite => ScrollContainer.Sprites["txt"];

    private IOptimizedNode? ActiveNode;
    private bool Dragging = false;
    private DragStates? DragState;
    private int DragLineOffset = 0;
    private Point? DragOriginPoint;
    private bool ValidatedDragMovement = false;

    public OptimizedTreeView(IContainer Parent) : base(Parent)
    {
        if (TreeIconsBitmap == null)
        {
            TreeIconsBitmap = new Bitmap("assets/img/tree_icons");
        }
        ScrollContainer = new Container(this);
        ScrollContainer.SetDocked(true);
        ScrollContainer.Sprites["hover"] = new Sprite(Viewport, new SolidBitmap(1, 1, new Color(55, 187, 255)));
        ScrollContainer.Sprites["hover"].Visible = false;
        ScrollContainer.Sprites["hover"].Z = 1;
        ScrollContainer.Sprites["bg"] = new Sprite(Viewport);
        ScrollContainer.Sprites["bg"].Z = 2;
        ScrollContainer.Sprites["txt"] = new Sprite(Viewport);
        ScrollContainer.Sprites["txt"].Z = 2;
        ScrollContainer.Sprites["drag"] = new Sprite(Viewport);
        ScrollContainer.Sprites["drag"].Z = 3;
        OnWidgetSelected += WidgetSelected;
    }

    public void SetRootNode(OptimizedNode Root)
    {
        if (this.Root != Root)
        {
            this.Root = Root;
            RedrawAllNodes();
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
                (int NodeCount, int SepHeight) = Node.GetChildrenHeight(false);
                int Y = GetDrawnYCoord(Node);
                int Height = (NodeCount + 1) * LineHeight + SepHeight;
                int movy = Y + LineHeight;
                int movh = BGSprite.Bitmap.Height - (movy);
                int movamt = Height - LineHeight;
                BGSprite.Bitmap.FillRect(0, Y, BGSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                TXTSprite.Bitmap.FillRect(0, Y, TXTSprite.Bitmap.Width, LineHeight, Color.ALPHA);
                BGSprite.Bitmap = BGSprite.Bitmap.ResizeWithoutBuild(BGSprite.Bitmap.Width, BGSprite.Bitmap.Height + movamt);
                TXTSprite.Bitmap = TXTSprite.Bitmap.ResizeWithoutBuild(TXTSprite.Bitmap.Width, TXTSprite.Bitmap.Height + movamt);
                TXTSprite.Bitmap.Font = Fonts.Paragraph;
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
                RedrawNode(Node, Y, false);
                int movy = Y + Height;
                int movh = BGSprite.Bitmap.Height - (movy);
                int movamt = Height - LineHeight;
                BGSprite.Bitmap.ShiftVertically(movy, movh, -movamt, true);
                TXTSprite.Bitmap.ShiftVertically(movy, movh, -movamt, true);
                BGSprite.Bitmap = BGSprite.Bitmap.ResizeWithoutBuild(BGSprite.Bitmap.Width, BGSprite.Bitmap.Height - movamt);
                TXTSprite.Bitmap = TXTSprite.Bitmap.ResizeWithoutBuild(TXTSprite.Bitmap.Width, TXTSprite.Bitmap.Height - movamt);
                TXTSprite.Bitmap.Font = Fonts.Paragraph;
                TXTSprite.Bitmap.BlendMode = BlendMode.Addition;
                TXTSprite.Bitmap.RecreateTexture();
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
        }
    }

    public void InsertNode(OptimizedNode ParentNode, int? InsertionIndex, IOptimizedNode NewNode)
    {
        bool DidNotHaveChildren = ParentNode.HasChildren;
        bool RedrawPrevSibling = ParentNode.HasChildren && (InsertionIndex == null || InsertionIndex == ParentNode.Children.Count);
        if (!ParentNode.Expanded && ParentNode.HasChildren) SetExpanded(ParentNode, true);
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
        ParentNode.InsertChild(InsertionIndex ?? ParentNode.Children.Count, NewNode);
        (int CountUntil, int SepHeightUntil) = ParentNode.GetChildrenHeightUntil(NewNode, false);
        int Y = GetDrawnYCoord(ParentNode) + LineHeight + CountUntil * LineHeight + SepHeightUntil;
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
        BGSprite.Bitmap = BGSprite.Bitmap.ResizeWithoutBuild(BGSprite.Bitmap.Width, BGSprite.Bitmap.Height + movamt);
        TXTSprite.Bitmap = TXTSprite.Bitmap.ResizeWithoutBuild(TXTSprite.Bitmap.Width, TXTSprite.Bitmap.Height + movamt);
        TXTSprite.Bitmap.Font = Fonts.Paragraph;
        TXTSprite.Bitmap.BlendMode = BlendMode.Addition;
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Unlock();
        BGSprite.Bitmap.ShiftVertically(movy, movh, movamt, true);
        TXTSprite.Bitmap.ShiftVertically(movy, movh, movamt, true);
        int Index = LastDrawData.FindIndex(d => d.Y >= Y);
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
        if (RedrawPrevSibling)
        {
            int x = (NewNode.Depth - 1) * DepthIndent + XOffset;
            int sy = GetDrawnYCoord(ParentNode) + LineHeight;
            int ey = movy;
            BGSprite.Bitmap.DrawLine(x + 19 - DepthIndent, sy, x + 19 - DepthIndent, ey, new Color(46, 104, 146));
        }
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
    }

    public void DeleteNode(IOptimizedNode Node, bool DeleteChildren)
    {
        OptimizedNode Parent = Node.Parent;
        int OldDepth = Node.Depth;
        int ChildIndex = Parent.Children.IndexOf(Node);
        Node.Delete(DeleteChildren);
        if (Root.Children.Count == 0)
        {
            BGSprite.Bitmap.Dispose();
            TXTSprite.Bitmap.Dispose();
            LastDrawData.Clear();
            return;
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
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
        int movy = Y + HeightToClear;
        int movh = BGSprite.Bitmap.Height - movy;
        int shift = DeleteChildren ? HeightToClear : LineHeight;
        BGSprite.Bitmap.ShiftVertically(movy, movh, -shift, true);
        TXTSprite.Bitmap.ShiftVertically(movy, movh, -shift, true);
        BGSprite.Bitmap = BGSprite.Bitmap.ResizeWithoutBuild(BGSprite.Bitmap.Width, BGSprite.Bitmap.Height - shift);
        TXTSprite.Bitmap = TXTSprite.Bitmap.ResizeWithoutBuild(TXTSprite.Bitmap.Width, TXTSprite.Bitmap.Height - shift);
        TXTSprite.Bitmap.Font = Fonts.Paragraph;
        TXTSprite.Bitmap.BlendMode = BlendMode.Addition;
        TXTSprite.Bitmap.RecreateTexture();
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
    }

    public void RedrawAllNodes()
    {
        LastDrawData.Clear();
        BGSprite.Bitmap?.Dispose();
        TXTSprite.Bitmap?.Dispose();
        if (Root.Children.Count == 0) return;
        (int RootNodeCount, int RootSepHeight) = Root.GetChildrenHeight(false);
        BGSprite.Bitmap = new Bitmap(Size.Width, RootNodeCount * LineHeight + RootSepHeight);
        BGSprite.Bitmap.Unlock();
        TXTSprite.Bitmap = new Bitmap(Size.Width, RootNodeCount * LineHeight + RootSepHeight);
        TXTSprite.Bitmap.Unlock();
        TXTSprite.Bitmap.Font = Fonts.Paragraph;
        List<IOptimizedNode> nodes = Root.GetAllChildren(false);
        int y = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            y = RedrawNode(nodes[i], y);
        }
        BGSprite.Bitmap.Lock();
        TXTSprite.Bitmap.Lock();
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
                BGSprite.Bitmap.Build(new Rect(x + 14, y + 6, 11, 11), TreeIconsBitmap, new Rect(sx, 0, 11, 11));
                if (RNode.Expanded) BGSprite.Bitmap.DrawLine(x + 19, y + 17, x + 19, y + 23, new Color(64, 104, 146));
            }
            TXTSprite.Bitmap.DrawText(RNode.Text, x + 30, y + 2, Color.WHITE);
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

    private int GetDrawnYCoord(IOptimizedNode Node)
    {
        (_, int Y) = LastDrawData.Find(d => d.Node == Node);
        return Y;
    }

    private void ClearSelection()
    {
        SelectionSprites.ForEach(s =>
        {
            ScrollContainer.Sprites[$"sel_{s.SpriteIndex}"].Dispose();
            ScrollContainer.Sprites.Remove($"sel_{s.SpriteIndex}");
        });
        SelectionSprites.Clear();
        SelectedNodes.Clear();
    }

    public void SetSelectedNode(IOptimizedNode Node, bool AllowMultiple)
    {
        if (!AllowMultiple) ClearSelection();
        int i = 0;
        while (ScrollContainer.Sprites.ContainsKey($"sel_{i}")) i++;
        ScrollContainer.Sprites[$"sel_{i}"] = new Sprite(this.Viewport);
        int height = Node is OptimizedNodeSeparator ? ((OptimizedNodeSeparator) Node).Height : LineHeight;
        ScrollContainer.Sprites[$"sel_{i}"].Bitmap = new SolidBitmap(Size.Width, height, new Color(28, 50, 73));
        int y = GetDrawnYCoord(Node);
        ScrollContainer.Sprites[$"sel_{i}"].Y = y;
        SelectionSprites.Add((Node, i));
        SelectedNodes.Add(Node);
    }

    public void SetHoveringNode(IOptimizedNode Node)
    {
        DragState = null;
        this.HoveringNode = Node;
        if (Node != null && Node.Selectable)
        {
            int Y = GetDrawnYCoord(this.HoveringNode);
            int Height = this.HoveringNode is OptimizedNodeSeparator ? ((OptimizedNodeSeparator) this.HoveringNode).Height : LineHeight;
            ScrollContainer.Sprites["hover"].Y = Y;
            ((SolidBitmap) ScrollContainer.Sprites["hover"].Bitmap).SetSize(2, Height);
            ScrollContainer.Sprites["hover"].Visible = !Dragging || !ValidatedDragMovement;
        }
        else
        {
            ScrollContainer.Sprites["hover"].Visible = false;
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        IOptimizedNode OldHoveringNode = this.HoveringNode;
        DragStates? OldDragState = this.DragState;
        DragLineOffset = 0;
        SetHoveringNode(null);
        if (!ScrollContainer.Mouse.Inside)
        {
            RedrawDragState();
            return;
        }
        int rx = e.X - ScrollContainer.Viewport.X;
        int ry = e.Y - ScrollContainer.Viewport.Y + ScrollContainer.TopCutOff;
        float yfraction = 0.5f;
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
            Point mp = new Point(e.X, e.Y);
            if (DragOriginPoint.Distance(mp) >= 10)
            {
                ValidatedDragMovement = true;
            }
        }
        // Drag-and-drop
        if (Dragging && ValidatedDragMovement && HoveringNode != null)
        {
            bool CanDragOver = this.HoveringNode.CanDragOver;
            bool TopArea = !CanDragOver && yfraction < 0.5f || yfraction < 1f / 3;
            bool MiddleArea = CanDragOver && yfraction < 2f / 3;
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
        ScrollContainer.Sprites["drag"].Bitmap?.Dispose();
        if (DragState != null)
        {
            int x = this.HoveringNode.Depth * DepthIndent + DragLineOffset;
            int y = GetDrawnYCoord(this.HoveringNode);
            int height = this.HoveringNode is OptimizedNodeSeparator ? ((OptimizedNodeSeparator) this.HoveringNode).Height : LineHeight;
            if (this.DragState == DragStates.Above || this.DragState == DragStates.Below ||
                this.DragState == DragStates.SharedAbove || this.DragState == DragStates.SharedBelow)
            {
                // Single line between two nodes
                ScrollContainer.Sprites["drag"].X = x;
                ScrollContainer.Sprites["drag"].Y = y + 2;
                switch (this.DragState)
                {
                    case DragStates.Below:
                        ScrollContainer.Sprites["drag"].Y = y + height - 2;
                        break;
                    case DragStates.SharedBelow:
                        ScrollContainer.Sprites["drag"].Y = y + height;
                        break;
                    case DragStates.SharedAbove:
                        ScrollContainer.Sprites["drag"].Y = y;
                        break;
                }
                int width = Math.Max(100, Size.Width - x - 10);
                ScrollContainer.Sprites["drag"].Bitmap = new SolidBitmap(width, 1, new Color(55, 187, 255));
            }
            else if (this.DragState == DragStates.Over)
            {
                // Over one node
                if (this.HoveringNode is OptimizedNode)
                    ScrollContainer.Sprites["drag"].X = x + TXTSprite.Bitmap.Font.TextSize(((OptimizedNode) this.HoveringNode).Text).Width + 30;
                else ScrollContainer.Sprites["drag"].X = 4;
                ScrollContainer.Sprites["drag"].Y = y + height / 2 - 3;
                ScrollContainer.Sprites["drag"].Bitmap = new Bitmap(7, 7);
                ScrollContainer.Sprites["drag"].Bitmap = new Bitmap(7, 7);
                ScrollContainer.Sprites["drag"].Bitmap.Unlock();
                Color c = new Color(55, 187, 255);
                ScrollContainer.Sprites["drag"].Bitmap.DrawLine(2, 0, 6, 0, c);
                ScrollContainer.Sprites["drag"].Bitmap.DrawLine(2, 0, 2, 6, c);
                ScrollContainer.Sprites["drag"].Bitmap.DrawLine(0, 4, 4, 4, c);
                ScrollContainer.Sprites["drag"].Bitmap.SetPixel(1, 5, c);
                ScrollContainer.Sprites["drag"].Bitmap.SetPixel(3, 5, c);
                ScrollContainer.Sprites["drag"].Bitmap.Lock();
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
            this.DragOriginPoint = new Point(e.X, e.Y);
        }
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (this.ActiveNode != null && this.ActiveNode.Selectable && this.ActiveNode == this.HoveringNode && (!this.Dragging || !this.ValidatedDragMovement))
        {
            int rx = e.X - ScrollContainer.Viewport.X + ScrollContainer.LeftCutOff;
            int ry = e.Y - ScrollContainer.Viewport.Y + ScrollContainer.TopCutOff;
            int NodeX = (this.ActiveNode.Depth - 1) * DepthIndent + XOffset;
            int NodeY = GetDrawnYCoord(ActiveNode);
            if (ActiveNode is OptimizedNode && rx >= NodeX + 14 && rx < NodeX + 25 && ry >= NodeY + 6 && ry < NodeY + 17)
            {
                SetExpanded((OptimizedNode) ActiveNode, !((OptimizedNode) ActiveNode).Expanded);
            }
            else
            {
                SetSelectedNode(this.ActiveNode, Input.Press(Keycode.CTRL));
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
}

enum DragStates
{
    Above,
    Over,
    Below,
    SharedAbove,
    SharedBelow
}