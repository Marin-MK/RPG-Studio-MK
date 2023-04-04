using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public interface IOptimizedNode
{
    public OptimizedNode Root { get; }
    public OptimizedNode Parent { get; }
    public int Depth { get; }
    public bool Selectable { get; }
    public bool Draggable { get; }
    public bool CanDragOver { get; }

    /// <summary>
    /// Called when this node gets a different parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<OptimizedNode> OnParentChanged { get; }
    /// <summary>
    /// Called when this node gets a different root.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<OptimizedNode> OnRootChanged { get; }
    /// <summary>
    /// Called when the depth of this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<int> OnDepthChanged { get; }

    public void SetRoot(OptimizedNode Root);
    public void SetParent(OptimizedNode Parent);
    public void SetDepth(int Depth);
    public void Delete(bool DeleteChildren);

    /// <summary>
    /// Returns a list of this node's ancestors, in order of ascending depth.
    /// </summary>
    /// <returns>A list of nodes.</returns>
    public List<OptimizedNode> GetAncestors();

    public IOptimizedNode Clone(OptimizedNode Root, OptimizedNode Parent);
}