using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

[DebuggerDisplay("==========")]
public class OptimizedNodeSeparator : IOptimizedNode
{
    public OptimizedNode Root { get; protected set; }
    public OptimizedNode Parent { get; protected set; }
    public int Depth { get; protected set; }
    public int Height { get; protected set; }
    public bool Selectable { get; protected set; } = true;
    public bool Draggable { get; protected set; } = false;
    public bool CanDragOver { get; protected set; } = false;

    public OptimizedNodeSeparator(int Height = 24)
    {
        this.Height = Height;
    }

    /// <summary>
    /// Called when this node gets a different parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnParentChanged { get; }
    /// <summary>
    /// Called when this node gets a different root.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnRootChanged { get; }
    /// <summary>
    /// Called when the depth of this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnDepthChanged { get; }

    public void SetRoot(OptimizedNode Root)
    {
        if (this.Root != Root)
        {
            OptimizedNode OldRoot = this.Root;
            this.Root = Root;
            OnRootChanged?.Invoke(new ObjectEventArgs(this.Root, OldRoot));
        }
    }

    public void SetParent(OptimizedNode Parent)
    {
        if (this.Parent != Parent)
        {
            OptimizedNode OldParent = this.Parent;
            this.Parent = Parent;
            OnParentChanged?.Invoke(new ObjectEventArgs(this.Parent, OldParent));
        }
    }

    public void SetDepth(int Depth)
    {
        if (this.Depth != Depth)
        {
            int OldDepth = this.Depth;
            this.Depth = Depth;
            OnDepthChanged?.Invoke(new ObjectEventArgs(this.Depth, OldDepth));
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
}