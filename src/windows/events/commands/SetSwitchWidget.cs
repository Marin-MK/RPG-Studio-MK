using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class SetSwitchWidget : BaseCommandWidget
{
    Label SwitchLabel;

    public SetSwitchWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex) 
    {
        SwitchLabel = new Label(this);
        SwitchLabel.SetFont(Fonts.Paragraph);
    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        int SwitchID1 = (int) (long) Command.Parameters[0];
        int SwitchID2 = (int) (long) Command.Parameters[1];
        int State = (int) (long) Command.Parameters[2];
        if (SwitchID1 == SwitchID2)
        {
            HeaderLabel.SetText("Set Switch");
            HeaderLabel.RedrawText(true);
            SwitchLabel.SetText($"[{Utilities.Digits(SwitchID1, 3)}: {Data.System.Switches[SwitchID1 - 1]}] to {(State == 0 ? "ON" : "OFF")}");
        }
        else
        {
            HeaderLabel.SetText("Set Switches");
            HeaderLabel.RedrawText(true);
            SwitchLabel.SetText($"[{Utilities.Digits(SwitchID1, 3)}..{Utilities.Digits(SwitchID2, 3)}] to {(State == 0 ? "ON" : "OFF")}");
        }
        SwitchLabel.SetPosition(GetStandardLabelPosition());
    }

    protected override void Edit(EditEvent Continue)
    {
        EditSwitchCommandWindow win = new EditSwitchCommandWindow(Command);
        win.OnClosed += _ =>
        {
            if (!win.Apply)
            {
                Continue(false);
                return;
            }
            Commands = new List<EventCommand>() { win.NewCommand };
            Continue();
        };
    }
}
