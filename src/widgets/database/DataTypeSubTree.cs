using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class DataTypeSubTree : Widget
{
    public TreeNode SelectedItem => (TreeNode) TreeView.SelectedNode;
    public TreeNode HoveringItem => (TreeNode) TreeView.HoveringNode;
    public TreeNode Root => TreeView.Root;

    TreeView TreeView;
    Container ScrollContainer;

    public BoolEvent OnSelectionChanged { get { return TreeView.OnSelectionChanged; } set { TreeView.OnSelectionChanged = value; } }
    public GenericObjectEvent<int> OnMaximumChanged;

    public DataTypeSubTree(string HeaderText, IContainer Parent) : base(Parent)
    {
        Sprites["line1"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(10, 23, 37)));
        Sprites["line1"].X = 188;
        Sprites["line1"].Y = 29;
        Sprites["line2"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, Color.BLACK));
        Sprites["line2"].X = 200;
        Sprites["line2"].Y = 29;
        Sprites["line3"] = new Sprite(this.Viewport, new SolidBitmap(200, 1, new Color(10, 23, 37)));
        Sprites["headerbg"] = new Sprite(this.Viewport, new SolidBitmap(201, 29, new Color(17, 33, 51)));
        Sprites["header"] = new Sprite(this.Viewport);
        Size s = Fonts.Header.TextSize(HeaderText);
        Sprites["header"].Bitmap = new Bitmap(s);
        Sprites["header"].Bitmap.Font = Fonts.Header;
        Sprites["header"].Bitmap.Unlock();
        Sprites["header"].Bitmap.DrawText(HeaderText, Color.WHITE);
        Sprites["header"].Bitmap.Lock();
        Sprites["header"].X = 100 - s.Width / 2;
        Sprites["header"].Y = 6;
        ScrollContainer = new Container(this);
        ScrollContainer.SetPosition(0, 29);
        VScrollBar vs = new VScrollBar(this);
        vs.SetPosition(190, 30);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;
        vs.OnValueChanged += e => OnScrolling?.Invoke(e);
        TreeView = new TreeView(ScrollContainer);
        TreeView.SetLineHeight(24);
        TreeView.SetFont(Fonts.Paragraph);
        TreeView.SetCanDragAndDrop(false);
        TreeView.SetXOffset(-6);
        TreeView.SetAutoResize(false);
        SetWidth(201);
    }

    public override void SetContextMenuList(List<IMenuItem> Items)
    {
        TreeView.SetContextMenuList(Items);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ScrollContainer.SetSize(Size.Width - 13, Size.Height - 70);
        ScrollContainer.VScrollBar.SetHeight(Size.Height - 72);
        ((SolidBitmap) Sprites["line1"].Bitmap).SetSize(1, Size.Height - 70);
        ((SolidBitmap) Sprites["line2"].Bitmap).SetSize(1, Size.Height - 29);
        Sprites["line3"].Y = Size.Height - 41;
    }

    public void SetItems(List<TreeNode> Items, TreeNode? SelectedNode = null)
    {
        TreeView.SetNodes(Items, SelectedNode);
    }

    public void SetSelectedNode(TreeNode SelectedNode)
    {
        TreeView.SetSelectedNode(SelectedNode, false, true);
        TreeView.EnsureSelectedNodeVisible();
    }

    public int GetScroll()
    {
        return ScrollContainer.ScrolledY;
    }

    public void SetScroll(int scroll)
    {
        ScrollContainer.ScrolledY = Math.Clamp(scroll, 0, TreeView.Size.Height - ScrollContainer.Size.Height);
        ScrollContainer.UpdateAutoScroll();
    }

    public void CenterOnSelectedNode()
    {
        TreeView.CenterOnNode(SelectedItem);
    }

    public void RedrawNodeText(TreeNode node)
    {
        TreeView.RedrawNodeText(node);
    }

    public void RedrawNode(TreeNode node)
    {
        TreeView.RedrawNode(node);
    }

    public void RedrawAllNodes()
    {
        TreeView.RedrawAllNodes();
    }

    public void SetActiveAndSelectedNode(TreeNode node)
    {
        TreeView.SetActiveAndSelectedNode(node);

	}
}
