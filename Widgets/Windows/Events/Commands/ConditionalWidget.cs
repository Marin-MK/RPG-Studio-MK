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
    ExpandArrow ExpandIfArrow;
    ExpandArrow ExpandElseArrow;

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
        ExpandIfArrow = new ExpandArrow(this);
        ExpandIfArrow.SetExpanded(true);
        ExpandIfArrow.OnStateChanged += _ =>
        {
            VStackPanel1.SetVisible(ExpandIfArrow.Expanded);
            UpdateHeight();
        };
        ExpandElseArrow = new ExpandArrow(this);
        ExpandElseArrow.SetExpanded(true);
        ExpandElseArrow.OnStateChanged += _ =>
        {
            VStackPanel2.SetVisible(ExpandElseArrow.Expanded);
            UpdateHeight();
        };
        OnSizeChanged += _ =>
        {
            ExpandIfArrow.SetPosition(Size.Width - 16, 8);
            ExpandElseArrow.SetPosition(Size.Width - 16, ElseLabel.Position.Y);
        };
        //DrawEndLabels = false;
    }

    private void UpdateLabels()
    {
        int endy = 0;
        if (ElseLabel.Visible)
        {
            int elsey = VStackPanel.Padding.Up + VStackPanel1.Size.Height;
            if (!VStackPanel1.Visible) elsey = 22;
            ElseLabel.SetPosition(8, elsey + 4);
            endy = elsey + VStackPanel2.Margins.Up + VStackPanel2.Size.Height;
            if (!VStackPanel2.Visible) endy = ElseLabel.Visible ? elsey + 22 : 0;
        }
        else
        {
            endy = VStackPanel.Padding.Up + VStackPanel1.Size.Height;
            if (!VStackPanel1.Visible) endy = 22;
        }
        EndLabel.SetPosition(8, endy + 4);
        EndLabel.SetVisible(DrawEndLabels);
        if (DrawEndLabels)
        {
            if (VStackPanel1.Visible && VStackPanel2.Visible) HeightAdd = 22;
            else if (VStackPanel1.Visible && !VStackPanel2.Visible) HeightAdd = ElseLabel.Visible ? 44 : 22;
            else if (!VStackPanel1.Visible && VStackPanel2.Visible) HeightAdd = 22;
            else HeightAdd = ElseLabel.Visible ? 44 : 22;
        }
        else
        {
            HeightAdd = ElseLabel.Visible ? VStackPanel2.Visible ? 0 : 22 : 0;
        }
    }

    public override void LoadCommand()
    {
        // Draw conditional
        HeaderLabel.SetText(CommandHelper.GetText(Map, Event));

        VStackPanel1?.Dispose();
        VStackPanel2?.Dispose();

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
            ExpandElseArrow.SetVisible(true);
            VStackPanel2.SetVisible(true);
        }
        else
        {
            // Draw true commands
            ParseCommands(Commands.GetRange(1, Commands.Count - 2), VStackPanel1);
            VStackPanel1.UpdateLayout();
            ElseLabel.SetVisible(false);
            ExpandElseArrow.SetVisible(false);
            VStackPanel2.SetVisible(false);
        }
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

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        ExpandIfArrow.SetVisible(false);
        ExpandElseArrow.SetVisible(false);
        if (!Mouse.Inside) return;
        if (ElseLabel.Visible)
        {
            int ry = e.Y - Viewport.Y + TopCutOff;
            ExpandIfArrow.SetVisible(ry < ElseLabel.Position.Y);
            ExpandElseArrow.SetVisible(!ExpandIfArrow.Visible);
        }
        else ExpandIfArrow.SetVisible(true);
    }
}
