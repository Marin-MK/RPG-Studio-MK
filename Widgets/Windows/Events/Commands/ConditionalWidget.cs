using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class ConditionalWidget : BaseCommandWidget
{
    Label ElseLabel;
    Label EndLabel;
    VStackPanel VStackPanel1;
    VStackPanel VStackPanel2;

    public ConditionalWidget(IContainer Parent) : base(Parent, new Color(128, 128, 255))
    {
        ElseLabel = new Label(this);
        ElseLabel.SetFont(Fonts.ProductSansMedium.Use(9));
        ElseLabel.SetText("Else");
        ElseLabel.SetTextColor(HeaderLabel.TextColor);
        EndLabel = new Label(this);
        EndLabel.SetFont(Fonts.ProductSansMedium.Use(9));
        EndLabel.SetText("End");
        EndLabel.SetTextColor(HeaderLabel.TextColor);
    }

    public override void LoadCommand()
    {
        // Draw conditional
        HeaderLabel.SetText(CommandHelper.GetText(Map, Event));

        VStackPanel1?.Dispose();
        VStackPanel2?.Dispose();

        void UpdateLabels()
        {
            ElseLabel.SetPosition(8, VStackPanel.Padding.Up + VStackPanel1.Size.Height + 4);
            EndLabel.SetPosition(8, VStackPanel.Padding.Up + VStackPanel1.Size.Height + VStackPanel2.Margins.Up + VStackPanel2.Size.Height + 4);
        }

        VStackPanel1 = new VStackPanel(VStackPanel);
        VStackPanel1.SetHDocked(true);

        VStackPanel2 = new VStackPanel(VStackPanel);
        VStackPanel2.SetHDocked(true);
        VStackPanel2.SetMargins(0, 22, 0, 0);

        EventCommand ElseCmd = Commands.Find(c => c.Code == CommandCode.BranchElse && c.Indent == Command.Indent);
        int ElseCmdIdx = Commands.IndexOf(ElseCmd);
        if (ElseCmd != null)
        {
            // Draw true commands
            ParseCommands(Commands.GetRange(1, ElseCmdIdx - 1), VStackPanel1);
            VStackPanel1.UpdateLayout();

            // Draw false commands
            ParseCommands(Commands.GetRange(ElseCmdIdx + 1, Commands.Count - ElseCmdIdx - 2), VStackPanel2);
            VStackPanel2.UpdateLayout();
        }
        else
        {
            // Draw true commands
            ParseCommands(Commands.GetRange(1, Commands.Count - 2), VStackPanel1);
            VStackPanel1.UpdateLayout();
            ElseLabel.SetVisible(false);
        }
        if (DrawEndLabels) HeightAdd = 22;
        UpdateLabels();

        VStackPanel1.OnSizeChanged += _ =>
        {
            UpdateLabels();
            UpdateHeight();
        };
        VStackPanel2.OnSizeChanged += _ =>
        {
            UpdateLabels();
            UpdateHeight();
        };
    }
}
