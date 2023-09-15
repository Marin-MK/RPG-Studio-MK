using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class SpeciesDropdownBox : DropdownBox
{
	public SpeciesResolver Species;
	public GenericObjectEvent<SpeciesResolver> OnSpeciesChanged;

    public SpeciesDropdownBox(bool includeForms, IContainer parent) : base(parent)
    {
		SetItems(includeForms ? Data.Sources.SpeciesAndForms : Data.Sources.Species);
		this.Species = (SpeciesResolver) (Species) Items[0].Object;
		OnSelectionChanged += _ =>
		{
			this.Species = (SpeciesResolver) (Species) SelectedItem?.Object;
			OnSpeciesChanged?.Invoke(new GenericObjectEventArgs<SpeciesResolver>(this.Species));
		};
	}

	public void SetSpecies(SpeciesResolver spc)
	{
		if (spc.Valid) SetSelectedIndex(Items.FindIndex(item => (Species) item.Object == spc.Species));
		else SetText(spc.ID);
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
			if (Editor.MainWindow.DatabaseWidget.Mode == DatabaseMode.Species)
			{
				Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget.SelectData(this.Species);
			}
			else
			{
				Editor.ProjectSettings.LastSpeciesID = this.Species.ID;
				Editor.ProjectSettings.LastSpeciesScroll = 0;
				Editor.MainWindow.DatabaseWidget.SetMode(DatabaseMode.Species);
			}
			return;
		}
		base.LeftMouseDownInside(e);
	}
}
