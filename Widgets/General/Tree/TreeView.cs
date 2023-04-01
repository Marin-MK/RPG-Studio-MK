﻿using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class TreeView : Widget
{
    public delegate void NodeEvent(TreeNode Node, TreeNode OldSelectedNode);

    public List<TreeNode> Nodes { get; protected set; } = new List<TreeNode>();
    public TreeNode SelectedNode { get; protected set; }
    public TreeNode HoveringNode { get; protected set; }
    public TreeNode DraggingNode { get; protected set; }

    public int TrailingBlank = 0;

    public MouseEvent OnSelectedNodeChanged;
    public BaseEvent OnDragAndDropped;
    public NodeEvent OnNodeCollapseChanged;

    public bool HoverTop = false;
    public bool HoverOver = false;
    public bool HoverBottom = false;

    bool MergeTopLines = false;
    bool MergeBottomLines = false;
    int LineDepth = 0;
    TreeNode OldHoveringNode;

    public TreeView(IContainer Parent) : base(Parent)
    {
        Sprites["selector"] = new Sprite(Viewport, new SolidBitmap(1, 21, new Color(28, 50, 73)));
        Sprites["hover"] = new Sprite(Viewport, new SolidBitmap(2, 21, new Color(55, 187, 255)));
        Sprites["hover"].Visible = false;
        Sprites["list"] = new Sprite(Viewport);
        Sprites["list_drag"] = new Sprite(Viewport);
        Sprites["text"] = new Sprite(Viewport);
        OnWidgetSelected += WidgetSelected;
    }

    public void SetNodes(List<TreeNode> Nodes)
    {
        this.Nodes = Nodes;
        Sprites["hover"].Visible = false;
        Sprites["hover"].Y = -24;
        SelectedNode = Nodes.Count > 0 ? Nodes[0] : null;
        Redraw();
        MouseMoving(Graphics.LastMouseEvent);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        (Sprites["selector"].Bitmap as SolidBitmap).SetSize(Size.Width, 21);
    }

    protected override void Draw()
    {
        Sprites["list"].Bitmap?.Dispose();
        Sprites["text"].Bitmap?.Dispose();
        int maxwidth = GetMaxNodeWidth(Nodes);
        int items = 0;
        for (int i = 0; i < Nodes.Count; i++)
        {
            items++;
            items += Nodes[i].GetDisplayedNodeCount();
        }
        int height = items * 24;
        SetSize(maxwidth, height + TrailingBlank);
        if (height == 0) height++;
        Sprites["list"].Bitmap = new Bitmap(Size.Width, height, Graphics.MaxTextureSize);
        Sprites["text"].Bitmap = new Bitmap(Size.Width, height, Graphics.MaxTextureSize);
        Sprites["list"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Unlock();

        int y = 0;
        Sprites["selector"].Y = -24;
        for (int i = 0; i < Nodes.Count; i++)
        {
            y = DrawNode(Nodes[i], 15, y, true, i == Nodes.Count - 1);
            y += 24;
        }

        Sprites["list"].Bitmap.Lock();
        Sprites["text"].Bitmap.Lock();

        base.Draw();
    }

    private int DrawNode(TreeNode node, int x, int y, bool FirstGeneration, bool LastNode)
    {
        bool selected = DraggingNode == null && node == SelectedNode || node == DraggingNode;
        if (selected) Sprites["selector"].Y = y;
        node.PixelsIndented = x;
        Sprites["text"].Bitmap.Font = Fonts.Paragraph;
        string text = node.Name ?? node.Object.ToString();
        Size s = Fonts.Paragraph.TextSize(text);
        Color c = selected ? new Color(55, 187, 255) : Color.WHITE;
        Sprites["text"].Bitmap.DrawText(text, x + 12, y + 1, c);

        if (!FirstGeneration)
        {
            if (x - 6 < Sprites["list"].Bitmap.Width) Sprites["list"].Bitmap.DrawLine(x - 16, y + 8, x - 6, y + 8, new Color(64, 104, 146));
        }

        if (node.Nodes.Count > 0)
        {
            int count = node.GetDisplayedNodeCount();
            if (node.Nodes.Count > 0) count -= node.Nodes[node.Nodes.Count - 1].GetDisplayedNodeCount();
            if (count > 0 && x < Sprites["list"].Bitmap.Width) Sprites["list"].Bitmap.DrawLine(x, y + 8, x, y + 8 + count * 24, new Color(64, 104, 146));
            if (x + 5 < Sprites["list"].Bitmap.Width) Utilities.DrawCollapseBox(Sprites["list"].Bitmap, x - 5, y + 3, node.Collapsed);
            if (!node.Collapsed)
            {
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    y += 24;
                    y = DrawNode(node.Nodes[i], x + 16, y, false, i == node.Nodes.Count - 1);
                }
            }
        }
        return y;
    }

    int GetMaxNodeWidth(List<TreeNode> Nodes, int depth = 0)
    {
        int width = 128;
        foreach (TreeNode n in Nodes)
        {
            int textlength = Fonts.Paragraph.TextSize(n.Name ?? n.Object.ToString()).Width;
            int nodelength = 31 + depth * 16 + textlength;
            if (nodelength > width) width = nodelength;
            if (!n.Collapsed)
            {
                int result = GetMaxNodeWidth(n.Nodes, depth + 1);
                if (result > width) width = result;
            }
        }
        return width;
    }

    void UpdateHoverDrag()
    {
        Sprites["list_drag"].Bitmap?.Dispose();
        if (DraggingNode == null || HoveringNode == null || HoveringNode == DraggingNode) return;
        Sprites["hover"].Visible = false;
        int y = 0;
        bool Found = false;
        for (int i = 0; i < Nodes.Count; i++)
        {
            (int Y, bool Continue) result = CalculateNodeYUntil(Nodes[i], HoveringNode);
            y += result.Y;
            if (!result.Continue)
            {
                Found = true;
                break;
            }
        }
        if (!Found) throw new Exception($"Failed to find DraggingNode.");
        if (HoverTop || HoverBottom)
        {
            Sprites["list_drag"].X = LineDepth * 16 + 12;
            Sprites["list_drag"].Bitmap = new SolidBitmap(Size.Width - Sprites["list_drag"].X - 2, 1, new Color(55, 187, 255));
            if (HoverTop) Sprites["list_drag"].Y = y + (MergeTopLines ? -2 : 3);
            else Sprites["list_drag"].Y = y + (MergeBottomLines ? 22 : 17);
        }
        else if (HoverOver)
        {
            Sprites["list_drag"].Bitmap = new Bitmap(7, 7);
            Sprites["list_drag"].X = 2;
            Sprites["list_drag"].Y = y + 7;
            Sprites["list_drag"].Bitmap.Unlock();
            Color c = new Color(55, 187, 255);
            Sprites["list_drag"].Bitmap.DrawLine(2, 0, 6, 0, c);
            Sprites["list_drag"].Bitmap.DrawLine(2, 0, 2, 6, c);
            Sprites["list_drag"].Bitmap.DrawLine(0, 4, 4, 4, c);
            Sprites["list_drag"].Bitmap.SetPixel(1, 5, c);
            Sprites["list_drag"].Bitmap.SetPixel(3, 5, c);
            Sprites["list_drag"].Bitmap.Lock();
        }
    }

    (int Y, bool Continue) CalculateNodeYUntil(TreeNode Node, TreeNode Target)
    {
        if (Node == Target) return (0, false);
        if (Node.Collapsed) return (24, true);
        int sumy = 24;
        foreach (TreeNode n in Node.Nodes)
        {
            (int Y, bool Continue) result = CalculateNodeYUntil(n, Target);
            sumy += result.Y;
            if (!result.Continue) return (sumy, false);
        }
        return (sumy, true);
    }

    int GetNodeDepth(TreeNode node, int depth = 0, List<TreeNode> NodeList = null)
    {
        if (NodeList == null) NodeList = Nodes;
        foreach (TreeNode child in NodeList)
        {
            if (child == node) return depth;
            int result = GetNodeDepth(node, depth + 1, child.Nodes);
            if (result != -1) return result;
        }
        return -1;
    }

    public void SetSelectedNode(TreeNode node, bool CallEvent = true)
    {
        SelectedNode = node;
        if (node == null && Nodes.Count == 0)
        {
            HoveringNode = null;
            Sprites["hover"].Visible = false;
        }
        if (CallEvent) OnSelectedNodeChanged?.Invoke(Graphics.LastMouseEvent);
        Redraw();
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["hover"].Visible = Mouse.Inside;
        if (!Mouse.Inside) HoveringNode = null;
        UpdateHoverDrag();
        MouseMoving(e);
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (Nodes.Count == 0 && HoveringNode != null)
        {
            HoveringNode = null;
            Sprites["hover"].Visible = false;
            return;
        }
        if (!Mouse.Inside) return;
        int rx = e.X - Viewport.X + Position.X - ScrolledPosition.X;
        int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
        if (ry >= Size.Height - TrailingBlank)
        {
            Sprites["hover"].Visible = false;
            HoveringNode = null;
            return;
        }
        int globalindex = (int)Math.Floor(ry / 24d);
        int part = ry % 24;
        Sprites["hover"].Visible = true;
        Sprites["hover"].Y = globalindex * 24;
        HoverTop = part < 7;
        HoverOver = !HoverTop && part < 17;
        HoverBottom = !HoverTop && !HoverOver;
        int index = 0;
        TreeNode n = null;
        for (int i = 0; i < Nodes.Count; i++)
        {
            n = Nodes[i].FindNodeIndex(globalindex - index);
            if (n != null) break;
            index += Nodes[i].GetDisplayedNodeCount() + 1;
        }
        HoveringNode = n;
        if (DraggingNode != null && HoveringNode != null)
        {
            LineDepth = GetNodeDepth(HoveringNode);
            MergeTopLines = false;
            TreeNode upnode = null;
            if (globalindex > 0)
            {
                int upidx = globalindex - 1;
                index = 0;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    upnode = Nodes[i].FindNodeIndex(upidx - index);
                    if (upnode != null) break;
                    index += Nodes[i].GetDisplayedNodeCount() + 1;
                }
            }
            if (upnode != null)
            {
                TreeNode HoverParent = null;
                if (Nodes.Contains(HoveringNode))
                {
                    int diff = Nodes.IndexOf(HoveringNode) - Nodes.IndexOf(upnode);
                    if (diff == 1)
                    {
                        // Upnode is indeed above the current node in our parent.
                        // Now whether it is also visually above our current node
                        // depends on if it has children and is not collapsed.
                        if (upnode.Nodes.Count == 0 || upnode.Collapsed)
                        {
                            MergeTopLines = true;
                        }
                    }
                }
                else if (upnode.Nodes.Count > 0 && upnode.Nodes[0] == HoveringNode) MergeTopLines = true;
                else
                {
                    foreach (TreeNode MainNode in Nodes)
                    {
                        HoverParent = MainNode.FindParentNode(n => n == HoveringNode);
                        if (HoverParent != null) break;
                    }
                    if (HoverParent.Nodes.Contains(upnode))
                    {
                        int diff = HoverParent.Nodes.IndexOf(HoveringNode) - HoverParent.Nodes.IndexOf(upnode);
                        if (diff == 1)
                        {
                            // Upnode is indeed above the current node in our parent.
                            // Now whether it is also visually above our current node
                            // depends on if it has children and is not collapsed.
                            if (upnode.Nodes.Count == 0 || upnode.Collapsed)
                            {
                                MergeTopLines = true;
                            }
                        }
                    }
                }
            }
            MergeBottomLines = false;
            TreeNode btmnode = null;
            int dwnidx = globalindex + 1;
            index = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                btmnode = Nodes[i].FindNodeIndex(dwnidx - index);
                if (btmnode != null) break;
                index += Nodes[i].GetDisplayedNodeCount() + 1;
            }
            if (btmnode != null)
            {
                TreeNode HoverParent = null;
                if (Nodes.Contains(HoveringNode))
                {
                    if (HoveringNode.Nodes.Count > 0 && HoveringNode.Nodes[0] == btmnode)
                    {
                        MergeBottomLines = true;
                        if (HoverBottom) LineDepth = GetNodeDepth(btmnode);
                    }
                    else
                    {
                        int diff = Nodes.IndexOf(btmnode) - Nodes.IndexOf(HoveringNode);
                        if (diff == 1)
                        {
                            // Btmnode is indeed below the current node in our parent.
                            // Now whether it is also visually above our current node
                            // depends on if the hovering node has children and if it is not collapsed.
                            if (HoveringNode.Nodes.Count == 0 || HoveringNode.Collapsed)
                            {
                                MergeBottomLines = true;
                            }
                        }
                    }
                }
                else if (HoveringNode.Nodes.Count > 0 && HoveringNode.Nodes[0] == btmnode)
                {
                    MergeBottomLines = true;
                    if (HoverBottom) LineDepth = GetNodeDepth(btmnode);
                }
                else
                {
                    foreach (TreeNode MainNode in Nodes)
                    {
                        HoverParent = MainNode.FindParentNode(n => n == HoveringNode);
                        if (HoverParent != null) break;
                    }
                    if (HoverParent.Nodes.Contains(btmnode))
                    {
                        int diff = HoverParent.Nodes.IndexOf(btmnode) - HoverParent.Nodes.IndexOf(HoveringNode);
                        if (diff == 1)
                        {
                            // Btmnode is indeed below the current node in our parent.
                            // Now whether it is also visually above our current node
                            // depends on if the hovering node has children and if it is not collapsed.
                            if (HoveringNode.Nodes.Count == 0 || HoveringNode.Collapsed)
                            {
                                MergeBottomLines = true;
                            }
                        }
                    }
                }
            }
            UpdateHoverDrag();
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside) return;
        TreeNode oldselected = SelectedNode;
        if (e.LeftButton && !e.OldLeftButton && e.LeftButton)
        {
            int rx = e.X - Viewport.X + Position.X - ScrolledPosition.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            if (ry >= Size.Height - TrailingBlank)
            {
                DraggingNode = null;
                UpdateHoverDrag();
                return;
            }
            int globalindex = (int)Math.Floor(ry / 24d);
            int index = 0;
            TreeNode n = null;
            for (int i = 0; i < Nodes.Count; i++)
            {
                n = Nodes[i].FindNodeIndex(globalindex - index);
                if (n != null) break;
                index += Nodes[i].GetDisplayedNodeCount() + 1;
            }
            if (n == null)
            {
                DraggingNode = null;
                UpdateHoverDrag();
            }
            int nodex = n.PixelsIndented;
            if (n.Nodes.Count > 0 && rx < nodex + 10 && rx > nodex - 10)
            {
                TreeNode selnode = SelectedNode;
                n.Collapsed = !n.Collapsed;
                if (n.Collapsed && n.ContainsNode(SelectedNode)) SelectedNode = n;
                OnNodeCollapseChanged?.Invoke(n, selnode);
            }
            else
            {
                DraggingNode = n;
            }
        }
        if (SelectedNode != oldselected)
        {
            OnSelectedNodeChanged?.Invoke(e);
            Redraw();
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        if (Mouse.LeftMouseReleased && Mouse.LeftStartedInside)
        {
            if (DraggingNode != null && HoveringNode != null)
            {
                if (DraggingNode == HoveringNode)
                {
                    SelectedNode = HoveringNode;
                    OnSelectedNodeChanged?.Invoke(e);
                    Redraw();
                }
                else
                {
                    OnDragAndDropped?.Invoke(new BaseEventArgs());
                }
            }
            DraggingNode = null;
            UpdateHoverDrag();
            Redraw();
        }
    }

    public override void Update()
    {
        base.Update();
        if (OldHoveringNode != HoveringNode)
        {
            if (TimerExists("long_hover")) DestroyTimer("long_hover");
        }
        if (DraggingNode != null)
        {
            if (HoveringNode != null && HoveringNode.Collapsed && HoveringNode.Nodes.Count > 0 && HoverOver && !TimerExists("long_hover"))
            {
                SetTimer("long_hover", 500);
            }
            else if (HoveringNode != null && HoveringNode.Collapsed && HoveringNode.Nodes.Count > 0 && HoverOver && TimerPassed("long_hover"))
            {
                DestroyTimer("long_hover");
                HoveringNode.Collapsed = false;
                OnNodeCollapseChanged?.Invoke(HoveringNode, SelectedNode);
                Redraw();
            }
            else if (TimerExists("long_hover") && (HoveringNode == null || !HoveringNode.Collapsed || HoveringNode.Nodes.Count == 0 || !HoverOver))
            {
                DestroyTimer("long_hover");
            }
        }
        else
        {
            if (TimerExists("long_hover")) DestroyTimer("long_hover");
        }
        OldHoveringNode = HoveringNode;
    }
}

