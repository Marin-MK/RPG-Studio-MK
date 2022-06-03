using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class ExpandableCommandWidget : BaseCommandWidget
{
    public bool Expanded => ExpandArrow.Expanded;
    public bool Expandable { get; protected set; } = true;

    ExpandArrow ExpandArrow;

    public ExpandableCommandWidget(IContainer Parent) : base(Parent)
    {
        ExpandArrow = new ExpandArrow(this);
        ExpandArrow.OnStateChanged += _ =>
        {
            LoadCommand();
            UpdateHeight();
            ((VStackPanel) Parent).UpdateLayout();
        };
        OnSizeChanged += _ => ExpandArrow.SetPosition(Size.Width - 16, 8);
    }

    public void SetExpandable(bool Expandable)
    {
        if (this.Expandable != Expandable)
        {
            this.Expandable = Expandable;
            ExpandArrow.SetVisible(Expandable);
        }
    }
}
