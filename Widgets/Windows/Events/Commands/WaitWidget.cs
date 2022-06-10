using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class WaitWidget : BaseCommandWidget
{
    public WaitWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex) { }

    public override void LoadCommand()
    {
        HeaderLabel.SetText($"Wait {Command.Parameters[0]} frame{((long) Command.Parameters[0] > 1 ? "s" : "")}");
    }

    protected override void Edit(EditEvent Continue)
    {
        GenericNumberPicker win = new GenericNumberPicker("Wait", "Frames:", (int) (long) Command.Parameters[0], 1, null);
        win.OnClosed += _ =>
        {
            if (!win.Apply)
            {
                Continue(false);
                return;
            }
            Commands = new List<EventCommand>()
            {
                new EventCommand(CommandCode.Wait, 0, new List<object>() { (long) win.Value })
            };
            Continue();
        };
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (e.Handled || this.Indentation == -1)
        {
            CancelDoubleClick();
            return;
        }
        SetSelected(true);
    }
}
