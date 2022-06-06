using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class BlankWidget : BaseCommandWidget
{
    public BlankWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex) { }

    public override void LoadCommand()
    {
        HeaderLabel.SetVisible(false);
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (e.Handled || this.Indentation == -1 || InsideChild())
        {
            CancelDoubleClick();
            return;
        }
        SetSelected(true);
    }
}
