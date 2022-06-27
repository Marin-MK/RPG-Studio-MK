using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class WaitForMoveCompletionWidget : BaseCommandWidget
{
    public WaitForMoveCompletionWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex, new Color(255, 128, 128)) { }

    public override void LoadCommand()
    {
        base.LoadCommand();
        HeaderLabel.SetText("Wait for Move's Completion");
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

    protected override bool IsEditable()
    {
        return false;
    }
}
