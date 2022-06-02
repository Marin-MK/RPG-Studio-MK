using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class ChoiceWidget : BaseCommandWidget
{
    Label EndLabel;
    List<Label> BranchLabels = new List<Label>();
    List<VStackPanel> StackPanels = new List<VStackPanel>();

    public ChoiceWidget(IContainer Parent) : base(Parent, new Color(128, 128, 255))
    {
        EndLabel = new Label(this);
        EndLabel.SetFont(Fonts.ProductSansMedium.Use(9));
        EndLabel.SetText("End");
        EndLabel.SetTextColor(HeaderLabel.TextColor);
    }

    public override void LoadCommand()
    {
        BranchLabels.ForEach(l => l.Dispose());
        BranchLabels.Clear();

        StackPanels.ForEach(s => s.Dispose());
        StackPanels.Clear();

        HeaderLabel.SetText("Show Choices");

        List<object> Choices = (List<object>) Command.Parameters[0];

        List<EventCommand> BranchCommands = Commands.FindAll(c => (c.Code == CommandCode.BranchWhenXXX || c.Code == CommandCode.BranchWhenCancel) && c.Indent == Command.Indent);
        
        void UpdateLabels()
        {
            int y = 22;
            for (int i = 0; i < BranchCommands.Count; i++)
            {
                BranchLabels[i].SetPosition(8, y + 4);
                y += StackPanels[i].Margins.Up + StackPanels[i].Size.Height;
            }
            if (DrawEndLabels) EndLabel.SetPosition(8, y + 4);
        }

        for (int i = 0; i < BranchCommands.Count; i++)
        {
            EventCommand BranchCmd = BranchCommands[i];
            int BranchIdx = Commands.IndexOf(BranchCmd);

            Label BranchLabel = new Label(this);
            BranchLabel.SetFont(Fonts.ProductSansMedium.Use(9));
            if (BranchCmd.Code == CommandCode.BranchWhenCancel) BranchLabel.SetText($"When cancelled:");
            else BranchLabel.SetText($"When [{Choices[i]}]:");
            BranchLabels.Add(BranchLabel);

            VStackPanel StackPanel = new VStackPanel(this.VStackPanel);
            StackPanel.SetHDocked(true);
            StackPanel.SetMargins(0, 20, 0, 0);
            StackPanels.Add(StackPanel);

            if (i < BranchCommands.Count - 1)
            {
                // There's another branch after this
                int NextBranchIdx = Commands.IndexOf(BranchCommands[i + 1]);
                ParseCommands(Commands.GetRange(BranchIdx + 1, NextBranchIdx - BranchIdx - 1), StackPanel);
            }
            else ParseCommands(Commands.GetRange(BranchIdx + 1, Commands.Count - BranchIdx - 2), StackPanel);
            StackPanel.OnSizeChanged += _ =>
            {
                UpdateLabels();
                UpdateHeight();
            };
        }
        if (DrawEndLabels) HeightAdd = 22;
        UpdateLabels();
    }
}
