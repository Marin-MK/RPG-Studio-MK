using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

[DebuggerDisplay("{Text}")]
public class OptimizedNode : IOptimizedNode
{
    /// <summary>
    /// The root node of the entire tree.
    /// </summary>
    public OptimizedNode Root { get; protected set; }
    /// <summary>
    /// A reference to the parent node of this node.
    /// </summary>
    public OptimizedNode Parent { get; protected set; }
    /// <summary>
    /// A list of child nodes of this node.
    /// </summary>
    public List<IOptimizedNode> Children { get; protected set; }
    /// <summary>
    /// Whether this node has children.
    /// </summary>
    public bool HasChildren => Children.Count > 0;
    /// <summary>
    /// The global index in a tree view. If all nodes were expanded,
    /// this would be the position of this node in the overall list, excluding any non-node items that don't have a global index.
    /// </summary>
    public int GlobalIndex { get; protected set; }
    /// <summary>
    /// The text of this node.
    /// </summary>
    public string Text { get; protected set; }
    /// <summary>
    /// An object associated with this node.
    /// </summary>
    public object Object { get; protected set; }
    /// <summary>
    /// Whether this node is in its expanded view, i.e. you can see its children.
    /// </summary>
    public bool Expanded { get; protected set; }
    /// <summary>
    /// The depth of the tree at this node.
    /// </summary>
    public int Depth { get; protected set; }
    /// <summary>
    /// Whether this node can be selected and hovered over.
    /// </summary>
    public bool Selectable { get; protected set; } = true;
    /// <summary>
    /// Whether this node can be drag-and-dropped.
    /// </summary>
    public bool Draggable { get; protected set; } = true;
    /// <summary>
    /// Whether a node can be drag-and-dropped over this node.
    /// </summary>
    public bool CanDragOver { get; protected set; } = true;

    /// <summary>
    /// Called when this node is expanded, regardless of whether it has children.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public BaseEvent OnExpanded;
    /// <summary>
    /// Called when this node is collapsed, regardless of whether it has children.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public BaseEvent OnCollapsed;
    /// <summary>
    /// Called when this node is expanded or collapsed, regardless of whether it has children.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<bool> OnExpansionChanged;
    /// <summary>
    /// Called when the text of this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<string> OnTextChanged;
    /// <summary>
    /// Called when the object associated with this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnObjectChanged;
    /// <summary>
    /// Called when a node is inserted as a child of this node.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<IOptimizedNode> OnNodeInserted;
    /// <summary>
    /// Called when the global index of this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<int> OnGlobalIndexChanged;
    /// <summary>
    /// Called when this node gets a different parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<OptimizedNode> OnParentChanged { get; set; }
    /// <summary>
    /// Called when this node gets a different root.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<OptimizedNode> OnRootChanged { get; set; }
    /// <summary>
    /// Called when the depth of this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public GenericObjectEvent<int> OnDepthChanged { get; set; }

    public OptimizedNode(string Text, object Object = null)
    {
        this.Text = Text;
        this.Object = Object;
        Children = new List<IOptimizedNode>();
        Root = this;
    }

    /// <summary>
    /// Sets whether this node is expanded or collapsed.
    /// </summary>
    /// <param name="Expanded">The expansion state.</param>
    public void SetExpanded(bool Expanded)
    {
        if (this.Expanded != Expanded)
        {
            this.Expanded = Expanded;
            if (this.Expanded) OnExpanded?.Invoke(new BaseEventArgs());
            else OnCollapsed?.Invoke(new BaseEventArgs());
            OnExpansionChanged?.Invoke(new GenericObjectEventArgs<bool>(this.Expanded, !this.Expanded));
        }
    }

    /// <summary>
    /// Expands this node.
    /// </summary>
    public void Expand()
    {
        SetExpanded(true);
    }

    /// <summary>
    /// Collapses this node.
    /// </summary>
    public void Collapse()
    {
        SetExpanded(false);
    }

