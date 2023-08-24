using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class MoveDropdownBox : DropdownBox
{
	public MoveResolver Move;
	public GenericObjectEvent<MoveResolver> OnMoveChanged;

    public MoveDropdownBox(IContainer parent) : base(parent)
    {
		SetItems(Data.Sources.Moves);
		this.Move = (MoveResolver) (Move) Items[0].Object;
		OnSelectionChanged += _ =>
		{
			this.Move = (MoveResolver) (Move) SelectedItem?.Object;
			OnMoveChanged?.Invoke(new GenericObjectEventArgs<MoveResolver>(this.Move));
		};
	}

	public void SetMove(MoveResolver move)
	{
		if (move.Valid) SetSelectedIndex(Items.FindIndex(item => (Move) item.Object == move.Move));
		else SetText(move.ID);
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
			if (Editor.MainWindow.DatabaseWidget.Mode == DatabaseMode.Moves)
			{
				((DataTypeMoves) Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget).SelectMove(this.Move);
			}
			else
			{
				Editor.ProjectSettings.LastMoveID = this.Move.ID;
				Editor.ProjectSettings.LastMoveScroll = 0;
				Editor.MainWindow.DatabaseWidget.SetMode(DatabaseMode.Moves);
			}
			return;
		}
		base.LeftMouseDownInside(e);
	}
}