public class TreeNode : ICloneable
{
    public string Name;
    public object Object = "treeNode";
    public bool Collapsed = true;
    public List<TreeNode> Nodes = new List<TreeNode>();
    public int PixelsIndented = 0;

    public TreeNode()
    {

    }

    public int GetDisplayedNodeCount()
    {
        int count = 0;
        if (Collapsed) return 0;
        for (int i = 0; i < Nodes.Count; i++)
        {
            count++;
            count += Nodes[i].GetDisplayedNodeCount();
        }
        return count;
    }

    public TreeNode FindNodeIndex(int Index = 0)
    {
        if (Index == 0) return this;
        if (Collapsed)
        {
            return null;
        }
        for (int i = 0; i < Nodes.Count; i++)
        {
            Index -= 1;
            TreeNode n = Nodes[i].FindNodeIndex(Index);
            if (n != null) return n;
            Index -= Nodes[i].GetDisplayedNodeCount();
        }
        return null;
    }

    public bool ContainsNode(TreeNode n)
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i] == n) return true;
            if (Nodes[i].ContainsNode(n)) return true;
        }
        return false;
    }

    public TreeNode RemoveNode(TreeNode n)
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i] == n)
            {
                Nodes.RemoveAt(i);
                if (Nodes.Count == 0) return this;
                return i >= Nodes.Count ? Nodes[i - 1] : Nodes[i];
            }
            else if (Nodes[i].ContainsNode(n))
            {
                return Nodes[i].RemoveNode(n);
            }
        }
        return null;
    }

    public TreeNode FindNode(Predicate<TreeNode> match)
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            bool it = match.Invoke(Nodes[i]);
            if (it) return Nodes[i];
            else
            {
                TreeNode n = Nodes[i].FindNode(match);
                if (n != null) return n;
            }
        }
        return null;
    }

    public TreeNode FindVisibleNode(Predicate<TreeNode> match)
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            bool it = match.Invoke(Nodes[i]);
            if (it) return Nodes[i];
            else if (!Nodes[i].Collapsed)
            {
                TreeNode n = Nodes[i].FindVisibleNode(match);
                if (n != null) return n;
            }
        }
        return null;
    }

    public TreeNode FindParentNode(Predicate<TreeNode> match)
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            bool it = match.Invoke(Nodes[i]);
            if (it) return this;
            else
            {
                TreeNode n = Nodes[i].FindParentNode(match);
                if (n != null) return n;
            }
        }
        return null;
    }

    public object Clone()
    {
        TreeNode n = new TreeNode();
        n.Name = Name;
        n.Object = Object;
        n.Collapsed = Collapsed;
        n.Nodes = Nodes.ConvertAll(n => (TreeNode)n.Clone());
        n.PixelsIndented = PixelsIndented;
        return n;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (obj is TreeNode)
        {
            TreeNode n = (TreeNode) obj;
            return Name == n.Name &&
                   Object.Equals(n.Object) &&
                   Collapsed == n.Collapsed &&
                   Nodes.Equals(n.Nodes) &&
                   PixelsIndented == n.PixelsIndented;
        }
        return false;
    }
}