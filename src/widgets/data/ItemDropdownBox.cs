using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ItemDropdownBox : DropdownBox
{
	public ItemResolver Item;
	public GenericObjectEvent<ItemResolver> OnItemChanged;

    public ItemDropdownBox(IContainer parent) : base(parent)
    {
		SetItems(Data.Sources.Items);
		this.Item = (ItemResolver) (Item) Items[0].Object;
		OnSelectionChanged += _ =>
		{
			this.Item = (ItemResolver) (Item) SelectedItem?.Object;
			OnItemChanged?.Invoke(new GenericObjectEventArgs<ItemResolver>(this.Item));
		};
	}

	public void SetItem(ItemResolver item)
	{
		if (item.Valid) SetSelectedIndex(Items.FindIndex(lItem => (Item) lItem.Object == item.Item));
		else SetText(item.ID);
	}

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
		if (Input.Press(Keycode.CTRL) && !e.CursorHandled)
		{
			if (Mouse.Inside)
			{
				Input.SetCursor(CursorType.Hand);
				e.CursorHandled = true;
			}
		}
	}

	public override void HoverChanged(MouseEventArgs e)
	{
		base.HoverChanged(e);
		if (!Mouse.Inside && Input.SystemCursor == CursorType.Hand)
		{
			Input.SetCursor(CursorType.Arrow);
			e.CursorHandled = true;
		}
	}

	public override void LeftMouseDownInside(MouseEventArgs e)
	{
		if (Input.Press(Keycode.CTRL) && Input.SystemCursor == CursorType.Hand)
		{
			if (Editor.MainWindow.DatabaseWidget.Mode == DatabaseMode.Items)
			{
				//((DataTypeItems) Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget).SelectItem(this.Item);
			}
			else
			{
				//Editor.ProjectSettings.LastItemID = this.Item.ID;
				//Editor.ProjectSettings.LastItemScroll = 0;
				Editor.MainWindow.DatabaseWidget.SetMode(DatabaseMode.Items);
			}
			return;
		}
		base.LeftMouseDownInside(e);
	}
}
