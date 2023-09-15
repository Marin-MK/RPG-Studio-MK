using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;


public class DropdownBox : amethyst.TextBox
{
    public bool ReadOnly => TextArea.ReadOnly;
    public int SelectedIndex { get; protected set; } = 0;
    public TreeNode? SelectedItem => SelectedIndex != -1 ? Items[SelectedIndex] : null;
    public List<TreeNode> Items { get; protected set; } = new List<TreeNode>();
    public bool Enabled { get; protected set; } = true;
    public bool AllowInvalidSelection { get; protected set; } = false;

    public BaseEvent OnDropDownClicked;
    public BaseEvent OnSelectionChanged;

    protected DropdownWidget DropdownWidget;

    public DropdownBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        TextArea.SetPosition(6, 2);
        TextArea.SetFont(Fonts.Paragraph);
        TextArea.SetCaretColor(Color.WHITE);
        TextArea.SetDeselectOnEnterPress(false);
        TextArea.OnTextChanged += _ =>
        {
            if (!TextArea.SelectedWidget) return;
            string query = TextArea.Text.ToLower();
            // Matches to items starting with the text, and then with items containing the text.
            List<TreeNode> filtered = this.Items.FindAll(it => it.Text.ToLower().StartsWith(query)).Concat(this.Items.FindAll(it => it.Text.ToLower().Contains(query))).Distinct().ToList();
            if (DropdownWidget is not null)
            {
                if (filtered.Count == 0)
                {
                    // Dispose
                    DropdownWidget.SetSelectedIndex(-1, false);
                    DropdownWidget.Dispose();
                    DropdownWidget = null;
                }
                else
                {
                    // Update existing dropdown widget
                    DropdownWidget.SetItems(filtered);
                    DropdownWidget.SetSelectedIndex(0, true);
                }
            }
            else if (filtered.Count > 0)
            {
                // Create new dropdown
                CreateDropdownWidget(filtered, 0);
                DropdownWidget.OnDisposed += _ =>
                {
                    DropdownWidget = null;
                    Redraw();
                };
                DropdownWidget.OnDisposeByClick += e =>
                {
                    if (!e.Object && TextArea.SelectedWidget)
                    {
                        // This is called whenever the dropdown widget is disposed by clicking outside of the boundaries.
                        // If this is the case, the TextArea will still be the selected widget. So we deselect it.
                        Window.UI.SetSelectedWidget(null);
                        // This in turn will update any stale selections in the text area.
                    }
                    else if (e.Object)
                    {
						// This is called whenever the dropdown widget is disposed by clicking a valid item.
						// This is valid selection.
						TreeNode item = DropdownWidget.Items[DropdownWidget.SelectedIndex];
                        this.SetSelectedIndex(this.Items.IndexOf(item), false);
                        TextArea.WidgetSelected(new BaseEventArgs());
                    }
                };
            }
        };
        TextArea.OnEnterPressed += _ =>
        {
            if (DropdownWidget is not null)
            {
				TreeNode item = DropdownWidget.Items[DropdownWidget.SelectedIndex];
                DropdownWidget.Dispose();
                DropdownWidget = null;
				this.SetSelectedIndex(this.Items.IndexOf(item), false);
                TextArea.WidgetSelected(new BaseEventArgs());
            }
        };
        TextArea.OnWidgetDeselected += _ =>
        {
            if (this.AllowInvalidSelection) return;
			string query = TextArea.Text.ToLower();
            if (!SelectedItem.Text.ToLower().Contains(query))
            {
                // Selection is probably stale; find closest match to the typed text and select that.
			    List<TreeNode> filtered = this.Items.FindAll(it => it.Text.ToLower().StartsWith(query)).Concat(this.Items.FindAll(it => it.Text.ToLower().Contains(query))).Distinct().ToList();
                if (filtered.Count == 0)
                {
                    // No close matches exist; select first possible item.
                    SetSelectedIndex(Items.Count == 0 ? -1 : 0);
                }
                else
                {
                    // Select first close match
                    SetSelectedIndex(this.Items.IndexOf(filtered[0]));
                }
            }
            TextArea.SetText(SelectedItem.Text);
		};
        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveDown(), true, e => e.Value = this.SelectedWidget || TextArea.SelectedWidget),
            new Shortcut(this, new Key(Keycode.UP), _ => MoveUp(), true, e => e.Value = this.SelectedWidget || TextArea.SelectedWidget)
        });
        MinimumSize.Height = MaximumSize.Height = 24;
        SetHeight(24);
    }

    private void MoveDown()
    {
        if (DropdownWidget is not null || this.SelectedIndex >= this.Items.Count - 1) return;
        this.SetSelectedIndex(this.SelectedIndex + 1, false);
    }

    private void MoveUp()
    {
        if (DropdownWidget is not null || this.SelectedIndex <= 0) return;
        this.SetSelectedIndex(this.SelectedIndex - 1, false);
    }

	public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.TextArea.SetEnabled(Enabled);
            this.Redraw();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(this.Size.Width - 28, this.Size.Height - 3);
    }

    public void SetReadOnly(bool ReadOnly)
    {
        TextArea.SetReadOnly(ReadOnly);
        Redraw();
    }

    public void SetSelectedIndex(int Index, bool callTextChangedEvent = true)
    {
        if (this.SelectedIndex != Index)
        {
            this.TextArea.SetText(Index >= Items.Count || Index == -1 ? "" : Items[Index].Text, callTextChangedEvent);
            this.SelectedIndex = Index;
            this.OnSelectionChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void SetAllowInvalidSelection(bool AllowInvalidSelection)
    {
        if (this.AllowInvalidSelection != AllowInvalidSelection)
        {
            this.AllowInvalidSelection = AllowInvalidSelection;
        }
    }

    public void SetItems(List<TreeNode> Items)
    {
        this.Items = Items;
        this.TextArea.SetText(SelectedIndex >= Items.Count || SelectedIndex == -1 ? "" : Items[SelectedIndex].Text);
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Color Filler = this.Enabled ? new Color(10, 23, 37) : new Color(24, 38, 53);
        Sprites["bg"].Bitmap.FillRect(Size.Width, Size.Height - 2, Filler);
        Color ArrowColor = new Color(86, 108, 134);
        Color ArrowShadow = new Color(17, 27, 38);
        Color LineColor = Mouse.Inside && this.Enabled ? Color.WHITE : ArrowColor;
        Sprites["bg"].Bitmap.FillRect(0, Size.Height - 2, Size.Width, 2, LineColor);
        int x = Size.Width - 18;
        int y = Size.Height / 2 - 4;
        if (DropdownWidget != null) y -= 5;
        Sprites["bg"].Bitmap.FillRect(x, y, 11, 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 3, y + 4, x + 7, y + 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 4, y + 5, x + 6, y + 5, ArrowColor);
        Sprites["bg"].Bitmap.SetPixel(x + 5, y + 6, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x, y + 2, x + 5, y + 7, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x, y + 3, x + 5, y + 8, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 6, x + 10, y + 2, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 7, x + 10, y + 3, ArrowShadow);
        if (DropdownWidget != null) Sprites["bg"].Bitmap.FlipVertically(x, y, 11, 11);
        Sprites["bg"].Bitmap.Lock();
        base.Draw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!TextArea.Mouse.Inside && TextArea.SelectedWidget)
        {
            Window.UI.SetSelectedWidget(null);
        }
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (rx >= Size.Width - 28 && rx < Size.Width &&
            ry >= 0 && ry < Size.Height && this.Enabled)
        {
            this.OnDropDownClicked?.Invoke(new BaseEventArgs());
            if (this.Items.Count > 0)
            {
                CreateDropdownWidget(this.Items, this.SelectedIndex);
				DropdownWidget.OnDisposed += delegate (BaseEventArgs e)
				{
					if (DropdownWidget.SelectedIndex != -1)
					{
						this.SetSelectedIndex(DropdownWidget.SelectedIndex);
					}
					DropdownWidget = null;
					Redraw();
				};
				Redraw();
            }
        };
    }

    private void CreateDropdownWidget(List<TreeNode> items, int selectedIndex)
    {
        if (DropdownWidget is not null) throw new Exception("Dropdown widget exists already!");
		DropdownWidget = new DropdownWidget(Window.UI, this.Size.Width, items, this);
		DropdownWidget.SetPosition(this.Viewport.X, this.Viewport.Y + this.Viewport.Height - 2);
		DropdownWidget.SetSelectedIndex(selectedIndex);
	}
}

