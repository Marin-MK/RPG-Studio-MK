using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RPGStudioMK.Widgets;

[DebuggerDisplay("==========")]
public class TreeNodeSeparator : ITreeNode
{
    public TreeNode Root { get; protected set; }
    public TreeNode Parent { get; protected set; }
    public int Depth { get; protected set; }
    public int Height { get; protected set; }
    public bool Selectable { get; protected set; } = true;
    public bool Draggable { get; protected set; } = false;
    public bool CanDragOver { get; protected set; } = false;

    public TreeNodeSeparator(int Height = 24)
    {
        this.Height = Height;
    }

    /// <summary>
    /// Called when this node gets a different parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<TreeNode> OnParentChanged { get; }
    /// <summary>
    /// Called when this node gets a different root.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<TreeNode> OnRootChanged { get; }
    /// <summary>
    /// Called when the depth of this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<int> OnDepthChanged { get; }

    public void SetRoot(TreeNode Root)
    {
        if (this.Root != Root)
        {
            TreeNode OldRoot = this.Root;
            this.Root = Root;
            OnRootChanged?.Invoke(new GenericObjectEventArgs<TreeNode>(this.Root, OldRoot));
        }
    }

    public void SetParent(TreeNode Parent)
    {
        if (this.Parent != Parent)
        {
            TreeNode OldParent = this.Parent;
            this.Parent = Parent;
            OnParentChanged?.Invoke(new GenericObjectEventArgs<TreeNode>(this.Parent, OldParent));
        }
    }

    public void SetDepth(int Depth)
    {
        if (this.Depth != Depth)
        {
            int OldDepth = this.Depth;
            this.Depth = Depth;
            OnDepthChanged?.Invoke(new GenericObjectEventArgs<int>(this.Depth, OldDepth));
        }
    }

    public void SetHeight(int Height)
    {
        if (this.Height != Height)
        {
            this.Height = Height;
        }
    }

    public void SetSelectable(bool Selectable)
    {
        if (this.Selectable != Selectable)
        {
            this.Selectable = Selectable;
        }
    }

    public void SetDraggable(bool Draggable)
    {
        if (this.Draggable != Draggable)
        {
            this.Draggable = Draggable;
        }
    }

    public void SetCanDragOver(bool CanDragOver)
    {
        if (this.CanDragOver != CanDragOver) 
        {
            this.CanDragOver = CanDragOver;
        }
    }

    public void Delete(bool _)
    {
        Parent.Children.Remove(this);
    }

    /// <summary>
    /// Returns a list of this node's ancestors, in order of ascending depth.
    /// </summary>
    /// <returns>A list of nodes.</returns>
    public List<TreeNode> GetAncestors()
    {
        List<TreeNode> List = new List<TreeNode>();
        List.AddRange(Parent.GetAncestors());
        List.Add(Parent);
        return List;
    }

    /// <summary>
    /// Makes a deep copy of the node.
    /// </summary>
    /// <returns>The copied new.</returns>
    public ITreeNode Clone(TreeNode Root = null, TreeNode Parent = null)
    {
        if (Root is null) throw new ArgumentException($"Node separators cannot be root nodes.");
        TreeNodeSeparator n = new TreeNodeSeparator(this.Height);
        n.Root = Root;
        n.Parent = Parent;
        n.Depth = this.Depth;
        n.Height = this.Height;
        n.Selectable = this.Selectable;
        n.Draggable = this.Draggable;
        n.CanDragOver = this.CanDragOver;
        return n;
    }
}