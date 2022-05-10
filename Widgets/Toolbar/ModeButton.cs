namespace RPGStudioMK.Widgets;

public class ModeButton : GradientButton
{
    public bool Selected { get; private set; }

    public BaseEvent OnSelection;
    public BaseEvent OnDeselection;

    public ModeButton(IContainer Parent, string Text) : base(Parent, Text)
    {
        Sprites["bg"].Visible = false;

        OnWidgetSelected += WidgetSelected;
        OnLeftMouseDownInside += _ => SetSelected(true);
    }

    public void SetSelected(bool Selected, bool Starting = false)
    {
        if (this.Selected != Selected || Starting)
        {
            if (Selected)
            {
                foreach (Widget w in Parent.Widgets)
                {
                    if (!(w is ModeButton)) continue;
                    ModeButton b = w as ModeButton;
                    if (b.Selected) b.SetSelected(false);
                }
            }
            this.Selected = Selected;
            if (!Starting)
            {
                if (Selected) this.OnSelection?.Invoke(new BaseEventArgs());
                if (!Selected) this.OnDeselection?.Invoke(new BaseEventArgs());
            }
            Sprites["bg"].Visible = Selected;
            Sprites["bg"].SrcRect.Y = 0;
        }
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["bg"].Visible = this.Selected || Mouse.Inside;
        if (this.Selected) Sprites["bg"].SrcRect.Y = 0;
    }
}
