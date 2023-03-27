using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class TransferPlayerWidget : BaseCommandWidget
{
    Label TransferLabel;

    public TransferPlayerWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex) 
    {
        TransferLabel = new Label(this);
        TransferLabel.SetFont(Fonts.Paragraph);
    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        HeaderLabel.SetText("Transfer Player");
        HeaderLabel.RedrawText(true);
        // 0; 0 = assignment, 1 = variable-appointed
        // 1; map id, or variable containing map id
        // 2; x, or variable containing x
        // 3; y, or variable containing y
        // 4; dir, or variable containing dir (0 = retain)
        // 5; 0 for fade, 1 for no fade
        int Type = (int) (long) Command.Parameters[0];
        int MapID = (int) (long) Command.Parameters[1];
        int X = (int) (long) Command.Parameters[2];
        int Y = (int) (long) Command.Parameters[3];
        int Dir = (int) (long) Command.Parameters[4];
        int Fade = (int) (long) Command.Parameters[5];
        string Text = "";
        if (Type == 0)
        {
            // Assigned
            Text += $"[{Utilities.Digits(MapID, 3)}: {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : "")}] ({X}, {Y})";
        }
        else
        {
            // Variable appointed
            Text += $"Variable [{Utilities.Digits(MapID, 3)}] ([{Utilities.Digits(X, 3)}], [{Utilities.Digits(Y, 3)}])";
        }
        if (Dir != 0) // If we don't retain direction, then state direction
        {
            string dirtext = Dir switch
            {
                2 => "Down",
                4 => "Left",
                6 => "Right",
                8 => "Up",
                _ => "Unknown"
            };
            Text += ", " + dirtext;
        }
        if (Fade == 1) // If we don't have a fade, state so
        {
            Text += ", No fade";
        }
        TransferLabel.SetText(Text);
        TransferLabel.SetPosition(GetStandardLabelPosition());
    }

    protected override void Edit(EditEvent Continue)
    {
        EditTransferPlayerCommandWindow win = new EditTransferPlayerCommandWindow(this.Command);
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
