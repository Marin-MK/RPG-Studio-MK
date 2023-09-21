using System.Collections.Generic;


namespace RPGStudioMK.Widgets;

public class ListBox : Widget
{
    public int HoveringIndex => ListDrawer.HoveringIndex;
    public TreeNode HoveringItem => ListDrawer.HoveringItem;
    public int SelectedIndex => ListDrawer.SelectedIndex;
    public TreeNode SelectedItem => ListDrawer.SelectedItem;
    public List<TreeNode> SelectedItems => ListDrawer.SelectedItems;
    public List<TreeNode> Items => ListDrawer.Items;
    public int LineHeight => ListDrawer.LineHeight;
    public bool CanMultiSelect => ListDrawer.CanMultiSelect;
    public bool Enabled => ListDrawer.Enabled;

    public BaseEvent OnSelectionChanged { get => ListDrawer.OnSelectionChanged; set => ListDrawer.OnSelectionChanged = value; }
    public BaseEvent OnDoubleClicked { get => ListDrawer.OnDoubleClicked; set => ListDrawer.OnDoubleClicked = value; }

    public ListDrawer ListDrawer;

    bool wasVscrollBarVisible = true;
    bool wasHScrolLBarVisible = true;

    public ListBox(IContainer parent) : base(parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);

        ListDrawer = new ListDrawer(this);
        ListDrawer.SetDocked(true);
        ListDrawer.SetPadding(2, 2, 1, 2);
        ListDrawer.SetDocked(true);
        ListDrawer.OnScrollBarVisiblityChanged += e =>
        {
            RedrawBox(e.Object.Item1, e.Object.Item2);
        };

        SetSize(132, 174);
    }

	public override void RegisterShortcuts(List<Shortcut> Shortcuts, bool DeregisterExisting = false)
	{
        ListDrawer.RegisterShortcuts(Shortcuts, DeregisterExisting);
	}

	public void SetFont(Font font)
    {
        ListDrawer.SetFont(font);
    }

    public void SetItems(List<TreeNode> items)
    {
        ListDrawer.SetItems(items);
    }

    public void SetLineHeight(int lineHeight)
    {
        ListDrawer.SetLineHeight(lineHeight);
    }

    public void SetCanMultiSelect(bool canMultiSelect)
    {
        ListDrawer.SetCanMultiSelect(canMultiSelect);
    }

    public void ClearSelection()
    {
        ListDrawer.ClearSelection();
    }

    public void RedrawBox(bool vScrollBarVisible, bool hScrollBarVisible)
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(Size, this.Enabled ? new Color(86, 108, 134) : new Color(36, 34, 36));
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, this.Enabled ? new Color(10, 23, 37) : new Color(72, 72, 72));
        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
        Color DarkOutline = this.Enabled ? new Color(40, 62, 84) : new Color(36, 34, 36);
        Sprites["bg"].Bitmap.SetPixel(1, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, DarkOutline);

        if (hScrollBarVisible)
        {
            Sprites["bg"].Bitmap.DrawLine(1, Size.Height - 12, Size.Width - 2, Size.Height - 12, DarkOutline);
		    Sprites["bg"].Bitmap.FillRect(Size.Width - 12, Size.Height - 12, 11, 11, new Color(64, 104, 146));
        }

		Sprites["bg"].Bitmap.Lock();
        wasVscrollBarVisible = vScrollBarVisible;
        wasHScrolLBarVisible = hScrollBarVisible;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawBox(wasVscrollBarVisible, wasHScrolLBarVisible);
    }

    public void SetSelectedIndex(int idx)
    {
        ListDrawer.SetSelectedIndex(idx);
    }

    public void SetSelectedItem(TreeNode item)
    {
        ListDrawer.SetSelectedItem(item);
    }

    public void InsertItem(int index, TreeNode item)
    {
        ListDrawer.InsertItem(index, item);
    }

    public void AddItem(TreeNode item)
    {
        ListDrawer.AddItem(item);
    }

    public void RedrawItem(TreeNode item)
    {
        ListDrawer.RedrawItem(item);
    }

    public void RemoveItem(TreeNode item)
    {
        ListDrawer.RemoveItem(item);
    }

	public void MoveDown()
	{
		ListDrawer.MoveDown();
	}

	public void MoveUp()
	{
		ListDrawer.MoveUp();
	}

	public void MovePageDown()
	{
		ListDrawer.MovePageDown();
	}

	public void MovePageUp()
	{
		ListDrawer.MovePageUp();
	}
}
