using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class ListDrawer : Widget
{
    public Font Font => tree.Font;
    public int LineHeight => tree.LineHeight;
    public List<TreeNode> Items => tree.Root.Children.Select(n => (TreeNode) n).ToList();
    public int SelectedIndex => tree.Root.Children.IndexOf(tree.SelectedNode);
    public int HoveringIndex => tree.Root.Children.IndexOf(tree.HoveringNode);
    public TreeNode SelectedItem => (TreeNode) tree.SelectedNode;
    public List<TreeNode> SelectedItems => tree.SelectedNodes.Select(n => (TreeNode) n).ToList();
    public TreeNode HoveringItem => (TreeNode) tree.HoveringNode;
    public bool CanMultiSelect => tree.CanMultiSelect;
    public bool Enabled { get; protected set; } = true;

    public BaseEvent OnSelectionChanged;
    public BaseEvent OnDoubleClicked;
    public BoolEvent OnVScrollBarVisiblityChanged { get => tree.OnVScrollBarVisibilityChanged; set => tree.OnVScrollBarVisibilityChanged = value; }
	public BoolEvent OnHScrollBarVisiblityChanged { get => tree.OnHScrollBarVisibilityChanged; set => tree.OnHScrollBarVisibilityChanged = value; }
    public GenericObjectEvent<(bool, bool)> OnScrollBarVisiblityChanged { get => tree.OnScrollBarVisiblityChanged; set => tree.OnScrollBarVisiblityChanged = value; }

	TreeView tree;

    public ListDrawer(IContainer parent) : base(parent)
    {
        tree = new TreeView(this);
        tree.SetDocked(true);
        tree.SetXOffset(-12);
        tree.SetLineHeight(24);
        tree.SetCanDragAndDrop(false);
        tree.SetHScrollBarPaddingAlone(new Padding(tree.HScrollBarPaddingAlone.Left, tree.HScrollBarPaddingAlone.Up, tree.HScrollBarPaddingAlone.Right, tree.HScrollBarPaddingAlone.Down - 1));
		tree.SetHScrollBarPaddingShared(new Padding(tree.HScrollBarPaddingShared.Left, tree.HScrollBarPaddingShared.Up, tree.HScrollBarPaddingShared.Right, tree.HScrollBarPaddingShared.Down - 1));
		tree.OnSelectionChanged += e =>
        {
            if (e.Value) OnDoubleClicked?.Invoke(new BaseEventArgs());
            OnSelectionChanged?.Invoke(new BaseEventArgs());
        };
    }

    public void SetAutoResize(bool autoResize)
    {
        tree.SetAutoResize(autoResize);
    }

    public void SetItems(List<TreeNode> items)
    {
        tree.SetNodes(items);
    }

    public void SetFont(Font f)
    {
        tree.SetFont(f);
    }

    public void SetLineHeight(int height)
    {
        tree.SetLineHeight(height);
    }

    public void SetSelectedIndex(int index)
    {
        TreeNode node = (TreeNode) tree.Root.Children[index];
        tree.SetSelectedNode(node, false);
    }

    public void SetCanMultiSelect(bool canMultiSelect)
    {
        tree.SetCanMultiSelect(canMultiSelect);
    }

    public void ClearSelection()
    {
        tree.ClearSelection();
    }

    public void SetSelectedItem(TreeNode node)
    {
        tree.SetSelectedNode(node, false);
        tree.EnsureSelectedNodeVisible();
    }

    public void InsertItem(int? index, TreeNode item)
    {
        tree.InsertNode(tree.Root, index, item);
        tree.SetSelectedNode(tree.SelectedNode, false);
        tree.EnsureSelectedNodeVisible();
    }

    public void AddItem(TreeNode item)
    {
        InsertItem(null, item);
    }

    public void RedrawItem(TreeNode item)
    {
        tree.RedrawNode(item);
    }

    public void RemoveItem(TreeNode item)
    {
        tree.DeleteNode(item, true);
    }

    public void MoveDown()
    {
        tree.MoveDown();
    }

    public void MoveUp()
    {
        tree.MoveUp();
    }

    public void MovePageDown()
    {
        tree.MovePageDown();
    }

    public void MovePageUp()
    {
        tree.MovePageUp();
    }
}
