using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class SetSelfSwitchWidget : BaseCommandWidget
{
    public SetSelfSwitchWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex, new Color(255, 255, 128)) { }

    public override void LoadCommand()
    {
        char Switch = ((string) Command.Parameters[0])[0];
        int State = (int) (long) Command.Parameters[1];
        int EventID = Command.Parameters.Count == 3 ? (int) (long) Command.Parameters[2] : -1;
        string EventName = EventID == -1 ? "" : "[" + (Map.Events.ContainsKey(EventID) ? Map.Events[EventID].Name : Utilities.Digits(EventID, 3)) + "]'s ";
        HeaderLabel.SetText($"Set {EventName}Self Switch {Switch} to {(State == 0 ? "ON" : "OFF")}");
    }

    protected override void Edit(EditEvent Continue)
    {
        EditSelfSwitchCommandWindow win = new EditSelfSwitchCommandWindow(Map, Event, Command);
        win.OnClosed += _ =>
        {
            if (!win.Apply)
            {
                Continue(false);
                return;
            }
            Commands = new List<EventCommand>() { win.NewCommand };
            Continue(true);
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
