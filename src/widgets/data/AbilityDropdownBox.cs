using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class AbilityDropdownBox : DropdownBox
{
	public AbilityResolver Ability;
	public GenericObjectEvent<AbilityResolver> OnAbilityChanged;

    public AbilityDropdownBox(IContainer parent) : base(parent)
    {
		SetItems(Data.Sources.Abilities);
		this.Ability = (AbilityResolver) (Ability) Items[0].Object;
		OnSelectionChanged += _ =>
		{
			this.Ability = (AbilityResolver) (Ability) SelectedItem?.Object;
			OnAbilityChanged?.Invoke(new GenericObjectEventArgs<AbilityResolver>(this.Ability));
		};
	}

	public void SetAbility(AbilityResolver abil)
	{
		if (abil.Valid) SetSelectedIndex(Items.FindIndex(item => (Ability) item.Object == abil.Ability));
		else SetText(abil.ID);
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
			if (Editor.MainWindow.DatabaseWidget.Mode == DatabaseMode.Abilities)
			{
				//((DataTypeAbilities) Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget).SelectAbility(this.Ability);
			}
			else
			{
				//Editor.ProjectSettings.LastAbilityID = this.Ability.ID;
				//Editor.ProjectSettings.LastAbilityScroll = 0;
				Editor.MainWindow.DatabaseWidget.SetMode(DatabaseMode.Abilities);
			}
			return;
		}
		base.LeftMouseDownInside(e);
	}
}
