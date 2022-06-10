using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class SetSwitchWidget : BaseCommandWidget
{
    public SetSwitchWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex) { }

    public override void LoadCommand()
    {
        int SwitchID1 = (int) (long) Command.Parameters[0];
        int SwitchID2 = (int) (long) Command.Parameters[1];
        int State = (int) (long) Command.Parameters[2];
        if (SwitchID1 == SwitchID2)
        {
            HeaderLabel.SetText($"Set Switch [{Utilities.Digits(SwitchID1, 3)}: {Data.System.Switches[SwitchID1 - 1]}] to {(State == 0 ? "ON" : "OFF")}");
        }
        else
        {
            HeaderLabel.SetText($"Set Switches [{Utilities.Digits(SwitchID1, 3)}..{Utilities.Digits(SwitchID2, 3)}] to {(State == 0 ? "ON" : "OFF")}");
        }
    }

    protected override void Edit(EditEvent Continue)
    {
        //SwitchPicker win = new SwitchPicker((int) (long) Command.Parameters[0]);
        //win.OnClosed += _ =>
        //{
        //    if (!win.Apply)
        //    {
        //        Continue(false);
        //        return;
        //    }
        //    Commands = new List<EventCommand>() { new EventCommand(CommandCode.ControlSwitches, 0, new List<object>() { (long) win.SwitchID }) };
        //    Continue();
        //};
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
