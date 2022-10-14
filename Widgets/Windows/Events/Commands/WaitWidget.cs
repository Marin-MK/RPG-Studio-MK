using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class WaitWidget : BaseCommandWidget
{
    Label WaitLabel;

    public WaitWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex) 
    {
        WaitLabel = new Label(this);
        WaitLabel.SetFont(Fonts.CabinMedium.Use(9));
    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        WaitLabel.SetPosition(GetStandardLabelPosition());
        WaitLabel.SetText($"{Command.Parameters[0]} frame{((long) Command.Parameters[0] > 1 ? "s" : "")}");
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
}