public class DropdownWidget : Widget
{
    public int SelectedIndex { get; protected set; }
    public List<TreeNode> Items => List.Items;

    ListDrawer List;
    DropdownBox DropdownBox;
    Container ScrollContainer;
    int Width;

    public GenericObjectEvent<bool> OnDisposeByClick;

    public DropdownWidget(IContainer Parent, int Width, List<TreeNode> Items, DropdownBox DropdownBox) : base(Parent)
    {
        this.DropdownBox = DropdownBox;
        this.Width = Width;

        SetZIndex(Window.ActiveWidget is UIManager ? 9 : (Window.ActiveWidget as Widget).ZIndex + 9);
        SetSize(Width, Math.Min(9, Items.Count) * 20 + 3);

        WindowLayer = Window.ActiveWidget.WindowLayer + 1;
        Window.SetActiveWidget(this);

        ScrollContainer = new Container(this);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(1, 2, 12, 1);

        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, 3, 0, 2);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        List = new ListDrawer(ScrollContainer);
        //List.ForceMouseStart = true; // Allows the mouse to be captured immediately,
        //                             // rather than having to press within the listbox boundaries for it to be captured
        List.SetHDocked(true);
        List.SetItems(Items);

		RegisterShortcuts(new List<Shortcut>()
		{
			new Shortcut(this, new Key(Keycode.DOWN), _ => MoveDown(), true),
			new Shortcut(this, new Key(Keycode.UP), _ => MoveUp(), true)
		});

