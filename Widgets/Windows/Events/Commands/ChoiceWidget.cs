using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class ChoiceWidget : BaseCommandWidget
{
    GradientBox EndGradient;
    Label EndLabel;
    List<Label> BranchLabels = new List<Label>();
    List<VStackPanel> StackPanels = new List<VStackPanel>();
    List<ExpandArrow> ExpandArrows = new List<ExpandArrow>();
    List<GradientBox> GradientBoxes = new List<GradientBox>();
    List<(ShadowWidget BranchShadow, ShadowWidget LabelShadow)> Shadows = new List<(ShadowWidget BranchShadow, ShadowWidget LabelShadow)>();
    ExpandArrow ExpandAllArrow;
    ShadowWidget RightHeaderShadow;
    ShadowWidget EndLabelShadow;

    public ChoiceWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex, new Color(128, 128, 255))
    {
        RightHeaderShadow = new ShadowWidget(this);
        EndGradient = new GradientBox(this);
        EndLabelShadow = new ShadowWidget(this);
        EndLabel = new Label(this);
        EndLabel.SetFont(Fonts.CabinMedium.Use(9));
        EndLabel.SetText("End");
        EndLabel.SetTextColor(HeaderLabel.TextColor);
        ExpandAllArrow = new ExpandArrow(this);
        ExpandAllArrow.SetExpanded(true);
        ExpandAllArrow.SetVisible(false);
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
                    //StackPanels[i].SetVisible(ExpandArrows[i].Expanded);
                    //BranchLabels[i].SetVisible(true);
                }
            }
            UpdateLabels();
            UpdateSize();
        };
        OnSizeChanged += _ =>
        {
            ExpandAllArrow.SetPosition(Size.Width - 16, 6);
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
                EndLabel.SetPosition(8, StandardHeight + 4);
                HeightAdd = StandardHeight;
            }
            else HeightAdd = 0;
            return;
        }
        int y = StandardHeight + ShadowSize;
        for (int i = 0; i < BranchLabels.Count; i++)
        {
            BranchLabels[i].SetPosition(12, y + 4);
            if (i > 0) GradientBoxes[i].SetPadding(BarWidth + ShadowSize, y);
            ExpandArrows[i].SetPosition(Size.Width - 16, y + 4);
            StackPanels[i].SetPosition(StackPanels[i].Position.X, y + StandardHeight + MarginBetweenWidgets + ShadowSize);
            if (StackPanels[i].Visible)
            {
                GradientBox GB = i > 0 ? GradientBoxes[i] : GradientBox;
                ShadowWidget BranchShadow = Shadows[i].BranchShadow;
                BranchShadow.SetPosition(GB.Padding.Left, GB.Padding.Up + GB.Size.Height);
                BranchShadow.SetSize(GB.Size.Width + ShadowSize, StackPanels[i].Size.Height + MarginBetweenWidgets * 2 + ShadowSize * 2);
                ShadowWidget LabelShadow = Shadows[i].LabelShadow;
                LabelShadow.SetPosition(BranchShadow.Position.X + BranchShadow.Size.Width - ShadowSize, BranchShadow.Position.Y + BranchShadow.Size.Height - ShadowSize);
                LabelShadow.SetSize(ShadowSize, StandardHeight + 2 * ShadowSize);
            }
            y += StandardHeight + (ExpandArrows[i].Expanded ? StackPanels[i].Size.Height : 0) + MarginBetweenWidgets * 2 + ShadowSize * 2;
        }
        if (DrawEndLabels)
        {
            EndLabel.SetPosition(12, y + 4);
            EndGradient.SetPadding(BarWidth + ShadowSize, y);
        }
        else y -= StandardHeight;
        HeightAdd = y - ShadowSize;
        RightHeaderShadow.SetPosition(GradientBox.Padding.Left + GradientBox.Size.Width, 0);
        RightHeaderShadow.SetSize(ShadowSize, GradientBox.Size.Height + ShadowSize * 2);
        EndLabelShadow.SetPosition(0, y + StandardHeight);
        EndLabelShadow.SetSize(EndGradient.Size.Width + BarWidth + 2 * ShadowSize, ShadowSize);
    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        BranchLabels.ForEach(l => l.Dispose());
        BranchLabels.Clear();

        StackPanels.ForEach(s => s.Dispose());
        StackPanels.Clear();

        ExpandArrows.ForEach(e => e.Dispose());
        ExpandArrows.Clear();

        GradientBoxes.ForEach(e => e.Dispose());
        GradientBoxes.Clear();

        HeaderLabel.SetText("Show Choices");

        GradientBox.SetDocked(false);
        GradientBox.SetSize(GetStandardWidth(Indentation) - BarWidth - ShadowSize * 2, StandardHeight * 2);
        ShadowWidget.SetSize(GetStandardWidth(Indentation), Size.Height);
        ShadowWidget.ShowBottom(false);
        ShadowWidget.ShowRight(false);
        ShadowWidget.ShowBottomRight(false);

        RightHeaderShadow.ShowTop(false);
        RightHeaderShadow.ShowBottom(false);
        RightHeaderShadow.ShowLeft(false);
        RightHeaderShadow.ShowTopLeft(false);
        RightHeaderShadow.ShowTopRight(false);
        RightHeaderShadow.ShowBottomLeft(false);

        EndLabelShadow.ShowBottomLeft(false);
        EndLabelShadow.ShowLeft(false);
        EndLabelShadow.ShowTopLeft(false);
        EndLabelShadow.ShowTop(false);
        EndLabelShadow.ShowTopRight(false);
        EndLabelShadow.ShowRight(false);
        EndLabelShadow.ShowBottomRight(false);

        List<object> Choices = (List<object>) Command.Parameters[0];

        List<EventCommand> BranchCommands = Commands.FindAll(c => (c.Code == CommandCode.BranchWhenXXX || c.Code == CommandCode.BranchWhenCancel) && c.Indent == Command.Indent);

        int gidx = this.GlobalCommandIndex + 2; // + 0 = Show Choices, + 1 = BranchWhenXXX nr. 1, + 2 = First command
        for (int i = 0; i < BranchCommands.Count; i++)
        {
            EventCommand BranchCmd = BranchCommands[i];
            int BranchIdx = Commands.IndexOf(BranchCmd);

            // The first gradient box is the Main box, covering the header and first branch label.
            if (i > 0)
            {
                GradientBox GBox = new GradientBox(this);
                GBox.SetTopLeftColor(GradientBox.TopLeftColor);
                GBox.SetBottomRightColor(GradientBox.BottomRightColor);
                GBox.SetSize(GetStandardWidth(Indentation) - BarWidth - ShadowSize * 2, StandardHeight);
                GradientBoxes.Add(GBox);
            }
            else GradientBoxes.Add(null);

            Label BranchLabel = new Label(this);
            BranchLabel.SetFont(Fonts.CabinMedium.Use(9));
            if (BranchCmd.Code == CommandCode.BranchWhenCancel) BranchLabel.SetText($"When cancelled:");
            else BranchLabel.SetText($"When [{Choices[i]}]:");
            BranchLabels.Add(BranchLabel);

            ExpandArrow Arrow = new ExpandArrow(this);
            Arrow.SetExpanded(true);
            Arrow.SetVisible(false);
            ExpandArrows.Add(Arrow);

            VStackPanel StackPanel = new VStackPanel(this);
            StackPanel.SetWidth(GetStandardWidth(Indentation));
            StackPanel.SetPosition(ChildIndent + ShadowSize, 0);
            StackPanel.HDockWidgets = false;
            StackPanels.Add(StackPanel);

            ShadowWidget BranchShadow = new ShadowWidget(this);
            BranchShadow.SetInverted(true);
            BranchShadow.ShowTopRight(false);
            BranchShadow.ShowRight(false);
            BranchShadow.ShowBottomRight(false);

            ShadowWidget LabelShadow = new ShadowWidget(this);
            LabelShadow.ShowTop(false);
            LabelShadow.ShowTopLeft(false);
            LabelShadow.ShowLeft(false);
            LabelShadow.ShowBottomLeft(false);
            LabelShadow.ShowBottom(false);
            LabelShadow.Viewport.Z = 10000;

            Shadows.Add((BranchShadow, LabelShadow));

            if (i < BranchCommands.Count - 1)
            {
                // There's another branch after this
                int NextBranchIdx = Commands.IndexOf(BranchCommands[i + 1]);
                ParseCommands(Commands.GetRange(BranchIdx + 1, NextBranchIdx - BranchIdx - 1), StackPanel, gidx);
                gidx += NextBranchIdx - BranchIdx; // + 1 for the BranchWhenXXX command
            }
            else ParseCommands(Commands.GetRange(BranchIdx + 1, Commands.Count - BranchIdx - 2), StackPanel, gidx);
            StackPanel.OnSizeChanged += _ =>
            {
                UpdateLabels();
                UpdateSize();
            };
            Arrow.OnStateChanged += _ =>
            {
                StackPanel.SetVisible(Arrow.Expanded);
                UpdateLabels();
                UpdateSize();
            };
        }
        EndGradient.SetVisible(DrawEndLabels);
        if (DrawEndLabels)
        {
            HeightAdd = StandardHeight;
            EndGradient.SetTopLeftColor(GradientBox.TopLeftColor);
            EndGradient.SetBottomRightColor(GradientBox.BottomRightColor);
            EndGradient.SetSize(GetStandardWidth(Indentation) - BarWidth - ShadowSize * 2, StandardHeight);
        }
        UpdateLabels();
    }

    protected override void UpdateBackdrops()
    {
        
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        ExpandArrows.ForEach(e => e.SetVisible(false));
        //ExpandAllArrow.SetVisible(Mouse.Inside);
        if (!Mouse.Inside) return;
        int ry = e.Y - Viewport.Y + TopCutOff;
        int idx = -1;
        for (int i = 0; i < BranchLabels.Count; i++)
        {
            if (ry >= BranchLabels[i].Position.Y) idx = i;
        }
        if (idx > -1 && (ry < Size.Height - StandardHeight || !DrawEndLabels))
        {
            //ExpandArrows[idx].SetVisible(true);
        }
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        int ry = e.Y - Viewport.Y + TopCutOff;
        if (e.Handled || this.Indentation == -1 || InsideChild())
        {
            CancelDoubleClick();
            return;
        }
        bool sel = ry < StandardHeight || !ExpandAllArrow.Expanded;
        if (!sel)
        {
            for (int i = 0; i < BranchLabels.Count; i++)
            {
                if (ry >= BranchLabels[i].Position.Y && ry < BranchLabels[i].Position.Y + StandardHeight)
                {
                    sel = true;
                    break;
                }
            }
        }
        if (sel) SetSelected(true);
        else CancelDoubleClick();
    }
}
