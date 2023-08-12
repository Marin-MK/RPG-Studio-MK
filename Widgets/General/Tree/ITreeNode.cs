using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public interface ITreeNode
{
    public TreeNode Root { get; }
    public TreeNode Parent { get; }
    public int Depth { get; }
    public bool Selectable { get; }
    public bool Draggable { get; }
    public bool CanDragOver { get; }

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

    public void SetRoot(TreeNode Root);
    public void SetParent(TreeNode Parent);
    public void SetDepth(int Depth);
    public void Delete(bool DeleteChildren);

    /// <summary>
    /// Returns a list of this node's ancestors, in order of ascending depth.
    /// </summary>
    /// <returns>A list of nodes.</returns>
    public List<TreeNode> GetAncestors();

    public ITreeNode Clone(TreeNode Root, TreeNode Parent);
}