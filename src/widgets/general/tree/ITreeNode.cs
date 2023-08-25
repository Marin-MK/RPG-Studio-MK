using System.Collections.Generic;
using System.Diagnostics;

namespace RPGStudioMK.Widgets;

public interface ITreeNode
{
    /// <summary>
    /// The root node of the entire tree.
    /// </summary>
    public TreeNode Root { get; }
    /// <summary>
    /// A reference to the parent node of this node.
    /// </summary>
    public TreeNode Parent { get; }
    /// <summary>
    /// The depth of the tree at this node.
    /// </summary>
    public int Depth { get; }
    /// <summary>
    /// Whether this node can be selected.
    /// </summary>
    public bool Selectable { get; }
    /// <summary>
    /// Whether this node is drag-and-drop-able.
    /// </summary>
    public bool Draggable { get; }
    /// <summary>
    /// Whether drag-and-drop can be released over this node.
    /// </summary>
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

    /// <summary>
    /// Sets the root node of this node.
    /// </summary>
    /// <param name="Root"></param>
    public void SetRoot(TreeNode Root);
    /// <summary>
    /// Sets the parent of this node.
    /// </summary>
    /// <param name="Parent">The new parent node.</param>
    public void SetParent(TreeNode Parent);
    /// <summary>
    /// Sets the depth of the tree at this node.
    /// </summary>
    /// <param name="Depth">The new tree depth.</param>
    public void SetDepth(int Depth);
    /// <summary>
    /// Deletes this node from its parent.
    /// </summary>
    /// <param name="DeleteChildren">Whether to delete or concatenate this node's children in the parent child list.</param>
    public void Delete(bool DeleteChildren);

    /// <summary>
    /// Returns a list of this node's ancestors, in order of ascending depth.
    /// </summary>
    /// <returns>A list of nodes.</returns>
    public List<TreeNode> GetAncestors();

    /// <summary>
    /// Clones this node.
    /// </summary>
    /// <param name="Root">The new root node.</param>
    /// <param name="Parent">The new parent node.</param>
    /// <returns>The cloned node.</returns>
    public ITreeNode Clone(TreeNode Root, TreeNode Parent);
}