using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class MoveRouteWidget : BaseCommandWidget
{
    MultilineLabel CommandLabel;

    public MoveRouteWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex)
    {
        CommandLabel = new MultilineLabel(this);
        CommandLabel.SetPosition(ChildIndent * 2, HeaderLabel.Position.Y + StandardHeight);
        CommandLabel.SetFont(Fonts.Paragraph);
        CommandLabel.SetHDocked(true);
        CommandLabel.SetLineHeight(StandardHeight);
        CommandLabel.SetTextColor(HeaderLabel.TextColor);
    }

    protected override void Edit(EditEvent Continue)
    {
        EditMoveRouteWindow win = new EditMoveRouteWindow(Map, Event, Page, (MoveRoute) Command.Parameters[1], (int) (long) Command.Parameters[0], false);
        win.OnClosed += _ =>
        {
            if (!win.Apply)
            {
                Continue(false);
                return;
            }
            Commands = win.NewCommands;
            Continue(true);
        };
    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        int EventID = (int) (long) this.Command.Parameters[0];
        MoveRoute MoveRoute = (MoveRoute) this.Command.Parameters[1];
        string EventName = EventID == -1 ? "Player" : EventID == 0 ? "Self" : Map.Events[EventID].Name;
        string header = $"Set Move Route: {EventName}";
        if (MoveRoute.Skippable || MoveRoute.Repeat)
        {
            header += " (";
            if (MoveRoute.Skippable)
            {
                header += "Skippable";
                if (MoveRoute.Repeat) header += ", ";
            }
            if (MoveRoute.Repeat) header += "Repeat";
            header += ")";
        }
        HeaderLabel.SetText(header);
        string text = "";
        for (int i = 0; i < MoveRoute.Commands.Count - 1; i++)
        {
            text += MoveRoute.Commands[i].ToString();
            if (i != MoveRoute.Commands.Count - 2) text += "\n";
        }
        CommandLabel.SetText(text);
        HeightAdd = (MoveRoute.Commands.Count - 1) * StandardHeight;
    }
}
