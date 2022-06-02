using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class BlankWidget : BaseCommandWidget
{
    public BlankWidget(IContainer Parent) : base(Parent) { }

    public override void LoadCommand()
    {
        HeaderLabel.SetVisible(false);
    }
}