    /// <summary>
    /// Sets the text of this node.
    /// </summary>
    /// <param name="Text">The new text of the node.</param>
    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            OnTextChanged?.Invoke(new GenericObjectEventArgs<string>(this.Text));
        }
    }

    /// <summary>
    /// Sets the object associated with this node.
    /// </summary>
    /// <param name="Object">The object to associate with this node.</param>
    public void SetObject(object Object)
    {
        if (this.Object != Object)
        {
            object OldObject = this.Object;
            this.Object = Object;
            OnObjectChanged?.Invoke(new ObjectEventArgs(this.Object, OldObject));
        }
    }

    /// <summary>
    /// Gets the next sibling of this node.
    /// </summary>
    /// <returns>The next sibling node, or null if it doesn't exist.</returns>
    public OptimizedNode GetNextSibling()
    {
        if (Parent == null) return null;
        int CurrentIndex = Parent.Children.IndexOf(this) + 1;
        while (CurrentIndex < Parent.Children.Count)
        {
            if (Parent.Children[CurrentIndex] is OptimizedNode)
                return (OptimizedNode) Parent.Children[CurrentIndex];
            CurrentIndex++;
        }
        return null;
    }

    /// <summary>
    /// Gets the previous sibling of this node.
    /// </summary>
    /// <returns>The previous sibling node, or null if it doesn't exist.</returns>
    public OptimizedNode GetPreviousSibling()
    {
        if (Parent == null) return null;
        int CurrentIndex = Parent.Children.IndexOf(this) - 1;
        while (CurrentIndex >= 0)
        {
            if (Parent.Children[CurrentIndex] is OptimizedNode)
                return (OptimizedNode) Parent.Children[CurrentIndex];
            CurrentIndex--;
        }
        return null;
    }

    /// <summary>
    /// Returns the very last node of the very last child.
    /// </summary>
    /// <returns>The last node in this node child hierarchy, or itself if no children exist.</returns>
    public OptimizedNode GetLastNode(bool IgnoreExpansion = true)
    {
        if (HasChildren && (IgnoreExpansion || Expanded))
        {
            int Index = Children.Count - 1;
            while (Index >= 0)
            {
                if (Children[Index] is OptimizedNode)
                    return ((OptimizedNode) Children[Index]).GetLastNode(IgnoreExpansion);
                Index--;
            }
            return this;
        }
        return this;
    }

    /// <summary>
    /// Gets the node with its global index one lower than this node's.
    /// </summary>
    /// <returns>The previous node in the hierarchy, or null if it doesn't exist.</returns>
    public OptimizedNode GetPreviousNode(bool IgnoreExpansion = true)
    {
        // The previous node can be found in one of two ways:
        // - If we have a previous sibling, the previous node is the last node of our previous sibling.
        // - If we have no previous siblings, the previous node is our parent.
        OptimizedNode PreviousSibling = GetPreviousSibling();
        if (PreviousSibling != null) return PreviousSibling.GetLastNode(IgnoreExpansion);
        return Parent;
    }

    /// <summary>
    /// Gets the node with its global index one higher than this node's.
    /// </summary>
    /// <returns>The next node in the hierarchy, or null if it doesn't exist.</returns>
    public OptimizedNode GetNextNode(bool IgnoreExpansion = true)
    {
        // If this node has children, the next node is the first child.
        // If we don't have children, the next node is our next sibling.
        // If we don't have a next sibling, the next node is our parent's next sibling, etc. recursively/iteratively.
        if (HasChildren && (IgnoreExpansion || Expanded))
        {
            int Index = 0;
            while (Index < Children.Count)
            {
                if (Children[Index] is OptimizedNode)
                    return (OptimizedNode) Children[Index];
                Index++;
            }
        }
        OptimizedNode TestNode = this;
        while (TestNode != null)
        {
            OptimizedNode NextSibling = TestNode.GetNextSibling();
            if (NextSibling != null) return NextSibling;
            TestNode = TestNode.Parent;
        }
        return null;
    }

    /// <summary>
    /// Returns the node that passes the predicate if it exists, or null otherwise.
    /// </summary>
    /// <param name="Predicate">The condition for finding the node.</param>
    /// <param name="IgnoreSelf">Whether to ignore itself and only check its children.</param>
    /// <param name="IgnoreExpansion">Whether to ignore whether a node is expanded or not.</param>
    /// <returns>The node if it exists, or null otherwise.</returns>
    public OptimizedNode? GetNode(Func<OptimizedNode, bool> Predicate, bool IgnoreSelf = true, bool IgnoreExpansion = true)
    {
        if (!IgnoreSelf && Predicate(this)) return this;
        if (IgnoreExpansion || Expanded)
        {
            foreach (IOptimizedNode Child in Children)
            {
                if (Child is not OptimizedNode) continue;
                OptimizedNode? Node = ((OptimizedNode) Child).GetNode(Predicate, IgnoreExpansion);
                if (Node != null) return Node;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns a list of this node's ancestors, in order of ascending depth.
    /// </summary>
    /// <returns>A list of nodes.</returns>
    public List<OptimizedNode> GetAncestors()
    {
        if (this == Root) return new List<OptimizedNode>() { };
        List<OptimizedNode> List = new List<OptimizedNode>();
        List.AddRange(((IOptimizedNode) Parent).GetAncestors());
        List.Add(Parent);
        return List;
    }

    /// <summary>
    /// Returns whether the specified node is contained by any of this node's children.
    /// </summary>
    /// <param name="Node">The node to search for.</param>
    /// <returns>Whether the node is part of one of this node's children.</returns>
    public bool Contains(IOptimizedNode Node, bool IgnoreExpansion = true)
    {
        if (IgnoreExpansion || Expanded) return Children.Any(c =>
        {
            if (c == Node) return true;
            if (c is not OptimizedNode) return false;
            return ((OptimizedNode) c).Contains(Node, IgnoreExpansion);
        });
        return false;
    }

    /// <summary>
    /// Increases the global index of all nodes starting at the specified index.
    /// </summary>
    /// <param name="Index">The index to start incrementing from.</param>
    public void ChangeIndexFrom(int Index, int Count)
    {
        if (GlobalIndex >= Index)
        {
            GlobalIndex += Count;
            OnGlobalIndexChanged?.Invoke(new GenericObjectEventArgs<int>(GlobalIndex, GlobalIndex - Count));
        }
        Children.FindAll(c => c is OptimizedNode).ForEach(c => ((OptimizedNode) c).ChangeIndexFrom(Index, Count));
    }

    /// <summary>
    /// Inserts a child at the specified index and increments all global indices accordingly.
    /// </summary>
    /// <param name="Index">The index to insert the child node at.</param>
    /// <param name="NewNode">The node to insert.</param>
    public void InsertChild(int Index, IOptimizedNode NewNode)
    {
        // Automatically expand a node if a child is added to it, and it did not have any children before.
        if (!HasChildren) Expanded = true;
        // We're inserting in place of another node.
        if (Index < Children.Count)
        {
            // Increment all nodes following this node by the size of the node we're adding.
            IOptimizedNode OldNode = Children[Index];
            // Record the old global index of the node in this position; this is where we will start
            // with setting the global index of the new node.
            int OldIndex = -1;
            if (OldNode is OptimizedNode) OldIndex = ((OptimizedNode) OldNode).GlobalIndex;
            else
            {
                // We're inserting at a non-node object, so we find the previous node and use its global index instead.
                int CurrentIndex = Index - 1;
                while (CurrentIndex >= 0)
                {
                    if (Children[CurrentIndex] is OptimizedNode)
                    {
                        // Plus 1 because we don't want to change our previous sibling's global index.
                        OldIndex = ((OptimizedNode) Children[CurrentIndex]).GetLastNode().GlobalIndex + 1;
                        break;
                    }
                    CurrentIndex--;
                }
                if (CurrentIndex == -1)
                {
                    // No previous node sibling exists, so our parent is the previous index-containing node.
                    // Plus 1 because we don't want to change our parent's global index.
                    OldIndex = Parent.GlobalIndex + 1;
                }
            }
            // The size of the new node.
            int Size = NewNode is OptimizedNode ? ((OptimizedNode) NewNode).GetTotalNodeCount() : 0;
            // Add to the global indices of all nodes following the new node based on the size of the new node.
            Root.ChangeIndexFrom(OldIndex, Size);
            // Insert the new node.
            Children.Insert(Index, NewNode);
            // Set the proper global indices for this new node.
            if (NewNode is OptimizedNode) ((OptimizedNode) NewNode).SetIndicesSequentially(OldIndex);
        }
        else
        {
            // We're adding to the end of the list of children.
            // There is no node we're replacing, so we don't know the new global index yet.
            // To find this index, we find the previous node right above our current node, although this may
            // be a child buried deep into the previous sibling.
            OptimizedNode LastNode = null;
            if (HasChildren)
            {
                int CurrentIndex = Children.Count - 1;
                while (CurrentIndex >= 0)
                {
                    if (Children[CurrentIndex] is OptimizedNode)
                    {
                        LastNode = ((OptimizedNode) Children[CurrentIndex]).GetLastNode();
                        break;
                    }
                    CurrentIndex--;
                }
                if (CurrentIndex == -1)
                {
                    // Could not find an index-containing child, so the last global index is the node we're adding to.
                    LastNode = this;
                }
            }
            else LastNode = this;
            // The new global index of the node is the global index of the last node prior to the new node + 1.
            int NewIndex = LastNode.GlobalIndex + 1;
            // The size of the new node.
            int Size = NewNode is OptimizedNode ? ((OptimizedNode) NewNode).GetTotalNodeCount() : 0;
            // Add to the global indices of all nodes following the new node based on the size of the new node.
            Root.ChangeIndexFrom(NewIndex, Size);
            // Add the new node.
            Children.Add(NewNode);
            // Set the proper global indices for this new node.
            if (NewNode is OptimizedNode) ((OptimizedNode) NewNode).SetIndicesSequentially(NewIndex);
        }
        NewNode.SetRoot(Root);
        NewNode.SetParent(this);
        NewNode.SetDepth(Depth + 1);
        OnNodeInserted?.Invoke(new GenericObjectEventArgs<IOptimizedNode>(NewNode));
    }

    /// <summary>
    /// Deletes this node from its parent, properly updating both its parent's tree and its own tree.
    /// </summary>
    public void Delete(bool DeleteChildren)
    {
        if (this.Parent == null && !DeleteChildren && HasChildren) throw new Exception("Cannot flatten children into parent, because this node is the root node and it does not have a parent.");
        int Size = DeleteChildren ? GetTotalNodeCount() : 1;
        int MaxIndex = DeleteChildren ? (GetLastNode()?.GlobalIndex ?? this.GlobalIndex) : this.GlobalIndex;
        Root.ChangeIndexFrom(MaxIndex + 1, -Size);
        int Index = Parent.Children.IndexOf(this);
        Parent.Children.Remove(this);
        if (DeleteChildren)
        {
            this.SetRoot(this);
            this.SetDepth(0);
            this.SetIndicesSequentially(0);
        }
        else if (this.Children.Count > 0)
        {
            // Flatten the children into the parent list
            Parent.Children.InsertRange(Index, this.Children);
            this.Children.ForEach(c =>
            {
                c.SetParent(Parent);
                c.SetDepth(c.Depth - 1);
            });
        }
        this.Parent = null;
    }

    /// <summary>
    /// Recursively sets the root of this node and all its children to the specified node.
    /// </summary>
    /// <param name="Root">The new root node of this node and all its children.</param>
    public void SetRoot(OptimizedNode Root)
    {
        if (this.Root != Root)
        {
            OptimizedNode OldRoot = this.Root;
            this.Root = Root;
            OnRootChanged?.Invoke(new GenericObjectEventArgs<OptimizedNode>(this.Root, OldRoot));
        }
        Children.ForEach(c => c.SetRoot(Root));
    }

    public void SetParent(OptimizedNode Parent)
    {
        if (this.Parent != Parent)
        {
            OptimizedNode OldParent = this.Parent;
            this.Parent = Parent;
            OnParentChanged?.Invoke(new GenericObjectEventArgs<OptimizedNode>(this.Parent, OldParent));
        }
    }

    /// <summary>
    /// Sets the depth of this node and all child nodes.
    /// </summary>
    /// <param name="Count">The new depth of the node.</param>
    public void SetDepth(int Depth)
    {
        if (this.Depth != Depth)
        {
            int OldDepth = this.Depth;
            this.Depth = Depth;
            OnDepthChanged?.Invoke(new GenericObjectEventArgs<int>(this.Depth, OldDepth));
        }
        Children.ForEach(c => c.SetDepth(Depth + 1));
    }

    /// <summary>
    /// Sets whether this node can be selected and hovered over.
    /// </summary>
    /// <param name="Selectable">Whether the node can be selected and hovered over.</param>
    public void SetSelectable(bool Selectable)
    {
        if (this.Selectable != Selectable)
        {
            this.Selectable = Selectable;
        }
    }

    /// <summary>
    /// Sets whether this node can be drag-and-dropped.
    /// </summary>
    /// <param name="Draggable">Whether the node can be drag-and-dropped.</param>
    public void SetDraggable(bool Draggable)
    {
        if (this.Draggable != Draggable)
        {
            this.Draggable = Draggable;
        }
    }

    /// <summary>
    /// Sets whether a node can be dropped over this node.
    /// </summary>
    /// <param name="CanDragOver"></param>
    public void SetCanDragOver(bool CanDragOver)
    {
        if (this.CanDragOver != CanDragOver)
        {
            this.CanDragOver = CanDragOver;
        }
    }

    public void ClearChildren()
    {
        this.Children.Clear();
    }

    /// <summary>
    /// Adds a child node as the last in the list of children.
    /// </summary>
    /// <param name="NewNode">The node to add.</param>
    public void AddChild(IOptimizedNode NewNode)
    {
        InsertChild(Children.Count, NewNode);
    }

    /// <summary>
    /// Returns the number of nodes this node's children contain, and includes itself.
    /// For instance, a node with 3 children (all without children of themselves), will yield a count of 4.
    /// </summary>
    /// <returns>The number of nodes this node contains in total.</returns>
    public int GetTotalNodeCount(bool IgnoreExpansion = true)
    {
        int Count = 1;
        if (IgnoreExpansion || Expanded) Count += Children.Sum(c => c is OptimizedNode ? ((OptimizedNode) c).GetTotalNodeCount(IgnoreExpansion) : 0);
        return Count;
    }

    /// <summary>
    /// Returns the number of (visible) child nodes and the height of all separators in this tree.
    /// </summary>
    /// <returns></returns>
    public (int NodeCount, int SeparatorHeightSum) GetChildrenHeight(bool IgnoreExpansion = true)
    {
        int NodeCount = 0;
        int SeparatorHeight = 0;
        if (IgnoreExpansion || Expanded) Children.ForEach(c =>
        {
            if (c is OptimizedNode)
            {
                NodeCount++;
                (int NodeCount, int SepHeight) result = ((OptimizedNode) c).GetChildrenHeight(IgnoreExpansion);
                NodeCount += result.NodeCount;
                SeparatorHeight += result.SepHeight;
            }
            else if (c is OptimizedNodeSeparator)
            {
                SeparatorHeight += ((OptimizedNodeSeparator) c).Height;
            }
        });
        return (NodeCount, SeparatorHeight);
    }

    public (int NodeCount, int SeparatorHeight) GetChildrenHeightUntil(IOptimizedNode NodeToStopAt, bool IgnoreExpansion = true)
    {
        int NodeCount = 0;
        int SeparatorHeight = 0;
        if (IgnoreExpansion || Expanded)
        {
            foreach (IOptimizedNode c in Children)
            {
                if (c == NodeToStopAt) return (NodeCount, SeparatorHeight);
                if (c is OptimizedNode)
                {
                    NodeCount++;
                    (int NodeCount, int SepHeight) result = ((OptimizedNode) c).GetChildrenHeightUntil(NodeToStopAt, IgnoreExpansion);
                    NodeCount += result.NodeCount;
                    SeparatorHeight += result.SepHeight;
                }
                else if (c is OptimizedNodeSeparator)
                {
                    SeparatorHeight += ((OptimizedNodeSeparator) c).Height;
                }
            }
        }
        return (NodeCount, SeparatorHeight);
    }

    /// <summary>
    /// Sets the global indices of this node and all its children to sequential values starting at a specific value.
    /// </summary>
    /// <param name="Start">The value to start at.</param>
    /// <returns>The new maximum global index + 1; used in the algorithm only.</returns>
    public int SetIndicesSequentially(int Start)
    {
        int CurrentIndex = Start;
        if (GlobalIndex != CurrentIndex)
        {
            int OldIndex = GlobalIndex;
            GlobalIndex = CurrentIndex;
            OnGlobalIndexChanged?.Invoke(new GenericObjectEventArgs<int>(GlobalIndex, OldIndex));
        }
        CurrentIndex++;
        foreach (IOptimizedNode Child in Children)
        {
            if (Child is not OptimizedNode) continue;
            CurrentIndex = ((OptimizedNode) Child).SetIndicesSequentially(CurrentIndex);
        }
        return CurrentIndex;
    }

    public List<IOptimizedNode> GetAllChildren(bool IgnoreExpansion = true)
    {
        List<IOptimizedNode> List = new List<IOptimizedNode>();
        if (IgnoreExpansion || this.Expanded)
            Children.ForEach(c =>
            {
                List.Add(c);
                if (c is OptimizedNode && (IgnoreExpansion || ((OptimizedNode) c).Expanded))
                    List.AddRange(((OptimizedNode) c).GetAllChildren(IgnoreExpansion));
            });
        return List;
    }

    /// <summary>
    /// Returns the text property of the node.
    /// </summary>
    /// <returns>The text property of the node.</returns>
    public override string ToString()
    {
        return Text;
    }

    /// <summary>
    /// Makes a deep copy of the node and all its children.
    /// </summary>
    /// <returns>The copied node.</returns>
    public IOptimizedNode Clone(OptimizedNode Root = null, OptimizedNode Parent = null)
    {
        OptimizedNode n = new OptimizedNode(this.Text, this.Object);
        if (Root is null)
        {
            Root = n;
            Parent = null;
        }
        n.Root = Root;
        n.Parent = Parent;
        n.Children = this.Children.Select(c => c.Clone(Root, n)).ToList();
        n.GlobalIndex = this.GlobalIndex;
        n.Expanded = this.Expanded;
        n.Depth = this.Depth;
        n.Selectable = this.Selectable;
        n.Draggable = this.Draggable;
        n.CanDragOver = this.CanDragOver;
        return n;
    }
}