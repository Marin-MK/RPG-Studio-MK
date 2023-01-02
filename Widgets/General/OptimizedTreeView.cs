using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	public OptimizedNode Root { get; protected set; }
	public int LineHeight { get; protected set; } = 24;
	public int DepthIndent { get; protected set; } = 20;
	public int TextX { get; protected set; } = 0;
	public int TextY { get; protected set; } = 0;
	public OptimizedNode? HoveringNode { get; protected set; }
	public List<OptimizedNode> SelectedNodes { get; protected set; } = new List<OptimizedNode>();
	public bool MultipleSelected => SelectedNodes.Count > 1;
	public OptimizedNode? SelectedNode => SelectedNodes.Count > 0 ? SelectedNodes[0] : null;

	Dictionary<OptimizedNode, int> LastDrawPositions = new Dictionary<OptimizedNode, int>();

	Container ScrollContainer;

	Sprite BG => ScrollContainer.Sprites["bg"];

	public OptimizedTreeView(IContainer Parent) : base(Parent)
	{
		ScrollContainer = new Container(this);
		ScrollContainer.SetDocked(true);
		ScrollContainer.Sprites["bg"] = new Sprite(this.Viewport);
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
			this.RedrawAllNodes();
		}
	}

	public void SetDepthIndent(int DepthIndent)
	{
		if (this.DepthIndent != DepthIndent)
		{
			this.DepthIndent = DepthIndent;
			this.RedrawAllNodes();
		}
	}

	public void SetTextX(int TextX)
	{
		if (this.TextX != TextX)
		{
			this.TextX = TextX;
			BG.X = TextX;
		}
	}

	public void SetTextY(int TextY)
	{
		if (this.TextY != TextY)
		{
			this.TextY = TextY;
			BG.Y = TextY;
		}
	}

	public void RedrawAllNodes()
	{
		BG.Bitmap?.Dispose();
		BG.Bitmap = new Bitmap(Size.Width, Root.GetTotalNodeCount(false) * LineHeight);
		BG.Bitmap.Unlock();
		BG.Bitmap.Font = Fonts.Paragraph;
		List<IOptimizedNode> en = Root.GetAllChildren(false);
		int y = 0;
        for (int i = 0; i < en.Count; i++)
		{
			IOptimizedNode Node = en[i];
			for (int j = 0; j < Node.Depth; j++) Console.Write("  ");
			Console.WriteLine($"- {Node}");
			int x = (Node.Depth - 1) * DepthIndent;
			BG.Bitmap.DrawText(Node is OptimizedNode ? ((OptimizedNode) Node).Text : "-----", x, y, Color.WHITE);
			y += LineHeight;
		}
		BG.Bitmap.Lock();
	}
}

public interface IOptimizedNode
{
	public OptimizedNode Root { get; }
    public OptimizedNode? Parent { get; }
	public int Depth { get; }

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

    public void SetRoot(OptimizedNode Root);
	public void SetParent(OptimizedNode? Parent);
    public void SetDepth(int Depth);
}

