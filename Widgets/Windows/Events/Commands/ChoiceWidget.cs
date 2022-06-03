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
    List<ExpandArrow> ExpandArrows = new List<ExpandArrow>();
    List<bool> OldVisibleStates = new List<bool>();
    ExpandArrow ExpandAllArrow;

    public ChoiceWidget(IContainer Parent) : base(Parent, new Color(128, 128, 255))
    {
        EndLabel = new Label(this);
        EndLabel.SetFont(Fonts.ProductSansMedium.Use(9));
        EndLabel.SetText("End");
        EndLabel.SetTextColor(HeaderLabel.TextColor);
        ExpandAllArrow = new ExpandArrow(this);
        ExpandAllArrow.SetExpanded(true);
        ExpandAllArrow.OnStateChanged += _ =>
        {
            if (!ExpandAllArrow.Expanded)
            {
                StackPanels.ForEach(s => s.SetVisible(false));
                BranchLabels.ForEach(b => b.SetVisible(false));
            }
            else
            {
                for (int i = 0; i < StackPanels.Count; i++)
                {
                    StackPanels[i].SetVisible(ExpandArrows[i].Expanded);
                    BranchLabels[i].SetVisible(true);
                }
            }
            UpdateLabels();
            UpdateHeight();
        };
        OnSizeChanged += _ =>
        {
            ExpandAllArrow.SetPosition(Size.Width - 16, 8);
            ExpandArrows.ForEach(e => e.SetPosition(Size.Width - 16, e.Position.Y));
        };
        //DrawEndLabels = false;
    }

    private void UpdateLabels()
    {
        EndLabel.SetVisible(DrawEndLabels);
        if (!ExpandAllArrow.Expanded)
        {
            if (DrawEndLabels)
            {
                EndLabel.SetPosition(8, 22 + 4);
                HeightAdd = 22;
            }
            else HeightAdd = 0;
            return;
        }
        int y = 22;
        for (int i = 0; i < BranchLabels.Count; i++)
        {
            BranchLabels[i].SetPosition(8, y + 4);
            ExpandArrows[i].SetPosition(Size.Width - 16, y + 4);
            StackPanels[i].SetPosition(StackPanels[i].Position.X, y + 22);
            y += 22 + (ExpandArrows[i].Expanded ? StackPanels[i].Size.Height : 0);
        }
        if (DrawEndLabels)
        {
            EndLabel.SetPosition(8, y + 4);
        }
        else y -= 22;
        HeightAdd = y;
    }

    public override void LoadCommand()
    {
        BranchLabels.ForEach(l => l.Dispose());
        BranchLabels.Clear();

        StackPanels.ForEach(s => s.Dispose());
        StackPanels.Clear();

        ExpandArrows.ForEach(e => e.Dispose());
        ExpandArrows.Clear();

        HeaderLabel.SetText("Show Choices");

        List<object> Choices = (List<object>) Command.Parameters[0];

        List<EventCommand> BranchCommands = Commands.FindAll(c => (c.Code == CommandCode.BranchWhenXXX || c.Code == CommandCode.BranchWhenCancel) && c.Indent == Command.Indent);
        
        for (int i = 0; i < BranchCommands.Count; i++)
        {
            EventCommand BranchCmd = BranchCommands[i];
            int BranchIdx = Commands.IndexOf(BranchCmd);

            Label BranchLabel = new Label(this);
            BranchLabel.SetFont(Fonts.ProductSansMedium.Use(9));
            if (BranchCmd.Code == CommandCode.BranchWhenCancel) BranchLabel.SetText($"When cancelled:");
            else BranchLabel.SetText($"When [{Choices[i]}]:");
            BranchLabels.Add(BranchLabel);

            ExpandArrow Arrow = new ExpandArrow(this);
            Arrow.SetExpanded(true);
            ExpandArrows.Add(Arrow);

            VStackPanel StackPanel = new VStackPanel(this);
            StackPanel.SetHDocked(true);
            StackPanel.SetPosition(ChildIndent, 0);
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
            Arrow.OnStateChanged += _ =>
            {
                StackPanel.SetVisible(Arrow.Expanded);
                UpdateLabels();
                UpdateHeight();
            };
        }
        if (DrawEndLabels) HeightAdd = 22;
        UpdateLabels();
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        ExpandArrows.ForEach(e => e.SetVisible(false));
        ExpandAllArrow.SetVisible(Mouse.Inside);
        if (!Mouse.Inside) return;
        int ry = e.Y - Viewport.Y + TopCutOff;
        int idx = -1;
        for (int i = 0; i < BranchLabels.Count; i++)
        {
            if (ry >= BranchLabels[i].Position.Y) idx = i;
        }
        if (idx > -1 && (ry < Size.Height - 22 || !DrawEndLabels))
        {
            ExpandArrows[idx].SetVisible(true);
        }
    }
}