		Sprites["bg"] = new Sprite(this.Viewport);
    }

	private void MoveDown()
	{
		if (this.SelectedIndex >= this.Items.Count - 1) return;
		this.SetSelectedIndex(this.SelectedIndex + 1);
		DropdownBox.SetSelectedIndex(DropdownBox.Items.IndexOf(this.Items[this.SelectedIndex]), false);
	}

	private void MoveUp()
	{
		if (this.SelectedIndex <= 0) return;
		this.SetSelectedIndex(this.SelectedIndex - 1);
		DropdownBox.SetSelectedIndex(DropdownBox.Items.IndexOf(this.Items[this.SelectedIndex]), false);
	}

	public override void MouseMoving(MouseEventArgs e)
	{
        if (DropdownBox.Mouse.InsideButNotNecessarilyAccessible) Input.SetCursor(CursorType.IBeam);
        else Input.SetCursor(CursorType.Arrow);
		base.MouseMoving(e);
	}

	public override void Dispose()
    {
        if (this.Window.ActiveWidget == this)
        {
            this.Window.Widgets.RemoveAt(Window.Widgets.Count - 1);
            this.Window.SetActiveWidget(Window.Widgets[Window.Widgets.Count - 1]);
        }
        base.Dispose();
    }

    public void SetItems(List<TreeNode> Items)
    {
        List.SetItems(Items);
		SetSize(this.Width, Math.Min(9, Items.Count) * 20 + 3);
        if (List.SelectedIndex >= Items.Count) List.SetSelectedIndex(Items.Count - 1);
        this.SelectedIndex = List.SelectedIndex;
	}

    public void SetSelectedIndex(int SelectedIndex, bool ScrollToSelection = true)
    {
        List.SetSelectedIndex(SelectedIndex);
        this.SelectedIndex = List.SelectedIndex;
        if (ScrollToSelection)
        {
            // Start dropdown at the currently selected index
            int scrolly = SelectedIndex * List.LineHeight;
            scrolly = Math.Clamp(scrolly, 0, List.Size.Height - ScrollContainer.Size.Height);
            ScrollContainer.ScrolledY = scrolly;
            ScrollContainer.UpdateAutoScroll();
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if ((Mouse.LeftMouseTriggered || Mouse.RightMouseTriggered) && !Mouse.Inside)
        {
            OnDisposeByClick?.Invoke(new GenericObjectEventArgs<bool>(false));
            Dispose();
        }
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (List.Mouse.Inside && List.HoveringIndex != -1)
        {
            if (Mouse.LeftStartedInside) this.SelectedIndex = List.SelectedIndex;
            if (DropdownBox.Mouse.LeftStartedInside) this.SelectedIndex = List.HoveringIndex;
            OnDisposeByClick?.Invoke(new GenericObjectEventArgs<bool>(true));
            Dispose();
        }
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Color Outline = new Color(32, 170, 221);
        Color Filler = new Color(10, 23, 37);
        Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, Outline);
        Sprites["bg"].Bitmap.DrawLine(1, 1, Size.Width - 2, 1, Outline);
        Sprites["bg"].Bitmap.FillRect(1, 2, Size.Width - 2, Size.Height - 3, Filler);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 2, Size.Width - 12, Size.Height - 2, 40, 62, 84);
        Sprites["bg"].Bitmap.Lock();
        base.Draw();
    }
}