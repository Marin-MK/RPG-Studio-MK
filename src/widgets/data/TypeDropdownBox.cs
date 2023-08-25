using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class TypeDropdownBox : DropdownBox
{
	public TypeResolver Type;
	public GenericObjectEvent<TypeResolver> OnTypeChanged;

    public TypeDropdownBox(IContainer parent) : base(parent)
    {
		SetItems(Data.Sources.Types);
		this.Type = (TypeResolver) (Game.Type) Items[0].Object;
		OnSelectionChanged += _ =>
		{
			this.Type = (TypeResolver) (Game.Type) SelectedItem?.Object;
			OnTypeChanged?.Invoke(new GenericObjectEventArgs<TypeResolver>(this.Type));
		};
	}

	public void SetType(TypeResolver type)
	{
		if (type.Valid) SetSelectedIndex(Items.FindIndex(item => (Game.Type) item.Object == type.Type));
		else SetText(type.ID);
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
			if (Editor.MainWindow.DatabaseWidget.Mode == DatabaseMode.Types)
			{
				//((DataTypeTypes) Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget).SelectType(this.Type);
			}
			else
			{
				//Editor.ProjectSettings.LastTypeID = this.Type.ID;
				//Editor.ProjectSettings.LastTypeScroll = 0;
				Editor.MainWindow.DatabaseWidget.SetMode(DatabaseMode.Types);
			}
			return;
		}
		base.LeftMouseDownInside(e);
	}
}