public class OptimizedNodeSeperator : IOptimizedNode
{
	public OptimizedNode Root { get; protected set; }
	public OptimizedNode? Parent { get; protected set; }
	public int Depth { get; protected set; }
	public int Height { get; protected set; }

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
			this.OnRootChanged?.Invoke(new ObjectEventArgs(this.Root, OldRoot));
		}
	}

	public void SetParent(OptimizedNode? Parent)
	{
		if (this.Parent != Parent)
		{
			OptimizedNode? OldParent = this.Parent;
			this.Parent = Parent;
			this.OnParentChanged?.Invoke(new ObjectEventArgs(this.Parent, OldParent));
		}
	}

	public void SetDepth(int Depth)
	{
		if (this.Depth != Depth)
		{
			int OldDepth = this.Depth;
			this.Depth = Depth;
			this.OnDepthChanged?.Invoke(new ObjectEventArgs(this.Depth, OldDepth));
		}
	}

	public void SetHeight(int Height)
	{
		if (this.Height != Height)
		{
			this.Height = Height;
		}
	}
}

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
	public OptimizedNode? Parent { get; protected set; }
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
	/// this would be the position of this node in the overall list.
	/// </summary>
	public int GlobalIndex { get; protected set; }
	/// <summary>
	/// The text of this node.
	/// </summary>
	public string Text { get; protected set; }
	/// <summary>
	/// An object associated with this node.
	/// </summary>
	public object? Object { get; protected set; }
	/// <summary>
	/// Whether this node is in its expanded view, i.e. you can see its children.
	/// </summary>
	public bool Expanded { get; protected set; }
	/// <summary>
	/// The depth of the tree at this node.
	/// </summary>
	public int Depth { get; protected set; }

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
    public BaseEvent OnExpansionChanged;
	/// <summary>
	/// Called when the text of this node changes.
	/// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public BaseEvent OnTextChanged;
	/// <summary>
	/// Called when the object associated with this node changes.
	/// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public BaseEvent OnObjectChanged;
	/// <summary>
	/// Called when a node is inserted as a child of this node.
	/// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnNodeInserted;
	/// <summary>
	/// Called when the global index of this node changes.
	/// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnGlobalIndexChanged;
    /// <summary>
    /// Called when this node gets a different parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ObjectEvent OnParentChanged { get; set; }
    /// <summary>
    /// Called when this node gets a different root.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnRootChanged { get; set; }
    /// <summary>
    /// Called when the depth of this node changes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObjectEvent OnDepthChanged { get; set; }

    public OptimizedNode(string Text, object? Object = null)
	{
		this.Text = Text;
		this.Object = Object;
		this.Children = new List<IOptimizedNode>();
		this.Root = this;
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
			if (this.Expanded) this.OnExpanded?.Invoke(new BaseEventArgs());
			else this.OnCollapsed?.Invoke(new BaseEventArgs());
			this.OnExpansionChanged?.Invoke(new BaseEventArgs());
		}
	}

	/// <summary>
	/// Expands this node.
	/// </summary>
	public void Expand()
	{
		this.SetExpanded(true);
	}

	/// <summary>
	/// Collapses this node.
	/// </summary>
	public void Collapse()
	{
		this.SetExpanded(false);
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
			this.OnTextChanged?.Invoke(new BaseEventArgs());
		}
	}

	/// <summary>
	/// Sets the object associated with this node.
	/// </summary>
	/// <param name="Object">The object to associate with this node.</param>
	public void SetObject(object? Object)
	{
		if (this.Object != Object)
		{
			this.Object = Object;
			this.OnObjectChanged?.Invoke(new BaseEventArgs());
		}
	}

	/// <summary>
	/// Gets the next sibling of this node.
	/// </summary>
	/// <returns>The next sibling node, or null if it doesn't exist.</returns>
	public OptimizedNode? GetNextSibling()
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
	public OptimizedNode? GetPreviousSibling()
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
					return (OptimizedNode) Children[Index];
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
	public OptimizedNode? GetPreviousNode(bool IgnoreExpansion = true)
	{
		// The previous node can be found in one of two ways:
		// - If we have a previous sibling, the previous node is the last node of our previous sibling.
		// - If we have no previous siblings, the previous node is our parent.
		OptimizedNode? PreviousSibling = GetPreviousSibling();
		if (PreviousSibling != null) return PreviousSibling.GetLastNode(IgnoreExpansion);
		return Parent;
	}

	/// <summary>
	/// Gets the node with its global index one higher than this node's.
	/// </summary>
	/// <returns>The next node in the hierarchy, or null if it doesn't exist.</returns>
	public OptimizedNode? GetNextNode(bool IgnoreExpansion = true)
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
			OptimizedNode? NextSibling = TestNode.GetNextSibling();
			if (NextSibling != null) return NextSibling;
			TestNode = TestNode.Parent;
		}
		return null;
	}

	/// <summary>
	/// Returns whether the specified node is contained by any of this node's children.
	/// </summary>
	/// <param name="Node">The node to search for.</param>
	/// <returns>Whether the node is part of one of this node's children.</returns>
	public bool Contains(OptimizedNode Node, bool IgnoreExpansion = true)
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
		if (this.GlobalIndex >= Index)
		{
			this.GlobalIndex += Count;
			this.OnGlobalIndexChanged?.Invoke(new ObjectEventArgs(this.GlobalIndex, this.GlobalIndex - Count));
		}
		this.Children.FindAll(c => c is OptimizedNode).ForEach(c => ((OptimizedNode) c).ChangeIndexFrom(Index, Count));
	}

	/// <summary>
	/// Inserts a child at the specified index and increments all global indices accordingly.
	/// </summary>
	/// <param name="Index">The index to insert the child node at.</param>
	/// <param name="NewNode">The node to insert.</param>
	public void InsertChild(int Index, IOptimizedNode NewNode)
	{
		// Automatically expand a node if a child is added to it, and it did not have any children before.
		if (!HasChildren) this.Expanded = true;
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
		NewNode.SetDepth(this.Depth + 1);
		this.OnNodeInserted?.Invoke(new ObjectEventArgs(NewNode));
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
			this.OnRootChanged?.Invoke(new ObjectEventArgs(this.Root, OldRoot));
		}
		this.Children.ForEach(c => c.SetRoot(Root));
	}

	public void SetParent(OptimizedNode? Parent)
	{
		if (this.Parent != Parent)
		{
			OptimizedNode? OldParent = this.Parent;
			this.Parent = Parent;
			this.OnParentChanged?.Invoke(new ObjectEventArgs(this.Parent, OldParent));
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
			this.OnDepthChanged?.Invoke(new ObjectEventArgs(this.Depth, OldDepth));
		}
        this.Children.ForEach(c => c.SetDepth(Depth + 1));
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
	/// Sets the global indices of this node and all its children to sequential values starting at a specific value.
	/// </summary>
	/// <param name="Start">The value to start at.</param>
	/// <returns>The new maximum global index + 1; used in the algorithm only.</returns>
	public int SetIndicesSequentially(int Start)
	{
		int CurrentIndex = Start;
		if (this.GlobalIndex != CurrentIndex)
		{
			int OldIndex = this.GlobalIndex;
			this.GlobalIndex = CurrentIndex;
			this.OnGlobalIndexChanged?.Invoke(new ObjectEventArgs(this.GlobalIndex, OldIndex));
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
}
